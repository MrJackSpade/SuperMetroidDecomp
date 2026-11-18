using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One destruction actor's animation, colouring, horizontal motion and slide (#1903); where it starts is installed (#1615).</summary>
[IndependentFlag("independent authored actor facts: whether the actor drifts round the drift loop and whether it completes the scene")]
internal readonly record struct CeresDestructionActorDefinition(
    CeresAnimation Animation,
    CeresSpriteColouring Colouring,
    SignedSixteenSixteen HorizontalDelta,
    bool DriftsRoundLoop,
    SignedEightEight? SlideAcceleration,
    bool CompletesScene);
