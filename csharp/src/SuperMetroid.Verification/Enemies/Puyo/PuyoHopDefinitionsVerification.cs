using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>Compares Puyo hop records and initial vertical-curve integration with retail bank-$A2 data.</summary>
internal static class PuyoHopDefinitionsVerification
{
    /// <summary>Checks the seven hops against their native records and the six non-dropping integrations.</summary>
    internal static void VerifyCompiledPuyoHops(CartridgeImportAddressSpace rom)
    {
        Action<PuyoEnemyState> calculate = PuyoBehavior.CalculateInitialPuyoHopSpeed;
        var state = new PuyoEnemyState(PuyoAction.Grounded, new PuyoSpawnSettings(HopCooldown: 0, HopDistance: 0));
        foreach (PuyoHopType hopType in Enum.GetValues<PuyoHopType>())
        {
            int index = PuyoNativeWords.HopRecordOffsets.Word(hopType);
            var hop = PuyoHopDefinitions.Hops[hopType];
            AssertEqual(rom.ReadCartridgeWord((0xa29a07 + index)), hop.JumpHeight, "Puyo native hop height");
            // The record's second value is both the horizontal speed in 256ths of a pixel and, in 256ths of a mark, the
            // step the launch's search takes along the speed curve (#2065).
            // The hop moves by the speed's whole pixels only; its fraction is never read.
            AssertEqual(rom.ReadCartridgeWord((0xa29a09 + index)) >> 8, hop.XPixels, "Puyo native horizontal whole pixels");
            AssertEqual(rom.ReadCartridgeWord((0xa29a09 + index)), hop.LaunchSearchStep.InTwoHundredFiftySixthsOfMark, "Puyo native launch search step");
            // The record's third value is both the hop's curve step and, in 256ths of a pixel, the speed its drop falls at
            // when the hop is cut short (#1820); the drop itself takes no curve step.
            int third = rom.ReadCartridgeWord(0xa29a0b + index);
            AssertEqual(third * 256, hop.DropSpeed.RawValue, "Puyo native drop speed");
            if (hop.Movement != PuyoAirborneMovement.Dropping)
                AssertEqual(third, hop.CurveStep.InTwoHundredFiftySixthsOfMark, "Puyo native curve step");
            AssertEqual(rom.ReadCartridgeWord((0xa29a0d + index)), PuyoNativeWords.PuyoAirborneMovements.Word(hop.Movement), "Puyo native airborne callback");
            if (hop.Movement == PuyoAirborneMovement.Dropping)
                continue; // Dropping bypasses hop initialization and uses constant speed.
            ushort time = 0, distance = 0;
            ushort height = rom.ReadCartridgeWord((0xa29a07 + index));
            ushort target = unchecked((ushort)((height << 8) | (LittleEndianBytes.HighByte(height))));
            do
            {
                time = unchecked((ushort)(time + rom.ReadCartridgeWord((0xa29a09 + index))));
                distance = unchecked((ushort)(distance + rom.ReadCartridgeWord((0xa08390 + (LittleEndianBytes.HighByte(time)) * 8))));
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
            int nativeDrop = rom.ReadCartridgeWord(0xa29a0b + PuyoNativeWords.HopRecordOffsets.Word(cut));
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Puyo, place.KeptDefinition);
            place.SetSpawnParameter1(EnemyDefinitionId.Puyo, 5);
            PuyoBehavior.InitializePuyo(place, place.SpawnSettings<PuyoSpawnSettings>());
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            place.Radius = RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.Puyo).Radius;
            PuyoEnemyState state = SuperMetroid.Core.Enemies.EnemyPartners.Listed<PuyoEntity>(place).State;
            state.Action = PuyoAction.Rising;
            state.AirborneMovement = PuyoAirborneMovement.Dropping;
            state.HopType = PuyoHopType.Dropping;
            state.CurrentHop = cut;
            enemies.PuyoBehavior.RunPuyoMainAi(place.HeldOccupant(), place, place.RequireSpeciesState<PuyoEnemyState>(), new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with
                {
                    Camera = Position16<RoomPixels>.Zero,
                    TimeIsFrozen = false,
                    Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(600, 128)) },
                },
                new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero)));
            AssertEqual(SixteenSixteenWords.ToCoordinate(checked((uint)(128 * onePixel + nativeDrop * 256))), place.Position.Y,
                $"a Puyo dropping after its {cut} hop was cut short falls {nativeDrop}/256 pixels in one update");
        }
        Console.WriteLine("  Puyo drop speeds: a drop after each of the seven hops falls at its cut hop's native speed.");
    }
}
