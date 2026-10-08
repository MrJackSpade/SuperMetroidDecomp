namespace SuperMetroid.Core.Game;

/// <summary>The seven mutually exclusive retail world-area indices.</summary>
/// <remarks>
/// The order is the cartridge ABI used by room headers, load-station tables, maps, and
/// area-dependent gameplay tables. It is not a flags field.
/// </remarks>
public enum AreaId : byte
{
    /// <summary>The planetary surface and adjoining caverns, encoded as retail area index 0.</summary>
    Crateria = 0,

    /// <summary>The upper and lower Brinstar region, encoded as retail area index 1.</summary>
    Brinstar = 1,

    /// <summary>The upper and lower Norfair region, encoded as retail area index 2.</summary>
    Norfair = 2,

    /// <summary>The derelict spacecraft region, encoded as retail area index 3.</summary>
    WreckedShip = 3,

    /// <summary>The flooded Maridia region, encoded as retail area index 4.</summary>
    Maridia = 4,

    /// <summary>The final Tourian region, encoded as retail area index 5.</summary>
    Tourian = 5,

    /// <summary>The Ceres research station prologue, encoded as retail area index 6.</summary>
    Ceres = 6,
}

/// <summary>Checked conversions at raw cartridge and array-index boundaries.</summary>
public static class AreaIds
{
    /// <summary>The number of area indices accepted by retail room and save data.</summary>
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
