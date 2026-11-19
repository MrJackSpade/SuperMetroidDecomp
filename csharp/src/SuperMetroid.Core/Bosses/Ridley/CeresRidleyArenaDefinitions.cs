namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where Ceres Ridley's fight plays in his chamber (#2232): where he stands and may fly, the lines his lift-off, hover,
/// fireballs, lunge and swoop turn on, the points he flies to, and where the Baby he carries falls and is caught.
/// </summary>
internal static class CeresRidleyArenaDefinitions
{
    /// <summary>Where Ceres Ridley stands as he is placed, on his chamber's floor.</summary>
    internal static readonly Position16<RoomPixels> Spawn = new(186, 169);

    /// <summary>
    /// The top-left corner of the box his movement keeps him in, where it lies from the room's origin: the box reaches
    /// above the chamber, to a negative height.
    /// </summary>
    internal static readonly Offset16<RoomPixels> MovementMinimum = new(40, -32);

    /// <summary>The bottom-right corner of the box his movement keeps him in, where it lies from the room's origin.</summary>
    internal static readonly Offset16<RoomPixels> MovementMaximum = new(224, 176);

    /// <summary>The top his movement box rises to as he retreats, so he can leave the chamber upward.</summary>
    internal const int RetreatMovementTop = -192;

    /// <summary>The height above which his lift-off's climb starts slowing.</summary>
    internal const int LiftoffSlowingLineY = 112;

    /// <summary>
    /// The height above which his slowing lift-off ends in his hover, and above which his climb toward the fireball
    /// position counts toward giving the fireballs up for a swoop.
    /// </summary>
    internal const int HoverLineY = 80;

    /// <summary>The point his hover holds and returns to between attacks.</summary>
    internal static readonly Position16<RoomPixels> HoverPoint = new(192, 100);

    /// <summary>How near the hover point he must be, on each axis, to choose his next attack at once.</summary>
    internal static readonly Extent16<RoomPixels> HoverReach = new(8, 8);

    /// <summary>The height his move into his fireball position climbs toward.</summary>
    internal const int FireballMoveTargetY = 88;

    /// <summary>The height above which his move into his fireball position starts the fireballing.</summary>
    internal const int FireballLineY = 128;

    /// <summary>The height above which Samus stands high enough that his lunge aims at a fixed height instead of above her.</summary>
    internal const int LungeSamusHighLineY = 132;

    /// <summary>The height his lunge aims at while Samus stands high.</summary>
    internal const int LungeHighTargetY = 64;

    /// <summary>How far above a low Samus his lunge aims.</summary>
    internal static readonly Offset16<RoomPixels> LungeAboveSamus = new(0, 68);

    /// <summary>How near his lunge's target he must come, on each axis, to end the lunge.</summary>
    internal static readonly Extent16<RoomPixels> LungeReach = new(2, 2);

    /// <summary>The point he flies to before his swoop dives.</summary>
    internal static readonly Position16<RoomPixels> SwoopStart = new(192, 80);

    /// <summary>The height above which his flight to the swoop's start begins the dive.</summary>
    internal const int SwoopStartLineY = 96;

    /// <summary>
    /// The point his fake retreat flies toward after his hundredth hit; his fake retreat starts rising once he is at or
    /// right of its X.
    /// </summary>
    internal static readonly Position16<RoomPixels> FakeRetreatPoint = new(192, 128);

    /// <summary>
    /// The point above the chamber both his retreats fly toward, where it lies from the room's origin, at a negative
    /// height; his real retreat ends once he is above it.
    /// </summary>
    internal static readonly Offset16<RoomPixels> RetreatPoint = new(192, -128);

    /// <summary>The height above which his rising fake retreat lets the Baby fall.</summary>
    internal const int BabyDropLineY = 32;

    /// <summary>Where his hand flies, relative to the falling Baby, as he retrieves it.</summary>
    internal static readonly Offset16<RoomPixels> RetrievalHandAboveBaby = new(10, 56);

    /// <summary>How near the Baby his feet must come, on each axis, exclusive, to catch it.</summary>
    internal const int BabyGraspReach = 8;

    /// <summary>The floor the dropped Baby falls to and rests on.</summary>
    internal const int BabyFloorY = 192;

    /// <summary>The gravity the dropped Baby's fall gains each update.</summary>
    internal static readonly SignedEightEight BabyFallGravity = SignedEightEight.FromPixelFraction(1, 32);

    /// <summary>The Baby's anchor in Ridley's arms before the fake retreat.</summary>
    internal static readonly Offset16<RoomPixels> BabyArmsAnchor = new(-16, 22);

    /// <summary>The Baby's anchor in Ridley's feet once retrieved, also the grasp point the retrieval compares.</summary>
    internal static readonly Offset16<RoomPixels> BabyFeetAnchor = new(14, 66);
}
