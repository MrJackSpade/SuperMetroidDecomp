namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The three shared COLDATA shadow bytes (cartridge <c>$0074-$0076</c>) that NMI <c>$80:91EE</c> uploads to
/// <c>$2132</c>. Each slot holds a raw COLDATA byte: channel select bits and a five-bit intensity. Slot names follow
/// the cartridge's mirrors; Fireflea deliberately writes its blue byte into the green slot and its green byte into the
/// blue slot. The shadows persist across rooms, as the cartridge's do.
/// </summary>
public sealed class FixedColorState
{
    /// <summary>The raw byte in the first shadow slot, normally the red write.</summary>
    public byte Red { get; internal set; }

    /// <summary>The raw byte in the second shadow slot.</summary>
    public byte Green { get; internal set; }

    /// <summary>The raw byte in the third shadow slot.</summary>
    public byte Blue { get; internal set; }

    /// <summary>The shadows as an accepted NMI publishes them.</summary>
    public FixedColorRegisterSnapshot Snapshot() => new(Red, Green, Blue);
}
