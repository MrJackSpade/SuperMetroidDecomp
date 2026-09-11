using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Issue 415 turnaround and walljump comparisons using earned charge and input.
    /// Exclusive-create keeps earlier evidence intact.
    /// No pose, charge or animation state is forced after the initial fixture setup.
    /// </summary>
    private static void VerifyAerialSpreadTransitions(string? tracePath = null, string? outputPath = null, bool wallRoute = false)
    {
        using var output = outputPath is null ? null : new StreamWriter(new FileStream(outputPath, FileMode.CreateNew));
        using var native = tracePath is null ? null : File.OpenText(tracePath);
        const string header = "left,delay,frame,input,pose,x,xsub,y,ysub,charge,spread,bombs";
        output?.WriteLine(header);
        if (native is not null) AssertEqual(header, native.ReadLine(), "aerial native trace header");
        foreach (bool left in new[] { false, true })
        for (int timingCase = 0; timingCase < (wallRoute ? 14 : 10); timingCase++)
        {
            int delay = timingCase < 10 ? timingCase : timingCase + 30;
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            var level = runtime.LevelData!;
            // Match the grounded technique clearing, extending it upward for the jump.
            for (int y = 16; y < 36; y++)
            for (int x = 16; x < 48; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                    y >= 32 || (wallRoute && x == (left ? 29 : 33)) ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
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
            for (int frame = 0; frame < (wallRoute ? 300 : 260); frame++)
            {
                ushort input = runtime.ControllerBindings.Shoot;
                if (frame is >= 70 and < 125) input |= runtime.ControllerBindings.Jump;
                if (frame >= 76 && frame < 105 && frame != 78 + delay)
                    input |= (ushort)SnesButton.Down;
                if (frame is >= 78 and < 125) input |= (ushort)(left ? SnesButton.Right : SnesButton.Left);
                if (wallRoute)
                {
                    int morphDelay = timingCase >= 12 ? 0 : delay;
                    input = timingCase == 12 || frame < 87 || frame >= 94 + morphDelay ? runtime.ControllerBindings.Shoot : (ushort)0;
                    if (frame is >= 70 and < 86 or >= 89 and < 200) input |= runtime.ControllerBindings.Jump;
                    if (frame is >= 68 and < 87) input |= (ushort)(left ? SnesButton.Left : SnesButton.Right);
                    if (frame is >= 87 and < 140) input |= (ushort)(left ? SnesButton.Right : SnesButton.Left);
                    if (timingCase == 13 && frame == 93)
                    {
                        input &= unchecked((ushort)~(SnesButton.Left | SnesButton.Right));
                        input |= (ushort)(left ? SnesButton.Left : SnesButton.Right);
                    }
                    if (frame >= 94 + morphDelay && frame < 115 + morphDelay) input |= (ushort)SnesButton.Down;
                }
                uint previousY = ((uint)samus.YPosition << 16) | samus.Kinematics.YSubposition;
                runtime.StepFrame(input);
                string row = $"{(left ? 1 : 0)},{delay},{frame},{input:X4},{samus.Pose:X4},{samus.XPosition:X4},{samus.Kinematics.XSubposition:X4},{samus.YPosition:X4},{samus.Kinematics.YSubposition:X4},{samus.ProjectileFlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}";
                output?.WriteLine(row);
                if (native is not null) AssertEqual(native.ReadLine(), row, $"native aerial transition left={left}, delay={delay}, frame={frame}");
                AssertEqual(samus.ProjectileFlareCounter, runtime.Projectiles.FlareCounter, "aerial charge mirror");
                if (wallRoute)
                    VerifyWallSpreadFrame(runtime, left, timingCase, delay, frame, previousY);
                else
                    VerifyTurnaroundSpreadFrame(runtime, left, delay, frame);
                if (wallRoute ? frame >= 115 : delay == 6 && frame >= 105)
                    for (int slotIndex = 0; slotIndex < SamusBombProjectileSystem.SlotCount; slotIndex++)
                    {
                        var slot = runtime.BombProjectiles.Slots[slotIndex];
                        // Inactive native slots retain scratch words that are overwritten
                        // at allocation. Compare their inactive ownership, not dead bytes.
                        string bomb = $"bomb,{slotIndex},{(slot.IsActive ? 1 : 0)}";
                        if (slot.IsActive)
                            bomb += $",{slot.Type:X4},{slot.XPosition:X4},{slot.XSubposition:X4},{slot.YPosition:X4},{slot.YSubposition:X4},{slot.BombSpreadXVelocity:X4},{slot.BombSpreadYVelocity:X4},{slot.BombSpreadYSubvelocity:X4},{slot.BombTimer:X4},{slot.XRadius:X4},{slot.YRadius:X4},{slot.InstructionPointer:X4},{slot.InstructionTimer:X4},{slot.SpritemapPointer:X4}";
                        output?.WriteLine(bomb);
                        if (native is not null) AssertEqual(native.ReadLine(), bomb, $"native aerial bomb left={left}, frame={frame}");
                    }
            }
        }
        if (native is not null) AssertTrue(native.ReadLine() is null, "aerial native trace fully consumed");
        Console.WriteLine(wallRoute
            ? "Charged-walljump spread: 8400 frames, 25900 bomb-slot observations; early/late morph success and held-Shoot/turn controls agree."
            : "Aerial down-aim spread: both directions and ten input timings (5200 frames), with 1550 bomb-slot observations through expiry.");
    }

    private static void VerifyTurnaroundSpreadFrame(SuperMetroidRuntime runtime, bool left, int delay, int frame)
    {
        var samus = runtime.Samus!;
        bool releasedSpread = delay == 6 && frame >= 105;
        if (frame < 125 || delay != 6)
            AssertEqual((ushort)(releasedSpread ? 5 : 0), runtime.BombProjectiles.BombCounter,
                $"only native one-frame window produces spread: left={left}, delay={delay}, frame={frame}");
        if (frame == 259) AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "aerial spread expires completely");
        if (delay == 6 && frame is >= 92 and <= 104)
        {
            AssertEqual((ushort)80, samus.ProjectileFlareCounter, "airborne morph preserves earned charge while Down is held");
            AssertEqual((ushort)(frame - 91), samus.BombSpreadChargeTimeoutCounter, "airborne hold advances after morph completes");
        }
        if (releasedSpread)
            AssertEqual((ushort)0, samus.ProjectileFlareCounter, "airborne spread consumes charge");
    }

    private static void VerifyWallSpreadFrame(SuperMetroidRuntime runtime, bool left, int timingCase, int delay, int frame, uint previousY)
    {
        var samus = runtime.Samus!;
        if (frame == 89)
            AssertEqual(left ? SamusPoseIds.WallJumpRightPose : SamusPoseIds.WallJumpLeftPose,
                samus.Pose, "real input must earn a walljump, including negative controls");
        if (timingCase < 12)
        {
            int morphFrame = 94 + delay;
            int releaseFrame = 115 + delay;
            if (frame >= 87 && frame < releaseFrame)
                AssertEqual((ushort)71, samus.ProjectileFlareCounter, "walljump retains charge through released Shoot and morph");
            if (frame >= morphFrame && frame < morphFrame + 6)
                AssertEqual(left ? SamusPoseIds.MorphingTransitionRightPose : SamusPoseIds.MorphingTransitionLeftPose,
                    samus.Pose, "six-frame airborne morph follows charged walljump");
            if (frame >= morphFrame + 7 && frame < releaseFrame)
                AssertEqual((ushort)(frame - morphFrame - 6), samus.BombSpreadChargeTimeoutCounter, "walljump spread hold cadence");
            if (frame < releaseFrame)
                AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "Down retains the aerial spread");
            if (frame == releaseFrame)
                AssertEqual((ushort)5, runtime.BombProjectiles.BombCounter, "Down release launches all five bombs");
            if (frame >= releaseFrame)
                AssertEqual((ushort)0, samus.ProjectileFlareCounter, "walljump spread consumes charge");
            if (delay == 41 && frame == 134)
                AssertTrue((((uint)samus.YPosition << 16) | samus.Kinematics.YSubposition) > previousY,
                    "late success starts morph after descent has already begun, as native permits");
        }
        else
        {
            AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "held Shoot or intervening turn prevents walljump spread");
            if (timingCase == 12 && frame == 90)
                AssertEqual(left ? SamusPoseIds.NormalJumpGunExtendedRightPose : SamusPoseIds.NormalJumpGunExtendedLeftPose, samus.Pose,
                    "held Shoot interrupts the newly earned walljump into normal jump");
            if (timingCase == 13 && frame == 95)
                AssertEqual((ushort)0, samus.ProjectileFlareCounter, "intervening turn fires the retained charge instead of morphing");
        }
        if (frame == 299) AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "walljump spread expires");
    }
}
