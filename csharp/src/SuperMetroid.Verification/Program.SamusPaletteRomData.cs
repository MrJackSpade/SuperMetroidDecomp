using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Audits every shared Samus palette table through the complete record count consumed
    /// by the translated handlers. Existing state-machine checks then prove those named
    /// ranges still drive hurt, visor, boost, X-ray, Crystal Flash, death, and Hyper Beam.
    /// </summary>
    static void VerifySamusPaletteRomData()
    {
        AssertEqual(
            SamusPaletteRomData.Common.SamusObjPaletteStart +
                SamusPaletteRomData.Common.VisorColorOffset,
            SamusXrayRomData.Palette.VisorCgramIndex,
            "shared visor destination agrees with X-ray catalog");
        AssertEqual(
            SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteCount,
            SamusPaletteRomData.HyperBeamFx.FrameCount,
            "full-body and projectile Hyper Beam cycles have equal record counts");
        AssertTrue(
            SamusPaletteRomData.SuitPickup.WhiteRed >=
                SamusPaletteRomData.SuitPickup.InitialRed &&
            SamusPaletteRomData.SuitPickup.WhiteGreen >=
                SamusPaletteRomData.SuitPickup.VariaGreen &&
            SamusPaletteRomData.SuitPickup.WhiteBlue >=
                SamusPaletteRomData.SuitPickup.GravityBlue,
            "suit-pickup white ramp advances every enabled component");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus palette ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        TouchRange(bus, SamusPaletteRomData.Common.NormalSuitPointers,
            3 * sizeof(ushort), "normal suit palette pointers");
        TouchRange(bus, SamusPaletteRomData.HurtFlash.Colors,
            SamusPaletteRomData.Common.ColorsPerObjPalette * sizeof(ushort),
            "Samus hurt-flash colors");
        TouchRange(bus, SamusPaletteRomData.HurtFlash.IntroColors,
            SamusPaletteRomData.Common.ColorsPerObjPalette * sizeof(ushort),
            "intro Samus colors");
        TouchRange(bus, SamusPaletteRomData.Visor.Colors,
            SamusPaletteRomData.Visor.CycleEndByteOffset,
            "Samus visor colors");

        foreach (int table in new[]
        {
            SamusPaletteRomData.FullBodyCycles.SpeedBoostPointers,
            SamusPaletteRomData.FullBodyCycles.ScrewAttackLists,
            SamusPaletteRomData.FullBodyCycles.SpeedBoosterLists,
            SamusPaletteRomData.FullBodyCycles.StoredShineLists,
            SamusPaletteRomData.FullBodyCycles.ActiveShinesparkLists,
        })
        {
            TouchRange(bus, table, 3 * sizeof(ushort),
                $"suit-indexed palette table ${table:X6}");
        }
        TouchRange(bus, SamusPaletteRomData.FullBodyCycles.HyperBeamPointers,
            SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteCount * sizeof(ushort),
            "full-body Hyper Beam palette pointers");

        TouchRange(bus, SamusPaletteRomData.HyperBeamFx.ObjectDefinition,
            2 * sizeof(ushort), "Hyper Beam palette-FX object definition");
        TouchRange(bus,
            SamusPaletteRomData.Banks.PaletteFx |
                SamusPaletteRomData.HyperBeamFx.InitialList,
            2 * sizeof(ushort) +
                SamusPaletteRomData.HyperBeamFx.FrameCount *
                    SamusPaletteRomData.HyperBeamFx.FrameByteCount +
                2 * sizeof(ushort),
            "Hyper Beam palette-FX instruction stream");

        TouchRange(bus, SamusPaletteRomData.Death.SuitPointers,
            3 * SamusPaletteRomData.Death.PaletteCount * sizeof(ushort),
            "suited death palette pointers");
        TouchRange(bus, SamusPaletteRomData.Death.SuitlessPointers,
            SamusPaletteRomData.Death.PaletteCount * sizeof(ushort),
            "suitless death palette pointers");
        TouchRange(bus, SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices,
            9 * 2, "death explosion timer/palette records");
        TouchRange(bus, SamusPaletteRomData.Death.WhiteoutShades,
            SamusPaletteRomData.Death.WhiteoutShadeCount * sizeof(ushort),
            "death whiteout shades");

        TouchRange(bus, SamusPaletteRomData.CrystalFlash.BeamPalettePointers,
            SamusPaletteRomData.CrystalFlash.BeamPaletteCount * sizeof(ushort),
            "Crystal Flash restored beam palettes");
        TouchRange(bus, SamusPaletteRomData.CrystalFlash.BodyRecords,
            SamusPaletteRomData.CrystalFlash.BodyRecordCount *
                SamusPaletteRomData.CrystalFlash.BodyRecordByteCount,
            "Crystal Flash body palette records");
        TouchRange(bus, SamusPaletteRomData.CrystalFlash.BubblePointers,
            SamusPaletteRomData.CrystalFlash.BubblePaletteCount * sizeof(ushort),
            "Crystal Flash bubble palette pointers");

        TouchRange(bus, SamusPaletteRomData.PowerBomb.PreExplosionColors,
            SamusPaletteRomData.PowerBomb.PreExplosionColorCount *
                SamusPaletteRomData.PowerBomb.BytesPerColor,
            "Power Bomb pre-explosion fixed colors");
        TouchRange(bus, SamusPaletteRomData.PowerBomb.ExplosionColors,
            SamusPaletteRomData.PowerBomb.ExplosionColorCount *
                SamusPaletteRomData.PowerBomb.BytesPerColor,
            "Power Bomb explosion fixed colors");

        Console.WriteLine(
            "  Samus palette ROM data: suit, visor, boost, shine, Crystal Flash, death, Hyper Beam, and Power Bomb ranges are valid.");
    }
}
