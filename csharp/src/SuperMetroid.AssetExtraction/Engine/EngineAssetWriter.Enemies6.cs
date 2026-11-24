using System.Text.Json.Nodes;
using SuperMetroid.Core.Audio;
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
}
