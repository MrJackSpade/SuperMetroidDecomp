namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// One of the seventeen updates of the gunship's landing brakes after Ceres (#2343), in the order the ship takes them: each
/// moves the ship and Samus by its offset, and the last opens the entrance pad.
/// </summary>
public readonly record struct GunshipLandingBrake
{
    /// <summary>The brake at <paramref name="place"/> of the brakes' authored schedule.</summary>
    private GunshipLandingBrake(int place) => Place = place;

    /// <summary>The brake's place in the brakes' authored schedule (<see cref="GunshipMotionDefinitions.LandingBrakeYDeltas"/>).</summary>
    private int Place { get; }

    /// <summary>The schedule's first brake.</summary>
    public static GunshipLandingBrake First => new(0);

    /// <summary>How far this brake moves the ship and Samus, in whole pixels, downward positive.</summary>
    public int YDelta => GunshipMotionDefinitions.LandingBrakeYDeltas[Place];

    /// <summary>The brake after this one; none after the last.</summary>
    public GunshipLandingBrake? Next => Place + 1 < GunshipMotionDefinitions.LandingBrakeYDeltas.Length ? new GunshipLandingBrake(Place + 1) : null;
}
