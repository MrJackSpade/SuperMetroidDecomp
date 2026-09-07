using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyXraySetupBuffers()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom);
        var samus = runtime.Samus!;
        var support = runtime.Enemies.Slots[1];
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = support.XPosition;
        samus.YPosition = (ushort)(support.YPosition - support.YRadius - samus.Kinematics.YRadius);
        runtime.StepFrame(0);
        AssertTrue(samus.Xray.TryBegin(bus, samus, samus.ReadMovementType(bus)), "setup fixture activates X-ray");
        ushort held = runtime.ControllerBindings.Dash;
        runtime.StepFrame(held); // Stage one, before either BG1 page is captured.
        var first = Enumerable.Repeat((ushort)0x0123, XrayTilemapLayout.ScreenWords).ToArray();
        var second = Enumerable.Repeat((ushort)0x0456, XrayTilemapLayout.ScreenWords).ToArray();
        runtime.Vram.ExecuteWordTransfer(second,
            SnesPpuLayout.GameplayBg1TilemapWord + XrayTilemapLayout.ScreenWords, 1);
        runtime.StepFrame(held);
        VerifySaved(XraySetupMemory.SavedBg1SecondScreen, new ushort[second.Length], "stage two only queues the second page read");
        // The source is sampled at the NEXT NMI, not when the request is issued.
        Array.Fill(second, (ushort)0x0789);
        runtime.Vram.ExecuteWordTransfer(second,
            SnesPpuLayout.GameplayBg1TilemapWord + XrayTilemapLayout.ScreenWords, 1);
        runtime.Vram.ExecuteWordTransfer(first, SnesPpuLayout.GameplayBg1TilemapWord, 1);
        runtime.StepFrame(held);
        VerifySaved(XraySetupMemory.SavedBg1SecondScreen, second, "stage-two read completes at the next NMI");

        // Deliberately destroy the live source before stage four. A render-time rebuild
        // or a single late VRAM snapshot cannot pass this page-ownership assertion.
        runtime.Vram.ExecuteWordTransfer(new ushort[XrayTilemapLayout.ScreenWords],
            SnesPpuLayout.GameplayBg1TilemapWord + XrayTilemapLayout.ScreenWords, 1);
        int source = SamusXrayRomData.Palette.VisorWords;
        first[0] = (ushort)(bus.ReadByte(source) | bus.ReadByte(source + 1) << 8);
        runtime.VramWrites.Enqueue(sizeInBytes: 2, sourceAddress: source,
            encodedVramDestination: SnesPpuLayout.GameplayBg1TilemapWord);
        var captured = new SnesVram();
        captured.ExecuteWordTransfer(first.Concat(second).ToArray(), SnesPpuLayout.GameplayBg1TilemapWord, 1);
        var scroll = runtime.BackgroundScroll;
        var room = runtime.ActiveRoom!;
        var expected = XrayRevealTilemap.Build(bus, runtime.LevelData!, captured,
            unchecked((ushort)(scroll.Layer1XPosition + scroll.Bg1XOffset)),
            unchecked((ushort)(scroll.Layer1YPosition + scroll.Bg1YOffset)),
            scroll.Layer1XPosition, scroll.Layer1YPosition, (byte)room.AreaIndex);
        XrayRevealOverlays.Apply(bus, runtime.LevelData!, expected, runtime.Plms.Collectibles, runtime.System,
            room.State.XrayPointer, scroll.Layer1XPosition, scroll.Layer1YPosition);
        runtime.StepFrame(held);
        VerifySaved(XraySetupMemory.SavedBg1, first, "stage-three read completes before stage-four build");
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(bus)), "stage four builds from the separately captured pages and overlays");
        AssertTrue(expected.Any(word => word == first[1] || word == second[0]), "fixture observes captured BG1 content, not only reveal replacements");
        runtime.Vram.ExecuteWordTransfer(new ushort[XrayTilemapLayout.BufferWords], SnesPpuLayout.GameplayBg1TilemapWord, 1);
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(held);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(bus)), "widening/full beam never rebuilds the frozen reveal map");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "setup fixture tears down normally");
        Console.WriteLine("  X-ray setup: separate BG1 capture calls, frozen WRAM reveal/overlays and full-beam lifetime agree.");

        void VerifySaved(int address, ushort[] words, string context)
        {
            for (int i = 0; i < words.Length; i++)
                AssertEqual(words[i], (ushort)(bus.ReadByte(address + i * 2) | bus.ReadByte(address + i * 2 + 1) << 8), $"{context}, word {i}");
        }
    }
}
