using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks suit-indexed full-body palette lists and their live CGRAM cycles.</summary>
internal static class FullBodyPalettePointerListsVerification
{
    /// <summary>Compares four native pointer families with the cartridge and checks runtime palette updates without table reads.</summary>
    internal static void VerifyFullBodyPalettePointerLists()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => (uint)(address - SamusPaletteDefinitions.FullBodyCycles.ScrewAttackLists) < 0x2a ||
                (uint)(address - SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLists) < 0x1e ||
                (uint)(address - SamusPaletteDefinitions.FullBodyCycles.StoredShineLists) < 0x2a ||
                (uint)(address - SamusPaletteDefinitions.FullBodyCycles.ActiveShinesparkLists) < 0x1e,
            address => $"Runtime read of compiled full-body palette pointer ${address:X6}.");
        SamusEquipmentFlags[] equipment =
        [
            0,
            SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit,
            (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        FullBodyPalettePointerListsFamily[] families =
        [
            new FullBodyPalettePointerListsFamily("Screw Attack", SamusPaletteDefinitions.FullBodyCycles.ScrewAttackLists, 0xda50, 0x9ca0, 6, true),
            new FullBodyPalettePointerListsFamily("Speed Booster", SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLists, 0xdaaf, 0x9b20, 4, false),
            new FullBodyPalettePointerListsFamily("stored shine", SamusPaletteDefinitions.FullBodyCycles.StoredShineLists, 0xdb16, 0x9ba0, 6, true),
            new FullBodyPalettePointerListsFamily("active shinespark", SamusPaletteDefinitions.FullBodyCycles.ActiveShinesparkLists, 0xdb7b, 0x9c20, 4, false),
        ];

        foreach (var family in families)
        {
            for (int suit = 0; suit < 3; suit++)
            {
                ushort suitOffset = (ushort)(suit * 2);
                ushort expectedList = (ushort)(family.FirstList + suit * family.Phases * 2);
                AssertEqual(expectedList, CartridgeImportSource.Require(rom).ReadCartridgeWord((family.Top + suitOffset)),
                    $"{family.Name} top-level list for suit {suit}");
                for (int phase = 0; phase < family.Phases; phase++)
                {
                    int shade = family.PingPong ? Math.Min(phase, 6 - phase) : phase;
                    ushort expectedPalette = (ushort)(family.FirstPalette + suit * 0x200 + shade * 0x20);
                    AssertEqual(expectedPalette,
                        CartridgeImportSource.Require(rom).ReadCartridgeWord((SamusPaletteDefinitionsBanksConstants.Movement | (expectedList + phase * 2))),
                        $"{family.Name} nested pointer for suit {suit}, phase {phase}");
                }
            }
        }

        foreach (SamusEquipmentFlags items in equipment)
        {
            int suit = items.HasAny(SamusEquipmentFlags.GravitySuit) ? 2 :
                items.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 0;
            for (int phase = 0; phase < 6; phase++)
            {
                var speed = new SamusHorizontalSpeedState { CyclePaletteStep = phase };
                var actual = new SamusColours();
                AssertTrue(speed.UpdateSpeedBoosterPalette(actual, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 1, EquippedItems = (items | SamusEquipmentFlags.ScrewAttack) }),
                    "Screw Attack copies a palette without reading either pointer table");
                int shade = Math.Min(phase, 6 - phase);
                AssertPaletteColors(rom, actual, 0x9ca0 + suit * 0x200 + shade * 0x20,
                    $"Screw Attack suit {suit}, phase {phase}");
                AssertEqual(phase == 5 ? 0 : phase + 1, speed.CyclePaletteStep,
                    "Screw Attack palette phase wraps after its sixth frame");
            }

            for (int phase = 0; phase < 4; phase++)
            {
                var speed = new SamusHorizontalSpeedState
                {
                    SpeedBoostCounter = new SamusSpeedBoostCounter(4, 0),
                    SpecialPaletteTimer = 1,
                    CyclePaletteStep = phase,
                };
                var actual = new SamusColours();
                AssertTrue(speed.UpdateSpeedBoosterPalette(actual, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = items }),
                    "active Speed Booster copies a palette without reading either pointer table");
                AssertPaletteColors(rom, actual, 0x9b20 + suit * 0x200 + phase * 0x20,
                    $"active Speed Booster suit {suit}, phase {phase}");
                AssertEqual(phase == 3 ? 3 : phase + 1, speed.CyclePaletteStep,
                    "active Speed Booster palette pins its fourth frame");
            }

            var stored = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { EquippedItems = items };
            AssertTrue(stored.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0)), "seed stored shine");
            for (int phase = 0; phase < 6; phase++)
            {
                var actual = new SamusColours();
                AssertTrue(stored.Shinespark.UpdatePalette(actual, items),
                    "stored shine copies a palette without reading either pointer table");
                int shade = Math.Min(phase, 6 - phase);
                AssertPaletteColors(rom, actual, 0x9ba0 + suit * 0x200 + shade * 0x20,
                    $"stored shine suit {suit}, phase {phase}");
                AssertEqual(phase == 5 ? 0 : phase + 1,
                    stored.Shinespark.PaletteStep, "stored shine cycles six phases");
            }

            var active = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { EquippedItems = items };
            AssertTrue(active.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0)), "seed active shine");
            active.Shinespark.BeginWindup(active);
            for (int phase = 0; phase < 4; phase++)
            {
                var actual = new SamusColours();
                AssertTrue(active.Shinespark.UpdatePalette(actual, items),
                    "active shinespark copies a palette without reading either pointer table");
                AssertPaletteColors(rom, actual, 0x9c20 + suit * 0x200 + phase * 0x20,
                    $"active shinespark suit {suit}, phase {phase}");
                AssertEqual(phase == 3 ? 0 : phase + 1,
                    active.Shinespark.PaletteStep, "active shinespark cycles four phases");
            }
        }

        AssertTrue(!SamusPaletteDefinitions.FullBodyCycles.TryScrewAttackPalettePointer(0, 12, out _),
            "Screw Attack does not catalogue a seventh phase");
        AssertTrue(!SamusPaletteDefinitions.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(0, 8, out _),
            "active Speed Booster does not catalogue a fifth phase");
        AssertTrue(!SamusPaletteDefinitions.FullBodyCycles.TryStoredShinePalettePointer(1, 0, out _),
            "stored shine does not catalogue an unaligned suit offset");
        AssertTrue(!SamusPaletteDefinitions.FullBodyCycles.TryActiveShinesparkPalettePointer(0, 1, out _),
            "active shinespark does not catalogue an unaligned phase offset");

        Console.WriteLine("Full-body palettes: all four suit-indexed list families match ROM and live CGRAM without pointer-table reads.");
    }

    /// <summary>Compares the sixteen Samus OBJ palette colors with one native palette source.</summary>
    private static void AssertPaletteColors(
        ISnesAddressSpace rom, SamusColours actual, int pointer, string context)
    {
        for (int color = 0; color < SamusColourSets.ColoursPerSet; color++)
            AssertEqual((ushort)(RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(rom),
                    SamusSourceDefinitions.PaletteBank.Value | (pointer + color * sizeof(ushort))) & 0x7fff),
                actual.ShownSuit(InstalledFixtures.SamusShadeSources)[color],
                $"{context} CGRAM color {color} matches ROM");
    }

    /// <summary>Runs full-body palette pointer and live color checks.</summary>
    [VerificationCommand("--full-body-palette-pointer-lists")]
    internal static int FullBodyPalettePointerListsCommand()
    {
        Suite(nameof(VerifyFullBodyPalettePointerLists), () => VerifyFullBodyPalettePointerLists());
        return 0;
    }
}
