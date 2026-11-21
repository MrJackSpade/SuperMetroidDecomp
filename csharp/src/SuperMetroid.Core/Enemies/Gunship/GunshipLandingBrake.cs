using SuperMetroid.Core.Bosses;
namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// One of the seventeen updates of the gunship's landing brakes after Ceres (#2343), in the order the ship takes them: each
/// moves the ship and Samus by its offset, and the last opens the entrance pad.
/// </summary>
public readonly record struct GunshipLandingBrake
{
    /// <summary>The brake <paramref name="step"/> holds, with its place in the schedule.</summary>
    private GunshipLandingBrake(ListStep<int> step) => Step = step;

    /// <summary>The brake's offset, with its place in the schedule.</summary>
    private ListStep<int> Step { get; }

    /// <summary>The schedule's first brake.</summary>
    public static GunshipLandingBrake First => new(ListStep<int>.FirstOf(GunshipMotionDefinitions.LandingBrakeYDeltas));

    /// <summary>How far this brake moves the ship and Samus, in whole pixels, downward positive.</summary>
    public int YDelta => Step.Entry;

    /// <summary>The brake after this one; none after the last.</summary>
    public GunshipLandingBrake? Next => Step.Following is { } following ? new GunshipLandingBrake(following) : null;
}
