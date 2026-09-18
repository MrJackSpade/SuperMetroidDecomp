namespace SuperMetroid.Core.Game;

/// <summary>Fixed cartridge policy controlling the arm-cannon cover for each HUD item.</summary>
internal static class SamusArmCannonDefinitions
{
    /// <summary>
    /// <c>$90:C7D9</c>, six bytes indexed by the selected HUD item. The native symbol is
    /// <c>ArmCannonOpenFlags</c>; one requests an open cover and zero a closed cover.
    /// </summary>
    public const int OpenFlagTable = 0x90c7d9;

    /// <summary>Number of HUD selections accepted by the native arm-cannon dispatcher.</summary>
    public const int HudItemCount = 6;

    private static ReadOnlySpan<byte> OpenFlags => [0, 1, 1, 0, 1, 0];

    /// <summary>Returns the native desired cover state for a validated HUD-item index.</summary>
    public static byte DesiredOpenFlag(ushort selectedHudItem)
    {
        if (selectedHudItem >= HudItemCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedHudItem),
                selectedHudItem,
                $"HUD item must be in the native range 0..{HudItemCount - 1}.");
        }

        return OpenFlags[selectedHudItem];
    }

    /// <summary>Returns the complete six-byte native table for cartridge verification.</summary>
    public static ReadOnlySpan<byte> AllOpenFlags => OpenFlags;
}
