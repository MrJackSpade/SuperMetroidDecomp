namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The per-update behaviour an Evir body, arms or spit runs.</summary>
public enum EvirAction
{
    /// <summary>Bobs the body vertically or positions its following arms record.</summary>
    HandleBodyOrArms,
    /// <summary>Keeps the reusable projectile at the body's mouth until a launch is selected.</summary>
    ProjectileIdle,
    /// <summary>Checks for regeneration beyond the viewport, then advances the aimed projectile's fixed-point position.</summary>
    ProjectileMoving,
    /// <summary>Anchors the growing projectile to the mouth plus its animation-controlled offset; a frozen body holds this state.</summary>
    ProjectileRegenerating,
}
