namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainHandBeamBodyInstructionDefinitions"/>; never linked by player hosts.</summary>
internal static class MotherBrainHandBeamBodyInstructionDefinitionsTooling
{
    /// <summary>All fifteen visual operands; dust coordinates, durations and opcodes are excluded.</summary>
    internal static IReadOnlyList<ushort> PresentationOperands => VisualOperands;

    /// <summary>Provides indexed addresses for the beam body's interleaved spritemap operands.</summary>
    internal static readonly MotherBrainHandBeamBodyInstructionDefinitionsTooling.VisualOperandList VisualOperands = new();
    /// <summary>Number of dust records preceding the beam attack.</summary>
    public const int DustRecordCount = 8;
    /// <summary>$A9:9A44-9A4F: three entry duration/spritemap pairs after the pose callback.</summary>
    internal const int EntryFrames = 3;
    /// <summary>$A9:9AB0: duration/spritemap pair immediately before the beam-spawn callback.</summary>
    internal const ushort BeforeEmissionFrame = MotherBrainHandBeamBodyInstructionDefinitions.FirstDustRecord + DustRecordCount * MotherBrainHandBeamBodyInstructionDefinitions.DustRecordStride;
    /// <summary>$A9:9AB6-9AC1: three duration/spritemap pairs after the beam-spawn callback.</summary>
    internal const ushort AfterEmissionFrames = BeforeEmissionFrame + 3 * sizeof(ushort);
    /// <summary>Enumerates the visual operand addresses in body-program order, omitting mechanics words.</summary>
    internal sealed class VisualOperandList : IReadOnlyList<ushort>
    {
        /// <summary>Number of spritemap operands across the entry, dust, emission, and recovery frames.</summary>
        public int Count => EntryFrames + DustRecordCount + 1 + EntryFrames;

        /// <summary>Gets the instruction address of one interleaved spritemap operand.</summary>
        /// <param name="index">The zero-based visual operand slot.</param>
        /// <returns>The address of that frame's spritemap word.</returns>
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index < EntryFrames) return (ushort)(MotherBrainHandBeamBodyInstructionDefinitions.Start + 2 * sizeof(ushort) + index * 2 * sizeof(ushort));
                index -= EntryFrames;
                if (index < DustRecordCount) return (ushort)(MotherBrainHandBeamBodyInstructionDefinitions.FirstDustRecord + index * MotherBrainHandBeamBodyInstructionDefinitions.DustRecordStride + MotherBrainHandBeamBodyInstructionDefinitions.DustRecordStride - sizeof(ushort));
                index -= DustRecordCount;
                return index == 0 ? (ushort)(BeforeEmissionFrame + sizeof(ushort)) :
                    (ushort)(AfterEmissionFrames + (index - 1) * 2 * sizeof(ushort) + sizeof(ushort));
            }
        }
        /// <summary>Enumerates the fifteen visual operand addresses in instruction-list order.</summary>
        /// <returns>An enumerator over the addresses returned by the indexer.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over the beam body's visual operand addresses.</summary>
        /// <returns>An enumerator following instruction-list order.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
