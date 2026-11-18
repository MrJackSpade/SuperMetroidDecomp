using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using static SuperMetroid.Core.Frontend.CeresCinematics.CeresDestructionActorDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Selects the Ceres destruction's and the Zebes reveal's actor definitions (<see cref="CeresDestructionActorDefinitions"/>)
/// by role.
/// </summary>
internal static class CeresDestructionActors
{
    /// <summary>Selects the definition of the persistent destruction actor that shows <paramref name="role"/>.</summary>
    /// <remarks>
    /// The large asteroids are the rear view's with their own animation, the small asteroids are the rear view's
    /// unchanged, and the vortex is the rear view's held still: this vortex has no motion, so it neither
    /// moves nor drifts round the loop. The small asteroids and the vortex are drawn after every other actor, so the large
    /// asteroids and the blasts overlap them. No slide applies to these actors.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a destruction role.</exception>
    public static CeresDestructionActorDefinition InitialActor(CeresDestructionActorRole role) => role switch
    {
        CeresDestructionActorRole.LargeAsteroids => RearViewVariant(CeresRearViewActorRole.LargeAsteroids) with
        {
            Animation = CeresAnimation.DestructionLargeAsteroid,
        },
        CeresDestructionActorRole.SmallAsteroids => RearViewVariant(CeresRearViewActorRole.SmallAsteroids) with { DrawnLast = true },
        // The destruction's vortex holds still.
        CeresDestructionActorRole.Vortex => RearViewVariant(CeresRearViewActorRole.Vortex) with
        {
            HorizontalDelta = SignedSixteenSixteen.Zero,
            DriftsRoundLoop = false,
            DrawnLast = true,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    /// <summary>
    /// The destruction's copy of a rear-view actor: its behaviour, animation, colouring and horizontal motion, with no
    /// slide away, no part in ending the scene and drawn in spawn order.
    /// </summary>
    private static CeresDestructionActorDefinition RearViewVariant(CeresRearViewActorRole role)
    {
        CeresFlightActorDefinition flight = CeresFlightActors.RearViewActor(role);
        return new(flight.Animation, flight.Colouring, flight.HorizontalDelta, flight.DriftsRoundLoop, null, false, false);
    }

    /// <summary>Selects the definition of the Zebes reveal actor that shows <paramref name="role"/>.</summary>
    /// <remarks>
    /// The reveal's spawns select the planet, the four star quarters and the title; the title spawns the
    /// Japanese subtitle. All four star quarters start with the same colouring and acceleration; where each
    /// starts is installed (#1615). Only the lower-right quarter completes the scene. Planet and title are
    /// separate named roles with their own starts and animations, not curve exceptions.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a reveal role.</exception>
    public static CeresDestructionActorDefinition ZebesActor(ZebesRevealActorRole role) => role switch
    {
        ZebesRevealActorRole.Planet => ZebesPlanet,
        ZebesRevealActorRole.StarsUpperLeft or ZebesRevealActorRole.StarsUpperRight or ZebesRevealActorRole.StarsLowerLeft
            or ZebesRevealActorRole.StarsLowerRight => ZebesStars(role),
        ZebesRevealActorRole.Title => ZebesTitle,
        ZebesRevealActorRole.JapaneseSubtitle => ZebesSubtitle,
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    /// <summary>The definition of one quarter of the reveal's star field.</summary>
    private static CeresDestructionActorDefinition ZebesStars(ZebesRevealActorRole quarter)
    {
        return new(
            StarSheets[quarter],
            CeresSpriteColouring.SpaceObjects, SignedSixteenSixteen.Zero, false, SignedEightEight.FromParts(0, 32),
            quarter == SceneCompletingStars, false);
    }
}
