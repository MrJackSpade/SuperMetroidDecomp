namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="EvirAnimations"/>.
/// </summary>
internal enum EvirProgram
{
    /// <summary><c>InstList_Evir_Body_FacingLeft</c> at $A8:86A7.</summary>
    BodyFacingLeft,
    /// <summary><c>InstList_Evir_Arms_FacingLeft</c> at $A8:86C3.</summary>
    ArmsFacingLeft,
    /// <summary><c>InstList_Evir_Body_FacingRight</c> at $A8:870B.</summary>
    BodyFacingRight,
    /// <summary><c>InstList_Evir_Arms_FacingRight</c> at $A8:8727.</summary>
    ArmsFacingRight,
    /// <summary><c>InstList_Evir_Projectile_Normal</c> at $A8:876F.</summary>
    ProjectileNormal,
    /// <summary><c>InstList_Evir_Projectile_Regenerating_0</c> at $A8:8775.</summary>
    ProjectileRegenerating,
    /// <summary><c>InstList_Evir_Projectile_Regenerating_1</c> at $A8:877D.</summary>
    ProjectileRegenerationLoop,
}
