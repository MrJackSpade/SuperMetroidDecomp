using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Verification's native view of the Torizo identities (#1413): the bank-$AA routine words naming each member. Core's
/// values are arbitrary; a ROM oracle translates through these tables.
/// </summary>
internal static class TorizoNativeWords
{
    /// <summary>The routine word naming each <see cref="TorizoFunction"/>.</summary>
    internal static readonly NativeWordIdentities<TorizoFunction> Functions = new(new Dictionary<TorizoFunction, ushort>
    {
        [TorizoFunction.Idle] = 0xc6ab,
        [TorizoFunction.Falling] = 0xc6bf,
        [TorizoFunction.WaitForHandTrigger] = 0xc6c6,
        [TorizoFunction.Active] = 0xc6ff,
        [TorizoFunction.GoldenWaitForSamus] = 0xd5c2,
        [TorizoFunction.GoldenGravity] = 0xd5df,
        [TorizoFunction.GoldenPreInstruction] = 0xd5e6,
    });

    /// <summary>The routine word naming each <see cref="TorizoPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<TorizoPreInstruction> PreInstructions = new(new Dictionary<TorizoPreInstruction, ushort>
    {
        [TorizoPreInstruction.Idle] = 0xc95e,
        [TorizoPreInstruction.AirTransition] = 0xc752,
        [TorizoPreInstruction.Gravity] = 0xc828,
        [TorizoPreInstruction.Jump] = 0xc82c,
        [TorizoPreInstruction.GoldenAirTransition] = 0xd5f1,
        [TorizoPreInstruction.GoldenGravity] = 0xd5ed,
    });
}
