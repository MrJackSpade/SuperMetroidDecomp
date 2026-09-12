namespace SuperMetroid.Core.Game;

/// <summary>Fixed NTSC launch velocities for Alcoon and Fune/Namihe projectiles.</summary>
public static class EnemyFireballLaunchDefinitions
{
    /// <summary>$86:9EF9, Alcoon fireball Y velocities selected by byte offsets 0, 2 and 4.</summary>
    public const int AlcoonReferenceAddress = 0x869ef9;
    /// <summary>$86:DEB6, NamiFuneFireball_XVelocityTable: eight signed left/right pairs.</summary>
    public const int NamiFuneReferenceAddress = 0x86deb6;

    /// <summary>Retains the native byte selector and signed 8.8 encoding.</summary>
    public static ushort AlcoonYVelocity(ushort byteOffset) => byteOffset switch
    {
        0 => 0xff00,
        2 => 0,
        4 => 0x100,
        _ => throw new InvalidDataException($"Alcoon launch byte offset {byteOffset} is not 0, 2 or 4."),
    };

    /// <summary>Only parameter two's low byte selects the authored velocity pair.</summary>
    public static (ushort Left, ushort Right) NamiFuneVelocities(ushort parameter)
    {
        int index = (byte)parameter;
        if (index >= 8)
            throw new InvalidDataException($"Fune/Namihe launch index {index} exceeds eight authored records.");
        ushort magnitude = (ushort)((index + 1) * 0x40);
        return (unchecked((ushort)-magnitude), magnitude);
    }
}
