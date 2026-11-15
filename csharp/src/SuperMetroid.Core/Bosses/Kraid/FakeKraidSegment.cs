namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Generated from the verified animations; see <see cref="FakeKraidAnimations"/>.
/// </summary>
internal enum FakeKraidSegment
{
    /// <summary>Mini-Kraid facing left chooses its next action.</summary>
    ChooseActionFacingLeft,
    /// <summary>Mini Kraid animation: step forwards, facing left.</summary>
    StepForwardsFacingLeft,
    /// <summary>Mini-Kraid facing left chooses its next action again: the second choice, authored after its forward step.</summary>
    ChooseAlternateActionFacingLeft,
    /// <summary>Mini Kraid animation: step backwards, facing left.</summary>
    StepBackwardsFacingLeft,
    /// <summary>Mini Kraid animation: fire spit, facing left.</summary>
    FireSpitFacingLeft,
    /// <summary>Mini-Kraid facing right chooses its next action.</summary>
    ChooseActionFacingRight,
    /// <summary>Mini Kraid animation: step forwards, facing right.</summary>
    StepForwardsFacingRight,
    /// <summary>Mini-Kraid facing right chooses its next action again: the second choice, authored after its forward step.</summary>
    ChooseAlternateActionFacingRight,
    /// <summary>Mini Kraid animation: step backwards, facing right.</summary>
    StepBackwardsFacingRight,
    /// <summary>Mini Kraid animation: fire spit, facing right.</summary>
    FireSpitFacingRight,
}
