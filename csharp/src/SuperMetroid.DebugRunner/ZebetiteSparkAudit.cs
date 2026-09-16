using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>Room-local #442 spark exploration, with an explicitly constructed stored shine.</summary>
internal static class ZebetiteSparkAudit
{
    public static int Run(string romPath)
    {
        foreach (int escapeFrame in Enumerable.Range(78, 25))
            RunCase(romPath, escapeFrame);
        return 0;
    }

    public static int Export(string romPath, string directory, int frameCount = 80)
    {
        Directory.CreateDirectory(directory);
        RunCase(romPath, 90, Path.Combine(directory, "full"), false, frameCount);
        RunCase(romPath, 90, Path.Combine(directory, "isolated"), true, frameCount);
        if (!File.ReadAllBytes(Path.Combine(directory, "full.csv")).SequenceEqual(File.ReadAllBytes(Path.Combine(directory, "isolated.csv"))))
            throw new InvalidDataException("Omitting other actors changes the spark comparison interval.");
        Console.WriteLine($"Spark actor omission matches for {frameCount} frames; native comparison remains outstanding.");
        return 0;
    }

    public static int ExportPassage(string romPath, string directory)
    {
        Directory.CreateDirectory(directory);
        foreach ((string name, int escapeFrame) in new[] { ("success", 82), ("early-failure", 81) })
        {
            string full = Path.Combine(directory, name + "-full");
            string isolated = Path.Combine(directory, name + "-isolated");
            int frameCount = name == "success" ? 179 : 160;
            RunCase(romPath, escapeFrame, full, isolate: false, frameCount: frameCount, postMode: 2, quiet: true);
            RunCase(romPath, escapeFrame, isolated, isolate: true, frameCount: frameCount, postMode: 2, quiet: true);
            if (!File.ReadAllBytes(full + ".csv").SequenceEqual(File.ReadAllBytes(isolated + ".csv")))
                throw new InvalidDataException($"Omitting other actors changes the {name} passage interval.");
        }

        int successMinimumX = ReadMinimumWholeX(Path.Combine(directory, "success-full.csv"));
        int failureMinimumX = ReadMinimumWholeX(Path.Combine(directory, "early-failure-full.csv"));
        if (successMinimumX >= 800 || failureMinimumX < 800)
            throw new InvalidDataException($"Expected success/failure split was lost: success={successMinimumX}, failure={failureMinimumX}.");
        Console.WriteLine($"Spark passage export: success minX={successMinimumX}; one-frame-early failure minX={failureMinimumX}; other actors omitted identically.");
        return 0;
    }

    public static int ScanCadence(string romPath)
    {
        int candidates = 0;
        foreach (int period in Enumerable.Range(2, 7))
        foreach (int phase in Enumerable.Range(0, period))
        foreach (int escape in Enumerable.Range(78, 33))
        {
            int minimumX = RunCase(romPath, escape, jumpPeriod: period, jumpPhase: phase, quiet: true);
            Console.WriteLine($"CADENCE period={period} phase={phase} escape={escape} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, escape, jumpPeriod: period, jumpPhase: phase);
            }
        }
        Console.WriteLine($"Candidate passages={candidates}; native parity and subsequent control not established.");
        return 0;
    }

    public static int ScanHeldCadence(string romPath)
    {
        int candidates = 0;
        foreach (int period in new[] { 4, 6 })
        foreach (int hold in new[] { 2, 3 })
        foreach (int phase in Enumerable.Range(0, period))
        foreach (int escape in Enumerable.Range(78, 33))
        {
            int minimumX = RunCase(romPath, escape, jumpPeriod: period, jumpPhase: phase, jumpHold: hold, quiet: true);
            Console.WriteLine($"HELD period={period} phase={phase} hold={hold} escape={escape} minX={minimumX}");
            if (minimumX < 800)
            {
                candidates++;
                RunCase(romPath, escape, jumpPeriod: period, jumpPhase: phase, jumpHold: hold);
            }
        }
        Console.WriteLine($"Held-jump candidate passages={candidates}; not native certification.");
        return 0;
    }

    public static int ScanEscapeInputs(string romPath)
    {
        int candidates = 0;
        foreach (int postMode in Enumerable.Range(0, 4))
        foreach (int period in Enumerable.Range(2, 7))
        foreach (int phase in Enumerable.Range(0, period))
        foreach (int escape in Enumerable.Range(78, 63))
        {
            int minimumX = RunCase(romPath, escape, jumpPeriod: period, jumpPhase: phase,
                quiet: true, postMode: postMode);
            if (minimumX >= 800) continue;
            candidates++;
            Console.WriteLine($"ESCAPE candidate mode={postMode} period={period} phase={phase} escape={escape} minX={minimumX}");
        }
        Console.WriteLine($"Combined-input candidate passages={candidates}; not native certification.");
        return 0;
    }

    private static int ReadMinimumWholeX(string path) => File.ReadLines(path).Skip(1)
        .Select(line => unchecked((int)(uint.Parse(line.Split(',')[2]) >> 16))).Min();

    private static int RunCase(string romPath, int escapeFrame, string? prefix = null, bool isolate = false, int frameCount = 180,
        int jumpPeriod = 2, int jumpPhase = 0, bool quiet = false, int jumpHold = 1, int postMode = 0)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var samus = runtime.Samus!;
        samus.XPosition = 837;
        samus.YPosition = 195;
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = ushort.MaxValue;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.MorphBall);
        samus.EquippedBeams = samus.CollectedBeams = 0;
        samus.Health = samus.MaxHealth = 399;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        runtime.Camera!.SetPosition(768, 0);
        // Isolate the room-local passage from earning/storing charge in the prior room.
        // No invincibility cheat or fabricated collision bypass is enabled.
        if (!samus.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
            throw new InvalidDataException("Constructed stored-shine setup rejected.");
        if (isolate)
        {
            foreach (var slot in runtime.Enemies.Slots.Where(slot => slot.NativeIndex != 128)) slot.Clear();
            foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
        }
        if (prefix is not null)
        {
            RoomMovementSeedExporter.Write(runtime, prefix + ".movement-seed");
            ZebetiteSkipSeed.Write(runtime, prefix + ".actors");
        }
        using var trace = prefix is null ? null : new StreamWriter(prefix + ".csv");
        trace?.WriteLine("frame,input,x,y,pose,anim,timer,xradius,yradius,health,inv,zebHealth");
        if (!quiet) Console.WriteLine($"CASE escapeFrame={escapeFrame} jumpPeriod={jumpPeriod} jumpPhase={jumpPhase} jumpHold={jumpHold}\nframe,input,x,y,pose,phase,health,inv,zebHealth");
        int minimumX = samus.XPosition;
        for (int frame = 0; frame < frameCount; frame++)
        {
            bool jumpPulse = (frame + jumpPeriod - jumpPhase) % jumpPeriod < jumpHold;
            ushort input = frame < 60 ? (ushort)(SnesButton.A | SnesButton.R) :
                frame < escapeFrame ? (ushort)(SnesButton.Down | (jumpPulse ? SnesButton.A : 0)) :
                postMode switch
                {
                    0 => (ushort)SnesButton.Left,
                    1 => (ushort)(SnesButton.Left | SnesButton.Down),
                    2 => (ushort)(SnesButton.Left | (jumpPulse ? SnesButton.A : 0)),
                    3 => (ushort)(SnesButton.Left | SnesButton.Down | (jumpPulse ? SnesButton.A : 0)),
                    _ => throw new ArgumentOutOfRangeException(nameof(postMode)),
                };
            runtime.StepFrame(input);
            if (prefix is not null && frame == 77 &&
                (samus.Pose != SamusPoseIds.FacingLeftNormalPose || samus.AnimationFrameTimer != 10 || samus.Kinematics.YRadius != 19))
                throw new InvalidDataException("Spark finish must preserve native post-animation standing timer and old live radius.");
            trace?.WriteLine($"{frame},{input},{samus.Kinematics.XFixed},{samus.Kinematics.YFixed},{samus.Pose},{samus.AnimationFrame},{samus.AnimationFrameTimer},{samus.Kinematics.XRadius},{samus.Kinematics.YRadius},{samus.Health},{samus.InvincibilityTimer},{runtime.Enemies.Slots[2].Health}");
            minimumX = Math.Min(minimumX, samus.XPosition);
            if (!quiet) Console.WriteLine($"{frame},{input},{samus.Kinematics.XFixed},{samus.Kinematics.YFixed},{samus.Pose},{samus.Shinespark.Phase},{samus.Health},{samus.InvincibilityTimer},{runtime.Enemies.Slots[2].Health}");
        }
        if (!quiet) Console.WriteLine("Exploration only: no successful or native passage claim.");
        return minimumX;
    }
}
