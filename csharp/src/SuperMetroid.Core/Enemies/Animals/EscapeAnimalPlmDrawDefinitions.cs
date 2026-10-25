using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Three-block vertical rescue-wall mutations, independent of editable art.</summary>
internal static class EscapeAnimalPlmDrawDefinitions
{
    /// <summary>Maps one of four rescue-wall draw identities to three vertical solid blocks, or rejects another family's draw.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (request is not PlmFrameDraw<EscapeAnimalDraw> { Frame: var list })
        {
            draw = default;
            return false;
        }
        int word = list switch
        {
            EscapeAnimalDraw.Frame0 => 0x8053,
            EscapeAnimalDraw.Frame1 => 0x8054,
            EscapeAnimalDraw.Frame2 => 0x8055,
            EscapeAnimalDraw.Blank => 0x80ff,
            _ => throw new InvalidOperationException($"Undefined EscapeAnimalDraw {list}."),
        };
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x8003, Enumerable.Repeat(checked((ushort)word), 3).ToArray(), new(0, 0)),
        });
        return true;
    }
}
