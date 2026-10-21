using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="EnemyArtworkCatalogsTooling"/> that only verification reads.</summary>
internal static class EnemyArtworkCatalogsToolingConstants
{
    /// <summary>The packed bank-and-word identity of a frame, as the cartridge-address catalogs keyed it.</summary>
    internal static int NativeIdentity(EnemyFrameId frame) => (int)EnemyFrameCartridgeLookup.Of(frame);
}
