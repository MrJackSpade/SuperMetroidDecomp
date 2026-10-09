namespace SuperMetroid.Core.Game;

/// <summary>
/// One target in Crocomire's hidden-wall rumble schedule. Negative targets carry the
/// cooldown and approach delta loaded when their oscillation completes.
/// </summary>
/// <param name="TargetYOffset">The signed target Y offset for this waveform entry; negative values identify an oscillation minimum.</param>
/// <param name="NextTargetOffset">The byte offset of the next target word or the cursor restored after the terminator.</param>
/// <param name="Cooldown">The wait loaded after a negative target's oscillation completes.</param>
/// <param name="Delta">The per-frame approach amount used while moving from this negative target toward the next positive target.</param>
/// <param name="IsTerminator">Whether this entry represents the native end marker rather than a waveform target.</param>
internal readonly record struct CrocomireRumbleDefinition(
    short TargetYOffset,
    ushort NextTargetOffset,
    ushort Cooldown,
    ushort Delta,
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
    /// $A4:98FA: selected final repeat counter3 in the shake/sound performance.
    /// This counts completed oscillations, not elapsed frames. The consumer adds
    /// one final traversal before loading the next phase.
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
    /// two cycles, then halves. The final selected repeat count belongs to the authored shake cadence.
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

    /// <summary>$A4:98CA: exact copied prefix word4, accepted as a target by the
    /// preserved restored-cursor domain. Native entry starts at byte offset4;
    /// this is compatibility content, not a claimed normal-entry amplitude.</summary>
    private const short CopiedPrefixTarget0 = 4;
    /// <summary>$A4:98CC: second exact prefix word1. Its original normal-entry
    /// purpose is not established; preserving it does not invent a physical rule.</summary>
    private const short CopiedPrefixTarget1 = 1;

    /// <summary>
    /// $A4:98CA-9909 stores one-word nonnegative targets and three-word negative
    /// targets. After the initial zero, seven oscillations each occupy eight bytes:
    /// one positive return target followed by the negative target and next timing.
    /// </summary>
    internal static TargetSequence All => default;

    /// <summary>
    /// Presents the copied prefix, authored waveform targets, and terminal marker as an indexed read-only sequence.
    /// </summary>
    internal readonly struct TargetSequence : IReadOnlyList<CrocomireRumbleDefinition>
    {
        /// <summary>
        /// Gets the number of target entries, including the two copied prefix words and terminal marker words.
        /// </summary>
        public int Count => 4 + PhaseCount * 2;

        /// <summary>
        /// Gets the rumble definition represented by its sequential table-word index.
        /// </summary>
        /// <param name="index">The zero-based entry index within the complete target sequence.</param>
        /// <returns>The decoded target, timing operands, and terminator status for that entry.</returns>
        public CrocomireRumbleDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index < 2)
                    return new(index == 0 ? CopiedPrefixTarget0 : CopiedPrefixTarget1, (ushort)(index * 2 + 2),
                        0, 0, false);
                if (index >= Count - 2)
                    return new(unchecked((short)EndMarker), CompletedCursor,
                        0, 0, true);
                int phase = (index - 2) / 2;
                bool negative = (index & 1) != 0;
                ushort offset = (ushort)(4 + phase * 8 + (negative ? 2 : 0));
                if (!negative)
                    return new(phase == 0 ? (short)0 : (short)ApproachDelta(phase - 1),
                        (ushort)(offset + 2), 0, 0, false);
                return new(NegativeTarget(phase), (ushort)(offset + 6),
                    Cooldown(phase), ApproachDelta(phase), false);
            }
        }

        /// <summary>
        /// Enumerates the prefix, waveform, and terminal entries in their cartridge table order.
        /// </summary>
        /// <returns>An enumerator over all computed rumble definitions.</returns>
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

