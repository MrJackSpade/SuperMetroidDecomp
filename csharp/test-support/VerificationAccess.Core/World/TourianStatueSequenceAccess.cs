namespace SuperMetroid.Core.World;

/// <summary>Verification access to a <see cref="TourianStatueSequence"/>'s statue animations, which production keeps private (#2290).</summary>
internal static class TourianStatueSequenceAccess
{
    /// <summary>Reads the sequence's statue animations.</summary>
    extension(TourianStatueSequence self)
    {
        /// <summary>The four statue animations in Kraid, Phantoon, Draygon, Ridley order, the order they run in.</summary>
        internal IReadOnlyList<TourianStatueSequence.StatueBlockAnimation> Statues =>
            self.Statues;
    }
}
