namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$AA Golden Torizo behavioral-property bits in enemy parameter 2.</summary>
internal static class GoldenTorizoBehavioralProperties
{
    /// <summary>
    /// Bit $2000 marks Golden Torizo stunned by a Super Missile. The callable
    /// $AA:D193 stun list clears it with instruction $AA:D1E7.
    /// </summary>
    internal const ushort Stunned = 0x2000;
}
