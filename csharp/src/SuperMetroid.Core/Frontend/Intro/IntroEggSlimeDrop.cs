using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created when the confused baby first cries.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position and impact program.</summary>
    internal readonly IntroDiscoverySprite sprite;
    /// <summary>
    /// The drop's trajectory, 0 through 3: it selects the horizontal velocity, and its parity the gravity curve; it never
    /// changes.
    /// </summary>
    private readonly int trajectory;
    /// <summary>The frames the drop has fallen, 0 through 255: its gravity-table index.</summary>
    private int framesFallen;
    /// <summary>Stops velocity integration when the drop reaches the puddle-animation height.</summary>
    private bool motionEnabled = true;

    /// <summary>Starts one of four drops at the confused Baby's position, using its index as trajectory selector.</summary>
    public IntroEggSlimeDrop(Position16<ScreenPixels> babyPosition, int index)
    {
        if (index is < 0 or >= IntroEggEffectDefinitions.SlimeDropCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        CinematicActorDefinition definition = IntroEggEffectDefinitions.SlimeDrop;
        trajectory = index;

        // The slime spawn copies the confused baby's slot coordinates rather than using a separate
        // literal position table.
        sprite = new IntroDiscoverySprite(
            babyPosition,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: definition.Animation);
        sprite.InstallFrameBehavior(definition.FrameBehavior);
    }

    /// <summary>The frames fallen count runs past 255 back to zero.</summary>
    private const int FramesFallenWrap = 256;

    /// <summary>Applies index-specific horizontal motion and parity-specific gravity until Y 168, then runs the puddle list in place.</summary>
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
            AddVelocity(axis: Axis.Horizontal, IntroEggMotion.SlimeX(trajectory));

            // The trajectory's parity selects one of two gravity curves, not an animation-frame toggle: odd drops begin
            // at -2 px/frame, evens at -3.
            AddVelocity(axis: Axis.Vertical, IntroEggMotion.SlimeY(
                framesFallen, odd: trajectory % 2 != 0));

            if (CoordinateAxis.Difference(sprite.WholePosition.Y, 168) >= 0)
            {
                // Landing changes both animation and frame behaviour, freezing the impact point
                // while the puddle animation plays four ten-frame frames and deletes the actor.
                sprite.Start(CinematicSpriteSegment.MetroidEggParticleHitGround);
                sprite.InstallFrameBehavior(
                    CinematicSpriteFrameBehavior.NoOp);
                motionEnabled = false;
            }
            else
            {
                framesFallen = (framesFallen + 1) % FramesFallenWrap;
            }
        }

        sprite.Step();
    }

    /// <summary>Draws the drop's current frame with installed egg-effect artwork when provided.</summary>
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        sprite.Draw(objects, installedArt: installedArt);

    /// <summary>Adds 16.16 motion to one screen coordinate while preserving its fractional part.</summary>
    private void AddVelocity(Axis axis, SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(sprite.Position.X.MovedBy(velocity))
            : sprite.Position.WithY(sprite.Position.Y.MovedBy(velocity));
    }
}
