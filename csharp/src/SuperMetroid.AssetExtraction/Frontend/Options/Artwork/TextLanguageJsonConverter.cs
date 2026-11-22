using SuperMetroid.Core;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>Reads and writes a <see cref="TextLanguage"/> in an installed document by its written-out key (#2560).</summary>
internal sealed partial class TextLanguageJsonConverter() : InstalledKeyJsonConverter<TextLanguage>(TextLanguageKeys.Installed);
