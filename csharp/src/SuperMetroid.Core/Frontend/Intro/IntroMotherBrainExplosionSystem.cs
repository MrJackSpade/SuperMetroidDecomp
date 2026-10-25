using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The eight cinematic sprite objects spawned by Mother Brain's fourth intro missile hit.
/// </summary>
/// <remarks>
/// Definitions $CF15 and $CF1B: initializer tables place each object and delay its first frame, and each runs its own
/// typed program (#641). Every object keeps its own timer and program, which is what creates the staggered blast
/// pattern.
/// </remarks>
internal sealed class IntroMotherBrainExplosionSystem
{
    /// <summary>Three small and five big actors retained in native spawn and OAM order.</summary>
    internal readonly List<ExplosionActor> actors = [];

    /// <summary>
    /// Replays the eight spawn calls at $8B:B7C6-$B80B in their original order.
    /// </summary>
    public void SpawnFourthHitExplosions()
    {
        if (actors.Count != 0)
            throw new InvalidOperationException("The intro Mother Brain explosions were already spawned.");

        // Despite their adjacent definition addresses, the retail routine explicitly
        // spawns three small actors first and five big actors second. Each initializer uses
        // its parameter as an index into independent position and start-delay tables.
        for (int parameter = 0; parameter < IntroMotherBrainDefinitions.SmallExplosionCount; parameter++)
            actors.Add(ExplosionActor.CreateSmall(parameter));
        for (int parameter = 0; parameter < IntroMotherBrainDefinitions.BigExplosionCount; parameter++)
            actors.Add(ExplosionActor.CreateBig(parameter));
    }

    /// <summary>Advances every allocated object through the exact bank-$8B list format.</summary>
    public void Step(int introCrossfadeTimer)
    {
        foreach (ExplosionActor actor in actors)
            actor.Step(introCrossfadeTimer);
    }

    /// <summary>Adds each visible spritemap in native actor order to cinematic OAM.</summary>
    internal void Draw(ScreenSprites objects, CinematicSpriteSheet installedArt)
    {
        Ensure.NotNull(objects);
        Ensure.NotNull(installedArt);
        foreach (ExplosionActor actor in actors)
        {
            // The blank pose draws nothing.
            if (!actor.IsActive || actor.Pose is not { } pose || pose == CinematicSpritePose.Blank)
                continue;

            installedArt.Draw(pose, objects, actor.Position.ToSpritePosition(), IntroCinematicDefinitions.Objects.ExplosionPalette);
        }
    }

    /// <summary>One bank-$8B cinematic object's delayed program and visible frame.</summary>
    internal sealed class ExplosionActor
    {
        /// <summary>The cinematic sprite object running the explosion's program.</summary>
        private readonly IntroDiscoverySprite sprite;

        /// <summary>Starts one explosion at its authored offset and independent launch delay.</summary>
        private ExplosionActor(
            Position16<ScreenPixels> position,
            CinematicSpriteProgram program,
            int startDelay)
        {
            sprite = new IntroDiscoverySprite(position, IntroCinematicDefinitions.Objects.ExplosionPalette, program);
            sprite.DelayFirstInstruction(startDelay);
        }

        /// <summary>Fixed cinematic screen origin selected by the small or big placement table.</summary>
        public Position16<ScreenPixels> Position => sprite.WholePosition;

        /// <summary>The pose the explosion's program shows; null before its first frame and after deletion.</summary>
        public CinematicSpritePose? Pose => sprite.Pose;

        /// <summary>Cleared by the program's delete after the later crossfade or list completion.</summary>
        public bool IsActive => sprite.IsActive;

        /// <summary>Starts a big explosion from its five-entry position and delay table.</summary>
        public static ExplosionActor CreateBig(int parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainDefinitions.BigExplosion(parameter);
            return new ExplosionActor(
                IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset,
                IntroMotherBrainDefinitions.BigExplosionActor.Program,
                placement.StartDelayFrames);
        }

        /// <summary>Starts a small explosion from its three-entry position and delay table.</summary>
        public static ExplosionActor CreateSmall(int parameter)
        {
            IntroMotherBrainExplosionPlacement placement =
                IntroMotherBrainDefinitions.SmallExplosion(parameter);
            return new ExplosionActor(
                IntroMotherBrainDefinitions.MotherBrainOrigin + placement.Offset,
                IntroMotherBrainDefinitions.SmallExplosionActor.Program,
                placement.StartDelayFrames);
        }

        /// <summary>Consumes one cinematic handler call.</summary>
        public void Step(int introCrossfadeTimer)
        {
            if (!IsActive)
                return;

            // $8B:BA0F deletes these actors only when the later page-two crossfade timer
            // reaches zero. Starting the delete before the generic decrement makes deletion
            // occur in this same handler call.
            if (introCrossfadeTimer == 0)
                sprite.Start(CinematicSpriteProgram.Delete);
            sprite.Step();
        }
    }
}
