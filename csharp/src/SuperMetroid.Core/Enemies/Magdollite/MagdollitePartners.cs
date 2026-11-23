namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One Magdollite (#1542, #2554): the room lists it as three enemies in a row, its head, its rising body, then its tracking
/// overlay. Its head finds the other two as it is placed, first, and begins the states they hold, which each receives
/// as it is built after the head.
/// </summary>
internal sealed partial class MagdollitePartners
{
    /// <summary>
    /// Holds the Magdollite's places and the states its head began. An ordinary constructor, not a primary one, so a restore
    /// of the game's state makes the partners before it reads the places, whose body and overlay hold them too (#2554).
    /// </summary>
    /// <param name="composite">The three places of the Magdollite.</param>
    /// <param name="head">The Magdollite's head (#2558).</param>
    /// <param name="headRise">How the head and its arm rise, as the head's spawn settings name (#2474).</param>
    /// <param name="headState">The head's state.</param>
    /// <param name="bodyState">The body's state, which the head begins and the body sets up as it is built.</param>
    /// <param name="overlayState">The overlay's state, which the head begins and the overlay sets up as it is built.</param>
    internal MagdollitePartners(MagdolliteComposite composite, RoomEnemyOccupant head, MagdolliteHeadSpawnSettings headRise,
        MagdolliteHeadState headState, MagdolliteBodyState bodyState, MagdolliteOverlayState overlayState)
    {
        Composite = composite;
        Head = Ensure.NotNull(head);
        HeadRise = Ensure.NotNull(headRise);
        HeadState = Ensure.NotNull(headState);
        BodyState = Ensure.NotNull(bodyState);
        OverlayState = Ensure.NotNull(overlayState);
    }

    /// <summary>The three places of the Magdollite.</summary>
    internal MagdolliteComposite Composite { get; }

    /// <summary>The Magdollite's head (#2558).</summary>
    internal RoomEnemyOccupant Head { get; }

    /// <summary>How the head and its arm rise, as the head's spawn settings name (#2474).</summary>
    internal MagdolliteHeadSpawnSettings HeadRise { get; }

    /// <summary>The head's state.</summary>
    internal MagdolliteHeadState HeadState { get; }

    /// <summary>The body's state, which the head begins and the body sets up as it is built.</summary>
    internal MagdolliteBodyState BodyState { get; }

    /// <summary>The overlay's state, which the head begins and the overlay sets up as it is built.</summary>
    internal MagdolliteOverlayState OverlayState { get; }
}
