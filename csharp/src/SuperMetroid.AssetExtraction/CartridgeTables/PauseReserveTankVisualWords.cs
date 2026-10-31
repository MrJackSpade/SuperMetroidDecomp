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
        [PauseReserveTankVisual.Full] = 27,
        [PauseReserveTankVisual.EndCap] = 31,
        [PauseReserveTankVisual.Empty] = 32,
        [PauseReserveTankVisual.Fill1] = 33,
        [PauseReserveTankVisual.Fill2] = 34,
        [PauseReserveTankVisual.Fill3] = 35,
        [PauseReserveTankVisual.Fill4] = 36,
        [PauseReserveTankVisual.Fill5] = 37,
        [PauseReserveTankVisual.Fill6] = 38,
        [PauseReserveTankVisual.Fill7] = 39,
    });
}
