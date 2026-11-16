using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Verification's native view of the Kraid identities (#1413): the cartridge words naming each member. Core's values are
/// arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KraidNativeWords
{
    /// <summary>
    /// The bank-$A7 routine word naming each <see cref="KraidPhase"/>, a part's record word A, and for the foot's walk the
    /// way it walks (#2165), the cartridge giving each way a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(KraidPhase Phase, HorizontalDirection? Walk)> Phases = new(new Dictionary<(KraidPhase Phase, HorizontalDirection? Walk), ushort>
    {
        // A resting part's record word A names the bank's bare return (#1870).
        [(KraidPhase.Resting, null)] = 0xba2d,
        [(KraidPhase.GrowBreakCeilingPlatforms, null)] = 0xac4d,
        [(KraidPhase.GrowBringBodyInFront, null)] = 0xad3a,
        [(KraidPhase.GrowSettleParts, null)] = 0xad61,
        [(KraidPhase.GrowShowRoomBackdrop, null)] = 0xad8e,
        [(KraidPhase.GrowFadeInRoomBackground, null)] = 0xae23,
        [(KraidPhase.MainloopThinking, null)] = 0xaea4,
        [(KraidPhase.SecondPhaseThinking, null)] = 0xaec4,
        [(KraidPhase.MouthOpenReaction, null)] = 0xaee4,
        [(KraidPhase.InitializeEyeGlow, null)] = 0xb6bf,
        [(KraidPhase.GlowEye, null)] = 0xb6d7,
        [(KraidPhase.UnglowEye, null)] = 0xb73d,
        [(KraidPhase.LintInactive, null)] = 0xb831,
        [(KraidPhase.LintProduce, null)] = 0xb832,
        [(KraidPhase.LintCharge, null)] = 0xb868,
        [(KraidPhase.LintFire, null)] = 0xb89b,
        [(KraidPhase.FingernailWaitForLint, null)] = 0xb907,
        [(KraidPhase.AlignPartToKraid, null)] = 0xb923,
        [(KraidPhase.WaitToResumePhase, null)] = 0xb92d,
        [(KraidPhase.FootWaitBeforeWalk, null)] = 0xb93f,
        [(KraidPhase.FootFirstPhaseThinking, null)] = 0xb960,
        [(KraidPhase.AdvanceHeadAnimationAndTimer, null)] = 0xb965,
        [(KraidPhase.FootSecondPhaseThinking, null)] = 0xba2e,
        [(KraidPhase.FootSecondPhaseWalking, HorizontalDirection.Right)] = 0xbb45,
        [(KraidPhase.FootSecondPhaseWalkToStart, null)] = 0xbb6e,
        [(KraidPhase.FootSecondPhaseInitialize, null)] = 0xbba4,
        [(KraidPhase.FootSecondPhaseWalking, HorizontalDirection.Left)] = 0xbbae,
        [(KraidPhase.MainAttackWithMouthOpen, null)] = 0xbbea,
        [(KraidPhase.FingernailInitialize, null)] = 0xbd60,
        [(KraidPhase.FingernailFire, null)] = 0xbe8e,
        [(KraidPhase.FootPrepareFirstPhaseLunge, null)] = 0xbf2d,
        [(KraidPhase.FootFirstPhaseLunge, null)] = 0xbf5d,
        [(KraidPhase.FootFirstPhaseRetreat, null)] = 0xbfab,
        [(KraidPhase.GrowReleaseCamera, null)] = 0xc0a1,
        [(KraidPhase.DeathInitialize, null)] = 0xc360,
        [(KraidPhase.DeathFadeOut, null)] = 0xc3f9,
        [(KraidPhase.DeathShowBody, null)] = 0xc4a4,
        [(KraidPhase.DeathStartSinking, null)] = 0xc4c8,
        [(KraidPhase.DeathSink, null)] = 0xc537,
        [(KraidPhase.DeathHideBody, null)] = 0xc715,
        [(KraidPhase.DeathWaitBeforeFadeIn, null)] = 0xc751,
        [(KraidPhase.DeathFadeInBackground, null)] = 0xc815,
        [(KraidPhase.DeathFinishedWasAlive, null)] = 0xc843,
        [(KraidPhase.DeathFinishedWasDead, null)] = 0xc851,
        [(KraidPhase.RestrictSamusToFirstScreen, null)] = 0xc865,
        [(KraidPhase.RaiseShowBody, null)] = 0xc86b,
        [(KraidPhase.RaiseStartQuakeAndMusic, null)] = 0xc89a,
        [(KraidPhase.RaiseRocksEvery16Frames, null)] = 0xc8e0,
        [(KraidPhase.RaiseRocksEvery8Frames, null)] = 0xc902,
        [(KraidPhase.RaiseBody, null)] = 0xc924,
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
