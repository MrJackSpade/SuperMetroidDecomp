namespace SuperMetroid.Core.Fx;

/// <summary>Semantic queries for rendering dispatcher values.</summary>
public static class RenderingDiscriminantExtensions
{
    /// <summary>
    /// True only for the two backdrop-blending configurations that animate
    /// Samus's visor. Other raw configuration indices remain unnamed and return false.
    /// </summary>
    public static bool AnimatesVisor(this LayerBlendingConfiguration configuration) =>
        configuration is
            LayerBlendingConfiguration.VisorBackdrop28 or
            LayerBlendingConfiguration.VisorBackdrop2A;
}
