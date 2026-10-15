namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations as typed programs (#641), one per bank-$A6 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="FakeKraidAnimations"/>.
/// </summary>
internal enum FakeKraidProgram
{
    /// <summary><c>InstList_MiniKraid_ChooseAction</c> at $A6:99AC.</summary>
    ChooseActionFacingLeft,
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingLeft</c> at $A6:99AE.</summary>
    StepForwardsFacingLeft,
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate</c> at $A6:99C4.</summary>
    ChooseAlternateActionFacingLeft,
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingLeft</c> at $A6:99C6.</summary>
    StepBackwardsFacingLeft,
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingLeft</c> at $A6:99DC.</summary>
    FireSpitFacingLeft,
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again2</c> at $A6:99FA.</summary>
    ChooseActionFacingRight,
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingRight</c> at $A6:99FC.</summary>
    StepForwardsFacingRight,
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again3</c> at $A6:9A12.</summary>
    ChooseAlternateActionFacingRight,
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingRight</c> at $A6:9A14.</summary>
    StepBackwardsFacingRight,
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingRight</c> at $A6:9A2A.</summary>
    FireSpitFacingRight,
}
