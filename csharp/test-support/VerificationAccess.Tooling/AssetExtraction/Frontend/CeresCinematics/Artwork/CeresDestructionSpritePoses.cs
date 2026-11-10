using System.Collections.Frozen;
using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork.CeresDestructionSpriteFrameDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Selects the Ceres destruction's sprite poses (<see cref="CeresDestructionSpriteFrameDefinitions"/>): the asteroid and station blast
/// frames, the station blasts' part counts and each backdrop's pose.
/// </summary>
internal static class CeresDestructionSpritePoses
{
    /// <summary>The pose of single-part asteroid explosion frame <paramref name="frame"/>, 0 through 3.</summary>
    internal static CinematicSpritePose LargeFrame(int frame) =>
        (uint)frame < 4 ? Frames[13 + frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>
    /// The OAM part count of station blast <paramref name="frame"/>: frames zero through two use one part per reflected
    /// quadrant; frame four has two lobes per quadrant; frames three and five add two tips to a core per quadrant.
    /// </summary>
    internal static int StationPartCount(int frame) =>
        (uint)frame < 6 ? Frames[17 + frame].StockPartCount : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>The pose of station blast <paramref name="frame"/>, 0 through 5.</summary>
    internal static CinematicSpritePose StationFrame(int frame) =>
        (uint)frame < 6 ? Frames[17 + frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>The pose that shows <paramref name="backdrop"/>.</summary>
    internal static CinematicSpritePose BackdropPose(CeresDestructionBackdrop backdrop) =>
        BackdropPoses.TryGetValue(backdrop, out CinematicSpritePose pose) ? pose : throw new ArgumentOutOfRangeException(nameof(backdrop));

    /// <summary>
    /// Each backdrop's pose: the destruction's first seven frames draw the seven backdrops in the order
    /// <see cref="CeresDestructionBackdrop"/> lists them.
    /// </summary>
    private static readonly FrozenDictionary<CeresDestructionBackdrop, CinematicSpritePose> BackdropPoses =
        Enum.GetValues<CeresDestructionBackdrop>()
            .Select((backdrop, frame) => KeyValuePair.Create(backdrop, Frames[frame].Pose))
            .ToFrozenDictionary();
}
