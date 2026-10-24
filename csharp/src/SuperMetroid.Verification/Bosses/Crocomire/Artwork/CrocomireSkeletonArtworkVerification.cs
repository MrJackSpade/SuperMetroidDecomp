using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Crocomire.Artwork;

/// <summary>Checks Crocomire skeleton tile uploads against native source bytes and full VRAM results.</summary>
internal static class CrocomireSkeletonArtworkVerification
{
    /// <summary>Exercises six installed uploads and a single-pixel PNG override with cartridge reads forbidden.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire skeleton artwork")]
    internal static void VerifyCrocomireSkeletonArtwork(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        CrocomireSkeletonArtwork artwork = stock.Bosses.CrocomireSkeleton ??
            throw new InvalidDataException("Installed enemy art omitted Crocomire skeleton tiles.");
        CrocomireSkeletonTransferSequence frames =
            CrocomireSkeletonTransferDefinitions.Frames;
        AssertEqual(6, frames.Length, "Crocomire has six native skeleton uploads");
        for (int index = 0; index < frames.Length; index++)
        {
            CrocomireSkeletonTransferDefinition frame = frames[index];
            ushort destination = ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords + index * 2);
            ushort source = ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords + index * 2);
            AssertEqual(destination, frame.FirstCharacter * 16,
                $"Crocomire skeleton destination {index} matches the cartridge");
            AssertEqual(0xad0000 | source, frame.SourceAddress,
                $"Crocomire skeleton source {index} matches the cartridge");
            AssertEqual(CrocomireSkeletonTransferDefinitions.ChunkByteCount,
                artwork.Chunk(index).Length,
                $"Crocomire skeleton upload {index} has the native length");
            for (int offset = 0; offset < artwork.Chunk(index).Length; offset++)
                AssertEqual(rom.ReadCartridgeByte(frame.SourceAddress + offset),
                    artwork.Chunk(index).Span[offset],
                    $"Crocomire skeleton chunk {index} byte {offset} matches cartridge art");
        }
        AssertEqual((ushort)0xffff, ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords +
                frames.Length * 2),
            "Crocomire skeleton seventh destination is the native terminator");
        AssertTrue(!CrocomireSkeletonTransferDefinitions.TryGet(frames.Length, out _),
            "compiled Crocomire skeleton terminator ends uploads");
        int invalidIndex = frames.Length + 1;
        AssertThrows<InvalidDataException>(
            () => CrocomireSkeletonTransferDefinitions.TryGet(invalidIndex, out _),
            "out-of-range Crocomire skeleton upload fails loudly");

        (byte[] installedCharacters, CrocomireDeathState installedState,
            CartridgeReadGuard denied) = RunInstalled(stock);
        var nativeCharacters = new byte[ObjCharacterUploads.TableByteCount];
        for (int index = 0; index < frames.Length; index++)
        {
            int destination = ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords + index * 2);
            int source = 0xad0000 | ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords + index * 2);
            byte[] page = Enumerable.Range(0, 0x200).Select(offset => rom.ReadCartridgeByte(source + offset)).ToArray();
            page.CopyTo(nativeCharacters, destination * 2);
        }
        AssertEqual(nativeCharacters.Length, installedCharacters.Length, "the OBJ page spans the native OBJ characters");
        AssertEqual(-1, Enumerable.Range(0, nativeCharacters.Length).FirstOrDefault(index => installedCharacters[index] != nativeCharacters[index], -1),
            "all six installed Crocomire skeleton uploads match the native OBJ characters (first differing byte)");
        AssertEqual(frames.Length,
            installedState.SkeletonChunksShown,
            "the update after the last chunk shows no further chunk");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Crocomire skeleton upload reads no ROM bytes");

        string fileName = CrocomireSkeletonTransferDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "crocomire-skeleton-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, fileName);
        using (var input = new MemoryStream(original, writable: false))
        {
            IndexedPngImage image = IndexedPng.Read(input, new(256, 24));
            image.Pixels[0] ^= 1;
            using var output = File.Create(overridePath);
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Colours);
        }
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrides);
        (byte[] editedCharacters, _, CartridgeReadGuard editedGuard) =
            RunInstalled(edited);
        int firstDestination = frames[0].FirstCharacter * 32;
        AssertEqual((byte)(installedCharacters[firstDestination] ^ 0x80),
            editedCharacters[firstDestination],
            "Crocomire skeleton PNG edit changes the actual uploaded planar pixel");
        AssertTrue(installedCharacters[..firstDestination].SequenceEqual(
                editedCharacters[..firstDestination]) &&
            installedCharacters[(firstDestination + 1)..].SequenceEqual(
                editedCharacters[(firstDestination + 1)..]),
            "Crocomire skeleton edit leaves every other OBJ character byte unchanged");
        AssertEqual(0, editedGuard.ForbiddenReadAttempts,
            "edited Crocomire skeleton remains ROM-free at upload time");
        AssertEqual(editedCharacters[firstDestination], RunInstalled(
                EnemyTileArtworkFiles.Load(stockDirectory, overrides)).Characters[firstDestination],
            "Crocomire skeleton PNG override survives catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Crocomire skeleton fails its manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "malformed Crocomire skeleton override fails loudly");
        Console.WriteLine("Crocomire skeleton art: six native transfers, OBJ character parity, " +
            "guarded installed upload, isolated visible PNG edit, reload and strict files pass.");

        static RunInstalledResult RunInstalled(InstalledEnemyArtwork art)
        {
            var system = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses, AreaArtwork = art.Areas };
            var vram = new SnesVram();
            var guard = NewCrocomireSkeletonNoReadBus();
            var state = new CrocomireDeathState();
            SetRuntimeFields(system, guard, vram);
            for (int index = 0;
                 index <= CrocomireSkeletonTransferDefinitions.Frames.Length; index++)
                ShowNextChunk(system, state);
            // The skeleton's chunks as its sheets picture them: each installed chunk where the cartridge uploads it (#1406).
            var shown = new ObjCharacterUploads();
            foreach (RuntimeCharacterUpload upload in CrocomireSkeletonTransferDefinitions.Uploads)
                shown.Record(upload);
            return new RunInstalledResult(new ObjCharacterTablePage().Page(shown, new EnemyTileTransfers(art)).Characters[..ObjCharacterUploads.TableByteCount].ToArray(),
                state, guard);
        }

        static void SetRuntimeFields(RoomEnemySystem system,
            ISnesAddressSpace bus, SnesVram vram)
        {
            system.Presentation.BindVram(BgArtworkFixtures.For(vram));
        }

        static void ShowNextChunk(RoomEnemySystem system, CrocomireDeathState state)
        {
            system.CrocomireBehavior.Skeleton.ShowNextCrocomireSkeletonChunk(state);
        }
    }

    /// <summary>Blocks all cartridge reads and writes during installed skeleton uploads.</summary>
    private static CartridgeReadGuard NewCrocomireSkeletonNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Crocomire skeleton read cartridge byte ${address:X6}.")
            .RejectWrites(address => $"Installed Crocomire skeleton wrote cartridge byte ${address:X6}.");

    /// <summary>Reads a native destination or source-table word for the independent upload oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire's native skeleton destination and source tables")]
    private static ushort ReadNativeCrocomireSkeletonWord(ISnesAddressSpace rom, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
}
