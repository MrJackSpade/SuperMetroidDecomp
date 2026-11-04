using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Which frames each encounter artwork sequence is drawn under, and at which of its steps (#1513). A sheet holds a frame's
/// picture of a step only when the frame is drawn at that step, and its loaded picture only when it is drawn before the
/// sequence's first step or a step leaves it as loaded. A frame no entry lists for a sequence shows only its loaded
/// picture under it. A step that lands in the same update as a later one is never seen and is listed for no frame.
/// </summary>
internal static class EncounterArtworkDrawingDefinitions
{
    /// <summary>The frames Mother Brain's dead brain shows as a corpse, the only frames her corpse's rot changes.</summary>
    private static readonly EnemyFrameId[] MotherBrainCorpseFrames =
    [
        EnemyFrameId.MotherBrainHead6,
        EnemyFrameId.MotherBrainHead7,
        EnemyFrameId.MotherBrainHead8,
        EnemyFrameId.MotherBrainHead11,
        EnemyFrameId.MotherBrainHead12,
    ];

    /// <summary>Every entry, by sequence.</summary>
    internal static IReadOnlyList<EncounterArtworkDrawing> All { get; } =
    [
        // Draygon's opening readies the Evirs at her intro's first update; the opening Evirs spawn right after it, and the
        // Evirs her death releases long after: none is drawn before it.
        new(EncounterArtworkSequence.DraygonEvirs, DrawnBeforeFirstStep: false, [new(0, 0)])
        {
            SpriteObjects = [RoomSpriteObjectKind.DraygonIntroEvir, RoomSpriteObjectKind.DraygonDeathEvirFacingRight],
        },

        // The gunship's hull is drawn throughout its takeoff, one page an update.
        new(EncounterArtworkSequence.GunshipLiftoff, DrawnBeforeFirstStep: true, [new(0, null)])
        {
            Frames = [.. GunshipFrameDefinitions.ShownFrames.Select(entry => entry.Value)],
        },
        // The takeoff's dust spawns once the engines have fired, after the last page.
        new(EncounterArtworkSequence.GunshipLiftoff, DrawnBeforeFirstStep: true, [new(4, 4)])
        {
            Projectiles = [.. GunshipDustVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        },

        // Crocomire's skeleton shows only once his death's delay has loaded its last chunk, one chunk an update.
        new(EncounterArtworkSequence.CrocomireSkeleton, DrawnBeforeFirstStep: false, [new(5, 5)])
        {
            Frames = [.. CrocomireSkeletonVisualDefinitions.Frames],
        },
        // His fight's projectiles are drawn before his death, and the spike wall's pieces after the skeleton shows.
        new(EncounterArtworkSequence.CrocomireSkeleton, DrawnBeforeFirstStep: true, [new(5, 5)])
        {
            Projectiles = [.. CrocomireProjectileVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        },

        // Bomb Torizo's statue is drawn as it crumbles (steps 0 to 7); its gut's swaps land in one update, seen at step 11;
        // its face's swaps land in two updates, seen at steps 15 and 20.
        new(EncounterArtworkSequence.BombTorizo, DrawnBeforeFirstStep: true, [new(0, 7), new(11, 11), new(15, 15), new(20, 20)])
        {
            Frames = [.. TorizoFrameDefinitions.ShownFrames.Select(entry => entry.Value)],
        },
        // Its projectiles fly only once it has woken.
        new(EncounterArtworkSequence.BombTorizo, DrawnBeforeFirstStep: false, [new(7, 7), new(11, 11), new(15, 15), new(20, 20)])
        {
            Projectiles = [.. TorizoProjectileVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        },

        // Golden Torizo's awakening is its first program's first cue, before any frame shows; it is drawn as it crumbles
        // (steps 1 to 8), its egg release's swaps land two to an update, seen at every second step from 10 to 22, and its
        // eye glow and stun are seen at every step (23 to 30).
        new(EncounterArtworkSequence.GoldenTorizo, DrawnBeforeFirstStep: false,
            [new(0, 8), new(10, 10), new(12, 12), new(14, 14), new(16, 16), new(18, 18), new(20, 20), new(22, null)])
        {
            Frames = [.. TorizoFrameDefinitions.ShownFrames.Select(entry => entry.Value)],
            Projectiles = [.. TorizoProjectileVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        },

        // Ceres Ridley's escape pages show through his room's backgrounds alone; no frame is drawn over them.

        // The pre-explosion overlay door readies its artwork at its initialization, before its first draw; the doors of the
        // set's other rooms are drawn as their load leaves them.
        new(EncounterArtworkSequence.CeresDoor, DrawnBeforeFirstStep: true, [new(0, 0)])
        {
            Frames = [.. CeresDoorFrameDefinitions.ShownFrames.Select(entry => entry.Value)],
        },

        // Mother Brain's brain is drawn in her tank before her legs load, hidden as she rises (steps 0 to 11), drawn through
        // the Baby, her second attack pages and the loading of her corpse (12 to 25), and hidden for her escape (26 to 34).
        new(EncounterArtworkSequence.MotherBrainPages, DrawnBeforeFirstStep: true, [new(12, 25)])
        {
            Frames = [.. MotherBrainVisualDefinitions.HeadFrames.Select(entry => entry.Value), MotherBrainVisualDefinitions.NeckJointFrame],
        },
        // Her body shows once the legs have loaded, with her first attack pages, until her death.
        new(EncounterArtworkSequence.MotherBrainPages, DrawnBeforeFirstStep: false, [new(11, 25)])
        {
            Frames = [.. MotherBrainBodyVisualDefinitions.ShownFrames.Select(entry => entry.Value)],
        },
        // The Baby is drawn only once its last page has loaded, and hidden as her attack pages load again.
        new(EncounterArtworkSequence.MotherBrainPages, DrawnBeforeFirstStep: false, [new(15, 15)])
        {
            Frames = [.. BabyMetroidVisualDefinitions.ShownFrames.Select(entry => entry.Value)],
        },
        // Her attacks fly from her first attack pages until her death; the escape door's fragments and the subtitle spawn
        // once the exploded door's last page has loaded.
        new(EncounterArtworkSequence.MotherBrainPages, DrawnBeforeFirstStep: true, [new(8, 25), new(34, 34)])
        {
            Projectiles = [.. MotherBrainEffectFrameDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        },
        // Dust rises from her death, her corpse's rot and the exploding escape door.
        new(EncounterArtworkSequence.MotherBrainPages, DrawnBeforeFirstStep: true, [new(20, 34)])
        {
            Projectiles = [.. MiscDustProjectileFrameDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        },

        // Her corpse rots once it has tipped over, every rot step drawn over the corpse it rots.
        new(EncounterArtworkSequence.MotherBrainCorpseRot, DrawnBeforeFirstStep: true, [new(0, null)])
        {
            Frames = MotherBrainCorpseFrames,
        },
    ];
}
