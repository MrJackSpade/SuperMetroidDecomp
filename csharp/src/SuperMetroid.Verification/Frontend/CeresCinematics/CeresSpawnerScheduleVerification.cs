using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>
/// Checks the retail Ceres destruction around its explosion spawner's repeating blasts (#2489): the cinematic sprite
/// oracle matches the spawner's animation to the cartridge's list and the repeating-explosions routine it installs, read
/// with the countdown the fade-in seeds and without its departure stop; this confirms the scene those two readings assume.
/// </summary>
internal static class CeresSpawnerScheduleVerification
{
    /// <summary>
    /// Runs the retail destruction and checks that its fade-in completes no later than the spawner's first repeating blast,
    /// so the countdown the fade-in seeds is the one the stretch starts from, and that its gunship flies away only after the
    /// spawner's last repeating blast, so the routine's departure stop never withholds one.
    /// </summary>
    internal static void VerifyCeresSpawnerSchedule()
    {
        var spawner = new CinematicActor<CeresAnimation, CinematicSpritePose, CeresExplosionSpawnerCue>(new(0, 0),
            CeresShades.Drawn(CeresSpriteColouring.Explosions), CeresAnimations.ExplosionSpawner, CeresExplosionDefinitions.SpawnerActor.Animation);
        var repeatingUpdates = new List<int>();
        for (int update = 1; spawner.IsActive; update++)
        {
            spawner.Step(cue =>
            {
                if (cue == CeresExplosionSpawnerCue.SpawnRepeatingBlast)
                    repeatingUpdates.Add(update);
            });
        }
        AssertEqual(CeresExplosionDefinitions.RepeatingBlasts.Count, repeatingUpdates.Count, "the spawner spawns one repeating blast per place");

        var audio = AudioQueues.Standalone();
        CeresDestructionCinematicState state = CreateRetailDestructionFixture(audio);
        int? fadeInCompleted = null;
        int? departed = null;
        // The spawner's first update is the scene's setup, the update after its opening waits.
        for (int step = 1; step < 5000 && !state.Finished && departed is null; step++)
        {
            CeresDestructionPhase before = state.Phase;
            state.Step();
            audio.AdvanceFrame(default);
            int spawnerUpdate = step - CeresDestructionSceneDefinitions.InitialFrameWaits;
            if (before == CeresDestructionPhase.FadeInAndDrift && state.Phase == CeresDestructionPhase.ApproachExplosion)
                fadeInCompleted = spawnerUpdate;
            if (state.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
                departed = spawnerUpdate;
        }
        AssertTrue(fadeInCompleted is { } faded && faded <= repeatingUpdates[0],
            $"the fade-in completes on spawner update {fadeInCompleted}, no later than the first repeating blast's {repeatingUpdates[0]}");
        AssertTrue(departed is { } left && left > repeatingUpdates[^1],
            $"the gunship flies away on spawner update {departed}, after the last repeating blast's {repeatingUpdates[^1]}");
    }
}
