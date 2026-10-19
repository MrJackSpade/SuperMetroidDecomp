namespace SuperMetroid.Core.Progression;

/// <summary>Definition data of the cartridge's random-number word.</summary>
internal static class CartridgeRandomRomData
{
    /// <summary>The state the reset vector (<c>$80:8534</c>) stores in <c>$05E5</c>.</summary>
    internal static SharedRandomState PowerOnState { get; } = new(0x0061);
}
