using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One actor of the Zebes reveal (#1768, #2489): its definition, the sprite showing it and, once the scene slides away, its
/// upward speed.
/// </summary>
/// <param name="definition">The actor's animation, colouring and slide.</param>
/// <param name="sprite">The cinematic sprite showing the actor: its position and animation.</param>
internal sealed class ZebesRevealActor(
    CeresDestructionActorDefinition definition,
    CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite)
{

    /// <summary>The actor's animation, colouring and slide.</summary>
    internal CeresDestructionActorDefinition Definition { get; } = definition;

    /// <summary>The cinematic sprite showing the actor: its position and animation.</summary>
    internal CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> Sprite { get; } = Ensure.NotNull(sprite);

    /// <summary>The actor's upward speed, from rest when the slide starts.</summary>
    private SignedEightEight slideSpeed;

    /// <summary>
    /// Slides the actor up by its speed after the speed grows by its acceleration, Zebes's faster than each star
    /// quarter's. Each actor leaves once it passes <see cref="CeresDestructionSceneDefinitions.RevealActorExitY"/> above the screen, within a few
    /// dozen updates, so its speed stays small.
    /// </summary>
    /// <exception cref="InvalidDataException">The actor is a caption, which the reveal deletes before the scene slides.</exception>
    internal void Slide()
    {
        if (Definition.SlideAcceleration is not { } acceleration)
            throw new InvalidDataException("A PLANET ZEBES caption actor remained active when scene sliding began.");
        slideSpeed += acceleration;
        Sprite.Position = Sprite.Position.WithY(Sprite.Position.Y - slideSpeed.ToSixteenSixteenDelta());
    }
}
