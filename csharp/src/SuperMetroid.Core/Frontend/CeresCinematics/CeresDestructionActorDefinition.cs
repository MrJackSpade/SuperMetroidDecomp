using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One destruction actor's animation, colouring, horizontal motion, slide and drawing order (#1903, #2489); where it starts
/// is installed (#1615).
/// </summary>
/// <param name="Animation">The animation the actor plays.</param>
/// <param name="Colouring">The colouring the actor draws in.</param>
/// <param name="HorizontalDelta">How far the actor drifts sideways each update.</param>
/// <param name="DriftsRoundLoop">Whether the actor drifts round the Ceres scenes' drift loop.</param>
/// <param name="SlideAcceleration">How fast the actor's upward slide grows as the reveal slides away, or null for one that does not slide.</param>
/// <param name="CompletesScene">Whether the actor's leaving the screen ends the reveal.</param>
/// <param name="DrawnLast">Whether the scene over Ceres draws the actor after every other actor, so the others overlap it.</param>
internal readonly partial record struct CeresDestructionActorDefinition(
    CeresAnimation Animation,
    CeresSpriteColouring Colouring,
    SignedSixteenSixteen HorizontalDelta,
    bool DriftsRoundLoop,
    SignedEightEight? SlideAcceleration,
    bool CompletesScene,
    bool DrawnLast);
