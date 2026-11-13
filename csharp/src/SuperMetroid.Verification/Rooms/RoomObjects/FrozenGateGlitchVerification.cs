using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Globalization;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        ushort GateFrameBehavior,
        ushort GateAnimationScript);

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
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            RoomHeaderDefinitions.Caterpillar,
            new Position16<RoomPixels>(FrozenGateCameraX, FrozenGateCameraY));

        RoomLevelData level = runtime.Room.LoadedLevelData;
        RoomObjectSystem roomObjects = runtime.RoomObjects;
        RoomObjectSlotSnapshot gate = roomObjects.PopulationSlots.Single(slot =>
            slot.Kind == RoomObjectKind.DownwardGate);
        AssertEqual(level.BlockAt(FrozenGateBlockIndex), gate.Block,
            "Caterpillar retail downward gate block");

        RoomEnemySlot[] zeros = runtime.Enemies.SlotTable.Slots
            .Where(slot => slot.DefinitionId == EnemyDefinitionId.Zero)
            .ToArray();
        AssertEqual(3, zeros.Length, "Caterpillar retains its three retail Zero enemies");
        RoomEnemySlot zero = zeros[0];
        AssertEqual((ushort)8, zero.Radius.X, "retail Zero horizontal collision radius");
        AssertEqual((ushort)8, zero.Radius.Y, "retail Zero vertical collision radius");
        AssertTrue(!zero.Contact.HasAny(EnemyContact.SolidToSamus),
            "unfrozen retail Zero does not use the always-solid property route");

        for (int warm = 0; warm < 2; warm++)
        {
            roomObjects.Step(level);
        }
        gate = roomObjects.PopulationSlots.Single(slot =>
            slot.Kind == RoomObjectKind.DownwardGate);
        AssertTrue(gate.Behaviour is DownwardGateObject { Phase: DownwardGatePhase.RestingClosed, WakeArmed: true },
            "Caterpillar gate rests closed, waiting for a trigger or Samus in its column");

        SamusState samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeX(FrozenGateStartX);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0);
        samus.Position = samus.Position.WithWholeY(FrozenGateY);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        AssertEqual((ushort)5, samus.Kinematics.Radius.X,
            "standing Samus horizontal collision radius");
        AssertEqual((ushort)21, samus.Kinematics.Radius.Y,
            "standing Samus vertical collision radius");

        samus.Kinematics.InteractiveEnemies =
        [
            new SolidEnemyCollisionBody(
                zero.SlotIndex,
                new(expected.EnemyX, FrozenGateY),
                new(zero.Radius.X, zero.Radius.Y),
                expected.Frozen ||
                zero.Contact.HasAny(EnemyContact.SolidToSamus)),
        ];

        BlockMoveResult movement = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = SixteenSixteenWords.ToDisplacement(SixteenSixteen.FromWhole(expected.Distance)) });
        AssertTrue(movement.Collided,
            $"{expected.Distance}-pixel native case collides with enemy or terrain");
        AssertEqual(expected.EnemyCollision, movement.EnemyCollision.HasValue,
            $"{expected.Distance}-pixel case preserves native collision owner");
        AssertEqual(expected.FinalX, samus.Position.X.Whole,
            $"{expected.Distance}-pixel case preserves native final X");

        roomObjects.Step(level);
        gate = roomObjects.PopulationSlots.Single(slot =>
            slot.Kind == RoomObjectKind.DownwardGate);
        bool gateWoke = gate.Behaviour is not DownwardGateObject { Phase: DownwardGatePhase.RestingClosed };
        AssertEqual(expected.GateWoke, gateWoke,
            $"{expected.Distance}-pixel case preserves native gate wake state");

        // The typed gate stops waiting once it wakes, as the cartridge's moving gate stops running its wake. Compare the
        // observable wake result above; the raw CPU words stay in the fixture for disassembly auditing.
        AssertEqual(expected.GateWoke,
            gate.Behaviour is DownwardGateObject { WakeArmed: false, Phase: DownwardGatePhase.Reopening },
            $"{expected.Distance}-pixel case starts the gate rising");
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
