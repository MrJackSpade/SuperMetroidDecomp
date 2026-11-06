namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Displayed swing-frame selection only; physical body placement remains compiled separately.</summary>
public sealed class InstalledGrappleSwingFrames
{
    /// <summary>Copied edited angle mapping; null retains the compiled nearest-eight-angle rule.</summary>
    internal readonly int[]? frames;
    /// <summary>Stores a separate frame table only when an edited angle the import pipeline validated differs from the stock selector.</summary>
    internal InstalledGrappleSwingFrames(int[] frames)
    {
        for (int angle = 0; angle < frames.Length; angle++)
            if (frames[angle] != GrappleSwingFrameDefinitions.FrameForAngle(new ByteAngle(angle)))
            { this.frames = frames; return; }
    }
    /// <summary>Selects the editable displayed orientation frame for one high angle byte, independently of the compiled body-placement frame and the runtime's separate straight-down animation handling.</summary>
    /// <param name="angle">High byte of the mirrored swing angle, representing one of 256 positions around a full turn.</param>
    /// <returns>A displayed orientation frame index from 0 through 31, within the current facing's swing artwork.</returns>
    public int Resolve(ByteAngle angle) => frames is null
        ? GrappleSwingFrameDefinitions.FrameForAngle(angle) : frames[angle.RawValue];
}
