using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
internal static class GunshipEscapeTimerVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    internal static int VerifyGunshipEscapeTimer()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var maps = InstalledFixtures.Maps;
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(0x91f8);
        runtime.Room.Camera!.SetPosition(new Position16<RoomPixels>(0x0400, 0x0400));
        var samus = runtime.Player.Samus!;
        var top = runtime.Enemies.SlotTable.Slots.First(slot => slot.Definition.InitializationAiPointer == 0xa644);
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(top.Position.X.Whole, (ushort)(top.Position.Y.Whole - 30)));
        samus.Presentation.InitializeAnimation(bus);
        samus.Presentation.PrimeGraphics(bus);
        typeof(EscapeTimer).GetProperty(nameof(EscapeTimer.RawStatus))!.SetValue(runtime.EscapeTimer, (ushort)0x8006);
        runtime.EscapeTimer.SetTime(2, 0x59, 0x50);
        runtime.QueueEscapeTimerSpriteTiles();
        runtime.StepFrame(0);
        var timerTilesBefore = runtime.Vram.Bytes.Slice(0xfc00, 0x400).ToArray();
        AssertTrue(!runtime.Enemies.GunshipHealthBehavior.HasGunshipHealthHandler, "Idle ship retains normal Samus handler");
        AssertTrue(TimerSpritesPresent(), "Ordinary escape timer remains visible before boarding");
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeY(0x0440);
        samus.Presentation.InitializeAnimation(bus);
        var timeBeforeBoarding = Time();
        runtime.StepFrame(0x0400);
        AssertEqual(GunshipFrameEvent.EntryStarted, runtime.Enemies.GunshipBehavior.LastGunshipEvent, "Down starts actual Landing Site boarding");
        AssertEqual(timeBeforeBoarding, Time(), "Boarding handler stops the timer on its installation frame");
        AssertTrue(!TimerSpritesPresent(), "Boarding removes timer OAM immediately");
        bool sawDustUpload = false;
        for (int frame = 0; frame < 500; frame++)
        {
            runtime.StepFrame(0);
            AssertTrue(runtime.EscapeTimer.IsActive, "Gunship preserves native active timer status");
            AssertEqual(timeBeforeBoarding, Time(), "Gunship handler omits timer countdown");
            AssertTrue(!TimerSpritesPresent(), "No timer sprites reference repurposed dust tiles");
            if (top.VariableF == 0xac1b)
            {
                sawDustUpload = true;
                break;
            }
        }
        AssertTrue(sawDustUpload, "Confirmation reaches the actual liftoff dust upload");
        runtime.StepFrame(0); // NMI consumes the final queued dust tile transfer.
        AssertTrue(!runtime.Vram.Bytes.Slice(0xfc00, 0x400).SequenceEqual(timerTilesBefore), "Liftoff actually repurposes the timer tiles");
        AssertTrue(!TimerSpritesPresent(), "No stale timer OAM remains after dust artwork reaches VRAM");
        AssertEqual(timeBeforeBoarding, Time(), "Liftoff keeps the timer suspended");
        Console.WriteLine("Gunship escape timer: normal timer OAM, same-frame boarding suppression, preserved active status and no timer sprites through liftoff dust upload passed.");
        return 0;

        (byte, byte, byte) Time() => (runtime.EscapeTimer.MinutesBcd, runtime.EscapeTimer.SecondsBcd, runtime.EscapeTimer.CentisecondsBcd);
        bool TimerSpritesPresent()
        {
            var expected = new OamBuffer();
            EscapeTimerRenderer.Draw(runtime.EscapeTimer, expected, maps.EscapeTimer);
            for (int offset = 0; offset < runtime.Oam.LastFinalizedSpriteCount * 4; offset += 4)
                for (int timerOffset = 0; timerOffset < expected.NextByteOffset; timerOffset += 4)
                    if (runtime.Oam.LowTable.Slice(offset, 4).SequenceEqual(expected.LowTable.Slice(timerOffset, 4))) return true;
            return false;
        }
    }

    [VerificationCommand("--gunship-escape-timer")]
    internal static int GunshipEscapeTimerCommand()
    {
        return VerifyGunshipEscapeTimer();
    }
}
