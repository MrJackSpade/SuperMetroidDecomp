using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema for displayed Grapple swing orientations; physical body offsets, facing selection, and animation timing remain engine-owned.</summary>
public sealed record GrappleSwingFrameDocument
{
    /// <summary>Schema revision, which must equal <see cref="GrappleSwingFrameDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 orientation-frame indices in 0..31, indexed by the high byte of the mirrored swing angle, corresponding to visual selections.</summary>
    public required int[] Frames { get; init; }
}
