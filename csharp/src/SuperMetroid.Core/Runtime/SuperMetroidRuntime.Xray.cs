using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Installed X-ray art, rebound by the host after debugger-state restore.</summary>
    [field: NonSerialized]
    public XrayRevealVisualCatalog? XrayRevealVisuals { get; set; }

    /// <summary>
    /// Setup call four's reveal construction (<c>$91:CB8E</c>), run before the setup stage
    /// advances. WRAM owns these buffers, so state saves retain the exact captured map and
    /// rendering never reruns game logic to synthesize a replacement.
    /// </summary>
    private void BuildXrayRevealTilemap()
    {
        var room = ActiveRoom ?? throw new InvalidOperationException("X-ray setup has no room.");
        var level = LevelData ?? throw new InvalidOperationException("X-ray setup has no level data.");
        // The builder reads only BG1 tilemap words. Feed the two captured pages,
        // not current VRAM: each page was read on a different setup call.
        var captured = new SnesVram();
        var xrayMemory = new XrayMemory(MemoryBus);
        ushort[] saved = xrayMemory.SavedBg1.ToArray();
        var bytes = new byte[saved.Length * 2];
        for (int i = 0; i < saved.Length; i++)
        {
            bytes[i * 2] = (byte)saved[i];
            bytes[i * 2 + 1] = (byte)(saved[i] >> 8);
        }
        captured.LoadBytes(GameplayVramLayout.Bg1TilemapWord * 2, bytes);
        Position16<RoomPixels> layer1 = BackgroundScroll.Layer1Position;
        Position16<RoomPixels> bg1Scroll = layer1 + BackgroundScroll.Bg1Offset;
        var map = XrayRevealTilemap.Build(level, captured, bg1Scroll, layer1,
            (byte)room.AreaIndex, XrayRevealVisuals);
        XrayRevealOverlays.Apply(level, map, Plms.Collectibles, System,
            room.State.XrayPointer, layer1, XrayRevealVisuals);
        WorkRamWordTable reveal = xrayMemory.RevealTilemap;
        for (int i = 0; i < map.Length; i++) reveal[i] = map[i];
    }

    /// <summary>Completes the previous setup call's VRAM read after NMI's video writes.</summary>
    private void TransferXrayBg1Read()
    {
        if (Samus?.Xray is not { IsActive: true } xray) return;
        // SetupStage already names the NEXT main-thread call. Calls two and three enqueue
        // reads; the following NMI performs them before call four consumes the captured
        // pages. In particular, queued writes must win first.
        (int Source, int SavedScreen)? read = xray.SetupStage switch
        {
            XraySetupStage.ReadBg1FirstScreen => (
                GameplayVramLayout.Bg1TilemapWord + XrayTilemapLayout.ScreenWords, 1),
            XraySetupStage.BuildRevealReadBg2FirstScreen => (
                GameplayVramLayout.Bg1TilemapWord, 0),
            XraySetupStage.Complete or XraySetupStage.FreezeTimeBackupBg2Registers or
                XraySetupStage.ReadBg1SecondScreen or XraySetupStage.ReadBg2SecondScreen or
                XraySetupStage.TransferRevealFirstScreen or XraySetupStage.InitializeTransferRevealSecondScreen or
                XraySetupStage.BackdropColor => null,
            _ => throw new InvalidOperationException($"Undefined X-ray setup stage {xray.SetupStage}."),
        };
        if (read is not (int source, int savedScreen)) return;
        WorkRamWordTable saved = new XrayMemory(MemoryBus).SavedBg1;
        int firstWord = savedScreen * XrayTilemapLayout.ScreenWords;
        for (int i = 0; i < XrayTilemapLayout.ScreenWords; i++)
            saved[firstWord + i] = Vram.ReadWord(source + i);
    }
}
