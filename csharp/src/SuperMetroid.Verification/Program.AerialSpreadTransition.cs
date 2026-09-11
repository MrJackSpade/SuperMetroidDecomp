using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Issue 415 down-aim turnaround comparison: earn charge, jump, aim down,
    /// turn and sweep the morph press. Exclusive-create keeps earlier evidence intact.
    /// No pose, charge or animation state is forced after the initial fixture setup.
    /// </summary>
    private static void VerifyAerialSpreadTransitions(string? tracePath = null, string? outputPath = null)
    {
        using var output = outputPath is null ? null : new StreamWriter(new FileStream(outputPath, FileMode.CreateNew));
        using var native = tracePath is null ? null : File.OpenText(tracePath);
        const string header = "left,delay,frame,input,pose,x,xsub,y,ysub,charge,spread,bombs";
        output?.WriteLine(header);
        if (native is not null) AssertEqual(header, native.ReadLine(), "aerial native trace header");
        foreach (bool left in new[] { false, true })
        for (int delay = 0; delay < 10; delay++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            var level = runtime.LevelData!;
            // Match the grounded technique clearing, extending it upward for the jump.
            for (int y = 16; y < 36; y++)
            for (int x = 24; x < 40; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                    y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
                level.SetBehavior(index, 0);
            }
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            samus.XPosition = 512;
            samus.YPosition = 490;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            for (int frame = 0; frame < 125; frame++)
            {
                ushort input = runtime.ControllerBindings.Shoot;
                if (frame >= 70) input |= runtime.ControllerBindings.Jump;
                if (frame >= 76 && frame < 105 && frame != 78 + delay)
                    input |= (ushort)SnesButton.Down;
                if (frame >= 78) input |= (ushort)(left ? SnesButton.Right : SnesButton.Left);
                runtime.StepFrame(input);
                string row = $"{(left ? 1 : 0)},{delay},{frame},{input:X4},{samus.Pose:X4},{samus.XPosition:X4},{samus.Kinematics.XSubposition:X4},{samus.YPosition:X4},{samus.Kinematics.YSubposition:X4},{samus.ProjectileFlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}";
                output?.WriteLine(row);
                if (native is not null) AssertEqual(native.ReadLine(), row, $"native aerial transition left={left}, delay={delay}, frame={frame}");
                AssertEqual(samus.ProjectileFlareCounter, runtime.Projectiles.FlareCounter, "aerial charge mirror");
                bool releasedSpread = delay == 6 && frame >= 105;
                AssertEqual((ushort)(releasedSpread ? 5 : 0), runtime.BombProjectiles.BombCounter,
                    $"only native one-frame window produces spread: left={left}, delay={delay}, frame={frame}");
                if (delay == 6 && frame is >= 92 and <= 104)
                {
                    AssertEqual((ushort)80, samus.ProjectileFlareCounter, "airborne morph preserves earned charge while Down is held");
                    AssertEqual((ushort)(frame - 91), samus.BombSpreadChargeTimeoutCounter, "airborne hold advances after morph completes");
                }
                if (releasedSpread)
                    AssertEqual((ushort)0, samus.ProjectileFlareCounter, "airborne spread consumes charge");
            }
        }
        if (native is not null) AssertTrue(native.ReadLine() is null, "aerial native trace fully consumed");
        Console.WriteLine("Aerial down-aim spread: both directions and ten neighboring input timings verified (2500 frames). Walljump route remains separate.");
    }
}
