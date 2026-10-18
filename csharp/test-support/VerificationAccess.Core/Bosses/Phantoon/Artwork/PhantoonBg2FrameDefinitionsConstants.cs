using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values of <see cref="PhantoonBg2FrameDefinitions"/> that only verification reads.</summary>
internal static class PhantoonBg2FrameDefinitionsConstants
{
    /// <summary>VRAM destination base for Phantoon's BG2 frame tiles.</summary>
    public const ushort VramBase = EnemyBg2FrameLayout.VramBase;
    /// <summary>WRAM staging base from which Phantoon's BG2 frame upload is copied.</summary>
    public const ushort WorkingRamBase = EnemyBg2FrameLayout.WorkingRamBase;

    /// <summary>Provides Phantoon BG2 frame bases in VRAM and working RAM for animation checks.</summary>
    extension(PhantoonBg2FrameDefinitions)
    {
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.VramBase"/>
        internal static ushort VramBase => PhantoonBg2FrameDefinitionsConstants.VramBase;
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.WorkingRamBase"/>
        internal static ushort WorkingRamBase => PhantoonBg2FrameDefinitionsConstants.WorkingRamBase;
    }
    /// <summary>$A7:DEDD, invulnerable body without active collision rectangles.</summary>
    internal static ushort InvulnerableBody => PhantoonBg2FrameDefinitions.Frame(0).Pointer;
    /// <summary>$A7:DEFB, the closed eyelid frame.</summary>
    internal static ushort ClosedEye => PhantoonBg2FrameDefinitions.Frame(3).Pointer;
    /// <summary>$A7:DF23, the centered pupil frame.</summary>
    internal static ushort CenteredEye => PhantoonBg2FrameDefinitions.Frame(7).Pointer;
    /// <summary>$A7:DF05/DF0F/DF19, three progressively opening eyelid frames.</summary>
    internal static ushort OpeningEye(int phase) => (uint)phase < 3 ? PhantoonBg2FrameDefinitions.Frame(4 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>$A7:DF2D..DF73 are named compass pupil placements, selected by CC A7..CC D1 gaze programs.</summary>
    internal static ushort Gaze(PhantoonGazeDirection direction) => PhantoonBg2FrameDefinitions.Frame(direction switch
    {
        PhantoonGazeDirection.Up => 8,
        PhantoonGazeDirection.UpRight => 15,
        PhantoonGazeDirection.Right => 11,
        PhantoonGazeDirection.DownRight => 13,
        PhantoonGazeDirection.Down => 9,
        PhantoonGazeDirection.DownLeft => 12,
        PhantoonGazeDirection.Left => 10,
        PhantoonGazeDirection.UpLeft => 14,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    }).Pointer;
    /// <summary>$A7:DFB3/DFC5/DFD7, the three two-component tentacle poses.</summary>
    internal static ushort TentaclePose(int phase) => (uint)phase < 3 ? PhantoonBg2FrameDefinitions.Frame(16 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>$A7:DFE9/DFF3/DFFD, the three mouth poses from rest to flame release.</summary>
    internal static ushort MouthPose(int phase) => (uint)phase < 3 ? PhantoonBg2FrameDefinitions.Frame(19 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
}
