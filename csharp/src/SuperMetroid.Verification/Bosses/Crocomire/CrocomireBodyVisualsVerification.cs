using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedBg2ReferenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Retail comparisons for Crocomire's mixed OAM/BG2 body frames and installed editable artwork.</summary>
internal static class CrocomireBodyVisualsVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Derives the 50 selected body roots from bank-$A4 instructions and checks frame ordering, component counts, and the 42-frame BG2 domain.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire body frame geometry")]
    internal static void VerifyCrocomireBodyFrameGeometry(SuperMetroidAddressSpace rom)
    {
        var original = new SortedSet<ushort>(CrocomireAnimationsVerification.TypedBodySpritemaps(CrocomireBodyFrameSet.Body));
        AssertEqual(50, original.Count, "native body frame selection count");
        AssertEqual(50, CrocomireBodyVisualDefinitions.Frames.Count, "calculated body frame count");
        ushort[] expected = original.ToArray();
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], CrocomireBodyVisualDefinitions.FramePointer(index), "body native frame address order");
            AssertEqual(expected[index], CrocomireBodyVisualDefinitions.Frames[index], "body indexed sequence");
            ushort count = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa40000 | expected[index]);
            AssertEqual((ushort)(index >= 42 ? 1 : index is < 12 or >= 18 and < 24 ? 6 : 7), count,
                "native component count supporting each frame stride");
        }
        int enumerated = 0;
        foreach (ushort frame in CrocomireBodyVisualDefinitions.Frames)
            AssertEqual(expected[enumerated++], frame, "body frame enumeration order");
        AssertEqual(50, enumerated, "body frame enumeration count");
        AssertTrue(expected.SequenceEqual(CrocomireBodyVisualDefinitions.Frames.ToArray()), "body materialization matches original selection");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(pointer < 0xca7e && original.Contains((ushort)pointer),
                CrocomireBodyVisualDefinitions.HasBg2((ushort)pointer), "body exact mixed-frame domain");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBodyVisualDefinitions.FramePointer(-1), "negative body frame");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBodyVisualDefinitions.FramePointer(50), "body frame past end");
    }
    /// <summary>Checks generated BG2 keys against retail selectors and confirms the extractor and loader preserve all 42 mixed-frame streams.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire BG2 generated catalog")]
    internal static void VerifyCrocomireBg2GeneratedCatalog(SuperMetroidAddressSpace rom)
    {
        var pointers = new SortedSet<ushort>(CrocomireAnimationsVerification.TypedBodySpritemaps(CrocomireBodyFrameSet.Body)
            .Where(CrocomireBodyVisualDefinitions.HasBg2));
        EnemyBg2FrameDefinition[] expected = pointers.Select(pointer =>
            new EnemyBg2FrameDefinition(pointer, $"crocomire_body_bg2_{pointer:X4}")).ToArray();
        AssertEqual(42, expected.Length, "native Crocomire BG2 catalog size");
        AssertEqual(expected.Length, CrocomireBg2FrameDefinitions.Frames.Length, "generated BG2 definition count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], CrocomireBg2FrameDefinitions.Frame(i), "native BG2 pointer and published artwork name");
        AssertTrue(expected.SequenceEqual(CrocomireBg2FrameDefinitions.Frames.ToArray()), "BG2 sequence materialization");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBg2FrameDefinitions.Frame(-1), "negative BG2 definition");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBg2FrameDefinitions.Frame(42), "BG2 definition past end");
        AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireBg2FrameDefinitions.Frames[-1], "negative generated sequence index");
        AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireBg2FrameDefinitions.Frames[42], "generated sequence index past end");

        // Confirm the changed generator boundary through the real extractor and
        // loader, using original-ROM-derived definitions through the span path.
        byte[] expectedJson = EnemyBg2FrameFiles.Extract(rom, 0xa4, new EnemyBg2FrameDefinitionSequence(expected.Length, index => expected[index]), 1,
            EnemyBg2FrameLayout.MaximumComponents, "Crocomire", allowMixedOam: true);
        byte[] generatedJson = CrocomireBg2FrameFiles.Extract(rom);
        AssertTrue(expectedJson.SequenceEqual(generatedJson), "generated catalog preserves exact extracted JSON");
        using var input = new MemoryStream(generatedJson, writable: false);
        var loaded = CrocomireBg2FrameCatalog.Load(input);
        foreach (var frame in expected)
            AssertTrue(loaded.TryGet(frame.Pointer, out var writes) && writes.Length > 0,
                "generated catalog loads every native BG2 stream by its published key");
        AssertTrue(!loaded.TryGet(0xca7e, out _), "pure-OAM frame excluded from BG2 catalog");
    }
    /// <summary>Compares installed OAM and VRAM output for every body frame with visual ROM reads denied, then checks a BG2 edit obeys the new-frame gate.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Crocomire body visuals")]
    internal static void VerifyInstalledCrocomireBodyVisuals(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.ExtendedFrames is not null && stock.CrocomireBg2Frames is not null,
            "installed Crocomire body has both OAM and BG2 presentations");
        Suite(nameof(VerifyCrocomireBodyFrameGeometry), () => VerifyCrocomireBodyFrameGeometry(rom));
        AssertEqual(CrocomireBodyVisualDefinitions.MixedBg2FrameCount,
            CrocomireBg2FrameDefinitions.Frames.Length,
            "forty-two Crocomire body roots have a BG2 half");

        foreach (ushort pointer in CrocomireBodyVisualDefinitions.Frames)
        {
            AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGetDisplay(
                    CrocomireBodyVisualDefinitions.Bank, pointer, out _),
                $"Crocomire body OAM frame $A4:{pointer:X4} is installed");
            var guard = NewExtendedVisualReadGuard(rom, out Action<byte, ushort> guardBlockFrame);
            guardBlockFrame(CrocomireBodyVisualDefinitions.Bank, pointer);
            foreach (bool newFrame in new[] { false, true })
            {
                (OamBuffer nativeOam, byte[] nativeVram) = DrawNative(pointer,
                    newFrame);
                (OamBuffer installedOam, byte[] installedVram) = Draw(stock, guard,
                    pointer, newFrame);
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                           nativeOam.NextByteOffset == installedOam.NextByteOffset,
                    $"Crocomire body $A4:{pointer:X4} OAM matches native; new frame={newFrame}");
                AssertTrue(nativeVram.SequenceEqual(installedVram),
                    $"Crocomire body $A4:{pointer:X4} BG2 writes match native; new frame={newFrame}");
            }
        }

        foreach (EnemyBg2FrameDefinition frame in CrocomireBg2FrameDefinitions.Frames)
        {
            AssertTrue((stock.CrocomireBg2Frames ?? throw new InvalidOperationException("Verification requires stock.CrocomireBg2Frames.")).TryGet(frame.Pointer,
                    out ReadOnlyMemory<EnemyBg2TilemapWrite> installed),
                $"Crocomire BG2 half $A4:{frame.Pointer:X4} is installed");
            var native = new List<(ushort DestinationWord, ushort[] Tiles)>();
            int componentCount = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa40000 | frame.Pointer));
            for (int component = 0; component < componentCount; component++)
            {
                int record = 0xa40000 | unchecked((ushort)(
                    frame.Pointer + 2 + component * 8));
                ushort stream = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((record + 4));
                if (CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa40000 | stream)) != EnemyBg2FrameLayout.StreamMarker)
                    continue;
                ushort cursor = unchecked((ushort)(stream + 2));
                bool terminated = false;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream;
                     command++)
                {
                    ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa40000 | cursor));
                    if (destination == 0xffff)
                    {
                        terminated = true;
                        break;
                    }
                    ushort count = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa40000 |
                        unchecked((ushort)(cursor + 2))));
                    var tiles = new ushort[count];
                    for (int tile = 0; tile < tiles.Length; tile++)
                        tiles[tile] = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((0xa40000 |
                            unchecked((ushort)(cursor + 4 + tile * 2))));
                    native.Add((unchecked((ushort)((destination -
                        EnemyBg2FrameLayout.WorkingRamBase) >> 1)), tiles));
                    cursor = unchecked((ushort)(cursor + 4 + count * 2));
                }
                AssertTrue(terminated,
                    $"Crocomire native BG2 stream $A4:{stream:X4} terminates");
            }
            AssertEqual(native.Count, installed.Length,
                $"Crocomire BG2 frame $A4:{frame.Pointer:X4} retains every command");
            for (int index = 0; index < native.Count; index++)
            {
                AssertEqual(native[index].DestinationWord,
                    installed.Span[index].DestinationWord,
                    $"Crocomire BG2 frame $A4:{frame.Pointer:X4} write {index} destination");
                AssertTrue(native[index].Tiles.SequenceEqual(
                        installed.Span[index].Tiles.Span),
                    $"Crocomire BG2 frame $A4:{frame.Pointer:X4} write {index} tiles");
            }
        }

        string overrideDirectory = Path.Combine(stockDirectory, "crocomire-body-override");
        Directory.CreateDirectory(overrideDirectory);
        string source = Path.Combine(stockDirectory, CrocomireBg2FrameDefinitions.FileName);
        var authored = RequiredJson.Deserialize<EnemyBg2FrameDocument>(
            File.ReadAllBytes(source),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        EnemyBg2FrameDefinition editedFrame = CrocomireBg2FrameDefinitions.Frames[0];
        EnemyBg2WriteDocument lastWrite = authored.Frames[editedFrame.Name][^1];
        lastWrite.Tiles[0] ^= 1;
        string overridePath = Path.Combine(overrideDirectory,
            CrocomireBg2FrameDefinitions.FileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(authored,
            CamelCaseJson));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var editedGuard = NewExtendedVisualReadGuard(rom, out Action<byte, ushort> editedGuardBlockFrame);
        editedGuardBlockFrame(CrocomireBodyVisualDefinitions.Bank, editedFrame.Pointer);
        (OamBuffer stockOam, byte[] stockVram) = Draw(stock, editedGuard,
            editedFrame.Pointer, true);
        (OamBuffer editedOam, byte[] editedVram) = Draw(edited, editedGuard,
            editedFrame.Pointer, true);
        AssertTrue(stockOam.ImageEquals(editedOam),
            "Crocomire BG2 edit does not move its OAM components");
        AssertTrue(!stockVram.SequenceEqual(editedVram),
            "Crocomire BG2 edit changes the live VRAM result");
        AssertTrue(Draw(edited, editedGuard, editedFrame.Pointer, false).Vram
                .SequenceEqual(Draw(stock, editedGuard, editedFrame.Pointer, false).Vram),
            "Crocomire BG2 edit still obeys the native new-frame gate");
        Console.WriteLine(
            "  Crocomire body: 236 selectors, 50 mixed/pure OAM roots and 42 BG2 roots match native OAM and VRAM with visual ROM reads denied; BG2 edits obey the new-frame gate.");

        // Build cartridge-based OAM and BG2 VRAM references for one selected body frame.
        (OamBuffer Oam, byte[] Vram) DrawNative(ushort pointer, bool newFrame) =>
            (DrawReferenceExtendedFrame(rom, 0xa4, pointer, new(0x0080, 0x0080)),
                ReadReferenceExtendedBg2Vram(rom, 0xa4, pointer, newFrame));
        // Draw one compiled Crocomire frame through production enemy presentation into fresh OAM and VRAM buffers.
        static (OamBuffer Oam, byte[] Vram) Draw(EnemyTileArtworkCatalog art,
            ISnesAddressSpace bus, ushort pointer, bool newFrame)
        {
            var enemies = new RoomEnemySystem { TileArtwork = art };
            var vram = new SnesVram();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            enemies.Presentation._vram = vram;
            enemies.Drawing.Enqueue(0, 0);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Crocomire, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
                { Bank = CrocomireBodyVisualDefinitions.Bank });
            slot.ExtraProperties = slot.ExtraProperties.With(
                EnemyExtraProperties.UsesExtendedSpritemap);
            if (newFrame)
                slot.ExtraProperties = slot.ExtraProperties.With(
                    EnemyExtraProperties.NewInstructionFrame);
            slot.SpritemapPointer = pointer;
            slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
            var oam = new OamBuffer();
            enemies.Drawing.DrawLayers(oam, new(0, 0), 0, 0);
            return (oam, vram.Bytes.ToArray());
        }
    }
}
