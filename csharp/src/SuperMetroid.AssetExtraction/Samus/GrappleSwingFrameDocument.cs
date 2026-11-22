
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema for displayed Grapple swing orientations; physical body offsets, facing selection, and animation timing remain engine-owned.</summary>
public sealed partial record GrappleSwingFrameDocument
{
    /// <summary>Schema revision, which must equal <see cref="GrappleSwingFrameFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 32 swing pictures, one for each grapple swing direction in order from the angle zero, each the number 0..31 of the swing direction the picture was drawn for.</summary>
    public required int[] Frames { get; init; }
}
