using SuperMetroid.AssetExtraction.Frontend.Map;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>Cartridge values and helpers of <see cref="FileSelectSourcePresentation"/> that only verification reads.</summary>
internal static class FileSelectSourcePresentationConstants
{
    /// <summary>Reads and validates both extracted file-select documents.</summary>
    /// <param name="extraction">The extraction's layout and artwork documents.</param>
    /// <exception cref="InvalidDataException">Either document is invalid, or a patch placement escapes its page.</exception>
    public static FileSelectSourcePresentation Load(FileSelectExtraction extraction)
    {
        Ensure.NotNull(extraction);
        return FileSelectSourcePresentation.Load(new MemoryStream(extraction.Layout, writable: false), new MemoryStream(extraction.Artwork, writable: false));
    }
}
