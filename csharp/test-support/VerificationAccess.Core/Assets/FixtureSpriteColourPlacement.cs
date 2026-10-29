using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A placement for fixtures drawn outside any room or screen (#1464): it places the colourings every room places alike,
/// a shared role's and an encounter's, such as the fixture colourings <c>SpriteColouring.InFixtureSet</c> makes.
/// </summary>
internal sealed class FixtureSpriteColourPlacement : SpriteColourPlacement
{
    /// <summary>The one fixture placement.</summary>
    internal static FixtureSpriteColourPlacement Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> places fixture colourings.</summary>
    private FixtureSpriteColourPlacement()
    {
    }

    /// <inheritdoc />
    internal override SpriteColourStore StoreOf(SpriteColouring colouring) => GraphicsSetColourPlacement.FixedStore(colouring);
}
