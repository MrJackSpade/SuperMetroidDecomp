using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>A palette-FX colour group of the sprite sets, from colour <paramref name="Colour"/> of set <paramref name="Set"/> (#1464).</summary>
/// <param name="Set">The sprite set the group's first written colour is in.</param>
/// <param name="Colour">The group's first written colour within its set.</param>
internal sealed record SpriteSetPlacement(int Set, int Colour) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners) => Ensure.NotNull(owners).SpriteSets.FromSet(Set, Colour);
}
