using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Verification.Rooms.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks that Crocomire's melt publishes through bound characters each frame, not VRAM writes (#1355), and that his
/// melting body draws from pictures of each state the melt shows (#1392).
/// </summary>
internal static class CrocomireMeltPublicationVerification
{
    /// <summary>
    /// A constructed Crocomire melting in his room erases one column per frame on the cartridge's cadence, and each
    /// accepted NMI publishes the masked characters that frame bound, while VRAM itself never receives them. Every word of
    /// his melting body resolves to a picture painting what its characters show that frame.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks the melting records' native VRAM words")]
    internal static void VerifyCrocomireMeltPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
        CrocomireBehavior crocomire = runtime.Enemies.CrocomireBehavior;
        CrocomireEnemyState state = crocomire.Access.Crocomire ?? throw new InvalidOperationException("Crocomire's room loads Crocomire.");
        CrocomireDeathState death = crocomire.Access.CrocomireDeath ?? throw new InvalidOperationException("Crocomire's room loads his death state.");
        // The death sequence's order: the melting tilemap loads, then the pass's graphics, then the dissolve begins.
        crocomire.Melting.LoadFirstCrocomireMeltingTilemap(state);
        death.MeltingTableOffset = (ushort)CrocomireMeltingHeader.First;
        crocomire.Melting.InitializeCrocomireMeltingGraphics(state);
        crocomire.Melting.BeginCrocomireMelting(state);

        TileWordSheet sheet = (runtime.Room.Scope?.BodyPictures ?? throw new InvalidOperationException("Crocomire's room has no tile words.")).Sheet;
        int frames = 0, painted = 0;
        for (int frame = 0; frame < 12; frame++)
        {
            byte[] heightsBefore = death.MeltingColumnHeights.ToArray();
            int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
            if (!CrocomireMeltingTransferDefinitions.TryUpload(recordOffset, out CrocomireMeltingUpload upload))
                AssertTrue(CrocomireMeltingTransferDefinitions.TryUpload(death.MeltingTableOffset, out upload), "a melt pass restarts at its first record");
            crocomire.Melting.RunCrocomireMelting(state, null);
            AssertTrue(!heightsBefore.AsSpan().SequenceEqual(death.MeltingColumnHeights), $"melt frame {frame} erases a column");

            runtime.FrameBoundary.BeginAcceptedFrame(0);
            ReadOnlySpan<byte> published = runtime.ComposedVram();
            int source = upload.SourceWord - CrocomireMeltingTransferDefinitions.ScratchStart;
            ReadOnlySpan<byte> melted = death.MeltingGraphics.Slice(source, upload.ByteCount);
            AssertTrue(published.Slice(upload.DestinationWord * 2, upload.ByteCount).SequenceEqual(melted),
                $"melt frame {frame} publishes its masked characters");
            AssertTrue(!runtime.Vram.Bytes.Slice(upload.DestinationWord * 2, upload.ByteCount).SequenceEqual(melted),
                $"melt frame {frame} writes no characters into VRAM");
            ImmutableArray<ushort> words = [.. death.Bg2WorkingTilemap];
            ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver(sheet).Resolve(words,
                RoomCharacterSource.Of(RoomBackgroundCharacters.Tileset, runtime.BgArtwork, runtime.EnemySession.ObjCharacters));
            for (int tile = 0; tile < words.Length; tile++)
            {
                BgTilePicture picture = pictures[tile] ?? throw new InvalidOperationException($"melt frame {frame}: tile {tile} has no picture.");
                var word = new SnesBgTilemapWord(words[tile]);
                for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    RoomBlockPicturesVerification.AssertPaintsTile($"melt frame {frame} tile {tile} pixel ({x}, {y})", published, word, x, y,
                        picture.Picture, new Position16<ImagePixels>((ushort)(picture.Corner.X + x), (ushort)(picture.Corner.Y + y)));
                }
                painted++;
            }
            frames++;
        }
        Console.WriteLine($"  Crocomire melt: {frames} frames each erase a column and publish its masked characters; {painted} body words draw from pictures of what they show.");
    }

    /// <summary>Runs the Crocomire melt publication check.</summary>
    [VerificationCommand("--crocomire-melt")]
    internal static int CrocomireMeltCommand()
    {
        Suite(nameof(VerifyCrocomireMeltPublication), () => VerifyCrocomireMeltPublication());
        return 0;
    }
}
