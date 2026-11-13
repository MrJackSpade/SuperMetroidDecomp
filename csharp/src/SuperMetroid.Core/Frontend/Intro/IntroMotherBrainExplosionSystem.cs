using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The eight cinematic sprite objects spawned by Mother Brain's fourth intro missile hit.
/// </summary>
/// <remarks>
/// The large and small explosion definitions: initializer tables place each object and delay its first frame, and each runs its own
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

        // Although their definitions are listed together, the retail game explicitly
        // spawns three small actors first and five big actors second. Each initializer uses
        // its parameter as an index into independent position and start-delay tables.
        for (int parameter = 0; parameter < IntroMotherBrainDefinitions.SmallExplosionCount; parameter++)
            actors.Add(ExplosionActor.CreateSmall(parameter));
        for (int parameter = 0; parameter < IntroMotherBrainDefinitions.BigExplosionCount; parameter++)
            actors.Add(ExplosionActor.CreateBig(parameter));
    }

    /// <summary>Advances every allocated object through the exact cinematic animation format.</summary>
    public void Step(int introCrossfadeTimer)
    {
        foreach (ExplosionActor actor in actors)
            actor.Step(introCrossfadeTimer);
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

            installedArt.Draw(pose, objects, actor.Position.ToSpritePosition(), IntroShades.Drawn(IntroSpriteColouring.Explosions));
        }
    }

    /// <summary>One cinematic object's delayed animation and visible frame.</summary>
    internal sealed class ExplosionActor
    {
        /// <summary>The cinematic sprite object running the explosion's animation.</summary>
        private readonly IntroDiscoverySprite sprite;

        /// <summary>Starts one explosion at its authored offset and independent launch delay.</summary>
        private ExplosionActor(
            Position16<ScreenPixels> position,
            CinematicSpriteSegment segment,
            int startDelay)
        {
            sprite = new IntroDiscoverySprite(position, IntroShades.Drawn(IntroSpriteColouring.Explosions), segment);
            sprite.DelayFirstStep(startDelay);
        }

        /// <summary>Fixed cinematic screen origin selected by the small or big placement table.</summary>
        public Position16<ScreenPixels> Position => sprite.WholePosition;

        /// <summary>The pose the explosion's animation shows; null before its first frame and after deletion.</summary>
        public CinematicSpritePose? Pose => sprite.Pose;

        /// <summary>Cleared by the animation's delete after the later crossfade or list completion.</summary>
        public bool IsActive => sprite.IsActive;

        /// <summary>Starts a big explosion from its five-entry position and delay table.</summary>
        public static ExplosionActor CreateBig(int parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainExplosionPlacements.BigExplosion(parameter);
            return new ExplosionActor(
                IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset,
                IntroMotherBrainDefinitions.BigExplosionActor.Animation,
                placement.StartDelayFrames);
        }

        /// <summary>Starts a small explosion from its three-entry position and delay table.</summary>
        public static ExplosionActor CreateSmall(int parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainExplosionPlacements.SmallExplosion(parameter);
            return new ExplosionActor(
                IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset,
                IntroMotherBrainDefinitions.SmallExplosionActor.Animation,
                placement.StartDelayFrames);
        }

        /// <summary>Consumes one cinematic handler call.</summary>
        public void Step(int introCrossfadeTimer)
        {
            if (!IsActive)
                return;

            // The explosions delete these actors only when the later page-two crossfade timer
            // reaches zero. Starting the delete before the generic decrement makes deletion
            // occur in this same handler call.
            if (introCrossfadeTimer == 0)
                sprite.Start(CinematicSpriteSegment.Delete);
            sprite.Step();
        }
    }
}
