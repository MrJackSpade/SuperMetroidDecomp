using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One actor of the Zebes reveal (#1768, #2489): what it shows, its definition, the sprite showing it and, once the scene
/// slides away, its upward speed.
/// </summary>
/// <param name="role">What the actor shows, which selects its definition and installed placement.</param>
/// <param name="definition">The actor's animation, colouring and slide.</param>
/// <param name="sprite">The cinematic sprite showing the actor: its position and animation.</param>
internal sealed class ZebesRevealActor(
    ZebesRevealActorRole role,
    CeresDestructionActorDefinition definition,
    CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite)
{
    /// <summary>What the actor shows.</summary>
    internal ZebesRevealActorRole Role { get; } = role;

    /// <summary>The actor's animation, colouring and slide.</summary>
    internal CeresDestructionActorDefinition Definition { get; } = definition;

    /// <summary>The cinematic sprite showing the actor: its position and animation.</summary>
    internal CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> Sprite { get; } = Ensure.NotNull(sprite);

    /// <summary>The actor's upward speed, from rest when the slide starts.</summary>
    private SignedEightEight slideSpeed;

    /// <summary>
    /// Slides the actor up by its speed after the speed grows by its acceleration: Zebes's by a quarter pixel an
    /// update and each star quarter's by an eighth. Each actor leaves once it passes 128 pixels above the screen, within a few
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
