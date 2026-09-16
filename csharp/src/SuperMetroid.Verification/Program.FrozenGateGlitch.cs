using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private const int FrozenGateStartX = 602;
    private const int FrozenGateY = 860;
    private const int FrozenGateBlockIndex = 0x0a16;
    private const ushort FrozenGateCameraX = 512;
    private const ushort FrozenGateCameraY = 768;

    private readonly record struct FrozenGateNativeRecord(
        int Distance,
        ushort EnemyX,
        bool Frozen,
        bool EnemyCollision,
        ushort FinalX,
        bool GateWoke,
        ushort GatePreInstruction,
        ushort GateInstructionList);

    private static void VerifyFrozenEnemyGateGlitch()
    {
        FrozenGateNativeRecord[] expected = File.ReadLines(
                "csharp/test-fixtures/movement-release/frozen-gate-407-native.csv")
            .Skip(1)
            .Select(ParseFrozenGateNativeRecord)
            .ToArray();
        AssertEqual(4, expected.Length,
            "frozen-gate native fixture has one success and three adjacent controls");
        AssertEqual(1, expected.Count(record => record.GateWoke),
            "only the strict-overlap native setup wakes the gate");

        foreach (FrozenGateNativeRecord record in expected)
            VerifyFrozenEnemyGateRecord(record);

        Console.WriteLine(
            "PASS Caterpillar frozen-enemy gate clip: actual-room NTSC collision priority, " +
            "one-pixel boundary, unfrozen control, and gate wake match the original CPU.");
    }

    private static void VerifyFrozenEnemyGateRecord(FrozenGateNativeRecord expected)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            RoomHeaderPointers.Caterpillar,
            FrozenGateCameraX,
            FrozenGateCameraY);

        RoomLevelData level = runtime.LevelData!;
        RoomPlmSystem plms = runtime.Plms;
        RoomPlmSlotSnapshot gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        AssertEqual(FrozenGateBlockIndex, gate.BlockIndex,
            "Caterpillar retail downward gate block");

        RoomEnemySlot[] zeros = runtime.Enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer == RoomEnemySystem.ZeroDefinition)
            .ToArray();
        AssertEqual(3, zeros.Length, "Caterpillar retains its three retail Zero enemies");
        RoomEnemySlot zero = zeros[0];
        AssertEqual((ushort)8, zero.XRadius, "retail Zero horizontal collision radius");
        AssertEqual((ushort)8, zero.YRadius, "retail Zero vertical collision radius");
        AssertTrue(!zero.Properties.HasAny(EnemyProperties.SolidToSamus),
            "unfrozen retail Zero does not use the always-solid property route");

        for (int warm = 0; warm < 2; warm++)
        {
            plms.Step(
                bus,
                level,
                runtime.BackgroundStreamer!,
                FrozenGateCameraX,
                FrozenGateCameraY,
                bg1XOffset: 0);
        }
        gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        AssertEqual(DownwardGatePreInstructionCodes.WakeIfTriggeredOrSamusBelow,
            gate.PreInstruction, "Caterpillar gate enters proximity-aware sleep");
        ushort sleepingInstruction = gate.InstructionPointer;

        SamusState samus = runtime.Samus!;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = FrozenGateStartX;
        samus.Kinematics.XSubposition = 0;
        samus.YPosition = FrozenGateY;
        samus.Kinematics.YSubposition = 0;
        samus.RefreshCollisionRadii(bus);
        AssertEqual((ushort)5, samus.Kinematics.XRadius,
            "standing Samus horizontal collision radius");
        AssertEqual((ushort)21, samus.Kinematics.YRadius,
            "standing Samus vertical collision radius");

        samus.Kinematics.InteractiveEnemies =
        [
            new SolidEnemyCollisionBody(
                zero.NativeIndex,
                expected.EnemyX,
                FrozenGateY,
                zero.XRadius,
                zero.YRadius,
                expected.Frozen ? (ushort)1 : (ushort)0,
                zero.Properties),
        ];

        BlockMoveResult movement = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            expected.Distance << 16,
            plms: plms);
        AssertTrue(movement.Collided,
            $"{expected.Distance}-pixel native case collides with enemy or terrain");
        AssertEqual(expected.EnemyCollision, movement.EnemyCollision.HasValue,
            $"{expected.Distance}-pixel case preserves native collision owner");
        AssertEqual(expected.FinalX, samus.XPosition,
            $"{expected.Distance}-pixel case preserves native final X");

        plms.Step(
            bus,
            level,
            runtime.BackgroundStreamer!,
            FrozenGateCameraX,
            FrozenGateCameraY,
            bg1XOffset: 0);
        gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        bool gateWoke = gate.InstructionPointer != sleepingInstruction;
        AssertEqual(expected.GateWoke, gateWoke,
            $"{expected.Distance}-pixel case preserves native gate wake state");

        // The C# coroutine represents the original moving pre-instruction with an inert
        // callback plus the same advancing instruction list. Compare the observable wake
        // result above; retain the raw CPU words in the fixture for disassembly auditing.
        AssertEqual(expected.GateWoke,
            gate.PreInstruction == DownwardGatePreInstructionCodes.Inert,
            $"{expected.Distance}-pixel case selects the translated moving-gate phase");
    }

    private static FrozenGateNativeRecord ParseFrozenGateNativeRecord(string row)
    {
        string[] columns = row.Split(',');
        AssertEqual(8, columns.Length, "frozen-gate native fixture column count");
        return new FrozenGateNativeRecord(
            int.Parse(columns[0], CultureInfo.InvariantCulture),
            ushort.Parse(columns[1], CultureInfo.InvariantCulture),
            columns[2] == "1",
            columns[3] == "65535",
            ushort.Parse(columns[4], CultureInfo.InvariantCulture),
            columns[5] == "1",
            ushort.Parse(columns[6], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            ushort.Parse(columns[7], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }
}
