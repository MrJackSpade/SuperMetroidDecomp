using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SamusFramesBeyondAnimation = SuperMetroid.Core.Compatibility.FramesBeyondAnimation<SuperMetroid.Core.Samus.Poses.SamusPoseId, SuperMetroid.Core.Samus.Poses.SamusAnimationFrame, SuperMetroid.Core.Samus.Poses.SamusAnimationPlaceName, SuperMetroid.Core.Samus.Poses.SamusPoseAnimationStep>;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Validates an installed overread document and builds the Samus overread services from it (#1474).</summary>
internal static class SamusOverreadServicesReader
{
    /// <summary>Every story event by its name.</summary>
    private static readonly IReadOnlyDictionary<string, StoryEvent> StoryEventsByName =
        Enum.GetValues<StoryEvent>().ToDictionary(storyEvent => storyEvent.ToString(), StringComparer.Ordinal);

    /// <summary>Every item location by its name.</summary>
    private static readonly IReadOnlyDictionary<string, ItemLocation> ItemLocationsByName =
        ItemLocationDefinitions.All.ToDictionary(location => location.Name, StringComparer.Ordinal);

    /// <summary>Every locked door by its name.</summary>
    private static readonly IReadOnlyDictionary<string, LockedDoor> LockedDoorsByName =
        LockedDoorDefinitions.All.ToDictionary(door => door.Name, StringComparer.Ordinal);

    /// <summary>Every station the used-station record keeps, by its name.</summary>
    private static readonly IReadOnlyDictionary<string, LoadStation> UsedStationsByName =
        LoadStationDefinitions.Usable.ToDictionary(station => station.Name, StringComparer.Ordinal);

    /// <summary>Builds every service, requiring an entry for every situation each one covers.</summary>
    /// <param name="document">The installed overread document.</param>
    /// <param name="straddledTerrain">The installed straddled terrain (#1612).</param>
    /// <exception cref="InvalidDataException">The document is incomplete or names something the game does not have.</exception>
    internal static SamusOverreadServices Read(SamusOverreadDocument document, StraddledTerrain<RoomTerrainBlock> straddledTerrain)
    {
        Ensure.NotNull(document);
        Ensure.NotNull(straddledTerrain);
        if (document.Version != SamusOverreadFormat.Version)
            throw new InvalidDataException("Samus overread file has an unsupported version.");
        var projectiles = new SamusProjectileOverreads(
            SpaceTimeOverwrite(Require(document.SpaceTimeOverwrites, nameof(document.SpaceTimeOverwrites))),
            ChainsawFuseShift(Require(document.ChainsawFuseLandings, nameof(document.ChainsawFuseLandings))),
            PostureAdmission(Require(document.PostureAdmissions, nameof(document.PostureAdmissions))),
            UnauthoredFiring(Require(document.UnauthoredBeamLaunchSpeeds, nameof(document.UnauthoredBeamLaunchSpeeds)),
                Require(document.UnauthoredBeamCooldowns, nameof(document.UnauthoredBeamCooldowns))),
            DrainedCrashDepartures(Require(document.DrainedCrashDepartures, nameof(document.DrainedCrashDepartures))));
        return new SamusOverreadServices(
            FramesBeyondAnimationOf(document),
            projectiles,
            straddledTerrain);
    }

    /// <summary>
    /// What every pose's animation reads where its frame names no step of its own (#1535, #2048): the step each cue value's
    /// frame reads, what each frame read past the animation reaches, and the frame each named place lands on in every other
    /// animation, each named as the installation names it. The installation holds each program's outcomes once, under the
    /// pose naming it, and every pose of the program reads them.
    /// </summary>
    /// <exception cref="InvalidDataException">An outcome is missing, names no frame, or reaches a frame that names no step.</exception>
    private static SamusFramesBeyondAnimation FramesBeyondAnimationOf(SamusOverreadDocument document)
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> cueValueSteps =
            Require(document.AnimationCueValueSteps, nameof(document.AnimationCueValueSteps));
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>> readsPast =
            Require(document.AnimationReadsPast, nameof(document.AnimationReadsPast));
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> landings =
            Require(document.AnimationPlaceLandings, nameof(document.AnimationPlaceLandings));
        Dictionary<SamusPoseId, SamusPoseId> naming = SamusAnimationProgramLayout.NamingPoses();
        SamusPoseId[] programs = [.. naming.Values.Distinct()];
        if (cueValueSteps.Count != programs.Length)
            throw new InvalidDataException($"Samus overread file holds cue-value steps for {cueValueSteps.Count} programs, not {programs.Length}.");
        if (readsPast.Count != programs.Length)
            throw new InvalidDataException($"Samus overread file holds reads past {readsPast.Count} programs, not {programs.Length}.");
        var programCueValues = new Dictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, SamusPoseAnimationStep>>();
        foreach (SamusPoseId program in programs)
        {
            if (!cueValueSteps.TryGetValue(program.ToString(), out IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>? steps))
                throw new InvalidDataException($"Samus overread file has no cue-value steps for {program}'s program.");
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(program);
            programCueValues.Add(program, Require(steps, $"cue-value steps for {program}").ToDictionary(
                pair => NoStepOf(animation, animation.FrameNamed(pair.Key), program, "cue-value frame"),
                pair => CueValueStep(program, Require(pair.Value, $"{program} cue-value step"))));
        }
        SamusPoseAnimationStep? StepAt(SamusPoseId pose, SamusAnimationFrame frame) =>
            SamusAnimationDelays.AnimationForPose(pose).TryStepAt(frame) ??
            (programCueValues[naming[pose]].TryGetValue(frame, out SamusPoseAnimationStep? step) ? step : null);
        var programReads = new Dictionary<SamusPoseId, IReadOnlyDictionary<SamusAnimationFrame, SamusFramesBeyondAnimation.Read>>();
        foreach (SamusPoseId program in programs)
        {
            if (!readsPast.TryGetValue(program.ToString(), out IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>? entries))
                throw new InvalidDataException($"Samus overread file has no reads past {program}'s program.");
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(program);
            programReads.Add(program, Require(entries, $"reads past {program}'s program").ToDictionary(
                pair => StepAt(program, animation.FrameNamed(pair.Key)) is null
                    ? animation.FrameNamed(pair.Key)
                    : throw new InvalidDataException($"Samus overread file reads past {program}'s animation at '{pair.Key}', which names a step of its own."),
                pair =>
                {
                    SamusOverreadDocument.ReadPast entry = Require(pair.Value, $"{program} read past its animation");
                    SamusPoseId reader = Parse<SamusPoseId>(entry.Pose, "pose read past into");
                    if (!naming.ContainsKey(reader))
                        throw new InvalidDataException($"Samus overread file reads past {program}'s animation into {reader}, which has no animation program.");
                    SamusPoseAnimation readAnimation = SamusAnimationDelays.AnimationForPose(reader);
                    SamusAnimationFrame frame = readAnimation.FrameNamed(entry.Frame);
                    if (StepAt(reader, frame) is null)
                        throw new InvalidDataException($"Samus overread file reads past {program}'s animation onto '{entry.Frame}' of {reader}'s, which names no step.");
                    return new SamusFramesBeyondAnimation.Read(reader, frame, Require(entry.Moves, $"{program} read past moves")
                        .ToDictionary(move => readAnimation.FrameNamed(move.Key), move => animation.FrameNamed(move.Value)));
                }));
        }
        var places = new Dictionary<SamusAnimationPlaceName, IReadOnlyDictionary<SamusPoseId, SamusAnimationFrame>>();
        foreach (SamusAnimationPlace place in SamusAnimationPlaces.All)
        {
            if (!landings.TryGetValue(place.Name.ToString(), out IReadOnlyDictionary<string, string>? frames))
                throw new InvalidDataException($"Samus overread file has no landings of place {place.Name}.");
            SamusPoseId[] met = [.. programs.Where(program => !ReferenceEquals(SamusAnimationDelays.AnimationForPose(program), place.Animation))];
            if (Require(frames, $"landings of place {place.Name}").Count != met.Length)
                throw new InvalidDataException($"Samus overread file holds landings of place {place.Name} in {frames.Count} programs, not {met.Length}.");
            Dictionary<SamusPoseId, SamusAnimationFrame> landed = met.ToDictionary(program => program, program => frames.TryGetValue(program.ToString(), out string? frame)
                ? SamusAnimationDelays.AnimationForPose(program).FrameNamed(frame)
                : throw new InvalidDataException($"Samus overread file has no landing of place {place.Name} in {program}'s program."));
            places.Add(place.Name, naming.Where(pose => landed.ContainsKey(pose.Value)).ToDictionary(pose => pose.Key, pose => landed[pose.Value]));
        }
        if (landings.Count != places.Count)
            throw new InvalidDataException($"Samus overread file holds landings of {landings.Count} places, not {places.Count}.");
        return new SamusFramesBeyondAnimation(
            naming.ToDictionary(pose => pose.Key, pose => programCueValues[pose.Value]),
            naming.ToDictionary(pose => pose.Key, pose => programReads[pose.Value]),
            places);
    }

    /// <summary><paramref name="frame"/>, which must name no step of <paramref name="animation"/>'s own.</summary>
    /// <exception cref="InvalidDataException">The frame names one of the animation's own steps.</exception>
    private static SamusAnimationFrame NoStepOf(SamusPoseAnimation animation, SamusAnimationFrame frame, SamusPoseId pose, string what) =>
        animation.TryStepAt(frame) is null
            ? frame
            : throw new InvalidDataException($"Samus overread file names {what} {animation.NameOf(frame)} of {pose}'s animation, which is a step of its own.");

    /// <summary>
    /// The step a cue value's frame of <paramref name="pose"/>'s animation reads, from its decoded kind and the values it
    /// names; a frame it names is a named frame of the pose's animation.
    /// </summary>
    /// <exception cref="InvalidDataException">The step lacks a value its kind needs, or carries one it does not.</exception>
    internal static SamusPoseAnimationStep CueValueStep(SamusPoseId pose, SamusOverreadDocument.CueValueStep step)
    {
        Ensure.NotNull(step);
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        SamusOverreadDocument.CueValueStepKind kind = Parse<SamusOverreadDocument.CueValueStepKind>(step.Kind, "cue-value step kind");
        SamusPoseId Pose(int index, int count) =>
            Require(step.Poses, $"{kind} poses") is { } poses && poses.Count == count
                ? Parse<SamusPoseId>(poses[index], $"{kind} pose")
                : throw new InvalidDataException($"Samus overread file's {kind} cue-value step names {step.Poses?.Count} poses, not {count}.");
        SamusAnimationTarget Frame(int index, int count)
        {
            IReadOnlyList<string> frames = Require(step.Frames, $"{kind} frames");
            if (frames.Count != count)
                throw new InvalidDataException($"Samus overread file's {kind} cue-value step names no valid frame {index} of {count}.");
            SamusAnimationFrame frame = animation.FrameNamed(frames[index]);
            return new SamusAnimationTarget(animation.SegmentName(frame), frame.Frame);
        }
        SamusEquipmentFlags Items() => Require(step.Items, $"{kind} items")
            .Aggregate(SamusEquipmentFlags.None, (items, name) => items | Parse<SamusEquipmentFlags>(name, "cue-value step item"));
        return kind switch
        {
            SamusOverreadDocument.CueValueStepKind.Delay => step.Ticks is >= 0 and < SamusOverreadSourceDefinitions.FirstAnimationInstructionByte
                ? new SamusAnimationDelay(step.Ticks.Value)
                : throw new InvalidDataException($"Samus overread file's cue-value delay holds {step.Ticks} ticks."),
            SamusOverreadDocument.CueValueStepKind.Hold => new SamusAnimationHold(),
            SamusOverreadDocument.CueValueStepKind.LoopUnlessLowEnergy => new SamusAnimationLoopUnlessLowEnergy(),
            SamusOverreadDocument.CueValueStepKind.InstallDrainedFall => new SamusAnimationInstallDrainedFall(),
            SamusOverreadDocument.CueValueStepKind.TurnTransition => new SamusAnimationTurnTransition(Pose(0, 1)),
            SamusOverreadDocument.CueValueStepKind.ItemAirborneTransition =>
                new SamusAnimationItemAirborneTransition(Items(), Pose(0, 4), Pose(1, 4), Pose(2, 4), Pose(3, 4)),
            SamusOverreadDocument.CueValueStepKind.VerticalSpeedTransition => new SamusAnimationVerticalSpeedTransition(Pose(0, 2), Pose(1, 2)),
            SamusOverreadDocument.CueValueStepKind.WallJumpSelect => new SamusAnimationWallJumpSelect(Frame(0, 3), Frame(1, 3), Frame(2, 3)),
            SamusOverreadDocument.CueValueStepKind.ItemTransition => new SamusAnimationItemTransition(Items(), Pose(0, 2), Pose(1, 2)),
            SamusOverreadDocument.CueValueStepKind.Transition => new SamusAnimationTransition(Pose(0, 1)),
            SamusOverreadDocument.CueValueStepKind.Repeat => new SamusAnimationRepeat(Frame(0, 1)),
            SamusOverreadDocument.CueValueStepKind.Restart => new SamusAnimationRestart(),
            _ => throw new InvalidDataException($"Samus overread file names cue-value step kind {kind}, which has no step."),
        };
    }

    /// <summary>Every SpaceTime animation position's overwrite, by slot.</summary>
    private static SpaceTimeProgressionOverwrite<SamusProjectileSegmentBeat, ProgressionOverwrite> SpaceTimeOverwrite(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>> overwrites)
    {
        var result = new Dictionary<SamusProjectileSegmentBeat, IReadOnlyList<ProgressionOverwrite?>>();
        foreach ((string programName, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>> positions) in overwrites)
        {
            SamusProjectileSegment program = Parse<SamusProjectileSegment>(programName, "SpaceTime program");
            // A segment's places: before each beat, and at its end.
            int steps = SamusProjectileAnimations.Segments.Beats(program).Count + 1;
            if (positions.Count != steps)
                throw new InvalidDataException($"Samus overread file holds SpaceTime overwrites for {positions.Count} of {program}'s {steps} steps.");
            foreach ((string positionName, IReadOnlyList<SamusOverreadDocument.Overwrite?> bySlot) in positions)
            {
                int position = ParseCount(positionName, $"{program} step");
                if (position >= steps)
                    throw new InvalidDataException($"Samus overread file names step {position} of {program}, which has {steps}.");
                if (bySlot.Count != SamusProjectileSystem.PlaceCount)
                    throw new InvalidDataException($"Samus overread file holds SpaceTime overwrites for {bySlot.Count} slots, not {SamusProjectileSystem.PlaceCount}.");
                result.Add(new(program, position), [.. bySlot.Select(Overwrite)]);
            }
        }
        if (result.Count == 0)
            throw new InvalidDataException("Samus overread file holds no SpaceTime overwrites.");
        return new(result);
    }

    /// <summary>One overwrite, its progress and startup route named by the identities the game knows them by.</summary>
    /// <exception cref="InvalidDataException">
    /// The overwrite names something the game does not have, names a member twice, leaves on a member it does not decide,
    /// or leaves a Zebetite count the game cannot hold.
    /// </exception>
    private static ProgressionOverwrite? Overwrite(SamusOverreadDocument.Overwrite? overwrite) => overwrite is null
        ? null
        : new ProgressionOverwrite(
            Decided(Require(overwrite.Events, nameof(overwrite.Events)), StoryEventsByName, "SpaceTime story event"),
            overwrite.ZebetitesDestroyed is null or (>= 0 and <= PersistentProgress.ZebetiteCount)
                ? overwrite.ZebetitesDestroyed
                : throw new InvalidDataException(
                    $"Samus overread file's SpaceTime overwrite leaves {overwrite.ZebetitesDestroyed} Zebetites destroyed, not 0 through {PersistentProgress.ZebetiteCount}."),
            Require(overwrite.AreaBosses, nameof(overwrite.AreaBosses)).ToDictionary(
                area => Parse<AreaId>(area.Key, "SpaceTime boss area"),
                area => area.Value.Aggregate(DefeatedBosses.None, (bosses, name) => bosses | Parse<DefeatedBosses>(name, "SpaceTime boss"))),
            Decided(Require(overwrite.CollectedItems, nameof(overwrite.CollectedItems)), ItemLocationsByName, "SpaceTime item location"),
            Decided(Require(overwrite.OpenedDoors, nameof(overwrite.OpenedDoors)), LockedDoorsByName, "SpaceTime door"),
            Decided(Require(overwrite.UsedStations, nameof(overwrite.UsedStations)), UsedStationsByName, "SpaceTime used station"),
            Require(overwrite.AreaMaps, nameof(overwrite.AreaMaps)).ToDictionary(
                area => Parse<AreaId>(area.Key, "SpaceTime map area"), area => area.Value),
            overwrite.SavedStartupRoute is null ? null : Parse<SaveLoadingGameState>(overwrite.SavedStartupRoute, "SpaceTime startup route"));

    /// <summary>What an overwrite decides for one kind of progress, its members found by name.</summary>
    /// <exception cref="InvalidDataException">A name names nothing of the kind, is listed twice, or is left on without being decided.</exception>
    private static DecidedFlags<T> Decided<T>(SamusOverreadDocument.DecidedMembers members, IReadOnlyDictionary<string, T> byName, string what)
        where T : notnull
    {
        HashSet<T> Members(IReadOnlyList<string>? names, string list)
        {
            var found = new HashSet<T>();
            foreach (string? name in Require(names, $"{what} {list} list"))
            {
                if (name is null || !byName.TryGetValue(name, out T? member))
                    throw new InvalidDataException($"Samus overread file names no {what} '{name}'.");
                if (!found.Add(member))
                    throw new InvalidDataException($"Samus overread file lists {what} '{name}' twice among those its overwrite {list}.");
            }
            return found;
        }
        HashSet<T> decided = Members(members.Decided, "decides");
        HashSet<T> set = Members(members.Set, "leaves on");
        if (!set.IsSubsetOf(decided))
            throw new InvalidDataException($"Samus overread file's overwrite leaves on a {what} it does not decide.");
        return new DecidedFlags<T>(decided, set);
    }

    /// <summary>Every animation position the Chainsaw fuse lands somewhere.</summary>
    private static ChainsawFuseAnimationShift<SamusProjectileSegmentBeat> ChainsawFuseShift(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> landings)
    {
        var result = new Dictionary<SamusProjectileSegmentBeat, SamusProjectileSegmentBeat>();
        foreach ((string programName, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor> positions) in landings)
        {
            SamusProjectileSegment program = Parse<SamusProjectileSegment>(programName, "Chainsaw program");
            foreach ((string positionName, SamusOverreadDocument.Cursor landing) in positions)
                result.Add(new(program, ParseCount(positionName, $"{program} step")), Cursor(landing));
        }
        if (landings.Count != Enum.GetValues<SamusProjectileSegment>().Length)
            throw new InvalidDataException($"Samus overread file holds Chainsaw landings for {landings.Count} programs.");
        return new(result);
    }

    /// <summary>A decoded animation place, which must name a place of its segment: before one of its beats, or at its end.</summary>
    private static SamusProjectileSegmentBeat Cursor(SamusOverreadDocument.Cursor cursor)
    {
        Ensure.NotNull(cursor);
        SamusProjectileSegment animation = Parse<SamusProjectileSegment>(cursor.Animation, "animation position's animation");
        return (uint)cursor.Position <= (uint)SamusProjectileAnimations.Segments.Beats(animation).Count
            ? new(animation, cursor.Position)
            : throw new InvalidDataException($"Samus overread file names step {cursor.Position} of {animation}, which it does not have.");
    }

    /// <summary>Whether each pose outside the transition table admits weapons.</summary>
    private static PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission(IReadOnlyDictionary<string, bool> admissions)
    {
        if (admissions.Count == 0)
            throw new InvalidDataException("Samus overread file holds no posture admissions.");
        return new(admissions.ToDictionary(pair => Parse<SamusPoseId>(pair.Key, "posture-admission pose"), pair => pair.Value));
    }

    /// <summary>The launch speeds and cooldowns of every Spazer and Plasma combination, which the firing tables do not author.</summary>
    private static UnauthoredBeamFiring<SamusBeamCombination> UnauthoredFiring(
        IReadOnlyDictionary<string, SamusOverreadDocument.LaunchSpeeds> speeds,
        IReadOnlyDictionary<string, SamusOverreadDocument.Cooldowns> cooldowns)
    {
        var cardinal = new Dictionary<SamusBeamCombination, int>();
        var diagonal = new Dictionary<SamusBeamCombination, int>();
        var waits = new Dictionary<SamusBeamCombination, BeamShotCooldowns>();
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>().Where(beams => !beams.IsRetail))
        {
            if (!speeds.TryGetValue(beams.ToString(), out SamusOverreadDocument.LaunchSpeeds? pair) || pair is null ||
                pair.Cardinal is < short.MinValue or > short.MaxValue || pair.Diagonal is < short.MinValue or > short.MaxValue)
                throw new InvalidDataException($"Samus overread file holds no valid launch speeds for {beams}.");
            cardinal.Add(beams, pair.Cardinal);
            diagonal.Add(beams, pair.Diagonal);
            if (!cooldowns.TryGetValue(beams.ToString(), out SamusOverreadDocument.Cooldowns? wait) || wait is null ||
                wait.Uncharged is < 0 or > byte.MaxValue || wait.Charged is < 0 or > byte.MaxValue || wait.HeldFire is < 0 or > byte.MaxValue)
                throw new InvalidDataException($"Samus overread file holds no valid cooldowns for {beams}.");
            waits.Add(beams, new BeamShotCooldowns(wait.Uncharged, wait.Charged, wait.HeldFire));
        }
        if (speeds.Count != cardinal.Count)
            throw new InvalidDataException($"Samus overread file holds launch speeds for {speeds.Count} combinations, not {cardinal.Count}.");
        if (cooldowns.Count != waits.Count)
            throw new InvalidDataException($"Samus overread file holds cooldowns for {cooldowns.Count} combinations, not {waits.Count}.");
        return new(cardinal, diagonal, waits);
    }

    /// <summary>The echo angles of a crash finishing in each drained pose a crash can finish in.</summary>
    private static DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> DrainedCrashDepartures(
        IReadOnlyDictionary<string, SamusOverreadDocument.CrashDepartures> departures)
    {
        var result = new Dictionary<SamusPoseId, ShinesparkDepartureAngles>();
        foreach (SamusPoseId pose in SamusOverreadDecoding.DrainedCrashPoses)
        {
            if (!departures.TryGetValue(pose.ToString(), out SamusOverreadDocument.CrashDepartures? pair) || pair is null ||
                pair.First is < 0 or > byte.MaxValue || pair.Second is < 0 or > byte.MaxValue)
                throw new InvalidDataException($"Samus overread file holds no valid crash departures for {pose}.");
            result.Add(pose, ShinesparkEchoDefinitions.Departure(pair.First, pair.Second));
        }
        if (departures.Count != result.Count)
            throw new InvalidDataException($"Samus overread file holds crash departures for {departures.Count} poses, not {result.Count}.");
        return new(result);
    }

    /// <summary>Requires a document member.</summary>
    private static T Require<T>(T? value, string name) where T : class =>
        value ?? throw new InvalidDataException($"Samus overread file has no {name}.");

    /// <summary>Parses a defined enum member by its exact name.</summary>
    private static T Parse<T>(string? name, string what) where T : struct, Enum =>
        name is { Length: > 0 } && !char.IsDigit(name[0]) && Enum.TryParse(name, ignoreCase: false, out T value) && Enum.IsDefined(value)
            ? value
            : throw new InvalidDataException($"Samus overread file names no {what} '{name}'.");

    /// <summary>Parses a nonnegative decimal count or index.</summary>
    private static int ParseCount(string text, string what) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
            ? value
            : throw new InvalidDataException($"Samus overread file names no {what} '{text}'.");
}
