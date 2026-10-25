namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Development-tool members of <see cref="CeresEscapeOverlayTilemapDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresEscapeOverlayTilemapDefinitionsTooling
{
    /// <summary>Accepts only a known Ceres overlay source whose native byte length matches the requested range.</summary>
    internal static bool IsSource(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
            if (CeresEscapeNativeSources.OverlaySource(page) == sourceAddress &&
                page.WordCount * sizeof(ushort) == byteCount)
                return true;
        return false;
    }
}
