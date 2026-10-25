using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks Botwoon palette-phase health thresholds and the runtime's ROM-free progression.</summary>
internal static class BotwoonHealthPaletteDefinitionsVerification
{
    /// <summary>Compares all signed health decisions with native thresholds and drives the production palette update.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Botwoon health palette definitions")]
    internal static void VerifyBotwoonHealthPaletteDefinitions(SuperMetroidAddressSpace rom)
    {

        for (int phase = 0; phase < BotwoonHealthPaletteDefinitions.CompletePhase; phase++)
        {
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase * sizeof(ushort));
            for (int health = 0; health <= ushort.MaxValue; health++)
            {
                bool expected = unchecked((short)((ushort)health - threshold)) < 0;
                AssertEqual(expected,
                    BotwoonHealthPaletteDefinitions.ShouldAdvance(phase, health),
                    $"Botwoon phase ${phase:X2} health {health} threshold decision");
            }
        }

        foreach (int invalid in new[] { -1, 8, 9, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => BotwoonHealthPaletteDefinitions.ShouldAdvance(invalid, 0),
                $"Botwoon rejects restored palette phase {invalid}");
        }

        var guard = NewBotwoonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(BotwoonColorExtractor.Extract(rom), writable: false);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), BossArtwork = new BossArtworkCatalog { BotwoonColors = BotwoonColorFile.Read(colors) },
        };
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
        Action<RoomEnemySlot, BotwoonEnemyState> update = enemies.BotwoonBehavior.UpdateBotwoonHealthPalette;
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        var state = new BotwoonEnemyState()
        {
            // Sprite palette seven starts at CGRAM color 240.
            PaletteFirstColour = BotwoonHealthPaletteDefinitions.FirstColour,
        };

        for (int phase = 0; phase < BotwoonHealthPaletteDefinitions.CompletePhase; phase++)
        {
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase * sizeof(ushort));
            state.PalettePhase = phase;
            head.Health = threshold;
            update(head, state);
            AssertEqual(phase, state.PalettePhase,
                $"production Botwoon phase ${phase:X2} holds at threshold");

            state.PalettePhase = phase;
            head.Health = threshold - 1;
            update(head, state);
            AssertEqual(phase + 1, state.PalettePhase,
                $"production Botwoon phase ${phase:X2} advances below threshold");
        }

        state.PalettePhase = BotwoonHealthPaletteDefinitions.CompletePhase;
        update(head, state);
        AssertEqual(BotwoonHealthPaletteDefinitions.CompletePhase,
            state.PalettePhase,
            "production Botwoon completed palette phase remains terminal");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Botwoon palette progression performs no threshold reads");

        foreach (int invalid in new[] { -1, 9 })
        {
            state.PalettePhase = invalid;
            AssertThrows<InvalidDataException>(
                () => update(head, state),
                $"production Botwoon rejects restored palette phase {invalid}");
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
