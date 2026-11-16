using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six authored shell fragments spawned when the egg hatches.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>The cinematic sprite showing the fragment: its position and animation.</summary>
    internal readonly CinematicActor sprite;

    /// <summary>Which piece of the shell this is: it selects the fragment's animation and horizontal velocity, and never changes.</summary>
    private readonly IntroEggShellFragment fragment;

    /// <summary>
    /// The frames the fragment has fallen: its step along the fall's gravity curve, then past it. A plain count (#1781):
    /// every fragment reaches the floor within its curve or the installed steps past it, and a later step throws, so the
    /// count never approaches the 256 at which the game's byte would have wrapped.
    /// </summary>
    private int framesFallen;

    /// <summary>The installed steps the fragment falls by once it has fallen past its gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve fallPastCurve;

    /// <summary>Spawns one of six shell fragments at its compiled position with its selected animation.</summary>
    /// <param name="fragment">Which piece of the shell to spawn.</param>
    /// <param name="fallPastCurve">The installed steps a fragment falls by once it has fallen past its gravity curve.</param>
    public IntroEggParticle(IntroEggShellFragment fragment, ShellFragmentFallPastCurve fallPastCurve)
    {
        this.fallPastCurve = Ensure.NotNull(fallPastCurve);
        CinematicActorDefinition definition = IntroEggParticleActors.Particle(fragment);
        this.fragment = fragment;
        // The compiled start of each fragment matches all twelve authored values,
        // so this spawn does not read game data.
        sprite = new CinematicActor(
            IntroEggMotion.FragmentInitialPosition(fragment),
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: definition.Animation);
    }

    /// <summary>Applies 1/65536-pixel fragment velocity and gravity, redirecting to deletion once it reaches the floor.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;

        // The piece flies sideways at its own speed and vertically at its fall's speed for the updates it has flown.
        AddSixteenSixteenVelocity(sprite, axis: Axis.Horizontal, IntroEggMotion.FragmentX(fragment));
        AddSixteenSixteenVelocity(sprite, axis: Axis.Vertical, IntroEggMotion.FragmentFallSpeed(framesFallen, fallPastCurve));
        if (sprite.ScreenPlace.Y >= IntroEggMotionDefinitions.FragmentFloorY)
        {
            // The landed piece is gone from this update on.
            sprite.Delete();
        }
        else
        {
            framesFallen++;
        }

        sprite.Step();
    }

    /// <summary>Draws the fragment's current cinematic pose using installed art when supplied.</summary>
    public void Draw(ScreenSprites screenSprites,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        sprite.Draw(screenSprites, installedArt: installedArt);

    /// <summary>Adds a compiled signed whole/fraction velocity to one axis without changing the other coordinate.</summary>
    private static void AddSixteenSixteenVelocity(
        CinematicActor sprite,
        Axis axis,
        SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(sprite.Position.X + velocity)
            : sprite.Position.WithY(sprite.Position.Y + velocity);
    }
}
