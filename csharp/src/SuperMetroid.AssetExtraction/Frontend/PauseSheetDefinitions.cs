using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>Definitions the pause screen's sprite sheet is composed with (#1463).</summary>
[EncodingOwner("object-selection register value of the pause screen")]
internal static class PauseSheetDefinitions
{
    /// <summary>The sprite-size selection the pause screen installs: the sprite sizes the pause sheet's frames are composed in.</summary>
    internal const byte ObjectSelection = 0x01;
}
