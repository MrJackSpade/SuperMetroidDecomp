using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created when the confused baby first cries.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position and impact animation.</summary>
    internal readonly IntroDiscoverySprite sprite;
    /// <summary>
    /// The drop's trajectory, 0 through 3: it selects the horizontal velocity, and its parity the gravity curve; it never
    /// changes.
    /// </summary>
    private readonly int trajectory;
    /// <summary>
    /// The frames the drop has fallen: its step along the fall's gravity curve. A plain count (#1781): every drop reaches
    /// the puddle height within its curve, whose next step throws, so the count never approaches the 256 at which the
    /// game's byte would have wrapped.
    /// </summary>
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

    /// <summary>Applies index-specific horizontal motion and parity-specific gravity until Y 168, then plays the puddle animation in place.</summary>
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
