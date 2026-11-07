using SuperMetroid.AssetExtraction.Bosses.Crocomire;
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
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Crocomire.Artwork;

/// <summary>Checks Crocomire skeleton tile uploads against native source bytes and full VRAM results.</summary>
internal static class CrocomireSkeletonArtworkVerification
{
    /// <summary>Exercises six installed uploads with cartridge reads forbidden.</summary>
    internal static void VerifyCrocomireSkeletonArtwork(
        ISnesAddressSpace rom, InstalledEnemyArtwork stock)
    {
        CrocomireSkeletonArtwork artwork = stock.Bosses.CrocomireSkeleton ??
            throw new InvalidDataException("Installed enemy art omitted Crocomire skeleton tiles.");
        int frames = CrocomireSkeletonCharacterDefinitions.ChunkCount;
        AssertEqual(6, frames, "Crocomire has six native skeleton uploads");
        IReadOnlyList<EncounterArtworkUpload> chunks = EncounterLookUploads.Uploads(ComposedEncounter.Crocomire,
            EncounterLookUploads.IndexOf(ComposedEncounter.Crocomire, CrocomireSkeletonLookDefinitions.SkeletonLook.Name), new EnemyTileTransfers(stock));
        AssertEqual(frames, chunks.Count, "the skeleton look copies every native skeleton chunk");
        for (int index = 0; index < frames; index++)
        {
            // The import's source of each chunk is the cartridge's own selection.
            int chunkSource = CrocomireSourceDefinitions.SkeletonChunk(index);
            ushort destination = ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords + index * 2);
            ushort source = ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords + index * 2);
            AssertEqual(destination, chunks[index].FirstByte / 2,
                $"Crocomire skeleton destination {index} matches the cartridge");
            AssertEqual(0xad0000 | source, chunkSource,
                $"Crocomire skeleton source {index} matches the cartridge");
            AssertEqual(CrocomireSkeletonCharacterDefinitions.ChunkByteCount,
                artwork.Chunk(index).Length,
                $"Crocomire skeleton upload {index} has the native length");
            for (int offset = 0; offset < artwork.Chunk(index).Length; offset++)
                AssertEqual(rom.ReadCartridgeByte(chunkSource + offset),
                    artwork.Chunk(index).Span[offset],
                    $"Crocomire skeleton chunk {index} byte {offset} matches cartridge art");
        }
        AssertEqual((ushort)0xffff, ReadNativeCrocomireSkeletonWord(rom,
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords +
                frames * 2),
            "Crocomire skeleton seventh destination is the native terminator");
        AssertTrue(!CrocomireSkeletonArtwork.IsChunk(frames),
            "compiled Crocomire skeleton terminator ends uploads");
        int invalidIndex = frames + 1;
        AssertThrows<InvalidDataException>(
            () => CrocomireSkeletonArtwork.IsChunk(invalidIndex),
            "out-of-range Crocomire skeleton upload fails loudly");

        (byte[] installedCharacters, CrocomireDeathState installedState,
            CartridgeReadGuard denied) = RunInstalled(stock);
        var nativeCharacters = new byte[SpriteArtworkPlacements.TableByteCount];
        for (int index = 0; index < frames; index++)
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
        AssertEqual(CrocomireSkeletonLookDefinitions.BaringDuration,
            installedState.SkeletonBaringElapsed,
            "the update after the skeleton is bared bares nothing more");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Crocomire skeleton upload reads no ROM bytes");

        Console.WriteLine("Crocomire skeleton art: six native transfers, OBJ character parity, " +
            "guarded installed upload pass.");

        static RunInstalledResult RunInstalled(InstalledEnemyArtwork art)
        {
            // The skeleton look shows on the last update of the baring, and an update after it bares nothing more (#1538).
            var system = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses };
            var vram = new SnesVram();
            var guard = NewCrocomireSkeletonNoReadBus();
            var state = new CrocomireDeathState();
            SetRuntimeFields(system, guard, vram);
            for (int index = 0; index < CrocomireSkeletonLookDefinitions.BaringDuration; index++)
            {
                AssertTrue(!system.Room.Session.Looks.Shows(CrocomireSkeletonLookDefinitions.SkeletonLook), $"the skeleton is not yet bared before baring update {index}");
                BareSkeleton(system, state);
            }
            AssertTrue(system.Room.Session.Looks.Shows(CrocomireSkeletonLookDefinitions.SkeletonLook), "the skeleton look shows from the baring's last update");
            BareSkeleton(system, state);
            // The skeleton's chunks as its sheets picture them: every copy of the skeleton look in order (#1406, #1538).
            var characters = new byte[SpriteArtworkPlacements.TableByteCount];
            foreach (EncounterArtworkUpload upload in EncounterLookUploads.Steps(ComposedEncounter.Crocomire, new EnemyTileTransfers(art)).Looks
                         .SelectMany(look => look))
                upload.ApplyTo(characters);
            return new RunInstalledResult(characters, state, guard);
        }

        static void SetRuntimeFields(RoomEnemySystem system,
            ISnesAddressSpace bus, SnesVram vram)
        {
            system.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        }

        static void BareSkeleton(RoomEnemySystem system, CrocomireDeathState state)
        {
            system.CrocomireBehavior.Skeleton.BareCrocomireSkeleton(state);
        }
    }

    /// <summary>Blocks all cartridge reads and writes during installed skeleton uploads.</summary>
    private static CartridgeReadGuard NewCrocomireSkeletonNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Crocomire skeleton read cartridge byte ${address:X6}.")
            .RejectWrites(address => $"Installed Crocomire skeleton wrote cartridge byte ${address:X6}.");

    /// <summary>Reads a native destination or source-table word for the independent upload oracle.</summary>
    private static ushort ReadNativeCrocomireSkeletonWord(ISnesAddressSpace rom, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
}
