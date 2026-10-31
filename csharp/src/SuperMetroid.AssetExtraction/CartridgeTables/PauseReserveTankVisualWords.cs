using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The pause-menu spritemap ordinals that name the reserve-tank visuals (#1481), as the cartridge's reserve drawing
/// code and its partial-fill table select them, decoded once here. Core's values are arbitrary.
/// </summary>
internal static class PauseReserveTankVisualWords
{
    /// <summary>The menu spritemap ordinal of each reserve-tank visual.</summary>
    internal static readonly NativeWordIdentities<PauseReserveTankVisual> Ordinals = new(new Dictionary<PauseReserveTankVisual, ushort>
    {
        [PauseReserveTankVisual.Full] = 0x1b,
        [PauseReserveTankVisual.EndCap] = 0x1f,
        [PauseReserveTankVisual.Empty] = 0x20,
        [PauseReserveTankVisual.Fill1] = 0x21,
        [PauseReserveTankVisual.Fill2] = 0x22,
        [PauseReserveTankVisual.Fill3] = 0x23,
        [PauseReserveTankVisual.Fill4] = 0x24,
        [PauseReserveTankVisual.Fill5] = 0x25,
        [PauseReserveTankVisual.Fill6] = 0x26,
        [PauseReserveTankVisual.Fill7] = 0x27,
    });
}
