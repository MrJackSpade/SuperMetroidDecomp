namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations as typed programs (#641), one per bank-$A6 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="FakeKraidAnimations"/>.
/// </summary>
internal enum FakeKraidProgram
{
    /// <summary><c>InstList_MiniKraid_ChooseAction</c>.</summary>
    ChooseActionFacingLeft,
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingLeft</c>.</summary>
    StepForwardsFacingLeft,
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate</c>.</summary>
    ChooseAlternateActionFacingLeft,
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingLeft</c>.</summary>
    StepBackwardsFacingLeft,
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingLeft</c>.</summary>
    FireSpitFacingLeft,
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again2</c>.</summary>
    ChooseActionFacingRight,
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingRight</c>.</summary>
    StepForwardsFacingRight,
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again3</c>.</summary>
    ChooseAlternateActionFacingRight,
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingRight</c>.</summary>
    StepBackwardsFacingRight,
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingRight</c>.</summary>
    FireSpitFacingRight,
}
