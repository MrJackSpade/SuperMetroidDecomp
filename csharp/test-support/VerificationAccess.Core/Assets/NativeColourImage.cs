using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access: owner-keyed colours in the cartridge's 256-colour order (#1464), the native boundary at which
/// checks compare colours with native snapshots and recorded colour memory. Only this boundary and the import pipeline
/// know where each owner lies in that order.
/// </summary>
internal static class NativeColourImage
{
    /// <summary>The colours of a native colour image.</summary>
    internal const int ColorCount = SnesCgram.ColorCount;

    /// <summary>The native image of each published frame's colours, built once per frame.</summary>
    private static readonly ConditionalWeakTable<BackgroundColours, RgbColor[]> Published = [];

    /// <summary>The native image of each base palette's starting colours, built once per palette.</summary>
    private static readonly ConditionalWeakTable<GameplayStartingColours, RgbColor[]> Starting = [];

    /// <summary>
    /// The native image of <paramref name="colours"/>, built on its first read and kept, as published colours never
    /// change.
    /// </summary>
    internal static ReadOnlySpan<RgbColor> Of(BackgroundColours colours) =>
        Published.GetValue(colours, shown => shown.Backgrounds switch
        {
            GameplayShownBackgrounds gameplay => Encode(gameplay.Sets, shown.Sprites),
            // A screen's sets lie in the native image in their own order, as the import pipeline decodes them.
            ScreenShownBackgrounds screen => EncodeScreen(screen.Sets, shown.Sprites),
            _ => throw new InvalidOperationException($"Unknown shown background colours {shown.Backgrounds}."),
        });

    /// <summary>The native image of a frontend screen's background and sprite sets, each set at its own order.</summary>
    [WholeImageFixture("a 256-colour native colour image")]
    private static RgbColor[] EncodeScreen(ShownColourSets backgrounds, ShownColourSets sprites)
    {
        var image = new RgbColor[ColorCount];
        for (int set = 0; set < ColourSets.SetCount; set++)
        {
            backgrounds.Set(set).CopyTo(image.AsSpan(GameplayColourPositionDefinitions.BackgroundSetsFirst + set * ColourSets.SetColours));
            sprites.Set(set).CopyTo(image.AsSpan(GameplayColourPositionDefinitions.SpriteSetsFirst + set * ColourSets.SetColours));
        }
        return image;
    }

    /// <summary>The native starting image <paramref name="starting"/> was decoded from, built on its first read and kept.</summary>
    internal static ReadOnlySpan<RgbColor> Of(GameplayStartingColours starting) =>
        Starting.GetValue(starting, colours => Encode(colours.Backgrounds, colours.Sprites));

    /// <summary>The native image of a background and a sprite owner's sets: the background sets, then the sprite sets.</summary>
    [WholeImageFixture("a 256-colour native colour image")]
    internal static RgbColor[] Encode(ShownBackgroundColourSets backgrounds, ShownColourSets sprites)
    {
        var image = new RgbColor[ColorCount];
        BackgroundColourSetImage.Encode(backgrounds).CopyTo(image.AsSpan(GameplayColourPositionDefinitions.BackgroundSetsFirst));
        for (int set = 0; set < ColourSets.SetCount; set++)
            sprites.Set(set).CopyTo(image.AsSpan(GameplayColourPositionDefinitions.SpriteSetsFirst + set * ColourSets.SetColours));
        return image;
    }

    /// <summary>The colours a frame shows when its colour memory holds <paramref name="image"/>, a native 256-colour image.</summary>
    [WholeImageFixture("a frame's colours from a 256-colour native colour image")]
    internal static BackgroundColours Decode(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, ColorCount);
        BackgroundColourSets backgrounds = BackgroundColourSetImage.Decode(
            image.Slice(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        var sprites = new SpriteColourSets();
        sprites.All.CopyFrom(image.Slice(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        return new BackgroundColours(ShownBackgroundColourSets.Of(backgrounds), ShownColourSets.Of(sprites));
    }

    /// <summary>
    /// The colours a frame shows when its colour memory holds <paramref name="image"/>, in the colour model of
    /// <paramref name="like"/>: gameplay's background sets, or a frontend screen's sets each at its own order.
    /// </summary>
    internal static BackgroundColours DecodeLike(BackgroundColours like, ReadOnlySpan<RgbColor> image) =>
        Ensure.NotNull(like).Backgrounds switch
        {
            GameplayShownBackgrounds => Decode(image),
            ScreenShownBackgrounds => DecodeScreen(image),
            _ => throw new InvalidOperationException($"Unknown shown background colours {like.Backgrounds}."),
        };

    /// <summary>The colours a frontend screen shows when its colour memory holds <paramref name="image"/>, each set at its own order.</summary>
    [WholeImageFixture("a frame's colours from a 256-colour native colour image")]
    private static BackgroundColours DecodeScreen(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, ColorCount);
        var backgrounds = new ScreenColourSets();
        backgrounds.All.CopyFrom(image.Slice(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        var sprites = new SpriteColourSets();
        sprites.All.CopyFrom(image.Slice(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        return new BackgroundColours(ShownColourSets.Of(backgrounds), ShownColourSets.Of(sprites));
    }
}
