namespace SuperMetroid.Core.Samus.Hud;

/// <summary>The HUD selection handlers that movement types dispatch to, valued by handler address.</summary>
internal enum SamusHudHandler
{
    /// <summary>Standard HUD handler.</summary>
    Standard,
    /// <summary>HUDSelectionHandler_MorphBall: bomb/Power Bomb dispatch for ball movement.</summary>
    MorphBall,
    /// <summary>HUDSelectionHandler_SpinWallJump_Knockback_Shinespark_CF_etc: inactive Grapple preserves charge.</summary>
    Jump,
    /// <summary>Grapple HUD handler.</summary>
    Grapple,
    /// <summary>Draygon-held HUD handler; all held poses use normal weapons.</summary>
    DraygonHeld,
    /// <summary>Turning HUD handler.</summary>
    Turning,
    /// <summary>Posture-transition HUD handler.</summary>
    Transition,
}
