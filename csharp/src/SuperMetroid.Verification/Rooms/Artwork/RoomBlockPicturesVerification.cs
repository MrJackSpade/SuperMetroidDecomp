using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks rooms draw their blocks from the installed pre-coloured block sheets (#1392): each block the displayed planes
/// show is a picture that paints exactly what its tiles paint over the characters the room's bindings show, as those
/// bindings change under animated tiles and collectibles.
/// </summary>
internal static class RoomBlockPicturesVerification
{
    /// <summary>Displayed frames each room is checked over: enough for every animated tile to change frame.</summary>
    private const int Frames = 40;

    /// <summary>Pixels along a character's side.</summary>
    private const int CharacterPixels = 8;

    /// <summary>
    /// In the Landing Site, whose blocks show its load alone, in the Wasteland, whose spikes animate, in the Morph Ball
    /// room, whose item binds its graphics and writes its blocks, in the East Sand Hall, whose sand animates, and in the
    /// Wrecked Ship main shaft with its treadmills running, every displayed block's picture paints what its tiles paint
    /// over the live characters.
    /// </summary>
    internal static void VerifyRoomBlockPictures()
    {
        Check(RoomHeaderDefinitions.LandingSite, expectUploads: false);
        Check(RoomHeaderDefinitions.Wasteland, expectUploads: true);
        Check(RoomHeaderDefinitions.MorphBall, expectUploads: true);
        Check(RoomHeaderDefinitions.EastSandHall, expectUploads: true);
        // The treadmills run once Phantoon, the Wrecked Ship's boss, is defeated.
        Check(RoomHeaderDefinitions.WreckedShipMainShaft, expectUploads: true,
            RoomStateSelectionDefinitions.GetStates(RoomHeaderDefinitions.WreckedShipMainShaft)
                .Single(state => state.Fx?.Records.Any(record => record.AnimatedTileBitset != 0) == true),
            runtime => runtime.System.SetBossBits((int)AreaId.WreckedShip, BossBits.AreaBoss));
    }

    /// <summary>
    /// Loads <paramref name="room"/>, in <paramref name="state"/> when given and after <paramref name="progress"/> sets the
    /// session's progress, and checks its displayed blocks over <see cref="Frames"/> frames.
    /// </summary>
    private static void Check(RoomHeaderDefinition room, bool expectUploads, CartridgeRoomState? state = null,
        Action<SuperMetroidRuntime>? progress = null)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        progress?.Invoke(runtime);
        runtime.LoadCartridgeRoomForDebug(room, state: state);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        int checkedPictures = 0, uploaded = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            runtime.StepFrame(0);
            (int pictures, int over) = AssertPicturesPaintLive(runtime, $"{room.Name} frame {frame}");
            checkedPictures += pictures;
            uploaded += over;
        }
        AssertTrue(checkedPictures > 0, $"{room.Name} shows block pictures");
        AssertEqual(expectUploads, uploaded > 0, $"{room.Name} shows blocks over artwork bound after its load");
        Console.WriteLine($"Room block pictures: {room.Name} showed {checkedPictures} pictures, {uploaded} over artwork bound after its load.");
    }

    /// <summary>
    /// Asserts each picture the displayed BG1 resolves paints, pixel for pixel, the palette index, priority and standard
    /// colour its tiles paint over the characters the bindings compose into VRAM; returns the pictures checked and those
    /// over artwork bound after the room's load.
    /// </summary>
    private static (int Pictures, int Uploaded) AssertPicturesPaintLive(SuperMetroidRuntime runtime, string where)
    {
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException($"{where}: no room backgrounds are displayed.");
        RoomBlockPictureTable table = rooms.Bg1.Pictures ?? throw new InvalidOperationException($"{where}: BG1 shows no block pictures.");
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        ReadOnlySpan<byte> definitions = rooms.Bg1.Plane.Definitions.AsSpan();
        int pictures = 0, uploaded = 0;
        for (int definition = 0; definition < table.Definitions; definition++)
        {
            if (table.Picture(definition) is not { } picture)
                continue;
            pictures++;
            SpriteImage sheet = picture.Sheet;
            bool overUpload = false;
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
            {
                int at = definition * RoomLevelPlaneDefinitionBytes + tile * sizeof(ushort);
                var word = new SnesBgTilemapWord(SnesLittleEndian.Word(definitions[at], definitions[at + 1]));
                BgArtworkShown shown = runtime.BgArtwork.Showing(word.CharacterIndex * RoomBlockPictureNames.CharacterWords, RoomBlockPictureNames.CharacterWords);
                overUpload |= shown.Artwork != RoomBgArtwork.CommonRoomElements && !shown.Artwork.Name.StartsWith("tileset characters ", StringComparison.Ordinal);
                for (int y = 0; y < CharacterPixels; y++)
                {
                    int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + row * 2;
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                        int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                            | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
                        int pixel = (picture.Corner.Y + tile / 2 * CharacterPixels + y) * sheet.Size.X + picture.Corner.X + tile % 2 * CharacterPixels + x;
                        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
                        AssertTrue(sheet.PaletteIndices[pixel] == expected && (colour == 0 || (sheet.Priorities[pixel] != 0) == word.HasPriority)
                            && (colour == 0 || sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgba32()),
                            $"{where}: definition {definition}'s picture paints index {sheet.PaletteIndices[pixel]} priority {sheet.Priorities[pixel]} " +
                            $"at tile {tile} pixel ({x}, {y}), where its tile ${word.Raw:X4} paints index {expected} over {shown.Artwork.Name}+{shown.WordOffset:X4}");
                    }
                }
            }
            if (overUpload)
                uploaded++;
        }
        return (pictures, uploaded);
    }

    /// <summary>Bytes per block definition.</summary>
    private const int RoomLevelPlaneDefinitionBytes = SuperMetroid.Core.Rooms.Backgrounds.RoomLevelPlane.DefinitionBytes;

    /// <summary>Runs the room block picture check.</summary>
    [VerificationCommand("--room-block-pictures")]
    internal static int RoomBlockPicturesCommand()
    {
        Suite(nameof(VerifyRoomBlockPictures), VerifyRoomBlockPictures);
        return 0;
    }
}
