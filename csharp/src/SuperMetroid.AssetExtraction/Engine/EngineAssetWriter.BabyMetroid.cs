using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The giant Metroid of Tourian: its pulses, its cries, its membrane's colours and the walls of its arena.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the giant Metroid of Tourian and the patches that wall its arena in and open it again.</summary>
    private void WriteBabyMetroid()
    {
        EngineEnemy baby = Enemy("enemies/baby_metroid.json", "BabyMetroid", EnemyDefinitionId.BabyMetroid);

        // Its walls: two columns of blocks that become solid, and air again, and look as they did throughout.
        const string wallsUp = "walls_up", wallsDown = "walls_down";
        var origin = new Position16<RoomBlocks>(0, 0);
        foreach ((RoomDefinition room, RoomStateSettings state) in StatesHolding(EnemyDefinitionId.BabyMetroid))
        {
            foreach ((string patch, BabyMetroidWallMutation mutation) in new[]
                     {
                         (wallsUp, BabyMetroidWallMutation.Create), (wallsDown, BabyMetroidWallMutation.Clear),
                     })
            {
                Layouts.Patch(room, state, patch, origin, BabyMetroidFightDefinitions.WallBlocks.SelectMany(wall =>
                    Layouts.CellsKeepingPictures(room, state, wall, BabyMetroidWallMutationDefinitions.Shapes[mutation])));
            }
        }

        // Its pulses. The end of its remorse may start the remorse again.
        static string? Cue(BabyMetroidCue cue) => cue switch
        {
            BabyMetroidCue.MaybeRemorseCry => "maybe_cry",
            _ => throw new InvalidDataException($"The giant Metroid's pulse raises {cue}, which this does not know."),
        };
        string Picture(BabyMetroidFrame frame) => baby.Picture(BabyMetroidVisualDefinitions.ShownFrames.Of(frame));
        foreach ((string name, BabyMetroidSegment segment) in new[]
                 {
                     ("normal", BabyMetroidSegment.Normal), ("latched_on", BabyMetroidSegment.LatchedOn),
                     ("finish_draining", BabyMetroidSegment.FinishDraining), ("remorse", BabyMetroidSegment.Remorse),
                 })
        {
            baby.Timeline(name, EngineTimelines.Of(BabyMetroidAnimations.Segments, BabyMetroidAnimations.StartingAt[segment], Picture, Cue));
        }

        // Its pictures at each beat of its membrane, in the colours its room gives it with the beat's laid over them.
        foreach (BabyMetroidMembranePulse pulse in BabyMetroidMembranePulse.All)
            baby.ShadedSet(MembraneSet(pulse), $"{TourianCorpseShades.BabyMetroidTargets.Key}+{BabyMetroidShades.Membrane(pulse).Key}");

        baby.BodyColliders("Body", "Hurtbox")
            .Value("walls_up_patch", wallsUp)
            .Value("walls_down_patch", wallsDown)
            .Value("cry_sound", EngineNames.Sound(BabyMetroidSoundDefinitions.DrainCry))
            .Value("feeding_cry_sound", EngineNames.Sound(BabyMetroidSoundDefinitions.QuickDrainCry))
            .Value("fly_away_cry_sound", EngineNames.Sound(BabyMetroidSoundDefinitions.FlyAwayCry))
            .Value("remorse_cry_sound", EngineNames.Sound(BabyMetroidSoundDefinitions.RemorseCry))
            .Value("calm_pulse_updates", BabyMetroidFightDefinitions.CalmPulsePhaseUpdates)
            .Value("feeding_pulse_updates", BabyMetroidFightDefinitions.FeedingPulsePhaseUpdates)
            .Value("pulse_beats", BabyMetroidMembranePulse.All.Count)
            .Value("cry_beat", BabyMetroidMembranePulse.CryPhase.Step)
            .Value("beats_a_cry", BabyMetroidFightDefinitions.PulsesPerCry)
            .PictureSets("{shade}")
            .Write();
    }

    /// <summary>The name of the set of the giant Metroid's pictures that shows one beat of its membrane.</summary>
    private static string MembraneSet(BabyMetroidMembranePulse pulse) => $"_{pulse.Step}";
}
