namespace SuperMetroid.Core.Progression;

/// <summary>Checked conversions between areas and their zero-based indices in room and save data.</summary>
public static class AreaIds
{
    /// <summary>The number of area indices accepted by retail room and save data.</summary>
    public const int RetailCount = 7;

    /// <summary>Validates and names one area index read from room or save data.</summary>
    public static AreaId FromIndex(byte value, string source)
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
