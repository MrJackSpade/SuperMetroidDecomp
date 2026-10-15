namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's random jump selection.</summary>
internal static class KagoDefinitions
{
    /// <summary>The random bits that add 0-768 to a Kago bug's upward jump magnitude.</summary>
    internal const ushort JumpHeightRandomBits = 0x0300;

    /// <summary>Bit 8 of the jump magnitude: the bug jumps left.</summary>
    internal const ushort LeftJumpBit = 0x0100;
}
