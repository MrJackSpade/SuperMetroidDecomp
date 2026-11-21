using SuperMetroid.Core.Rooms.LevelData;
using System.Globalization;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SamusFramesBeyondAnimation = SuperMetroid.Core.Compatibility.FramesBeyondAnimation<SuperMetroid.Core.Samus.Poses.SamusAnimationName, SuperMetroid.Core.Samus.Poses.SamusAnimationFrame, SuperMetroid.Core.Samus.Poses.SamusAnimationPlaceName, SuperMetroid.Core.Samus.Poses.SamusPoseAnimationStep>;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Validates an installed overread document and builds the Samus overread services from it (#1474).</summary>
internal static class SamusOverreadServicesReader
{
    /// <summary>Every story event by its name.</summary>
    private static readonly IReadOnlyDictionary<string, StoryEvent> StoryEventsByName =
        Enum.GetValues<StoryEvent>().ToDictionary(storyEvent => StoryEventKeys.Installed.Of(storyEvent), StringComparer.Ordinal);

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
            SpaceTimeOverwrite(document.SpaceTimeOverwrites),
            ChainsawFuseShift(document.ChainsawFuseLandings),
            PostureAdmission(document.PostureAdmissions),
            UnauthoredFiring(document.UnauthoredBeamLaunchSpeeds,
                document.UnauthoredBeamCooldowns),
            DrainedCrashDepartures(document.DrainedCrashDepartures));
        return new SamusOverreadServices(
            FramesBeyondAnimationOf(document),
            projectiles,
            straddledTerrain);
    }

    /// <summary>
    /// What every animation reads where its frame names no step of its own (#1535, #2048): the step each cue value's frame
    /// reads, what each frame read past the animation reaches, and the frame each named place lands on in every other
    /// animation, each named as the installation names it. The installation holds each animation's outcomes once, under its
    /// name, and every pose animating by it reads them.
    /// </summary>
    /// <exception cref="InvalidDataException">An outcome is missing, names no frame, or reaches a frame that names no step.</exception>
    private static SamusFramesBeyondAnimation FramesBeyondAnimationOf(SamusOverreadDocument document)
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> cueValueSteps =
            document.AnimationCueValueSteps;
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> framesAfterCues =
            document.AnimationFramesAfterCues;
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>> readsPast =
            document.AnimationReadsPast;
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> landings =
            document.AnimationPlaceLandings;
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> delaysRead =
            document.AnimationDelaysRead;
        IReadOnlyList<SamusPoseAnimation> animations = SamusAnimationDelayDefinitions.All;
        if (cueValueSteps.Count != animations.Count)
            throw new InvalidDataException($"Samus overread file holds cue-value steps for {cueValueSteps.Count} animations, not {animations.Count}.");
        if (framesAfterCues.Count != animations.Count)
            throw new InvalidDataException($"Samus overread file holds frames after cues for {framesAfterCues.Count} animations, not {animations.Count}.");
        if (readsPast.Count != animations.Count)
            throw new InvalidDataException($"Samus overread file holds reads past {readsPast.Count} animations, not {animations.Count}.");
        if (delaysRead.Count != animations.Count)
            throw new InvalidDataException($"Samus overread file holds delays read for {delaysRead.Count} animations, not {animations.Count}.");
        var animationCueValues = new Dictionary<SamusAnimationName, IReadOnlyDictionary<SamusAnimationFrame, SamusPoseAnimationStep>>();
        foreach (SamusPoseAnimation animation in animations)
        {
            if (!cueValueSteps.TryGetValue(SamusAnimationNameKeys.Installed.Of(animation.Name), out IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>? steps))
                throw new InvalidDataException($"Samus overread file has no cue-value steps for the {animation.Name} animation.");
            animationCueValues.Add(animation.Name, steps.ToDictionary(
                pair => NoStepOf(animation, animation.FrameNamed(pair.Key), "cue-value frame"),
                pair => CueValueStep(animation, pair.Value)));
        }
        var animationFramesAfterCues = new Dictionary<SamusAnimationName, IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>>();
        foreach (SamusPoseAnimation animation in animations)
        {
            if (!framesAfterCues.TryGetValue(SamusAnimationNameKeys.Installed.Of(animation.Name), out IReadOnlyDictionary<string, string>? frames))
                throw new InvalidDataException($"Samus overread file has no frames after the {animation.Name} animation's cues.");
            animationFramesAfterCues.Add(animation.Name, frames.ToDictionary(
                pair => PastCueOf(animation, animation.FrameNamed(pair.Key)), pair => animation.FrameNamed(pair.Value)));
        }
        bool HasStepAt(SamusPoseAnimation animation, SamusAnimationFrame frame) =>
            animation.TryStepAt(frame) is not null || animationCueValues[animation.Name].ContainsKey(frame);
        var animationReads = new Dictionary<SamusAnimationName, IReadOnlyDictionary<SamusAnimationFrame, SamusFramesBeyondAnimation.Read>>();
        foreach (SamusPoseAnimation animation in animations)
        {
            if (!readsPast.TryGetValue(SamusAnimationNameKeys.Installed.Of(animation.Name), out IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>? entries))
                throw new InvalidDataException($"Samus overread file has no reads past the {animation.Name} animation.");
            animationReads.Add(animation.Name, entries.ToDictionary(
                pair => !HasStepAt(animation, animation.FrameNamed(pair.Key))
                    ? animation.FrameNamed(pair.Key)
                    : throw new InvalidDataException($"Samus overread file reads past the {animation.Name} animation at '{pair.Key}', which names a step of its own."),
                pair =>
                {
                    SamusOverreadDocument.ReadPast entry = pair.Value;
                    SamusAnimationName reached = Parse(SamusAnimationNameKeys.Installed, entry.Animation, "animation read past into");
                    SamusPoseAnimation readAnimation = SamusAnimationDelays.AnimationNamed(reached);
                    SamusAnimationFrame frame = readAnimation.FrameNamed(entry.Frame);
                    if (!HasStepAt(readAnimation, frame))
                        throw new InvalidDataException($"Samus overread file reads past the {animation.Name} animation onto '{entry.Frame}' of the {reached} animation, which names no step.");
                    return new SamusFramesBeyondAnimation.Read(reached, frame, entry.Moves
                        .ToDictionary(move => readAnimation.FrameNamed(move.Key), move => animation.FrameNamed(move.Value)));
                }));
        }
        var places = new Dictionary<SamusAnimationPlaceName, IReadOnlyDictionary<SamusAnimationName, SamusAnimationFrame>>();
        foreach (SamusAnimationPlace place in SamusAnimationPlaces.All)
        {
            if (!landings.TryGetValue(SamusAnimationPlaceNameKeys.Installed.Of(place.Name), out IReadOnlyDictionary<string, string>? frames))
                throw new InvalidDataException($"Samus overread file has no landings of place {place.Name}.");
            SamusPoseAnimation[] met = [.. animations.Where(animation => !ReferenceEquals(animation, place.Animation))];
            if (frames.Count != met.Length)
                throw new InvalidDataException($"Samus overread file holds landings of place {place.Name} in {frames.Count} animations, not {met.Length}.");
            places.Add(place.Name, met.ToDictionary(animation => animation.Name, animation => frames.TryGetValue(SamusAnimationNameKeys.Installed.Of(animation.Name), out string? frame)
                ? animation.FrameNamed(frame)
                : throw new InvalidDataException($"Samus overread file has no landing of place {place.Name} in the {animation.Name} animation.")));
        }
        if (landings.Count != places.Count)
            throw new InvalidDataException($"Samus overread file holds landings of {landings.Count} places, not {places.Count}.");
        var animationDelaysRead = new Dictionary<SamusAnimationName, IReadOnlyDictionary<SamusAnimationFrame, int>>();
        foreach (SamusPoseAnimation animation in animations)
        {
            if (!delaysRead.TryGetValue(SamusAnimationNameKeys.Installed.Of(animation.Name), out IReadOnlyDictionary<string, int>? ticks))
                throw new InvalidDataException($"Samus overread file has no delays read for the {animation.Name} animation.");
            animationDelaysRead.Add(animation.Name, ticks.ToDictionary(
                pair => animation.TryDelayAt(animation.FrameNamed(pair.Key)) is null
                    ? animation.FrameNamed(pair.Key)
                    : throw new InvalidDataException($"Samus overread file reads a delay at '{pair.Key}' of the {animation.Name} animation, which holds one of its own."),
                pair => pair.Value is >= 0 and <= byte.MaxValue
                    ? pair.Value
                    : throw new InvalidDataException($"Samus overread file reads delay {pair.Value} at '{pair.Key}' of the {animation.Name} animation, which no byte holds.")));
        }
        return new SamusFramesBeyondAnimation(animationCueValues, animationFramesAfterCues, animationReads, places, animationDelaysRead);
    }

    /// <summary>
    /// <paramref name="frame"/>, which must lie on or past the cue ending one of <paramref name="animation"/>'s segments other
    /// than its last.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is one of the segment's delays, or a frame of the last segment.</exception>
    private static SamusAnimationFrame PastCueOf(SamusPoseAnimation animation, SamusAnimationFrame frame) =>
        frame.Segment != animation.Segments[^1].Name && animation.Segment(frame.Segment) is { End: not null } segment && frame.Frame >= segment.CueFrame
            ? frame
            : throw new InvalidDataException($"Samus overread file names a frame after {animation.NameOf(frame)} of the {animation.Name} animation, which lies on no segment's cue.");

    /// <summary><paramref name="frame"/>, which must name no step of <paramref name="animation"/>'s own.</summary>
    /// <exception cref="InvalidDataException">The frame names one of the animation's own steps.</exception>
    private static SamusAnimationFrame NoStepOf(SamusPoseAnimation animation, SamusAnimationFrame frame, string what) =>
        animation.TryStepAt(frame) is null
            ? frame
            : throw new InvalidDataException($"Samus overread file names {what} {animation.NameOf(frame)} of the {animation.Name} animation, which is a step of its own.");

    /// <summary>
    /// The step a cue value's frame of <paramref name="animation"/> reads, from its decoded kind and the values it names; a
    /// frame it names is a named frame of the animation.
    /// </summary>
    /// <exception cref="InvalidDataException">The step lacks a value its kind needs, or carries one it does not.</exception>
    internal static SamusPoseAnimationStep CueValueStep(SamusPoseAnimation animation, SamusOverreadDocument.CueValueStep step)
    {
        Ensure.NotNull(animation);
        Ensure.NotNull(step);
        SamusOverreadDocument.CueValueStepKind kind = Parse(CueValueStepKindKeys.Installed, step.Kind, "cue-value step kind");
        SamusPoseId Pose(int index, int count) =>
            step.Poses is { } poses && poses.Count == count
                ? Parse(SamusPoseIdKeys.Installed, poses[index], $"{kind} pose")
                : throw new InvalidDataException($"Samus overread file's {kind} cue-value step names {step.Poses?.Count} poses, not {count}.");
        SamusAnimationFrame Frame(int index, int count) =>
            step.Frames is { } frames && frames.Count == count
                ? animation.FrameNamed(frames[index])
                : throw new InvalidDataException($"Samus overread file's {kind} cue-value step names no valid frame {index} of {count}.");
        SamusEquipmentFlags Items() => (step.Items is { } items && items.Count >= 0 ? items
                : throw new InvalidDataException($"Samus overread file's {kind} cue-value step names no items."))
            .Aggregate(SamusEquipmentFlags.None, (items, name) => items | Parse(SamusEquipmentFlagsKeys.Installed, name, "cue-value step item"));
        return kind switch
        {
            SamusOverreadDocument.CueValueStepKind.Delay => step.Ticks is >= 0 and <= SamusAnimationDelay.LongestTicks
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

    /// <summary>Every SpaceTime animation position's overwrite, by shot place.</summary>
    private static SpaceTimeProgressionOverwrite<SamusProjectileSegmentBeat, ProgressionOverwrite> SpaceTimeOverwrite(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Overwrite?>>> overwrites)
    {
        var result = new Dictionary<SamusProjectileSegmentBeat, IReadOnlyDictionary<int, ProgressionOverwrite?>>();
        foreach ((string programName, IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Overwrite?>> positions) in overwrites)
        {
            SamusProjectileSegment program = Parse(SamusProjectileSegmentKeys.Installed, programName, "SpaceTime program");
            // A segment's places: before each beat, and at its end.
            int steps = SamusProjectileAnimations.Segments.Beats(program).Count + 1;
            if (positions.Count != steps)
                throw new InvalidDataException($"Samus overread file holds SpaceTime overwrites for {positions.Count} of {program}'s {steps} steps.");
            foreach ((string positionName, IReadOnlyDictionary<string, SamusOverreadDocument.Overwrite?> byPlace) in positions)
            {
                int position = ParseCount(positionName, $"{program} step");
                if (position >= steps)
                    throw new InvalidDataException($"Samus overread file names step {position} of {program}, which has {steps}.");
                IEnumerable<int> places = Enumerable.Range(0, SamusProjectileSystem.PlaceCount);
                if (byPlace.Count != SamusProjectileSystem.PlaceCount || places.Any(place => !byPlace.ContainsKey(SamusOverreadFormat.ShotPlaceName(place))))
                    throw new InvalidDataException($"Samus overread file must hold SpaceTime's overwrite from each of the {SamusProjectileSystem.PlaceCount} shot places, by its name.");
                result.Add(new(program, position), places.ToDictionary(place => place, place => Overwrite(byPlace[SamusOverreadFormat.ShotPlaceName(place)])));
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
    private static ProgressionOverwrite? Overwrite(SamusOverreadDocument.Overwrite? absent) => absent is not { } overwrite
        ? null
        : new ProgressionOverwrite(
            Decided(overwrite.Events, StoryEventsByName, "SpaceTime story event"),
            overwrite.ZebetitesDestroyed is null or (>= 0 and <= PersistentProgress.ZebetiteCount)
                ? overwrite.ZebetitesDestroyed
                : throw new InvalidDataException(
                    $"Samus overread file's SpaceTime overwrite leaves {overwrite.ZebetitesDestroyed} Zebetites destroyed, not 0 through {PersistentProgress.ZebetiteCount}."),
            overwrite.AreaBosses.ToDictionary(
                area => Parse(AreaIdKeys.Installed, area.Key, "SpaceTime boss area"),
                area => area.Value.Aggregate(DefeatedBosses.None, (bosses, name) => bosses | Parse(DefeatedBossesKeys.Installed, name, "SpaceTime boss"))),
            Decided(overwrite.CollectedItems, ItemLocationsByName, "SpaceTime item location"),
            Decided(overwrite.OpenedDoors, LockedDoorsByName, "SpaceTime door"),
            Decided(overwrite.UsedStations, UsedStationsByName, "SpaceTime used station"),
            overwrite.AreaMaps.ToDictionary(
                area => Parse(AreaIdKeys.Installed, area.Key, "SpaceTime map area"), area => area.Value),
            overwrite.SavedStartupRoute is null ? null : Parse(SaveStartupRouteKeys.Installed, overwrite.SavedStartupRoute, "SpaceTime startup route"));

    /// <summary>What an overwrite decides for one kind of progress, its members found by name.</summary>
    /// <exception cref="InvalidDataException">A name names nothing of the kind, is listed twice, or is left on without being decided.</exception>
    private static DecidedFlags<T> Decided<T>(SamusOverreadDocument.DecidedMembers members, IReadOnlyDictionary<string, T> byName, string what)
        where T : notnull
    {
        HashSet<T> Members(IReadOnlyList<string> names, string list)
        {
            var found = new HashSet<T>();
            foreach (string name in names)
            {
                if (!byName.TryGetValue(name, out T? member))
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
            SamusProjectileSegment program = Parse(SamusProjectileSegmentKeys.Installed, programName, "Chainsaw program");
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
        SamusProjectileSegment animation = Parse(SamusProjectileSegmentKeys.Installed, cursor.Animation, "animation position's animation");
        return (uint)cursor.Position <= (uint)SamusProjectileAnimations.Segments.Beats(animation).Count
            ? new(animation, cursor.Position)
            : throw new InvalidDataException($"Samus overread file names step {cursor.Position} of {animation}, which it does not have.");
    }

    /// <summary>Whether each pose outside the transition table admits weapons.</summary>
    private static PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission(IReadOnlyDictionary<string, bool> admissions)
    {
        if (admissions.Count == 0)
            throw new InvalidDataException("Samus overread file holds no posture admissions.");
        return new(admissions.ToDictionary(pair => Parse(SamusPoseIdKeys.Installed, pair.Key, "posture-admission pose"), pair => pair.Value));
    }

    /// <summary>The launch speeds and cooldowns of every Spazer and Plasma combination, which the firing tables do not author.</summary>
    private static UnauthoredBeamFiring<SamusBeamCombination> UnauthoredFiring(
        IReadOnlyDictionary<string, SamusOverreadDocument.LaunchSpeeds> speeds,
        IReadOnlyDictionary<string, SamusOverreadDocument.Cooldowns> cooldowns)
    {
        var cardinal = new Dictionary<SamusBeamCombination, SignedEightEight>();
        var diagonal = new Dictionary<SamusBeamCombination, SignedEightEight>();
        var waits = new Dictionary<SamusBeamCombination, BeamShotCooldowns>();
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>().Where(beams => !beams.CanBeEquipped))
        {
            if (!speeds.TryGetValue(SamusBeamCombinationKeys.Installed.Of(beams), out SamusOverreadDocument.LaunchSpeeds? pair) || pair is null)
                throw new InvalidDataException($"Samus overread file holds no launch speeds for {beams}.");
            cardinal.Add(beams, SignedEightEight.FromPixels(pair.Cardinal));
            diagonal.Add(beams, SignedEightEight.FromPixels(pair.Diagonal));
            if (!cooldowns.TryGetValue(SamusBeamCombinationKeys.Installed.Of(beams), out SamusOverreadDocument.Cooldowns? wait) || wait is null ||
                wait.Uncharged < 0 || wait.Charged < 0 || wait.HeldFire < 0)
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
            if (!departures.TryGetValue(SamusPoseIdKeys.Installed.Of(pose), out SamusOverreadDocument.CrashDepartures? pair) || pair is null ||
                pair.First is < 0 or > byte.MaxValue || pair.Second is < 0 or > byte.MaxValue)
                throw new InvalidDataException($"Samus overread file holds no valid crash departures for {pose}.");
            result.Add(pose, ShinesparkEchoDefinitions.Departure(pair.First, pair.Second));
        }
        if (departures.Count != result.Count)
            throw new InvalidDataException($"Samus overread file holds crash departures for {departures.Count} poses, not {result.Count}.");
        return new(result);
    }

    /// <summary>Parses the member <paramref name="keys"/> spells <paramref name="name"/> by.</summary>
    private static T Parse<T>(InstalledKeys<T> keys, string? name, string what) where T : struct, Enum =>
        keys.TryParse(name, out T value)
            ? value
            : throw new InvalidDataException($"Samus overread file names no {what} '{name}'.");

    /// <summary>Parses a nonnegative decimal count or index.</summary>
    private static int ParseCount(string text, string what) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
            ? value
            : throw new InvalidDataException($"Samus overread file names no {what} '{text}'.");
}
