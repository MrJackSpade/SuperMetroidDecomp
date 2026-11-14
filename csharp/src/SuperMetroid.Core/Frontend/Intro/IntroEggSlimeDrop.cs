using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created when the confused baby first cries.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position and impact animation.</summary>
    internal readonly IntroDiscoverySprite sprite;
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
    public IntroEggSlimeDrop(Position16<ScreenPixels> babyPosition, IntroEggSlimeLaunch launch)
    {
        CinematicActorDefinition definition = IntroEggEffectDefinitions.SlimeDrop;
        this.launch = launch;

        // Every drop starts where the confused Baby is.
        sprite = new IntroDiscoverySprite(
            babyPosition,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: definition.Animation);
        sprite.InstallFrameBehavior(definition.FrameBehavior);
    }

    /// <summary>Flies the drop by its launch until Y 168, then plays the puddle animation in place.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;

        if (motionEnabled)
        {
            if (sprite.FrameBehavior != IntroEggEffectDefinitions.SlimeDrop.FrameBehavior)
            {
                throw new InvalidDataException(
                    $"Intro egg slime names invalid frame behaviour {sprite.FrameBehavior}.");
            }
            AddVelocity(axis: Axis.Horizontal, launch.Drift);
            AddVelocity(axis: Axis.Vertical, launch.Fall.Speed(framesFallen));

            if (CoordinateAxis.Difference(sprite.WholePosition.Y, 168) >= 0)
            {
                // Landing changes both animation and frame behaviour, freezing the impact point
                // while the puddle animation plays four ten-frame frames and deletes the actor.
                sprite.Start(CinematicSpriteSegment.MetroidEggSlimeImpact);
                sprite.InstallFrameBehavior(
                    CinematicSpriteFrameBehavior.NoOp);
                motionEnabled = false;
            }
            else
            {
                framesFallen++;
            }
        }

        sprite.Step();
    }

    /// <summary>Draws the drop's current frame with installed egg-effect artwork when provided.</summary>
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        sprite.Draw(objects, installedArt: installedArt);

    /// <summary>Adds 1/65536-pixel motion to one screen coordinate while preserving its fractional part.</summary>
    private void AddVelocity(Axis axis, SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(sprite.Position.X.MovedBy(velocity))
            : sprite.Position.WithY(sprite.Position.Y.MovedBy(velocity));
    }
}
