using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created by $8B:BA73.</summary>
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

        // $AA9A copies the confused baby's slot coordinates rather than using a separate
        // literal position table. Its low timer byte remains the horizontal trajectory ID.
        sprite = new IntroDiscoverySprite(
            babyPosition,
            palette: IntroCinematicDefinitions.Objects.DiscoveryPalette,
            program: definition.Program)
        {
            GeneralTimer = index,
        };
        sprite.InstallPreInstruction(definition.PreInstruction);
    }

    /// <summary>
    /// The actor's timer word holds the drop's trajectory number below the frames it has fallen, which each fall step
    /// counts up by one; the frames wrap within their eight bits without touching the trajectory. The two stay in the
    /// actor's one timer word because instruction-program counter steps also count that word.
    /// </summary>
    private const int TrajectoryBits = 8;

    /// <summary>The drop's horizontal trajectory number, 0 through 3; it never changes.</summary>
    private int Drop => unchecked((byte)sprite.GeneralTimer);

    /// <summary>The frames the drop has fallen, 0 through 255: its gravity-table index.</summary>
    private int FramesFallen => unchecked((byte)(sprite.GeneralTimer >> TrajectoryBits));

    /// <summary>Applies index-specific horizontal motion and parity-specific gravity until Y $A8, then runs the puddle list in place.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;

        if (motionEnabled)
        {
            if (sprite.PreInstruction != IntroEggEffectDefinitions.SlimeDrop.PreInstruction)
            {
                throw new InvalidDataException(
                    $"Intro egg slime names invalid pre-instruction {sprite.PreInstruction}.");
            }
            AddVelocity(axis: Axis.Horizontal, IntroEggMotionDefinitions.SlimeX(Drop));

            // BIT #1 selects two different gravity curves. This is the parameter's parity,
            // not an animation-frame toggle: odd drops begin at -2 px/frame, evens at -3.
            AddVelocity(axis: Axis.Vertical, IntroEggMotionDefinitions.SlimeY(
                FramesFallen, odd: (sprite.GeneralTimer & 1) != 0));

            if (unchecked((short)(sprite.WholePosition.Y - 168)) >= 0)
            {
                // $AAB3 changes both list and pre-instruction, freezing the impact point
                // while CD71 plays four ten-frame puddle frames and deletes the actor.
                sprite.Start(CinematicSpriteProgram.MetroidEggParticleHitGround);
                sprite.InstallPreInstruction(
                    CinematicSpritePreInstruction.NoOp);
                motionEnabled = false;
            }
            else
            {
                sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 256));
            }
        }

        sprite.Step();
    }

    /// <summary>Draws the drop's current frame with installed egg-effect artwork when provided.</summary>
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        sprite.Draw(objects, installedArt: installedArt);

    /// <summary>Adds native 16.16 motion to one screen coordinate while preserving its fractional part.</summary>
    private void AddVelocity(Axis axis, SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.X, velocity.WholeWord, velocity.Fraction))
            : sprite.Position.WithY(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.Y, velocity.WholeWord, velocity.Fraction));
    }
}
