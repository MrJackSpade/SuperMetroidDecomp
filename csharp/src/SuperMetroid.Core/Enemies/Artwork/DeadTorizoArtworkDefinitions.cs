namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The dead Torizo's artwork the game draws besides its ordinary frames.</summary>
internal static class DeadTorizoArtworkDefinitions
{
    /// <summary>The frame of the corpse's sand heap, drawn after the room's enemies.</summary>
    internal const EnemyFrameId SandHeapFrame = EnemyFrameId.DeadTorizoRubbleMound;

    /// <summary>Where in the room the sand heap's frame is drawn, below the corpse.</summary>
    internal static readonly Position16<RoomPixels> SandHeapPlace = new(296, 187);
}
