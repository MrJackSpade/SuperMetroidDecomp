using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Room-integrated regression for Mother Brain's shared FX, OAM and BG2 ownership.</summary>
internal static class MotherBrainRoomAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var brain = runtime.Enemies.MotherBrain!;
        var acidHeights = new HashSet<ushort>();
        for (int frame = 0; frame < 800; frame++)
        {
            runtime.StepFrame(0);
            acidHeights.Add(runtime.RoomLayer3Fx.CurrentYPosition);
        }
        Console.WriteLine($"Initial acid: {runtime.RoomLayer3Fx.CurrentYPosition}, target={runtime.RoomLayer3Fx.TargetYPosition}");
        if (runtime.RoomLayer3Fx.CurrentYPosition != 184 || acidHeights.Count < 40)
            throw new InvalidDataException("Mother Brain acid did not rise gradually from 232 to 184.");
        Directory.CreateDirectory("csharp/test-temp/mother-brain-room");
        PngWriter.WriteRgba("csharp/test-temp/mother-brain-room/acid.png", 256, 224,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
        runtime.System.SetEvent(EventNumber.MotherBrainGlassDestroyed);
        brain.Head!.Health = 0;
        runtime.Samus!.XPosition = 128;
        runtime.Samus.YPosition = 160;
        int postureTransitions = 0;
        var previousPose = brain.Pose;
        for (int frame = 0; frame < 4800; frame++)
        {
            bool expectedOverride = brain.HasBg2ScrollOverride;
            ushort expectedDisplayedY = brain.Bg2YScroll;
            runtime.StepFrame(0);
            if (brain.Pose == MotherBrainBodyPose.Standing && previousPose == MotherBrainBodyPose.CrouchingTransition)
                postureTransitions++;
            previousPose = brain.Pose;
            if (expectedOverride &&
                (runtime.DisplayedGameplayPpu.Bg2VerticalScroll != expectedDisplayedY ||
                 unchecked((ushort)(brain.Body.YPosition + brain.Bg2YScroll)) != 62 &&
                 unchecked((ushort)(brain.Body.YPosition + brain.Bg2YScroll)) != 63))
                throw new InvalidDataException($"Body/posture/display lost alignment on frame {frame}: body={brain.Body.YPosition}, scroll={brain.Bg2YScroll}, display={runtime.DisplayedGameplayPpu.Bg2VerticalScroll}.");
            if (frame % 400 == 0)
                Console.WriteLine($"{frame}: {brain.Function}, bodyY={brain.Body.YPosition}, scroll={brain.Bg2YScroll}/{runtime.DisplayedGameplayPpu.Bg2VerticalScroll}, rinkas={runtime.Enemies.RinkaTerminationFlag}");
        }
        if (postureTransitions < 3 || runtime.RoomLayer3Fx.CurrentYPosition != 232)
            throw new InvalidDataException($"Expected repeated stand cycles and lowered acid: cycles={postureTransitions}, acid={runtime.RoomLayer3Fx.CurrentYPosition}.");
        Directory.CreateDirectory("csharp/test-temp/mother-brain-room");
        PngWriter.WriteRgba("csharp/test-temp/mother-brain-room/battle.png", 256, 224,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
        if (runtime.BackgroundScroll.Bg2VerticalScroll != brain.Bg2YScroll)
            throw new InvalidDataException("Mother Brain body BG2 register was overwritten before display.");
        if (runtime.Enemies.RinkaTerminationFlag != 1 || runtime.Enemies.Slots.Any(
                slot => runtime.Enemies.RinkaStates[slot.SlotIndex] is not null && !slot.Properties.HasAny(EnemyProperties.Deleted)))
            throw new InvalidDataException("Phase-one Rinkas survived the phase-two graphics replacement.");
        var capture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var registers = ((OrdinaryGameplayRenderLayer)capture.Layers[0]).Registers;
        if (registers.Bg2WidthTiles != 32 || registers.Bg2HeightTiles != 32)
            throw new InvalidDataException("Mother Brain body must use the native single BG2 page.");
        _ = SoftwareLayeredSnapshotRenderer.Render(capture);
        Console.WriteLine($"Mother Brain room passed: {acidHeights.Count} acid heights, {postureTransitions} stand transitions, correct BG2 capture and retired Rinkas.");
        return 0;
    }
}
