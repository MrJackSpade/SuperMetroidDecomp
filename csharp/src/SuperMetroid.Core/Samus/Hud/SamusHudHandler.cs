namespace SuperMetroid.Core.Samus.Hud;

/// <summary>The native bank-$90 HUD selection handlers that movement types dispatch to, valued by handler address.</summary>
internal enum SamusHudHandler : ushort
{
    /// <summary>$90:DD3D standard HUD handler.</summary>
    Standard,
    /// <summary>$90:BF9D HUDSelectionHandler_MorphBall: bomb/Power Bomb dispatch for ball movement.</summary>
    MorphBall,
    /// <summary>$90:DDB6 HUDSelectionHandler_SpinWallJump_Knockback_Shinespark_CF_etc: inactive Grapple preserves charge.</summary>
    Jump,
    /// <summary>$90:DD6F Grapple HUD handler.</summary>
    Grapple,
    /// <summary>$90:DDD8 Draygon-held HUD handler; all held poses use normal weapons.</summary>
    DraygonHeld,
    /// <summary>$90:DD74 turning HUD handler.</summary>
    Turning,
    /// <summary>$90:DD8C posture-transition HUD handler.</summary>
    Transition,
}
