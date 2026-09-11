using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static class SamusEaterAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.VramWrites.DrainTo(runtime.Vram, bus);
        runtime.LoadCartridgeRoomForDebug(MawFloorContactData.Room, 256, 0);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.XPosition = 368;
        samus.YPosition = 219;
        samus.Health = samus.MaxHealth = 999;
        samus.EquippedItems = samus.EquippedBeams = 0;
        var level = runtime.LevelData!;
        var block = level.GetCollisionBlock(23, 14);
        Console.WriteLine($"Plant trigger: {block.LevelWord:X4}/{block.Behavior:X2}, radius={samus.Kinematics.YRadius}.");
        if (block.CollisionType != RoomCollisionType.SpecialAir || block.Behavior != 0x80)
            throw new InvalidDataException("Fixture does not touch a retail floor-plant trigger.");
        runtime.StepFrame(0);
        if (level.GetCollisionBlock(23, 14).CollisionType == RoomCollisionType.SpecialAir)
            throw new InvalidDataException("Samus Eater inside-block setup did not deactivate its trigger.");
        int soundCount = 0;
        for (int frame = 1; frame < 257; frame++)
        {
            runtime.StepFrame(frame >= 170 ? (ushort)SuperMetroid.Core.Input.SnesButton.Right : (ushort)0);
            int damageCalls = Enumerable.Range(0, 4).Sum(cycle =>
                (frame >= cycle * 40 + 15 ? 1 : 0) + (frame >= cycle * 40 + 35 ? 1 : 0));
            if (samus.Health != 999 - damageCalls * 2)
                throw new InvalidDataException($"Plant damage frame {frame}: expected {999 - damageCalls * 2}, actual {samus.Health}.");
            if (frame <= 160 && (samus.XPosition != 368 || samus.YPosition != 218))
                throw new InvalidDataException($"Plant lost captured position at frame {frame}: {samus.XPosition},{samus.YPosition}.");
            ushort expectedBlock = frame >= 256 ? (ushort)0x35a1 : frame >= 160 ? (ushort)0x05a7
                : SamusEaterAuditData.ChewBlocks[(frame % 40) / 5];
            if (level.GetCollisionBlock(23, 14).LevelWord != expectedBlock)
                throw new InvalidDataException($"Plant draw timing differs at frame {frame}.");
            foreach (var sound in runtime.Plms.SoundRequests)
            {
                soundCount++;
                Console.WriteLine($"Plant sound frame {frame}: {sound}");
            }
        }
        if (soundCount != 4 || samus.XPosition == 368 || level.GetCollisionBlock(23, 14).CollisionType != RoomCollisionType.SpecialAir)
            throw new InvalidDataException($"Plant release/restoration failed: sounds={soundCount}, X={samus.XPosition}, block={level.GetCollisionBlock(23, 14).LevelWord:X4}.");
        Console.WriteLine($"Samus Eater completed: health={samus.Health}, position={samus.XPosition},{samus.YPosition}.");
        return 0;
    }
}

/// <summary>Independent first-block words for the eight five-frame floor-plant draws at $84:ACBF.</summary>
internal static class SamusEaterAuditData
{
    public static ReadOnlySpan<ushort> ChewBlocks => [0x05a5, 0x05a3, 0x05a5, 0x05a7, 0x05a5, 0x05a3, 0x05a5, 0x05a7];
}
