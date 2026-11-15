using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The eight cinematic sprite objects spawned by Mother Brain's fourth intro missile hit.
/// </summary>
/// <remarks>
/// The large and small explosions: each explosion's authored placement sets where it stands and delays its first frame, and each runs its own
/// typed animation (#641). Every object keeps its own timer and animation, which is what creates the staggered blast
/// pattern.
/// </remarks>
internal sealed class IntroMotherBrainExplosionSystem
{
    /// <summary>Three small and five big actors retained in spawn and drawing order.</summary>
    internal readonly List<ExplosionActor> actors = [];

    /// <summary>
    /// Replays the eight spawn calls in their original order.
    /// </summary>
    public void SpawnFourthHitExplosions()
    {
        if (actors.Count != 0)
            throw new InvalidOperationException("The intro Mother Brain explosions were already spawned.");

        // The three small explosions spawn first and the five big ones second, each at its own placement.
        foreach (IntroMotherBrainExplosionPlacement placement in IntroMotherBrainDefinitions.SmallExplosions)
            actors.Add(ExplosionActor.Create(placement, IntroMotherBrainDefinitions.SmallExplosionActor.Animation));
        foreach (IntroMotherBrainExplosionPlacement placement in IntroMotherBrainDefinitions.BigExplosions)
            actors.Add(ExplosionActor.Create(placement, IntroMotherBrainDefinitions.BigExplosionActor.Animation));
    }

    /// <summary>Advances every explosion; page two's crossfade reaching its final update (#1980) ends them.</summary>
    public void Step(bool crossfadeFinishing)
    {
        foreach (ExplosionActor actor in actors)
            actor.Step(crossfadeFinishing);
    }

    /// <summary>Adds each visible frame in actor order to the cinematic sprites.</summary>
    internal void Draw(ScreenSprites objects, CinematicSpriteSheet installedArt)
    {
        Ensure.NotNull(objects);
        Ensure.NotNull(installedArt);
        foreach (ExplosionActor actor in actors)
        {
            // The blank pose draws nothing.
            if (!actor.IsActive || actor.Pose is not { } pose || pose == CinematicSpritePose.Blank)
                continue;

            installedArt.Draw(pose, objects, actor.Position, IntroShades.Drawn(IntroSpriteColouring.Explosions));
        }
    }

    /// <summary>One cinematic object's delayed animation and visible frame.</summary>
    internal sealed class ExplosionActor
    {
        /// <summary>The cinematic sprite object running the explosion's animation.</summary>
        private readonly IntroDiscoverySprite sprite;

        /// <summary>Starts one explosion at its authored offset and independent launch delay.</summary>
        private ExplosionActor(
            Offset16<ScreenPixels> position,
            CinematicSpriteSegment segment,
            int startDelay)
        {
            sprite = new IntroDiscoverySprite(position, IntroShades.Drawn(IntroSpriteColouring.Explosions), segment);
            sprite.DelayFirstStep(startDelay);
        }

        /// <summary>Fixed cinematic screen origin selected by the small or big placement table.</summary>
        public Offset16<ScreenPixels> Position => sprite.ScreenPlace;

        /// <summary>The pose the explosion's animation shows; null before its first frame and after deletion.</summary>
        public CinematicSpritePose? Pose => sprite.Pose;

        /// <summary>Cleared by the animation's delete after the later crossfade or animation completion.</summary>
        public bool IsActive => sprite.IsActive;

        /// <summary>Starts an explosion running <paramref name="animation"/> at <paramref name="placement"/>.</summary>
        public static ExplosionActor Create(IntroMotherBrainExplosionPlacement placement, CinematicSpriteSegment animation) =>
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
