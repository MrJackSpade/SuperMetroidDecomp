using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Verification's native view of the Corpses identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CorpsesNativeWords
{
    /// <summary>The routine word naming each <see cref="DeadSidehopperAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DeadSidehopperAiFunction> DeadSidehopperAiFunctions = new(new Dictionary<DeadSidehopperAiFunction, ushort>
    {
        [DeadSidehopperAiFunction.AliveWaitForCamera] = 0xd8e2,
        [DeadSidehopperAiFunction.ActivatedMovement] = 0xd8f1,
        [DeadSidehopperAiFunction.NoOperation] = 0xd90f,
        [DeadSidehopperAiFunction.BeginPostLandingDelay] = 0xd910,
        [DeadSidehopperAiFunction.PostLandingDelay] = 0xd91d,
        [DeadSidehopperAiFunction.TransformPalette] = 0xda08,
        [DeadSidehopperAiFunction.WaitForSamusCollision] = 0xda64,
        [DeadSidehopperAiFunction.PreRotDelay] = 0xda8f,
        [DeadSidehopperAiFunction.Rotting] = 0xdaba,
    });

    /// <summary>The routine word naming each <see cref="DeadTorizoFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DeadTorizoFunction> DeadTorizoFunctions = new(new Dictionary<DeadTorizoFunction, ushort>
    {
        [DeadTorizoFunction.Wait] = 0xd3ad,
        [DeadTorizoFunction.PreRot] = 0xd3c8,
        [DeadTorizoFunction.Rotting] = 0xd3e6,
        [DeadTorizoFunction.NoOperation] = 0xd3c7,
    });
}
