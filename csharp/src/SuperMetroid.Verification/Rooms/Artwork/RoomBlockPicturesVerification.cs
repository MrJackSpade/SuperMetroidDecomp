using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Abilities;
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
    /// In the Parlor, whose rock background shows its room's load, and in the Tourian statue room, whose statues' frames
    /// rebind the characters its background reads, BG2 draws the library background's image, and every pixel of its two
    /// pages paints what BG2's tilemap tiles paint over the live characters, frame after frame.
    /// </summary>
    internal static void VerifyRoomBackgroundImages()
    {
        CheckBackground(RoomHeaderDefinitions.Parlor, expectRebound: false);
        CheckBackground(RoomHeaderDefinitions.Statues, expectRebound: true);
    }

    /// <summary>Loads <paramref name="room"/> and checks its background image over <see cref="Frames"/> frames.</summary>
    private static void CheckBackground(RoomHeaderDefinition room, bool expectRebound)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        int rebound = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            runtime.StepFrame(0);
            rebound += AssertBackgroundPaintsTiles(runtime, $"{room.Name} frame {frame}");
        }
        AssertEqual(expectRebound, rebound > 0, $"{room.Name}'s background shows cells over artwork bound after its load");
        Console.WriteLine($"Room background images: {room.Name} painted its two pages for {Frames} frames, {rebound} cell pictures over rebound artwork.");
    }

    /// <summary>
    /// Asserts BG2's library background paints, pixel for pixel over both pages, the palette index, priority and standard
    /// colour BG2's tilemap tiles paint over the characters the bindings compose into VRAM; returns its rebound cells.
    /// </summary>
    private static int AssertBackgroundPaintsTiles(SuperMetroidRuntime runtime, string where)
    {
        RoomBackgroundImagePlane plane = runtime.Display.Latched.Rooms?.Bg2Image
            ?? throw new InvalidOperationException($"{where}: BG2 shows no library background image.");
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        // A 32x64 map lays the two pages out one above the other, as the image does.
        IBgSource shown = plane.At(new Extent16<TilemapCells>(RoomBackgroundImage.PageTiles, RoomBackgroundImage.Pages * RoomBackgroundImage.PageTiles));
        for (int y = 0; y < RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels; y++)
        for (int x = 0; x < RoomBackgroundImage.PagePixels; x++)
        {
            int at = GameplayVramLayout.Bg2TilemapWord * 2 + (y / CharacterPixels * RoomBackgroundImage.PageTiles + x / CharacterPixels) * 2;
            var word = new SnesBgTilemapWord((ushort)(vram[at] | vram[at + 1] << 8));
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + pixelRow * 2;
            int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            AssertTrue(shown.TrySample(new Position16<BgPlanePixels>((ushort)x, (ushort)y), out BgSample sample),
                $"{where}: the background shows ({x}, {y})");
            SpriteSheetFrame picture = sample.Picture ?? throw new InvalidOperationException($"{where}: the background shows a tile at ({x}, {y}).");
            SpriteImage sheet = picture.Sheet;
            int pixel = (picture.Corner.Y + sample.PicturePixel.Y) * sheet.Size.X + picture.Corner.X + sample.PicturePixel.X;
            AssertTrue(sheet.PaletteIndices[pixel] == expected && (colour == 0 || ((sheet.Priorities[pixel] != 0) == word.HasPriority
                    && sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgba32())),
                $"{where}: the background paints index {sheet.PaletteIndices[pixel]} at ({x}, {y}) where its tile ${word.Raw:X4} paints {expected}");
        }
        int rebound = 0;
        for (int cell = 0; cell < RoomBackgroundImage.Words / RoomBlockPictureNames.Tiles; cell++)
        {
            if (plane.Cell(cell) is not null)
                rebound++;
        }
        return rebound;
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

    /// <summary>
    /// Asserts the X-ray reveal the frame displays paints, pixel for pixel, the palette index, priority and standard colour
    /// the reveal buffer's tile words paint over the characters the bindings compose into VRAM (#1392).
    /// </summary>
    internal static void AssertRevealPaintsWords(SuperMetroidRuntime runtime, ReadOnlySpan<ushort> words, string where)
    {
        XrayRevealPlane reveal = runtime.Display.Latched.XrayReveal ?? throw new InvalidOperationException($"{where}: no X-ray reveal is displayed.");
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        for (int column = 0; column < XrayTilemapLayout.MetatileColumns; column++)
        for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
        for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
        {
            var word = new SnesBgTilemapWord(words[XrayRevealTilemap.Destination(column, row) + y / CharacterPixels * 32 + x / CharacterPixels]);
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + pixelRow * 2;
            int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            AssertTrue(reveal.TrySample(new Position16<BgPlanePixels>((ushort)(column * 16 + x), (ushort)(row * 16 + y)), out BgSample sample),
                $"{where}: the reveal shows block ({column}, {row})");
            SpriteSheetFrame picture = sample.Picture ?? throw new InvalidOperationException($"{where}: the reveal shows a tile, not a picture.");
            SpriteImage sheet = picture.Sheet;
            int pixel = (picture.Corner.Y + sample.PicturePixel.Y) * sheet.Size.X + picture.Corner.X + sample.PicturePixel.X;
            AssertTrue(sheet.PaletteIndices[pixel] == expected && (colour == 0 || (sheet.Priorities[pixel] != 0) == word.HasPriority)
                && (colour == 0 || sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgba32()),
                $"{where}: reveal block ({column}, {row}) pixel ({x}, {y}) paints index {sheet.PaletteIndices[pixel]} where its tile ${word.Raw:X4} paints {expected}");
        }
    }

    /// <summary>Bytes per block definition.</summary>
    private const int RoomLevelPlaneDefinitionBytes = SuperMetroid.Core.Rooms.Backgrounds.RoomLevelPlane.DefinitionBytes;

    /// <summary>Runs the room block picture checks.</summary>
    [VerificationCommand("--room-block-pictures")]
    internal static int RoomBlockPicturesCommand()
    {
        Suite(nameof(VerifyRoomBlockPictures), VerifyRoomBlockPictures);
        Suite(nameof(VerifyRoomBackgroundImages), VerifyRoomBackgroundImages);
        return 0;
    }
}
