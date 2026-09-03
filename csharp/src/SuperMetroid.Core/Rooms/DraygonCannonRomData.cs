namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 and WRAM identities used by the retail Draygon cannon family.</summary>
internal static class DraygonCannonRomData
{
    /// <summary>Pre-instruction <c>$84:DB64</c>, accepting missiles and Super Missiles.</summary>
    public const ushort MissileHitPreInstruction = 0xdb64;

    /// <summary>Native collision/BTS word installed at a live cannon origin.</summary>
    public const ushort CannonCollisionWord = 0xc044;

    /// <summary>Native vertical extension installed immediately below a live cannon.</summary>
    public const ushort CannonExtensionWord = 0xd0ff;

    /// <summary>Native special-solid word written to both cannon cells after destruction.</summary>
    public const ushort DestroyedCannonWord = 0xa003;

    /// <summary>
    /// Value written to <c>PLM_RoomArguments</c> for a Super Missile. Incrementing its low
    /// byte in instruction <c>$8A91</c> immediately clears the three-hit threshold.
    /// </summary>
    public const ushort SuperMissileHitCounterSeed = 0x0077;
}

/// <summary>Horizontal orientation selected by one of the three retail cannon headers.</summary>
public enum DraygonCannonOrientation : byte
{
    Left,
    Right,
}
