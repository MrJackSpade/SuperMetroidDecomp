namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The per-update behaviour an Evir body, arms or spit runs.</summary>
public enum EvirAiFunction
{
    /// <summary><c>Function_Evir_HandleBodyArms</c>: bobs the body vertically or positions its following arms record.</summary>
    HandleBodyOrArms,
    /// <summary><c>Function_EvirProjectile_Idle</c>: keeps the reusable projectile at the body's mouth until a launch is selected.</summary>
    ProjectileIdle,
    /// <summary><c>Function_EvirProjectile_Moving</c>: checks for regeneration beyond the viewport, then advances the aimed projectile's fixed-point position.</summary>
    ProjectileMoving,
    /// <summary><c>Function_EvirProjectile_Regenerating</c>: anchors the growing projectile to the mouth plus its animation-controlled offset; a frozen body holds this state.</summary>
    ProjectileRegenerating,
}
