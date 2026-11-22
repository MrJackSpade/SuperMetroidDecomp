namespace SuperMetroid.Core.Assets;

/// <summary>
/// Builds a check's gameplay shading over a constructed room picture that names its backdrop (#2554), as gameplay builds
/// it over the room sheet whose backdrop the sheet holds.
/// </summary>
internal static class GameplayShadingBuilderFixtureAccess
{
    /// <summary>Builds the shading over the constructed room picture.</summary>
    extension(GameplayShadingBuilder self)
    {
        /// <summary>The shading over <paramref name="roomColours"/>, a constructed picture naming its backdrop.</summary>
        /// <exception cref="InvalidOperationException">The picture names no backdrop.</exception>
        internal GameplayShading BuildOverFixture(SpriteImage roomColours) =>
            roomColours.TryGetBackdrop(out ColourKey backdrop)
                ? self.Build(roomColours, backdrop)
                : throw new InvalidOperationException($"The constructed room picture {roomColours.Name} names no backdrop.");
    }
}
