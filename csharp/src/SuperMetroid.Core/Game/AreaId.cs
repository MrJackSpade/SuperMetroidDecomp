namespace SuperMetroid.Core.Game;

/// <summary>The seven mutually exclusive retail world-area indices.</summary>
/// <remarks>
/// The order is the cartridge ABI used by room headers, load-station tables, maps, and
/// area-dependent gameplay tables. It is not a flags field.
/// </remarks>
public enum AreaId : byte
{
    Crateria = 0,
    Brinstar = 1,
    Norfair = 2,
    WreckedShip = 3,
    Maridia = 4,
    Tourian = 5,
    Ceres = 6,
}

/// <summary>Checked conversions at raw cartridge and array-index boundaries.</summary>
public static class AreaIds
{
    public const int RetailCount = 7;

    /// <summary>Validates and names one area byte read from cartridge data.</summary>
    public static AreaId FromCartridge(byte value, string source)
    {
        if (value >= RetailCount)
        {
            throw new InvalidDataException(
                $"{source} contains area ${value:X2}, outside the seven retail areas.");
        }

        return (AreaId)value;
    }

    /// <summary>Returns a safe zero-based index for a typed area value.</summary>
    public static int ToIndex(AreaId area)
    {
        int index = (byte)area;
        if ((uint)index >= RetailCount)
            throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID.");
        return index;
    }
}
