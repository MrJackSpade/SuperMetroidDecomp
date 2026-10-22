using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>
/// The uploads over the OBJ characters Ceres Ridley's getaway makes (#1392): the escape timer's pages and the warning's
/// characters, in the order its transfer list makes them. Its other transfers reach the backgrounds.
/// </summary>
internal static class CeresEscapeCharacterUploads
{
    /// <summary>Each transfer of the getaway's list that reaches the OBJ characters, as the upload it makes there.</summary>
    [PpuLayoutBoundary("codec: Ceres escape VRAM transfer records' native destinations ($A6:C26E)")]
    internal static IReadOnlyList<RuntimeCharacterUpload> All { get; } = [.. CeresEscapeVramTransferDefinitions.Records
        .Where(record => ObjCharacterUploads.TableByte(VramDmaDestination.FromNative(record.DestinationWord)) is >= 0 and < ObjCharacterUploads.TableByteCount)
        .Select(record => RuntimeCharacterUpload.To(VramDmaDestination.FromNative(record.DestinationWord), record.ByteCount, Source(record)))];

    /// <summary>The artwork a getaway transfer to the OBJ characters shows: an escape timer page, or the warning's characters.</summary>
    /// <exception cref="InvalidDataException">The transfer names neither.</exception>
    private static ObjCharacterSource Source(TileTransferDefinition record) =>
        EscapeTimerTilePages.TryMatch(record.SourceAddress, record.ByteCount, record.DestinationWord, out EscapeTimerTilePage page)
            ? new AssetObjCharacters(EscapeTimerTilePages.AssetOf(page))
            : CeresEscapeTileCharacters.ForTransfer(record.SourceAddress, record.ByteCount) ?? throw new InvalidDataException(
                $"Ceres escape transfer from ${record.SourceAddress:X6} to the OBJ characters names neither a timer page nor a warning page.");
}
