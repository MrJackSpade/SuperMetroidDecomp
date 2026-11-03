using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The two colours the game installs over the HUD's effect colours while a message box is open (#1512), which the import
/// pre-colours each message box picture in. Gameplay names no colour of the message box: its pictures show their own.
/// </summary>
internal static class MessageBoxColourDefinitions
{
    /// <summary>The ink of a tileset's BG3 picture the light message colour shows in: the HUD effect colours' tenth.</summary>
    internal const int LightInk = 25;

    /// <summary>The light colour installed for the message box.</summary>
    internal static RgbColor Light => new(17, 29, 2);

    /// <summary>The ink of a tileset's BG3 picture the dark message colour shows in: the HUD effect colours' eleventh.</summary>
    internal const int DarkInk = 26;

    /// <summary>The dark colour installed for the message box.</summary>
    internal static RgbColor Dark => new(31, 0, 0);
}
