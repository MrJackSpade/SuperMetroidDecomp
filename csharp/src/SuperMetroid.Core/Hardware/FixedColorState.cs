namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The three shared COLDATA shadows (cartridge <c>$0074-$0076</c>) that NMI <c>$80:91EE</c> uploads to
/// <c>$2132</c>. Each slot holds a COLDATA write: channel-select bits and a five-bit intensity. Slot names follow
/// the cartridge's mirrors; Fireflea deliberately writes its blue value into the green slot and its green value into the
/// blue slot. The shadows persist across rooms, as the cartridge's do.
/// </summary>
public sealed class FixedColorState
{
    /// <summary>The write in the first shadow slot, 0 through <see cref="FixedColorRegisterSnapshot.MaximumWrite"/>.</summary>
    public int Red
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, FixedColorRegisterSnapshot.MaximumWrite, value);
    }

    /// <summary>The write in the second shadow slot, 0 through <see cref="FixedColorRegisterSnapshot.MaximumWrite"/>.</summary>
    public int Green
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, FixedColorRegisterSnapshot.MaximumWrite, value);
    }

    /// <summary>The write in the third shadow slot, 0 through <see cref="FixedColorRegisterSnapshot.MaximumWrite"/>.</summary>
    public int Blue
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, FixedColorRegisterSnapshot.MaximumWrite, value);
    }

    /// <summary>The shadows as an accepted NMI publishes them.</summary>
    public FixedColorRegisterSnapshot Snapshot() => new(Red, Green, Blue);
}
