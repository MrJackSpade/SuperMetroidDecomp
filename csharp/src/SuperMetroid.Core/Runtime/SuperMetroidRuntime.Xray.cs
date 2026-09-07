using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>
    /// Executes the BG1 capture/reveal-building portion of native setup, before the
    /// setup counter advances. WRAM owns these buffers, so state saves retain the exact
    /// captured map and rendering never reruns game logic to synthesize a replacement.
    /// </summary>
    private void PrepareXrayTilemap(byte stage)
    {
        switch (stage)
        {
            case XraySetupMemory.BuildRevealStage:
                var room = ActiveRoom ?? throw new InvalidOperationException("X-ray setup has no room.");
                var level = LevelData ?? throw new InvalidOperationException("X-ray setup has no level data.");
                // The builder reads only BG1 tilemap words. Feed the two captured pages,
                // not current VRAM: each page was read on a different setup call.
                var captured = new SnesVram();
                var bytes = new byte[XrayTilemapLayout.BufferWords * 2];
                for (int i = 0; i < bytes.Length; i++) bytes[i] = _addressSpace.ReadByte(XraySetupMemory.SavedBg1 + i);
                captured.LoadBytes(SnesPpuLayout.GameplayBg1TilemapWord * 2, bytes);
                ushort x = BackgroundScroll.Layer1XPosition;
                ushort y = BackgroundScroll.Layer1YPosition;
                var map = XrayRevealTilemap.Build(_addressSpace, level, captured,
                    unchecked((ushort)(x + BackgroundScroll.Bg1XOffset)),
                    unchecked((ushort)(y + BackgroundScroll.Bg1YOffset)), x, y, (byte)room.AreaIndex);
                XrayRevealOverlays.Apply(_addressSpace, level, map, Plms.Collectibles, System,
                    room.State.XrayPointer, x, y);
                for (int i = 0; i < map.Length; i++) WriteXrayWord(XraySetupMemory.RevealTilemap + i * 2, map[i]);
                break;
        }
    }

    /// <summary>Completes the previous setup call's VRAM read after NMI's video writes.</summary>
    private void TransferXrayBg1Read()
    {
        if (Samus?.Xray is not { IsActive: true } xray) return;
        // SetupStage already points at the NEXT main-thread call. Native stages two
        // and three enqueue reads; the following NMI performs them before stage four
        // consumes the captured pages. In particular, queued writes must win first.
        int source, destination;
        switch (xray.SetupStage)
        {
            case XraySetupMemory.ReadSecondScreenStage + 1:
                source = SnesPpuLayout.GameplayBg1TilemapWord + XrayTilemapLayout.ScreenWords;
                destination = XraySetupMemory.SavedBg1SecondScreen;
                break;
            case XraySetupMemory.ReadFirstScreenStage + 1:
                source = SnesPpuLayout.GameplayBg1TilemapWord;
                destination = XraySetupMemory.SavedBg1;
                break;
            default:
                return;
        }
        for (int i = 0; i < XrayTilemapLayout.ScreenWords; i++)
            WriteXrayWord(destination + i * 2, Vram.ReadWord(source + i));
    }

    private void WriteXrayWord(int address, ushort value)
    {
        _addressSpace.WriteByte(address, (byte)value);
        _addressSpace.WriteByte(address + 1, (byte)(value >> 8));
    }
}
