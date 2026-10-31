using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Where each owner of gameplay's colours lies in the cartridge's 256-colour image (#1464): the import pipeline's private
/// knowledge, which decodes the installed starting image and native colour images into owners. Core never addresses a
/// colour by these positions.
/// </summary>
[EncodingOwner("colour positions of gameplay's 256-colour image")]
internal static class GameplayColourPositionDefinitions
{
    /// <summary>The first background set's first colour.</summary>
    internal const int BackgroundSetsFirst = 0;

    /// <summary>The first sprite set's first colour: the image's upper half.</summary>
    internal const int SpriteSetsFirst = 128;

    /// <summary>The colours of one owner's eight sets.</summary>
    internal const int SetsColours = 128;

    /// <summary>The colours of one set.</summary>
    internal const int SetColours = 16;

    /// <summary>Samus's first suit colour, which her draws show as sprite set four.</summary>
    internal const int SamusSuitFirst = 0xc0;

    /// <summary>Samus's first beam colour, which her draws show as sprite set six.</summary>
    internal const int SamusBeamsFirst = 0xe0;

    /// <summary>The enemy projectiles' first colour, sprite set five, which every room load restores from the starting image.</summary>
    internal const int EnemyProjectilesFirst = 208;
}
