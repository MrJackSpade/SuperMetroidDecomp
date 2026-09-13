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

    public static int ExportApproach(string romPath, string directory)
    {
        Directory.CreateDirectory(directory);
        string prefix = Path.Combine(directory, "approach");
        RunCase(romPath, 20, prefix + "-full", groundLeftLead: 1, jumpRelease: 1, exportEnd: 220);
        RunCase(romPath, 20, prefix + "-isolated", isolate: true, groundLeftLead: 1, jumpRelease: 1, exportEnd: 220);
        if (!File.ReadAllBytes(prefix + "-full.csv").SequenceEqual(File.ReadAllBytes(prefix + "-isolated.csv")))
            throw new InvalidDataException("Actor omission changes the longer controller-earned approach.");
        Console.WriteLine("Controller-earned approach exported; omission matches, native comparison pending.");
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

    public static int ScanTurnBeforeJump(string romPath)
    {
        int candidates = 0;
        foreach (int offset in Enumerable.Range(1, 16).Select(value => value * 2))
        foreach (int lead in Enumerable.Range(1, 16))
        foreach (int hold in new[] { 4, 8, 16, 24 })
        {
            int minimumX = RunCase(romPath, offset, quiet: true, jumpHold: hold,
                jumpRelease: 1, groundLeftLead: lead);
            Console.WriteLine($"TURN offset={offset} lead={lead} hold={hold} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, offset, jumpHold: hold, jumpRelease: 1, groundLeftLead: lead);
            }
        }
        Console.WriteLine($"Turn-before-jump candidate crossings={candidates}; not native certification.");
        foreach (int freezeEnd in Enumerable.Range(0, 25).Select(value => 108 + value * 4))
        foreach (int offset in new[] { 0, 4, 8, 12, 16, 20, 24, 28 })
        foreach (int hold in new[] { 4, 8, 16, 24 })
        {
            int minimumX = RunCase(romPath, offset, quiet: true, jumpHold: hold,
                jumpRelease: 1, groundLeftLead: 4, freezeEnd: freezeEnd);
            Console.WriteLine($"FREEZE end={freezeEnd} offset={offset} hold={hold} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, offset, jumpHold: hold, jumpRelease: 1, groundLeftLead: 4, freezeEnd: freezeEnd);
            }
        }
        Console.WriteLine($"Including freeze wait: candidate crossings={candidates}; not native certification.");
        return 0;
    }

    public static int ScanAlignedNeutralJump(string romPath)
    {
        int candidates = 0;
        foreach (int offset in new[] { 4, 8, 12, 16, 20, 24, 28, 32 })
        foreach (int lead in new[] { 1, 4, 8, 12 })
        foreach (int hold in new[] { 4, 8, 16, 24 })
        {
            int minimumX = RunCase(romPath, offset, quiet: true, jumpHold: hold,
                jumpRelease: 1, groundLeftLead: lead, neutralJumpOnAlignment: true);
            Console.WriteLine($"NEUTRAL offset={offset} lead={lead} hold={hold} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, offset, jumpHold: hold, jumpRelease: 1,
                    groundLeftLead: lead, neutralJumpOnAlignment: true);
            }
        }
        Console.WriteLine($"Neutral-jump candidate crossings={candidates}; native certification remains outstanding.");
        return 0;
    }

    public static int ScanInvulnerableReapproach(string romPath)
    {
        int candidates = 0;
        foreach (int wait in new[] { 0, 2, 4, 8 })
        foreach (int back in Enumerable.Range(1, 12))
        foreach (int hold in new[] { 4, 12, 24 })
        {
            int minimumX = RunCase(romPath, 20, quiet: true, jumpHold: hold,
                jumpRelease: 1, groundLeftLead: 1, neutralJumpOnAlignment: true,
                recoveryBackFrames: back, recoveryWaitFrames: wait);
            Console.WriteLine($"RECOVER wait={wait} back={back} hold={hold} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, 20, jumpHold: hold, jumpRelease: 1, groundLeftLead: 1,
                    neutralJumpOnAlignment: true, recoveryBackFrames: back, recoveryWaitFrames: wait);
            }
        }
        Console.WriteLine($"Invulnerable reapproach candidate crossings={candidates}; native certification remains outstanding.");
        return 0;
    }

    private static int RunCase(string romPath, int stepBackFrames, string? exportPrefix = null, bool isolate = false, bool repeatStepBack = false,
        bool quiet = false, int jumpHold = 24, int jumpRelease = 12, int initialLeftDelay = 0, int? alignedX = null, int groundLeftLead = 0, int freezeEnd = 120, int exportEnd = 160,
        bool neutralJumpOnAlignment = false, int recoveryBackFrames = 0, int recoveryWaitFrames = 0)
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
        bool reportedCrouchAlignment = false;
        int? neutralJumpStart = null;
        int? recoveryStart = null;
        int jumpStart = freezeEnd + stepBackFrames + groundLeftLead;
        if (!quiet) Console.WriteLine($"CASE stepBackFrames={stepBackFrames} repeatStepBack={repeatStepBack} hold={jumpHold} release={jumpRelease} delay={initialLeftDelay} lead={groundLeftLead} freezeEnd={freezeEnd}");
        using var trace = exportPrefix is null ? null : new StreamWriter(exportPrefix + ".csv");
        trace?.WriteLine("frame,input,x,y,pose,anim,timer,xradius,yradius,health,frozen");
        for (int frame = 0; frame < (exportPrefix is null ? 360 : exportEnd); frame++)
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
                frame < freezeEnd ? (ushort)(SnesButton.Up | SnesButton.X) :
                frame < freezeEnd + stepBackFrames ? (ushort)SnesButton.Right :
                frame < jumpStart ? (ushort)SnesButton.Left :
                (ushort)(SnesButton.Left | ((frame - jumpStart) % (jumpHold + jumpRelease) < jumpHold ? SnesButton.A : 0));
            if (repeatStepBack && frame >= 120)
            {
                int phase = (frame - 120) % 80;
                input = phase < stepBackFrames ? (ushort)SnesButton.Right :
                    (ushort)(SnesButton.Left | (phase < stepBackFrames + 32 ? SnesButton.A : 0));
            }
            if (frame >= jumpStart && frame < jumpStart + initialLeftDelay)
                input = (ushort)SnesButton.A;
            // A second short spinjump must be earned while the first hit's immunity
            // is still active. The ordinary repeated-jump search never steps back here.
            if (recoveryBackFrames != 0 && recoveryStart is null &&
                samus.InvincibilityTimer != 0 && !samus.KnockbackActive)
            {
                recoveryStart = frame;
                Console.WriteLine($"RECOVERY-SETUP frame={frame} x={samus.XPosition}.{samus.Kinematics.XSubposition:X4} y={samus.YPosition}.{samus.Kinematics.YSubposition:X4} inv={samus.InvincibilityTimer}");
            }
            if (recoveryStart is { } recovery)
            {
                int phase = frame - recovery - recoveryWaitFrames;
                input = phase < 0 ? (ushort)0 : phase < recoveryBackFrames ? (ushort)SnesButton.Right :
                    phase == recoveryBackFrames ? (ushort)SnesButton.Left :
                    (ushort)(SnesButton.Left | ((phase - recoveryBackFrames - 1) % (jumpHold + jumpRelease) < jumpHold ? SnesButton.A : 0));
            }
            // Controller-only recovery: release jump/direction, initiate a neutral
            // jump, then steer left. Never manufacture alignment or invulnerability.
            if (neutralJumpOnAlignment && neutralJumpStart is null && frame >= jumpStart &&
                samus.Pose == SamusPoseIds.CrouchingLeftPose && samus.XPosition == 836 &&
                samus.YPosition < 160 && samus.InvincibilityTimer != 0)
            {
                neutralJumpStart = frame;
                Console.WriteLine($"NEUTRAL-SETUP frame={frame} xSub={samus.Kinematics.XSubposition:X4} y={samus.YPosition}.{samus.Kinematics.YSubposition:X4} inv={samus.InvincibilityTimer}");
            }
            if (neutralJumpStart is { } start)
                input = frame == start ? (ushort)0 : frame == start + 1 ? (ushort)SnesButton.A :
                    (ushort)(SnesButton.Left | SnesButton.A);
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
            if (!reportedCrouchAlignment && frame >= jumpStart && samus.XPosition == 836 &&
                samus.Pose == SamusPoseIds.CrouchingLeftPose && samus.YPosition < 160)
            {
                reportedCrouchAlignment = true;
                Console.WriteLine($"ALIGNMENT offset={stepBackFrames} lead={groundLeftLead} hold={jumpHold} release={jumpRelease} freezeEnd={freezeEnd} frame={frame} xSub={samus.Kinematics.XSubposition:X4} y={samus.YPosition}.{samus.Kinematics.YSubposition:X4} inv={samus.InvincibilityTimer}");
            }
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
