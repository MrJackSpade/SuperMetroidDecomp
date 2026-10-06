namespace SuperMetroid.Core.Game;

/// <summary>
/// One target in Crocomire's hidden-wall rumble schedule. Negative targets carry the
/// cooldown and approach delta loaded when their oscillation completes.
/// </summary>
internal readonly record struct CrocomireRumbleDefinition(
    ushort TableOffset,
    short TargetYOffset,
    ushort NextTargetOffset,
    ushort Cooldown,
    ushort Delta,
    bool HasTiming,
    bool IsTerminator);

/// <summary>Fixed hidden-wall rumble schedule from <c>$A4:98CA-$A4:9909</c>.</summary>
internal static class CrocomireRumbleDefinitions
{
    /// <summary>$A4:9902-9909: sentinel words ending the hidden-wall waveform.</summary>
    private const ushort EndMarker = 0x8080;
    /// <summary>$A4:9867 installs this completed rumble cursor at the terminator.</summary>
    private const ushort CompletedCursor = 0x0080;

    /// <summary>$A4:98D0-9900: seven negative targets in the native waveform.</summary>
    private const int PhaseCount = 7;

    /// <summary>
    /// $A4:98FA: final live cooldown is three frames, differing from the halving
    /// envelope's four. Its independent choice remains unresolved under issue1165.
    /// </summary>
    private const ushort FinalDecayHold = 3;

    /// <summary>
    /// $A4:98D4-98FC: six live approach deltas rise from one to two for the middle
    /// pair, then return to one; the seventh phase carries the terminal marker.
    /// </summary>
    private static ushort ApproachDelta(int phase) => phase == PhaseCount - 1
        ? EndMarker : (ushort)(1 + Math.Min(phase, 5 - phase) / 2);

    /// <summary>
    /// $A4:98D2-98FA: cooldown rises by four from eight, saturates at sixteen for
    /// two cycles, then halves. The final live hold remains an explicit residual.
    /// </summary>
    private static ushort Cooldown(int phase) => phase switch
    {
        PhaseCount - 1 => EndMarker,
        PhaseCount - 2 => FinalDecayHold,
        _ => (ushort)(phase <= 3 ? Math.Min(8 + 4 * phase, 16) : 16 >> (phase - 3)),
    };

    /// <summary>
    /// Negative extrema in $A4:98D0-9900 double after each two-cycle buildup,
    /// reaching amplitude4 at phase4, then halve each phase through phase6.
    /// </summary>
    private static short NegativeTarget(int phase) =>
        (short)-(1 << (phase <= 4 ? phase / 2 : 6 - phase));

    // Native initialization starts at offset4; preserve the two preceding words
    // only for the existing restored-cursor contract. Their purpose is unresolved.
    private static readonly short[] PrefixTargets = [4, 1];

    /// <summary>
    /// $A4:98CA-9909 stores one-word nonnegative targets and three-word negative
    /// targets. After the initial zero, seven oscillations each occupy eight bytes:
    /// one positive return target followed by the negative target and next timing.
    /// </summary>
    internal static TargetSequence All => default;

    internal readonly struct TargetSequence : IReadOnlyList<CrocomireRumbleDefinition>
    {
        public int Count => 4 + PhaseCount * 2;
        public CrocomireRumbleDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index < 2)
                    return new((ushort)(index * 2), PrefixTargets[index], (ushort)(index * 2 + 2),
                        0, 0, false, false);
                if (index >= Count - 2)
                    return new((ushort)(4 + PhaseCount * 8 + (index - (Count - 2)) * 2), unchecked((short)EndMarker), CompletedCursor,
                        0, 0, false, true);
                int phase = (index - 2) / 2;
                bool negative = (index & 1) != 0;
                ushort offset = (ushort)(4 + phase * 8 + (negative ? 2 : 0));
                if (!negative)
                    return new(offset, phase == 0 ? (short)0 : (short)ApproachDelta(phase - 1),
                        (ushort)(offset + 2), 0, 0, false, false);
                return new(offset, NegativeTarget(phase), (ushort)(offset + 6),
                    Cooldown(phase), ApproachDelta(phase), true, false);
            }
        }

        public IEnumerator<CrocomireRumbleDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Rejects the timing operands while resolving every native target cursor.</summary>
    internal static CrocomireRumbleDefinition AtOffset(ushort tableOffset)
    {
        if (tableOffset is 0 or 2) return All[tableOffset / 2];
        int terminatorOffset = 4 + PhaseCount * 8;
        if (tableOffset == terminatorOffset || tableOffset == terminatorOffset + 2)
            return All[All.Count - 2 + (tableOffset - terminatorOffset) / 2];
        int offset = tableOffset - 4;
        if (offset >= 0 && offset < PhaseCount * 8 && offset % 8 is 0 or 2)
            return All[2 + offset / 8 * 2 + offset % 8 / 2];
        throw new InvalidDataException("Crocomire rumble offset $" + tableOffset.ToString("X4") + " is not an authored target word.");
    }
}
