namespace SuperMetroid.Desktop.Input;

/// <summary>Actual bit mask returned by WinMM's JOYCAPS capability field.</summary>
[Flags]
internal enum JoystickCapabilityFlags : uint
{
    /// <summary><c>JOYCAPS_HASPOV</c>: the device reports directional-hat information; without this capability, the adapter supplies the centered POV value regardless of the returned position field.</summary>
    HasPointOfView = 0x00000010,
}
