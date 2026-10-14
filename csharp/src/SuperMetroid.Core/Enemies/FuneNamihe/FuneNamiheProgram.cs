namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="FuneNamiheAnimations"/>.
/// </summary>
internal enum FuneNamiheProgram
{
    /// <summary><c>InstList_Fune_FacingLeft_Idling</c> at $A8:9399.</summary>
    FuneIdleLeft,
    /// <summary><c>InstList_Fune_FacingLeft_Active</c> at $A8:939F.</summary>
    FuneActiveLeft,
    /// <summary><c>InstList_Fune_FacingRight_Idling</c> at $A8:93C9.</summary>
    FuneIdleRight,
    /// <summary><c>InstList_Fune_FacingRight_Active</c> at $A8:93CF.</summary>
    FuneActiveRight,
    /// <summary><c>InstList_Namihe_FacingLeft_Idling</c> at $A8:95BD.</summary>
    NamiheIdleLeft,
    /// <summary><c>InstList_Namihe_FacingLeft_Active</c> at $A8:95C3.</summary>
    NamiheActiveLeft,
    /// <summary><c>InstList_Namihe_FacingRight_Idling</c> at $A8:95F1.</summary>
    NamiheIdleRight,
    /// <summary><c>InstList_Namihe_FacingRight_Active</c> at $A8:95F7.</summary>
    NamiheActiveRight,
}
