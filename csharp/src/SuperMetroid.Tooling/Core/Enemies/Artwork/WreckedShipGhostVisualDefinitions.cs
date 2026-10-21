using SuperMetroid.Core.Enemies.WreckedShip;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>WreckedShipGhost's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class WreckedShipGhostVisualDefinitions
{
    /// <summary>WreckedShipGhost instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>Three Coven floating compositions at $A8:9E46..9E72.</summary>
    internal const int FrameCount = 3;

    /// <summary>The compositions the ghost's typed floating loop (#641) shows, in order.</summary>
    internal static ushort[] Spritemaps() => [.. WreckedShipGhostAnimationsTooling.Spritemaps(WreckedShipGhostProgram.Floating)];
}
