using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>On-disk names and manifest bounds for seven area maps and their shared presentation resources.</summary>
public static class AreaMapCatalogFormat
{
    /// <summary>
    /// Supported extracted map-catalog schema version; other manifest versions are rejected. 82 installs no colour document
    /// only the import reads (#1637); 83 makes Mother Brain's rainbow colours and the palette-effect colours import data,
    /// installs her beam backdrop's colours by purpose, and states each highlight frame's look as its highlight colour (#1637);
    /// 84 also installs no document of packed tile placements only the import reads (#1638); 85 installs the menus' and the
    /// ending text's layouts alone, their extracted documents and the map screens being import data (#1638); 86 installs no
    /// manifest, which is import data (#1693); 87 installs neither the staff credits nor the highlight cycle's colours, which
    /// are import data, and installs the highlight cycle as each frame's hold alone (#1693).
    /// </summary>
    public const int Version = 87;
    /// <summary>Manifest-bound non-area artwork files, including Ceres Mode-7 colors, the scene colours (#1544), Mother Brain's beam colours (#1637), the file-select, HUD, menu and ending-text artwork (#1638) and the highlight cycle's holds and colours (#1693).</summary>
    public const int SharedResourceCount = 72;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    /// <summary>
    /// Import-data manifest filename containing extraction provenance and the SHA-256 digest of every catalog resource; only
    /// the import's read opens it (#1693).
    /// </summary>
    public const string ManifestFile = "manifest.json";
    /// <summary>Builds the extracted JSON filename from a validated retail area identity using its lowercase enum name.</summary>
    /// <param name="area">One of the seven retail area identities.</param>
    /// <returns>The area's lowercase name followed by <c>.json</c>, without a directory.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a retail area identity.</exception>
    public static string FileName(AreaId area)
    {
        _ = AreaIds.Require(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
