using System.Buffers.Binary;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decodes what each of Samus's glitch reads yields from the player's cartridge (#1474): it runs each read the way the
/// original game does, over the cartridge, for every game situation that can reach it, and keeps only the result, keyed
/// by that situation.
/// </summary>
internal static class SamusOverreadDecoding
{
    /// <summary>
    /// The drained poses a Shinespark crash can finish in: Mother Brain's drain replaces the crash pose with a drained
    /// crouch while the crash keeps running.
    /// </summary>
    internal static readonly SamusPoseId[] DrainedCrashPoses = [SamusPoseId.DrainedCrouchingRightPose, SamusPoseId.DrainedCrouchingLeftPose];

    /// <summary>Decodes every glitch read into the installed document.</summary>
    internal static SamusOverreadDocument Decode(IImportCartridgeSource cartridge)
    {
        SamusProjectileListLayout lists = SamusProjectileListEntries.Read(cartridge);
        return new SamusOverreadDocument(
            SamusOverreadFormat.Version,
            AnimationContinuations(cartridge),
            SamusAnimationCueValueDecoding.CueValueSteps(cartridge),
            SpaceTimeOverwrites(cartridge, lists),
            ChainsawFuseLandings(lists),
            PostureAdmissions(cartridge),
            SpeedBoosterOverrunColours(cartridge),
            UnauthoredBeamLaunchSpeeds(cartridge),
            UnauthoredBeamCooldowns(cartridge),
            DrainedCrashDepartures(cartridge),
            ChainsawPaletteColours(cartridge));
    }

    /// <summary>
    /// Each pose's program's successor in the cartridge's layout of the animation programs: the program whose first
    /// frame lies nearest after its own, named by its lowest-numbered pose, and the distance between their starts.
    /// </summary>
    /// <exception cref="InvalidDataException">Two poses of one program start apart, or two programs start together.</exception>
    private static Dictionary<string, SamusOverreadDocument.Continuation?> AnimationContinuations(IImportCartridgeSource cartridge)
    {
        SamusAnimationProgramLayout.LaidOutProgram[] laidOut = SamusAnimationProgramLayout.Read(cartridge);
        var successors = new Dictionary<SamusPoseAnimation, SamusOverreadDocument.Continuation?>();
        for (int index = 0; index < laidOut.Length; index++)
        {
            successors.Add(laidOut[index].Program, index + 1 < laidOut.Length
                ? new SamusOverreadDocument.Continuation(laidOut[index + 1].NamedBy.ToString(), laidOut[index + 1].Start - laidOut[index].Start)
                : null);
        }
        return SamusAnimationProgramLayout.PosesWithRecords()
            .ToDictionary(pose => pose.ToString(), pose => successors[SamusAnimationDelays.AnimationForPose(pose)]);
    }

    /// <summary>
    /// What SpaceTime's palette copy overwrites in progression for every animation position a SpaceTime shot can hold and
    /// every shot slot.
    /// </summary>
    private static Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>> SpaceTimeOverwrites(
        IImportCartridgeSource cartridge, SamusProjectileListLayout lists)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>>();
        foreach (SamusProjectileSegment program in SpaceTimeShotPrograms())
        {
            var positions = new Dictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>();
            // Every place of the segment: before each beat, and at its end.
            for (int position = 0; position <= SamusProjectileAnimations.Segments.Beats(program).Count; position++)
            {
                int place = lists.Position(new SamusProjectileSegmentBeat(program, position));
                positions.Add(position.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    [.. Enumerable.Range(0, SamusProjectileSystem.SlotCount).Select(slot => SpaceTimeOverwrite(cartridge, place, slot))]);
            }
            result.Add(program.ToString(), positions);
        }
        return result;
    }

    /// <summary>
    /// Every segment a SpaceTime shot can animate by: the segments its header selects in every aim direction, and every
    /// segment their animations play.
    /// </summary>
    internal static SamusProjectileSegment[] SpaceTimeShotPrograms()
    {
        var programs = new HashSet<SamusProjectileSegment>();
        var pending = new Queue<SamusProjectileSegment>();
        foreach (bool charged in (bool[])[false, true])
        {
            foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
            {
                if (SamusBeamFrameBehaviors.Resolve(charged, beams).FrameBehavior != SamusProjectileFrameBehavior.SpacetimePaletteCopyTail)
                    continue;
                foreach (SamusProjectileSegment program in EquippedBeamProjectilePrograms.BeamShotPrograms(beams, charged))
                {
                    if (programs.Add(program))
                        pending.Enqueue(program);
                }
            }
        }
        while (pending.TryDequeue(out SamusProjectileSegment program))
        {
            foreach (AnimationPhase<SamusProjectileSegment> phase in SamusProjectileAnimations.StartingWith(program).Phases)
            {
                if (programs.Add(phase.Segment))
                    pending.Enqueue(phase.Segment);
            }
        }
        return [.. programs.Order()];
    }

    /// <summary>
    /// Runs the SpaceTime copy for a shot in <paramref name="slot"/> whose animation stands at list position
    /// <paramref name="place"/>: from one colour past the slot in the beam palette, reading from eight bytes before the
    /// position and advancing a word at a time while the signed read index stays below the palette's size. Returns the
    /// leading entries of each progression table the copy replaces, or null when it stops before progression.
    /// </summary>
    /// <exception cref="InvalidDataException">A byte the copy writes into progression would come from scratch memory.</exception>
    private static SamusOverreadDocument.Overwrite? SpaceTimeOverwrite(IImportCartridgeSource cartridge, int place, int slot)
    {
        var slice = new byte?[ProgressionSliceLayout.ByteCount];
        int destination = (slot + 1) * sizeof(ushort);
        int source = unchecked((ushort)(place - SamusProjectileListLayout.TimedRecordByteCount + sizeof(ushort)));
        while (unchecked((short)(source - SamusOverreadSourceDefinitions.PaletteByteCount)) < 0)
        {
            for (int offset = 0; offset < sizeof(ushort); offset++)
            {
                int sliceOffset = destination + offset - SamusOverreadSourceDefinitions.ProgressionSliceDistance;
                if (sliceOffset < 0 || sliceOffset >= slice.Length)
                    continue;
                int read = source + offset;
                if (read is < SamusOverreadSourceDefinitions.CartridgeHalfStart or > ushort.MaxValue)
                    throw new InvalidDataException($"SpaceTime's copy from list position {place} in slot {slot} writes progression from scratch memory.");
                slice[sliceOffset] = cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.SpacetimeCopySourceBank | read);
            }
            destination = unchecked((ushort)(destination + sizeof(ushort)));
            source = unchecked((ushort)(source + sizeof(ushort)));
        }
        return ProgressionOverwrite(slice);
    }

    /// <summary>Decodes the copied prefix of the progression slice into the events, bosses, orbs, items, doors, stations and maps it decides.</summary>
    /// <exception cref="InvalidDataException">The copied bytes are not a prefix of the slice, or split the loading state.</exception>
    private static SamusOverreadDocument.Overwrite? ProgressionOverwrite(byte?[] slice)
    {
        int copied = 0;
        while (copied < slice.Length && slice[copied] is not null)
            copied++;
        if (slice.Skip(copied).Any(value => value is not null))
            throw new InvalidDataException("SpaceTime's copy reaches progression other than from its start.");
        if (copied == 0)
            return null;
        byte[] Leading(int start, int end) =>
            [.. slice[start..Math.Clamp(copied, start, end)].Select(value => value ?? throw new InvalidDataException("A copied progression byte is missing."))];
        int? loadingState = null;
        if (copied > ProgressionSliceLayout.LoadingGameStateOffset)
        {
            if (copied < ProgressionSliceLayout.ByteCount)
                throw new InvalidDataException("SpaceTime's copy replaces only half the saved loading state.");
            loadingState = BinaryPrimitives.ReadUInt16LittleEndian(
                (byte[])[.. slice[ProgressionSliceLayout.LoadingGameStateOffset..].Select(value =>
                    value ?? throw new InvalidDataException("A copied loading-state byte is missing."))]);
        }
        return new SamusOverreadDocument.Overwrite(
            ProgressionTableCodec.PrefixOverwrite(Leading(ProgressionSliceLayout.EventsOffset, ProgressionSliceLayout.BossBitsOffset)),
            ProgressionTableCodec.AreaBossesPrefix(Leading(ProgressionSliceLayout.BossBitsOffset, ProgressionSliceLayout.RoomChozoBitsOffset))
                .ToDictionary(area => area.Key.ToString(), area => (IReadOnlyList<string>)[.. BossFlagNames(area.Value)]),
            ProgressionTableCodec.PrefixOverwrite(Leading(ProgressionSliceLayout.CollectedItemBitsOffset, ProgressionSliceLayout.OpenedDoorBitsOffset)),
            ProgressionTableCodec.PrefixOverwrite(Leading(ProgressionSliceLayout.OpenedDoorBitsOffset, ProgressionSliceLayout.PaddingOffset)),
            ProgressionTableCodec.UsedStationsPrefix(Leading(ProgressionSliceLayout.UsedSaveStationsOffset, ProgressionSliceLayout.MapStationsOffset))
                .ToDictionary(area => area.Key.ToString(), area => area.Value),
            ProgressionTableCodec.AreaMapsPrefix(Leading(ProgressionSliceLayout.MapStationsOffset, ProgressionSliceLayout.LoadingGameStateOffset))
                .ToDictionary(area => area.Key.ToString(), area => area.Value),
            loadingState);
    }

    /// <summary>The names of the boss flags <paramref name="bosses"/> holds.</summary>
    private static IEnumerable<string> BossFlagNames(BossBits bosses) =>
        BossBitMasks.InMarkerOrder.Where(flag => bosses.HasFlag(flag)).Select(flag => flag.ToString());

    /// <summary>
    /// Where the Chainsaw fuse's fast-list offset moves every animation position that lands on one; a position whose move
    /// lands on no list position is left out, and the game fails if its shot ever reaches it.
    /// </summary>
    private static Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> ChainsawFuseLandings(SamusProjectileListLayout lists)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>>();
        foreach (SamusProjectileSegment program in Enum.GetValues<SamusProjectileSegment>())
        {
            var landings = new Dictionary<string, SamusOverreadDocument.Cursor>();
            for (int position = 0; position <= SamusProjectileAnimations.Segments.Beats(program).Count; position++)
            {
                int landing = unchecked((ushort)(lists.Position(new SamusProjectileSegmentBeat(program, position)) +
                    SamusOverreadSourceDefinitions.FastAnimationOffset));
                if (landing != 0 && lists.TryPlaceAt(landing, out SamusProjectileSegmentBeat landed))
                    landings.Add(position.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        new SamusOverreadDocument.Cursor(landed.Segment.ToString(), landed.Beat));
            }
            result.Add(program.ToString(), landings);
        }
        return result;
    }

    /// <summary>
    /// Whether the posture-transition admission admits weapons for each pose outside its table: it reads one byte per pose
    /// number from the table's place for pose zero, and a zero admits weapons.
    /// </summary>
    private static Dictionary<string, bool> PostureAdmissions(IImportCartridgeSource cartridge) =>
        Enum.GetValues<SamusPoseId>()
            .Where(pose => SamusPoseMetadata.PostureTransitionAdmission(pose) == SamusPostureTransitionAdmission.BeyondTable)
            .OrderBy(SamusPoseNumbers.Number)
            .ToDictionary(pose => pose.ToString(), pose =>
                cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.HudPostureFlagsForPoseZero + SamusPoseNumbers.Number(pose)) == 0);

    /// <summary>
    /// The colours Gravity copies on each Speed Booster step past its cycle: the expansion window's open-bus words, then
    /// the Grapple handler's code read as colour words.
    /// </summary>
    private static Dictionary<string, IReadOnlyList<PaletteRgb5>> SpeedBoosterOverrunColours(IImportCartridgeSource cartridge) => new()
    {
        [SpeedBoosterPaletteOverrun.FirstStepPastCycle.ToString()] = [.. Enumerable.Range(0, SamusColourSets.ColoursPerSet)
            .Select(_ => PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(SamusOverreadSourceDefinitions.ExpansionOpenBusWord)))],
        [SpeedBoosterPaletteOverrun.SecondStepPastCycle.ToString()] = [.. Enumerable.Range(0, SamusColourSets.ColoursPerSet)
            .Select(index => PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(cartridge.ReadCartridgeWord(
                SamusOverreadSourceDefinitions.SpeedBoosterGrappleCode + index * ColourWordCodec.ByteCount))))],
    };

    /// <summary>
    /// The speeds each Spazer and Plasma combination launches at: the beam initializer reads a cardinal then a diagonal
    /// word at four bytes per combination from the speed table's start, so these combinations read the words after the
    /// table's twelve rows.
    /// </summary>
    private static Dictionary<string, SamusOverreadDocument.LaunchSpeeds> UnauthoredBeamLaunchSpeeds(IImportCartridgeSource cartridge) =>
        Enum.GetValues<SamusBeamCombination>()
            .Where(beams => !beams.IsRetail)
            .ToDictionary(beams => beams.ToString(), beams =>
            {
                int row = SamusOverreadSourceDefinitions.BeamSpeedTable + beams.TableIndex * 2 * sizeof(ushort);
                return new SamusOverreadDocument.LaunchSpeeds(
                    unchecked((short)cartridge.ReadCartridgeWord(row)),
                    unchecked((short)cartridge.ReadCartridgeWord(row + sizeof(ushort))));
            });

    /// <summary>
    /// The cooldowns each Spazer and Plasma combination waits: the fire routines read a combination's byte from the
    /// uncharged, charged and held-fire rows, so these combinations read the zero bytes after the first two rows' twelve
    /// cells and the projectile sound table after the held-fire row.
    /// </summary>
    private static Dictionary<string, SamusOverreadDocument.Cooldowns> UnauthoredBeamCooldowns(IImportCartridgeSource cartridge) =>
        Enum.GetValues<SamusBeamCombination>()
            .Where(beams => !beams.IsRetail)
            .ToDictionary(beams => beams.ToString(), beams => new SamusOverreadDocument.Cooldowns(
                cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.UnchargedCooldowns + beams.TableIndex),
                cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.ChargedCooldowns + beams.TableIndex),
                cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.HeldFireCooldowns + beams.TableIndex)));

    /// <summary>
    /// The angles a crash finishing in each drained pose releases its echoes at: the crash's end reads two angle bytes at
    /// the pose's distance from the first crash pose, so a drained pose reads past the table's six pairs.
    /// </summary>
    private static Dictionary<string, SamusOverreadDocument.CrashDepartures> DrainedCrashDepartures(IImportCartridgeSource cartridge) =>
        DrainedCrashPoses.ToDictionary(pose => pose.ToString(), pose =>
        {
            int pair = SamusOverreadSourceDefinitions.CrashDepartureTable +
                (SamusPoseNumbers.Number(pose) - SamusPoseNumbers.Number(SamusPoseId.ShinesparkHorizontalRightPose)) * 2;
            return new SamusOverreadDocument.CrashDepartures(cartridge.ReadCartridgeByte(pair), cartridge.ReadCartridgeByte(pair + 1));
        });

    /// <summary>
    /// The Chainsaw beam's sixteen colours: the loader copies sixteen words from the place it reads past the palette
    /// table, whose first byte answers with the open bus and whose other bytes are Samus_Animate's code. The palette keeps
    /// each word without its top bit.
    /// </summary>
    private static PaletteRgb5[] ChainsawPaletteColours(IImportCartridgeSource cartridge) =>
        [.. Enumerable.Range(0, ChainsawBeamColours<RgbColor>.ColourCount).Select(colour =>
        {
            int place = SamusOverreadSourceDefinitions.ChainsawPaletteStart + colour * ColourWordCodec.ByteCount;
            int word = colour == 0
                ? SamusOverreadSourceDefinitions.ChainsawPaletteOpenBusByte | cartridge.ReadCartridgeByte(place + 1) << LittleEndianBytes.BitsPerByte
                : cartridge.ReadCartridgeWord(place);
            return PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(word));
        })];
}
