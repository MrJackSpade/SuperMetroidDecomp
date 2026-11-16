using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Cinematics;

/// <summary>The spritemap one step of an ending typed text shows (#2195), which the import reads its stock artwork from.</summary>
/// <param name="Typed">The text and how many of its letters the step shows.</param>
/// <param name="Spritemap">Where the step's spritemap is stored.</param>
internal readonly record struct CinematicTypedSpritemapSource(EndingTextTyping Typed, SnesAddress Spritemap);
