using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy whose animation places the picture it shows (#2210): the picture sits apart from the enemy's place, or at a
/// depth of its own, as each beat of the animation says, as Kraid's foot steps its pictures along its walk and his parts
/// sit behind the room's front tiles while he rises. The common draw places the enemy's picture so.
/// </summary>
internal interface IPlacedEnemyPicture
{
    /// <summary>How far the shown picture sits from the enemy's place.</summary>
    Offset16<RoomPixels> PicturePlacement { get; }

    /// <summary>The depth the shown picture is drawn at, or null for its picture's own.</summary>
    ActorDepth? PictureDepth { get; }
}
