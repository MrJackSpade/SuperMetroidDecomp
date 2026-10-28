namespace SuperMetroid.Core.Rendering;

/// <summary>Definitions used when the gameplay display is captured.</summary>
internal static class GameplayRenderDefinitions
{
    /// <summary>The gameplay sprites' setting: small sprites are 8 pixels and large ones 16, drawn from the gameplay sprite tiles.</summary>
    internal const byte ObjectSelection = 0x03;
    /// <summary>During the Ceres Ridley getaway the room's background replaces the turning floor on the last sixteen screen lines.</summary>
    internal const int CeresFloorFirstLine = ScreenGeometry.ScreenHeightPixels - 16;
}
