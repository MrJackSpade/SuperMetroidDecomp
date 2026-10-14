using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Checks compiled mechanics ownership for enemy projectile instruction entry points.</summary>
internal static class EnemyProjectileInstructionOwnerCoverageVerification
{
    /// <summary>Checks every nonzero initial, touch, and shot entry without a cartridge fallback.</summary>
    internal static void VerifyEnemyProjectileInstructionOwnerCoverage()
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        var forbiddenBus = NewEnemyProjectileMechanicsFallbackForbiddenBus();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(forbiddenBus);

        int definitionCount = 0;
        int entryCount = 0;
        int checksum = 0;
        foreach (RoomEnemyProjectileKind kind in Enum.GetValues<RoomEnemyProjectileKind>())
        {
            if (kind == RoomEnemyProjectileKind.None)
                continue;

            definitionCount++;
            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            var projectile = new RoomEnemyProjectileSlot(0)
            {
                Kind = kind,
            };
            // Botwoon's, Crocomire's, Draygon's and Kraid's spawners (and Kago's, which shares Kraid's rock pose) start
            // typed animations (#641); their oracles match them to these native lists frame by frame, and no
            // interpreter owns the lists. Crocomire's mouth projectile, Draygon's goop and Kraid's spat rock also start
            // typed shot programs, and the goop a typed touch program.
            bool typed = kind is RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit or
                RoomEnemyProjectileKind.CrocomireProjectile or RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces or RoomEnemyProjectileKind.DraygonGoop or
                RoomEnemyProjectileKind.DraygonWallTurret or RoomEnemyProjectileKind.KraidSpitRock or
                RoomEnemyProjectileKind.KraidCeilingRock or RoomEnemyProjectileKind.KraidRisingRockLeft or
                RoomEnemyProjectileKind.KraidRisingRockRight or RoomEnemyProjectileKind.KagoBug or
                RoomEnemyProjectileKind.MotherBrainRoomTurret or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
                RoomEnemyProjectileKind.MotherBrainGlassShard or RoomEnemyProjectileKind.MotherBrainGlassSparkle or
                RoomEnemyProjectileKind.MotherBrainHandBeamCharging or RoomEnemyProjectileKind.MotherBrainHandBeamFired;
            if (!typed)
                VerifyEntry(definition.InitialInstructionList, "initial");
            if (kind is not (RoomEnemyProjectileKind.DraygonGoop or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet))
                VerifyEntry(definition.TouchInstructionList, "touch");
            if (kind is not (RoomEnemyProjectileKind.CrocomireProjectile or RoomEnemyProjectileKind.DraygonGoop or
                RoomEnemyProjectileKind.KraidSpitRock or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet))
                VerifyEntry(definition.ShotInstructionList, "shot");

            void VerifyEntry(ushort address, string role)
            {
                if (address == 0)
                    return;

                try
                {
                    checksum += EnemyProjectileEngine.ReadEnemyProjectileInstructionMechanicsWord(projectile, address);
                    entryCount++;
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Projectile {kind} {role} entry $86:{address:X4} has no compiled " +
                        "mechanics owner.",
                        exception);
                }
            }
        }

        AssertTrue(definitionCount > 100,
            "projectile owner audit covers the complete translated definition catalog");
        AssertTrue(entryCount > definitionCount,
            "projectile owner audit covers optional touch and shot entries");
        AssertTrue(checksum != 0,
            "projectile owner audit consumes every nonzero definition entry");
        AssertEqual(0, forbiddenBus.ForbiddenReadAttempts,
            "projectile definition entries never use the removed mechanics fallback");

        var unknown = new RoomEnemyProjectileSlot(0)
        {
            Kind = RoomEnemyProjectileKind.None,
        };
        InvalidDataException unknownException = AssertThrows<InvalidDataException>(
            () => EnemyProjectileEngine.ReadEnemyProjectileInstructionMechanicsWord(unknown, 0xffff),
            "uncatalogued projectile mechanics fail explicitly");
        AssertTrue(unknownException.Message.Contains("uncompiled", StringComparison.Ordinal),
            "uncatalogued projectile mechanics identify the missing compiled owner");
        AssertEqual(0, forbiddenBus.ForbiddenReadAttempts,
            "uncatalogued projectile mechanics fail without probing cartridge data");

        Console.WriteLine(
            $"Enemy-projectile mechanics owners: {definitionCount} translated definitions " +
            $"and {entryCount} initial/touch/shot entries resolve without a bank-$86 " +
            "fallback; unknown entries fail explicitly.");
    }

    /// <summary>Creates an unbacked bus that rejects any attempt to recover projectile mechanics from the ROM.</summary>
    private static CartridgeReadGuard NewEnemyProjectileMechanicsFallbackForbiddenBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Projectile mechanics attempted forbidden cartridge read ${address:X6}.")
            .RejectWrites(address => $"Projectile mechanics attempted unexpected write ${address:X6}.");

    /// <summary>Runs the projectile mechanics owner coverage check.</summary>
    [VerificationCommand("--enemy-projectile-instruction-owner-coverage")]
    internal static int EnemyProjectileInstructionOwnerCoverageCommand()
    {
        Suite(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage());
        return 0;
    }
}
