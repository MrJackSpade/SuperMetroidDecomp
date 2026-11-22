namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One immutable layer of a frame's scene, drawn in the scene's order from back to front. Only the layers the display model
/// declares are layers, so every layer a scene holds is one the backend draws.
/// </summary>
public abstract partial record DisplayLayer
{
    /// <summary>Closes the hierarchy to the layers the display model declares.</summary>
    private protected DisplayLayer()
    {
    }
}
