using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyChozoTourianDustInstructionProgramDefinitions() =>
        VerifyChozoTourianDustInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyChozoTourianDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ChozoTourianDustInstructionMechanicsWord definition =
                ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Chozo/Tourian dust mechanics word $86:{definition.Address:X4}");
        }

        var guard = new ChozoTourianDustInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_nextRandom", instanceFlags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 4));
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", instanceFlags)!;
        MethodInfo spawnFootstep = typeof(RoomEnemySystem).GetMethod(
            "SpawnWreckedShipChozoFootstep", instanceFlags)!;
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeEnemyProjectileFromDefinition", staticFlags)!;

        var statue = new RoomEnemySlot(0)
        {
            XPosition = 0x0100,
            YPosition = 0x0080,
        };
        spawnFootstep.Invoke(enemies, [statue, (ushort)0]);
        RoomEnemyProjectileSlot footstep = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep);
        Run(footstep, 1);
        AssertEqual((ushort)0x0104, footstep.XPosition,
            "real Chozo footstep applies the compiled X mask and center");
        AssertEqual((ushort)0x009d, footstep.YPosition,
            "real Chozo footstep applies the compiled Y mask and center");
        AssertEqual((ushort)0xaece, footstep.InstructionPointer,
            "real Chozo footstep reaches its second authored pose");
        Run(footstep, 3);
        AssertEqual((ushort)0xaeda, footstep.InstructionPointer,
            "real Chozo footstep reaches deletion after four authored poses");
        Run(footstep, 1);
        AssertTrue(!footstep.IsActive,
            "real Chozo footstep deletes after its fourth authored pose");

        var alternate = new RoomEnemyProjectileSlot(0);
        initialize.Invoke(null,
        [
            alternate,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate,
            (ushort)0,
        ]);
        alternate.XPosition = 0x0100;
        alternate.YPosition = 0x0080;
        Run(alternate, 1);
        AssertEqual((ushort)0x0104, alternate.XPosition,
            "alternate spike explosion applies the compiled X mask and center");
        AssertEqual((ushort)0x0081, alternate.YPosition,
            "alternate spike explosion applies the compiled Y mask and center");
        Run(alternate, 5);
        AssertEqual((ushort)0xaefa, alternate.InstructionPointer,
            "alternate spike explosion reaches deletion after six authored poses");
        Run(alternate, 1);
        AssertTrue(!alternate.IsActive,
            "alternate spike explosion deletes after its sixth authored pose");

        enemies.SpawnTourianDescentDust();
        RoomEnemyProjectileSlot dust = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.TourianStatueDescentDust);
        Run(dust, 1);
        AssertEqual((ushort)132, dust.XPosition,
            "Tourian dust resets to its producer origin before applying random X");
        AssertEqual((ushort)188, dust.YPosition,
            "Tourian dust resets to its producer origin before applying random Y");
        AssertEqual((ushort)64, dust.GeneralTimer,
            "Tourian dust initializes its exact native loop count");
        Run(dust, 3);
        AssertEqual((ushort)0xaf30, dust.InstructionPointer,
            "Tourian dust reaches its counted branch after four authored poses");
        Run(dust, 1);
        AssertEqual((ushort)63, dust.GeneralTimer,
            "Tourian dust decrements its native loop count once per four-pose cycle");
        AssertEqual((ushort)132, dust.XPosition,
            "Tourian dust resets and randomizes X again at the next cycle");
        AssertEqual((ushort)188, dust.YPosition,
            "Tourian dust resets and randomizes Y again at the next cycle");
        Run(dust, 251);
        AssertTrue(dust.IsActive,
            "Tourian dust remains active through all 256 authored display frames");
        AssertEqual((ushort)0xaf30, dust.InstructionPointer,
            "Tourian dust reaches its final counted branch after 256 display frames");
        Run(dust, 1);
        AssertTrue(!dust.IsActive,
            "Tourian dust deletes on the tick after its 256th display frame");

        AssertEqual(ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Chozo/Tourian dust spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Chozo/Tourian dust mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ChozoTourianDustInstructionProgramDefinitions.ReadMechanicsWord(0xaecc),
            "Chozo footstep spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ChozoTourianDustInstructionProgramDefinitions.ReadMechanicsWord(0xaefc),
            "Chozo footstep initializer code is rejected as mechanics");

        _ = ProbeChozoTourianDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeChozoTourianDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Chozo/Tourian dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Chozo/Tourian dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Chozo/Tourian dust instruction mechanics: thirty-one compiled words, the " +
            "real footstep and Tourian producers, the authored alternate, and fourteen " +
            "live spritemap reads pass with mechanics bytes forbidden.");

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeChozoTourianDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ChozoTourianDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ChozoTourianDustInstructionProgramDefinitions.Footsteps
                    : ChozoTourianDustInstructionProgramDefinitions.TourianDescentDust);
        }
        return checksum;
    }

    private sealed class ChozoTourianDustInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ChozoTourianDustInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Chozo/Tourian dust mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = ChozoTourianDustInstructionProgramDefinitions
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
