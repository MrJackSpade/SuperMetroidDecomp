using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Shutters;

/// <summary>The native bank-$A2 shutter tables the shutter oracles read. Only verification reads them.</summary>
internal static class ShutterNativeTableAddresses
{
    /// <summary>$A2:EDFB, the vertical shutter's five-entry initial function table.</summary>
    internal static readonly SnesAddress VerticalInitialFunctions = SnesAddress.FromBusAddress(0xa2edfb);

    /// <summary>$A2:F107, the horizontal shutter's five-entry initial function table.</summary>
    internal static readonly SnesAddress HorizontalInitialFunctions = SnesAddress.FromBusAddress(0xa2f107);
}
