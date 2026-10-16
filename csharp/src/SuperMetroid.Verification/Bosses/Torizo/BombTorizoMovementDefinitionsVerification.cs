using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bomb Torizo's compiled posture offsets and walk velocities against native tables and their consumers.</summary>
internal static class BombTorizoMovementDefinitionsVerification
{
    /// <summary>Compares six source tables and exercises actual posture and walking consumers with migrated reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Bomb Torizo movement definitions")]
    internal static void VerifyBombTorizoMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int standingXAddress = 0xaac3ee;
        const int standingYAddress = 0xaac40e;
        const int sittingXAddress = 0xaac440;
        const int sittingYAddress = 0xaac460;
        const int normalWalkAddress = 0xaac4bd;
        const int facelessWalkAddress = 0xaac532;

        for (ushort tableOffset = 0; tableOffset < 32; tableOffset += 2)
        {
            Offset16<RoomPixels> displacement =
                BombTorizoMovementDefinitions.Posture(tableOffset);
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

        for (ushort tableOffset = 0; tableOffset < 40; tableOffset += 2)
        {
            ushort velocity = BombTorizoMovementDefinitions.WalkVelocity(tableOffset);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((normalWalkAddress + tableOffset)), velocity,
                $"Bomb Torizo normal walk velocity ${tableOffset:X2}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((facelessWalkAddress + tableOffset)), velocity,
                $"Bomb Torizo faceless walk velocity ${tableOffset:X2}");
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.Posture(1),
            "Bomb Torizo odd posture offset");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.Posture(32),
            "Bomb Torizo posture offset outside table");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.WalkVelocity(1),
            "Bomb Torizo odd walk offset");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.WalkVelocity(40),
            "Bomb Torizo walk offset outside table");

        BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        for (ushort tableOffset = 0; tableOffset < 32; tableOffset += 2)
        foreach (bool subtract in new[] { false, true })
        {
            Offset16<RoomPixels> expected =
                BombTorizoMovementDefinitions.Posture(tableOffset);
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 500)),
            };

            BombTorizoBehavior.ApplyBombTorizoMapOffset(slot, tableOffset, subtract);
            int sign = subtract ? -1 : 1;
            AssertEqual(unchecked((ushort)(1000 + sign * expected.X)), slot.Position.X.Whole,
                $"Bomb Torizo posture X ${tableOffset:X2} subtract {subtract}");
            AssertEqual(unchecked((ushort)(500 + sign * expected.Y)), slot.Position.Y.Whole,
                $"Bomb Torizo posture Y ${tableOffset:X2} subtract {subtract}");
        }

        var guarded = NewBombTorizoMovementReadGuard(rom);
        var enemies = new RoomEnemySystem();
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(guarded);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(new ushort[4096], new byte[4096], new ushort[4096]), new byte[8]);

        for (ushort tableOffset = 0; tableOffset < 40; tableOffset += 2)
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)),
                Radius = new(8, 8),
            };
            var state = new TorizoEnemyState(slot, isGolden: false);
            var next = enemies.BombTorizoBehavior.ProcessBombTorizoWalkInstruction(slot, state, null, level, new TorizoWalkStep { TableOffset = tableOffset, CollisionFacingRight = TorizoBodyProgram.FacingLeftTurningLeft, CollisionFacingLeft = TorizoBodyProgram.FacingRightTurningRight });

            AssertEqual(BombTorizoMovementDefinitions.WalkVelocity(tableOffset),
                state.Velocity.X.RawValue,
                $"Bomb Torizo walk consumer velocity ${tableOffset:X2}");
            AssertTrue(next is null,
                $"Bomb Torizo walk consumer runs on without a wall ${tableOffset:X2}");
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
