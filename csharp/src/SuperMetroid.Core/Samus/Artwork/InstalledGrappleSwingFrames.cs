namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Displayed swing-frame selection only, as installed (#1596); physical body placement remains compiled separately.</summary>
public sealed class InstalledGrappleSwingFrames
{
    /// <summary>How many high angle bytes a full turn holds, each with its own displayed frame.</summary>
    private const int AngleCount = 256;

    /// <summary>The installed displayed frame of every high angle byte.</summary>
    private readonly int[] frames;

    /// <summary>Copies the displayed frame of every angle the import pipeline validated.</summary>
    /// <param name="frames">One displayed frame for each of the 256 high angle bytes.</param>
    /// <exception cref="ArgumentException">An angle has no displayed frame, or a frame names no swing pose.</exception>
    internal InstalledGrappleSwingFrames(int[] frames)
    {
        Ensure.NotNull(frames);
        if (frames.Length != AngleCount || frames.Any(frame => frame is < 0 or >= GrappleSwingFrameDefinitions.FrameCount))
            throw new ArgumentException($"Grapple swing frames need one of {GrappleSwingFrameDefinitions.FrameCount} frames for each of {AngleCount} angles.", nameof(frames));
        this.frames = [.. frames];
    }

    /// <summary>Selects the editable displayed orientation frame for one high angle byte, independently of the compiled body-placement frame and the runtime's separate straight-down animation handling.</summary>
    /// <param name="angle">High byte of the mirrored swing angle, representing one of 256 positions around a full turn.</param>
    /// <returns>A displayed orientation frame index from 0 through 31, within the current facing's swing artwork.</returns>
    public int Resolve(TrigonometryAngle angle) => frames[angle.RawValue];
}
