using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Standalone Baby fixtures have no head enemy; their sequence's articulated brain
    /// stands in for <c>Enemy[1]</c> after they step its neck.
    /// </summary>
    private static MotherBrainHeadPosition HeadOf(MotherBrainRainbowBeamAttackSequence sequence) =>
        new(sequence.BrainXPosition, sequence.BrainYPosition);
}
