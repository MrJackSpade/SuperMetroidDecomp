using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Substitutes the articulated brain position for Baby Metroid fixture sequences
/// that do not include a separate head enemy slot.
/// </summary>
internal static class BabyMetroidFixtureHeadVerification
{
    /// <summary>
    /// Standalone Baby fixtures have no head enemy; their sequence's articulated brain
    /// stands in for <c>Enemy[1]</c> after they step its neck.
    /// </summary>
    internal static Position16<RoomPixels> HeadOf(MotherBrainRainbowBeamAttackSequence sequence) =>
        sequence.State.BrainPosition;
}
