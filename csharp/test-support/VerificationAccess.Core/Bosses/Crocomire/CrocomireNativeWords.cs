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
    /// <summary>The bit of body variable B holding each fight latch.</summary>
    private static readonly Dictionary<CrocomireFightFlags, int> FightFlagBits = new()
    {
        [CrocomireFightFlags.UnusedPreferClawAttack] = 0x0100,
        [CrocomireFightFlags.UnusedAttackSequenceStarted] = 0x0200,
        [CrocomireFightFlags.UnusedClawsMoving] = 0x0400,
        [CrocomireFightFlags.DamageReactionPending] = 0x0800,
        [CrocomireFightFlags.UnusedKeptByChargeReset] = 0x1000,
        [CrocomireFightFlags.UnusedForwardStepsDone] = 0x2000,
        [CrocomireFightFlags.ClawTouchedSamus] = 0x4000,
        [CrocomireFightFlags.Engaged] = 0x8000,
    };

    /// <summary>The low four bits of body variable B: the mouth-hit count.</summary>
    private const int HitCountMask = 0x000f;

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

    /// <summary>The bank-$A4 hitbox-list word naming each <see cref="CrocomireTongueHitboxList"/>.</summary>
    internal static readonly NativeWordIdentities<CrocomireTongueHitboxList> TongueHitboxLists = new(new Dictionary<CrocomireTongueHitboxList, ushort>
    {
        [CrocomireTongueHitboxList.Fight] = 0xcbb3,
        [CrocomireTongueHitboxList.Melting] = 0xcc3b,
    });

    /// <summary>The fight latches a recorded body variable B holds.</summary>
    internal static CrocomireFightFlags FightFlags(ushort word)
    {
        CrocomireFightFlags flags = 0;
        foreach ((CrocomireFightFlags flag, int bit) in FightFlagBits)
            if ((word & bit) != 0)
                flags |= flag;
        return flags;
    }

    /// <summary>The mouth-hit count a recorded body variable B holds.</summary>
    internal static int HitCount(ushort word) => word & HitCountMask;

    /// <summary>The body variable B the cartridge stores for <paramref name="flags"/> and <paramref name="hitCount"/>.</summary>
    internal static ushort FightWord(CrocomireFightFlags flags, int hitCount)
    {
        int word = hitCount;
        foreach ((CrocomireFightFlags flag, int bit) in FightFlagBits)
            if ((flags & flag) != 0)
                word |= bit;
        return checked((ushort)word);
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
