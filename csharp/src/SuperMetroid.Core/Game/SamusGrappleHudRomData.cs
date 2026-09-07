namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$90 HUD dispatcher identities used to admit Grapple input.</summary>
internal static class SamusGrappleHudRomData
{
    /// <summary>$90:DD69 selects Grapple at HUD item four.</summary>
    public const ushort SelectedItem = 4;
    /// <summary>$90:DD05 movement-type HUD handler pointer table.</summary>
    public const int MovementHandlers = 0x90dd05;
    /// <summary>$90:DD3D standard HUD handler.</summary>
    public const ushort StandardHandler = 0xdd3d;
    /// <summary>$90:DD6F Grapple HUD handler.</summary>
    public const ushort GrappleHandler = 0xdd6f;
    /// <summary>$90:DD74 turning HUD handler.</summary>
    public const ushort TurningHandler = 0xdd74;
    /// <summary>$90:DD8C posture-transition HUD handler.</summary>
    public const ushort TransitionHandler = 0xdd8c;
    /// <summary>$90:DDAA posture-transition flags: zero enters the standard handler.</summary>
    public const int TransitionFlags = 0x90ddaa;
    /// <summary>$90:DD9A subtracts pose $35 to index transition flags.</summary>
    public const byte FirstTransitionPose = SamusPoseIds.CrouchingTransitionRightPose;
    /// <summary>$90:DD94 suppresses poses $DB-$F0.</summary>
    public const byte NonFiringTransitionStart = SamusPoseIds.UnusedPoseDb;
    /// <summary>$90:DD8F admits poses at or above $F1 to the standard handler.</summary>
    public const byte StandardTransitionStart = SamusPoseIds.CrouchingTransitionAimUpRightPose;
}
