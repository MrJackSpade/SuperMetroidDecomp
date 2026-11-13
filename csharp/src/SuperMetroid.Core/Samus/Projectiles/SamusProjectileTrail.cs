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
    internal SamusProjectileTrail(SamusBeamCombination picturedBeams)
    {
        PicturedBeams = picturedBeams;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    /// <summary>The left sparkle.</summary>
    public SamusProjectileTrailSide Left { get; }

    /// <summary>The right sparkle.</summary>
    public SamusProjectileTrailSide Right { get; }

    /// <summary>The beam combination whose pictures both sparkles show (#1586): that of the shot that left the trail.</summary>
    public SamusBeamCombination PicturedBeams { get; }

    /// <summary>Whether both sparkles' animations have ended, so the trail shows nothing more.</summary>
    internal bool IsFinished => Left.FrameHold == 0 && Right.FrameHold == 0;
}
