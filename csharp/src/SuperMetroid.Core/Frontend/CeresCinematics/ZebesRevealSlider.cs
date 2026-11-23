using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One actor of the Zebes reveal that slides away as the scene ends (#1768, #2489): the planet or a quarter of the star
/// field, with its definition, the sprite showing it, how fast its upward slide grows and its upward speed.
/// </summary>
/// <param name="definition">The actor's animation, colouring and slide.</param>
/// <param name="sprite">The cinematic sprite showing the actor: its position and animation.</param>
/// <param name="slideAcceleration">How fast the actor's upward slide grows, Zebes's faster than each star quarter's.</param>
internal sealed partial class ZebesRevealSlider(
    CeresDestructionActorDefinition definition,
    CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite,
    SignedEightEight slideAcceleration)
{
    /// <summary>The actor's animation, colouring and slide.</summary>
    internal CeresDestructionActorDefinition Definition { get; } = definition;

    /// <summary>The cinematic sprite showing the actor: its position and animation.</summary>
    internal CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> Sprite { get; } = Ensure.NotNull(sprite);

    /// <summary>The actor's upward speed, from rest when the slide starts.</summary>
    private SignedEightEight slideSpeed;

    /// <summary>
    /// Slides the actor up by its speed after the speed grows by its acceleration. Each actor leaves once it passes
    /// <see cref="CeresDestructionSceneDefinitions.Places.RevealActorExitY"/> above the screen, within a few dozen updates,
    /// so its speed stays small.
    /// </summary>
    internal void Slide()
    {
        slideSpeed += slideAcceleration;
        Sprite.Position = Sprite.Position.WithY(Sprite.Position.Y - slideSpeed.ToSixteenSixteenDelta());
    }
}
