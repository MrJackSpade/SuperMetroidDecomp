using System.Buffers.Binary;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decodes what each of Samus's glitch reads yields from the player's cartridge (#1474): it runs each read the way the
/// original game does, over the cartridge, for every game situation that can reach it, and keeps only the result, keyed
/// by that situation.
/// </summary>
internal static class SamusOverreadDecoding
{
    /// <summary>Decodes every glitch read into the installed document.</summary>
    internal static SamusOverreadDocument Decode(IImportCartridgeSource cartridge)
    {
        SamusProjectileListLayout lists = SamusProjectileListEntries.Read(cartridge);
        return new SamusOverreadDocument(
            SamusOverreadFormat.Version,
            AnimationContinuations(cartridge),
            Enum.GetValues<SamusProjectileProgram>().ToDictionary(program => program.ToString(), program => (int)lists.Prefix(program)),
            TrailPlacementsPastRecords(cartridge, lists),
            SpaceTimeOverwrites(cartridge, lists),
            ChainsawFuseLandings(lists),
            ChargeFlareDelaysBelowPrograms(cartridge),
            PostureAdmissions(cartridge),
            SpeedBoosterOverrunColours(cartridge));
    }

    /// <summary>
    /// Each pose's program's successor in the cartridge's layout of the animation programs: the program whose first
    /// frame lies nearest after its own, named by its lowest-numbered pose, and the distance between their starts.
    /// </summary>
    /// <exception cref="InvalidDataException">Two poses of one program start apart, or two programs start together.</exception>
    private static Dictionary<string, SamusOverreadDocument.Continuation?> AnimationContinuations(IImportCartridgeSource cartridge)
    {
        var starts = new Dictionary<SamusAnimationProgram, int>();
        var namedBy = new Dictionary<SamusAnimationProgram, SamusPoseId>();
        SamusPoseId[] poses = [.. Enum.GetValues<SamusPoseId>().Where(SamusPoseNumbering.HasRecord).OrderBy(SamusPoseNumbering.Number)];
        foreach (SamusPoseId pose in poses)
        {
            SamusAnimationProgram program = SamusAnimationDelays.ProgramForPose(pose);
            int start = cartridge.ReadCartridgeWord(
                SamusOverreadSourceDefinitions.SamusAnimationProgramTable + SamusPoseNumbering.Number(pose) * sizeof(ushort));
            if (starts.TryGetValue(program, out int known))
            {
                if (known != start)
                    throw new InvalidDataException($"Pose {pose}'s animation program starts at {start}, but another pose of the same program starts at {known}.");
                continue;
            }
            starts.Add(program, start);
            namedBy.Add(program, pose);
        }
        if (starts.Values.Distinct().Count() != starts.Count)
            throw new InvalidDataException("Two Samus animation programs start at the same place.");
        KeyValuePair<SamusAnimationProgram, int>[] laidOut = [.. starts.OrderBy(pair => pair.Value)];
        var successors = new Dictionary<SamusAnimationProgram, SamusOverreadDocument.Continuation?>();
        for (int index = 0; index < laidOut.Length; index++)
        {
            successors.Add(laidOut[index].Key, index + 1 < laidOut.Length
                ? new SamusOverreadDocument.Continuation(namedBy[laidOut[index + 1].Key].ToString(), laidOut[index + 1].Value - laidOut[index].Value)
                : null);
        }
        return poses.ToDictionary(pose => pose.ToString(), pose => successors[SamusAnimationDelays.ProgramForPose(pose)]);
    }

    /// <summary>
    /// What a trail finds past each placement list's records at every frame it can read there. The original game indexes
    /// the list four bytes per frame within its bank and takes the record's four bytes from there; a record below the
    /// cartridge half of the bank, or running into the next bank, lies in scratch memory and draws no trail (#1318).
    /// </summary>
    private static Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Placement?>> TrailPlacementsPastRecords(
        IImportCartridgeSource cartridge, SamusProjectileListLayout lists)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Placement?>>();
        foreach (ProjectileTrailPlacementList list in Enum.GetValues<ProjectileTrailPlacementList>())
        {
            SnesAddress start = ProjectileTrailPlacementSources.Start(cartridge, list);
            var frames = new Dictionary<string, SamusOverreadDocument.Placement?>();
            foreach (int frame in TrailFramesPastRecords(list, lists))
            {
                SnesAddress record = start.AddWithinBank(frame * ProjectileTrailPlacementSources.RecordByteCount);
                SnesAddress last = record.AddWithinBank(ProjectileTrailPlacementSources.RecordByteCount - 1);
                if (!record.IsUpperLoRomWindow || last.Offset < record.Offset)
                {
                    frames.Add(frame.ToString(System.Globalization.CultureInfo.InvariantCulture), null);
                    continue;
                }
                byte[] bytes = RomDataReader.ReadFixedBank(cartridge, record, ProjectileTrailPlacementSources.RecordByteCount);
                frames.Add(frame.ToString(System.Globalization.CultureInfo.InvariantCulture), new SamusOverreadDocument.Placement(
                    unchecked((sbyte)bytes[0]), unchecked((sbyte)bytes[1]), unchecked((sbyte)bytes[2]), unchecked((sbyte)bytes[3])));
            }
            result.Add(list.ToString(), frames);
        }
        return result;
    }

    /// <summary>
    /// The frames a trail can read <paramref name="list"/> at past its placements, ascending: every Samus projectile
    /// record's animation frame and every word before a projectile list, which a trail spawned at a list's start reads.
    /// </summary>
    private static int[] TrailFramesPastRecords(ProjectileTrailPlacementList list, SamusProjectileListLayout lists) =>
        [.. SamusProjectileRecordDefinitions.Records.Select(record => record.AnimationFrame)
            .Concat(Enum.GetValues<SamusProjectileProgram>().Select(program => (int)lists.Prefix(program)))
            .Distinct()
            .Where(frame => frame >= ProjectileTrailPlacementDefinitions.Records[list].Length)
            .Order()];

    /// <summary>
    /// What SpaceTime's palette copy overwrites in progression for every animation position a SpaceTime shot can hold and
    /// every shot slot.
    /// </summary>
    private static Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>> SpaceTimeOverwrites(
        IImportCartridgeSource cartridge, SamusProjectileListLayout lists)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>>();
        foreach (SamusProjectileProgram program in SpaceTimeShotPrograms())
        {
            var positions = new Dictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>();
            for (int position = 0; position < SamusProjectileAnimations.Programs.Steps(program).Count; position++)
            {
                int place = lists.Position(new AnimationProgramCursor<SamusProjectileProgram>(program, position));
                positions.Add(position.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    [.. Enumerable.Range(0, SamusProjectileSystem.SlotCount).Select(slot => SpaceTimeOverwrite(cartridge, place, slot))]);
            }
            result.Add(program.ToString(), positions);
        }
        return result;
    }

    /// <summary>
    /// Every program a SpaceTime shot can animate by: the programs its header selects in every aim direction, and every
    /// program their steps go to or fall into.
    /// </summary>
    internal static SamusProjectileProgram[] SpaceTimeShotPrograms()
    {
        var programs = new HashSet<SamusProjectileProgram>();
        var pending = new Queue<SamusProjectileProgram>();
        foreach (bool charged in (bool[])[false, true])
        {
            foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
            {
                if (SamusBeamCallbacks.Resolve(charged, beams).Translated != SamusProjectilePreInstruction.SpacetimePaletteCopyTail)
                    continue;
                SamusProjectileHeader header = SamusProjectileSelection.Select(
                    charged ? SamusProjectileSelectorTable.ChargedBeam : SamusProjectileSelectorTable.UnchargedBeam, beams.TableIndex);
                foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
                {
                    if (SamusProjectileHeaders.Program(header, direction.ToAimIndex()) is { } program && programs.Add(program))
                        pending.Enqueue(program);
                }
            }
        }
        while (pending.TryDequeue(out SamusProjectileProgram program))
        {
            foreach (var step in SamusProjectileAnimations.Programs.Steps(program))
            {
                SamusProjectileProgram? target = step switch
                {
                    AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.GoToStep goTo => goTo.Target,
                    AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep fallInto => fallInto.Target,
                    _ => null,
                };
                if (target is { } next && programs.Add(next))
                    pending.Enqueue(next);
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
                if (read < SamusOverreadSourceDefinitions.CartridgeHalfStart || read > ushort.MaxValue)
                    throw new InvalidDataException($"SpaceTime's copy from list position {place} in slot {slot} writes progression from scratch memory.");
                slice[sliceOffset] = cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.SpacetimeCopySourceBank | read);
            }
            destination = unchecked((ushort)(destination + sizeof(ushort)));
            source = unchecked((ushort)(source + sizeof(ushort)));
        }
        return ProgressionOverwrite(slice);
    }

    /// <summary>Splits the copied prefix of the progression slice into each table's replaced leading entries.</summary>
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
        int[] Leading(int start, int end) =>
            [.. slice[start..Math.Clamp(copied, start, end)].Select(value => (int)(value ?? throw new InvalidDataException("A copied progression byte is missing.")))];
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
            Leading(ProgressionSliceLayout.EventsOffset, ProgressionSliceLayout.BossBitsOffset),
            Leading(ProgressionSliceLayout.BossBitsOffset, ProgressionSliceLayout.RoomChozoBitsOffset),
            Leading(ProgressionSliceLayout.RoomChozoBitsOffset, ProgressionSliceLayout.CollectedItemBitsOffset),
            Leading(ProgressionSliceLayout.CollectedItemBitsOffset, ProgressionSliceLayout.OpenedDoorBitsOffset),
            Leading(ProgressionSliceLayout.OpenedDoorBitsOffset, ProgressionSliceLayout.PaddingOffset),
            Leading(ProgressionSliceLayout.UsedSaveStationsOffset, ProgressionSliceLayout.MapStationsOffset),
            Leading(ProgressionSliceLayout.MapStationsOffset, ProgressionSliceLayout.LoadingGameStateOffset),
            loadingState);
    }

    /// <summary>
    /// Where the Chainsaw fuse's fast-list offset moves every animation position that lands on one; a position whose move
    /// lands on no list position is left out, and the game fails if its shot ever reaches it.
    /// </summary>
    private static Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> ChainsawFuseLandings(SamusProjectileListLayout lists)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>>();
        foreach (SamusProjectileProgram program in Enum.GetValues<SamusProjectileProgram>())
        {
            var landings = new Dictionary<string, SamusOverreadDocument.Cursor>();
            for (int position = 0; position < SamusProjectileAnimations.Programs.Steps(program).Count; position++)
            {
                int landing = unchecked((ushort)(lists.Position(new AnimationProgramCursor<SamusProjectileProgram>(program, position)) +
                    SamusOverreadSourceDefinitions.FastAnimationOffset));
                if (landing != 0 && lists.TryCursorAt(landing, out AnimationProgramCursor<SamusProjectileProgram> landed))
                    landings.Add(position.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        new SamusOverreadDocument.Cursor(landed.Program.ToString(), landed.Position));
            }
            result.Add(program.ToString(), landings);
        }
        return result;
    }

    /// <summary>The bytes stored just before the charge flare's programs, nearest first: the table naming the programs.</summary>
    private static int[] ChargeFlareDelaysBelowPrograms(IImportCartridgeSource cartridge) =>
        [.. Enumerable.Range(1, ChargeFlareAnimationDefinitions.ProgramTableByteCount)
            .Select(distance => (int)cartridge.ReadCartridgeByte(SamusSourceDefinitions.ChargeFlarePrograms.Value - distance))];

    /// <summary>
    /// Whether the posture-transition admission admits weapons for each pose outside its table: it reads one byte per pose
    /// number from the table's place for pose zero, and a zero admits weapons.
    /// </summary>
    private static Dictionary<string, bool> PostureAdmissions(IImportCartridgeSource cartridge) =>
        Enum.GetValues<SamusPoseId>()
            .Where(pose => SamusPoseNumbering.Precedes(pose, SamusHudDefinitions.NonFiringTransitionStart) &&
                !SamusPoseNumbering.InRange(pose, SamusHudDefinitions.FirstTransitionPose, SamusPoseId.UnusedPose40))
            .ToDictionary(pose => pose.ToString(), pose =>
                cartridge.ReadCartridgeByte(SamusOverreadSourceDefinitions.HudPostureFlagsForPoseZero + SamusPoseNumbering.Number(pose)) == 0);

    /// <summary>
    /// The colours Gravity copies on each Speed Booster step past its cycle: the expansion window's open-bus words, then
    /// the Grapple handler's code read as colour words.
    /// </summary>
    private static Dictionary<string, IReadOnlyList<PaletteRgb5>> SpeedBoosterOverrunColours(IImportCartridgeSource cartridge) => new()
    {
        [SpeedBoosterPaletteOverrun.FirstStepPastCycle.ToString()] = [.. Enumerable.Range(0, SamusPaletteDefinitions.Common.ColoursPerRow)
            .Select(_ => PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(SamusOverreadSourceDefinitions.ExpansionOpenBusWord)))],
        [SpeedBoosterPaletteOverrun.SecondStepPastCycle.ToString()] = [.. Enumerable.Range(0, SamusPaletteDefinitions.Common.ColoursPerRow)
            .Select(index => PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(cartridge.ReadCartridgeWord(
                SamusOverreadSourceDefinitions.SpeedBoosterGrappleCode + index * RgbColor.ByteCount))))],
    };
}
