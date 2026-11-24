using System.Text.Json.Nodes;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Projectiles;

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
}
