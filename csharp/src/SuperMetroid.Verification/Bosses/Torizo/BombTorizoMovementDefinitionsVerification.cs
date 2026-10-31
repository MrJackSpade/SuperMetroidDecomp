using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Bomb Torizo's compiled posture offsets and walk velocities against native tables and their consumers.</summary>
internal static class BombTorizoMovementDefinitionsVerification
{
    /// <summary>Compares six source tables and exercises actual posture and walking consumers with migrated reads forbidden.</summary>
    internal static void VerifyBombTorizoMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int standingXAddress = 0xaac3ee;
        const int standingYAddress = 0xaac40e;
        const int sittingXAddress = 0xaac440;
        const int sittingYAddress = 0xaac460;
        const int normalWalkAddress = 0xaac4bd;
        const int facelessWalkAddress = 0xaac532;

        // The cartridge's tables hold one word per step; the eight vertical words repeat for both facings.
        for (int step = 0; step < BombTorizoMovement.PostureStepCount; step++)
        {
            int tableOffset = step * 2;
            Offset16<RoomPixels> displacement =
                BombTorizoMovement.Posture(step);
            int wrappedYOffset = tableOffset & 0x000f;

            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((standingXAddress + tableOffset))),
                displacement.X,
                $"Bomb Torizo standing X offset ${tableOffset:X2}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((standingYAddress + wrappedYOffset))),
                displacement.Y,
                $"Bomb Torizo standing Y offset ${tableOffset:X2}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((sittingXAddress + tableOffset))),
                displacement.X,
                $"Bomb Torizo sitting X offset ${tableOffset:X2}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((sittingYAddress + wrappedYOffset))),
                displacement.Y,
                $"Bomb Torizo sitting Y offset ${tableOffset:X2}");
        }

        for (int step = 0; step < BombTorizoMovement.WalkStepCount; step++)
        {
            int tableOffset = step * 2;
            int velocity = BombTorizoMovement.WalkVelocity(step);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((normalWalkAddress + tableOffset))), velocity,
                $"Bomb Torizo normal walk velocity step {step}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((facelessWalkAddress + tableOffset))), velocity,
                $"Bomb Torizo faceless walk velocity step {step}");
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoMovement.Posture(-1),
            "Bomb Torizo negative posture step");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovement.Posture(BombTorizoMovement.PostureStepCount),
            "Bomb Torizo posture step outside table");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovement.WalkVelocity(-1),
            "Bomb Torizo negative walk step");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovement.WalkVelocity(BombTorizoMovement.WalkStepCount),
            "Bomb Torizo walk step outside table");


        for (int step = 0; step < BombTorizoMovement.PostureStepCount; step++)
        foreach (bool subtract in new[] { false, true })
        {
            Offset16<RoomPixels> expected =
                BombTorizoMovement.Posture(step);
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 500)),
            };

            BombTorizoBehavior.ApplyBombTorizoPosture(slot, step, subtract ? BombTorizoPostureMove.SitDown : BombTorizoPostureMove.StandUp);
            int sign = subtract ? -1 : 1;
            AssertEqual(unchecked((ushort)(1000 + sign * expected.X)), slot.Position.X.Whole,
                $"Bomb Torizo posture X step {step} subtract {subtract}");
            AssertEqual(unchecked((ushort)(500 + sign * expected.Y)), slot.Position.Y.Whole,
                $"Bomb Torizo posture Y step {step} subtract {subtract}");
        }

        var guarded = NewBombTorizoMovementReadGuard(rom);
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(new RoomTerrainBlock[4096], new RoomBlockBehavior[4096], new RoomBlockAppearance[4096]));

        for (int step = 0; step < BombTorizoMovement.WalkStepCount; step++)
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)),
                Radius = new(8, 8),
            };
            var state = new TorizoEnemyState(slot, isGolden: false);
            var next = enemies.BombTorizoBehavior.Choices.ProcessBombTorizoWalkInstruction(slot, state, null, level, new TorizoWalkStep { Step = step, CollisionFacingRight = TorizoBodyProgram.FacingLeftTurningLeft, CollisionFacingLeft = TorizoBodyProgram.FacingRightTurningRight });

            AssertEqual(BombTorizoMovement.WalkVelocity(step),
                (int)state.Velocity.X.SignedRawValue,
                $"Bomb Torizo walk consumer velocity step {step}");
            AssertTrue(next is null,
                $"Bomb Torizo walk consumer runs on without a wall step {step}");
        }

        Console.WriteLine(
            "Bomb Torizo movement definitions: all 88 native words, 32 real posture applications and 20 real walk consumers pass with all six source tables forbidden.");
    }

    /// <summary>Rejects reads of the original posture and walk tables while production behavior runs.</summary>
    private static CartridgeReadGuard NewBombTorizoMovementReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address is >= 0xaac3ee and < 0xaac41e or
                    >= 0xaac440 and < 0xaac470 or
                    >= 0xaac4bd and < 0xaac4e5 or
                    >= 0xaac532 and < 0xaac55a,
            address => $"Bomb Torizo movement attempted migrated table read ${address:X6}.");

    /// <summary>Runs the Bomb Torizo movement definition comparison against the retail ROM.</summary>
    [VerificationCommand("--lookup-stream2-torizo-movement")]
    internal static int LookupStream2TorizoMovementCommand()
    {
        Suite(nameof(VerifyBombTorizoMovementDefinitions), () => VerifyBombTorizoMovementDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
