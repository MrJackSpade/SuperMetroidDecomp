using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed gunship takeoff PNGs against five native DMA
/// transfers and the resulting VRAM image.</summary>
internal static class EnemyTileArtworkGunshipLiftoffVerification
{
    /// <summary>Compares transfer sources, destinations, and bytes, then checks
    /// a selected pixel override reaches pending and legacy queued uploads.</summary>
    internal static void VerifyInstalledGunshipLiftoffArtwork(
        SuperMetroidAddressSpace bus, string directory, InstalledEnemyArtwork stock)
    {
        GunshipLiftoffArtworkCatalog installed = stock.Enemies.GunshipLiftoff ??
            throw new InvalidDataException("Installed enemy art omitted gunship takeoff frames.");
        GunshipLiftoffTransferDefinition[] transfers =
            GunshipLiftoffTransferDefinitions.Frames;
        AssertEqual(5, transfers.Length, "gunship has five native takeoff transfers");
        for (int index = 0; index < transfers.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer = transfers[index];
            ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                EnemyRomTablePointersGunship.LiftoffGraphicsSourceWords + index * 2);
            ushort destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                EnemyRomTablePointersGunship.LiftoffVramDestinationWords + index * 2);
            AssertEqual(0x940000 | source, EnemySourceDefinitions.GunshipLiftoffPageSource(index),
                $"gunship frame {index}'s import source matches the cartridge's takeoff list");
            AssertEqual(destination, ObjCharacterNativeWords.Of(EncounterCharacterPlacementDefinitions.GunshipFirstCharacter
                    + index * GunshipLiftoffCharacterDefinitions.ByteCount / SpriteCharacterTableLayout.CharacterByteCount),
                $"gunship frame {index} compiled place among the OBJ characters matches the cartridge");
            ReadOnlySpan<byte> pixels = installed.Resolve(transfer.Asset).Span;
            AssertEqual(GunshipLiftoffCharacterDefinitions.ByteCount, pixels.Length,
                $"gunship frame {index} transfer length");
            for (int offset = 0; offset < pixels.Length; offset++)
                AssertEqual(CpuBusRegionRead.Read(bus, EnemySourceDefinitions.GunshipLiftoffPageSource(index) + offset), pixels[offset],
                    $"gunship frame {index} byte {offset} matches cartridge art");
        }

        var runtime = CreateRetailRuntimeFixture();
        runtime.EnemySession.TileArtwork = stock.Enemies;
        runtime.BossArtwork.Artwork = stock.Bosses;
        SpriteArtworkPlacements stockPages = ShowAllTakeoffPages(stock, RuntimeArtworkContent.Of(runtime));
        var nativeQueue = new VramWriteQueue();
        for (int index = 0; index < transfers.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer = transfers[index];
            AssertTrue(stockPages.Uploads[index].Source.Resolve(RuntimeArtworkContent.Of(runtime)).Span.SequenceEqual(installed.Resolve(transfer.Asset).Span),
                $"takeoff frame {index} shows its installed page");
            nativeQueue.Enqueue(GunshipLiftoffCharacterDefinitions.ByteCount, EnemySourceDefinitions.GunshipLiftoffPageSource(index),
                VramDmaDestination.FromNative(ObjCharacterNativeWords.Of(stockPages.Uploads[index].FirstByte / 32)));
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, bus);
        byte[] installedPage = new ObjCharacterTablePage().Page(stockPages, RuntimeArtworkContent.Of(runtime)).Characters.ToArray();
        int pagesByte = stockPages.Uploads[0].FirstByte, pagesBytes = transfers.Length * GunshipLiftoffCharacterDefinitions.ByteCount;
        AssertTrue(installedPage.AsSpan(pagesByte, pagesBytes).SequenceEqual(
                nativeVram.Bytes.Slice(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort) + pagesByte, pagesBytes)),
            "all five installed takeoff pages match the native OBJ characters");

        string fileName = EnemyTileArtworkFormat.GunshipLiftoffFileName(0);
        string stockPath = Path.Combine(directory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(directory, "gunship-liftoff-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, fileName);
        using (var stream = new MemoryStream(original, writable: false))
        {
            IndexedPngImage image = IndexedPng.Read(stream, new(256, 8));
            image.Pixels[0] ^= 1;
            using var output = File.Create(overridePath);
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Colours);
        }
        InstalledEnemyArtwork edited = EnemyArtworkEdits.WithEnemyParts(stock, parts => parts with { GunshipLiftoff = EnemyTileArtworkFiles.Open(directory, overrides).GunshipLiftoff() });
        runtime.EnemySession.TileArtwork = edited.Enemies;
        runtime.BossArtwork.Artwork = edited.Bosses;
        byte[] editedPage = new ObjCharacterTablePage().Page(ShowAllTakeoffPages(stock, RuntimeArtworkContent.Of(runtime)), RuntimeArtworkContent.Of(runtime)).Characters.ToArray();
        AssertEqual((byte)(installedPage[pagesByte] ^ 0x80), editedPage[pagesByte],
            "rebound PNG changes the takeoff page shown");
        AssertTrue(editedPage.AsSpan(..pagesByte).SequenceEqual(installedPage.AsSpan(..pagesByte)) &&
            editedPage.AsSpan((pagesByte + 1)..).SequenceEqual(installedPage.AsSpan((pagesByte + 1)..)),
            "gunship PNG edit changes only its selected planar pixel");
        AssertTrue(EnemyArtworkEdits.WithEnemyParts(stock, parts => parts with { GunshipLiftoff = EnemyTileArtworkFiles.Open(directory, overrides).GunshipLiftoff() }).Enemies.GunshipLiftoff is not null,
            "gunship PNG override survives reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(directory, null).GunshipLiftoff(),
            "corrupt stock gunship frame fails loudly where it is read");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(directory, overrides).GunshipLiftoff(),
            "malformed gunship override fails loudly");
        Console.WriteLine(
            "Gunship takeoff art: five compiled pages, installed PNG parity, " +
            "live/rebound page edit and strict resources pass.");

        static SpriteArtworkPlacements ShowAllTakeoffPages(InstalledEnemyArtwork art, IInstalledArtworkContent resolver)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())) { TileArtwork = art.Enemies, BossArtwork = art.Bosses };
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            var part = new GunshipPartState();
            top.SetSpeciesState(part, part.ClearWithRecord);
            for (int index = 0; index < GunshipLiftoffTransferDefinitions.Frames.Length; index++)
                enemies.GunshipBehavior.ShowGunshipTakeoffPage(top);
            AssertEqual<int?>(GunshipLiftoffTransferDefinitions.Frames.Length - 1,
                enemies.Room.Session.ArtworkProgress.StepOf(EncounterArtworkSequence.GunshipLiftoff), "five native takeoff calls reach the fifth page");
            return EncounterArtworkPlacements.Of(enemies.Room.Session.ArtworkProgress, resolver);
        }
    }
}
