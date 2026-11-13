
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema for displayed Grapple swing orientations; physical body offsets, facing selection, and animation timing remain engine-owned.</summary>
public sealed record GrappleSwingFrameDocument
{
    /// <summary>Schema revision, which must equal <see cref="GrappleSwingFrameFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 32 orientation-frame indices in 0..31, one for each grapple swing direction in order from the angle zero, corresponding to visual selections.</summary>
    public required int[] Frames { get; init; }
}
