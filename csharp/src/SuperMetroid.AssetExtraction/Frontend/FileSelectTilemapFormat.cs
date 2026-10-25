using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend.FileSelect;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>Control words in the bank-$81 file-select tilemap stream format.</summary>
internal static class FileSelectTilemapFormat
{
    /// <summary>Bank containing the file-select text streams.</summary>
    public const int Bank = MapSourceDefinitions.Menu.TilemapTextBank;
    /// <summary>Terminates one tilemap text stream.</summary>
    public const ushort End = ushort.MaxValue;
    /// <summary>Moves the output cursor to the same column on the following row.</summary>
    public const ushort NextRow = ushort.MaxValue - 1;

    /// <summary>The stream labelling save slot <paramref name="slot"/>; the main and copy/clear screens draw the same three.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is not a save slot.</exception>
    public static ushort SlotLabel(int slot) => (uint)slot < FileSelectLayout.SaveSlotCount
        ? FileSelectTilemapSourceDefinitions.SlotLabels[slot]
        : throw new ArgumentOutOfRangeException(nameof(slot));
}
