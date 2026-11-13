using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Verification's native view of the Corpses identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CorpsesNativeWords
{
    /// <summary>The routine word naming each <see cref="DeadSidehopperAction"/>.</summary>
    internal static readonly NativeWordIdentities<DeadSidehopperAction> DeadSidehopperActions = new(new Dictionary<DeadSidehopperAction, ushort>
    {
        [DeadSidehopperAction.AliveWaitForCamera] = 0xd8e2,
        [DeadSidehopperAction.ActivatedMovement] = 0xd8f1,
        [DeadSidehopperAction.NoOperation] = 0xd90f,
        [DeadSidehopperAction.BeginPostLandingDelay] = 0xd910,
        [DeadSidehopperAction.PostLandingDelay] = 0xd91d,
        [DeadSidehopperAction.Draining] = 0xda08,
        [DeadSidehopperAction.WaitForSamusCollision] = 0xda64,
        [DeadSidehopperAction.PreRotDelay] = 0xda8f,
        [DeadSidehopperAction.Rotting] = 0xdaba,
    });

    /// <summary>The routine word naming each <see cref="DeadTorizoPhase"/>.</summary>
    internal static readonly NativeWordIdentities<DeadTorizoPhase> DeadTorizoPhases = new(new Dictionary<DeadTorizoPhase, ushort>
    {
        [DeadTorizoPhase.Wait] = 0xd3ad,
        [DeadTorizoPhase.PreRot] = 0xd3c8,
        [DeadTorizoPhase.Rotting] = 0xd3e6,
        [DeadTorizoPhase.NoOperation] = 0xd3c7,
    });
}
