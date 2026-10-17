namespace SuperMetroid.Core.Input;

/// <summary>One USB vendor/product pair reported through WinMM's JOYCAPS structure.</summary>
/// <param name="ManufacturerId">Manufacturer identifier from <c>JOYCAPS.wMid</c>, distinct from the device's WinMM slot number.</param>
/// <param name="ProductId">Product identifier from <c>JOYCAPS.wPid</c>, paired with the manufacturer to select a known physical layout.</param>
public readonly record struct UsbGamepadIdentifier(int ManufacturerId, int ProductId)
{
    /// <summary>The manufacturer identifier, 0 through 65,535: the range a USB vendor identifier spans.</summary>
    public int ManufacturerId { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ManufacturerId);

    /// <summary>The product identifier, 0 through 65,535: the range a USB product identifier spans.</summary>
    public int ProductId { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ProductId);
}
