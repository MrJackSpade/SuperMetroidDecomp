namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$A8 function words stored by the Evir composite actors.</summary>
public enum EvirAiFunction : ushort
{
    /// <summary>$A8:8922, <c>Function_Evir_HandleBodyArms</c>: bobs the body vertically or positions its following arms record.</summary>
    HandleBodyOrArms = 0x8922,
    /// <summary>$A8:8A34, <c>Function_EvirProjectile_Idle</c>: keeps the reusable projectile at the body's mouth until a launch is selected.</summary>
    ProjectileIdle = 0x8a34,
    /// <summary>$A8:8A3B, <c>Function_EvirProjectile_Moving</c>: checks for regeneration beyond the viewport, then advances the aimed projectile's fixed-point position.</summary>
    ProjectileMoving = 0x8a3b,
    /// <summary>$A8:8A78, <c>Function_EvirProjectile_Regenerating</c>: anchors the growing projectile to the mouth plus its animation-controlled X offset; a frozen body holds this state.</summary>
    ProjectileRegenerating = 0x8a78,
}
