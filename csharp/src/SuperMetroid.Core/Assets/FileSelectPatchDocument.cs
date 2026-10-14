namespace SuperMetroid.Core.Assets;

/// <summary>Authored sparse tilemap patch whose cells are positioned relative to a slot-field anchor.</summary>
public sealed record FileSelectPatchDocument
{
    /// <summary>Nonempty caller-owned cell array; coordinates must be unique and the placed patch must fit its page.</summary>
    public required FileSelectPatchCellDocument[] Cells { get; init; }
}
