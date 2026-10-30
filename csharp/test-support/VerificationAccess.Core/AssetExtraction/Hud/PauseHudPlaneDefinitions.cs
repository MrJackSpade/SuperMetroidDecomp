using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The cartridge's tiles the pause screen's BG3 tilemap holds beyond the HUD's own (#1466), which verification lays out to
/// compare with native VRAM; the game draws nothing for them, as the cleared cell shows nothing.
/// </summary>
internal static class PauseHudPlaneDefinitions
{
    /// <summary>
    /// The tile <c>QueueClearingOfFxTilemap</c> ($80:A211) fills every BG3 row beneath the HUD with as pause opens: pause
    /// HUD character $4E in colour variant six, in the lower priority tier.
    /// </summary>
    internal static BackgroundTile ClearedCell { get; } = new(0x4e, 6, highPriority: false);
}
