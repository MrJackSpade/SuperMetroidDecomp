using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;

/// <summary>Checks six gunship liftoff dust programs and their compiled visual selectors against bank $86.</summary>
internal static class GunshipDustInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the gunship dust program comparison.</summary>
    internal static void VerifyGunshipDustInstructionProgramDefinitions() =>
        Suite(nameof(VerifyGunshipDustInstructionProgramDefinitions), () => VerifyGunshipDustInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks native control words, live frame timing and deletion, and guarded selector access.</summary>
    internal static void VerifyGunshipDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GunshipDustInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                GunshipDustInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                SnesLittleEndian.Word(GunshipDustInstructionProgramDefinitionsRomByte(rom, EnemyProjectileCodePointers.BankBase | definition.Address), GunshipDustInstructionProgramDefinitionsRomByte(rom, EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))))));
            AssertEqual(definition.Value, native,
                $"Gunship dust mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewGunshipDustInstructionGuard(rom);

        for (int programIndex = 0;
             programIndex < GunshipDustInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort parameter = unchecked((ushort)(programIndex * 2));
            GunshipDustInstructionProgramDefinition program =
                GunshipDustInstructionProgramDefinitions.Program(programIndex);
            RoomEnemySystem enemies = NewSystem();
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)) };
            enemies.Projectiles.SpawnGunshipLiftoffDustCloud(parameter, samus);
            RoomEnemyProjectileSlot dust = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind ==
                    RoomEnemyProjectileKind.GunshipLiftoffDustCloud);
            AssertEqual(
                GunshipDustInstructionProgramDefinitions.InitialForParameter(parameter),
                dust.InstructionPointer,
                "real gunship dust producer selects its named program");
            AssertEqual(program.Initial, dust.InstructionPointer,
                "gunship dust parameter and program catalogs agree");

            for (int frame = 0; frame < program.Durations.Length; frame++)
            {
                RunForcedTick(enemies, dust);
                AssertEqual(program.Durations[frame], dust.InstructionTimer.RawValue,
                    $"gunship dust program {programIndex} frame {frame} duration");
                AssertEqual(
                    unchecked((ushort)(program.FirstFrame + (frame + 1) * 4)),
                    dust.InstructionPointer,
                    $"gunship dust program {programIndex} frame {frame} continuation");
            }

            AssertEqual((ushort)1, dust.GeneralTimer,
                $"gunship dust program {programIndex} initializes one animation pass");
            RunForcedTick(enemies, dust);
            AssertTrue(!dust.IsActive,
                $"gunship dust program {programIndex} exits its decrement branch and deletes");
        }

        AssertEqual(0, guard.ObservedWords.Count, "compiled gunship visuals require no cartridge reads");
        for (int index = 0;
             index < GunshipDustInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GunshipDustInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "compiled gunship selector exists");
            AssertEqual(ReadVerificationWord(rom, (0x86 << 16) | address), selector,
                "compiled gunship selector matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids gunship dust mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(0xa19d),
            "gunship dust spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(0xa2e2),
            "gunship dust selector-table data is rejected as mechanics");

        _ = ProbeGunshipDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGunshipDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "gunship dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed gunship dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Gunship dust instruction mechanics: seventy-six compiled words, all six " +
            "real producers, frame loops, deletions, and forty-six native compiled spritemap operands pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            return enemies;
        }

        void RunForcedTick(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
        }
    }

    /// <summary>Reads a bank $86 byte for the independent native instruction comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Gunship dust instruction program definitions")]
    private static byte GunshipDustInstructionProgramDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Repeats warmed gunship dust lookups for the caller's allocation measurement.</summary>
    private static int ProbeGunshipDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GunshipDustInstructionProgramDefinitions.Index0
                    : GunshipDustInstructionProgramDefinitions.IndexA);
        }
        return checksum;
    }

    /// <summary>Forbids compiled mechanics rereads and observes native spritemap operands.</summary>
    private static CartridgeReadGuard NewGunshipDustInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(GunshipDustInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled gunship dust mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, GunshipDustInstructionProgramDefinitions.PresentationWordCount, GunshipDustInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Dispatches the gunship liftoff dust instruction verification command.</summary>
    [VerificationCommand("--gunship-dust-instruction-mechanics")]
    internal static int GunshipDustInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyGunshipDustInstructionProgramDefinitions), () => VerifyGunshipDustInstructionProgramDefinitions());
        return 0;
    }
}
