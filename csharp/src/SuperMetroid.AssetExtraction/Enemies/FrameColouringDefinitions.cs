using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.NoobTube;
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

    /// <summary>Crocomire's wall colours with his wall spikes' row, which his death sequence loads before his bridge and spike wall fall.</summary>
    private static readonly FrameColouring CrocomireWall = new(EnemyDefinitionId.Crocomire, [CrocomireShades.Wall, CrocomireShades.WallSpikes]);

    /// <summary>Draygon's opening colours, in which his goop draws.</summary>
    private static readonly FrameColouring DraygonOpening = new(EnemyDefinitionId.DraygonBody, [DraygonShades.Intro]);

    /// <summary>The colours Mother Brain's room entry loads for her falling tubes and glass shards.</summary>
    private static readonly FrameColouring MotherBrainRoomEntry = new(EnemyDefinitionId.MotherBrainBody, [MotherBrainShades.RoomEntry]);

    /// <summary>
    /// The bosses' projectile frames: Crocomire's bridge fragments and spike-wall pieces, drawn in his wall colours with
    /// his wall spikes' row his death sequence loads before they fall; Draygon's goop, drawn in his opening's colours; and
    /// Mother Brain's falling tubes and glass shards, drawn in the colours her room entry loads for them.
    /// </summary>
    private static readonly Dictionary<EnemyProjectileFrameId, FrameColouring> BossProjectiles = new()
    {
        [EnemyProjectileFrameId.CrocomireBridgeFragment] = CrocomireWall,
        [EnemyProjectileFrameId.CrocomireSpikeWallPiece] = CrocomireWall,
        [EnemyProjectileFrameId.DraygonGoopCurled] = DraygonOpening,
        [EnemyProjectileFrameId.DraygonGoopScattered] = DraygonOpening,
        [EnemyProjectileFrameId.MotherBrainTopTubeTopRight] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainTopTubeTopLeft] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleLeft] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleRight] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned0] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned45] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned90] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned135] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned180] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned225] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned270] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpTurned315] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned0] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned45] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned90] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned135] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned180] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned225] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned270] = MotherBrainRoomEntry,
        [EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned315] = MotherBrainRoomEntry,
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
        NoobTubeProjectileAnimations.ShownFrames.ToDictionary(frame => frame, _ => CommonSpawnerShades.TubeCracks);

    /// <summary>The enemy frames: Mother Brain's tubes, drawn from her room's entry in the tube colours it loads.</summary>
    internal static IReadOnlyDictionary<EnemyFrameId, FrameColouring> Frames { get; } = new Dictionary<EnemyFrameId, FrameColouring>
    {
        [EnemyFrameId.MotherBrainTubeBendRight] = MotherBrainRoomEntry,
        [EnemyFrameId.MotherBrainTubeBendLeft] = MotherBrainRoomEntry,
        [EnemyFrameId.MotherBrainTubeStraightLeft] = MotherBrainRoomEntry,
        [EnemyFrameId.MotherBrainTubeStraightRight] = MotherBrainRoomEntry,
        [EnemyFrameId.MotherBrainTubeColumn] = MotherBrainRoomEntry,
    };
}
