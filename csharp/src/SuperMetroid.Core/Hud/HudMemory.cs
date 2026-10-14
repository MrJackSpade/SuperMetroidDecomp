using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Hud;

/// <summary>Typed view of the gameplay HUD's WRAM tilemap buffer (#629).</summary>
[CpuBusBoundary("domain view: gameplay HUD WRAM")]
public readonly struct HudMemory(ISnesMemoryBus bus)
{
    /// <summary>The three mutable HUD rows, row-major.</summary>
    public WorkRamWordTable MutableTilemap => new(bus, HudWorkRam.MutableTilemap);

    /// <summary>Appends the queue entry that DMAs the whole mutable tilemap buffer to the HUD's mutable BG3 rows.</summary>
    public static void EnqueueMutableTilemapUpload(VramWriteQueue queue)
    {
        Ensure.NotNull(queue);
        queue.Enqueue(HudState.MutableByteCount, HudWorkRam.MutableTilemapAddress, RoomFxVram.HudMutableRowsDma);
    }
}
