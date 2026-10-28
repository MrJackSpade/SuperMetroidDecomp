using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile collision geometry read from bank $94.</summary>
internal static class SamusProjectileRomDataCollision
{
    /// <summary>Sixteen height bytes for each non-square slope shape.</summary>
    /// <remarks>Physical alias of <see cref="SlopeHeights.Read"/>. Proof: #625 / #914.</remarks>
    public const int NonSquareSlopeDefinitions = 0x948b2b;
    /// <summary>Four-quadrant solidity bytes for square slope shapes.</summary>
    /// <remarks>Physical bank-$94 alias of <see cref="SquareSlopeDefinitions.ReadSamusQuadrant"/>. Proof: #625 / #915.</remarks>
    public const int SquareSlopeDefinitions = 0x948e54;
}
