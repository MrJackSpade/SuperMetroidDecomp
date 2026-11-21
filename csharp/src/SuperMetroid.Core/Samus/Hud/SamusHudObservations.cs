using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>What the HUD reads of Samus's movement and posture.</summary>
internal static partial class SamusHudObservations
{
    /// <summary>The HUD selection mode of <paramref name="movement"/> (<see cref="SamusMovementTypeDefinitions"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The movement type has no definition.</exception>
    internal static SamusHudSelectionMode SelectionMode(SamusMovementType movement) =>
        SamusMovementTypeDefinitions.Movements.TryGetValue(movement, out SamusMovementTypeDefinition? definition)
            ? definition.HudSelection
            : throw new ArgumentOutOfRangeException(nameof(movement), movement, "Undefined movement type.");

    /// <summary>
    /// Whether the posture-transition admission admits weapons for <paramref name="pose"/>: the input transitions admit
    /// crouch and stand transitions and preserves the charge through morph and unmorph art; for any other pose the answer
    /// is the one <paramref name="beyond"/> decoded from what the admission reads there.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The admission reads no flag for the pose.</exception>
    internal static bool PostureAdmitsWeapons(SamusPoseId pose, PostureAdmissionBeyondTransitions<SamusPoseId> beyond)
    {
        Ensure.NotNull(beyond);
        SamusPostureTransitionAdmission admission = SamusPoseMetadata.PostureTransitionAdmission(pose);
        return admission switch
        {
            SamusPostureTransitionAdmission.CrouchOrStandAdmits => true,
            SamusPostureTransitionAdmission.MorphKeepsCharge => false,
            SamusPostureTransitionAdmission.AnsweredBeyondTransitions => beyond.AdmitsWeapons(pose),
            SamusPostureTransitionAdmission.AlwaysAdmits or SamusPostureTransitionAdmission.NeverAdmits =>
                throw new ArgumentOutOfRangeException(nameof(pose), pose, "The posture-transition admission reads no flag for this pose."),
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusPostureTransitionAdmission)} {admission}."),
        };
    }
}
