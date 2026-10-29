using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where every frontend screen places the colourings its sprites draw in (#1464): a screen colour group that is one whole
/// sprite set, in that set, as <see cref="ScreenColourGroupDefinitions"/> lays the groups out for every screen alike.
/// </summary>
internal sealed class ScreenSpriteColourPlacement : SpriteColourPlacement
{
    /// <summary>The one placement, which every screen shares.</summary>
    internal static ScreenSpriteColourPlacement Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> places screen colourings.</summary>
    private ScreenSpriteColourPlacement()
    {
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// The colouring is not a screen colour group, or its group is not one whole sprite set.
    /// </exception>
    internal override int HoldingSet(SpriteColouring colouring) => Ensure.NotNull(colouring) is ScreenSpriteColouring screen
        ? ScreenColourGroups.SpriteSet(screen.Group)
        : throw new InvalidOperationException($"A frontend screen places no sprite colouring {colouring}.");
}
