using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Verification's native view of Botwoon's identities (#1871): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot translates through these tables.
/// </summary>
internal static class BotwoonNativeWords
{
    /// <summary>The bank-$B3 routine word naming each <see cref="BotwoonPhase"/>: the head's record word D.</summary>
    internal static readonly NativeWordIdentities<BotwoonPhase> Phases = new(new Dictionary<BotwoonPhase, ushort>
    {
        [BotwoonPhase.InitialDelay] = 0x9878,
        [BotwoonPhase.ChooseNextAction] = 0x989d,
        [BotwoonPhase.FollowAuthoredPath] = 0x99a4,
        [BotwoonPhase.SpitWhileHidden] = 0x99e4,
        [BotwoonPhase.DeathDelay] = 0x9a46,
        [BotwoonPhase.HeadFalling] = 0x9a5e,
        [BotwoonPhase.WaitForBody] = 0x9aca,
        [BotwoonPhase.WallExplosions] = 0x9af9,
    });

    /// <summary>
    /// The bank-$B3 routine word naming each <see cref="BotwoonMovement"/>: the head's record word E. The cartridge's
    /// separate path-loading routine ($B3:E250) has no member: the port loads a walk as it draws it (#1868).
    /// </summary>
    internal static readonly NativeWordIdentities<BotwoonMovement> Movements = new(new Dictionary<BotwoonMovement, ushort>
    {
        [BotwoonMovement.MoveTowardHole] = 0x9bb7,
        [BotwoonMovement.FollowAuthoredPath] = 0xe28c,
    });

    /// <summary>The bank-$B3 routine word naming each <see cref="BotwoonHeadAction"/>: the head's record word F.</summary>
    internal static readonly NativeWordIdentities<BotwoonHeadAction> HeadActions = new(new Dictionary<BotwoonHeadAction, ushort>
    {
        [BotwoonHeadAction.AnimateFromMovement] = 0x9dc0,
        [BotwoonHeadAction.AimAtSamus] = 0x9e7d,
        [BotwoonHeadAction.WaitForSpitFrame] = 0x9ee0,
        [BotwoonHeadAction.SpawnImmediateVolley] = 0x9f34,
        [BotwoonHeadAction.AttackCooldown] = 0x9f7a,
    });
}
