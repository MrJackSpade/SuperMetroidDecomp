using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// What the import reads boss body frames from (#1466, #1515): the tile Phantoon, Draygon, Crocomire and Mother Brain clear
/// their body layer with, where Crocomire's death writes his melt images, how the cartridge's body layer cells lie on a
/// body's area, and the frames the cartridge writes only in part over the frame its part showed before.
/// </summary>
internal static class BossBodySourceDefinitions
{
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
        [EnemyFrameId.MotherBrainBodyHandBeamThird] = EnemyFrameId.MotherBrainBodyHandBeamSecond,
        [EnemyFrameId.MotherBrainBodyHandBeamFourth] = EnemyFrameId.MotherBrainBodyHandBeamThird,
        [EnemyFrameId.MotherBrainBodyHandBeamFifth] = EnemyFrameId.MotherBrainBodyHandBeamFourth,
        [EnemyFrameId.PhantoonEyeballCentered] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingUp] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingUpRight] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingRight] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingDownRight] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingDown] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingDownLeft] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingLeft] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.PhantoonEyeLookingUpLeft] = EnemyFrameId.PhantoonEyeOpenThird,
        [EnemyFrameId.DraygonArmsFacingLeftDyingSecond] = EnemyFrameId.DraygonArmsFacingLeftDyingFirst,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopFirst] = EnemyFrameId.DraygonBodyFacingLeftReset,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopSecond] = EnemyFrameId.DraygonBodyFacingLeftReset,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopThird] = EnemyFrameId.DraygonBodyFacingLeftReset,
        [EnemyFrameId.DraygonBodyFacingLeftFireGoopFourth] = EnemyFrameId.DraygonBodyFacingLeftReset,
        [EnemyFrameId.DraygonArmsFacingRightDyingSecond] = EnemyFrameId.DraygonArmsFacingRightDyingFirst,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopFirst] = EnemyFrameId.DraygonBodyFacingRightReset,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopSecond] = EnemyFrameId.DraygonBodyFacingRightReset,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopThird] = EnemyFrameId.DraygonBodyFacingRightReset,
        [EnemyFrameId.DraygonBodyFacingRightFireGoopFourth] = EnemyFrameId.DraygonBodyFacingRightReset,
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
