using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.NoobTube.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The colouring each frame family whose draws always show one is pre-coloured in (#1513), over the rows its frames ink,
/// on every sheet holding them, so its pictures show the colours the game draws them in rather than rows the room's load
/// leaves empty.
/// </summary>
internal static class FrameColouringDefinitions
{
    /// <summary>
    /// The Tourian statue scene's entrance colours, in which its statues' souls, glows and fragments draw: the scene's
    /// initialization installs them in the statue and base rows before any of them spawns, whichever sheet they draw from.
    /// </summary>
    private static readonly FrameColouring TourianStatueEntrance = new(EnemyDefinitionId.TourianStatue, [TourianStatueShades.Entrance]);

    /// <summary>
    /// The bosses' projectile frames: Crocomire's bridge fragments and spike-wall pieces, drawn in his wall colours with
    /// his wall spikes' row his death sequence loads before they fall; Draygon's goop, drawn in his opening's colours; and
    /// Mother Brain's falling tubes and glass shards, drawn in the colours her room entry loads for them.
    /// </summary>
    private static readonly Dictionary<EnemyProjectileFrameId, FrameColouring> BossProjectiles = new()
    {
        [EnemyProjectileFrameId.CrocomireFrame06] = new(EnemyDefinitionId.Crocomire, [CrocomireShades.Wall, CrocomireShades.WallSpikes]),
        [EnemyProjectileFrameId.CrocomireFrame07] = new(EnemyDefinitionId.Crocomire, [CrocomireShades.Wall, CrocomireShades.WallSpikes]),
        [EnemyProjectileFrameId.DraygonFrame06] = new(EnemyDefinitionId.DraygonBody, [DraygonShades.Intro]),
        [EnemyProjectileFrameId.DraygonFrame07] = new(EnemyDefinitionId.DraygonBody, [DraygonShades.Intro]),
        [EnemyProjectileFrameId.MotherBrainTopTubeFrame00] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainTopTubeFrame01] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainTopTubeFrame02] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainTopTubeFrame03] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame00] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame01] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame02] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame03] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame04] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame05] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame06] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame07] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame08] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame09] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame10] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame11] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame12] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame13] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame14] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyProjectileFrameId.MotherBrainGlassFrame15] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
    };

    /// <summary>
    /// The projectile frames always drawn in one colouring: the bosses' (<see cref="BossProjectiles"/>), and every frame of
    /// the Tourian statue scene's statue programs, drawn in the scene's entrance colours.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyProjectileFrameId, FrameColouring> Projectiles { get; } = new Dictionary<EnemyProjectileFrameId, FrameColouring>(
        BossProjectiles.Concat(TourianStatueProjectileAnimations.Segments.Frames
            .Select(frame => KeyValuePair.Create(frame, TourianStatueEntrance))));

    /// <summary>
    /// The projectile frames a common spawner's objects always draw in its shades: every piece of the n00b tube (its
    /// crack, shards and bubbles), drawn in the crack colours its cracks' initialization installs.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyProjectileFrameId, CommonSpawnerShades> CommonProjectiles { get; } =
        NoobTubeProjectileFrameDefinitions.ShownProjectileFrames.Values.Distinct().ToDictionary(frame => frame, _ => CommonSpawnerShades.TubeCracks);

    /// <summary>The enemy frames: Mother Brain's tubes, drawn from her room's entry in the tube colours it loads.</summary>
    internal static IReadOnlyDictionary<EnemyFrameId, FrameColouring> Frames { get; } = new Dictionary<EnemyFrameId, FrameColouring>
    {
        [EnemyFrameId.MotherBrainTube0] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyFrameId.MotherBrainTube1] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyFrameId.MotherBrainTube2] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyFrameId.MotherBrainTube3] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
        [EnemyFrameId.MotherBrainTube4] = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]),
    };
}
