namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Typed view of the shared menu VRAM planes (#630).</summary>
[PpuLayoutBoundary("rendering view: shared menu VRAM")]
public readonly struct MenuVram(SnesVram vram)
{
    /// <summary>The menu BG1 tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, MenuVramLayout.Bg1Tilemap);

    /// <summary>The file-select room map's area-map tilemap.</summary>
    public VramRegion AreaMapTilemap => new(vram, MenuVramLayout.AreaMapTilemap);

    /// <summary>The shared map characters.</summary>
    public VramRegion MapCharacters => new(vram, MenuVramLayout.MapCharacters);

    /// <summary>The world map's area-background tilemap.</summary>
    public VramRegion AreaBackgroundTilemap => new(vram, MenuVramLayout.AreaBackgroundTilemap);

    /// <summary>The map-object OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, MenuVramLayout.ObjectCharacters);

    /// <summary>The menu BG2 tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, MenuVramLayout.Bg2Tilemap);
}
