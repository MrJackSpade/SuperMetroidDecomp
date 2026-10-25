using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification's native view of Draygon's identities (#1413): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DraygonNativeWords
{
    /// <summary>The bank-$A5 routine word naming each <see cref="DraygonEyeFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DraygonEyeFunction> EyeFunctions = new(new Dictionary<DraygonEyeFunction, ushort>
    {
        [DraygonEyeFunction.Inert] = 0x804b,
        [DraygonEyeFunction.FacingLeft] = 0xc48d,
        [DraygonEyeFunction.FacingRight] = 0xc513,
    });

    /// <summary>Pre-instruction <c>$84:DB64</c>, accepting missiles and Super Missiles.</summary>
    public const ushort MissileHitPreInstruction = 0xdb64;
}
