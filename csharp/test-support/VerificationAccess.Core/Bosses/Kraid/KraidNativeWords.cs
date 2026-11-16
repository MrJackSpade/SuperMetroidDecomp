using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Verification's native view of the Kraid identities (#1413): the cartridge words naming each member. Core's values are
/// arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KraidNativeWords
{
    /// <summary>The bank-$A7 routine word naming each <see cref="KraidPhase"/>: a part's record word A.</summary>
    internal static readonly NativeWordIdentities<KraidPhase> Phases = new(new Dictionary<KraidPhase, ushort>
    {
        // A resting part's record word A names the bank's bare return (#1870).
        [KraidPhase.Resting] = 0xba2d,
        [KraidPhase.GrowBreakCeilingPlatforms] = 0xac4d,
        [KraidPhase.GrowBringBodyInFront] = 0xad3a,
        [KraidPhase.GrowSettleParts] = 0xad61,
        [KraidPhase.GrowShowRoomBackdrop] = 0xad8e,
        [KraidPhase.GrowFadeInRoomBackground] = 0xae23,
        [KraidPhase.MainloopThinking] = 0xaea4,
        [KraidPhase.SecondPhaseThinking] = 0xaec4,
        [KraidPhase.MouthOpenReaction] = 0xaee4,
        [KraidPhase.InitializeEyeGlow] = 0xb6bf,
        [KraidPhase.GlowEye] = 0xb6d7,
        [KraidPhase.UnglowEye] = 0xb73d,
        [KraidPhase.LintInactive] = 0xb831,
        [KraidPhase.LintProduce] = 0xb832,
        [KraidPhase.LintCharge] = 0xb868,
        [KraidPhase.LintFire] = 0xb89b,
        [KraidPhase.FingernailWaitForLint] = 0xb907,
        [KraidPhase.AlignPartToKraid] = 0xb923,
        [KraidPhase.WaitToResumePhase] = 0xb92d,
        [KraidPhase.FootWaitBeforeWalk] = 0xb93f,
        [KraidPhase.FootFirstPhaseThinking] = 0xb960,
        [KraidPhase.AdvanceHeadAnimationAndTimer] = 0xb965,
        [KraidPhase.FootSecondPhaseThinking] = 0xba2e,
        [KraidPhase.FootSecondPhaseWalkingRight] = 0xbb45,
        [KraidPhase.FootSecondPhaseWalkToStart] = 0xbb6e,
        [KraidPhase.FootSecondPhaseInitialize] = 0xbba4,
        [KraidPhase.FootSecondPhaseWalkingLeft] = 0xbbae,
        [KraidPhase.MainAttackWithMouthOpen] = 0xbbea,
        [KraidPhase.FingernailInitialize] = 0xbd60,
        [KraidPhase.FingernailFire] = 0xbe8e,
        [KraidPhase.FootPrepareFirstPhaseLunge] = 0xbf2d,
        [KraidPhase.FootFirstPhaseLunge] = 0xbf5d,
        [KraidPhase.FootFirstPhaseRetreat] = 0xbfab,
        [KraidPhase.GrowReleaseCamera] = 0xc0a1,
        [KraidPhase.DeathInitialize] = 0xc360,
        [KraidPhase.DeathFadeOut] = 0xc3f9,
        [KraidPhase.DeathShowBody] = 0xc4a4,
        [KraidPhase.DeathStartSinking] = 0xc4c8,
        [KraidPhase.DeathSink] = 0xc537,
        [KraidPhase.DeathHideBody] = 0xc715,
        [KraidPhase.DeathWaitBeforeFadeIn] = 0xc751,
        [KraidPhase.DeathFadeInBackground] = 0xc815,
        [KraidPhase.DeathFinishedWasAlive] = 0xc843,
        [KraidPhase.DeathFinishedWasDead] = 0xc851,
        [KraidPhase.RestrictSamusToFirstScreen] = 0xc865,
        [KraidPhase.RaiseShowBody] = 0xc86b,
        [KraidPhase.RaiseStartQuakeAndMusic] = 0xc89a,
        [KraidPhase.RaiseRocksEvery16Frames] = 0xc8e0,
        [KraidPhase.RaiseRocksEvery8Frames] = 0xc902,
        [KraidPhase.RaiseBody] = 0xc924,
    });

    /// <summary>
    /// The bank-$A7 routine words of the death's five one-update steps between hiding the body and the fade-in, each with
    /// the updates the wait then has left (#1888): Core runs them as one <see cref="KraidPhase.DeathWaitBeforeFadeIn"/>
    /// counted down in the body's phase timer, which <see cref="Phases"/> names by the first.
    /// </summary>
    internal static readonly IReadOnlyDictionary<ushort, int> DeathWaitUpdatesLeft = new Dictionary<ushort, int>
    {
        [0xc751] = 5,
        [0xc777] = 4,
        [0xc7a3] = 3,
        [0xc7c9] = 2,
        [0xc7ef] = 1,
    };

    /// <summary>The bank-$A7 hitbox record address naming each <see cref="KraidMouthHitbox"/>.</summary>
    internal static readonly NativeWordIdentities<KraidMouthHitbox> MouthHitboxes = new(new Dictionary<KraidMouthHitbox, ushort>
    {
        [KraidMouthHitbox.ClosedVulnerable] = 0x9788,
        [KraidMouthHitbox.OpeningVulnerable] = 0x9790,
        [KraidMouthHitbox.WiderVulnerable] = 0x9798,
        [KraidMouthHitbox.OpenVulnerable] = 0x97a0,
        [KraidMouthHitbox.OpeningInvulnerable] = 0x97b0,
        [KraidMouthHitbox.WiderInvulnerable] = 0x97b8,
        [KraidMouthHitbox.OpenInvulnerable] = 0x97c0,
    });

    /// <summary>
    /// The bank-$84 PLM header pointer the cartridge spawns each <see cref="KraidRoomMutation"/> with. Core spawns a
    /// mutation by what it does and keeps no header for it (#1608); only the oracle reads these words.
    /// </summary>
    internal static readonly NativeWordIdentities<KraidRoomMutation> RoomMutations = new(new Dictionary<KraidRoomMutation, ushort>
    {
        [KraidRoomMutation.CrumbleCeilingAtLeftEdge] = 0xb7a3,
        [KraidRoomMutation.CrumblePlatformInOddColumn] = 0xb7a7,
        [KraidRoomMutation.CrumbleCeilingInOddColumn] = 0xb7ab,
        [KraidRoomMutation.CrumblePlatformInEvenColumn] = 0xb7af,
        [KraidRoomMutation.CrumbleCeilingInEvenColumn] = 0xb7b3,
        [KraidRoomMutation.ClearCeiling] = 0xb7b7,
        [KraidRoomMutation.ClearSpikes] = 0xb7bb,
        [KraidRoomMutation.CrumbleSpikes] = 0xb7bf,
    });
}
