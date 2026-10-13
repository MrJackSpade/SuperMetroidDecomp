namespace SuperMetroid.Core.Assets;

/// <summary>The presentation boundary of the Baby Metroid (#641): the bank-$A9 spritemaps of its pulse, shared by the Tourian Shitroid and the cutscene Baby.</summary>
internal static class BabyMetroidVisualDefinitions
{
    /// <summary>$A9:F9A8: the closed pulse pose.</summary>
    private const ushort Closed = 0xf9a8;
    /// <summary>$A9:FA40: the half-open pulse pose.</summary>
    private const ushort HalfOpen = 0xfa40;
    /// <summary>$A9:FAD8: the wide-open pulse pose.</summary>
    private const ushort WideOpen = 0xfad8;

    /// <summary>The spritemap of pulse pose <paramref name="index"/>: closed, half open or wide open.</summary>
    internal static ushort Spritemap(int index) => index switch
    {
        0 => Closed,
        1 => HalfOpen,
        2 => WideOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "The Baby Metroid has three pulse poses."),
    };
}
