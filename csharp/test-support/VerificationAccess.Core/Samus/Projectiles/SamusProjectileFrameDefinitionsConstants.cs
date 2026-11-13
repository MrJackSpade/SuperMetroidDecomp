using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="SamusProjectileFrameDefinitions"/> that only verification reads.</summary>
internal static class SamusProjectileFrameDefinitionsConstants
{
    /// <summary>The number of frames.</summary>
    internal static int FrameCount => SamusProjectileFrameOrder.Frames.Count;
}
