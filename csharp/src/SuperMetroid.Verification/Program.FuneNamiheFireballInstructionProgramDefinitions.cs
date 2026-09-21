using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFuneNamiheFireballInstructionProgramDefinitions()
    {
        VerifyFuneNamiheFireballInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyFuneNamiheFireballInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FuneNamiheFireballInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FuneNamiheFireballInstructionMechanicsWord definition =
                FuneNamiheFireballInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadFuneNamiheFireballInstructionWord(rom, definition.Address),
                $"Fune/Namihe fireball mechanics word $86:{definition.Address:X4}");
        }

        var guard = new FuneNamiheFireballInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnFuneNamiheFireball",
            flags)!.CreateDelegate<Action<RoomEnemySlot, bool, RoomEnemyProjectileKind>>(
                enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            flags)!;
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 0x0120;
        source.YPosition = 0x0080;
        source.Parameter2 = 0;

        foreach (RoomEnemyProjectileKind kind in new[]
                 {
                     RoomEnemyProjectileKind.FuneFireball,
                     RoomEnemyProjectileKind.NamiheFireball,
                 })
        {
            source.Parameter1 = kind == RoomEnemyProjectileKind.FuneFireball
                ? (ushort)0
                : (ushort)1;
            foreach (bool movingRight in new[] { false, true })
            {
                foreach (RoomEnemyProjectileSlot candidate in enemies.EnemyProjectiles)
                    candidate.Clear();

                spawn(source, movingRight, kind);
                RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
                    candidate => candidate.Kind == kind);
                ushort program = movingRight
                    ? FuneNamiheFireballInstructionProgramDefinitions.Right
                    : FuneNamiheFireballInstructionProgramDefinitions.Left;
                AssertEqual(program, projectile.InstructionPointer,
                    $"{kind} {(movingRight ? "right" : "left")} selects named program");

                object?[] arguments = [projectile, null, (ushort)0, (ushort)0];
                for (int frame = 0; frame < 4; frame++)
                {
                    projectile.InstructionTimer = 1;
                    process.Invoke(enemies, arguments);
                }
                AssertEqual(unchecked((ushort)(program + 4)), projectile.InstructionPointer,
                    $"{kind} {(movingRight ? "right" : "left")} loops to first frame");
            }
        }

        AssertEqual(
            FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Fune/Namihe fireball spritemap operands remain cartridge reads");
        for (int index = 0;
             index < FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = FuneNamiheFireballInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Fune/Namihe presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Fune/Namihe fireball mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(
                0xde98),
            "Fune/Namihe fireball spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(
                0xdeb6),
            "adjacent Fune/Namihe velocity table is rejected as mechanics");

        _ = ProbeFuneNamiheFireballInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFuneNamiheFireballInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fune/Namihe allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fune/Namihe fireball lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fune/Namihe fireball instruction mechanics: ten compiled words, both " +
            "directional loops for both species, and six live spritemap reads pass " +
            "with mechanics bytes forbidden.");
    }

    private static int ProbeFuneNamiheFireballInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FuneNamiheFireballInstructionProgramDefinitions.Left
                    : FuneNamiheFireballInstructionProgramDefinitions.Right);
        }
        return checksum;
    }

    private static ushort ReadFuneNamiheFireballInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class FuneNamiheFireballInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FuneNamiheFireballInstructionProgramDefinitions
                .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fune/Namihe mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FuneNamiheFireballInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = FuneNamiheFireballInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
