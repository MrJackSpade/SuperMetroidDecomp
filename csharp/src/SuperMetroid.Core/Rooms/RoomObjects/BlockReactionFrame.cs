namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One image of a block reaction (<see cref="BlockReactionChange"/>): what it draws at the reaction's block, or the
/// block's saved terrain restored, how many updates it stays, and the sound that comes with it.
/// </summary>
/// <param name="Draw">What the image draws; null when the image restores the block's saved terrain.</param>
/// <param name="Updates">The updates the image stays before the next image, or before the reaction ends.</param>
/// <param name="SoundBefore">The sound requested as the image shows; null for none.</param>
internal readonly record struct BlockReactionFrame(
    RoomObjectDraw? Draw,
    int Updates,
    SoundEffectId? SoundBefore = null)
{
    /// <summary>How crowded the sound's queue may be for the image's request to join it; a normal request unless the image names another.</summary>
    public SoundRequestPriority SoundPriority { get; init; } = SoundRequestPriority.Normal;

    /// <summary>The block's saved terrain, restored for one update as a respawning block grows back.</summary>
    internal static BlockReactionFrame RestoredBlock { get; } = new(null, 1);
}
