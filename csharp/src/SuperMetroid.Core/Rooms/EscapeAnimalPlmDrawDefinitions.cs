namespace SuperMetroid.Core.Rooms;

/// <summary>Three-block vertical rescue-wall mutations, independent of editable art.</summary>
internal static class EscapeAnimalPlmDrawDefinitions
{
    /// <summary>Maps one of four rescue-wall draw identities to three vertical solid blocks, or rejects another pointer.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        var list = (EscapeAnimalDraw)pointer;
        if (!Enum.IsDefined(list))
        {
            draw = default;
            return false;
        }
        ushort word = list switch
        {
            EscapeAnimalDraw.Frame0 => 0x8053,
            EscapeAnimalDraw.Frame1 => 0x8054,
            EscapeAnimalDraw.Frame2 => 0x8055,
            EscapeAnimalDraw.Blank => 0x80ff,
            _ => throw new InvalidOperationException($"Undefined EscapeAnimalDraw {list}."),
        };
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x8003, new ushort[] { word, word, word }, new(0, 0)),
        });
        return true;
    }
}
