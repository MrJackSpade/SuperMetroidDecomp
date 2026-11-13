using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The picture store's index (#1635, #1644, #1693), <c>pictures.json</c>: its format revision, how many pages it holds,
/// every stored picture under its tag (#1639), and apart from the pictures each kind of map they are shown with: every
/// map's tag and size. A kind of map is absent exactly when no frame shows a picture with one, and the store then writes no
/// file for it. The index records no hash and no cartridge: nothing compared them, and the installation receipt names the
/// cartridge.
/// </summary>
/// <param name="Version">The format revision, <see cref="FormatVersion"/>.</param>
/// <param name="PageCount">How many pages the store holds, <c>pictures-0000.png</c> onward.</param>
/// <param name="Pictures">Every stored picture's page and rectangle, by its tag, in write order.</param>
/// <param name="RecolourRegionMaps">The recolour-region maps, or null where no frame shows a picture with one.</param>
/// <param name="BlendMaps">The blend maps, or null where no frame shows a picture with one.</param>
internal sealed record PictureStoreIndex(int Version, int PageCount, Dictionary<string, StoredPictureDocument> Pictures,
    StoredMapsDocument? RecolourRegionMaps, StoredMapsDocument? BlendMaps)
{
    /// <summary>
    /// The format revision: 1 holds shelf-packed RGBA pages with their recolour-region and blend maps (#1635); 2 lists every
    /// stored picture under its tag (#1639); 3 stores each picture by its pixels only, and lists the maps the pictures are
    /// shown with apart from the pages, each map once under its own tag (#1644); 4 stores each picture as the box of what its
    /// frames show, its clear margin left out, and each map at the size of that box (#1646); 5 is the store's index, with its
    /// page count and no page, map or cartridge hash (#1693).
    /// </summary>
    internal const int FormatVersion = 5;

    /// <summary>Camel-case fields, indented; unknown fields are rejected.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
