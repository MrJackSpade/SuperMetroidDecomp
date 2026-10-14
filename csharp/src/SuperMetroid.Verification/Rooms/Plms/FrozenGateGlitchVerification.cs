using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Compares the Caterpillar frozen-Zero gate clip and nearby controls with native CPU collision results.</summary>
internal static class FrozenGateGlitchVerification
{
    /// <summary>Starting Samus X used by the Caterpillar frozen-enemy gate fixture.</summary>
    private const int FrozenGateStartX = 602;
    /// <summary>Shared Y coordinate of Samus and the Zero enemy in the native cases.</summary>
    private const int FrozenGateY = 860;
    /// <summary>Retail downward-gate block index checked before collision playback.</summary>
    private const int FrozenGateBlockIndex = 0x0a16;
    /// <summary>Camera X of the supplied Caterpillar room setup.</summary>
    private const ushort FrozenGateCameraX = 512;
    /// <summary>Camera Y of the supplied Caterpillar room setup.</summary>
    private const ushort FrozenGateCameraY = 768;

    /// <summary>One native CPU collision case and its resulting gate state.</summary>
    internal readonly record struct FrozenGateNativeRecord(
        int Distance,
        ushort EnemyX,
        bool Frozen,
        bool EnemyCollision,
        ushort FinalX,
        bool GateWoke,
        ushort GatePreInstruction,
        ushort GateInstructionList);

    /// <summary>Compares one successful frozen-gate clip and three adjacent controls with native CPU results.</summary>
    internal static void VerifyFrozenEnemyGateGlitch()
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

    /// <summary>Replays one native case in the retail Caterpillar room through collision and gate PLM updates.</summary>
    private static void VerifyFrozenEnemyGateRecord(FrozenGateNativeRecord expected)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            RoomHeaderPointers.Caterpillar,
            new Position16<RoomPixels>(FrozenGateCameraX, FrozenGateCameraY));

        RoomLevelData level = runtime.Room.LoadedLevelData;
        RoomPlmSystem plms = runtime.Plms;
        RoomPlmSlotSnapshot gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        AssertEqual(FrozenGateBlockIndex, gate.BlockIndex,
            "Caterpillar retail downward gate block");

        RoomEnemySlot[] zeros = runtime.Enemies.SlotTable.Slots
            .Where(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zero)
            .ToArray();
        AssertEqual(3, zeros.Length, "Caterpillar retains its three retail Zero enemies");
        RoomEnemySlot zero = zeros[0];
        AssertEqual((ushort)8, zero.Radius.X, "retail Zero horizontal collision radius");
        AssertEqual((ushort)8, zero.Radius.Y, "retail Zero vertical collision radius");
        AssertTrue(!zero.Properties.HasAny(EnemyProperties.SolidToSamus),
            "unfrozen retail Zero does not use the always-solid property route");

        for (int warm = 0; warm < 2; warm++)
        {
            plms.Step(bus, level, (runtime.Room.BackgroundStreamer ?? throw new InvalidOperationException("Verification requires runtime.Room.BackgroundStreamer.")), new PlmView { Layer1Position = new(FrozenGateCameraX, FrozenGateCameraY), Bg1XOffset = 0 });
        }
        gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        AssertEqual((ushort)DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow,
            gate.PreInstruction, "Caterpillar gate enters proximity-aware sleep");
        ushort sleepingInstruction = gate.InstructionPointer;

        SamusState samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeX(FrozenGateStartX);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0);
        samus.Position = samus.Position.WithWholeY(FrozenGateY);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        AssertEqual((ushort)5, samus.Kinematics.Radius.X,
            "standing Samus horizontal collision radius");
        AssertEqual((ushort)21, samus.Kinematics.Radius.Y,
            "standing Samus vertical collision radius");

        samus.Kinematics.InteractiveEnemies =
        [
            new SolidEnemyCollisionBody(
                zero.NativeIndex,
                new(expected.EnemyX, FrozenGateY),
                new(zero.Radius.X, zero.Radius.Y),
                expected.Frozen ? (ushort)1 : (ushort)0,
                zero.Properties),
        ];

        BlockMoveResult movement = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = SnesSixteenSixteen.FromWhole(expected.Distance) });
        AssertTrue(movement.Collided,
            $"{expected.Distance}-pixel native case collides with enemy or terrain");
        AssertEqual(expected.EnemyCollision, movement.EnemyCollision.HasValue,
            $"{expected.Distance}-pixel case preserves native collision owner");
        AssertEqual(expected.FinalX, samus.Position.X.Whole,
            $"{expected.Distance}-pixel case preserves native final X");

        plms.Step(bus, level, (runtime.Room.BackgroundStreamer ?? throw new InvalidOperationException("Verification requires runtime.Room.BackgroundStreamer.")), new PlmView { Layer1Position = new(FrozenGateCameraX, FrozenGateCameraY), Bg1XOffset = 0 });
        gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        bool gateWoke = gate.InstructionPointer != sleepingInstruction;
        AssertEqual(expected.GateWoke, gateWoke,
            $"{expected.Distance}-pixel case preserves native gate wake state");

        // The C# coroutine represents the original moving pre-instruction with an inert
        // callback plus the same advancing instruction list. Compare the observable wake
        // result above; retain the raw CPU words in the fixture for disassembly auditing.
        AssertEqual(expected.GateWoke,
            gate.PreInstruction == (ushort)DownwardGatePreInstruction.Inert,
            $"{expected.Distance}-pixel case selects the translated moving-gate phase");
    }

    /// <summary>Parses the eight expected native collision and gate fields from a fixture row.</summary>
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

    /// <summary>Runs the frozen-enemy gate clip verification suite.</summary>
    [VerificationCommand("--frozen-gate-glitch")]
    internal static int FrozenGateGlitchCommand()
    {
        Suite(nameof(VerifyFrozenEnemyGateGlitch), () => VerifyFrozenEnemyGateGlitch());
        return 0;
    }
}
