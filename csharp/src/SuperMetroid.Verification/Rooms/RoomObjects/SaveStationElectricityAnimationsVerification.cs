using static SuperMetroid.TestSupport.MovieReplay.SaveStationElectricityNativeListAddresses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SaveStation;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#1296: the save-station electricity's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SaveStationElectricityAnimationsVerification
{
    /// <summary>
    /// Every program matches its native list, each sweep frame read as the operand that draws the band at its place, and each
    /// of those operands' spritemaps is the band's own moved by that place (#2557), so the one band picture drawn there shows
    /// exactly what the cartridge shows.
    /// </summary>
    internal static void VerifySaveStationElectricityAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        SpriteVisualPart[] band = ProgramFrameParts(rom, EnemyProjectileProgramFrameSources.Operand(EnemyProjectileFrameId.SaveStationElectricityBand));
        int placed = 0;
        foreach ((ushort operand, EnemyProjectilePlacedFrame shown) in EnemyProjectileProgramFrameDefinitions.PlacedAt)
        {
            if (shown.Frame != EnemyProjectileFrameId.SaveStationElectricityBand)
                continue;
            SpriteVisualPart[] expected = [.. band.Select(part => part with
            {
                OffsetX = part.OffsetX + shown.Placement.X,
                OffsetY = part.OffsetY + shown.Placement.Y,
            })];
            AssertTrue(ProgramFrameParts(rom, operand).SequenceEqual(expected),
                $"$86:{operand:X4} draws the band moved by ({shown.Placement.X}, {shown.Placement.Y})");
            placed++;
        }
        AssertEqual(SaveStationElectricityDefinitions.SweepPlaces - 1, placed, "every sweep place but the band's own is a moved band");
        Console.WriteLine($"  save-station electricity programs: {Entries.Length} programs ({steps} steps) match bank $86; {placed} sweep places draw the band moved.");
    }

    /// <summary>The parts of the bank-$8D spritemap the bank-$86 operand <paramref name="operand"/> names.</summary>
    private static SpriteVisualPart[] ProgramFrameParts(CartridgeImportAddressSpace rom, ushort operand)
    {
        int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(EnemySourceDefinitions.ProjectileInstructionBank, operand));
        return EnemySpritemapFiles.ExtractParts(rom, CartridgeFormatDefinitions.EnemyProjectileSpritemapBank, pointer);
    }
}
