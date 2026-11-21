namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One Magdollite (#1542, #2554): the room lists it as three enemies in a row, its head, its rising body, then its tracking
/// overlay. Its head finds the other two as it is placed, first, and begins the states they hold, which each receives
/// as it is built after the head.
/// </summary>
/// <param name="Composite">The three places of the Magdollite.</param>
/// <param name="Head">The Magdollite's head (#2558).</param>
/// <param name="HeadRise">How the head and its arm rise, as the head's spawn settings name (#2474).</param>
/// <param name="HeadState">The head's state.</param>
/// <param name="BodyState">The body's state, which the head begins and the body sets up as it is built.</param>
/// <param name="OverlayState">The overlay's state, which the head begins and the overlay sets up as it is built.</param>
internal sealed record MagdollitePartners(MagdolliteComposite Composite, RoomEnemyOccupant Head, MagdolliteHeadSpawnSettings HeadRise,
    MagdolliteHeadState HeadState, MagdolliteBodyState BodyState, MagdolliteOverlayState OverlayState);
