namespace SuperMetroid.Core.Input;

/// <summary>One USB vendor/product pair reported through WinMM's JOYCAPS structure.</summary>
/// <param name="ManufacturerId">Raw sixteen-bit manufacturer identifier from <c>JOYCAPS.wMid</c>, distinct from the device's WinMM slot number.</param>
/// <param name="ProductId">Raw sixteen-bit product identifier from <c>JOYCAPS.wPid</c>, paired with the manufacturer to select a known physical layout.</param>
public readonly record struct UsbGamepadIdentifier(ushort ManufacturerId, ushort ProductId);
