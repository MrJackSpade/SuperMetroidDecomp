using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

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
        var bytes = new byte[XrayTilemapLayout.BufferWords * 2];
        ISnesMutableMemory memory = MutableMemory;
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = memory.ReadWorkRamByte(XraySetupMemory.SavedBg1 + i);
        captured.LoadBytes(SnesPpuLayout.GameplayBg1TilemapWord * 2, bytes);
        ushort x = BackgroundScroll.Layer1XPosition;
        ushort y = BackgroundScroll.Layer1YPosition;
        var map = XrayRevealTilemap.Build(level, captured,
            unchecked((ushort)(x + BackgroundScroll.Bg1XOffset)),
            unchecked((ushort)(y + BackgroundScroll.Bg1YOffset)), x, y,
            (byte)room.AreaIndex, XrayRevealVisuals);
        XrayRevealOverlays.Apply(level, map, Plms.Collectibles, System,
            room.State.XrayPointer, x, y, XrayRevealVisuals);
        for (int i = 0; i < map.Length; i++) WriteXrayWord(XraySetupMemory.RevealTilemap + i * 2, map[i]);
    }

    /// <summary>Completes the previous setup call's VRAM read after NMI's video writes.</summary>
    private void TransferXrayBg1Read()
    {
        if (Samus?.Xray is not { IsActive: true } xray) return;
        // SetupStage already names the NEXT main-thread call. Calls two and three enqueue
        // reads; the following NMI performs them before call four consumes the captured
        // pages. In particular, queued writes must win first.
        (int Source, int Destination)? read = xray.SetupStage switch
        {
            XraySetupStage.ReadBg1FirstScreen => (
                SnesPpuLayout.GameplayBg1TilemapWord + XrayTilemapLayout.ScreenWords,
                XraySetupMemory.SavedBg1SecondScreen),
            XraySetupStage.BuildRevealReadBg2FirstScreen => (
                SnesPpuLayout.GameplayBg1TilemapWord, XraySetupMemory.SavedBg1),
            XraySetupStage.Complete or XraySetupStage.FreezeTimeBackupBg2Registers or
                XraySetupStage.ReadBg1SecondScreen or XraySetupStage.ReadBg2SecondScreen or
                XraySetupStage.TransferRevealFirstScreen or XraySetupStage.InitializeTransferRevealSecondScreen or
                XraySetupStage.BackdropColor => null,
            _ => throw new InvalidOperationException($"Undefined X-ray setup stage {xray.SetupStage}."),
        };
        if (read is not (int source, int destination)) return;
        for (int i = 0; i < XrayTilemapLayout.ScreenWords; i++)
            WriteXrayWord(destination + i * 2, Vram.ReadWord(source + i));
    }

    private void WriteXrayWord(int address, ushort value)
    {
        _addressSpace.WriteByte(address, (byte)value);
        _addressSpace.WriteByte(address + 1, (byte)(value >> 8));
    }
}
