namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon's destroyable flames start: enraged and spiral angles and rain columns.</summary>
public static class PhantoonFlameSpawns
{
    /// <summary>The enraged flame <paramref name="index"/>'s start angle, zero through fifteen.</summary>
    public static byte RageAngle(int index) => At(PhantoonFlameSpawnDefinitions.RageAngles, index, nameof(index));

    /// <summary>The screen X of rain column <paramref name="column"/>, zero through eight.</summary>
    public static byte RainX(int column) => At(PhantoonFlameSpawnDefinitions.RainXs, column, nameof(column));

    /// <summary>The spiral flame <paramref name="index"/>'s start angle, zero through seven.</summary>
    public static byte SpiralAngle(int index) => At(PhantoonFlameSpawnDefinitions.SpiralAngles, index, nameof(index));

    /// <summary>Reads one authored value, rejecting a position outside the table.</summary>
    private static byte At(byte[] table, int position, string name) =>
        (uint)position < (uint)table.Length ? table[position] : throw new ArgumentOutOfRangeException(name);
}
