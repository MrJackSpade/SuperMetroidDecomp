using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created when the confused baby first cries.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position, timer, and impact instruction list.</summary>
    internal readonly IntroDiscoverySprite sprite;
    /// <summary>Stops velocity integration when the drop reaches the puddle-animation height.</summary>
    private bool motionEnabled = true;

    /// <summary>Starts one of four drops at the confused Baby's position, using its index as trajectory selector.</summary>
    public IntroEggSlimeDrop(Position16<ScreenPixels> babyPosition, int index)
    {
        if (index is < 0 or >= IntroEggEffectDefinitions.SlimeDropCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        CinematicActorDefinition definition = IntroEggEffectDefinitions.SlimeDrop;

        // The slime spawn copies the confused baby's slot coordinates rather than using a separate
        // literal position table. Its low timer byte remains the horizontal trajectory ID.
        sprite = new IntroDiscoverySprite(
            babyPosition,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            program: definition.Program)
        {
            GeneralTimer = index,
        };
        sprite.InstallFrameBehavior(definition.FrameBehavior);
    }

    /// <summary>
    /// The actor's timer counts the drop's trajectory number below the frames it has fallen, each frame fallen counting
    /// as this step; the frames run past 255 back to zero without touching the trajectory. The two stay in the actor's
    /// one timer because instruction-program counter steps also count it.
    /// </summary>
    private const int FrameFallenStep = 256;

    /// <summary>The drop's horizontal trajectory number, 0 through 3; it never changes.</summary>
    private int Drop => sprite.GeneralTimer % FrameFallenStep;

    /// <summary>The frames the drop has fallen, 0 through 255: its gravity-table index.</summary>
    private int FramesFallen => sprite.GeneralTimer / FrameFallenStep;

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
                    $"Intro egg slime names invalid pre-instruction {sprite.FrameBehavior}.");
            }
            AddVelocity(axis: Axis.Horizontal, IntroEggMotion.SlimeX(Drop));

            // BIT #1 selects two different gravity curves. This is the parameter's parity,
            // not an animation-frame toggle: odd drops begin at -2 px/frame, evens at -3.
            AddVelocity(axis: Axis.Vertical, IntroEggMotion.SlimeY(
                FramesFallen, odd: (sprite.GeneralTimer & 1) != 0));

            if (CoordinateAxis.Difference(sprite.WholePosition.Y, 168) >= 0)
            {
                // Landing changes both animation and frame behaviour, freezing the impact point
                // while the puddle animation plays four ten-frame frames and deletes the actor.
                sprite.Start(CinematicSpriteProgram.MetroidEggParticleHitGround);
                sprite.InstallFrameBehavior(
                    CinematicSpriteFrameBehavior.NoOp);
                motionEnabled = false;
            }
            else
            {
                sprite.GeneralTimer = WrappingCounter.AddWord(sprite.GeneralTimer, FrameFallenStep);
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
