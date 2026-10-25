namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A barrier generation's form. Its value is arbitrary; verification's native view encodes it as the cartridge's sign bit (#1413).</summary>
[Flags]
public enum ZebetiteGenerationFlags : ushort
{
    /// <summary>A single large barrier.</summary>
    None = 0,
    /// <summary>The generation is a split pair, so the primary half spawns its linked second half.</summary>
    LinkedPair = 1,
}
