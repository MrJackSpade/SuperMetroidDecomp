using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The shades the import pre-colours the Zebetites in (#1512): each frame of their shared two-colour pulse.</summary>
public static partial class ZebetiteShades
{
    /// <summary>The Zebetites at pulse frame <paramref name="pulse"/> (#2259).</summary>
    public static PictureShade Pulse(ZebetitePulse pulse) => new("zebetite-pulse", pulse.Step);
}
