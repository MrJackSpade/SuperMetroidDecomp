namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A room object one of the two Chozo statues spawns, named for what it does to the statue's room (#1622).</summary>
public enum ChozoStatueRoomObject
{
    /// <summary>
    /// The Lower Norfair statue's hand: it lowers the acid at once when the statue has already done so, and otherwise
    /// waits for the Power Bomb block at its trigger to break open and makes that block the hand.
    /// </summary>
    LowerNorfairHand,

    /// <summary>The Wrecked Ship statue's hand: its spawn makes its block the hand, and it leaves on its first update.</summary>
    WreckedShipHand,

    /// <summary>The Wrecked Ship statue's slope access opening: it draws the opened access, then the spikes become slopes.</summary>
    SlopeAccessOpening,

    /// <summary>The Wrecked Ship statue's slope access closing: it draws the closed access, then the slopes become spikes.</summary>
    SlopeAccessClosing,

    /// <summary>
    /// The Lower Norfair room's plug crumbling as the hand wakes the statue; its spawn leaves the block the room's load
    /// last drew (<see cref="ChozoStatueRoomDefinitions.HandCrumbleSetupBlock"/>).
    /// </summary>
    PlugCrumbleAtHand,

    /// <summary>
    /// A plug crumbling under the walking statue's footstep; its spawn leaves block zero
    /// (<see cref="ChozoStatueRoomDefinitions.FootstepCrumbleSetupBlock"/>).
    /// </summary>
    PlugCrumbleUnderFootstep,
}
