using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles.Artwork;

/// <summary>Reads of what the enemy-projectile frames are drawn from (<see cref="EnemyProjectileRoomPictureDefinitions"/>).</summary>
internal static class EnemyProjectileFrames
{
    /// <summary>Where an enemy-projectile frame is drawn from.</summary>
    extension(EnemyProjectileFrameId frame)
    {
        /// <summary>Whether the frame is a room picture, drawn from the room's shared sheet in its standard colours.</summary>
        internal bool IsRoomPicture => EnemyProjectileRoomPictureDefinitions.RoomPictures.Contains(frame);
    }
}
