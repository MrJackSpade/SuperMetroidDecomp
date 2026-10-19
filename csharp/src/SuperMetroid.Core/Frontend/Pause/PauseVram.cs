using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Typed view of the pause screen's VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: pause screen VRAM")]
public readonly struct PauseVram(SnesVram vram)
{
    /// <summary>The shared map characters.</summary>
    public VramRegion MapCharacters => new(vram, PauseVramLayout.MapCharacters);

    /// <summary>The pause interface characters.</summary>
    public VramRegion InterfaceCharacters => new(vram, PauseVramLayout.InterfaceCharacters);

    /// <summary>The map-object OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, PauseVramLayout.ObjectCharacters);

    /// <summary>The pause frame's BG2 tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, PauseVramLayout.Bg2Tilemap);

    /// <summary>The map/equipment BG1 tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, PauseVramLayout.Bg1Tilemap);

    /// <summary>The button-label rows.</summary>
    public VramRegion ButtonRows => new(vram, PauseVramLayout.ButtonRows);

    /// <summary>The retained gameplay BG3 tilemap.</summary>
    public VramRegion RetainedHudTilemap => new(vram, PauseVramLayout.RetainedHudTilemap);

    /// <summary>The mutable HUD rows.</summary>
    public VramRegion AcceptedHudRows => new(vram, PauseVramLayout.AcceptedHudRows);

    /// <summary>Keeps gameplay's BG3 tilemap resident from the plane it displayed (#1351).</summary>
    public void RetainHudTilemap(Layer3Plane gameplay) =>
        RetainedHudTilemap.Write(Ensure.NotNull(gameplay).Words[..PauseVramLayout.RetainedHudTilemap.Count]);

    /// <summary>Mirrors the mutable HUD rows of the plane gameplay displayed (#1351).</summary>
    public void MirrorAcceptedHudRows(Layer3Plane gameplay) =>
        AcceptedHudRows.Write(Ensure.NotNull(gameplay).Words.Slice(
            PauseVramLayout.AcceptedHudRows.Offset - PauseVramLayout.RetainedHudTilemap.Offset, PauseVramLayout.AcceptedHudRows.Count));

    /// <summary>The cleared FX rows.</summary>
    public VramRegion Bg3FxRows => new(vram, PauseVramLayout.Bg3FxRows);
}
