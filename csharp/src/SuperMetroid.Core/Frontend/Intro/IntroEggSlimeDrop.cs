using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created when the confused baby first cries.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position and impact animation.</summary>
    internal readonly CinematicActor<IntroEggEffectAnimation, CinematicSpritePose, NoAnimationCue> sprite;
    /// <summary>How the drop flies: its sideways drift and its vertical speed for each update it has flown.</summary>
    private readonly IntroEggSlimeLaunch launch;
    /// <summary>
    /// The frames the drop has fallen: its place along its fall. A plain count (#1781): every drop reaches
    /// the puddle height within its curve, whose next step throws.
    /// </summary>
    private int framesFallen;
    /// <summary>Whether the drop still flies, or splashes where it reached the puddle-animation height.</summary>
    private IntroEggSlimeDropPhase phase;

    /// <summary>Starts a drop at the confused Baby's position, flying as <paramref name="launch"/> says.</summary>
    public IntroEggSlimeDrop(Offset16<ScreenPixels> babyPosition, IntroEggSlimeLaunch launch)
    {
        CinematicActorDefinition<IntroEggEffectAnimation> definition = IntroEggEffectDefinitions.SlimeDrop;
        this.launch = launch;

        // Every drop starts where the confused Baby is.
        sprite = new(babyPosition, IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), IntroEggEffectAnimations.Effects,
            definition.Animation);
    }

    /// <summary>
    /// Flies the drop by its launch until it reaches <see cref="IntroEggEffectDefinitions.SlimeDropLandingY"/>, then plays
    /// the puddle animation in place.
    /// </summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;

        if (phase == IntroEggSlimeDropPhase.Falling)
        {
            AddVelocity(axis: Axis.Horizontal, launch.Drift);
            AddVelocity(axis: Axis.Vertical, launch.Fall.Speed(framesFallen));

            if (sprite.ScreenPlace.Y >= IntroEggEffectDefinitions.SlimeDropLandingY)
            {
                // Landing changes the animation and stops the drop, freezing the impact point
                // while the puddle animation plays four ten-frame frames and deletes the actor.
                sprite.Start(IntroEggEffectAnimation.SlimeImpact);
                phase = IntroEggSlimeDropPhase.Splashing;
            }
            else
            {
                framesFallen++;
            }
        }

        sprite.Step();
    }

    /// <summary>Draws the drop's current frame with the installed egg-effect artwork.</summary>
    public void Draw(ScreenSprites screenSprites, IIntroCinematicSpritePresentation installedArt) =>
        sprite.Draw(screenSprites, installedArt: installedArt);

    /// <summary>Moves one screen coordinate by a velocity, carrying its fraction of a pixel.</summary>
    private void AddVelocity(Axis axis, SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(sprite.Position.X + velocity)
            : sprite.Position.WithY(sprite.Position.Y + velocity);
    }
}
