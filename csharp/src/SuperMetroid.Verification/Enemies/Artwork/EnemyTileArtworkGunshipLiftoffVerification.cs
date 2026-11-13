using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed gunship takeoff PNGs against five native DMA
/// transfers and the resulting VRAM image.</summary>
internal static class EnemyTileArtworkGunshipLiftoffVerification
{
    /// <summary>Compares transfer sources, destinations, and bytes with the installed frames.</summary>
    internal static void VerifyInstalledGunshipLiftoffArtwork(
        CartridgeImportAddressSpace bus, ImportEnemyArtwork stock)
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
        runtime.BossArtwork.KraidEyes = stock.Bosses.KraidColors?.Eyes();
        SpriteArtworkPlacements stockPages = ShowAllTakeoffPages(stock, RuntimeArtworkContent.Of(runtime, stock.Enemies));
        var nativeQueue = new VramWriteQueue();
        for (int index = 0; index < transfers.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer = transfers[index];
            AssertTrue(stockPages.Uploads[index].Source.Resolve(RuntimeArtworkContent.Of(runtime, stock.Enemies)).Span.SequenceEqual(installed.Resolve(transfer.Asset).Span),
                $"takeoff frame {index} shows its installed page");
            nativeQueue.Enqueue(GunshipLiftoffCharacterDefinitions.ByteCount, EnemySourceDefinitions.GunshipLiftoffPageSource(index),
                VramDmaDestination.FromNative(ObjCharacterNativeWords.Of(stockPages.Uploads[index].FirstByte / 32)));
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, bus);
        byte[] installedPage = new ObjCharacterTablePage().Page(stockPages, RuntimeArtworkContent.Of(runtime, stock.Enemies)).Characters.ToArray();
        int pagesByte = stockPages.Uploads[0].FirstByte, pagesBytes = transfers.Length * GunshipLiftoffCharacterDefinitions.ByteCount;
        AssertTrue(installedPage.AsSpan(pagesByte, pagesBytes).SequenceEqual(
                nativeVram.Bytes.Slice(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort) + pagesByte, pagesBytes)),
            "all five installed takeoff pages match the native OBJ characters");

        Console.WriteLine(
            "Gunship takeoff art: five compiled pages, installed PNG parity, " +
            "live page uploads pass.");

        static SpriteArtworkPlacements ShowAllTakeoffPages(ImportEnemyArtwork art, IInstalledArtworkContent resolver)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(art.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom()) { BossArtwork = art.Bosses };
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            var part = new GunshipPartState();
            top.SetSpeciesState(part);
            for (int index = 0; index < GunshipTakeoffDefinitions.EngineReadyingDuration; index++)
                enemies.GunshipBehavior.ReadyGunshipTakeoffEngines(top);
            AssertTrue(enemies.Room.Session.Looks.Shows(GunshipTakeoffDefinitions.TakeoffLook), "readying the engines shows the takeoff look");
            return EncounterArtworkPlacements.Of(enemies.Room.Session.Looks, resolver);
        }
    }
}
