using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.CeresBlastPlacementVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresSpawnerScheduleVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Ceres destruction actors, placement, timing, and spawn schedules.</summary>
internal static class CeresExplosionDefinitionsVerification
{
    /// <summary>Compares 70 native words and completes the real destruction scene without definition rereads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres explosion definitions")]
    internal static void VerifyCeresExplosionDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        Suite(nameof(VerifyActor), () => VerifyActor(retail, 0xcebb, CeresExplosionDefinitions.InitialActor, "initial explosion"));
        Suite(nameof(VerifyActor), () => VerifyActor(retail, 0xcec1, CeresExplosionDefinitions.RepeatingActor, "repeating explosion"));
        Suite(nameof(VerifyActor), () => VerifyActor(retail, 0xcec7, CeresExplosionDefinitions.FinalWaveActor, "final-wave explosion"));
        Suite(nameof(VerifyActor), () => VerifyActor(retail, 0xcf2d, CeresExplosionDefinitions.StationBlastActor, "station blast"));
        Suite(nameof(VerifyActor), () => VerifyActor(retail, 0xcf33, CeresExplosionDefinitions.SpawnerActor, "explosion spawner"));

        Suite(nameof(VerifyCeresInitialBlastX), () => VerifyCeresInitialBlastX(retail));
        Suite(nameof(VerifyCeresInitialBlastY), () => VerifyCeresInitialBlastY(retail));
        Suite(nameof(VerifyCeresInitialBlastDelay), () => VerifyCeresInitialBlastDelay(retail));
        Suite(nameof(VerifyCeresFinalBlastX), () => VerifyCeresFinalBlastX(retail));
        Suite(nameof(VerifyCeresFinalBlastDelay), () => VerifyCeresFinalBlastDelay(retail));

        Suite(nameof(VerifyCeresBurstLayout), () => VerifyCeresBurstLayout(retail));

        Suite(nameof(VerifyCeresSpawnerSchedule), () => VerifyCeresSpawnerSchedule(retail));

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresExplosionDefinitions.InitialExplosion(
                CeresExplosionDefinitions.InitialExplosionCount),
            "initial explosion definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresExplosionDefinitions.RepeatingExplosion(
                CeresExplosionDefinitions.RepeatingExplosionCount),
            "repeating explosion definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresExplosionDefinitions.FinalExplosion(
                CeresExplosionDefinitions.FinalExplosionCount),
            "final explosion definition boundary");

        var guard = NewCeresExplosionDefinitionReadGuard(retail);
        var state = CreateRetailDestructionFixture(guard);
        for (int frame = 0; frame < 5000 && !state.Finished; frame++)
            state.Step();
        AssertTrue(state.Finished, "Ceres destruction completes through production actor paths");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ceres destruction never rereads compiled explosion definitions");

        Console.WriteLine(
            "  Ceres explosion definitions: 70 native words and the complete production cinematic pass with actor, placement, timing and callback metadata reads forbidden.");

        static void VerifyActor(
            SuperMetroidAddressSpace source,
            ushort definitionPointer,
            CeresExplosionActorDefinition definition,
            string name)
        {
            // The six-byte header's first word (initialization callback) has no compiled owner.
            int address = CeresExplosionDefinitions.NativeBank | definitionPointer;
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 2)), definition.PreInstruction,
                $"{name} pre-instruction callback");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 4)), definition.InstructionList,
                $"{name} instruction list");
        }
    }

    /// <summary>Compares repeating and final-wave placement records and their index bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres burst layout")]
    internal static void VerifyCeresBurstLayout(ISnesAddressSpace retail)
    {
        for (int index = 0; index < CeresExplosionDefinitions.RepeatingExplosionCount; index++)
        {
            CeresExplosionPlacement placement = CeresExplosionDefinitions.RepeatingExplosion(index);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((0x8bc4eb + index * 4)), unchecked((ushort)placement.Offset.X),
                $"repeating explosion {index} X offset");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((0x8bc4ed + index * 4)), unchecked((ushort)placement.Offset.Y),
                $"repeating explosion {index} Y offset");
            AssertEqual((ushort)1, placement.DelayFrames,
                $"repeating explosion {index} initial instruction delay");
        }

        for (int index = 0; index < CeresExplosionDefinitions.FinalExplosionCount; index++)
        {
            CeresExplosionPlacement placement = CeresExplosionDefinitions.FinalExplosion(index);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((0x8bc57a + index * 2)), unchecked((ushort)placement.Offset.Y),
                $"final explosion {index} Y offset");
        }

        foreach (int invalid in new[] { int.MinValue, -1, 8, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresExplosionDefinitions.RepeatingExplosion(invalid), "repeating layout bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 4, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresExplosionDefinitions.FinalExplosion(invalid), "final layout bounds");
    }

    /// <summary>Rejects runtime reads from migrated explosion actor and placement records.</summary>
    private static CartridgeReadGuard NewCeresExplosionDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x8bc46b and < 0x8bc489 or
                    >= 0x8bc4a9 and < 0x8bc4ab or
                    >= 0x8bc4eb and < 0x8bc50b or
                    >= 0x8bc56a and < 0x8bc582 or
                    >= 0x8bce35 and < 0x8bce4b or
                    >= 0x8bcebb and < 0x8bcecd or
                    >= 0x8bcf2d and < 0x8bcf39,
                address => $"Ceres explosion reread compiled definition byte ${address:X6}.");

    /// <summary>Runs the Ceres destruction definition comparison with the retail cartridge.</summary>
    [VerificationCommand("--ceres-explosion-definitions")]
    internal static int CeresExplosionDefinitionsCommand()
    {
        Suite(nameof(VerifyCeresExplosionDefinitions), () => VerifyCeresExplosionDefinitions());
        return 0;
    }
}
