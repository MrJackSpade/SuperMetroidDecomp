using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created when the confused baby first cries.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position and impact animation.</summary>
    internal readonly CinematicActor sprite;
    /// <summary>How the drop flies: its sideways drift and its vertical speed for each update it has flown.</summary>
    private readonly IntroEggSlimeLaunch launch;
    /// <summary>
    /// The frames the drop has fallen: its place along its fall. A plain count (#1781): every drop reaches
    /// the puddle height within its curve, whose next step throws, so the count never approaches the 256 at which the
    /// game's byte would have wrapped.
    /// </summary>
    private int framesFallen;
    /// <summary>Stops velocity integration when the drop reaches the puddle-animation height.</summary>
    private bool motionEnabled = true;

    /// <summary>Starts a drop at the confused Baby's position, flying as <paramref name="launch"/> says.</summary>
    public IntroEggSlimeDrop(Offset16<ScreenPixels> babyPosition, IntroEggSlimeLaunch launch)
    {
        CinematicActorDefinition definition = IntroEggEffectDefinitions.SlimeDrop;
        this.launch = launch;

        // Every drop starts where the confused Baby is.
        sprite = new CinematicActor(
            babyPosition,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: definition.Animation);
    }

    /// <summary>
    /// Flies the drop by its launch until it reaches <see cref="IntroEggEffectDefinitions.SlimeDropLandingY"/>, then plays
    /// the puddle animation in place.
    /// </summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;

        if (motionEnabled)
        {
            AddVelocity(axis: Axis.Horizontal, launch.Drift);
            AddVelocity(axis: Axis.Vertical, launch.Fall.Speed(framesFallen));

            if (sprite.ScreenPlace.Y >= IntroEggEffectDefinitions.SlimeDropLandingY)
            {
                // Landing changes the animation and stops the drop, freezing the impact point
                // while the puddle animation plays four ten-frame frames and deletes the actor.
                sprite.Start(CinematicSpriteSegment.MetroidEggSlimeImpact);
                motionEnabled = false;
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

    /// <summary>Adds 1/65536-pixel motion to one screen coordinate while preserving its fractional part.</summary>
    private void AddVelocity(Axis axis, SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(sprite.Position.X + velocity)
            : sprite.Position.WithY(sprite.Position.Y + velocity);
    }
}
