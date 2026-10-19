namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values and helpers of <see cref="DraygonCannonRomData"/> that only verification reads.</summary>
internal static class DraygonCannonRomDataConstants
{
    /// <summary>Pre-instruction <c>$84:DB64</c>, accepting missiles and Super Missiles.</summary>
    public const ushort MissileHitPreInstruction = 0xdb64;
}
