namespace SuperMetroid.Core.Game;

/// <summary>Statue movement and Samus joint offsets for Wrecked Ship and Lower Norfair.</summary>
public static class ChozoCarryMotionDefinitions
{
    /// <summary>$AA:E630: X velocity in 8.8; its absolute magnitude also drives downward collision.</summary>
    private static ReadOnlySpan<short> Magnitudes => [0,0,0,0,0x200,0x300,0xe00,0x800,0x200,0x300,0xe00,0x800,0,0,0,0];
    /// <summary>$AA:E6B0: Samus Y offsets, identical for both facing halves.</summary>
    private static ReadOnlySpan<short> YOffsets => [-32,-25,-23,-23,-23,-24,-25,-24,-23,-24,-25,-24,-23,-23,-23,-23];

    /// <summary>$AA:E630/E670/E6B0: one of 32 velocity/X-joint/Y-joint records, selected by an even byte offset.</summary>
    public static (short Velocity, short SamusX, short SamusY) Read(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset > 62)
            throw new InvalidDataException($"Chozo carry offset {byteOffset:X4} is outside its 32 records.");
        int index = byteOffset >> 1;
        int local = index & 15;
        int sign = index < 16 ? -1 : 1;
        int x = local == 0 ? 28 : local == 1 ? 30 : 32;
        return ((short)(sign * Magnitudes[local]), (short)(sign * x), YOffsets[local]);
    }
}
