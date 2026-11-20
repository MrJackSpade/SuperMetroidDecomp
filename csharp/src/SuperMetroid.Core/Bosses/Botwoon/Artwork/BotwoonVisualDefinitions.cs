using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>
/// Botwoon's head presentation (#641): the installed artwork identity of every typed head frame. Gameplay animates
/// <see cref="BotwoonHeadFrame"/> values; only this catalog knows the frames that draw them. His body segments' and
/// spit's beats name their installed projectile frames themselves (<see cref="BotwoonAnimations.ProjectileFrames"/>).
/// </summary>
internal static class BotwoonVisualDefinitions
{
    /// <summary>
    /// The installed frame each head pose shows: his closed mouth, then his open mouth, facing each way in turn; the hidden
    /// head shows the shared empty frame.
    /// </summary>
    internal static EnemyFrameTable<BotwoonHeadFrame> HeadFrames { get; } = new()
    {
        { BotwoonHeadFrame.Hidden, EnemyFrameId.PointAtOrigin },
        { BotwoonHeadFrame.Closed(CompassDirection.UpLeft), EnemyFrameId.BotwoonHeadClosedUpLeft },
        { BotwoonHeadFrame.Closed(CompassDirection.Left), EnemyFrameId.BotwoonHeadClosedLeft },
        { BotwoonHeadFrame.Closed(CompassDirection.DownLeft), EnemyFrameId.BotwoonHeadClosedDownLeft },
        { BotwoonHeadFrame.Closed(CompassDirection.Down), EnemyFrameId.BotwoonHeadClosedDown },
        { BotwoonHeadFrame.Closed(CompassDirection.DownRight), EnemyFrameId.BotwoonHeadClosedDownRight },
        { BotwoonHeadFrame.Closed(CompassDirection.Right), EnemyFrameId.BotwoonHeadClosedRight },
        { BotwoonHeadFrame.Closed(CompassDirection.UpRight), EnemyFrameId.BotwoonHeadClosedUpRight },
        { BotwoonHeadFrame.Closed(CompassDirection.Up), EnemyFrameId.BotwoonHeadClosedUp },
        { BotwoonHeadFrame.Open(CompassDirection.UpLeft), EnemyFrameId.BotwoonHeadOpenUpLeft },
        { BotwoonHeadFrame.Open(CompassDirection.Left), EnemyFrameId.BotwoonHeadOpenLeft },
        { BotwoonHeadFrame.Open(CompassDirection.DownLeft), EnemyFrameId.BotwoonHeadOpenDownLeft },
        { BotwoonHeadFrame.Open(CompassDirection.Down), EnemyFrameId.BotwoonHeadOpenDown },
        { BotwoonHeadFrame.Open(CompassDirection.DownRight), EnemyFrameId.BotwoonHeadOpenDownRight },
        { BotwoonHeadFrame.Open(CompassDirection.Right), EnemyFrameId.BotwoonHeadOpenRight },
        { BotwoonHeadFrame.Open(CompassDirection.UpRight), EnemyFrameId.BotwoonHeadOpenUpRight },
        { BotwoonHeadFrame.Open(CompassDirection.Up), EnemyFrameId.BotwoonHeadOpenUp },
    };
}
