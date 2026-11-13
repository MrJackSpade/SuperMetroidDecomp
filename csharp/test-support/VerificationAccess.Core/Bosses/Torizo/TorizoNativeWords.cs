using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Verification's native view of the Torizo identities (#1413): the bank-$AA routine words naming each member. Core's
/// values are arbitrary; a ROM oracle translates through these tables.
/// </summary>
internal static class TorizoNativeWords
{
    /// <summary>The routine word naming each <see cref="TorizoPhase"/>.</summary>
    internal static readonly NativeWordIdentities<TorizoPhase> Phases = new(new Dictionary<TorizoPhase, ushort>
    {
        [TorizoPhase.Idle] = 0xc6ab,
        [TorizoPhase.Falling] = 0xc6bf,
        [TorizoPhase.WaitForHandTrigger] = 0xc6c6,
        [TorizoPhase.Active] = 0xc6ff,
        [TorizoPhase.GoldenWaitForSamus] = 0xd5c2,
        [TorizoPhase.GoldenGravity] = 0xd5df,
        [TorizoPhase.GoldenFrameBehavior] = 0xd5e6,
    });

    /// <summary>The routine word naming each <see cref="TorizoFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<TorizoFrameBehavior> FrameBehaviors = new(new Dictionary<TorizoFrameBehavior, ushort>
    {
        [TorizoFrameBehavior.Idle] = 0xc95e,
        [TorizoFrameBehavior.AirTransition] = 0xc752,
        [TorizoFrameBehavior.Gravity] = 0xc828,
        [TorizoFrameBehavior.Jump] = 0xc82c,
        [TorizoFrameBehavior.GoldenAirTransition] = 0xd5f1,
        [TorizoFrameBehavior.GoldenGravity] = 0xd5ed,
    });
}
