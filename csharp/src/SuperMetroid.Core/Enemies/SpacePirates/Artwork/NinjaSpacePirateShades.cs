using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The looks a Ninja Space Pirate's divekick flashes between (#1512): the gold wall Pirates' colours, and the Pirate's
/// own stance colours it flashes back to and lands in. The import colours the gold wall shade from the gold wall Pirate's
/// installed palette.
/// </summary>
internal static class NinjaSpacePirateShades
{
    /// <summary>The gold wall Pirates' colours a divekick flashes in.</summary>
    internal static PictureShade GoldWall { get; } = new("ninja-space-pirate-gold-wall", 0);

    /// <summary>A draw in the gold wall Pirates' colours.</summary>
    internal static PictureShading GoldWallFlash { get; } = PictureShading.Shaded(GoldWall);

    /// <summary>
    /// A draw in the Pirate's own colours, which the divekick flashes back to and lands in. The game selects a palette the
    /// Metal Pirates room loads the gold Ninja Pirates' colours into; the escape's Climb loads its silver Pirate's
    /// elsewhere, so there the game showed whatever that palette held. Each Pirate shows its own colours here.
    /// </summary>
    internal static PictureShading Stance { get; } = PictureShading.Standard;
}
