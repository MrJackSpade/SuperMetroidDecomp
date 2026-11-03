using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The shades the import pre-colours the Zebetites in (#1512): each frame of their shared two-colour pulse.</summary>
public static class ZebetiteShades
{
    /// <summary>The Zebetites at pulse frame <paramref name="frame"/>, zero through seven.</summary>
    public static PictureShade Pulse(int frame) => new("zebetite-pulse", frame);
}
