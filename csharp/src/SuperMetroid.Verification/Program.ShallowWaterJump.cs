using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyShallowWaterJump(bool exportOnly = false)
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa408, cameraY: 256);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = 104; samus.YPosition = 427;
        samus.Kinematics.YSubposition = 0xffff;
        samus.EquippedItems = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        AssertEqual(447, samus.Kinematics.BottomPixel, "standing feet occupy the pixel below the water surface");
        AssertEqual(SamusLiquidPhysicsState.Water, samus.LiquidPhysics.LiquidPhysicsType, "standing water contact is preserved");
        var level = runtime.LevelData!;
        const string output = "csharp/test-temp/issue-1258-water-jump";
        Directory.CreateDirectory(output);
        using (var writer = new BinaryWriter(File.Create(Path.Combine(output, "room.bin"))))
        {
            writer.Write((ushort)level.WidthInBlocks); writer.Write((ushort)level.HeightInBlocks);
            foreach (ushort tile in level.ForegroundEntries.Span) writer.Write(tile);
            writer.Write(level.BehaviorBytes.Span);
        }
        File.WriteAllText(Path.Combine(output, "seed.txt"),
            $"{samus.XPosition:X4} {samus.Kinematics.XSubposition:X4} {samus.YPosition:X4} {samus.Kinematics.YSubposition:X4} {samus.Pose:X4} {samus.AnimationFrame:X4} {samus.AnimationFrameTimer:X4} {samus.AnimationFrameBuffer:X4} {samus.LiquidPhysics.FxYPosition:X4} {samus.LiquidPhysics.LiquidOptions:X4} {samus.LiquidPhysics.LiquidPhysicsType:X4}\n");
        Console.WriteLine($"Takeoff: room={runtime.ActiveRoom!.Pointer:X4}, position={samus.XPosition}/{samus.YPosition}.{samus.Kinematics.YSubposition:X4}, floor={level.GetCollisionBlock(6,28).LevelWord:X4}, surface={samus.LiquidPhysics.FxYPosition}, bottom={samus.Kinematics.BottomPixel}, medium={samus.LiquidPhysics.LiquidPhysicsType}.");
        var rows = new List<string> { "frame,pose,y,yspeed,ydir,radius,medium" };
        uint apex = samus.Kinematics.YFixed;
        for (int frame = 0; frame < 60; frame++)
        {
            runtime.StepFrame((ushort)SnesButton.A);
            apex = Math.Min(apex, samus.Kinematics.YFixed);
            rows.Add($"{frame},{samus.Pose:X2},{samus.Kinematics.YFixed:X8},{samus.Kinematics.VerticalSpeedFixed:X8},{samus.Kinematics.YDirection},{samus.Kinematics.YRadius},{samus.LiquidPhysics.LiquidPhysicsType}");
        }
        File.WriteAllLines(Path.Combine(output, "managed.csv"), rows);
        Console.WriteLine($"Jump: {rows[1]}; apex={apex:X8}");
        if (exportOnly) return;
        AssertEqual(0x01530000u, apex, "cartridge jump reaches the room ceiling at Y=339");
        string nativePath = "csharp/test-fixtures/issue-1258-water-jump/native.csv";
        string[] native = File.ReadAllLines(nativePath);
        AssertSequenceEqual(native, rows, "reported shallow-water jump matches native pose, position, velocity, radius and medium on every frame");
    }
}