using System.Text.Json.Nodes;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.WreckedShip;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The dead things of Tourian, which crumble when they are disturbed.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the corpses and the Sidehopper the baby Metroid drains.</summary>
    private void WriteCorpses()
    {
        static string? CorpseCue(DeadTourianCue cue) => cue.Kind == DeadTourianCueKind.EndHop
            ? "end_hop"
            : throw new InvalidDataException($"A corpse's animation raises {cue.Kind}, which this does not know.");

        // Each corpse lies in one look, and has so many rows to crumble.
        foreach ((string file, EnemyDefinitionId id, DeadTourianCorpseDefinition corpse) in new[]
                 {
                     ("zoomer_on_its_back", EnemyDefinitionId.CorpseZoomer, DeadTourianCorpseDefinitions.ZoomerOnItsBack),
                     ("zoomer_upright", EnemyDefinitionId.CorpseZoomer, DeadTourianCorpseDefinitions.ZoomerUpright),
                     ("ripper_level", EnemyDefinitionId.CorpseRipper, DeadTourianCorpseDefinitions.RipperLevel),
                     ("ripper_tilted", EnemyDefinitionId.CorpseRipper, DeadTourianCorpseDefinitions.RipperTilted),
                     ("skree", EnemyDefinitionId.CorpseSkree, DeadTourianCorpseDefinitions.Skree),
                 })
        {
            Enemy($"enemies/corpse_{file}.json", "Corpse", id).BodyColliders("Body", "Hurtbox", "Solid")
                .Value("look", "corpse")
                .Value("rows", corpse.RowCount)
                .Value("disturbed_updates", CorpseRotScheduleDefinitions.PreRotDelayUpdates)
                .Value("dust_effect", DustFile(DustExplosionAnimation.CorpseDustCloud))
                .Value("dust_sound", EngineNames.Sound(EnemySound.LiquidDamage))
                .Timeline("corpse", DeadTourianAnimations.Segments, DeadTourianAnimations.StartingWith(corpse.InitialAnimation), CorpseCue)
                .Write();
        }

        Enemy("enemies/dead_sidehopper.json", "DeadSidehopper", EnemyDefinitionId.CorpseSidehopper).BodyColliders("Body", "Hurtbox", "Solid")
            .Value("pause_updates", DeadSidehopperVictimDefinitions.PostLandingPause)
            .Value("drain_step_updates", DeadSidehopperVictimDefinitions.DrainUpdatesPerStage)
            .Value("rows", DeadSidehopperCorpseDefinitions.RowCount)
            .Value("disturbed_updates", CorpseRotScheduleDefinitions.PreRotDelayUpdates)
            .Value("dust_effect", DustFile(DustExplosionAnimation.CorpseDustCloud))
            .Value("dust_sound", EngineNames.Sound(EnemySound.LiquidDamage))
            .Looks(DeadTourianAnimations.Segments, DeadTourianAnimations.StartingWith, CorpseCue,
                ("idle", DeadTourianSegment.SidehopperIdle), ("hopping", DeadTourianSegment.SidehopperHopping),
                ("corpse", DeadTourianSegment.SidehopperCorpse), ("initially_dead", DeadTourianSegment.SidehopperInitiallyDead))
            .Write();
    }

    /// <summary>Writes the Covern and the face block, which show themselves by more than their pictures.</summary>
    private void WriteGlowingEnemies()
    {
        // The Covern: the steps of its flicker as it comes and goes, each so long, and whether each leaves it shown.
        Enemy("enemies/covern.json", "Covern", EnemyDefinitionId.Coven).BodyColliders("Hurtbox")
            .Value("reappearance_delay", WreckedShipGhostAppearanceDefinitions.ReappearanceDelay)
            .Value("first_extra_delay", WreckedShipGhostAppearanceDefinitions.InitialDelay - WreckedShipGhostAppearanceDefinitions.ReappearanceDelay)
            .Value("visible_updates", WreckedShipGhostAppearanceDefinitions.VisibleDuration)
            .Value("still_updates", WreckedShipGhostAppearanceDefinitions.StillPositionDuration)
            .Value("steady_course_updates", WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration)
            .Value("bob_start_speed", EngineNames.Number(WreckedShipGhostAppearanceDefinitions.BobStartSpeed))
            .Value("bob_quickening", EngineNames.Number(WreckedShipGhostAppearanceDefinitions.BobAcceleration))
            .Group("flicker", EngineNames.List(WreckedShipGhostAppearanceDefinitions.FlickerTimings.Select(step => (JsonNode)new JsonObject
            {
                ["updates"] = step.Duration,
                ["shows"] = step.Visibility == WreckedShipGhostFlickerVisibility.Shown,
            })))
            .Looks(WreckedShipGhostAnimations.Segments, WreckedShipGhostAnimations.StartingWith, ("floating", WreckedShipGhostSegment.Floating))
            .Write();

        // The face block: its glow recolours the whole face, so its pictures are kept once as it sleeps and once for
        // each step of the glow, and its timelines name whichever set the Godot game picks.
        EngineEnemy face = Enemy("enemies/face_block.json", "FaceBlock", EnemyDefinitionId.FaceBlock)
            .Value("wakes_with", "morph_ball")
            .Value("glow_updates", FaceBlockGlowDefinitions.FrameDuration)
            .Value("glow_levels", FaceBlockGlowAnimations.Levels.Count)
            .PictureSets("{glow}")
            .Looks(BlueBrinstarFaceBlockAnimations.Segments, BlueBrinstarFaceBlockAnimations.StartingWith,
                ("looking_ahead", BlueBrinstarFaceBlockSegment.LookingAhead), ("samus_left", BlueBrinstarFaceBlockSegment.SamusLeft),
                ("samus_right", BlueBrinstarFaceBlockSegment.SamusRight));
        for (int level = 0; level < FaceBlockGlowAnimations.Levels.Count; level++)
            face.ShadedSet($"_glow_{level}", $"face-block-glow-{FaceBlockGlowAnimations.Levels[level]}/0");
        face.Write();
    }

    /// <summary>Writes the Zebetites of Mother Brain's room and the fake Kraid.</summary>
    private void WriteBarriersAndFakeKraid()
    {
        // The Zebetites: one enemy that stands as each barrier in turn, a single column or a pair, and shows how drained it is.
        static JsonArray HealthPictures(EngineEnemy zebetite, ZebetiteBarrierForm form) =>
            EngineNames.List(ZebetiteHealthPictureDefinitions.ByForm[form].Select(shown => (JsonNode)new JsonObject
            {
                ["health"] = shown.MinimumHealth,
                ["picture"] = zebetite.Picture(shown.Picture),
            }));
        EngineEnemy zebetite = Enemy("enemies/zebetite.json", "Zebetite", EnemyDefinitionId.Zebetite).BodyColliders("Hurtbox", "Hurtbox2", "Solid", "Solid2")
            .Value("pair_offset", ZebetiteDefinitions.SplitHalfOffset)
            .Value("shot_sound", EngineNames.Sound(ZebetiteDefinitions.ShotSound))
            // Each column goes with the small explosion of an enemy's death.
            .Value("fall_effect", DeathFile(EnemyDeathExplosion.Small))
            .Group("barriers", EngineNames.List(ZebetiteDefinitions.Generations.Select(barrier => (JsonNode)new JsonObject
            {
                ["x"] = barrier.X,
                ["y"] = barrier.CentreY,
                ["half_height"] = barrier.HalfHeight,
                ["pair"] = barrier.Form == ZebetiteBarrierForm.SplitPair,
            })));
        zebetite.Group("health_pictures", HealthPictures(zebetite, ZebetiteBarrierForm.Single))
            .Group("pair_health_pictures", HealthPictures(zebetite, ZebetiteBarrierForm.SplitPair))
            .Write();

        // The fake Kraid: its gobs of spit are thrown up and fall, and its spikes fly level; both end on what they meet.
        Enemy("enemies/fake_kraid.json", "FakeKraid", EnemyDefinitionId.FakeKraid).BodyColliders(Ordinary)
            .Value("step_pixels", FakeKraidProjectileDefinitions.StepPixels)
            .Value("death_drops", FakeKraidProjectileDefinitions.DeathDropCount)
            .Value("cry_sound", EngineNames.Sound(KraidSoundDefinitions.FakeKraidCry))
            .Value("spike_sound", EngineNames.Sound(KraidSoundDefinitions.FakeKraidSpikeLaunch))
            .Looks(FakeKraidAnimations.Segments, FakeKraidAnimations.StartingWith,
                static (FakeKraidCue cue) => cue switch
                {
                    FakeKraidCue.Move => "move",
                    FakeKraidCue.ChooseAction => "choose",
                    FakeKraidCue.Spit => "spit",
                    FakeKraidCue.PlayCrySound => "cry",
                    _ => throw new InvalidDataException($"The fake Kraid's animation raises {cue}, which this does not know."),
                },
                ("step_forwards_left", FakeKraidSegment.StepForwardsFacingLeft), ("step_forwards_right", FakeKraidSegment.StepForwardsFacingRight),
                ("step_backwards_left", FakeKraidSegment.StepBackwardsFacingLeft), ("step_backwards_right", FakeKraidSegment.StepBackwardsFacingRight),
                ("fire_spit_left", FakeKraidSegment.FireSpitFacingLeft), ("fire_spit_right", FakeKraidSegment.FireSpitFacingRight))
            .Shot("spit", RoomEnemyProjectileKind.FakeKraidSpit, static (kraid, _, projectile) =>
            {
                projectile["timeline"] = kraid.SpawnedPicture("spit", FakeKraidProjectileAnimation.ShownProjectileFrames[FakeKraidProjectilePose.Spit]);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["motion"] = "falling";
                projectile["acceleration"] = EngineNames.Number(FakeKraidProjectileDefinitions.SpitGravity);
                projectile["speed_limit"] = EngineNames.Number(FakeKraidProjectileDefinitions.SpitMaximumFallSpeed);
            })
            .Shot("spike", RoomEnemyProjectileKind.FakeKraidSpike, static (kraid, shot, projectile) =>
            {
                shot["speed"] = EngineNames.Number(FakeKraidProjectileDefinitions.SpikeSpeed);
                kraid.SpawnedPicture("spike_left", FakeKraidProjectileAnimation.ShownProjectileFrames[FakeKraidProjectilePose.SpikeLeft]);
                kraid.SpawnedPicture("spike_right", FakeKraidProjectileAnimation.ShownProjectileFrames[FakeKraidProjectilePose.SpikeRight]);
                projectile["timeline"] = $"{kraid.Folder}/spike_{{side}}.json";
                projectile["picture_offset"] = EngineEnemy.NoOffset();
            })
            .Write();
    }
}
