
namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ScreenTileSheet"/> that only verification reads.</summary>
internal static class ScreenTileSheetConstants
{
    /// <summary>Rows of tiles in each palette row's block of a sheet of <paramref name="characters"/> characters.</summary>
    public static int BlockRowsOf(int characters) => (Ensure.BetweenInclusive(1, int.MaxValue, characters) + ScreenTileSheet.Columns - 1) / ScreenTileSheet.Columns;
}
