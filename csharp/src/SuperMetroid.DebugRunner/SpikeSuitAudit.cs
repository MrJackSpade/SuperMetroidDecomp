using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Compares the morphed spike-hit Shinespark Suit input window with the original CPU.</summary>
internal static class SpikeSuitAudit
{
    private const string RetailRomSha256 =
        "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72";
    private const string DryTraceSha256 =
        "8EA5FE301EAA2034EADABED8D9054244DA0DFA51375378365E0006DB297825CE";
    private const string UnderwaterReserveTraceSha256 =
        "2E06311C877D780CFE480FAA961756816706C50B3E1C3F8FA13E93412B758D8C";

    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = ReadVerifiedTrace(bus, trace, DryTraceSha256);
        string traceHash = HashText(text);

        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(line => line.Split(','))
            .ToArray();
        if (rows.Length != 315 || rows.Any(row => row.Length != 20))
            throw new InvalidDataException(
                $"Incomplete spike-suit trace {traceHash}: expected 315 twenty-column frames.");

        int samples = 0;
        int mismatches = 0;
        var reported = new HashSet<string>();
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]}"))
        {
            string[] first = group.First();
            int unmorphFrame = int.Parse(first[0], CultureInfo.InvariantCulture);
            int launchFrame = int.Parse(first[1], CultureInfo.InvariantCulture);
            var runtime = CreateRuntime(bus);
            SamusState samus = runtime.Samus ?? throw new InvalidDataException("Missing spike fixture Samus.");
            int expectedFrame = 0;

            foreach (string[] row in group)
            {
                int frame = int.Parse(row[2], CultureInfo.InvariantCulture);
                if (frame != expectedFrame++)
                    throw new InvalidDataException($"Reordered spike-suit trace in case {group.Key}.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                ushort authoredInput = frame == unmorphFrame || frame == launchFrame
                    ? (ushort)SnesButton.A
                    : (ushort)0;
                if (input != authoredInput)
                    throw new InvalidDataException($"Changed spike-suit input in case {group.Key}, frame {frame}.");

                runtime.StepFrame(input);
                string actual = BuildComparableFrame(samus);
                string expected = string.Join(',', row[4..14].Concat(row[15..17]).Concat(row[18..20]));
                if (actual != expected)
                {
                    mismatches++;
                    if (reported.Add(group.Key) && reported.Count <= 12)
                        Console.WriteLine($"SPIKE SUIT {group.Key} frame {frame}: {actual} != {expected}");
                }
                samples++;
            }

            bool magicWindow = launchFrame == 9 && unmorphFrame is 1 or 2;
            if (magicWindow)
            {
                if (samus.Pose != SamusPoseIds.ShinesparkWindupRightPose ||
                    samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
                    samus.Shinespark.PaletteType != 6 ||
                    samus.SharedShineTimer == 0)
                {
                    throw new InvalidDataException(
                        $"Case {group.Key} did not retain the cartridge's usable Shinespark Suit state: " +
                        $"pose=${samus.Pose:X2}, phase={samus.Shinespark.Phase}, palette={samus.Shinespark.PaletteType}, " +
                        $"timer={samus.SharedShineTimer}, contact={samus.HorizontalSpeed.ContactDamageIndex}.");
                }
            }
            else if (launchFrame == 8 && unmorphFrame == 1 &&
                samus.Shinespark.Phase != ShinesparkPhase.Windup)
            {
                throw new InvalidDataException("The adjacent bad window no longer auto-launches its Shinespark.");
            }
        }

        Console.WriteLine(
            $"Spike Shinespark Suit: {samples} original-CPU frames, {mismatches} mismatches; trace SHA-256 {traceHash}.");
        return mismatches == 0 ? 0 : 1;
    }

    public static int RunUnderwaterReserve(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = ReadVerifiedTrace(bus, trace, UnderwaterReserveTraceSha256);
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(line => line.Split(','))
            .ToArray();
        if (rows.Length != 875 || rows.Any(row => row.Length != 21))
            throw new InvalidDataException(
                $"Incomplete underwater Reserve trace: expected 875 twenty-one-column frames.");

        int mismatches = 0;
        var reported = new HashSet<string>();
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]}"))
        {
            int freezeAfter = int.Parse(group.First()[0], CultureInfo.InvariantCulture);
            int launchAfter = int.Parse(group.First()[1], CultureInfo.InvariantCulture);
            var runtime = CreateRuntime(bus, water: true);
            SamusState samus = runtime.Samus!;
            for (int frame = 0; frame < freezeAfter; frame++)
                runtime.StepFrame(frame == 1 ? (ushort)SnesButton.A : (ushort)0);

            samus.Health = 0;
            samus.MaxHealth = 99;
            samus.ReserveEnergy = 60;
            samus.MaxReserveEnergy = 100;
            samus.ReserveTankMode = 1;
            var recovery = new SamusReserveAutoRecoveryState();
            recovery.Begin(samus);
            runtime.GameplayTimeFrozen = true;
            while (recovery.IsActive)
            {
                ushort recoveryInput = samus.ReserveEnergy == 1 && launchAfter == 0
                    ? (ushort)SnesButton.A
                    : (ushort)0;
                runtime.StepFrame(recoveryInput, afterAcceptedNmi: () =>
                {
                    SamusReserveAutoRecoveryStep step = recovery.StepAfterNmi(samus, runtime.NmiFrameCounter);
                    if (step.Completed)
                        runtime.GameplayTimeFrozen = false;
                });
            }

            int expectedFrame = 0;
            foreach (string[] row in group)
            {
                int frame = int.Parse(row[2], CultureInfo.InvariantCulture);
                if (frame != expectedFrame++)
                    throw new InvalidDataException($"Reordered underwater Reserve trace in case {group.Key}.");
                ushort authoredInput = frame == launchAfter ? (ushort)SnesButton.A : (ushort)0;
                ushort tracedInput = ushort.Parse(row[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (tracedInput != authoredInput)
                    throw new InvalidDataException(
                        $"Changed underwater Reserve input in case {group.Key}, frame {frame}.");
                if (frame != 0)
                {
                    runtime.StepFrame(authoredInput);
                }
                string actual = BuildReserveComparableFrame(samus);
                string expected = string.Join(',', row[4..21]);
                if (actual != expected)
                {
                    mismatches++;
                    if (reported.Add(group.Key) && reported.Count <= 20)
                        Console.WriteLine($"RESERVE SUIT {group.Key} frame {frame}: {actual} != {expected}");
                }
            }

            bool suitWindow = freezeAfter is >= 8 and <= 10 && launchAfter == 6;
            bool retainedSuit = samus.Pose == SamusPoseIds.ShinesparkWindupRightPose &&
                samus.Shinespark.Phase == ShinesparkPhase.Inactive &&
                samus.Shinespark.PaletteType == 6 &&
                samus.SharedShineTimer != 0;
            if (retainedSuit != suitWindow)
            {
                throw new InvalidDataException(
                    $"Case {group.Key} ended with retained-suit={retainedSuit}; expected {suitWindow}.");
            }
        }
        Console.WriteLine(
            $"Underwater Reserve Shinespark Suit: {rows.Length} original-CPU frames, " +
            $"{mismatches} mismatches; trace SHA-256 {HashText(text)}.");
        return mismatches == 0 ? 0 : 1;
    }

    private static SuperMetroid.Core.Runtime.SuperMetroidRuntime CreateRuntime(
        SuperMetroidAddressSpace bus,
        bool water = false)
    {
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException("Missing spike fixture room.");
        SamusState samus = runtime.Samus ?? throw new InvalidDataException("Missing spike fixture Samus.");
        foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
            enemy.Clear();
        foreach (RoomEnemyProjectileSlot projectile in runtime.Enemies.EnemyProjectiles)
            projectile.Clear();
        for (int blockIndex = 0; blockIndex < level.WidthInBlocks * level.HeightInBlocks; blockIndex++)
        {
            level.SetForegroundEntry(blockIndex, 0);
            level.SetBehavior(blockIndex, 0);
        }

        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.Kinematics.XPosition = 200;
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YPosition = 166;
        samus.Kinematics.YSubposition = 0;
        samus.EquippedItems = samus.CollectedItems = (ushort)(
            SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.SpeedBooster);
        samus.Health = samus.MaxHealth = 199;
        samus.ReserveEnergy = 0;
        if (water)
            samus.LiquidPhysics.ConfigureWater(8, 0x80);
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            (ushort)(((byte)SamusMovementType.MorphBallGround << 8) | 8);
        samus.PoseHistory.LastDifferentPose = 0;
        samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
        if (!samus.Shinespark.TryStoreFromSpeedBooster(
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
        {
            throw new InvalidDataException("Stored Shinespark setup was rejected.");
        }

        int centerBlock = (samus.YPosition >> 4) * level.WidthInBlocks + (samus.XPosition >> 4);
        level.SetForegroundEntry(centerBlock, (ushort)((ushort)RoomCollisionType.SpikeAir << 12));
        level.SetBehavior(centerBlock, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
        if (water)
        {
            for (int row = 0; row < level.HeightInBlocks; row++)
            for (int column = 0; column < level.WidthInBlocks; column++)
            {
                int block = row * level.WidthInBlocks + column;
                level.SetForegroundEntry(block, (ushort)((ushort)RoomCollisionType.SpikeAir << 12));
                level.SetBehavior(block, SamusTerrainHazardRomData.DamagingSpikeAirBehavior);
            }
        }
        return runtime;
    }

    private static string ReadVerifiedTrace(
        SuperMetroidAddressSpace bus,
        string trace,
        string expectedTraceSha256)
    {
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != RetailRomSha256)
            throw new InvalidDataException("Use the pinned Japan/USA retail ROM for the spike-suit audit.");

        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        string actualTraceSha256 = HashText(text);
        if (actualTraceSha256 != expectedTraceSha256)
        {
            throw new InvalidDataException(
                $"Spike-suit trace hash {actualTraceSha256} does not match {expectedTraceSha256}.");
        }
        return text;
    }

    private static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static string BuildComparableFrame(SamusState samus)
    {
        ushort nativeHandler = samus.KnockbackActive
            ? (ushort)0xdf38
            : (ushort)(samus.Shinespark.Phase switch
            {
                ShinesparkPhase.Windup => 0xd068,
                ShinesparkPhase.Vertical => 0xd0ab,
                ShinesparkPhase.Diagonal => 0xd0d7,
                ShinesparkPhase.Horizontal => 0xd106,
                _ => 0xa337,
            });
        // This fixture is right-facing Morph Ball throughout the hit. The host stores
        // the semantic up-right direction as two; native `$0A52` numbers that table arm one.
        ushort nativeKnockbackDirection = samus.KnockbackDirection == 2
            ? (ushort)1
            : samus.KnockbackDirection;
        return $"{samus.Pose:X2},{samus.XPosition:X4},{samus.YPosition:X4},{samus.Kinematics.YSubposition:X4}," +
            $"{samus.Kinematics.YSpeed:X4},{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
            $"{nativeHandler:X4},{samus.KnockbackTimer:X4},{nativeKnockbackDirection:X4}," +
            $"{samus.SharedShineTimer:X4},{samus.Shinespark.PaletteType:X4}," +
            $"{samus.InvincibilityTimer:X4},{samus.Health:X4}";
    }

    private static string BuildReserveComparableFrame(SamusState samus)
    {
        ushort nativeHandler = samus.KnockbackActive
            ? (ushort)0xdf38
            : (ushort)(samus.Shinespark.Phase switch
            {
                ShinesparkPhase.Windup => 0xd068,
                ShinesparkPhase.Vertical => 0xd0ab,
                ShinesparkPhase.Diagonal => 0xd0d7,
                ShinesparkPhase.Horizontal => 0xd106,
                _ => 0xa337,
            });
        ushort nativeKnockbackDirection = samus.KnockbackDirection == 2
            ? (ushort)1
            : samus.KnockbackDirection;
        return $"{samus.Pose:X2},{samus.XPosition:X4},{samus.YPosition:X4},{samus.Kinematics.YSubposition:X4}," +
            $"{samus.Kinematics.YSpeed:X4},{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
            $"{nativeHandler:X4},{samus.KnockbackTimer:X4},{nativeKnockbackDirection:X4}," +
            $"{samus.SharedShineTimer:X4},{samus.Shinespark.PaletteType:X4}," +
            $"{samus.InvincibilityTimer:X4},{samus.Health:X4},{samus.ReserveEnergy:X4}," +
            $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4}";
    }
}
