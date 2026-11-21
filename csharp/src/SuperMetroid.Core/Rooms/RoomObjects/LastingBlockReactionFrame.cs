namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One image of a block reaction that restores no block (<see cref="LastingBlockReactionChange"/>): what it draws at the
/// reaction's block, how many updates it stays, and the sound that comes with it.
/// </summary>
/// <param name="Draw">What the image draws at the reaction's block.</param>
/// <param name="Updates">The updates the image stays before the next image, or before the reaction ends.</param>
/// <param name="SoundBefore">The sound requested as the image shows; null for none.</param>
internal readonly record struct LastingBlockReactionFrame(
    RoomObjectDraw Draw,
    int Updates,
    SoundEffectId? SoundBefore = null)
{
    /// <summary>How crowded the sound's queue may be for the image's request to join it; a normal request unless the image names another.</summary>
    public SoundRequestPriority SoundPriority { get; init; } = SoundRequestPriority.Normal;

    /// <summary>The image as the room's block reactions record it shown (<see cref="BlockReactionImage"/>).</summary>
    internal BlockReactionImage Image { get; } = new BlockReactionImage.Drawn(Draw);
}
