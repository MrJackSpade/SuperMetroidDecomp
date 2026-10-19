using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="LibraryBackgroundSources"/> that only verification reads.</summary>
internal static class LibraryBackgroundSourcesConstants
{
    /// <summary>The command-list pointer of <paramref name="program"/>.</summary>
    public static ushort PointerOf(LibraryBackgroundProgram program)
    {
        Ensure.NotNull(program);
        return LibraryBackgroundSourceDefinitions.ByPointer.Single(pair => Object.ReferenceEquals(pair.Value, program)).Key;
    }
}
