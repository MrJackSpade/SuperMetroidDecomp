using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Verification's native view of Crocomire's identities and packed words (#1413): the cartridge words naming each
/// member, how the cartridge packs the fight latches with the mouth-hit count, and where each rumble step's target sits
/// in the cartridge's rumble table. Core's values are arbitrary; a native snapshot or ROM oracle translates through
/// these tables.
/// </summary>
internal static class CrocomireNativeWords
{
    /// <summary>
    /// The bit of body variable B holding the damage-reaction latch. Its top bit, which waking and a Power Bomb set, is read by
    /// nothing (#1686), so it is no latch of the game.
    /// </summary>
    private const int DamageReactionPendingBit = 0x0800;

    /// <summary>
    /// The byte offset into the death dispatcher's jump table, body variable A, naming each <see cref="CrocomireDeathPhase"/>.
    /// The cartridge advances a phase by one table word, the successor <see cref="CrocomireDeathPhases.Next"/> names.
    /// </summary>
    internal static readonly NativeWordIdentities<CrocomireDeathPhase> DeathPhases = new(new Dictionary<CrocomireDeathPhase, ushort>
    {
        [CrocomireDeathPhase.Fighting] = 0,
        [CrocomireDeathPhase.CrumbleBridgeAndSink] = 2,
        [CrocomireDeathPhase.FirstSubmergedPause] = 4,
        [CrocomireDeathPhase.FirstHopRise] = 6,
        [CrocomireDeathPhase.FirstHopSink] = 8,
        [CrocomireDeathPhase.SecondSubmergedPause] = 10,
        [CrocomireDeathPhase.SecondHopRise] = 12,
        [CrocomireDeathPhase.SecondHopSink] = 14,
        [CrocomireDeathPhase.InstallFirstMeltImage] = 16,
        [CrocomireDeathPhase.BeginFirstMeltPass] = 18,
        [CrocomireDeathPhase.ShowFirstMeltBands] = 20,
        [CrocomireDeathPhase.ThirdHopRise] = 22,
        [CrocomireDeathPhase.StartFirstDissolve] = 24,
        [CrocomireDeathPhase.DissolveFirstImage] = 26,
        [CrocomireDeathPhase.ClearFirstMeltImage] = 28,
        [CrocomireDeathPhase.FourthHopSink] = 30,
        [CrocomireDeathPhase.ThirdSubmergedPause] = 32,
        [CrocomireDeathPhase.FourthHopRise] = 34,
        [CrocomireDeathPhase.FifthHopSink] = 36,
        [CrocomireDeathPhase.FourthSubmergedPause] = 38,
        [CrocomireDeathPhase.FifthHopRise] = 40,
        [CrocomireDeathPhase.SixthHopSink] = 42,
        [CrocomireDeathPhase.InstallSecondMeltImage] = 44,
        [CrocomireDeathPhase.BeginSecondMeltPass] = 46,
        [CrocomireDeathPhase.ShowSecondMeltBands] = 48,
        [CrocomireDeathPhase.WaitBeforeSixthHopRise] = 50,
        [CrocomireDeathPhase.SixthHopRise] = 52,
        [CrocomireDeathPhase.StartSecondDissolve] = 54,
        [CrocomireDeathPhase.DissolveSecondImage] = 56,
        [CrocomireDeathPhase.ClearSecondMeltImage] = 58,
        [CrocomireDeathPhase.FinalSink] = 60,
        [CrocomireDeathPhase.WaitForSamusAtWall] = 62,
        [CrocomireDeathPhase.RumbleHiddenWall] = 64,
        [CrocomireDeathPhase.BreakSpikeWall] = 66,
        [CrocomireDeathPhase.DelaySkeletonFall] = 68,
        [CrocomireDeathPhase.ArcSkeletonIntoArena] = 70,
        [CrocomireDeathPhase.WaitForSkeletonTerminalImage] = 72,
        [CrocomireDeathPhase.ClearWallAndOpenScrolls] = 74,
        [CrocomireDeathPhase.WaitForStableSkeleton] = 76,
        [CrocomireDeathPhase.WaitBeforeRecordingDefeat] = 78,
        [CrocomireDeathPhase.PublishDefeatAndRestoreMusic] = 80,
        [CrocomireDeathPhase.InertCorpse] = 82,
        [CrocomireDeathPhase.WaitBeforePinningDefeatedBodyLayer] = 84,
        [CrocomireDeathPhase.PinDefeatedRoomBodyLayerPlacement] = 86,
        [CrocomireDeathPhase.RiverSkeletonDetour] = 88,
    });

    /// <summary>
    /// The byte offset into the fight-AI jump table, body variable C, naming each <see cref="CrocomirePhase"/>. The table's
    /// other entries belong to an attack chain no retail code writes an entry offset of, so a recording holding one is
    /// rejected.
    /// </summary>
    internal static readonly NativeWordIdentities<CrocomirePhase> Phases = new(new Dictionary<CrocomirePhase, ushort>
    {
        [CrocomirePhase.ResetAnimation] = 0,
        [CrocomirePhase.Sleeping] = 4,
        [CrocomirePhase.SteppingForward] = 6,
        [CrocomirePhase.ProjectileAttack] = 8,
        [CrocomirePhase.NearSpikeWallCharge] = 10,
        [CrocomirePhase.SteppingBack] = 12,
        [CrocomirePhase.BackingOffSpikeWall] = 14,
        [CrocomirePhase.WaitingForFirstDamage] = 18,
        [CrocomirePhase.WaitingForSecondDamage] = 20,
        [CrocomirePhase.PowerBombCharge] = 24,
    });

    /// <summary>The bank-$A4 frame word naming each <see cref="CrocomireTongueFrame"/>.</summary>
    internal static readonly NativeWordIdentities<CrocomireTongueFrame> TongueFrames = new(new Dictionary<CrocomireTongueFrame, ushort>
    {
        [CrocomireTongueFrame.Fight0] = 0xc65e,
        [CrocomireTongueFrame.Fight1] = 0xc668,
        [CrocomireTongueFrame.Fight2] = 0xc672,
        [CrocomireTongueFrame.Fight3] = 0xc67c,
        [CrocomireTongueFrame.MeltingPose0] = 0xcace,
        [CrocomireTongueFrame.MeltingPose1] = 0xcad8,
        [CrocomireTongueFrame.MeltingPose2] = 0xcae2,
        [CrocomireTongueFrame.MeltingPose3] = 0xcaec,
        [CrocomireTongueFrame.MeltingPose4] = 0xcaf6,
    });

    /// <summary>Whether a recorded body variable B holds the damage-reaction latch.</summary>
    internal static bool DamageReactionPending(ushort word) => (word & DamageReactionPendingBit) != 0;


    /// <summary>
    /// Imports the body's values the cartridge keeps in record words D, E and F (#1476): during the fight the queued
    /// steps and the projectile counter (E's reaction timer is read by nothing); while the wall rumbles the rumble step; otherwise in the
    /// death sequence the death-phase delay or the vertical acceleration in D, the vertical speed or the skeleton speed's
    /// fraction in E, and the vertical subpixel (the high byte) or the skeleton speed's whole pixels in F. Of the values
    /// one death-phase word holds, only the phase's own is read, so each is imported from the word.
    /// </summary>
    internal static void ImportSharedWords(CrocomireEnemyState state, ushort wordD, ushort wordE, ushort wordF)
    {
        if (state.DeathPhase == CrocomireDeathPhase.Fighting)
        {
            state.StepCounter = wordD;
            state.ShotsSpat = wordF % 2 == 0 ? wordF / 2
                : throw new InvalidDataException($"Crocomire's projectile counter {wordF} counts its shots by two and is never odd.");
            return;
        }

        if (state.DeathPhase == CrocomireDeathPhase.RumbleHiddenWall)
        {
            state.RumbleStep = RumbleStep(wordD);
        }
        else
        {
            state.DeathPhaseDelay = wordD;
            state.VerticalAcceleration = new SignedEightEight(wordD);
        }
        state.VerticalSpeed = new SignedEightEight(wordE);
        state.VerticalSubpixel = LittleEndianBytes.HighByte(wordF);
        state.SkeletonVelocity = SignedSixteenSixteen.FromParts(unchecked((short)wordF), wordE);
    }

    /// <summary>The rumble table's byte cursor of the first step's target: two copied prefix words precede it.</summary>
    private const int RumbleFirstCursor = 4;

    /// <summary>The bytes one oscillation occupies: a return target, then a negative target and its two timing words.</summary>
    private const int RumbleOscillationBytes = 8;

    /// <summary>
    /// The rumble table's byte cursor of <paramref name="step"/>'s target; the step past the last target is the
    /// cursor of the table's end marker.
    /// </summary>
    internal static int RumbleCursor(int step) =>
        RumbleFirstCursor + step / 2 * RumbleOscillationBytes + step % 2 * sizeof(ushort);

    /// <summary>The rumble step whose target a recorded byte cursor names.</summary>
    /// <exception cref="InvalidDataException"><paramref name="cursor"/> names no step's target.</exception>
    internal static int RumbleStep(int cursor)
    {
        for (int step = 0; step <= CrocomireRumbleDefinitions.All.Length; step++)
            if (RumbleCursor(step) == cursor)
                return step;
        throw new InvalidDataException($"Crocomire rumble cursor ${cursor:X4} names no rumble target.");
    }
}
