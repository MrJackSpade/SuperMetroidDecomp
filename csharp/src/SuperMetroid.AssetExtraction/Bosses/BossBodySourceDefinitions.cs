using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// What the import reads boss body frames from (#1466, #1515): the tile Phantoon, Draygon, Crocomire and Mother Brain clear
/// their body layer with, where Crocomire's death writes his melt images, how the cartridge's body layer cells lie on a
/// body's area, where each boss's area lay against the boss, and the frames the cartridge writes only in part over the
/// frame its part showed before.
/// </summary>
internal static class BossBodySourceDefinitions
{
    /// <summary>
    /// Where the top-left corner of each boss's body area lay against the boss as the cartridge scrolled its layer (#1617,
    /// #1802, #1858, #1949, #2031), which the import folds into each installed frame's anchor so the game places a body at
    /// its boss's position: Kraid's his X radius, his body's right-edge adjustment, left of him and 152 pixels, his body's
    /// vertical anchor, above him; Phantoon's 40 left of and 40 above him; Draygon's 62 left of and 64 above him, the place
    /// on the layer's repeat the cartridge placed 450 to the right and 192 below; Crocomire's 51 left of and 67 above him;
    /// Mother Brain's 38 left of and 63 above her.
    /// </summary>
    internal static IReadOnlyDictionary<BossBodyOwner, Offset16<RoomPixels>> AreaFromBoss { get; } = new Dictionary<BossBodyOwner, Offset16<RoomPixels>>
    {
        [BossBodyOwner.Kraid] = new(-RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.Kraid).Radius.X, -152),
        [BossBodyOwner.Phantoon] = new(-40, -40),
        [BossBodyOwner.Draygon] = new(-62, -64),
        [BossBodyOwner.Crocomire] = new(-51, -67),
        [BossBodyOwner.MotherBrain] = new(-38, -63),
    };

    /// <summary>The blank tile, 824 under colour variant zero, unmirrored, behind sprites, that clears the shared body layer.</summary>
    internal static BackgroundTile BlankTile { get; } = new(824, 0, highPriority: false);

    /// <summary>The cell Crocomire's death writes a melt image from: the start of the layer's second row.</summary>
    internal const int CrocomireMeltFirstCell = 32;

    /// <summary>Cells across one screen of a body layer, the cells of a row in reading order.</summary>
    internal const int LayerColumns = 32;

    /// <summary>Rows of one screen of a body layer, after which the next screen's rows begin.</summary>
    internal const int ScreenRows = 32;

    /// <summary>Screens across a body's area, which a body layer's screens fill left to right and then downward.</summary>
    internal const int ScreensAcross = 2;

    /// <summary>
    /// The frames the cartridge writes only in part, over the frame the same part of the boss showed before it in each of
    /// its programs: each is pictured whole as that frame with its own cells written over it. Mother Brain's hand beam
    /// charges over her standing pose a frame at a time; Phantoon's eyeball looks out of his opened eye; Draygon's mouth
    /// opens on his body, and his dying arms' second frames redraw only the cells that change.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyFrameId, EnemyFrameId> FramesDrawnOver { get; } = new Dictionary<EnemyFrameId, EnemyFrameId>
    {
        [EnemyFrameId.MotherBrainBodyStandingTwinB] = EnemyFrameId.MotherBrainBodyStandingTwin,
        [EnemyFrameId.MotherBrainBodyStandingTwinC] = EnemyFrameId.MotherBrainBodyStandingTwinB,
        [EnemyFrameId.MotherBrainBodyStandingTwinD] = EnemyFrameId.MotherBrainBodyStandingTwinC,
        [EnemyFrameId.PhantoonEyeballCentered] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingUp] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingUpRight] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingRight] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingDownRight] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingDown] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingDownLeft] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingLeft] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.PhantoonEyeLookingUpLeft] = EnemyFrameId.PhantoonEyeballCenteredTwin,
        [EnemyFrameId.DraygonLimpArmFacingLeftClawDropping] = EnemyFrameId.DraygonLimpArmFacingLeftClawRaised,
        [EnemyFrameId.DraygonBodyFacingLeftJawShut] = EnemyFrameId.DraygonBodyFacingLeftJawShutTwin,
        [EnemyFrameId.DraygonBodyFacingLeftJawParting] = EnemyFrameId.DraygonBodyFacingLeftJawShutTwin,
        [EnemyFrameId.DraygonBodyFacingLeftJawOpen] = EnemyFrameId.DraygonBodyFacingLeftJawShutTwin,
        [EnemyFrameId.DraygonBodyFacingLeftJawWide] = EnemyFrameId.DraygonBodyFacingLeftJawShutTwin,
        [EnemyFrameId.DraygonLimpArmFacingRightClawDropping] = EnemyFrameId.DraygonLimpArmFacingRightClawRaised,
        [EnemyFrameId.DraygonBodyFacingRightJawShut] = EnemyFrameId.DraygonBodyFacingRightJawShutTwin,
        [EnemyFrameId.DraygonBodyFacingRightJawParting] = EnemyFrameId.DraygonBodyFacingRightJawShutTwin,
        [EnemyFrameId.DraygonBodyFacingRightJawOpen] = EnemyFrameId.DraygonBodyFacingRightJawShutTwin,
        [EnemyFrameId.DraygonBodyFacingRightJawWide] = EnemyFrameId.DraygonBodyFacingRightJawShutTwin,
    };

    /// <summary>
    /// The frames whose pictures lie lower on the body's area than the cells the cartridge writes them to (#1802), by the
    /// pixels they lie lower: the second to eighth steps of Crocomire's charge forward and step back and the second to fifth
    /// of his moving claws, whose poses sit lower against him. Every other frame lies on its cells.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyFrameId, int> FramesLowered { get; } = new Dictionary<EnemyFrameId, int>
    {
        [EnemyFrameId.CrocomireBodyNearFootForwardArmSquare] = 2,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmReaching] = 2,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmSweeping] = 1,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmLifting] = 2,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmUpright] = 2,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmBent] = 1,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmSquare] = 1,
        [EnemyFrameId.CrocomireBodySwingingArmRaised] = 2,
        [EnemyFrameId.CrocomireBodySwingingArmOutstretched] = 4,
        [EnemyFrameId.CrocomireBodySwingingArmLowering] = 2,
        [EnemyFrameId.CrocomireBodySwingingArmReturning] = 1,
    };

    /// <summary>
    /// The frames whose cartridge drawing leaves a gap for another part, pictured whole with that part's frame written over
    /// them (#1513): Phantoon's bodies clear their eye socket, which his eye fills whenever his body shows, so each is
    /// pictured with his closed eye in it. The eye's own frames still show over it as his eye opens and looks about.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyFrameId, EnemyFrameId> FramesPicturedWith { get; } = new Dictionary<EnemyFrameId, EnemyFrameId>
    {
        [EnemyFrameId.PhantoonInvulnerableBody] = EnemyFrameId.PhantoonEyeClosed,
        [EnemyFrameId.PhantoonFullHitboxBody] = EnemyFrameId.PhantoonEyeClosed,
        [EnemyFrameId.PhantoonEyeHitboxBody] = EnemyFrameId.PhantoonEyeClosed,
    };
}
