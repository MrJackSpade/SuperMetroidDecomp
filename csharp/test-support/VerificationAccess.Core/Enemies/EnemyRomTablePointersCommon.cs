using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies;

/// <summary>Movement and collision tables shared across enemy banks.</summary>
internal static class EnemyRomTablePointersCommon
{
    /// <summary>256 signed 16-bit sine/cosine samples at $A0:B443 (512 bytes).</summary>
    /// <remarks>
    /// Physical view of <see cref="EnemyTrigonometryTables.SignedSine"/>
    /// within the 320-word prefix/full-cycle region. Proof: #625 / #910.
    /// </remarks>
    public const int SignedSineCosineWords = 0xa0b443;
    /// <summary>Thirty-two 16-byte slope-height profiles at $94:8B2B (512 bytes).</summary>
    /// <remarks>Physical alias of <see cref="SlopeHeightDefinitions.Read"/>. Proof: #625 / #914.</remarks>
    public const int SlopeHeightBytes = 0x948b2b;
}
