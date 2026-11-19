using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One trail a shot leaves behind it: a pair of sparkles, each running its own animation, shown in the pictures of the
/// beams of the shot that left it. It lasts until both sparkles' animations have ended.
/// </summary>
public sealed class SamusProjectileTrail
{
    /// <summary>Starts a trail of two sparkles that have run no animation yet.</summary>
    /// <param name="picturedBeams">The beam combination whose pictures both sparkles show (#1586).</param>
    /// <param name="origin">The place of the shot that left the trail, which both sparkles are placed from.</param>
    internal SamusProjectileTrail(SamusBeamCombination picturedBeams, Position16<RoomPixels> origin)
    {
        PicturedBeams = picturedBeams;
        Origin = origin;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    /// <summary>The left sparkle.</summary>
    public SamusProjectileTrailSide Left { get; }

    /// <summary>The right sparkle.</summary>
    public SamusProjectileTrailSide Right { get; }

    /// <summary>The beam combination whose pictures both sparkles show (#1586): that of the shot that left the trail.</summary>
    public SamusBeamCombination PicturedBeams { get; }

    /// <summary>
    /// The place of the shot that left the trail, where it stood when it left it: both sparkles stand from it by their
    /// own <see cref="SamusProjectileTrailSide.Offset"/>.
    /// </summary>
    public Position16<RoomPixels> Origin { get; }

    /// <summary>Whether both sparkles' animations have ended, so the trail shows nothing more.</summary>
    internal bool IsFinished => Left.HasEnded && Right.HasEnded;

    /// <summary>
    /// Where <paramref name="side"/> shows on the screen whose top-left corner is <paramref name="camera"/>: the shot's
    /// place on it, moved by the side's displayed offset. A sparkle beside a shot that has left the view's top or left
    /// edge lies off the screen there, as the shot does.
    /// </summary>
    internal Offset16<ScreenPixels> PlaceOnScreen(SamusProjectileTrailSide side, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(side);
        Offset16<ScreenPixels> shot = Origin.RelativeToCamera(camera);
        return Offset16<ScreenPixels>.Of(shot.X + side.DisplayedOffset.X, shot.Y + side.DisplayedOffset.Y);
    }
}
