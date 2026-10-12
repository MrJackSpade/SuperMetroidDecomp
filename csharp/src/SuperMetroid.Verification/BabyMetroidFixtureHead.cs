using SuperMetroid.Core.Game;

/// <summary>Represents baby metroid fixture head verification.</summary>
internal static class BabyMetroidFixtureHeadVerification
{
    /// <summary>
    /// Standalone Baby fixtures have no head enemy; their sequence's articulated brain
    /// stands in for <c>Enemy[1]</c> after they step its neck.
    /// </summary>
    internal static Position16<RoomPixels> HeadOf(MotherBrainRainbowBeamAttackSequence sequence) =>
        sequence.BrainPosition;
}
