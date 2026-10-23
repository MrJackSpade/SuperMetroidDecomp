using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>What an area map's two layers show (#1404).</summary>
/// <param name="Foreground">The area's foreground tiles.</param>
/// <param name="Background">The area's background tiles, which add to the foreground.</param>
internal sealed record AreaMapTiles(ScreenLayerPictures Foreground, ScreenLayerPictures Background);
