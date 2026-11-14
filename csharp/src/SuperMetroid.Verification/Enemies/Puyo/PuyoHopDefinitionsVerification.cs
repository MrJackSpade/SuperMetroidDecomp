using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>Compares Puyo hop records and initial vertical-curve integration with retail bank-$A2 data.</summary>
internal static class PuyoHopDefinitionsVerification
{
    /// <summary>Checks the seven hops against their native records and the six non-dropping integrations.</summary>
    internal static void VerifyCompiledPuyoHops(CartridgeImportAddressSpace rom)
    {
        Action<PuyoEnemyState> calculate = PuyoBehavior.CalculateInitialPuyoHopSpeed;
        var state = new PuyoEnemyState();
        foreach (PuyoHopType hopType in Enum.GetValues<PuyoHopType>())
        {
            int index = PuyoNativeWords.HopRecordOffsets.Word(hopType);
            var hop = PuyoHopDefinitions.Hops[hopType];
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a07 + index)), hop.JumpHeight, "Puyo native hop height");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a09 + index)), hop.XSpeed, "Puyo native horizontal speed");
            // The record's third value is both the hop's curve step and, in 256ths of a pixel, the speed its drop falls at
            // when the hop is cut short (#1820); the drop itself takes no curve step.
            int third = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa29a0b + index);
            AssertEqual(third * 256, hop.DropSpeed.RawValue, "Puyo native drop speed");
            if (hop.Movement != PuyoAirborneMovement.Dropping)
                AssertEqual(third, hop.CurveStep.InTwoHundredFiftySixthsOfMark, "Puyo native curve step");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a0d + index)), PuyoNativeWords.PuyoAirborneMovements.Word(hop.Movement), "Puyo native airborne callback");
            if (hop.Movement == PuyoAirborneMovement.Dropping)
                continue; // Dropping bypasses hop initialization and uses constant speed.
            ushort time = 0, distance = 0;
            ushort height = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a07 + index));
            ushort target = unchecked((ushort)((height << 8) | (LittleEndianBytes.HighByte(height))));
            do
            {
                time = unchecked((ushort)(time + CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a09 + index))));
                distance = unchecked((ushort)(distance + CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08390 + (LittleEndianBytes.HighByte(time)) * 8))));
            } while (unchecked((short)(target - distance)) >= 0);
            state.CurrentHop = hopType;
            calculate(state);
            AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(time), state.YSpeedCurvePlace, "Puyo real initial curve index without bus");
            AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(time / 2), state.InitialYSpeedCurvePlaceHalf, "Puyo half threshold");
            AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(time / 2 + time / 4), state.InitialYSpeedCurvePlaceThreeQuarters, "Puyo three-quarter threshold");
        }
        Console.WriteLine("Puyo definitions: all seven native records and six real initial hop integrations match without a bus; dropping remains a separate constant-speed path.");        Suite(nameof(VerifyPuyoDropSpeeds), () => VerifyPuyoDropSpeeds(rom));
    }

    /// <summary>
    /// #1820 regression: a Puyo whose hop a wall or ceiling cut short drops each update by the third value of the cut
    /// hop's native record, read in 256ths of a pixel: two pixels after a small, big or giant hop, three after the long
    /// hop, a pixel and three quarters after a dropped bounce and one pixel for the drop itself. The production main AI
    /// runs one dropping update of each from the same place in an open room.
    /// </summary>
    internal static void VerifyPuyoDropSpeeds(CartridgeImportAddressSpace rom)
    {
        const int onePixel = 65536;
        foreach (PuyoHopType cut in Enum.GetValues<PuyoHopType>())
        {
            int nativeDrop = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa29a0b + PuyoNativeWords.HopRecordOffsets.Word(cut));
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Puyo, slot.Definition);
            slot.SetSpawnParameter1(EnemyDefinitionId.Puyo, 5);
            PuyoBehavior.InitializePuyo(slot);
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            slot.Radius = RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.Puyo).Radius;
            PuyoEnemyState state = slot.RequireSpeciesState<PuyoEnemyState>();
            state.Action = PuyoAction.Airborne;
            state.AirborneMovement = PuyoAirborneMovement.Dropping;
            state.HopType = PuyoHopType.Dropping;
            state.CurrentHop = cut;
            enemies.PuyoBehavior.RunPuyoMainAi(slot, new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with
                {
                    Camera = Position16<RoomPixels>.Zero,
                    TimeIsFrozen = false,
                    Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(600, 128)) },
                },
                new FrameCounters(0)));
            AssertEqual(SixteenSixteenWords.ToCoordinate(checked((uint)(128 * onePixel + nativeDrop * 256))), slot.Position.Y,
                $"a Puyo dropping after its {cut} hop was cut short falls {nativeDrop}/256 pixels in one update");
        }
        Console.WriteLine("  Puyo drop speeds: a drop after each of the seven hops falls at its cut hop's native speed.");
    }
}
