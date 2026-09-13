using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>#442 room-local exploration; a trace is not yet a cartridge parity assertion.</summary>
internal static class ZebetiteSkipAudit
{
    public static int Run(string romPath)
    {
        foreach (int stepBackFrames in new[] { 0, 8, 20 })
            RunCase(romPath, stepBackFrames);
        Console.WriteLine("Exploratory Ice-only traces completed. Native passage/control parity is NOT established.");
        return 0;
    }

    public static int Export(string romPath, string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (int offset in new[] { 0, 8, 20 })
        {
            string prefix = Path.Combine(directory, $"offset-{offset}");
            RunCase(romPath, offset, prefix + "-full", false);
            RunCase(romPath, offset, prefix + "-isolated", true);
            if (!File.ReadAllBytes(prefix + "-full.csv").SequenceEqual(File.ReadAllBytes(prefix + "-isolated.csv")))
                throw new InvalidDataException($"Omitting other actors changes the offset-{offset} collision interval.");
        }
        Console.WriteLine("Exported private collision intervals; omission preserves the compared fields. Native parity remains unverified.");
        return 0;
    }

    public static int ExploreRepeatedJumps(string romPath)
    {
        foreach (int offset in new[] { 8, 12, 16, 20, 24, 28, 32 })
            RunCase(romPath, offset, repeatStepBack: true);
        return 0;
    }

    public static int ExportAligned(string romPath, string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (int x in new[] { 836, 837 })
        {
            string prefix = Path.Combine(directory, $"aligned-{x}");
            RunCase(romPath, 0, prefix + "-full", alignedX: x);
            RunCase(romPath, 0, prefix + "-isolated", isolate: true, alignedX: x);
            if (!File.ReadAllBytes(prefix + "-full.csv").SequenceEqual(File.ReadAllBytes(prefix + "-isolated.csv")))
                throw new InvalidDataException($"Actor omission changes constructed alignment {x}.");
        }
        Console.WriteLine("Constructed alignment intervals exported; this does not establish controller-earned alignment.");
        return 0;
    }

    public static int ScanJumpTiming(string romPath)
    {
        // Controller-only search after the same real projectile freeze setup. Crossing
        // the barrier's X coordinate is a candidate, not proof of a valid/native skip.
        int candidates = 0;
        foreach (int offset in Enumerable.Range(0, 17).Select(value => value * 2))
        foreach (int hold in new[] { 1, 2, 4, 8, 12, 24 })
        foreach (int release in new[] { 1, 2, 6, 12 })
        {
            int minimumX = RunCase(romPath, offset, quiet: true, jumpHold: hold, jumpRelease: release);
            Console.WriteLine($"SCAN offset={offset} hold={hold} release={release} minX={minimumX}");
            if (minimumX < 800) candidates++;
        }
        Console.WriteLine($"Candidate crossings={candidates}; native passage/control parity is NOT established.");
        foreach (int offset in new[] { 0, 4, 8, 12, 16, 20, 24, 28, 32 })
        foreach (int delay in Enumerable.Range(0, 25))
        {
            int minimumX = RunCase(romPath, offset, quiet: true, jumpRelease: 1, initialLeftDelay: delay);
            Console.WriteLine($"DELAY offset={offset} delay={delay} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, offset, jumpRelease: 1, initialLeftDelay: delay);
            }
        }
        Console.WriteLine($"Including delayed steering: candidate crossings={candidates}; not native certification.");
        return 0;
    }

    public static int TraceJumpTiming(string romPath, int offset, int delay)
    {
        if (offset is < 0 or > 32 || delay is < 0 or > 24)
            throw new ArgumentOutOfRangeException(nameof(offset), "Use scanned offsets 0..32 and delays 0..24.");
        RunCase(romPath, offset, jumpRelease: 1, initialLeftDelay: delay);
        return 0;
    }

    private static int RunCase(string romPath, int stepBackFrames, string? exportPrefix = null, bool isolate = false, bool repeatStepBack = false,
        bool quiet = false, int jumpHold = 24, int jumpRelease = 12, int initialLeftDelay = 0, int? alignedX = null)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        // No destroyed-Zebetite events: this must be the first barrier, not the
        // second-generation setup used by the ten-missile destruction fixture.
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var samus = runtime.Samus!;
        samus.XPosition = 900; samus.YPosition = 100;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit);
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Ice;
        samus.SelectedHudItem = 0;
        samus.Health = samus.MaxHealth = 399;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(768, 0);
        bool frozeSpawn = false;
        int minimumX = samus.XPosition;
        int jumpStart = 120 + stepBackFrames;
        if (!quiet) Console.WriteLine($"CASE stepBackFrames={stepBackFrames} repeatStepBack={repeatStepBack} hold={jumpHold} release={jumpRelease} delay={initialLeftDelay}");
        using var trace = exportPrefix is null ? null : new StreamWriter(exportPrefix + ".csv");
        trace?.WriteLine("frame,input,x,y,pose,anim,timer,xradius,yradius,health,frozen");
        for (int frame = 0; frame < (exportPrefix is null ? 360 : 160); frame++)
        {
            if (frame == 120 && exportPrefix is not null)
            {
                if (alignedX is { } x)
                {
                    // Deliberately constructed precondition, NOT a claimed earned skip:
                    // crouching on the frozen Rinka with native invulnerability active.
                    samus.XPosition = checked((ushort)x);
                    samus.YPosition = 142;
                    samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
                    samus.Pose = SamusPoseIds.CrouchingLeftPose;
                    samus.InvincibilityTimer = 120;
                    samus.RefreshCollisionRadii(bus);
                    samus.InitializeAnimation(bus);
                    samus.CommitPoseHistory(bus);
                }
                if (isolate)
                {
                    foreach (var slot in runtime.Enemies.Slots.Where(slot => slot.NativeIndex is not 128 and not 192)) slot.Clear();
                    foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
                    runtime.Projectiles.Reset();
                }
                RoomMovementSeedExporter.Write(runtime, exportPrefix + ".movement-seed");
                ZebetiteSkipSeed.Write(runtime, exportPrefix + ".actors");
            }
            ushort input = frame < 60 ? (ushort)SnesButton.Left :
                frame < 120 ? (ushort)(SnesButton.Up | SnesButton.X) :
                frame < jumpStart ? (ushort)SnesButton.Right :
                (ushort)(SnesButton.Left | ((frame - jumpStart) % (jumpHold + jumpRelease) < jumpHold ? SnesButton.A : 0));
            if (repeatStepBack && frame >= 120)
            {
                int phase = (frame - 120) % 80;
                input = phase < stepBackFrames ? (ushort)SnesButton.Right :
                    (ushort)(SnesButton.Left | (phase < stepBackFrames + 32 ? SnesButton.A : 0));
            }
            if (frame >= jumpStart && frame < jumpStart + initialLeftDelay)
                input = (ushort)SnesButton.A;
            runtime.StepFrame(input);
            if (alignedX is { } alignment)
            {
                // Original-CPU observations for this constructed precondition. Keep
                // these assertions separate from the still-unproven earned setup.
                if (frame == 120 && samus.Kinematics.YRadius != 16 ||
                    frame == 121 && samus.Kinematics.YRadius != 19 ||
                    frame == (alignment == 836 ? 142 : 131) && samus.Kinematics.YRadius != 19 ||
                    alignment == 836 && frame == 146 && samus.Kinematics.YRadius != 21)
                    throw new InvalidDataException($"Aligned {alignment} radius publication differs at frame {frame}.");
                if (frame >= 120 && runtime.Enemies.Slots[2].Health != 1000)
                    throw new InvalidDataException("Constructed passage must not destroy or damage the Zebetite.");
                if (frame == 159)
                {
                    uint expectedX = alignment == 836 ? 52060160u : 54853632u;
                    uint expectedY = alignment == 836 ? 8226815u : 8469504u;
                    if (unchecked((uint)samus.Kinematics.XFixed) != expectedX || unchecked((uint)samus.Kinematics.YFixed) != expectedY)
                        throw new InvalidDataException($"Aligned {alignment} endpoint differs from original CPU.");
                }
            }
            minimumX = Math.Min(minimumX, samus.XPosition);
            if (frame >= 120) trace?.WriteLine($"{frame},{input},{samus.Kinematics.XFixed},{samus.Kinematics.YFixed},{samus.Pose},{samus.AnimationFrame},{samus.AnimationFrameTimer},{samus.Kinematics.XRadius},{samus.Kinematics.YRadius},{samus.Health},{runtime.Enemies.Slots[3].FrozenTimer}");
            frozeSpawn |= runtime.Enemies.Slots.Any(slot => slot.EnemyDefinitionPointer == 0xd23f &&
                slot.XPosition == 823 && slot.YPosition == 166 && slot.FrozenTimer != 0);
            if (!quiet && (frame % 12 == 0 || frame >= jumpStart && frame <= jumpStart + 24))
            {
                var actors = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer is 0xd23f or 0xe27f)
                    .Select(slot => $"{slot.NativeIndex}:{slot.EnemyDefinitionPointer:X4}@{slot.XPosition},{slot.YPosition}/f{slot.FrozenTimer}/p{slot.Properties}");
                Console.WriteLine($"SKIP frame={frame} input={input:X4} samus={samus.XPosition}.{samus.Kinematics.XSubposition:X4},{samus.YPosition}.{samus.Kinematics.YSubposition:X4} pose={samus.Pose:X2} hp={samus.Health} inv={samus.InvincibilityTimer} actors={string.Join(';', actors)}");
            }
        }
        if (!frozeSpawn)
            throw new InvalidDataException("Ice-only exploration failed to freeze the lower Rinka at its spawn point.");
        if (!quiet) Console.WriteLine($"END stepBackFrames={stepBackFrames} x={samus.XPosition} y={samus.YPosition} pose={samus.Pose:X2} hp={samus.Health}; passage not asserted");
        return minimumX;
    }
}
