using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Compares the cartridge's physical movement-handler replacements with the translated
/// forced-state owners that retain an uncrashed spark's boost words as Blue Suit.
/// </summary>
internal static class ForcedBlueStateAudit
{
    public static int Run(string romPath, string tracePath)
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        if (Convert.ToHexString(SHA256.HashData(retail.Rom)) != ForcedBlueAuditDefinitions.RetailRomSha256)
            throw new InvalidDataException("Forced Blue Suit audit requires the pinned Japan/USA ROM.");

        string text = File.ReadAllText(tracePath).Replace("\r\n", "\n", StringComparison.Ordinal);
        string traceHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        if (traceHash != ForcedBlueAuditDefinitions.NativeTraceSha256)
            throw new InvalidDataException("Use the accepted original-CPU forced Blue Suit trace.");
        string[][] rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(line => line.Split(','))
            .ToArray();
        if (rows.Length != 5 || rows.Any(row => row.Length != 9))
            throw new InvalidDataException("Incomplete original-CPU forced Blue Suit matrix.");

        int mismatches = 0;
        foreach (string[] row in rows)
        {
            ForcedBlueSnapshot actual = row[0] switch
            {
                "drained-f7" => RunDrained(retail),
                "ceres-ridley" => RunCeres(retail),
                "elevator-command" => RunElevator(retail),
                "xray-teardown" => RunXray(retail),
                "reserve-unlock-control" => RunReserveControl(retail),
                _ => throw new InvalidDataException($"Unknown forced Blue Suit owner '{row[0]}'."),
            };
            ForcedBlueSnapshot expected = Parse(row);
            if (actual == expected)
                continue;

            mismatches++;
            Console.WriteLine($"Forced Blue Suit {row[0]}: {actual} != {expected}");
        }

        Console.WriteLine(
            $"Forced Blue Suit owners: {rows.Length} original-CPU cases, {mismatches} mismatches; " +
            $"trace SHA-256 {traceHash}.");
        return mismatches == 0 ? 0 : 1;
    }

    private static ForcedBlueSnapshot RunDrained(ISnesAddressSpace bus)
    {
        SamusState samus = CreateActiveHorizontalSpark(bus);
        samus.Drained.LetFall(bus, samus);
        samus.Drained.InstallFallingMovementHandler(samus);
        return Snapshot(samus, ForcedBlueAuditDefinitions.DrainedFallingMovementHandler);
    }

    private static ForcedBlueSnapshot RunCeres(ISnesAddressSpace bus)
    {
        SamusState samus = CreateActiveHorizontalSpark(bus);
        samus.CeresRidleyEjection.Request();
        samus.CeresRidleyEjection.BeginFrame(samus);
        return Snapshot(samus, ForcedBlueAuditDefinitions.RtsMovementOrInputHandler);
    }

    private static ForcedBlueSnapshot RunElevator(SuperMetroidAddressSpace retail)
    {
        var bus = new PopulationSelectionAddressSpace(
            retail,
            [new RoomEnemyPopulationRecord(
                ForcedBlueAuditDefinitions.ElevatorEnemyDefinition,
                136,
                256,
                0,
                0,
                0,
                0,
                0)]);
        SamusState samus = CreateActiveHorizontalSpark(bus);
        samus.XPosition = 136;
        samus.YPosition = 235;
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            PopulationSelectionAddressSpace.PopulationPointer,
            PopulationSelectionAddressSpace.TilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            () => 0,
            samus: samus);
        enemies.PublishElevatorDoorContact();
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 144,
            timeIsFrozen: false,
            samus,
            newlyPressedControllerInput: (ushort)SnesButton.Down);
        if (enemies.ElevatorStatus != ElevatorActorStatus.Departing)
            throw new InvalidDataException("Forced Blue Suit elevator fixture did not depart.");
        return Snapshot(samus, ForcedBlueAuditDefinitions.NormalMovementHandler);
    }

    private static ForcedBlueSnapshot RunXray(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 256,
            YPosition = 400,
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.XrayScope),
            CollectedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.XrayScope),
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        if (!samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing))
            throw new InvalidDataException("Forced Blue Suit X-Ray fixture rejected admission.");
        for (int calls = 0; samus.Xray.SetupStage != 0 && calls < 32; calls++)
            samus.Xray.StepBeam(bus, samus, controllerInput: 0);
        for (int calls = 0; samus.Xray.BeamPhase != XrayBeamPhase.Finish && calls < 512; calls++)
            samus.Xray.StepBeam(bus, samus, controllerInput: 0);
        if (samus.Xray.BeamPhase != XrayBeamPhase.Finish)
            throw new InvalidDataException("Forced Blue Suit X-Ray fixture did not reach teardown.");

        SeedActiveHorizontalSpark(bus, samus);
        samus.Xray.StepBeam(bus, samus, controllerInput: 0);
        if (samus.Xray.IsActive)
            throw new InvalidDataException("Forced Blue Suit X-Ray teardown remained active.");
        return Snapshot(samus, ForcedBlueAuditDefinitions.NormalMovementHandler);
    }

    private static ForcedBlueSnapshot RunReserveControl(ISnesAddressSpace bus)
    {
        SamusState samus = CreateActiveHorizontalSpark(bus);
        samus.Health = 0;
        samus.ReserveEnergy = 1;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ForcedBlueAuditDefinitions.AutomaticReserveMode;
        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        SamusReserveAutoRecoveryStep step = recovery.StepAfterNmi(samus, nmiFrameCounter: 1);
        if (!step.Completed)
            throw new InvalidDataException("Forced Blue Suit Reserve control did not complete.");
        return Snapshot(samus, ForcedBlueAuditDefinitions.HorizontalShinesparkMovementHandler);
    }

    private static SamusState CreateActiveHorizontalSpark(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 256,
            YPosition = 400,
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster,
            CollectedItems = (ushort)SamusEquipmentFlags.SpeedBooster,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        SeedActiveHorizontalSpark(bus, samus);
        return samus;
    }

    private static void SeedActiveHorizontalSpark(ISnesAddressSpace bus, SamusState samus)
    {
        if (!samus.Shinespark.TryStoreFromSpeedBooster(
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
            throw new InvalidDataException("Forced Blue Suit fixture could not store shine.");
        samus.Shinespark.BeginWindup(samus);
        samus.Shinespark.BeginDirectionalLaunch(
            bus,
            samus,
            SamusPoseIds.ShinesparkHorizontalRightPose);
        if (samus.Shinespark.Phase != ShinesparkPhase.Horizontal ||
            samus.HorizontalSpeed.SpeedBoostCounter !=
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter ||
            samus.HorizontalSpeed.ExtraRunSpeed !=
                ForcedBlueAuditDefinitions.InitialShinesparkExtraRunSpeed)
        {
            throw new InvalidDataException("Forced Blue Suit fixture did not enter active horizontal spark.");
        }
    }

    private static ForcedBlueSnapshot Snapshot(SamusState samus, ushort movementHandler) => new(
        movementHandler,
        ForcedBlueAuditDefinitions.PoseInputHandler(samus),
        samus.Pose,
        samus.HorizontalSpeed.SpeedBoostCounter,
        samus.HorizontalSpeed.ExtraRunSpeed,
        samus.HorizontalSpeed.ExtraRunSubspeed,
        samus.Shinespark.ShineTimer,
        samus.Shinespark.PaletteType);

    private static ForcedBlueSnapshot Parse(string[] row) => new(
        ParseWord(row[1]),
        ParseWord(row[2]),
        byte.Parse(row[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
        ParseWord(row[4]),
        ParseWord(row[5]),
        ParseWord(row[6]),
        ParseWord(row[7]),
        ParseWord(row[8]));

    private static ushort ParseWord(string text) =>
        ushort.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private readonly record struct ForcedBlueSnapshot(
        ushort MovementHandler,
        ushort InputHandler,
        byte Pose,
        ushort BoostCounter,
        ushort ExtraSpeed,
        ushort ExtraSubspeed,
        ushort ShineTimer,
        ushort PaletteType)
    {
        public override string ToString() =>
            $"{MovementHandler:X4},{InputHandler:X4},{Pose:X2},{BoostCounter:X4}," +
            $"{ExtraSpeed:X4},{ExtraSubspeed:X4},{ShineTimer:X4},{PaletteType:X4}";
    }
}
