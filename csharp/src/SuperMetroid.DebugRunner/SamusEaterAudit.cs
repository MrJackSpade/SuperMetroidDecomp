using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Desktop;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

internal static class SamusEaterAudit
{
    public static int Run(string rom, string? captureDirectory = null)
    {
        if (captureDirectory is not null) Directory.CreateDirectory(captureDirectory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        VerifyAdmission(bus);
        VerifyCeilingSequence(bus);
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
        SuperMetroidRuntime? restored = null;
        for (int frame = 1; frame < 257; frame++)
        {
            ushort input = frame >= 170 ? (ushort)SuperMetroid.Core.Input.SnesButton.Right : (ushort)0;
            runtime.StepFrame(input);
            if (restored is not null)
            {
                restored.StepFrame(input);
                if (restored.Samus!.XPosition != samus.XPosition || restored.Samus.YPosition != samus.YPosition ||
                    restored.Samus.Health != samus.Health || restored.Samus.InvincibilityTimer != samus.InvincibilityTimer ||
                    !restored.LevelData!.ForegroundEntries.Span.SequenceEqual(level.ForegroundEntries.Span))
                    throw new InvalidDataException($"Saved plant capture diverged on frame {frame}.");
                var originalPixels = SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                var restoredPixels = SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(restored)!);
                if (!originalPixels.AsSpan().SequenceEqual(restoredPixels))
                    throw new InvalidDataException($"Saved plant display diverged on frame {frame}.");
            }
            if (frame == 50)
            {
                using var saved = new MemoryStream();
                DebuggerObjectGraphSerializer.Serialize(saved, runtime);
                saved.Position = 0;
                restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidRuntime>(saved);
            }
            if (captureDirectory is not null && frame is 5 or 15 or 160 or 256)
                PngWriter.WriteRgba(Path.Combine(captureDirectory, $"plant-{frame:D4}.png"), FrontendFrame.Width, FrontendFrame.Height,
                    SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(runtime)!));
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

    private static void VerifyAdmission(SuperMetroidAddressSpace bus)
    {
        foreach (bool ceiling in new[] { false, true })
        foreach (int delta in new[] { -1, 0, 1 })
        {
            var words = new ushort[256];
            var bts = new byte[256];
            int trigger = 8 * 16 + 8;
            words[trigger] = 0x3000;
            bts[trigger] = ceiling ? (byte)0x81 : (byte)0x80;
            var level = new RoomLevelData(16, 16, words, bts, new ushort[256], []);
            var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 128 };
            samus.RefreshCollisionRadii(bus);
            samus.YPosition = (ushort)((ceiling ? 128 + samus.Kinematics.YRadius : 144 - samus.Kinematics.YRadius) + delta);
            var plms = new RoomPlmSystem();
            SamusInsideBlockReactions.PrepareFrame(bus, level, samus, AreaId.Brinstar, plms: plms);
            if (plms.ActiveCount != (delta == 0 ? 1 : 0) ||
                level.GetCollisionBlock(8, 8).CollisionType != (delta == 0 ? RoomCollisionType.Air : RoomCollisionType.SpecialAir))
                throw new InvalidDataException($"Plant alignment gate mismatch: ceiling={ceiling}, delta={delta}.");
        }
        Console.WriteLine("Plant admission: floor/ceiling exact-edge and one-pixel adjacent controls pass.");
    }

    private static void VerifyCeilingSequence(SuperMetroidAddressSpace bus)
    {
        var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
        var level = runtime.LevelData!;
        int trigger = level.GetBlockIndex(8, 8);
        level.SetForegroundEntry(trigger, 0x3000);
        level.SetBehavior(trigger, 0x81);
        var samus = runtime.Samus!;
        samus.XPosition = 128;
        samus.YPosition = (ushort)(128 + samus.Kinematics.YRadius);
        samus.Health = samus.MaxHealth = 999;
        samus.EquippedItems = 0;
        ushort heldY = (ushort)(samus.YPosition + 1);
        SamusInsideBlockReactions.PrepareFrame(bus, level, samus, AreaId.Brinstar, plms: runtime.Plms);
        int sounds = 0;
        for (int frame = 0; frame <= 416; frame++)
        {
            // Simulate displacement before the PLM pass; only the native pre-
            // instruction may undo it. This isolates the ceiling coroutine from
            // floor collision and does not claim a full-room ceiling approach.
            if (frame > 0) samus.XPosition++;
            runtime.Plms.Step(bus, level, runtime.BackgroundStreamer!, 0, 0, 0, null, 0, 0, 0);
            samus.LiquidPhysics.ApplyPeriodicDamage(samus, false);
            if (frame is > 0 and <= 320 && (samus.XPosition != 128 || samus.YPosition != heldY))
                throw new InvalidDataException($"Ceiling plant lost position ownership at frame {frame}.");
            if (frame > 320 && samus.XPosition == 128)
                throw new InvalidDataException("Ceiling plant retained position ownership after release.");
            int damageCalls = Enumerable.Range(0, 8).Sum(cycle =>
                (frame >= cycle * 40 + 15 ? 1 : 0) + (frame >= cycle * 40 + 35 ? 1 : 0));
            if (samus.Health != 999 - damageCalls * 2)
                throw new InvalidDataException($"Ceiling plant damage differs at frame {frame}.");
            sounds += runtime.Plms.SoundRequests.Count;
        }
        if (sounds != 8 || level.GetCollisionBlock(8, 8).CollisionType != RoomCollisionType.SpecialAir)
            throw new InvalidDataException("Ceiling plant did not restore its trigger after eight chewing cycles.");
        Console.WriteLine("Ceiling plant: 417 coroutine frames match capture, eight chewing cycles, 32 damage, release and restoration.");
    }
}

/// <summary>Independent first-block words for the eight five-frame floor-plant draws at $84:ACBF.</summary>
internal static class SamusEaterAuditData
{
    public static ReadOnlySpan<ushort> ChewBlocks => [0x05a5, 0x05a3, 0x05a5, 0x05a7, 0x05a5, 0x05a3, 0x05a5, 0x05a7];
}
