using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The colours Mother Brain's encounter has loaded (#1512): the shades laid over its sprites (her brain, neck, legs,
/// the Baby and the objects she spawns), over the background colours her body layer shares with her room, and, once her
/// brain's colours are copied into the corpse's row as she dies, the layer her detached brain draws in.
/// </summary>
internal sealed class MotherBrainColours : IGameplayShadingSource
{
    /// <summary>The pictures that share her background colours: her room's and her body layer's.</summary>
    private static readonly GameplayPictures[] Shared = [GameplayPictures.Room, GameplayPictures.BossBody];

    /// <summary>The shades laid over the encounter's sprites.</summary>
    internal LaidShades Sprites { get; } = new();

    /// <summary>The shades laid over the background colours her body layer and room share.</summary>
    internal LaidShades Background { get; } = new();

    /// <summary>The corpse's colours: her brain's as they stood when her death copied them, then the corpse's fade; null before.</summary>
    internal LaidShades? Corpse { get; private set; }

    /// <summary>The shading the encounter's sprites draw in.</summary>
    internal PictureShading SpriteShading => Sprites.Over(PictureShading.Standard);

    /// <summary>The shading her brain draws in: the corpse's colours once her death copied them, otherwise her sprites'.</summary>
    internal PictureShading BrainShading => (Corpse ?? Sprites).Over(PictureShading.Standard);

    /// <summary>Lays a load that writes the same colours over her body layer's and her brain's.</summary>
    internal void LayOverBodyAndSprites(PictureShade shade)
    {
        Background.Lay(shade);
        Sprites.Lay(shade);
    }

    /// <summary>Copies her brain's colours into the corpse's row, which her brain draws in from then on.</summary>
    internal void CopyBrainToCorpse() => Corpse = Sprites.Copy();

    /// <summary>Lays a load of the corpse's colours over its row.</summary>
    /// <exception cref="InvalidOperationException">Her brain's colours have not been copied into the corpse's row.</exception>
    internal void LayOverCorpse(PictureShade shade) =>
        (Corpse ?? throw new InvalidOperationException("Mother Brain's corpse fades after her brain's colours are copied to it.")).Lay(shade);

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (GameplayPictures pictures in Shared)
            foreach (PictureShade shade in Background.Laid)
                frame.Shade(pictures, shade);
    }
}
