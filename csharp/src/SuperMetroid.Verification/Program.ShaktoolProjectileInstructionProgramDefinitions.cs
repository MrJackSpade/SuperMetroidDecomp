using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolProjectileInstructionProgramDefinitions() =>
        VerifyShaktoolProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

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

        AssertEqual(ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Shaktool attack-circle spritemap operands remain cartridge reads");
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
            "three real linked-circle producers and eight live spritemap reads pass with " +
            "mechanics bytes forbidden.");

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
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
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

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
