using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Verification's native view of the Torizo identities (#1413): the bank-$AA routine words naming each member. Core's
/// values are arbitrary; a ROM oracle translates through these tables.
/// </summary>
internal static class TorizoNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="TorizoPhase"/>. Bomb Torizo's and Golden Torizo's falling routines do the
    /// same thing, so both name <see cref="TorizoPhase.Falling"/> (#1931).
    /// </summary>
    internal static readonly NativeWordIdentities<TorizoPhase> Phases = new(new Dictionary<ushort, TorizoPhase>
    {
        [0xc6ab] = TorizoPhase.Idle,
        [0xc6bf] = TorizoPhase.Falling,
        [0xc6c6] = TorizoPhase.WaitForHandTrigger,
        [0xc6ff] = TorizoPhase.Active,
        [0xd5c2] = TorizoPhase.GoldenWaitForSamus,
        [0xd5df] = TorizoPhase.Falling,
        [0xd5e6] = TorizoPhase.GoldenActive,
    });

    /// <summary>
    /// The body-motion signal each routine word an animation installs before its frames stands for (#1931): the empty
    /// routine stops the body, and Bomb Torizo's and Golden Torizo's walking and standing routines start the same motions.
    /// </summary>
    internal static readonly NativeWordIdentities<TorizoCueKind> MotionCues = new(new Dictionary<ushort, TorizoCueKind>
    {
        [0xc95e] = TorizoCueKind.StopMoving,
        [0xc752] = TorizoCueKind.StartWalking,
        [0xd5f1] = TorizoCueKind.StartWalking,
        [0xc828] = TorizoCueKind.StartStanding,
        [0xd5ed] = TorizoCueKind.StartStanding,
        [0xc82c] = TorizoCueKind.StartLeaping,
    });
}
