using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Verification's native view of Crocomire's identities and packed words (#1413): the cartridge words naming each
/// member, how the cartridge packs the fight latches with the mouth-hit count, and where each rumble oscillation's
/// targets sit in the cartridge's rumble table. Core's values are arbitrary; a native snapshot or ROM oracle translates through
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
    /// The byte offset into the death dispatcher's jump table, body variable A, naming each stage of his course
    /// (<see cref="CrocomireDeathStageDefinitions.Stages"/>, #2540), by the stage's place in it. The cartridge advances a
    /// stage by one table word, except that its last sink runs on to the river's entry past the corpse's and the river runs
    /// back to the wait behind the wall; the live room's inert corpse, and the already-defeated room's waiting and placing of
    /// his body layer, which the port's corpse does not need (#2031), are each the inert corpse.
    /// </summary>
    internal static readonly NativeWordIdentities<ListStep<CrocomireDeathStage>> DeathStages = new(
        new ushort[][]
        {
            [0], [2], [4], [6], [8], [10], [12], [14], [16], [18], [20], [22], [24], [26], [28], [30], [32], [34], [36], [38],
            [40], [42], [44], [46], [48], [50], [52], [54], [56], [58], [60], [88], [62], [64], [66], [68], [70], [72], [74],
            [76], [78], [80], [82, 84, 86],
        }.SelectMany((words, place) => words.Select(word =>
            KeyValuePair.Create(word, ListStep<CrocomireDeathStage>.At(CrocomireDeathStageDefinitions.Stages, place))))
        .ToDictionary());

    /// <summary>
    /// The byte offset into the fight-AI jump table, body variable C, naming each <see cref="CrocomirePhase"/>. The table's
    /// other entries belong to an attack chain no retail code writes an entry offset of, so a recording holding one is
    /// rejected.
    /// </summary>
    internal static readonly NativeWordIdentities<CrocomirePhase> Phases = new(new Dictionary<CrocomirePhase, ushort>
    {
        [CrocomirePhase.Dormant] = 4,
        [CrocomirePhase.SteppingForward] = 6,
        [CrocomirePhase.ProjectileAttack] = 8,
        [CrocomirePhase.NearSpikeWallCharge] = 10,
        [CrocomirePhase.SteppingBack] = 12,
        [CrocomirePhase.BackingOffSpikeWall] = 14,
        [CrocomirePhase.WaitingForFirstDamage] = 18,
        [CrocomirePhase.WaitingForSecondDamage] = 20,
        [CrocomirePhase.PowerBombCharge] = 24,
    });

    /// <summary>The bank-$A4 frame word of each installed frame Crocomire's tongue shows.</summary>
    internal static readonly NativeWordIdentities<EnemyFrameId> TongueFrames = new(new Dictionary<EnemyFrameId, ushort>
    {
        [EnemyFrameId.CrocomireTongueCurlingDown] = 0xc65e,
        [EnemyFrameId.CrocomireTongueFlatRaised] = 0xc668,
        [EnemyFrameId.CrocomireTongueCurlingUp] = 0xc672,
        [EnemyFrameId.CrocomireTongueFlat] = 0xc67c,
        [EnemyFrameId.CrocomireArmAloneBent] = 0xcace,
        [EnemyFrameId.CrocomireArmAloneRaised] = 0xcad8,
        [EnemyFrameId.CrocomireArmAloneOutstretched] = 0xcae2,
        [EnemyFrameId.CrocomireArmAloneLowering] = 0xcaec,
        [EnemyFrameId.CrocomireArmAloneReturning] = 0xcaf6,
    });

    /// <summary>Whether a recorded body variable B holds the damage-reaction latch.</summary>
    internal static bool DamageReactionPending(ushort word) => (word & DamageReactionPendingBit) != 0;


    /// <summary>
    /// Imports the body's values the cartridge keeps in record words D, E and F (#1476): during the fight the queued
    /// steps and the projectile counter (E's reaction timer is read by nothing); while the wall rumbles the rumble's place; otherwise in the
    /// death sequence the death phase's wait or the vertical acceleration in D, the vertical speed or the skeleton speed's
    /// fraction in E, and the carried vertical fraction (the high byte) or the skeleton speed's whole pixels in F. Of the values
    /// one death-phase word holds, only the phase's own is read, so each is imported from the word. The cartridge counts the
    /// wait down while it is above zero and moves on at zero, so the word is the wait's count (#2463).
    /// </summary>
    internal static void ImportSharedWords(CrocomireEnemyState state, ushort wordD, ushort wordE, ushort wordF)
    {
        if (state.DeathStage.Entry is CrocomireDeathStage.Fighting)
        {
            state.StepCounter = wordD;
            state.ShotsSpat = wordF % 2 == 0 ? wordF / 2
                : throw new InvalidDataException($"Crocomire's projectile counter {wordF} counts its shots by two and is never odd.");
            return;
        }

        if (state.DeathStage.Entry is CrocomireDeathStage.RumbleHiddenWall)
        {
            (state.RumbleOscillation, state.RumbleTowardExtreme) = RumbleAt(wordD);
        }
        else
        {
            state.DeathStageWait.SetCartridgeCount((int)wordD);
            state.VerticalAcceleration = SignedSixteenSixteen.FromParts(0, wordD);
        }
        state.VerticalSpeed = new SignedEightEight(wordE);
        state.VerticalFraction = SignedEightEight.FromParts(0, LittleEndianBytes.HighByte(wordF));
        state.SkeletonVelocity = SignedSixteenSixteen.FromParts(unchecked((short)wordF), wordE);
    }

    /// <summary>The rumble table's byte cursor of the first oscillation's return target: two copied prefix words precede it.</summary>
    private const int RumbleFirstCursor = 4;

    /// <summary>
    /// The bytes one oscillation occupies in the cartridge's flat table: its return target, its extreme, then the next
    /// oscillation's repeats and speed, whose sign-tested extreme told the table's reader they follow.
    /// </summary>
    private const int RumbleOscillationBytes = 8;

    /// <summary>
    /// The rumble table's byte cursor of <paramref name="oscillation"/>'s return target, or its extreme when
    /// <paramref name="towardExtreme"/>; the oscillation past the last is the cursor of the table's end marker.
    /// </summary>
    internal static int RumbleCursor(int oscillation, bool towardExtreme) =>
        RumbleFirstCursor + oscillation * RumbleOscillationBytes + (towardExtreme ? sizeof(ushort) : 0);

    /// <summary>The oscillation and heading a recorded byte cursor names.</summary>
    /// <exception cref="InvalidDataException"><paramref name="cursor"/> names no oscillation's target.</exception>
    internal static (int Oscillation, bool TowardExtreme) RumbleAt(int cursor)
    {
        for (int oscillation = 0; oscillation <= CrocomireRumbleDefinitions.Oscillations.Length; oscillation++)
            foreach (bool towardExtreme in new[] { false, true })
                if (RumbleCursor(oscillation, towardExtreme) == cursor &&
                    (oscillation < CrocomireRumbleDefinitions.Oscillations.Length || !towardExtreme))
                    return (oscillation, towardExtreme);
        throw new InvalidDataException($"Crocomire rumble cursor ${cursor:X4} names no rumble target.");
    }
}
