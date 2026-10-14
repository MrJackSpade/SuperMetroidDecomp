using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks Botwoon palette-phase health thresholds and the runtime's ROM-free progression.</summary>
internal static class BotwoonHealthPaletteDefinitionsVerification
{
    /// <summary>Compares all signed health decisions with native thresholds and drives the production palette update.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Botwoon health palette definitions")]
    internal static void VerifyBotwoonHealthPaletteDefinitions(SuperMetroidAddressSpace rom)
    {

        for (ushort phase = 0;
             phase < BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
             phase += 2)
        {
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase));
            for (int health = 0; health <= ushort.MaxValue; health++)
            {
                bool expected = unchecked((short)((ushort)health - threshold)) < 0;
                AssertEqual(expected,
                    BotwoonHealthPaletteDefinitions.ShouldAdvance(phase, (ushort)health),
                    $"Botwoon phase ${phase:X2} health {health} threshold decision");
            }
        }

        foreach (ushort invalid in new ushort[] { 1, 3, 15, 16, 18, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => BotwoonHealthPaletteDefinitions.ShouldAdvance(invalid, 0),
                $"Botwoon rejects restored palette phase ${invalid:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewBotwoonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(BotwoonColorExtractor.Extract(rom), writable: false);
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), BotwoonColors = BotwoonColorCatalog.Load(colors) }),
        };
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = new SnesCgram();
        Action<RoomEnemySlot, BotwoonEnemyState> update = enemies.BotwoonBehavior.UpdateBotwoonHealthPalette;
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        var state = new BotwoonEnemyState(head)
        {
            // Sprite palette seven starts at CGRAM color 240 / byte offset $01E0.
            PaletteDestinationByteOffset = 0x01e0,
        };

        for (ushort phase = 0;
             phase < BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
             phase += 2)
        {
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase));
            state.PalettePhaseByteOffset = phase;
            head.Health = threshold;
            update(head, state);
            AssertEqual(phase, state.PalettePhaseByteOffset,
                $"production Botwoon phase ${phase:X2} holds at threshold");

            state.PalettePhaseByteOffset = phase;
            head.Health = unchecked((ushort)(threshold - 1));
            update(head, state);
            AssertEqual(unchecked((ushort)(phase + 2)), state.PalettePhaseByteOffset,
                $"production Botwoon phase ${phase:X2} advances below threshold");
        }

        state.PalettePhaseByteOffset =
            BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
        update(head, state);
        AssertEqual(BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset,
            state.PalettePhaseByteOffset,
            "production Botwoon completed palette phase remains terminal");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Botwoon palette progression performs no threshold reads");

        foreach (ushort invalid in new ushort[] { 1, 18 })
        {
            state.PalettePhaseByteOffset = invalid;
            AssertThrows<InvalidDataException>(
                () => update(head, state),
                $"production Botwoon rejects restored palette phase ${invalid:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "malformed Botwoon palette phase performs no threshold reads");

        Console.WriteLine(
            "Botwoon health palette definitions: eight native thresholds and all " +
            "524,288 signed health/phase decisions match; real hold, one-band advance, " +
            "terminal, and malformed paths run with the threshold table forbidden.");
    }

    /// <summary>Forbids runtime access to the 16-byte native Botwoon threshold table.</summary>
    private static CartridgeReadGuard NewBotwoonHealthThresholdReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(BotwoonHealthPaletteDefinitions.NativeThresholdAddress, 16,
            address => $"Botwoon palette progression attempted threshold read ${address:X6}.");
}
