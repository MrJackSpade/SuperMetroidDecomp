namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Where the Torizos' projectiles are placed and moved, in room pixels (#1945). A placement relative to Torizo is the
/// right-facing one; a left-facing Torizo places it mirrored.
/// </summary>
internal static class TorizoProjectilePlacementDefinitions
{
    /// <summary>A caught Super Missile as Golden Torizo's hand takes it.</summary>
    internal static readonly Offset16<RoomPixels> CaughtSuperMissile = new(30, -52);

    /// <summary>A held Super Missile, kept beside Golden Torizo's hand each update until it is thrown.</summary>
    internal static readonly Offset16<RoomPixels> HeldSuperMissile = new(32, -52);

    /// <summary>The dust a landing Torizo's right foot raises; the left foot's lies mirrored.</summary>
    internal static readonly Offset16<RoomPixels> RightFootDust = new(24, 48);

    /// <summary>A sonic boom thrown high, ahead of Torizo.</summary>
    internal static readonly Offset16<RoomPixels> HighSonicBoom = new(32, -12);

    /// <summary>A sonic boom thrown low, ahead of Torizo.</summary>
    internal static readonly Offset16<RoomPixels> LowSonicBoom = new(32, 20);

    /// <summary>A low-health drool drop, ahead of Torizo's mouth.</summary>
    internal static readonly Offset16<RoomPixels> LowHealthDrool = new(8, -5);

    /// <summary>Golden Torizo's eye beam as it leaves its eyes.</summary>
    internal static readonly Offset16<RoomPixels> EyeBeamMuzzle = new(20, -30);

    /// <summary>How far landing dust rises at each of its rising cues.</summary>
    internal static readonly Offset16<RoomPixels> DustRise = new(0, -4);

    /// <summary>How far a drool drop is lifted as it splashes on the floor.</summary>
    internal static readonly Offset16<RoomPixels> DroolSplashLift = new(0, -3);

    /// <summary>How far below the top of the block it struck a Chozo orb or the eye beam bursts on the floor.</summary>
    internal const int FloorBurstDepth = 6;
}
