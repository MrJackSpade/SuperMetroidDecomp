using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Verification's native view of the Kraid identities (#1413): the cartridge words naming each member. Core's values are
/// arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KraidNativeWords
{
    /// <summary>The bank-$A7 bare return a resting part's record word A names (#1870): the arm's, and the foot's at rest.</summary>
    internal const ushort RestingWord = 0xba2d;

    /// <summary>The bank-$A7 routine word naming each <see cref="KraidRisePhase"/>, the body's record word A (#2208).</summary>
    internal static readonly NativeWordIdentities<KraidRisePhase> RisePhases = new(new Dictionary<KraidRisePhase, ushort>
    {
        [KraidRisePhase.RestrictSamusToFirstScreen] = 0xc865,
        [KraidRisePhase.ShowBody] = 0xc86b,
        [KraidRisePhase.StartQuakeAndMusic] = 0xc89a,
        [KraidRisePhase.SlowRocks] = 0xc8e0,
        [KraidRisePhase.FastRocks] = 0xc902,
        [KraidRisePhase.RaiseBody] = 0xc924,
    });

    /// <summary>
    /// The bank-$A7 routine word naming each <see cref="KraidFightPhase"/>, the body's record word A (#2208); the wait
    /// before the eye glows is the bank's shared timed wait, whose continuation the phase names.
    /// </summary>
    internal static readonly NativeWordIdentities<KraidFightPhase> FightPhases = new(new Dictionary<KraidFightPhase, ushort>
    {
        [KraidFightPhase.FirstPhaseThinking] = 0xaea4,
        [KraidFightPhase.SecondPhaseThinking] = 0xaec4,
        [KraidFightPhase.MouthOpenReaction] = 0xaee4,
        [KraidFightPhase.InitializeEyeGlow] = 0xb6bf,
        [KraidFightPhase.GlowEye] = 0xb6d7,
        [KraidFightPhase.UnglowEye] = 0xb73d,
        [KraidFightPhase.WaitBeforeEyeGlow] = 0xb92d,
        [KraidFightPhase.HeadAnimationBeforeGrowth] = 0xb965,
        [KraidFightPhase.MainAttackWithMouthOpen] = 0xbbea,
    });

    /// <summary>The bank-$A7 routine word naming each <see cref="KraidGrowthPhase"/>, the body's record word A (#2208).</summary>
    internal static readonly NativeWordIdentities<KraidGrowthPhase> GrowthPhases = new(new Dictionary<KraidGrowthPhase, ushort>
    {
        [KraidGrowthPhase.BreakCeilingPlatforms] = 0xac4d,
        [KraidGrowthPhase.BringBodyInFront] = 0xad3a,
        [KraidGrowthPhase.SettleParts] = 0xad61,
        [KraidGrowthPhase.ShowRoomBackdrop] = 0xad8e,
        [KraidGrowthPhase.FadeInRoomBackground] = 0xae23,
        [KraidGrowthPhase.ReleaseCamera] = 0xc0a1,
    });

    /// <summary>
    /// The bank-$A7 routine words naming each <see cref="KraidDeathPhase"/>, the body's record word A (#2208); the
    /// cartridge has two routines doing the finished phase, one for a Kraid it found alive and one for a Kraid it found
    /// already defeated (#2464).
    /// </summary>
    internal static readonly NativeWordIdentities<KraidDeathPhase> DeathPhases = new(new Dictionary<ushort, KraidDeathPhase>
    {
        [0xc360] = KraidDeathPhase.Initialize,
        [0xc3f9] = KraidDeathPhase.FadeOut,
        [0xc4a4] = KraidDeathPhase.ShowBody,
        [0xc4c8] = KraidDeathPhase.StartSinking,
        [0xc537] = KraidDeathPhase.Sink,
    });

    /// <summary>
    /// The bank-$A7 routine words naming each <see cref="KraidSunkPhase"/>, the body's record word A once he has sunk
    /// (#2554); the cartridge has two routines doing the finished phase, one for a Kraid it found alive and one for a Kraid
    /// it found already defeated (#2464).
    /// </summary>
    internal static readonly NativeWordIdentities<KraidSunkPhase> SunkPhases = new(new Dictionary<ushort, KraidSunkPhase>
    {
        [0xc715] = KraidSunkPhase.HideBody,
        [0xc751] = KraidSunkPhase.WaitBeforeFadeIn,
        [0xc815] = KraidSunkPhase.FadeInBackground,
        [0xc843] = KraidSunkPhase.Finished,
        [0xc851] = KraidSunkPhase.Finished,
    });

    /// <summary>
    /// The bank-$A7 routine word naming each <see cref="KraidLintPhase"/>, a lint's record word A (#2208); the alignment
    /// before production is the bank's shared timed alignment, whose continuation the phase names.
    /// </summary>
    internal static readonly NativeWordIdentities<KraidLintPhase> LintPhases = new(new Dictionary<KraidLintPhase, ushort>
    {
        [KraidLintPhase.Inactive] = 0xb831,
        [KraidLintPhase.Produce] = 0xb832,
        [KraidLintPhase.Charge] = 0xb868,
        [KraidLintPhase.Fire] = 0xb89b,
        [KraidLintPhase.AlignToKraidBeforeProducing] = 0xb923,
    });

    /// <summary>
    /// The bank-$A7 routine word naming each <see cref="KraidFootPhase"/>, the foot's record word A (#2208), and for its
    /// walk the way it walks (#2165), the cartridge giving each way a routine of its own; the wait at the second-phase
    /// start is the bank's shared timed wait, whose continuation the phase names.
    /// </summary>
    internal static readonly NativeWordIdentities<(KraidFootPhase Phase, HorizontalDirection? Walk)> FootPhases = new(new Dictionary<(KraidFootPhase Phase, HorizontalDirection? Walk), ushort>
    {
        // A resting foot's record word A names the bank's bare return (#1870).
        [(KraidFootPhase.Resting, null)] = RestingWord,
        [(KraidFootPhase.WaitBeforeSecondPhase, null)] = 0xb92d,
        [(KraidFootPhase.WaitBeforeRetreat, null)] = 0xb93f,
        [(KraidFootPhase.FirstPhaseThinking, null)] = 0xb960,
        [(KraidFootPhase.SecondPhaseThinking, null)] = 0xba2e,
        [(KraidFootPhase.SecondPhaseWalking, HorizontalDirection.Right)] = 0xbb45,
        [(KraidFootPhase.SecondPhaseWalkToStart, null)] = 0xbb6e,
        [(KraidFootPhase.SecondPhaseInitialize, null)] = 0xbba4,
        [(KraidFootPhase.SecondPhaseWalking, HorizontalDirection.Left)] = 0xbbae,
        [(KraidFootPhase.PrepareFirstPhaseLunge, null)] = 0xbf2d,
        [(KraidFootPhase.FirstPhaseLunge, null)] = 0xbf5d,
        [(KraidFootPhase.FirstPhaseRetreat, null)] = 0xbfab,
    });

    /// <summary>
    /// The bank-$A7 routine word naming each <see cref="KraidNailPhase"/>, a fingernail's record word A (#2208); the wait
    /// before its launch is the bank's shared timed wait, whose continuation the phase names.
    /// </summary>
    internal static readonly NativeWordIdentities<KraidNailPhase> NailPhases = new(new Dictionary<KraidNailPhase, ushort>
    {
        [KraidNailPhase.WaitForLint] = 0xb907,
        [KraidNailPhase.WaitBeforeLaunch] = 0xb92d,
        [KraidNailPhase.Initialize] = 0xbd60,
        [KraidNailPhase.Fire] = 0xbe8e,
    });

    /// <summary>Installs the body phase <paramref name="word"/> names, entering the stage whose phase it is.</summary>
    /// <exception cref="InvalidDataException">The word names no phase of the body's.</exception>
    internal static void ImportBodyPhase(KraidBodyState body, ushort word)
    {
        if (RisePhases.TryDecode(word, out KraidRisePhase rise))
            body.RisePhase = rise;
        else if (FightPhases.TryDecode(word, out KraidFightPhase fight))
            body.FightPhase = fight;
        else if (GrowthPhases.TryDecode(word, out KraidGrowthPhase growth))
            body.GrowthPhase = growth;
        else if (DeathPhases.TryDecode(word, out KraidDeathPhase death))
            body.DeathPhase = death;
        else
            body.SunkPhase = SunkPhases.Decode(word, "recorded Kraid body phase");
    }

    /// <summary>
    /// The bank-$A7 routine words of the death's five one-update steps between hiding the body and the fade-in, each with
    /// the updates the wait then has left (#1888): Core runs them as one <see cref="KraidSunkPhase.WaitBeforeFadeIn"/>
    /// counted down in the body's phase timer, which <see cref="SunkPhases"/> names by the first.
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
