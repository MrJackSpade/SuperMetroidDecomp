namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Development-tool members of <see cref="EnemyExtendedFrameSequence"/>; never linked by player hosts.</summary>
internal static class EnemyExtendedFrameSequenceTooling
{
    /// <summary>Range views of an extended-frame sequence, for the resource audit's family checks.</summary>
    extension(EnemyExtendedFrameSequence sequence)
    {
        /// <summary>The frames in <paramref name="range"/> of the sequence, without copying them.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The range does not fit the sequence.</exception>
        internal EnemyExtendedFrameSequence Slice(Range range)
        {
            int offset = range.Start.GetOffset(sequence.Length);
            int end = range.End.GetOffset(sequence.Length);
            if ((uint)end > (uint)sequence.Length || (uint)offset > (uint)end)
                throw new ArgumentOutOfRangeException(nameof(range), range, $"The range does not fit the {sequence.Length} frames.");
            return new(sequence.Start + offset, end - offset);
        }
    }
}
