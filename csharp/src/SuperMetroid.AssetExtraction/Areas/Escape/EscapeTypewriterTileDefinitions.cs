using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The tiles the cartridge's escape typewriters type their glyphs from (#1467): each program's tile of the letter A,
/// whose colour variant, mirroring and priority every glyph it types takes, and how a character steps from it. Only the
/// import reads them, to picture each typed glyph; gameplay writes the glyph's picture by name.
/// </summary>
internal static class EscapeTypewriterTileDefinitions
{
    /// <summary>The tile of the letter A Ceres's warning typewriter types from.</summary>
    internal static BackgroundTile CeresTileBase { get; } = new(0x182, 5, highPriority: true);

    /// <summary>The tile of the letter A the Zebes warning typewriter types from.</summary>
    internal static BackgroundTile ZebesTileBase { get; } = new(0x210, 1, highPriority: true);

    /// <summary>The letter the tile base draws; later letters draw the characters after it, in alphabet order.</summary>
    internal const byte FirstLetter = (byte)'A';

    /// <summary>The character the exclamation mark is typed as: the glyph after Z.</summary>
    internal const byte ExclamationGlyph = (byte)'Z' + 1;
}
