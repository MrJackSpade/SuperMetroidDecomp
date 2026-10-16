namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Typed view of Samus's OBJ character VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: Samus OBJ character VRAM")]
public readonly struct SamusVram(SnesVram vram)
{
    /// <summary>First half of the top-body tiles.</summary>
    public VramRegion BodyTopFirst => new(vram, SamusVramLayout.BodyTopFirst);

    /// <summary>Second half of the top-body tiles.</summary>
    public VramRegion BodyTopSecond => new(vram, SamusVramLayout.BodyTopSecond);

    /// <summary>First half of the bottom-body tiles.</summary>
    public VramRegion BodyBottomFirst => new(vram, SamusVramLayout.BodyBottomFirst);

    /// <summary>Second half of the bottom-body tiles.</summary>
    public VramRegion BodyBottomSecond => new(vram, SamusVramLayout.BodyBottomSecond);
}
