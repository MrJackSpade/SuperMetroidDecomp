namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Mutually exclusive result of the render owner's final generation-checked swapchain presentation attempt.</summary>
public enum D3D11PresentationResult
{
    /// <summary>DXGI Present returned success; the worker counts the submitted frame as presented, independently of simulation update count.</summary>
    Presented,
    /// <summary>DXGI reported occlusion; the worker retains the immutable frame and retries presentation after a bounded render-thread wait.</summary>
    Occluded,
    /// <summary>The presentation gate rejected the frame's old load/reset generation; DXGI Present was not called even though render/display commands were already submitted.</summary>
    StaleGeneration
}
