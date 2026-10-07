using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    private static void VerifyShaktoolProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < ShaktoolProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ShaktoolProjectileInstructionMechanicsWord definition =
                ShaktoolProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Shaktool attack-circle mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new ShaktoolProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var segment = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = RoomEnemySystem.ShaktoolDefinition,
            XPosition = 0x0100,
            YPosition = 0x0080,
            VariableD = 0,
        };
        enemies.SpawnUnusedShaktoolAttackCircles(segment);
        RoomEnemyProjectileSlot front = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.ShaktoolAttackFrontCircle);
        RoomEnemyProjectileSlot middle = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle);
        RoomEnemyProjectileSlot back = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.ShaktoolAttackBackCircle);
        AssertEqual(unchecked((ushort)(front.SlotIndex * 2)), middle.Variable0,
            "real middle-circle producer retains the front native slot index");
        AssertEqual(middle.Variable0, back.Variable0,
            "real back-circle producer retains the same front native slot index");

        Run(front, 3);
        AssertEqual((ushort)0xbd74, front.InstructionPointer,
            "front circle reaches its loop branch after three authored poses");
        Run(front, 1);
        AssertEqual((ushort)0xbd74, front.InstructionPointer,
            "front circle branch displays the held pose in the same tick");

        Run(middle, 1);
        AssertEqual((ushort)0xbd7c, middle.InstructionPointer,
            "middle circle completes its delayed first pose");
        Run(middle, 1);
        AssertEqual(
            EnemyProjectileCodePointers.PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving,
            middle.PreInstruction,
            "middle circle installs its linked movement callback");
        Run(middle, 1);
        AssertEqual((ushort)0xbd88, middle.InstructionPointer,
            "middle circle reaches its loop branch");
        Run(middle, 1);
        AssertEqual((ushort)0xbd88, middle.InstructionPointer,
            "middle circle branch displays the held pose in the same tick");

        Run(back, 1);
        AssertEqual((ushort)0xbd90, back.InstructionPointer,
            "back circle completes its delayed first pose");
        Run(back, 1);
        AssertEqual(
            EnemyProjectileCodePointers.PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving,
            back.PreInstruction,
            "back circle installs its linked movement callback");
        AssertEqual((ushort)0xbd98, back.InstructionPointer,
            "back circle reaches its loop branch after callback installation");
        Run(back, 1);
        AssertEqual((ushort)0xbd98, back.InstructionPointer,
            "back circle branch displays the held pose in the same tick");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "ShaktoolProjectile execution performs no live spritemap operand reads");
        AssertEqual(ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "ShaktoolProjectile executes every native visual operand");
        for (int index = 0; index < ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"ShaktoolProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "ShaktoolProjectile has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "ShaktoolProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Shaktool attack-circle mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ShaktoolProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xbd6a),
            "Shaktool attack-circle spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ShaktoolProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xbd9c),
            "Shaktool initializer code is rejected as mechanics");

        _ = ProbeShaktoolProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeShaktoolProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Shaktool projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Shaktool projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Shaktool projectile instruction mechanics: eighteen compiled words, all " +
            "three real linked-circle producers and eight native installed operands pass with " +
            "mechanics and presentation reads forbidden.");

        ushort NativeWord(ushort address) => (ushort)(rom.ReadByte(0x860000 | address) |
            rom.ReadByte(0x860000 | (ushort)(address + 1)) << 8);

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                ushort cursor = projectile.InstructionPointer;
                while (NativeWord(cursor) >= 0x8000)
                {
                    ushort opcode = NativeWord(cursor);
                    if (opcode == EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY)
                        cursor = NativeWord((ushort)(cursor + 2));
                    else if (opcode == EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY)
                        cursor += 4;
                    else throw new InvalidOperationException($"Unexpected native Shaktool control {opcode:X4}");
                }
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    private static int ProbeShaktoolProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShaktoolProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ShaktoolProjectileInstructionProgramDefinitions.Front
                    : ShaktoolProjectileInstructionProgramDefinitions.Back);
        }
        return checksum;
    }

    private sealed class ShaktoolProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (ShaktoolProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Shaktool mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = ShaktoolProjectileInstructionProgramDefinitions
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
