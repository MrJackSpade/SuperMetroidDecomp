using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The eight cinematic actors spawned by Mother Brain's fourth intro missile hit.
/// </summary>
/// <remarks>
/// The large and small explosions: each explosion's authored placement sets where it stands and delays its first frame,
/// and each keeps its own delay and animation (#641), which staggers the blast.
/// </remarks>
internal sealed partial class IntroMotherBrainExplosionSystem
{
    /// <summary>Three small and five big actors retained in spawn and drawing order.</summary>
    internal readonly List<ExplosionActor> actors = [];

    /// <summary>Keeps the explosions the fourth hit spawned.</summary>
    private IntroMotherBrainExplosionSystem()
    {
    }

    /// <summary>
    /// Spawns the eight explosions of the fourth hit on Mother Brain, in the order they appear, as the system that runs them.
    /// </summary>
    public static IntroMotherBrainExplosionSystem SpawnFourthHitExplosions()
    {
        var system = new IntroMotherBrainExplosionSystem();
        // The three small explosions spawn first and the five big ones second, each at its own placement.
        foreach (IntroMotherBrainExplosionPlacement placement in IntroMotherBrainDefinitions.SmallExplosions)
            system.actors.Add(ExplosionActor.Create(placement, IntroMotherBrainDefinitions.SmallExplosionActor.Animation));
        foreach (IntroMotherBrainExplosionPlacement placement in IntroMotherBrainDefinitions.BigExplosions)
            system.actors.Add(ExplosionActor.Create(placement, IntroMotherBrainDefinitions.BigExplosionActor.Animation));
        return system;
    }

    /// <summary>Advances every explosion; page two's crossfade reaching its final update (#1980) ends them.</summary>
    public void Step(bool crossfadeFinishing)
    {
        foreach (ExplosionActor actor in actors)
            actor.Step(crossfadeFinishing);
    }

    /// <summary>Adds each visible frame in actor order to the cinematic sprites.</summary>
    internal void Draw(ScreenSprites screenSprites, CinematicSpriteSheet installedArt)
    {
        Ensure.NotNull(screenSprites);
        Ensure.NotNull(installedArt);
        foreach (ExplosionActor actor in actors)
        {
            // The blank pose draws nothing.
            if (!actor.IsActive || actor.Pose is not { } pose)
                continue;

            installedArt.Draw(pose, screenSprites, actor.Position, IntroShades.Drawn(IntroSpriteColouring.Explosions));
        }
    }

    /// <summary>One cinematic actor's delayed animation and visible frame.</summary>
    internal sealed partial class ExplosionActor
    {
        /// <summary>The cinematic actor running the explosion's animation.</summary>
        private readonly CinematicActor<IntroMotherBrainExplosionAnimation, CinematicSpritePose, NoAnimationCue> sprite;

        /// <summary>Starts one explosion at its authored offset and independent launch delay.</summary>
        private ExplosionActor(
            Offset16<ScreenPixels> position,
            IntroMotherBrainExplosionAnimation animation,
            int startDelay)
        {
            sprite = new(position, IntroShades.Drawn(IntroSpriteColouring.Explosions), IntroMotherBrainAnimations.Explosions, animation);
            sprite.DelayFirstStep(startDelay);
        }

        /// <summary>Fixed cinematic screen origin selected by the small or big placement table.</summary>
        public Offset16<ScreenPixels> Position => sprite.ScreenPlace;

        /// <summary>The pose the explosion's animation shows; null before its first frame and after deletion.</summary>
        public CinematicSpritePose? Pose => sprite.Shown;

        /// <summary>Cleared by the animation's delete after the later crossfade or animation completion.</summary>
        public bool IsActive => sprite.IsActive;

        /// <summary>Starts an explosion running <paramref name="animation"/> at <paramref name="placement"/>.</summary>
        public static ExplosionActor Create(IntroMotherBrainExplosionPlacement placement, IntroMotherBrainExplosionAnimation animation) =>
            new(IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset, animation, placement.StartDelayFrames);

        /// <summary>Runs one cinematic update of the explosion.</summary>
        public void Step(bool crossfadeFinishing)
        {
            if (!IsActive)
                return;

            // The explosions are gone from the update page two's crossfade reaches its final update.
            if (crossfadeFinishing)
                sprite.Delete();
            sprite.Step();
        }
    }
}
