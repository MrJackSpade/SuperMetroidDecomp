namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified lists; see <see cref="FakeKraidAnimations"/>.
/// </summary>
internal enum FakeKraidSegment
{
    /// <summary>Mini Kraid animation: choose action.</summary>
    ChooseActionFacingLeft,
    /// <summary>Mini Kraid animation: step forwards, facing left.</summary>
    StepForwardsFacingLeft,
    /// <summary>Mini Kraid animation: choose action, duplicate.</summary>
    ChooseAlternateActionFacingLeft,
    /// <summary>Mini Kraid animation: step backwards, facing left.</summary>
    StepBackwardsFacingLeft,
    /// <summary>Mini Kraid animation: fire spit, facing left.</summary>
    FireSpitFacingLeft,
    /// <summary>Mini Kraid animation: choose action, duplicate, again 2.</summary>
    ChooseActionFacingRight,
    /// <summary>Mini Kraid animation: step forwards, facing right.</summary>
    StepForwardsFacingRight,
    /// <summary>Mini Kraid animation: choose action, duplicate, again 3.</summary>
    ChooseAlternateActionFacingRight,
    /// <summary>Mini Kraid animation: step backwards, facing right.</summary>
    StepBackwardsFacingRight,
    /// <summary>Mini Kraid animation: fire spit, facing right.</summary>
    FireSpitFacingRight,
}
