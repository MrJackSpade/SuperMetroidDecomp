using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static EnemyProjectileProgramOracleVerification;
using static SuiteTimingVerification;

/// <summary>Represents chozo tourian dust instruction program definitions verification.</summary>
internal static class ChozoTourianDustInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies chozo tourian dust instruction program definitions.</summary>
    internal static void VerifyChozoTourianDustInstructionProgramDefinitions() =>
        Suite(nameof(VerifyChozoTourianDustInstructionProgramDefinitions), () => VerifyChozoTourianDustInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Verifies chozo tourian dust instruction program definitions.</summary>
    internal static void VerifyChozoTourianDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = ReadNativeEnemyProjectileProgramWord(rom, definition.Address);
            AssertEqual(definition.Value, native,
                $"Chozo/Tourian dust mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewChozoTourianDustInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 4);

        var statue = new RoomEnemySlot(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0x0080)),
        };
        enemies.ChozoStatueBehavior.SpawnWreckedShipChozoFootstep(statue, (ushort)0);
        RoomEnemyProjectileSlot footstep = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep);
        Run(footstep, 1);
        AssertEqual((ushort)0x0104, footstep.Position.X.Whole,
            "real Chozo footstep applies the compiled X mask and center");
        AssertEqual((ushort)0x009d, footstep.Position.Y.Whole,
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
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(alternate,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate,
            (ushort)0);
        alternate.Position = alternate.Position.WithWhole(new(0x0100, 0x0080));
        Run(alternate, 1);
        AssertEqual((ushort)0x0104, alternate.Position.X.Whole,
            "alternate spike explosion applies the compiled X mask and center");
        AssertEqual((ushort)0x0081, alternate.Position.Y.Whole,
            "alternate spike explosion applies the compiled Y mask and center");
        Run(alternate, 5);
        AssertEqual((ushort)0xaefa, alternate.InstructionPointer,
            "alternate spike explosion reaches deletion after six authored poses");
        Run(alternate, 1);
        AssertTrue(!alternate.IsActive,
            "alternate spike explosion deletes after its sixth authored pose");

        enemies.TourianUnlockBehavior.SpawnTourianDescentDust();
        RoomEnemyProjectileSlot dust = enemies.Projectiles.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.TourianStatueDescentDust);
        Run(dust, 1);
        AssertEqual((ushort)132, dust.Position.X.Whole,
            "Tourian dust resets to its producer origin before applying random X");
        AssertEqual((ushort)188, dust.Position.Y.Whole,
            "Tourian dust resets to its producer origin before applying random Y");
        AssertEqual((ushort)64, dust.GeneralTimer,
            "Tourian dust initializes its exact native loop count");
        Run(dust, 3);
        AssertEqual((ushort)0xaf30, dust.InstructionPointer,
            "Tourian dust reaches its counted branch after four authored poses");
        Run(dust, 1);
        AssertEqual((ushort)63, dust.GeneralTimer,
            "Tourian dust decrements its native loop count once per four-pose cycle");
        AssertEqual((ushort)132, dust.Position.X.Whole,
            "Tourian dust resets and randomizes X again at the next cycle");
        AssertEqual((ushort)188, dust.Position.Y.Whole,
            "Tourian dust resets and randomizes Y again at the next cycle");
        Run(dust, 251);
        AssertTrue(dust.IsActive,
            "Tourian dust remains active through all 256 authored display frames");
        AssertEqual((ushort)0xaf30, dust.InstructionPointer,
            "Tourian dust reaches its final counted branch after 256 display frames");
        Run(dust, 1);
        AssertTrue(!dust.IsActive,
            "Tourian dust deletes on the tick after its 256th display frame");

        AssertEqual(0, guard.ObservedWords.Count,
            "Chozo/Tourian dust selectors require no runtime cartridge reads");
        for (int index = 0; index < ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                $"Chozo/Tourian dust selector $86:{address:X4} is compiled");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                $"Chozo/Tourian dust selector $86:{address:X4} preserves its native operand");
        }
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
            "compiled native selectors pass with zero runtime ROM reads.");

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            }
        }
    }

    /// <summary>Probes chozo tourian dust instruction mechanics allocation.</summary>
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

    /// <summary>Creates chozo tourian dust instruction read guard.</summary>
    private static CartridgeReadGuard NewChozoTourianDustInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ChozoTourianDustInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Chozo/Tourian dust mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount, ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Chozo tourian dust instruction mechanics command.</summary>
    [VerificationCommand("--chozo-tourian-dust-instruction-mechanics")]
    internal static int ChozoTourianDustInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyChozoTourianDustInstructionProgramDefinitions), () => VerifyChozoTourianDustInstructionProgramDefinitions());
        return 0;
    }
}
