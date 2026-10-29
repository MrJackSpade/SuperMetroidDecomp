using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>A palette-FX colour group of gameplay's sprite colours, from colour <paramref name="Colour"/> of store <paramref name="Store"/> (#1464).</summary>
/// <param name="Store">The sprite colour store the group's first written colour is in.</param>
/// <param name="Colour">The group's first written colour within its set.</param>
internal sealed record SpriteSetPlacement(SpriteColourStore Store, int Colour) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners) => Ensure.NotNull(owners).SpriteSets.FromSet(Store.Set, Colour);
}
