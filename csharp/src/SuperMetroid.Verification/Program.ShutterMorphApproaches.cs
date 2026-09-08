using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void AuditMorphShutterApproaches(bool reproduceOnly = false, bool exportNativeArc = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int cases = 0, worstGap = 0;
        var arc = new List<string>();
        string[]? nativeCheckedArc = reproduceOnly && !exportNativeArc
            ? File.ReadAllLines("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.csv") : null;
        if (nativeCheckedArc is not null) AssertEqual(143, nativeCheckedArc.Length, "native-checked shutter trajectory covers frames 117..259");
        using var bombInputs = exportNativeArc ? new BinaryWriter(File.Create("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.projectiles")) : null;
        foreach (int slotIndex in new[] { 0, 1 })
        foreach (bool approach in new[] { false, true })
        foreach (int interval in new[] { 8, 20, 40 })
        foreach (int rollAt in new[] { 30, 45, 60, 75, 90, 105, 120, 150 })
        foreach (int duration in new[] { 4, 8, 16 })
        {
            if (reproduceOnly && (slotIndex != 0 || approach || interval != 8 || rollAt != 75 || duration != 4)) continue;
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom);
            var samus = runtime.Samus!;
            var platform = runtime.Enemies.Slots[slotIndex];
            if (reproduceOnly)
                Console.WriteLine($"Native contact seed: platform radii={platform.XRadius},{platform.YRadius}; ceiling (23,3)={runtime.LevelData!.GetCollisionBlock(23, 3).CollisionType}");
            samus.InputLocked = false;
            samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            int toward = slotIndex == 0 ? -1 : 1;
            samus.XPosition = (ushort)(platform.XPosition - (approach ? toward * 32 : 0));
            samus.YPosition = (ushort)(platform.YPosition - platform.YRadius - samus.Kinematics.YRadius);
            runtime.StepFrame(0);
            for (int frame = 0; frame < 260; frame++)
            {
                ushort input = frame < 220 && frame % interval == 0 ? runtime.ControllerBindings.Shoot : (ushort)0;
                int direction = approach && frame < 12 ? toward : 0;
                if (frame >= rollAt && frame < rollAt + duration) direction = -toward;
                if (frame >= rollAt + duration && frame < rollAt + duration * 2) direction = toward;
                if (direction != 0) input |= (ushort)(direction < 0 ? SnesButton.Left : SnesButton.Right);
                string before = reproduceOnly && frame >= 110
                    ? $"X={samus.Kinematics.XFixed:X8} Y={samus.Kinematics.YFixed:X8} VY={samus.Kinematics.VerticalSpeedFixed:X8}/{samus.Kinematics.YDirection} pose={samus.Pose:X2} bomb={samus.BombJumpActive} platform={platform.YPosition}.{platform.YSubposition:X4}" : "";
                if (exportNativeArc && frame == 117) ExportShutterBombArcSeed(runtime);
                runtime.StepFrame(input);
                if (nativeCheckedArc is not null && frame >= 117)
                {
                    string actual = $"{frame},{samus.Kinematics.XFixed},{samus.Kinematics.YFixed},{samus.Kinematics.VerticalSpeedFixed},{samus.BombJumpDirection},{platform.YPosition},{platform.YSubposition}";
                    AssertEqual(nativeCheckedArc[frame - 117], actual, $"native-checked shutter ascent/landing/contact trajectory frame {frame}");
                }
                if (exportNativeArc && frame >= 117)
                {
                    foreach (var bomb in runtime.BombProjectiles.Slots)
                    {
                        bombInputs!.Write(bomb.XPosition); bombInputs.Write(bomb.YPosition);
                        bombInputs.Write(bomb.XRadius); bombInputs.Write(bomb.YRadius);
                        bombInputs.Write(bomb.Direction); bombInputs.Write(bomb.Type);
                        bombInputs.Write(bomb.Damage); bombInputs.Write(bomb.BombTimer);
                    }
                    arc.Add($"{frame},{samus.Kinematics.XFixed},{samus.Kinematics.YFixed},{samus.Kinematics.VerticalSpeedFixed},{samus.BombJumpDirection},{platform.YPosition},{platform.YSubposition}");
                    if (frame == 259)
                    {
                        File.WriteAllLines("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.csv", arc);
                        Console.WriteLine($"Exported {arc.Count} production bomb-ascent/landing frames for native comparison.");
                        return;
                    }
                }
                if (before.Length != 0)
                    Console.WriteLine($"trace {frame}: {before} -> Y={samus.Kinematics.YFixed:X8} VY={samus.Kinematics.VerticalSpeedFixed:X8}/{samus.Kinematics.YDirection} pose={samus.Pose:X2} extra={samus.Kinematics.ExtraYFixed} platform={platform.YPosition}.{platform.YSubposition:X4}");
                int gap = platform.YPosition - platform.YRadius - samus.YPosition - samus.Kinematics.YRadius;
                if (Math.Abs(samus.XPosition - platform.XPosition) < platform.XRadius + samus.Kinematics.XRadius &&
                    samus.YPosition < platform.YPosition && gap < worstGap)
                {
                    worstGap = gap;
                    Console.WriteLine($"Morph approach: slot={slotIndex} approach={approach} interval={interval} roll={rollAt}/{duration} frame={frame} gap={gap} Samus={samus.XPosition},{samus.YPosition}/{samus.Pose:X2} platformY={platform.YPosition}");
                }
                AssertTrue(samus.Kinematics.YRadius == 7, "morph-only approach never enters a standing/unmorph posture");
            }
            cases++;
        }
        Console.WriteLine($"{cases} morph-only bomb/roll/return sequences; minimum gap={worstGap}. " +
            (nativeCheckedArc is null ? "Exploratory sweep; not a cartridge-parity assertion." : "143 native-checked trajectory frames agree, including cartridge-permitted ceiling overlap."));
    }
}
