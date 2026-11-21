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
        [DeadSidehopperAction.Landing] = 0xd90f,
        [DeadSidehopperAction.BeginPostLandingDelay] = 0xd910,
        [DeadSidehopperAction.PostLandingDelay] = 0xd91d,
        [DeadSidehopperAction.Draining] = 0xda08,
        [DeadSidehopperAction.WaitForSamusCollision] = 0xda64,
        [DeadSidehopperAction.PreRotDelay] = 0xda8f,
        [DeadSidehopperAction.Rotting] = 0xdaba,
    });

    /// <summary>The routine word naming each <see cref="CorpsePhase"/> of the dead Torizo.</summary>
    internal static readonly NativeWordIdentities<CorpsePhase> DeadTorizoPhases = new(new Dictionary<CorpsePhase, ushort>
    {
        [CorpsePhase.Waiting] = 0xd3ad,
        [CorpsePhase.PreRotDelay] = 0xd3c8,
        [CorpsePhase.Rotting] = 0xd3e6,
        [CorpsePhase.Finished] = 0xd3c7,
    });
}
