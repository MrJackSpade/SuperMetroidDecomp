namespace SuperMetroid.Core.Rooms.Plms;
/// <summary>Verification access to <see cref="RoomPlmKraidVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmKraidVisualCatalogAccess
{
    /// <summary>Builds the Kraid room-art fixture from its explicit compiled visual parts.</summary>
    extension(RoomPlmKraidVisualCatalog)
    {
        /// <summary>Builds stock Kraid PLM art from the visual bits of its physical draw words.</summary>
        internal static RoomPlmKraidVisualCatalog Stock() => new(
            KraidRoomPlmDrawDefinitions.All.Select(draw =>
                new RoomPlmVisualEntry(
                    KraidRoomPlmDrawDefinitions.VisualId(draw.Draw),
                    draw.VisualWords())));
    }
}
