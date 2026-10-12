using SuperMetroid.Core.Game;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="CeresEscapeOverlayTilemapDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresEscapeOverlayTilemapDefinitionsTooling
{
    internal static bool IsSource(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
            if (page.SourceAddress == sourceAddress &&
                page.WordCount * sizeof(ushort) == byteCount)
                return true;
        return false;
    }
}
