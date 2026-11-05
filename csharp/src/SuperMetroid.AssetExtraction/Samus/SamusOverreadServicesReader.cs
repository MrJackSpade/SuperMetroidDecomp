using SuperMetroid.Core.Spatial;
using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Validates an installed overread document and builds the Samus overread services from it (#1474).</summary>
internal static class SamusOverreadServicesReader
{
    /// <summary>Builds every service, requiring an entry for every situation each one covers.</summary>
    /// <exception cref="InvalidDataException">The document is incomplete or names something the game does not have.</exception>
    internal static SamusOverreadServices Read(SamusOverreadDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != SamusOverreadFormat.Version)
            throw new InvalidDataException("Samus overread file has an unsupported version.");
        var projectiles = new SamusProjectileOverreads(
            TrailStarts(Require(document.TrailFramesAtListStart, nameof(document.TrailFramesAtListStart)),
                Require(document.TrailPlacementsPastRecords, nameof(document.TrailPlacementsPastRecords))),
            SpaceTimeOverwrite(Require(document.SpaceTimeOverwrites, nameof(document.SpaceTimeOverwrites))),
            ChainsawFuseShift(Require(document.ChainsawFuseLandings, nameof(document.ChainsawFuseLandings))),
            ChargeFlareDelays(Require(document.ChargeFlareDelaysBelowPrograms, nameof(document.ChargeFlareDelaysBelowPrograms))),
            PostureAdmission(Require(document.PostureAdmissions, nameof(document.PostureAdmissions))),
            UnauthoredFiring(Require(document.UnauthoredBeamLaunchSpeeds, nameof(document.UnauthoredBeamLaunchSpeeds)),
                Require(document.UnauthoredBeamCooldowns, nameof(document.UnauthoredBeamCooldowns))),
            DrainedCrashDepartures(Require(document.DrainedCrashDepartures, nameof(document.DrainedCrashDepartures))));
        return new SamusOverreadServices(
            AnimationContinuation(Require(document.AnimationContinuations, nameof(document.AnimationContinuations))),
            projectiles);
    }

    /// <summary>
    /// Builds the colours Samus's palette loaders read past their tables, which only the import reads to pre-colour her
    /// sheets (#1512).
    /// </summary>
    /// <exception cref="InvalidDataException">The document has an unsupported version or an incomplete colour run.</exception>
    internal static SamusOverrunColours ReadColours(SamusOverreadDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != SamusOverreadFormat.Version)
            throw new InvalidDataException("Samus overread file has an unsupported version.");
        return new SamusOverrunColours(
            SpeedBoosterColours(Require(document.SpeedBoosterOverrunColours, nameof(document.SpeedBoosterOverrunColours))),
            ChainsawColours(Require(document.ChainsawBeamColours, nameof(document.ChainsawBeamColours))));
    }

    /// <summary>What an animation frame past a pose's program reads, which only the import reads to compose Samus's body sheet (#1513).</summary>
    /// <exception cref="InvalidDataException">The document has an unsupported version or an incomplete continuation.</exception>
    internal static LaidOutProgramContinuation<SamusPoseId> ReadAnimationContinuation(SamusOverreadDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != SamusOverreadFormat.Version)
            throw new InvalidDataException("Samus overread file has an unsupported version.");
        return AnimationContinuation(Require(document.AnimationContinuations, nameof(document.AnimationContinuations)));
    }

    /// <summary>Every pose with an animation program and its program's successor.</summary>
    private static LaidOutProgramContinuation<SamusPoseId> AnimationContinuation(
        IReadOnlyDictionary<string, SamusOverreadDocument.Continuation?> continuations)
    {
        SamusPoseId[] poses = [.. Enum.GetValues<SamusPoseId>().Where(SamusPoseNumbering.HasRecord)];
        if (continuations.Count != poses.Length)
            throw new InvalidDataException($"Samus overread file holds {continuations.Count} animation continuations, not {poses.Length}.");
        var successors = new Dictionary<SamusPoseId, LaidOutProgramContinuation<SamusPoseId>.Successor?>();
        foreach (SamusPoseId pose in poses)
        {
            if (!continuations.TryGetValue(pose.ToString(), out SamusOverreadDocument.Continuation? continuation))
                throw new InvalidDataException($"Samus overread file has no animation continuation for {pose}.");
            successors.Add(pose, continuation is null
                ? null
                : new LaidOutProgramContinuation<SamusPoseId>.Successor(Parse<SamusPoseId>(continuation.Pose, "animation successor"), continuation.Shift));
        }
        return new LaidOutProgramContinuation<SamusPoseId>(successors);
    }

    /// <summary>Every program's fresh-list trail frame and every list's placements past its run.</summary>
    private static TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> TrailStarts(
        IReadOnlyDictionary<string, int> framesAtListStart,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Placement?>> pastRecords)
    {
        var frames = new Dictionary<SamusProjectileProgram, int>();
        foreach (SamusProjectileProgram program in Enum.GetValues<SamusProjectileProgram>())
        {
            frames.Add(program, framesAtListStart.TryGetValue(program.ToString(), out int frame) && frame is >= 0 and <= ushort.MaxValue
                ? frame
                : throw new InvalidDataException($"Samus overread file has no valid fresh-list trail frame for {program}."));
        }
        if (framesAtListStart.Count != frames.Count)
            throw new InvalidDataException($"Samus overread file holds {framesAtListStart.Count} fresh-list trail frames, not {frames.Count}.");
        var placements = new Dictionary<ProjectileTrailPlacementList, IReadOnlyDictionary<int, ProjectileTrailPlacement?>>();
        foreach (ProjectileTrailPlacementList list in Enum.GetValues<ProjectileTrailPlacementList>())
        {
            if (!pastRecords.TryGetValue(list.ToString(), out IReadOnlyDictionary<string, SamusOverreadDocument.Placement?>? byFrame))
                throw new InvalidDataException($"Samus overread file has no trail placements past {list}'s records.");
            placements.Add(list, byFrame.ToDictionary(
                pair => ParseCount(pair.Key, $"{list} trail frame"),
                pair => pair.Value is { } placement
                    ? new ProjectileTrailPlacement(Sparkle(placement.Left), Sparkle(placement.Right))
                    : (ProjectileTrailPlacement?)null));
        }
        if (pastRecords.Count != placements.Count)
            throw new InvalidDataException($"Samus overread file holds trail placements for {pastRecords.Count} lists, not {placements.Count}.");
        return new(frames, placements);
    }

    /// <summary>Every SpaceTime animation position's overwrite, by slot.</summary>
    private static SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> SpaceTimeOverwrite(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>> overwrites)
    {
        var result = new Dictionary<AnimationProgramCursor<SamusProjectileProgram>, IReadOnlyList<ProgressionOverwrite?>>();
        foreach ((string programName, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>> positions) in overwrites)
        {
            SamusProjectileProgram program = Parse<SamusProjectileProgram>(programName, "SpaceTime program");
            int steps = SamusProjectileAnimations.Programs.Steps(program).Count;
            if (positions.Count != steps)
                throw new InvalidDataException($"Samus overread file holds SpaceTime overwrites for {positions.Count} of {program}'s {steps} steps.");
            foreach ((string positionName, IReadOnlyList<SamusOverreadDocument.Overwrite?> bySlot) in positions)
            {
                int position = ParseCount(positionName, $"{program} step");
                if (position >= steps)
                    throw new InvalidDataException($"Samus overread file names step {position} of {program}, which has {steps}.");
                if (bySlot.Count != SamusProjectileSystem.SlotCount)
                    throw new InvalidDataException($"Samus overread file holds SpaceTime overwrites for {bySlot.Count} slots, not {SamusProjectileSystem.SlotCount}.");
                result.Add(new(program, position), [.. bySlot.Select(Overwrite)]);
            }
        }
        if (result.Count == 0)
            throw new InvalidDataException("Samus overread file holds no SpaceTime overwrites.");
        return new(result);
    }

    /// <summary>One decoded overwrite in the progression's own terms.</summary>
    private static ProgressionOverwrite? Overwrite(SamusOverreadDocument.Overwrite? overwrite) => overwrite is null
        ? null
        : new ProgressionOverwrite(
            Require(overwrite.Events, nameof(overwrite.Events)),
            Require(overwrite.AreaBosses, nameof(overwrite.AreaBosses)).ToDictionary(
                area => Parse<AreaId>(area.Key, "SpaceTime boss area"),
                area => area.Value.Aggregate(BossBits.None, (bosses, name) => bosses | Parse<BossBits>(name, "SpaceTime boss"))),
            Require(overwrite.ChozoOrbs, nameof(overwrite.ChozoOrbs)),
            Require(overwrite.CollectedItems, nameof(overwrite.CollectedItems)),
            Require(overwrite.OpenedDoors, nameof(overwrite.OpenedDoors)),
            Require(overwrite.UsedStations, nameof(overwrite.UsedStations)).ToDictionary(
                area => Parse<AreaId>(area.Key, "SpaceTime station area"), area => area.Value),
            Require(overwrite.AreaMaps, nameof(overwrite.AreaMaps)).ToDictionary(
                area => Parse<AreaId>(area.Key, "SpaceTime map area"), area => area.Value),
            overwrite.SavedLoadingGameState);

    /// <summary>Every animation position the Chainsaw fuse lands somewhere.</summary>
    private static ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> ChainsawFuseShift(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> landings)
    {
        var result = new Dictionary<AnimationProgramCursor<SamusProjectileProgram>, AnimationProgramCursor<SamusProjectileProgram>>();
        foreach ((string programName, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor> positions) in landings)
        {
            SamusProjectileProgram program = Parse<SamusProjectileProgram>(programName, "Chainsaw program");
            foreach ((string positionName, SamusOverreadDocument.Cursor landing) in positions)
                result.Add(new(program, ParseCount(positionName, $"{program} step")), Cursor(landing));
        }
        if (landings.Count != Enum.GetValues<SamusProjectileProgram>().Length)
            throw new InvalidDataException($"Samus overread file holds Chainsaw landings for {landings.Count} programs.");
        return new(result);
    }

    /// <summary>A decoded animation position, which must name a step of its program.</summary>
    private static AnimationProgramCursor<SamusProjectileProgram> Cursor(SamusOverreadDocument.Cursor cursor)
    {
        Ensure.NotNull(cursor);
        SamusProjectileProgram program = Parse<SamusProjectileProgram>(cursor.Program, "animation position program");
        return (uint)cursor.Position < (uint)SamusProjectileAnimations.Programs.Steps(program).Count
            ? new(program, cursor.Position)
            : throw new InvalidDataException($"Samus overread file names step {cursor.Position} of {program}, which it does not have.");
    }

    /// <summary>The charge flare's delays below its programs, one per byte of the table naming them.</summary>
    private static ChargeFlareDelaysBelowPrograms ChargeFlareDelays(IReadOnlyList<int> delays) =>
        delays.Count == ChargeFlareAnimationDefinitions.ProgramTableByteCount && delays.All(delay => delay is >= 0 and <= byte.MaxValue)
            ? new(delays)
            : throw new InvalidDataException($"Samus overread file holds {delays.Count} charge-flare delays below the programs, not {ChargeFlareAnimationDefinitions.ProgramTableByteCount} bytes.");

    /// <summary>Whether each pose outside the transition table admits weapons.</summary>
    private static PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission(IReadOnlyDictionary<string, bool> admissions)
    {
        if (admissions.Count == 0)
            throw new InvalidDataException("Samus overread file holds no posture admissions.");
        return new(admissions.ToDictionary(pair => Parse<SamusPoseId>(pair.Key, "posture-admission pose"), pair => pair.Value));
    }

    /// <summary>The colours of each Speed Booster step past Gravity's cycle, one suit row each.</summary>
    private static SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBoosterColours(
        IReadOnlyDictionary<string, IReadOnlyList<PaletteRgb5>> colours)
    {
        var result = new Dictionary<SpeedBoosterPaletteOverrun, IReadOnlyList<RgbColor>>();
        foreach (SpeedBoosterPaletteOverrun step in Enum.GetValues<SpeedBoosterPaletteOverrun>())
        {
            if (!colours.TryGetValue(step.ToString(), out IReadOnlyList<PaletteRgb5>? row) ||
                row.Count != SamusPaletteDefinitions.Common.ColoursPerSet)
                throw new InvalidDataException($"Samus overread file holds no suit row of colours for Speed Booster {step}.");
            result.Add(step, [.. row.Select(colour => colour.ToRgbColor())]);
        }
        if (colours.Count != result.Count)
            throw new InvalidDataException($"Samus overread file holds colours for {colours.Count} Speed Booster steps, not {result.Count}.");
        return new(result);
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

    /// <summary>The Chainsaw beam's sixteen colours.</summary>
    private static ChainsawBeamColours<RgbColor> ChainsawColours(IReadOnlyList<PaletteRgb5> colours) =>
        colours.Count == ChainsawBeamColours<RgbColor>.ColourCount
            ? new([.. colours.Select(colour => colour.ToRgbColor())])
            : throw new InvalidDataException($"Samus overread file holds {colours.Count} Chainsaw beam colours, not {ChainsawBeamColours<RgbColor>.ColourCount}.");

    /// <summary>A sparkle's signed offset from its projectile, -128 through 127 pixels.</summary>
    private static Offset16<RoomPixels> Sparkle(Offset16<RoomPixels> offset) => new(SparkleOffset(offset.X), SparkleOffset(offset.Y));

    /// <summary>One sparkle offset component, which a trail record holds as a signed byte.</summary>
    private static short SparkleOffset(int pixels) => pixels is >= sbyte.MinValue and <= sbyte.MaxValue
        ? (short)pixels
        : throw new InvalidDataException($"Samus overread trail placement offset {pixels} lies outside -128 through 127.");

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
