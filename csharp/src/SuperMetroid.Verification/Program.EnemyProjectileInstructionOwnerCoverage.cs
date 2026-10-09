using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Audits every catalogued projectile entry for a compiled mechanics owner and rejects cartridge fallback.</summary>
    private static void VerifyEnemyProjectileInstructionOwnerCoverage()
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        var forbiddenBus = new EnemyProjectileMechanicsFallbackForbiddenBus();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, forbiddenBus);
        MethodInfo read = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyProjectileInstructionMechanicsWord",
            staticFlags)!;

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
            VerifyEntry(definition.InitialInstructionList, "initial");
            VerifyEntry(definition.TouchInstructionList, "touch");
            VerifyEntry(definition.ShotInstructionList, "shot");

            void VerifyEntry(ushort address, string role)
            {
                if (address == 0)
                    return;

                try
                {
                    checksum += (ushort)read.Invoke(null, [projectile, address])!;
                    entryCount++;
                }
                catch (TargetInvocationException exception) when (exception.InnerException is not null)
                {
                    throw new InvalidOperationException(
                        $"Projectile {kind} {role} entry $86:{address:X4} has no compiled " +
                        "mechanics owner.",
                        exception.InnerException);
                }
            }
        }

        AssertTrue(definitionCount > 100,
            "projectile owner audit covers the complete translated definition catalog");
        AssertTrue(entryCount > definitionCount,
            "projectile owner audit covers optional touch and shot entries");
        AssertTrue(checksum != 0,
            "projectile owner audit consumes every nonzero definition entry");
        AssertEqual(0, forbiddenBus.ReadAttempts,
            "projectile definition entries never use the removed mechanics fallback");

        var unknown = new RoomEnemyProjectileSlot(0)
        {
            Kind = RoomEnemyProjectileKind.None,
        };
        TargetInvocationException unknownException = AssertThrows<TargetInvocationException>(
            () => read.Invoke(null, [unknown, (ushort)0xffff]),
            "uncatalogued projectile mechanics fail explicitly");
        AssertTrue(unknownException.InnerException is InvalidDataException,
            "uncatalogued projectile mechanics surface an InvalidDataException");
        AssertTrue(unknownException.InnerException!.Message.Contains("uncompiled", StringComparison.Ordinal),
            "uncatalogued projectile mechanics identify the missing compiled owner");
        AssertEqual(0, forbiddenBus.ReadAttempts,
            "uncatalogued projectile mechanics fail without probing cartridge data");

        Console.WriteLine(
            $"Enemy-projectile mechanics owners: {definitionCount} translated definitions " +
            $"and {entryCount} initial/touch/shot entries resolve without a bank-$86 " +
            "fallback; unknown entries fail explicitly.");
    }

    /// <summary>Fails on every cartridge read or write so mechanics lookups cannot silently fall back to ROM.</summary>
    private sealed class EnemyProjectileMechanicsFallbackForbiddenBus : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of forbidden address-space reads attempted by the audited lookups.</summary>
        internal int ReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the same fail-fast check as address-space reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Counts and rejects a cartridge read instead of returning fallback mechanics data.</summary>
        /// <param name="address">Full cartridge address that the caller attempted to read.</param>
        /// <returns>This guard never returns a byte because every read is forbidden.</returns>
        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Projectile mechanics attempted forbidden cartridge read ${address:X6}.");
        }

        /// <summary>Rejects writes because this audit expects mechanics resolution to be read-only.</summary>
        /// <param name="address">Full cartridge address that the caller attempted to write.</param>
        /// <param name="value">Byte the caller attempted to store.</param>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException(
                $"Projectile mechanics attempted unexpected write ${address:X6}.");
    }
}
