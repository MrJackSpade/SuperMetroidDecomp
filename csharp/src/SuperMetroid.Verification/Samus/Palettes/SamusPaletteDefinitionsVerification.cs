using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementDefinitionsVerification;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>
/// Audits named Samus palette table spans and their shared destinations against the
/// retail ROM when it is available.
/// </summary>
internal static class SamusPaletteDefinitionsVerification
{
    /// <summary>
    /// Audits every shared Samus palette table through the complete record count consumed
    /// by the translated handlers. Existing state-machine checks then prove those named
    /// ranges still drive hurt, visor, boost, X-ray, Crystal Flash, death, and Hyper Beam.
    /// </summary>
    internal static void VerifySamusPaletteRomData()
    {
        AssertEqual(
            SamusPaletteDefinitions.Common.SamusObjPaletteStart +
                SamusPaletteDefinitions.Common.VisorColorOffset,
            XrayScopeDefinitionsPaletteConstants.VisorCgramIndex,
            "shared visor destination agrees with X-ray catalog");
        AssertEqual(
            SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount,
            SamusPaletteDefinitions.HyperBeamFx.FrameCount,
            "full-body and projectile Hyper Beam cycles have equal record counts");
        AssertTrue(
            SamusPaletteDefinitions.SuitPickup.WhiteRed >=
                SamusPaletteDefinitions.SuitPickup.InitialRed &&
            SamusPaletteDefinitions.SuitPickup.WhiteGreen >=
                SamusPaletteDefinitions.SuitPickup.VariaGreen &&
            SamusPaletteDefinitions.SuitPickup.WhiteBlue >=
                SamusPaletteDefinitions.SuitPickup.GravityBlue,
            "suit-pickup white ramp advances every enabled component");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus palette ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        TouchRange(bus, SamusPaletteDefinitions.Common.NormalSuitPointers,
            3 * sizeof(ushort), "normal suit palette pointers");
        TouchRange(bus, SamusSourceDefinitions.HurtFlashColors.Value,
            SamusPaletteDefinitions.Common.ColorsPerObjPalette * sizeof(ushort),
            "Samus hurt-flash colors");
        TouchRange(bus, SamusSourceDefinitions.IntroHurtFlashColors.Value,
            SamusPaletteDefinitions.Common.ColorsPerObjPalette * sizeof(ushort),
            "intro Samus colors");
        TouchRange(bus, SamusSourceDefinitions.VisorColors.Value,
            SamusPaletteDefinitions.Visor.CycleEndByteOffset,
            "Samus visor colors");

        foreach (int table in new[]
        {
            SamusPaletteDefinitions.FullBodyCycles.SpeedBoostPointers,
            SamusPaletteDefinitions.FullBodyCycles.ScrewAttackLists,
            SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLists,
            SamusPaletteDefinitions.FullBodyCycles.StoredShineLists,
            SamusPaletteDefinitions.FullBodyCycles.ActiveShinesparkLists,
        })
        {
            TouchRange(bus, table, 3 * sizeof(ushort),
                $"suit-indexed palette table ${table:X6}");
        }
        TouchRange(bus, SamusPaletteDefinitions.FullBodyCycles.HyperBeamPointers,
            SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount * sizeof(ushort),
            "full-body Hyper Beam palette pointers");

        TouchRange(bus, SamusPaletteDefinitionsHyperBeamFxConstants.ObjectDefinition,
            2 * sizeof(ushort), "Hyper Beam palette-FX object definition");
        TouchRange(bus,
            SamusPaletteDefinitionsBanksConstants.PaletteFx |
                SamusPaletteDefinitions.HyperBeamFx.InitialList,
            2 * sizeof(ushort) +
                SamusPaletteDefinitions.HyperBeamFx.FrameCount *
                    SamusPaletteDefinitions.HyperBeamFx.FrameByteCount +
                2 * sizeof(ushort),
            "Hyper Beam palette-FX instruction stream");

        TouchRange(bus, SamusSourceDefinitions.SuitPointers.Value,
            3 * SamusPaletteDefinitions.Death.PaletteCount * sizeof(ushort),
            "suited death palette pointers");
        TouchRange(bus, SamusSourceDefinitions.SuitlessPointers.Value,
            SamusPaletteDefinitions.Death.PaletteCount * sizeof(ushort),
            "suitless death palette pointers");
        TouchRange(bus, SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value,
            9 * 2, "death explosion timer/palette records");
        TouchRange(bus, SamusSourceDefinitions.DeathWhiteoutShades.Value,
            SamusPaletteDefinitions.Death.WhiteoutShadeCount * sizeof(ushort),
            "death whiteout shades");

        TouchRange(bus, SamusPaletteDefinitions.CrystalFlash.BeamPalettePointers,
            SuperMetroid.AssetExtraction.Samus.BeamTileAtlasDefinitions.SelectionCount * sizeof(ushort),
            "Crystal Flash restored beam palettes");
        TouchRange(bus, SamusSourceDefinitions.CrystalFlashBodyRecords.Value,
            SamusPaletteDefinitions.CrystalFlash.BodyRecordCount *
                SamusPaletteDefinitions.CrystalFlash.BodyRecordByteCount,
            "Crystal Flash body palette records");
        TouchRange(bus, SamusSourceDefinitions.BubblePointers.Value,
            SamusPaletteDefinitions.CrystalFlash.BubblePaletteCount * sizeof(ushort),
            "Crystal Flash bubble palette pointers");

        TouchRange(bus, SamusSourceDefinitions.PowerBombPreExplosionColors.Value,
            SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount *
                SamusSourceDefinitions.PowerBombBytesPerColor,
            "Power Bomb pre-explosion fixed colors");
        TouchRange(bus, SamusSourceDefinitions.PowerBombExplosionColors.Value,
            SamusPaletteDefinitions.PowerBomb.ExplosionColorCount *
                SamusSourceDefinitions.PowerBombBytesPerColor,
            "Power Bomb explosion fixed colors");

        Console.WriteLine(
            "  Samus palette ROM data: suit, visor, boost, shine, Crystal Flash, death, Hyper Beam, and Power Bomb ranges are valid.");
    }
}
