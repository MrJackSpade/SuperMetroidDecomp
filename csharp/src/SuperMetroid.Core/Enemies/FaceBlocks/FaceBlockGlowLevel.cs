namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// How brightly the Blue Brinstar face blocks' eyes glow (#2477): from a dim red up to a bright yellow. The room-wide glow
/// brightens through each level and dims back through them again, so each level shows twice in a cycle, in the same colours.
/// Each level's colours are the installed shade the import pre-colours for it (<see cref="Artwork.FaceBlockShades"/>); the
/// order and holds of the cycle's steps are <see cref="FaceBlockGlowAnimations"/>.
/// </summary>
internal enum FaceBlockGlowLevel
{
    /// <summary>The eyes at their dimmest, a deep red, where the glow turns to brighten.</summary>
    Dimmest,

    /// <summary>The eyes a step brighter than their dimmest, a red-orange.</summary>
    Dim,

    /// <summary>The eyes a step short of their brightest, an orange.</summary>
    Bright,

    /// <summary>The eyes at their brightest, a yellow, where the glow turns to dim.</summary>
    Brightest,
}
