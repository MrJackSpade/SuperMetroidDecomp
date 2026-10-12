namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainHandBeamBodyInstructionDefinitions"/>; never linked by player hosts.</summary>
internal static class MotherBrainHandBeamBodyInstructionDefinitionsTooling
{
    /// <summary>All fifteen visual operands; dust coordinates, durations and opcodes are excluded.</summary>
    internal static IReadOnlyList<ushort> PresentationOperands => VisualOperands;
    /// <summary>Stores visual operands.</summary>
    internal static readonly MotherBrainHandBeamBodyInstructionDefinitionsTooling.VisualOperandList VisualOperands = new();
    /// <summary>Number of dust records preceding the beam attack.</summary>
    public const int DustRecordCount = 8;
    /// <summary>$A9:9A44-9A4F: three entry duration/spritemap pairs after the pose callback.</summary>
    internal const int EntryFrames = 3;
    /// <summary>$A9:9AB0: duration/spritemap pair immediately before the beam-spawn callback.</summary>
    internal const ushort BeforeEmissionFrame = MotherBrainHandBeamBodyInstructionDefinitions.FirstDustRecord + DustRecordCount * MotherBrainHandBeamBodyInstructionDefinitions.DustRecordStride;
    /// <summary>$A9:9AB6-9AC1: three duration/spritemap pairs after the beam-spawn callback.</summary>
    internal const ushort AfterEmissionFrames = BeforeEmissionFrame + 3 * sizeof(ushort);
    /// <summary>Represents visual operand list.</summary>
    internal sealed class VisualOperandList : IReadOnlyList<ushort>
    {
        /// <summary>Gets the number of items in this collection.</summary>
        public int Count => EntryFrames + DustRecordCount + 1 + EntryFrames;
        /// <summary>Accesses a value by index or range.</summary>
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
        /// <summary>Returns an enumerator over this collection.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns an enumerator over this collection.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
