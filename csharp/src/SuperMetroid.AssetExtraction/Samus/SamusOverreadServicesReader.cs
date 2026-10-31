using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
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
        return new SamusOverreadServices(
            AnimationContinuation(Require(document.AnimationContinuations, nameof(document.AnimationContinuations))),
            TrailStarts(Require(document.TrailFramesAtListStart, nameof(document.TrailFramesAtListStart)),
                Require(document.TrailPlacementsPastRecords, nameof(document.TrailPlacementsPastRecords))),
            SpaceTimeOverwrite(Require(document.SpaceTimeOverwrites, nameof(document.SpaceTimeOverwrites))),
            ChainsawFuseShift(Require(document.ChainsawFuseLandings, nameof(document.ChainsawFuseLandings))),
            ChargeFlareDelays(Require(document.ChargeFlareDelaysBelowPrograms, nameof(document.ChargeFlareDelaysBelowPrograms))),
            PostureAdmission(Require(document.PostureAdmissions, nameof(document.PostureAdmissions))),
            SpeedBoosterColours(Require(document.SpeedBoosterOverrunColours, nameof(document.SpeedBoosterOverrunColours))));
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
                    ? new ProjectileTrailPlacement(new(SparkleOffset(placement.LeftX), SparkleOffset(placement.LeftY)),
                        new(SparkleOffset(placement.RightX), SparkleOffset(placement.RightY)))
                    : (ProjectileTrailPlacement?)null));
        }
        if (pastRecords.Count != placements.Count)
            throw new InvalidDataException($"Samus overread file holds trail placements for {pastRecords.Count} lists, not {placements.Count}.");
        return new(frames, placements);
    }

    /// <summary>Every SpaceTime animation position's overwrite, by slot.</summary>
    private static SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>> SpaceTimeOverwrite(
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
        : new ProgressionOverwrite(overwrite.Events, overwrite.AreaBosses, overwrite.ChozoOrbs, overwrite.CollectedItems,
            overwrite.OpenedDoors, overwrite.UsedStations, overwrite.MapStations, overwrite.SavedLoadingGameState);

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
                row.Count != SamusPaletteDefinitions.Common.ColoursPerRow)
                throw new InvalidDataException($"Samus overread file holds no suit row of colours for Speed Booster {step}.");
            result.Add(step, [.. row.Select(colour => colour.ToRgbColor())]);
        }
        if (colours.Count != result.Count)
            throw new InvalidDataException($"Samus overread file holds colours for {colours.Count} Speed Booster steps, not {result.Count}.");
        return new(result);
    }

    /// <summary>A sparkle's signed offset from its projectile, -128 through 127 pixels.</summary>
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
