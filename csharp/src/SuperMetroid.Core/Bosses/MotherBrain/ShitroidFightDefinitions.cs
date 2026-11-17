namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where the Tourian Shitroid's scene plays and how long its stages last (#2254): its arena and invisible walls, its
/// entrance, where it latches and flies, how it locks onto Samus, how it drains her, how a shot pushes it back, and the
/// pace of its membrane's pulse and cries.
/// </summary>
internal static class ShitroidFightDefinitions
{
    /// <summary>The left edge of the Shitroid's arena: the camera is put here as its scene starts, and Samus left of it ends its pursuit.</summary>
    internal const int ArenaLeftX = 512;

    /// <summary>The invisible wall columns its scene raises and clears, in the order they are requested.</summary>
    internal static IReadOnlyList<Position16<RoomBlocks>> WallBlocks { get; } = [new(48, 3), new(31, 3)];

    /// <summary>The screens of the room's row of four its scene closes and opens again: the second and fourth.</summary>
    internal static IReadOnlyList<Position16<RoomScreens>> ClosingScreens { get; } = [new(1, 0), new(3, 0)];

    /// <summary>The updates of its entrance delay, once the camera is held, before it flies in.</summary>
    internal const int EntranceDelayUpdates = 465;

    /// <summary>The point it rushes to first, above its Sidehopper.</summary>
    internal static readonly Position16<RoomPixels> SidehopperRendezvous = new(584, 74);

    /// <summary>How near the rendezvous, on each axis, it must come before it chases the Sidehopper.</summary>
    internal static readonly Extent16<RoomPixels> RendezvousReach = new(1, 1);

    /// <summary>Where it latches onto a victim: above the Sidehopper's or Samus's position.</summary>
    internal static readonly Offset16<RoomPixels> LatchOffset = new(0, -32);

    /// <summary>The updates it drains its Sidehopper; on the last the Sidehopper's corpse starts.</summary>
    internal const int SidehopperDrainUpdates = 320;

    /// <summary>The updates it rises after feeding, and after draining Samus.</summary>
    internal const int RiseUpdates = 193;

    /// <summary>The height it rises to after feeding and after draining Samus.</summary>
    internal const int RiseY = 104;

    /// <summary>The updates it pauses after draining Samus before rising.</summary>
    internal const int PostDrainPauseUpdates = 121;

    /// <summary>The updates of each of its flights beside Samus, to her left and then to her right.</summary>
    internal const int FlyBesideUpdates = 89;

    /// <summary>How far left of Samus it flies to her left.</summary>
    internal const int BesideLeftX = -64;

    /// <summary>The height of its flight to her left.</summary>
    internal const int BesideLeftY = 100;

    /// <summary>How far right of Samus it flies to her right.</summary>
    internal const int BesideRightX = 96;

    /// <summary>The height of its flight to her right.</summary>
    internal const int BesideRightY = 104;

    /// <summary>The updates it holds the drained Samus, remorseful, before releasing her.</summary>
    internal const int HoldBeforeReleaseUpdates = 257;

    /// <summary>The point left of the room it leaves to.</summary>
    internal static readonly Position16<RoomPixels> ExitPoint = Position16<RoomPixels>.Wrapped(-128, 64);

    /// <summary>How near its exit point, on each axis, it must come before it is gone.</summary>
    internal static readonly Extent16<RoomPixels> ExitReach = new(8, 8);

    /// <summary>How near Samus, horizontally, its hover must stay for its lock on her to build.</summary>
    internal const int HoverLockReach = 8;

    /// <summary>The lock its hover builds before it chases Samus.</summary>
    internal const int HoverLockToChase = 256;

    /// <summary>The updates it hovers at Samus's height once its hover restarts.</summary>
    internal const int HoverUpdates = 32;

    /// <summary>The height it hovers at between those hovers.</summary>
    internal const int HoverY = 80;

    /// <summary>How near the released Samus, horizontally, it must stay for its lock on her to build.</summary>
    internal const int FollowLockReach = 2;

    /// <summary>The lock its follow of the released Samus builds before it leaves.</summary>
    internal const int FollowLockToLeave = 1024;

    /// <summary>How far above the released Samus it follows her.</summary>
    internal static readonly Offset16<RoomPixels> FollowOffset = new(0, -18);

    /// <summary>The X left of which the released Samus makes it leave.</summary>
    internal const int FollowLeaveSamusX = 128;

    /// <summary>The energy below which it stops draining Samus and lets her fall.</summary>
    internal const int DrainStopHealth = 2;

    /// <summary>The shot cooldown its drain holds Samus at.</summary>
    internal const int DrainShotCooldown = 8;


    /// <summary>The divisor its drain holds Samus's horizontal speed to.</summary>
    internal const int DrainXSpeedDivisor = 2;

    /// <summary>The fall speed, in whole pixels an update, from which its drain slows Samus's fall.</summary>
    internal const int DrainFallSpeedLimit = 4;

    /// <summary>The fall speed, in whole pixels an update, its drain slows Samus's fall to.</summary>
    internal const int DrainFallSpeed = 2;

    /// <summary>How far above Samus it shakes as it drains her.</summary>
    internal const int DrainAboveSamus = -20;

    /// <summary>The energy its drain takes each update from Samus wearing the Varia Suit.</summary>
    internal const int DrainWithVaria = 2;

    /// <summary>The energy its drain takes each update from Samus without the Varia Suit.</summary>
    internal const int DrainWithoutVaria = 4;

    /// <summary>A shot's recoil on it in 256ths of a pixel an update, per point of the shot's damage.</summary>
    internal const int RecoilPerDamage = 8;

    /// <summary>The strongest recoil a shot gives it, in 256ths of a pixel an update.</summary>
    internal const int RecoilLimit = 240;

    /// <summary>The updates between its membrane's pulses while it is calm, and the delay below which its cry is quick.</summary>
    internal const int CalmPulseDelay = 10;

    /// <summary>The updates between its membrane's pulses while it is latched on and feeding.</summary>
    internal const int FeedingPulseDelay = 1;

    /// <summary>The cry pulses that pass between its cries.</summary>
    internal const int PulsesPerCry = 4;
}
