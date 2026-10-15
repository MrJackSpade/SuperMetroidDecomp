namespace SuperMetroid.Core.Samus.Hud;

/// <summary>The native bank-$90 HUD selection handlers that movement types dispatch to, valued by handler address.</summary>
internal enum SamusHudHandler : ushort
{
    /// <summary>$90:DD3D standard HUD handler.</summary>
    Standard = 0xdd3d,
    /// <summary>$90:BF9D HUDSelectionHandler_MorphBall: bomb/Power Bomb dispatch for ball movement.</summary>
    MorphBall = 0xbf9d,
    /// <summary>$90:DDB6 HUDSelectionHandler_SpinWallJump_Knockback_Shinespark_CF_etc: inactive Grapple preserves charge.</summary>
    Jump = 0xddb6,
    /// <summary>$90:DD6F Grapple HUD handler.</summary>
    Grapple = 0xdd6f,
    /// <summary>$90:DDD8 Draygon-held HUD handler; all held poses use normal weapons.</summary>
    DraygonHeld = 0xddd8,
    /// <summary>$90:DD74 turning HUD handler.</summary>
    Turning = 0xdd74,
    /// <summary>$90:DD8C posture-transition HUD handler.</summary>
    Transition = 0xdd8c,
}
