namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What one image of a block reaction shows (#2558): a draw at the reaction's block, or the block's saved terrain
/// restored. Neither stands for the other: a restore draws nothing of its own.
/// </summary>
internal abstract record BlockReactionImage
{
    /// <summary>Only the two cases below are images.</summary>
    private BlockReactionImage()
    {
    }

    /// <summary>The block's saved terrain, restored.</summary>
    internal static BlockReactionImage SavedTerrain { get; } = new RestoresSavedTerrain();

    /// <summary>The image draws <paramref name="draw"/> at the reaction's block.</summary>
    /// <param name="draw">What the image draws.</param>
    public static implicit operator BlockReactionImage(RoomObjectDraw draw) => new Drawn(draw);

    /// <summary>The image draws <paramref name="Draw"/> at the reaction's block.</summary>
    /// <param name="Draw">What the image draws.</param>
    internal sealed record Drawn(RoomObjectDraw Draw) : BlockReactionImage;

    /// <summary>The image restores the block's saved terrain.</summary>
    internal sealed record RestoresSavedTerrain : BlockReactionImage;
}
