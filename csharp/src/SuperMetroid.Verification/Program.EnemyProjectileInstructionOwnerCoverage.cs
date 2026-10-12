using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents enemy projectile instruction owner coverage verification.</summary>
internal static class EnemyProjectileInstructionOwnerCoverageVerification
{
    /// <summary>Verifies enemy projectile instruction owner coverage.</summary>
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
            // Botwoon's spawners start typed animations (#641); VerifyBotwoonProjectileAnimations matches
            // them to these native initial lists frame by frame, and no interpreter owns the lists.
            if (kind is not (RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit))
                VerifyEntry(definition.InitialInstructionList, "initial");
            VerifyEntry(definition.TouchInstructionList, "touch");
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

    /// <summary>New enemy projectile mechanics fallback forbidden bus.</summary>
    private static CartridgeReadGuard NewEnemyProjectileMechanicsFallbackForbiddenBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Projectile mechanics attempted forbidden cartridge read ${address:X6}.")
            .RejectWrites(address => $"Projectile mechanics attempted unexpected write ${address:X6}.");

    /// <summary>Enemy projectile instruction owner coverage command.</summary>
    [VerificationCommand("--enemy-projectile-instruction-owner-coverage")]
    internal static int EnemyProjectileInstructionOwnerCoverageCommand()
    {
        Suite(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage());
        return 0;
    }
}
