namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// One of the seventeen updates of the gunship's landing brakes after Ceres (#2343), in the order the ship takes them: each
/// moves the ship and Samus by its offset, and the last opens the entrance pad.
/// </summary>
public readonly record struct GunshipLandingBrake
{
    /// <summary>The brake at <paramref name="step"/> of the schedule.</summary>
    private GunshipLandingBrake(int step) => Step = step;

    /// <summary>The brake's place in the schedule, from zero.</summary>
    private int Step { get; }

    /// <summary>Every brake of the schedule, in the order the ship takes them.</summary>
    public static IReadOnlyList<GunshipLandingBrake> All { get; } =
        [.. Enumerable.Range(0, GunshipMotionDefinitions.LandingBrakeYDeltas.Length).Select(step => new GunshipLandingBrake(step))];

    /// <summary>The schedule's first brake.</summary>
    public static GunshipLandingBrake First => All[0];

    /// <summary>How far this brake moves the ship and Samus, in whole pixels, downward positive.</summary>
    public int YDelta => GunshipMotionDefinitions.LandingBrakeYDeltas[Step];

    /// <summary>The brake after this one; none after the last.</summary>
    public GunshipLandingBrake? Next => Step + 1 < All.Count ? All[Step + 1] : null;
}
