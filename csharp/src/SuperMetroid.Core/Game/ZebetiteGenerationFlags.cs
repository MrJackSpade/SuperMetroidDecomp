namespace SuperMetroid.Core.Game;

/// <summary>Zebetite variable F: a barrier generation's form. Only the sign bit is set by the cartridge.</summary>
[Flags]
public enum ZebetiteGenerationFlags : ushort
{
    /// <summary>A single large barrier.</summary>
    None = 0,
    /// <summary>Bit 15: the generation is a split pair, so the primary half spawns its linked second half.</summary>
    LinkedPair = 0x8000,
}
