using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileProgramMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

/// <summary>Checks Fake Kraid's typed spit and spike programs against the retail lists through the real producers.</summary>
internal static class FakeKraidProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Fake Kraid projectile program verification suite.</summary>
    internal static void VerifyFakeKraidProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Spawns all three projectiles through Fake Kraid's real producers and plays their typed programs: each shows its
    /// native pose operand for the list's authored duration, then holds with the timer stopped where the list sleeps,
    /// and the shared shot reaction still deletes it. No production step reads the lists from ROM.
    /// </summary>
    internal static void VerifyFakeKraidProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyFakeKraidProjectileMechanicsMapping), () => VerifyFakeKraidProjectileMechanicsMapping(rom));
        Suite(nameof(VerifyFakeKraidProjectilePresentationAddresses), () => VerifyFakeKraidProjectilePresentationAddresses());

        var guard = new CartridgeReadGuard(rom)
            .Forbid(FakeKraidProjectileInstructionProgramDefinitionsToolingConstants.IsCompiledMechanicsByte,
                address => $"Production read compiled Fake Kraid mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                FakeKraidProjectileInstructionProgramDefinitionsTooling.PresentationWordCount,
                FakeKraidProjectileInstructionProgramDefinitionsTooling.PresentationWordAddress);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);

        var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            TileIndexBase = 0x0200,
            Colouring = SharedSpriteColouring.Frozen,
        };
        var state = new FakeKraidEnemyState();

        AssertTrue(enemies.FakeKraidBehavior.SpawnFakeKraidSpit(source, 12, new EightEightVector<RoomPixels>(0x0180, -0x0200)),
            "real Fake Kraid spit producer allocates its projectile");
        state.FacingDelta = -4;
        AssertTrue(enemies.FakeKraidBehavior.SpawnFakeKraidSpike(source, state, 0),
            "real Fake Kraid left-spike producer allocates its projectile");
        state.FacingDelta = 4;
        AssertTrue(enemies.FakeKraidBehavior.SpawnFakeKraidSpike(source, state, 1),
            "real Fake Kraid right-spike producer allocates its projectile");

        var cases = new[]
        {
            new FakeKraidProjectileInstructionProgramDefinitionsCase(Kind:RoomEnemyProjectileKind.FakeKraidSpit, Pose:FakeKraidProjectilePose.Spit,
List:FakeKraidProjectileInstructionProgramToolingDefinitions.Spit),
            new FakeKraidProjectileInstructionProgramDefinitionsCase(Kind:RoomEnemyProjectileKind.FakeKraidSpikeLeft, Pose:FakeKraidProjectilePose.SpikeLeft,
List:FakeKraidProjectileInstructionProgramDefinitionsConstants.SpikeLeft),
            new FakeKraidProjectileInstructionProgramDefinitionsCase(Kind:RoomEnemyProjectileKind.FakeKraidSpikeRight, Pose:FakeKraidProjectilePose.SpikeRight,
List:FakeKraidProjectileInstructionProgramDefinitionsConstants.SpikeRight),
        };

        foreach (var testCase in cases)
        {
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(
                candidate => candidate.Kind == testCase.Kind);
            AssertTrue(projectile.Animation is FakeKraidProjectileAnimation animation && animation.Pose == testCase.Pose,
                $"{testCase.Kind} plays its typed pose program");
            AssertTrue(projectile.Animation is not null, $"{testCase.Kind} runs no native list");

            RunForcedTick(projectile);
            ushort operand = unchecked((ushort)(testCase.List + 2));
            AssertEqual(operand, projectile.PresentationOperandAddress, $"{testCase.Kind} shows its native pose operand");
            AssertEqual(ReadFakeKraidProjectileInstructionWord(rom, testCase.List), projectile.InstructionTimer.RawValue,
                $"{testCase.Kind} pose lasts the native duration");

            RunForcedTick(projectile);
            AssertEqual((ushort)NativeEnemyProjectileInstruction.Sleep, ReadFakeKraidProjectileInstructionWord(rom, unchecked((ushort)(testCase.List + 4))),
                $"{testCase.Kind} native list sleeps after its pose");
            AssertEqual((ushort)0, projectile.InstructionTimer.RawValue,
                $"{testCase.Kind} held pose leaves the timer stopped, as the native sleep does");
            AssertEqual(operand, projectile.PresentationOperandAddress, $"{testCase.Kind} held pose keeps showing");

            projectile.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
            RunForcedTick(projectile);
            AssertTrue(!projectile.IsActive,
                $"{testCase.Kind} shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Fake Kraid frames never read presentation operands from ROM");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Fake Kraid and shared-delete mechanics byte");

        Console.WriteLine(
            "Fake Kraid projectile programs: all three real spit/spike producers show their native pose operand " +
            "for the native duration, hold where the lists sleep, and delete on the shared shot reaction without " +
            "instruction ROM reads.");

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = WrappingCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(InstalledFixtures.AnimationContinuation), Position16<RoomPixels>.Zero);
        }
    }

    /// <summary>Reads one bank-$86 projectile program word from the retail ROM as an exact verification oracle.</summary>
    internal static ushort ReadFakeKraidProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);

    /// <summary>Runs the Fake Kraid projectile program command and returns its suite status.</summary>
    [VerificationCommand("--fake-kraid-projectile-instruction-mechanics")]
    internal static int FakeKraidProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions());
        return 0;
    }
}
