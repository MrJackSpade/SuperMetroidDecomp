namespace SuperMetroid.Core.Game;

/// <summary>One authored Mode 7 getaway frame; velocities retain native signed-word encoding.</summary>
public readonly record struct CeresRidleyGetawayFrame(ushort Zoom, ushort XVelocity, ushort YVelocity);

/// <summary>Immutable NTSC Ceres getaway zoom and translation curves.</summary>
public static class CeresRidleyGetawayDefinitions
{
    /// <summary>$A6:AE4D, CeresRidleyGetawayZoomLevelTable: 112 frames followed by $FFFF.</summary>
    public const int ZoomReferenceAddress = 0xa6ae4d;
    /// <summary>$A6:AF2F, CeresRidleyGetawayYVelocityTable: signed vertical increments.</summary>
    public const int YReferenceAddress = 0xa6af2f;
    /// <summary>$A6:B00F, CeresRidleyGetawayXVelocityTable: signed values subtracted from horizontal offset.</summary>
    public const int XReferenceAddress = 0xa6b00f;
    /// <summary>Native zoom terminator; no translation records are consumed on this frame.</summary>
    public const ushort Finished = 0xffff;

    /// <summary>Preserves the serialized native byte index, including its terminal entry.</summary>
    public static CeresRidleyGetawayFrame FromByteIndex(ushort byteIndex)
    {
        if ((byteIndex & 1) != 0 || byteIndex > 224)
            throw new InvalidDataException($"Ceres Ridley getaway byte index ${byteIndex:X4} is outside its authored even entries.");
        int frame = byteIndex >> 1;
        if (frame == 112)
            return new(Finished, 0, 0);
        int zoom = frame switch
        {
            < 32 => 0x800,
            < 48 => 0x800 - (frame - 31) * 16,
            70 => 0x430,
            86 => 0x230,
            < 96 => 0x700 - (frame - 47) * 32,
            _ => Math.Max(0x20, 0x100 - (frame - 95) * 16),
        };
        int y = frame switch
        {
            < 12 => -6, < 22 => -4, < 29 => -2, < 45 => -1,
            < 80 => 0, < 88 => 1, < 96 => 2,
            96 => 3, < 104 => (frame - 95) * 2,
            104 => 0x14, 105 => 0x18, 106 => 0x2c, 107 => 0x30,
            108 => 0x80, _ => 0x100,
        };
        int x = frame switch
        {
            < 80 => -1, < 88 => 0, < 96 => 1,
            < 104 => 2 + (frame - 96) / 2,
            < 108 => 8 + (frame - 104) * 4,
            _ => 0x20,
        };
        return new((ushort)zoom, unchecked((ushort)x), unchecked((ushort)y));
    }
}
