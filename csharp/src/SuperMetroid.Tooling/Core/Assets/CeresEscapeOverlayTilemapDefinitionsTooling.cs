using SuperMetroid.Core.Game;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="CeresEscapeOverlayTilemapDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresEscapeOverlayTilemapDefinitionsTooling
{
    /// <summary>Checks whether a source address and byte length identify one compiled escape-overlay tilemap page.</summary>
    /// <param name="sourceAddress">Source address to compare with the compiled page identities.</param>
    /// <param name="byteCount">Payload length in bytes; it must equal the page's word count times two.</param>
    /// <returns><see langword="true"/> when both the address and expected byte length match a page.</returns>
    internal static bool IsSource(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
            if (page.SourceAddress == sourceAddress &&
                page.WordCount * sizeof(ushort) == byteCount)
                return true;
        return false;
    }
}
