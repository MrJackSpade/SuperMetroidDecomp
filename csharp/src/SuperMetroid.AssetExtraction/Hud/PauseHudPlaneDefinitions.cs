using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>The cartridge's tiles the pause screen's HUD plane shows beyond the HUD's own (#1466).</summary>
internal static class PauseHudPlaneDefinitions
{
    /// <summary>
    /// The tile <c>QueueClearingOfFxTilemap</c> ($80:A211) fills every BG3 row beneath the HUD with as pause opens: pause
    /// HUD character $4E in colour variant six, in the lower priority tier.
    /// </summary>
    internal static BackgroundTile ClearedCell { get; } = new(0x4e, 6, highPriority: false);
}
