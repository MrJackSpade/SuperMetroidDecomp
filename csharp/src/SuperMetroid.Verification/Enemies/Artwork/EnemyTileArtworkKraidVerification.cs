using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationOracleVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.World;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Kraid BG2, head, and room-background artwork against native transfers.</summary>
internal static class EnemyTileArtworkKraidVerification
{
    /// <summary>Matches Kraid's working BG2 map and both shown body pages.</summary>
    internal static void VerifyInstalledKraidBackground(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        KraidBackgroundArtwork art = stock.Bosses.KraidBackground
            ?? throw new InvalidDataException("Installed enemy artwork lacks Kraid maps.");
        AssertTrue(art.Upper.Bytes.Span.SequenceEqual(RomDataReader.Decompress(
                rom, KraidSourceDefinitions.UpperBodyTilemap,
                KraidWorkingMapDefinitions.SourceTilemapBytes)),
            "installed Kraid upper BG2 tile references preserve source words");
        AssertTrue(art.Lower.Bytes.Span.SequenceEqual(RomDataReader.Decompress(
                rom, KraidSourceDefinitions.LowerBodyTilemap,
                KraidWorkingMapDefinitions.SourceTilemapBytes)),
            "installed Kraid lower BG2 tile references preserve source words");

        ushort[] native = ReadImportedKraidWorkingMap(rom);
        AssertTrue(BuildKraidWorkingMap(stock).SequenceEqual(native),
            "installed Kraid BG2 construction preserves all native working words");
        KraidEnemyState initialized = InitializeKraidBackground();
        AssertTrue(initialized.ShowsBodyLayer, "Kraid's body initialization shows his body layer");
        AssertTrue(initialized.BodyAppearance == KraidBodyAppearance.BehindSprites && !initialized.BodyShown,
            "Kraid's body initialization readies his body behind the sprites, shown once he rises");
        ushort[] nativeShown = KraidBodyFixtures.Tilemap(native.AsSpan(0, KraidWorkingMapDefinitions.PageCells), [],
            native.AsSpan(KraidWorkingMapDefinitions.LowerPageFirstCell, KraidWorkingMapDefinitions.PageCells));
        AssertTrue(ShowKraidVisiblePages(stock).SequenceEqual(nativeShown),
            "installed Kraid BG2 sources preserve both shown body pages");

        Suite(nameof(VerifyInstalledKraidHeadFrames), () => VerifyInstalledKraidHeadFrames(rom, stock));
    }

    /// <summary>Checks each authored Kraid head frame against the head the cartridge shows.</summary>
    private static void VerifyInstalledKraidHeadFrames(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        ushort[] pointers = KraidHeadFrameDefinitions.PresentationKeys.Order().ToArray();
        AssertEqual(4, pointers.Length, "four distinct Kraid head art frames");
        foreach (ushort pointer in pointers)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                KraidSourceDefinitions.CodeBank | pointer,
                KraidWorkingMapDefinitions.HeadImageCells * sizeof(ushort));
            ReadOnlySpan<ushort> installed = (stock.Bosses.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).HeadWords(pointer);
            for (int word = 0; word < installed.Length; word++)
                AssertEqual(LittleEndianBytes.Word(native[word * 2], native[word * 2 + 1]),
                    installed[word], $"Kraid head ${pointer:X4} word {word}");

            (KraidEnemyState _, ushort[] nativeShown) = ReadImportedKraidHeadFrame(rom, pointer);
            (KraidEnemyState _, ushort[] installedShown) = RunKraidHeadFrame(stock, pointer);
            // His head shows over his body's top rows (#1515).
            AssertTrue(installedShown.AsSpan(0, KraidWorkingMapDefinitions.HeadImageCells)
                    .SequenceEqual(nativeShown.AsSpan(0, KraidWorkingMapDefinitions.HeadImageCells)),
                $"Kraid head ${pointer:X4} preserves the shown head");
        }

        Suite(nameof(VerifyInstalledKraidRoomBackground), () => VerifyInstalledKraidRoomBackground(rom, stock));
    }

    /// <summary>Checks the Kraid room-background character upload against the cartridge's VRAM.</summary>
    private static void VerifyInstalledKraidRoomBackground(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom,
            KraidSourceDefinitions.RoomBackgroundTiles,
            KraidSourceDefinitions.RoomBackgroundTileBytes);
        AssertTrue((stock.Bosses.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).RoomBackgroundTiles.Characters.Span.SequenceEqual(native),
            "installed Kraid room-background PNG preserves native planar characters");
        var baseline = new SnesVram();
        baseline.LoadBytes(KraidBackgroundDefinitions.RoomBackgroundTileVramWord * 2, native);
        SnesVram installed = UploadKraidRoomBackground(stock);
        AssertTrue(installed.ImageEquals(baseline),
            "installed Kraid room-background upload preserves full native VRAM");
    }

    /// <summary>The image a capture shows after the room-background characters bind, using a cartridge-free Kraid owner.</summary>
    private static SnesVram UploadKraidRoomBackground(ImportEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        Action upload = enemies.KraidBehavior.Rise.ShowKraidRoomBackdrop;
        upload();
        return BgArtworkFixtures.ComposedVram(vram, new BgArtworkBytes().WithKraidBackground(artwork.Bosses.KraidBackground
            ?? throw new InvalidOperationException("Verification requires installed Kraid background characters.")));
    }

    /// <summary>The BG2 tilemap a selected installed head pose shows over his risen body.</summary>
    private static RunKraidHeadFrameResult RunKraidHeadFrame(ImportEnemyArtwork artwork, ushort pointer)
    {
        KraidHeadFrame frame = KraidHeadTilemapFormat.HeadsByMouthStage.Single(candidate => KraidHeadFrameDefinitions.PresentationKey(candidate) == pointer);
        var state = new KraidEnemyState { BodyAppearance = KraidBodyAppearance.BehindSprites, DisplayedHeadFrame = frame };
        state.ShowBody();
        return new RunKraidHeadFrameResult(state, KraidBodyFixtures.Shown(artwork.Bosses, state));
    }

    /// <summary>Kraid's working map as his installed art composes it, without cartridge reads.</summary>
    internal static ushort[] BuildKraidWorkingMap(ImportEnemyArtwork artwork) => KraidBodyFixtures.WorkingMap(artwork.Bosses);

    /// <summary>Runs Kraid's background initializer against installed art without cartridge reads.</summary>
    private static KraidEnemyState InitializeKraidBackground()
    {
        var state = new KraidEnemyState();
        state.InitializeBodyLayer();
        return state;
    }

    /// <summary>What his body layer shows once Kraid's readied body has risen behind the sprites (#1515).</summary>
    private static ushort[] ShowKraidVisiblePages(ImportEnemyArtwork artwork)
    {
        KraidEnemyState state = InitializeKraidBackground();
        state.ShowBody();
        return KraidBodyFixtures.Shown(artwork.Bosses, state);
    }
}
