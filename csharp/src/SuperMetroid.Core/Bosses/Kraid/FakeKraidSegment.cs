namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations as typed programs (#641), one per list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified lists; see <see cref="FakeKraidAnimations"/>.
/// </summary>
internal enum FakeKraidSegment
{
    /// <summary>Mini Kraid animation script: choose action.</summary>
    ChooseActionFacingLeft,
    /// <summary>Mini Kraid animation script: step forwards, facing left.</summary>
    StepForwardsFacingLeft,
    /// <summary>Mini Kraid animation script: choose action, duplicate.</summary>
    ChooseAlternateActionFacingLeft,
    /// <summary>Mini Kraid animation script: step backwards, facing left.</summary>
    StepBackwardsFacingLeft,
    /// <summary>Mini Kraid animation script: fire spit, facing left.</summary>
    FireSpitFacingLeft,
    /// <summary>Mini Kraid animation script: choose action, duplicate, again 2.</summary>
    ChooseActionFacingRight,
    /// <summary>Mini Kraid animation script: step forwards, facing right.</summary>
    StepForwardsFacingRight,
    /// <summary>Mini Kraid animation script: choose action, duplicate, again 3.</summary>
    ChooseAlternateActionFacingRight,
    /// <summary>Mini Kraid animation script: step backwards, facing right.</summary>
    StepBackwardsFacingRight,
    /// <summary>Mini Kraid animation script: fire spit, facing right.</summary>
    FireSpitFacingRight,
}
