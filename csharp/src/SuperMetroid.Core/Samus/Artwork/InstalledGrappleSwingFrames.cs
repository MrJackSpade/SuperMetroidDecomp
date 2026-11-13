using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Displayed swing-frame selection only, as installed (#1596); physical body placement remains compiled separately.</summary>
public sealed class InstalledGrappleSwingFrames
{
    /// <summary>The installed displayed frame of every swing direction, in direction order.</summary>
    private readonly int[] frames;

    /// <summary>Copies the displayed frame of every swing direction the import pipeline validated.</summary>
    /// <param name="frames">One displayed frame for each <see cref="GrappleSwingDirection"/>, in direction order.</param>
    /// <exception cref="ArgumentException">A direction has no displayed frame, or a frame names no swing pose.</exception>
    internal InstalledGrappleSwingFrames(int[] frames)
    {
        Ensure.NotNull(frames);
        if (frames.Length != GrappleSwingDirection.Count || frames.Any(frame => frame is < 0 or >= GrappleSwingFrameDefinitions.FrameCount))
            throw new ArgumentException($"Grapple swing frames need one of {GrappleSwingFrameDefinitions.FrameCount} frames for each of {GrappleSwingDirection.Count} swing directions.", nameof(frames));
        this.frames = [.. frames];
    }

    /// <summary>Selects the editable displayed orientation frame for one swing direction, independently of the compiled body-placement offset and the runtime's separate straight-down animation handling.</summary>
    /// <param name="direction">The swing direction nearest the mirrored rope angle.</param>
    /// <returns>A displayed orientation frame index from 0 through 31, within the current facing's swing artwork.</returns>
    public int Resolve(GrappleSwingDirection direction) => frames[direction.Number];
}
