using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Native bank-$90 HUD dispatcher identities shared by weapon and Grapple admission.</summary>
internal static class SamusHudRomData
{
    /// <summary>$90:DD69 selects Grapple at HUD item four.</summary>
    public const ushort GrappleSelectedItem = 4;
    /// <summary>$90:DD9A subtracts pose $35 to index transition flags.</summary>
    public const SamusPoseId FirstTransitionPose = SamusPoseId.CrouchingTransitionRightPose;
    /// <summary>$90:DD94 suppresses poses $DB-$F0.</summary>
    public const SamusPoseId NonFiringTransitionStart = SamusPoseId.UnusedPoseDb;
    /// <summary>$90:DD8F admits poses at or above $F1 to the standard handler.</summary>
    public const SamusPoseId StandardTransitionStart = SamusPoseId.CrouchingTransitionAimUpRightPose;
}
