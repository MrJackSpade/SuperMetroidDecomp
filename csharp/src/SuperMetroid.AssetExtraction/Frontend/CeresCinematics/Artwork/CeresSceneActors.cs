using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The poses each Ceres scene's actors show and the colouring each is drawn in (#1513), from Core's actor definitions: the
/// poses every actor's program shows, through every program it goes, falls or repeats into, each in the colouring the
/// actor's draws name. A scene's sheet pictures exactly these poses, each in its colouring's sprite set.
/// </summary>
internal static class CeresSceneActors
{
    /// <summary>The Ceres flight's poses: the front stars' and every rear-view actor's, each in its definition's colouring.</summary>
    /// <exception cref="InvalidDataException">The flight draws a pose in two colourings of different sets.</exception>
    internal static Dictionary<CinematicSpritePose, CeresSpriteColouring> Flight()
    {
        const string scene = "The Ceres flight";
        var poses = new Dictionary<CinematicSpritePose, CeresSpriteColouring>();
        Add(poses, scene, CeresFlightActorDefinitions.FrontStars.Animation, CeresFlightActorDefinitions.FrontStars.Colouring);
        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorDefinition actor = CeresFlightActors.RearViewActor(index);
            Add(poses, scene, actor.Animation, actor.Colouring);
        }
        return poses;
    }

    /// <summary>
    /// The Ceres destruction's poses before the Zebes reveal loads its characters: its three initial actors' in their
    /// definitions' colourings and every blast's in the explosions' colouring, which the destruction's blast spawns name.
    /// </summary>
    /// <exception cref="InvalidDataException">The destruction draws a pose in two colourings of different sets.</exception>
    internal static Dictionary<CinematicSpritePose, CeresSpriteColouring> Destruction()
    {
        const string scene = "The Ceres destruction";
        var poses = new Dictionary<CinematicSpritePose, CeresSpriteColouring>();
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorDefinition actor = CeresDestructionActors.InitialActor(index);
            Add(poses, scene, actor.Animation, actor.Colouring);
        }
        Add(poses, scene, CeresExplosionDefinitions.InitialActor.Animation, CeresSpriteColouring.Explosions);
        Add(poses, scene, CeresExplosionDefinitions.RepeatingActor.Animation, CeresSpriteColouring.Explosions);
        Add(poses, scene, CeresExplosionDefinitions.FinalWaveActor.Animation, CeresSpriteColouring.Explosions);
        Add(poses, scene, CeresExplosionDefinitions.StationBlastActor.Animation, CeresSpriteColouring.Explosions);
        return poses;
    }

    /// <summary>
    /// The Zebes reveal's poses: its six actors', and the Japanese subtitle's the title spawns with Japanese text (#1733),
    /// each in its definition's colouring.
    /// </summary>
    /// <exception cref="InvalidDataException">The reveal draws a pose in two colourings of different sets.</exception>
    internal static Dictionary<CinematicSpritePose, CeresSpriteColouring> ZebesReveal()
    {
        const string scene = "The Zebes reveal";
        var poses = new Dictionary<CinematicSpritePose, CeresSpriteColouring>();
        for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
        {
            CeresDestructionActorDefinition actor = CeresDestructionActors.ZebesActor(index);
            Add(poses, scene, actor.Animation, actor.Colouring);
        }
        Add(poses, scene, CeresDestructionActorDefinitions.ZebesSubtitle.Animation, CeresDestructionActorDefinitions.ZebesSubtitle.Colouring);
        return poses;
    }

    /// <summary>Records every pose <paramref name="program"/> shows as drawn in <paramref name="colouring"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The scene already draws one of the poses in a colouring of another sprite set: one picture cannot show both.
    /// </exception>
    private static void Add(Dictionary<CinematicSpritePose, CeresSpriteColouring> poses, string scene, CinematicSpriteSegment program,
        CeresSpriteColouring colouring)
    {
        foreach (CinematicSpritePose pose in CinematicShownFrames.PosesOf([program]))
        {
            if (poses.TryGetValue(pose, out CeresSpriteColouring drawn))
            {
                if (CeresSheetInks.SpriteSet(drawn) != CeresSheetInks.SpriteSet(colouring))
                    throw new InvalidDataException($"{scene} draws pose {pose} in both {drawn} and {colouring}, which one picture cannot show.");
                continue;
            }
            poses.Add(pose, colouring);
        }
    }
}
