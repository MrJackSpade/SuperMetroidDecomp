using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>OBJ, HUD, cameras, scrolling, tilemaps, room data, and sky verification.</summary>
static void VerifyObjRendering()
{
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    var oam = new OamBuffer();

    // A single palette-index-1 pixel at the tile's upper-left corner. SNES 4-bpp plane 0
    // uses bit 7 of byte 0 for (0,0); every omitted byte reads as zero in this fixture.
    bus.WriteByte(0x828000, 0x80);
    vram.ExecuteQueuedWrite(bus, 0x828000, sizeInBytes: 32, encodedDestination: 0x0000);

    // One small tile-zero OBJ at (10,20), with caller-selected palette 2. Palette index
    // 128 + 2*16 + 1 is loaded with maximum red in native BGR555.
    bus.WriteBytes(0x818000, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    cgram.SetColor(128 + 2 * 16 + 1, 0x001f);
    oam.BeginFrame();
    oam.AddOnScreenSpritemap(bus, 0x818000, originX: 10, originY: 20, paletteBits: 0x0400);
    oam.FinalizeFrame();

    var pixels = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0);
    AssertEqual(new SuperMetroid.Core.Assets.Rgba32(255, 0, 0), pixels[20 * 256 + 10], "OBJ colored planar pixel");
    AssertEqual(new SuperMetroid.Core.Assets.Rgba32(0, 0, 0, 0), pixels[20 * 256 + 11], "OBJ color zero transparency");

    // OBJ priority controls only the winning sprite pixel's position relative to BGs.
    // It does not override OAM order when two sprites overlap. Model the Ceres failure
    // directly: an earlier Samus-like OBJ2 must exclude a later Ridley-like OBJ3 from the
    // OBJ3 plane, otherwise the compositor paints the hidden boss over Samus at the end of
    // its BG/OBJ ladder. Distinct palettes make the winner observable without private ROM
    // art; both entries deliberately use the same one-pixel tile and screen coordinate.
    cgram.SetColor(128 + 3 * 16 + 1, 0x03e0);
    oam.BeginFrame();
    oam.AddRawSmallSprite(x: 10, y: 20, attributes: 0x2400); // OAM 0: OBJ2, palette 2.
    oam.AddRawSmallSprite(x: 10, y: 20, attributes: 0x3600); // OAM 1: OBJ3, palette 3.
    oam.FinalizeFrame();
    Rgba32[] obj2Plane = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0, priority: 2);
    Rgba32[] obj3Plane = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0, priority: 3);
    ResolvedObjFrame resolvedObjects = SnesObjRenderer.RenderResolved(oam, vram, cgram, obsel: 0);
    AssertEqual(
        new SuperMetroid.Core.Assets.Rgba32(255, 0, 0),
        obj2Plane[20 * 256 + 10],
        "lower-OAM OBJ2 wins cross-priority overlap");
    AssertEqual(
        new SuperMetroid.Core.Assets.Rgba32(0, 0, 0, 0),
        obj3Plane[20 * 256 + 10],
        "higher-OAM OBJ3 excluded by cross-priority overlap");
    AssertEqual(
        new SuperMetroid.Core.Assets.Rgba32(255, 0, 0),
        resolvedObjects.Pixels[20 * 256 + 10],
        "single-pass resolved OBJ color");
    AssertEqual((byte)2, resolvedObjects.Priorities[20 * 256 + 10], "single-pass resolved OBJ priority");
    AssertEqual(
        SnesObjRenderer.TransparentPriority,
        resolvedObjects.Priorities[20 * 256 + 11],
        "single-pass transparent OBJ priority sentinel");

    // The production timer's first tile is $1E0 under OBSEL=$03. This assertion fixes
    // the important reverse-engineered relationship: character data starts at word $7E00.
    AssertEqual(0xfc00, SnesObjRenderer.ResolveTileByteAddress(0x1e0, 0x03), "timer tile $1E0 VRAM byte address");
    AssertEqual(0xff00, SnesObjRenderer.ResolveTileByteAddress(0x1f8, 0x03), "timer tile $1F8 VRAM byte address");

    Console.WriteLine("  OBJ: OBSEL addressing, planar pixels, CGRAM, transparency, and cross-priority OAM ownership agree.");
}

/// <summary>
/// Verifies the HUD ROM-template mutation, authentic WRAM queue source, 2-bpp tilemap
/// interpretation, palette selection, and final pixel without relying on the private ROM.
/// </summary>
static void VerifyHudStateAndBg3Rendering()
{
    var bus = new TestAddressSpace();

    // Seed the three-row ROM template entirely with the canonical blank HUD tile $2C0F.
    // The synthetic digit table uses conspicuous character names $100-$109.
    for (int tile = 0; tile < HudState.MutableTileCount; tile++)
        WriteTestWord(bus, 0x8098cb + tile * 2, 0x2c0f);
    for (int digit = 0; digit < 10; digit++)
    {
        WriteTestWord(bus, 0x809dbf + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
        WriteTestWord(bus, 0x809dd3 + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
    }

    var hud = new HudState();
    hud.Initialize(bus, HudSnapshot.CeresDebug);
    AssertEqual(0x2d09, hud.Tiles[0x8c / 2], "HUD health tens digit from ROM table");
    AssertEqual(0x2d09, hud.Tiles[0x8e / 2], "HUD health ones digit from ROM table");

    var damagedSamus = new SamusState { Health = 94, MaxHealth = 99 };
    hud.UpdateGameplayCounters(bus, damagedSamus);
    AssertEqual(0x2d09, hud.Tiles[0x8c / 2], "live HUD damage tens digit");
    AssertEqual(0x2d04, hud.Tiles[0x8e / 2], "live HUD damage ones digit");

    // HandleHudTilemap owns selection presentation independently of the input handler.
    // Installing missiles after initialization mirrors a live pickup, then selecting them
    // proves both palette directions and the one-frame QueueSfx publication.
    damagedSamus.MaxMissiles = 5;
    damagedSamus.Missiles = 5;
    damagedSamus.SelectedHudItem = 1;
    hud.UpdateGameplayCounters(bus, damagedSamus);
    AssertEqual(0x1000, hud.Tiles[0x14 / 2] & 0x1c00,
        "live HUD selects missile icon with palette four");
    AssertTrue(hud.SelectionSoundRequestedThisFrame,
        "live HUD publishes cartridge selection sound on change");
    hud.UpdateGameplayCounters(bus, damagedSamus);
    AssertTrue(!hud.SelectionSoundRequestedThisFrame,
        "stable HUD selection does not repeat the selection sound");
    damagedSamus.SelectedHudItem = 0;
    hud.UpdateGameplayCounters(bus, damagedSamus);
    AssertEqual(0x1400, hud.Tiles[0x14 / 2] & 0x1c00,
        "live HUD restores deselected missile icon to palette five");

    // Area zero points to a synthetic two-screen-wide Crateria map. Give every map tile a
    // character equal to its SNES-layout index and mark every coordinate as existing; the
    // expected HUD words then prove room origin + Samus screen coordinates, 5x3 centering,
    // exploration palette, and the map-station/unexplored palette independently.
    bus.WriteByte(0x82964a, 0x00);
    bus.WriteByte(0x82964b, 0x80);
    bus.WriteByte(0x82964c, 0xb5);
    WriteTestWord(bus, 0x829717, 0x9000);
    for (int index = 0; index < 0x100; index++)
        bus.WriteByte(0x829000 + index, 0xff);
    for (int index = 0; index < 0x800; index++)
        WriteTestWord(bus, 0xb58000 + index * 2, (ushort)(index & 0x03ff));

    var mapSystem = new Bank80SystemState();
    mapSystem.SetAreaMapAcquired(0);
    hud.UpdateMinimap(
        bus,
        mapSystem,
        areaIndex: AreaId.Crateria,
        roomMapX: 0x17,
        roomMapY: 0,
        roomWidthInBlocks: 9 * 16,
        roomHeightInBlocks: 5 * 16,
        samusX: 0x0440,
        samusY: 0x04bb,
        nmiFrameCounter: 8);
    AssertEqual(27, hud.MinimapCenterX, "minimap Landing Site absolute X");
    AssertEqual(5, hud.MinimapCenterY, "minimap Landing Site absolute Y");
    AssertEqual(0x2c99, hud.Tiles[26], "minimap top-left unvisited map-station tile");
    AssertEqual(0x28bb, hud.Tiles[60], "minimap explored center tile");

    var noMapSystem = new Bank80SystemState();
    hud.UpdateMinimap(
        bus,
        noMapSystem,
        areaIndex: AreaId.Crateria,
        roomMapX: 0x17,
        roomMapY: 0,
        roomWidthInBlocks: 9 * 16,
        roomHeightInBlocks: 5 * 16,
        samusX: 0x0440,
        samusY: 0x04bb,
        nmiFrameCounter: 0);
    AssertEqual(0x3cbb, hud.Tiles[60], "minimap blinking center palette");
    AssertEqual(0x2c1f, hud.Tiles[26], "minimap hides unvisited tile without map station");

    var queue = new VramWriteQueue();
    var vram = new SnesVram();
    hud.QueueUpload(bus, queue);
    AssertEqual(HudState.MutableByteCount, (int)queue.Entries[0].SizeInBytes, "HUD queue transfer size");
    AssertEqual(HudState.WorkRamAddress, queue.Entries[0].SourceAddress, "HUD queue WRAM source");
    AssertEqual(HudState.VramDestination, queue.Entries[0].EncodedVramDestination, "HUD queue VRAM destination");
    queue.DrainTo(vram, bus);
    AssertEqual(0x0f, vram.ReadByte(HudState.VramDestination * 2), "HUD first tilemap low byte reaches VRAM");
    AssertEqual(0x2c, vram.ReadByte(HudState.VramDestination * 2 + 1), "HUD first tilemap high byte reaches VRAM");

    // Isolate the BG3 renderer at a harmless tilemap base. Entry tile 2 / palette 1 points
    // at a tile whose upper-left plane-0 bit is set; CGRAM 5 is maximum green.
    bus.WriteBytes(0x818000, [0x02, 0x04]);
    vram.ExecuteQueuedWrite(bus, 0x818000, 2, encodedDestination: 0x0100);
    bus.WriteByte(0x828000, 0x80);
    vram.ExecuteQueuedWrite(bus, 0x828000, 16, encodedDestination: 0x0010);
    var cgram = new SnesCgram();
    cgram.SetColor(5, 0x03e0);
    var pixels = SnesBgTilemapRenderer.Render2Bpp(vram, cgram, tilemapBaseWord: 0x0100, characterBaseWord: 0, rowCount: 1);
    AssertEqual(new SuperMetroid.Core.Assets.Rgba32(0, 255, 0), pixels[0], "BG3 tile/palette pixel");

    Console.WriteLine("  HUD: inventory, live minimap coordinates/blink, WRAM upload, and BG3 pixels agree.");
}

static void SeedMotherBrainWalkProgram(
    TestAddressSpace bus,
    ushort listAddress,
    ushort duration,
    bool forward)
{
    // All four speed variants share command topology; only each visible frame duration
    // differs. These words are transcribed from `$A9:9730-$9972`, with harmless synthetic
    // spritemap operands because movement verification never draws the extended map.
    ushort[] commands = forward
        ? [0x95fc, 0x960c, 0x961c, 0x9622, 0x9638, 0x9648, 0x9658, 0x9668]
        : [0x96f0, 0x96e0, 0x96d0, 0x96ba, 0x96aa, 0x96a4, 0x9694, 0x967e];
    var words = new List<ushort> { 0x9708, duration, 0x1000 };
    for (int command = 0; command < commands.Length; command++)
    {
        words.Add(commands[command]);
        if (command == commands.Length - 1)
            words.Add(0x9700);
        words.Add(duration);
        words.Add(unchecked((ushort)(0x1001 + command)));
    }
    words.Add(0x812f);
    for (int index = 0; index < words.Count; index++)
        WriteTestWord(bus, 0xa90000 | unchecked((ushort)(listAddress + index * 2)), words[index]);
}

static void SeedMotherBrainCrouchFastProgram(TestAddressSpace bus)
{
    ushort[] words =
    [
        0x9718, 0x0008, 0x1200,
        0x95de, 0x0002, 0x1201,
        0x95e8, 0x0002, 0x1202,
        0x95f2, 0x9710, 0x0008, 0x1203,
        0x812f,
    ];
    WriteTestWords(bus, 0xa99a26, words);
}

static void SeedBabyCeilingToSamusRoute(TestAddressSpace bus)
{
    // Exact `$A9:CA24-$CA65` words. Records are eight bytes even though the AI reads a
    // fifth word at +8: for records zero through six that read aliases the following X
    // target, while final record `$CA5C` aliases `$CA64`'s negative `$CA66` function.
    ushort[] words =
    [
        0x00a0, 0x0078, 0x0000, 0xf466,
        0x0130, 0x007a, 0x0000, 0xf466,
        0x00c0, 0x0040, 0x0000, 0xf466,
        0x00c0, 0x0070, 0x0000, 0xf466,
        0x00e0, 0x0080, 0x0000, 0xf466,
        0x00cd, 0x0090, 0x0000, 0xf45f,
        0x00cc, 0x00a0, 0x0000, 0xf45f,
        0x00cb, 0x00b0, 0x0000, 0xf45f,
        0xca66,
    ];
    WriteTestWords(bus, 0xa9ca24, words);
}

/// <summary>Checks both edges and relative tile-step movement of the temporary host camera.</summary>
static void VerifyDebugRoomCamera()
{
    var camera = new DebugRoomCamera(roomWidth: 1024, roomHeight: 512, viewportWidth: 256, viewportHeight: 192);
    camera.MoveTo(-50, -20);
    AssertEqual(0, camera.X, "debug camera clamps negative X");
    AssertEqual(0, camera.Y, "debug camera clamps negative Y");

    camera.MoveTo(5000, 5000);
    AssertEqual(768, camera.X, "debug camera clamps right edge");
    AssertEqual(320, camera.Y, "debug camera clamps bottom edge");

    camera.MoveBy(-16, -16);
    AssertEqual(752, camera.X, "debug camera relative block X");
    AssertEqual(304, camera.Y, "debug camera relative block Y");
    Console.WriteLine("  Camera: host viewport movement and room-edge clamps agree.");
}

/// <summary>
/// Exercises the exact 50-byte room loader plus all four directional bank-$80 handlers at
/// internal red boundaries and physical room edges.
/// </summary>
static void VerifyRoomScrollGridAndBoundaryCamera()
{
    var bus = new TestAddressSpace();
    const int source = 0x808000;
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        bus.WriteByte(source + index, 1);

    // The logical 3x2 grid begins fully blue. Give the five copied padding bytes distinct
    // values to prove LoadExplicit does not synthesize zeroes after the sixth logical cell.
    for (int index = 6; index < RoomScrollGrid.StorageByteCount; index++)
        bus.WriteByte(source + index, (byte)(0x80 + index));

    RoomScrollGrid grid = RoomScrollGrid.LoadExplicit(bus, source, widthInScreens: 3, heightInScreens: 2);
    AssertEqual(0x86, grid.Storage[6], "scroll loader retains first nonlogical byte");
    AssertEqual(0xb1, grid.Storage[49], "scroll loader retains fiftieth byte");
    AssertEqual(0x86, bus.ReadByte(RoomScrollGrid.WorkRamAddress + 6), "scroll loader mirrors WRAM padding");

    // Only the width*height logical cells are scroll discriminators. The remaining bytes
    // deliberately preserve the native 50-byte overread above, but an unknown value inside
    // the room itself must fail at the ROM boundary instead of becoming a phantom camera mode.
    var invalidBus = new TestAddressSpace();
    invalidBus.WriteByte(source, 3);
    NotSupportedException invalidScroll = AssertThrows<NotSupportedException>(
        () => RoomScrollGrid.LoadExplicit(
            invalidBus, source, widthInScreens: 1, heightInScreens: 1),
        "unknown logical room scroll state fails loudly");
    AssertTrue(invalidScroll.Message.Contains("$03", StringComparison.Ordinal) &&
        invalidScroll.Message.Contains("logical cell 0", StringComparison.Ordinal),
        "unknown scroll diagnostic identifies value and logical cell");

    var camera = new ScrollBoundaryCamera(grid);

    // Right: the screen to the right is red, so the attempted +16 is rejected and the
    // routine's deliberate extra two-pixel retreat clamps the signed underflow to zero.
    grid.SetLogicalState(1, 0, RoomScrollState.RedBoundary);
    camera.SetPosition(0, 0);
    camera.MoveRight(16);
    AssertEqual(0, camera.XPosition, "$80:A641 red boundary moving right");

    // Left: entering a red current cell from its right edge produces the symmetric +2
    // retreat described by $80:A719-$80:A72B.
    camera.SetPosition(272, 0);
    camera.MoveLeft(16);
    AssertEqual(274, camera.XPosition, "$80:A6BB red boundary moving left");

    // Down: a blue current cell over a red lower cell rejects the move and retreats two
    // pixels above the pre-move position (224 -> proposed 240 -> result 222).
    grid.SetLogicalState(0, 0, RoomScrollState.Blue);
    grid.SetLogicalState(0, 1, RoomScrollState.RedBoundary);
    camera.SetPosition(0, 224);
    camera.MoveDown(16);
    AssertEqual(222, camera.YPosition, "$80:A893 red boundary moving down");

    // Up: starting inside that red lower cell gives the corresponding two-pixel retreat.
    camera.SetPosition(0, 272);
    camera.MoveUp(16);
    AssertEqual(274, camera.YPosition, "$80:A936 red boundary moving up");

    // Physical right edge is (width-1)*$100 regardless of the scroll padding bytes.
    grid.SetLogicalState(1, 0, RoomScrollState.Blue);
    camera.SetPosition(0x01f8, 0);
    camera.MoveRight(16);
    AssertEqual(0x0200, camera.XPosition, "scroll camera physical room maximum");

    // Issue #12's slot-one capture is Construction Zone immediately after returning from
    // First Missile. Its room header starts `[blue, red]`; the incoming door points at
    // `$8F:BE25`, which must replace that pair with `[green, blue]` before camera tracking.
    grid.SetLogicalState(0, 0, RoomScrollState.Blue);
    grid.SetLogicalState(0, 1, RoomScrollState.RedBoundary);
    DoorSetupCodeInterpreter.ApplyScrollWrites(
        DoorCodes.DoorASM_Scroll_0_Green_1_Blue,
        DoorPointers.ConstructionZoneFromFirstMissile,
        grid);
    AssertEqual((byte)RoomScrollState.Green, grid.ReadStorage(0),
        "$8F:BE25 writes Construction Zone screen zero green");
    AssertEqual((byte)RoomScrollState.Blue, grid.ReadStorage(1),
        "$8F:BE25 writes Construction Zone screen one blue");
    grid.SetStorage(6, RoomScrollState.RedBoundary);
    DoorSetupCodeInterpreter.ApplyScrollWrites(
        DoorCodes.DoorCode_Scroll6_Green,
        DoorPointers.ParlorFromClimb,
        grid);
    AssertEqual((byte)RoomScrollState.Green, grid.ReadStorage(6),
        "$8F:B981 writes early-route screen six green");

    // Door $83:AB4C leaves the Ceres Mode-7 shaft through `$8F:E513`. That routine
    // restores ordinary PPU state but performs no writes to the destination scroll array.
    // The scroll interpreter must accept it without corrupting an otherwise valid grid;
    // runtime-owned Mode-7 state is cleared by the paired door dispatcher.
    grid.SetStorage(7, RoomScrollState.Blue);
    DoorSetupCodeInterpreter.ApplyScrollWrites(
        DoorCodes.DoorASM_FromCeresElevatorShaft,
        DoorPointers.FromCeresElevatorShaft,
        grid);
    AssertEqual((byte)RoomScrollState.Blue, grid.ReadStorage(7),
        "$8F:E513 preserves destination scroll storage");

    Console.WriteLine("  Scrolls: 50-byte load and four directional boundary handlers agree.");
}

/// <summary>
/// Checks bank-$90 moved-axis target selection and 16.16 "distance + 1" arithmetic before
/// those results enter the already-verified bank-$80 boundary handlers.
/// </summary>
static void VerifyMovedSamusCameraTracking()
{
    var bus = new TestAddressSpace();
    const int source = 0x818000;
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        bus.WriteByte(source + index, 1);
    RoomScrollGrid grid = RoomScrollGrid.LoadExplicit(bus, source, widthInScreens: 3, heightInScreens: 2);
    var camera = new ScrollBoundaryCamera(grid);

    // Facing right, normal forward movement, distance slot zero targets Samus X-$60.
    // Samus moved four pixels, and $90:96C0 intentionally adds 1.0, so camera advances 5.
    camera.SetPosition(100, 0);
    var previous = new SamusCameraPoint(200, 0, 100, 0);
    var current = new SamusCameraPoint(204, 0, 100, 0);
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(0, MovementType: 0, XAccelerationMode: 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual(108, camera.IdealXPosition, "camera facing-right ideal X");
    AssertEqual(5, camera.CameraXSpeed, "camera X distance includes one pixel");
    AssertEqual(105, camera.XPosition, "camera X follows by calculated speed");

    // The complete fixed-point difference matters: 200.8000 -> 201.4000 is +0.C000;
    // adding 1.0 yields a camera delta of 1.C000.
    camera.SetPosition(100, 0);
    previous = previous with { XPosition = 200, XSubposition = 0x8000 };
    current = current with { XPosition = 201, XSubposition = 0x4000 };
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual(1, camera.CameraXSpeed, "camera fixed X speed integer");
    AssertEqual(0xc000, camera.CameraXSubspeed, "camera fixed X speed fraction");
    AssertEqual(101, camera.XPosition, "camera fixed X position integer");
    AssertEqual(0xc000, camera.XSubposition, "camera fixed X position fraction");

    // Downward Samus movement uses up_scroller. 210-$64 gives ideal 110; a two-pixel move
    // becomes camera speed three and advances layer Y from 100 to 103 without overshoot.
    camera.SetPosition(0, 100);
    previous = new SamusCameraPoint(0, 0, 208, 0);
    current = new SamusCameraPoint(0, 0, 210, 0);
    camera.TrackMovedSamusVertically(
        previous,
        current,
        new VerticalCameraContext(YDirection: 2, UpScroller: 100, DownScroller: 112));
    AssertEqual(110, camera.IdealYPosition, "camera downward ideal Y");
    AssertEqual(3, camera.CameraYSpeed, "camera Y distance includes one pixel");
    AssertEqual(103, camera.YPosition, "camera Y follows by calculated speed");

    // Equal integer coordinates take $80:A528/$80:A731. With identical fixed-point
    // samples, the bank-$90 distance routine still produces speed 1; autoscroll then adds
    // its own two pixels. A red current cell with a blue neighbor therefore drifts +3.
    grid.SetLogicalState(0, 0, RoomScrollState.RedBoundary);
    grid.SetLogicalState(1, 0, RoomScrollState.Blue);
    camera.SetPosition(0x0020, 0);
    var stationary = new SamusCameraPoint(200, 0, 200, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual(1, camera.CameraXSpeed, "stationary camera X speed still includes one");
    AssertEqual(0x0023, camera.XPosition, "$80:A528 red-cell rightward drift");

    // Red on both sides cancels that drift by rounding back to the current screen edge.
    grid.SetLogicalState(1, 0, RoomScrollState.RedBoundary);
    camera.SetPosition(0x0020, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual(0, camera.XPosition, "$80:A528 adjacent-red horizontal rounding");

    // The time-frozen flag short-circuits autoscrolling after bank $90 has calculated the
    // speed. It is intentionally not a blanket prohibition on moved-axis handling.
    grid.SetLogicalState(1, 0, RoomScrollState.Blue);
    camera.SetPosition(0x0020, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0),
        timeIsFrozen: true);
    AssertEqual(0x0020, camera.XPosition, "$80:A528 time-frozen return");

    // A 9x5 room at its padded bottom can select scroll index 50 after centered camera X
    // advances into screen five. Native WRAM continues into ExploredMapTiles at $CD52;
    // a zero byte there makes the blue bottom row clamp to Y=$0400 without an array error.
    var bottomEdgeBus = new TestAddressSpace();
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        bottomEdgeBus.WriteByte(source + index, 1);
    RoomScrollGrid bottomEdgeGrid = RoomScrollGrid.LoadExplicit(
        bottomEdgeBus,
        source,
        widthInScreens: 9,
        heightInScreens: 5);
    var bottomEdgeCamera = new ScrollBoundaryCamera(bottomEdgeGrid);
    bottomEdgeCamera.SetPosition(0x0492, 0x0415);
    bottomEdgeCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: 0, UpScroller: 0x70, DownScroller: 0xa0));
    AssertEqual(0x0400, bottomEdgeCamera.YPosition, "$80:A731 index-$32 adjacent-WRAM bottom clamp");

    // Vertical autoscrolling uses the centered X cell and the same speed+2 drift. This
    // three-row grid leaves the cell below blue so the candidate remains unrounded.
    var verticalBus = new TestAddressSpace();
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        verticalBus.WriteByte(source + index, 1);
    RoomScrollGrid verticalGrid = RoomScrollGrid.LoadExplicit(
        verticalBus,
        source,
        widthInScreens: 3,
        heightInScreens: 3);
    verticalGrid.SetLogicalState(0, 0, RoomScrollState.RedBoundary);
    verticalGrid.SetLogicalState(0, 1, RoomScrollState.Blue);
    var verticalCamera = new ScrollBoundaryCamera(verticalGrid);
    verticalCamera.SetPosition(0, 0x0020);
    verticalCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: 0, UpScroller: 0, DownScroller: 0));
    AssertEqual(1, verticalCamera.CameraYSpeed, "stationary camera Y speed still includes one");
    AssertEqual(0x0023, verticalCamera.YPosition, "$80:A731 red-cell downward drift");

    Console.WriteLine("  Camera: bank $90 tracking and bank $80 stationary autoscroll agree.");
}

/// <summary>
/// Checks the exact parallax multiply, scroll-register wrapping, signed block conversion,
/// and row/column selection at $80:A2F9-$80:A527.
/// </summary>
static void VerifyBackgroundScrollState()
{
    var state = new BackgroundScrollState
    {
        Layer1XPosition = 0x1234,
        Layer1YPosition = 0x0200,
        Layer2ScrollX = 0x80, // 128/256 = one-half parallax.
        Layer2ScrollY = 0,
        Bg1XOffset = 0x0010,
        Bg1YOffset = 0xfff0,
        Bg2XOffset = 3,
        Bg2YOffset = 4,
    };

    state.PrimePreviousBlocks();
    IReadOnlyList<BackgroundUpdateRequest> requests = state.StepScrolling();
    AssertEqual(0x1244, state.Bg1HorizontalScroll, "$80:A3B7 BG1 X plus offset");
    AssertEqual(0x01f0, state.Bg1VerticalScroll, "$80:A3C0 BG1 Y wrapping offset");
    AssertEqual(0x091a, state.Layer2XPosition, "$80:A2F9 half-speed X parallax");
    AssertEqual(0x0200, state.Layer2YPosition, "$80:A33A zero mode copies layer 1");
    AssertEqual(0x091d, state.Bg2HorizontalScroll, "$80:A3CF BG2 X plus offset");
    AssertEqual(0, requests.Count, "primed scrolling emits no unchanged block updates");

    // Crossing one 16-pixel boundary right/down creates requests in native order: level
    // column, background column, level row, background row. Coordinate offsets are the
    // literal +$10 and +$0F selected by the assembly.
    state.Layer1XPosition = 0x1244;
    state.Layer1YPosition = 0x0210;
    requests = state.StepScrolling();
    AssertEqual(4, requests.Count, "four scrolling update requests");
    AssertEqual(
        new BackgroundUpdateRequest(BackgroundLayer.Level, BackgroundUpdateAxis.Column, 0x0134, 0x0021, 0x0135, 0x0020),
        requests[0],
        "rightward level column request");
    AssertEqual(BackgroundLayer.Background, requests[1].Layer, "second request is BG2 column");
    AssertEqual(BackgroundUpdateAxis.Column, requests[1].Axis, "second request axis");
    AssertEqual(
        new BackgroundUpdateRequest(BackgroundLayer.Level, BackgroundUpdateAxis.Row, 0x0124, 0x0030, 0x0125, 0x002f),
        requests[2],
        "downward level row request");
    AssertEqual(BackgroundLayer.Background, requests[3].Layer, "fourth request is BG2 row");
    AssertEqual(BackgroundUpdateAxis.Row, requests[3].Axis, "fourth request axis");

    // Mode one preserves both the layer-2 position and its PPU scroll mirror, and its odd
    // low bit suppresses both BG2 stream directions.
    ushort oldLayer2X = state.Layer2XPosition;
    ushort oldBg2X = state.Bg2HorizontalScroll;
    state.Layer2ScrollX = 1;
    state.Layer2ScrollY = 1;
    state.Layer1XPosition = 0x1300;
    state.Layer1YPosition = 0x0300;
    requests = state.StepScrolling();
    AssertEqual(oldLayer2X, state.Layer2XPosition, "fixed BG2 X position remains unchanged");
    AssertEqual(oldBg2X, state.Bg2HorizontalScroll, "fixed BG2 X register remains unchanged");
    AssertEqual(2, requests.Count, "fixed BG2 modes emit only level updates");

    // World coordinates are sign-extended after division by 16, whereas PPU registers are
    // always logically shifted. $FFF0 therefore means block -1 ($FFFF), not $0FFF.
    state.Layer1XPosition = 0xfff0;
    state.PrimePreviousBlocks();
    AssertEqual(0xffff, state.Layer1XBlock, "$80:A4CD signed layer block");

    ushort frozenBg1X = state.Bg1HorizontalScroll;
    state.Layer1XPosition = 0x4444;
    requests = state.StepScrolling(timeIsFrozen: true);
    AssertEqual(frozenBg1X, state.Bg1HorizontalScroll, "$80:A3AB frozen scroll registers");
    AssertEqual(0, requests.Count, "$80:A3AB frozen update list");

    var destinationParallax = new BackgroundScrollState
    {
        Layer2ScrollX = 0x80,
        Layer2ScrollY = 0x40,
    };
    destinationParallax.PrepareDoorOpeningDestination(0x0400, 0x0200);
    AssertEqual((ushort)0x0200, destinationParallax.Layer2XPosition,
        "door setup calculates destination half-speed BG2 X");
    AssertEqual((ushort)0x0080, destinationParallax.Layer2YPosition,
        "door setup calculates destination quarter-speed BG2 Y");

    // `$80:AE29` first derives four offsets from the retained source BG1 registers, the
    // cleared BG2 registers, and the destination's staged +/-$100 coordinate. The first
    // built-in four-pixel IRQ step must therefore move BG1 by exactly four pixels from the
    // source image while BG2 starts four pixels from its cleared origin.
    var rightDoor = new BackgroundScrollState
    {
        Layer1XPosition = 0xff04, // destination $0000 - $00FC
        Layer2XPosition = 0xff04,
    };
    rightDoor.ConfigureDoorOpeningOffsets(
        retainedBg1Horizontal: 0x0120,
        retainedBg1Vertical: 0x0340,
        stagedLayer1X: 0xff00,
        stagedLayer1Y: 0x0000);
    rightDoor.PrimeHorizontalDoorOpeningBlocks(orientation: 0);
    requests = rightDoor.CalculateScrollsAndUpdates();
    AssertEqual((ushort)0x0124, rightDoor.Bg1HorizontalScroll,
        "$80:AE29 right-door BG1 retains source plus first step");
    AssertEqual((ushort)0x0004, rightDoor.Bg2HorizontalScroll,
        "$80:AE29 right-door BG2 advances from cleared register");
    AssertEqual(2, requests.Count, "$80:AD4A right-door initial BG1/BG2 streams");
    AssertEqual((ushort)0x0000, requests[0].SourceXBlock,
        "$80:AD4A right-door initial level column");

    var leftDoor = new BackgroundScrollState
    {
        Layer1XPosition = 0x00fc, // destination $0000 + $00FC
        Layer2XPosition = 0x00fc,
    };
    leftDoor.ConfigureDoorOpeningOffsets(
        retainedBg1Horizontal: 0x0120,
        retainedBg1Vertical: 0x0340,
        stagedLayer1X: 0x0100,
        stagedLayer1Y: 0x0000);
    leftDoor.PrimeHorizontalDoorOpeningBlocks(orientation: 1);
    requests = leftDoor.CalculateScrollsAndUpdates();
    AssertEqual((ushort)0x011c, leftDoor.Bg1HorizontalScroll,
        "$80:AE29 left-door BG1 retains source minus first step");
    AssertEqual((ushort)0xfffc, leftDoor.Bg2HorizontalScroll,
        "$80:AE29 left-door BG2 retreats from cleared register");
    AssertEqual(2, requests.Count, "$80:AD74 left-door initial BG1/BG2 streams");
    AssertEqual((ushort)0x000f, requests[0].SourceXBlock,
        "$80:AD74 left-door initial level column");

    // `$80:AD1D` runs against the source room before an upward destination replaces its
    // level-data pointer. Its temporary -16-pixel probe writes the current top row for
    // both scrolling layers, then restores every visible coordinate/register word.
    var upwardSource = new BackgroundScrollState
    {
        Layer1YPosition = 0x0200,
        Layer2YPosition = 0x0200,
    };
    upwardSource.PrimePreviousBlocks();
    ushort sourceBg1Vertical = upwardSource.Bg1VerticalScroll;
    requests = upwardSource.FixDoorsMovingUp();
    AssertEqual(2, requests.Count, "$80:AD1D upward source row count");
    AssertEqual(BackgroundUpdateAxis.Row, requests[0].Axis,
        "$80:AD1D emits a BG1 row");
    AssertEqual((ushort)0x0020, requests[0].SourceYBlock,
        "$80:AD1D repairs the source viewport's current top row");
    AssertEqual((ushort)0x0200, upwardSource.Layer1YPosition,
        "$80:AD1D restores source layer-one Y");
    AssertEqual(sourceBg1Vertical, upwardSource.Bg1VerticalScroll,
        "$80:AD1D restores source BG1VOFS");

    // Upward setup carries frame counter one from that repair. It primes previous rows at
    // destination+$100, presents destination+$FB after the built-in first moving call,
    // and intentionally emits no destination row until counter five.
    var upwardDestination = new BackgroundScrollState
    {
        Layer1YPosition = 0x02fb,
        Layer2YPosition = 0x01dc,
    };
    upwardDestination.ConfigureDoorOpeningOffsets(
        retainedBg1Horizontal: 0x0120,
        retainedBg1Vertical: 0x0340,
        stagedLayer1X: 0,
        stagedLayer1Y: 0x0300);
    requests = upwardDestination.PrimeVerticalDoorOpeningBlocks(
        orientation: 3,
        stagedLayer1Y: 0x0300,
        stagedLayer2Y: 0x01e0);
    AssertEqual(0, requests.Count, "$80:ADC8 upward setup defers destination rows");
    AssertEqual((ushort)0x0031, upwardDestination.PreviousLayer1YBlock,
        "$80:ADC8 upward previous-row bias");
    AssertEqual((ushort)0x033b, upwardDestination.Bg1VerticalScroll,
        "$80:AF89 upward setup preserves source BG1 minus five pixels");

    Console.WriteLine("  BG scroll: parallax, signed blocks, row/column dispatch, and door setup agree.");
}

/// <summary>Checks all four branches shared by $80:AA95 and $80:AC57.</summary>
static void VerifyLevelBlockTilemapExpansion()
{
    // Put the definition at index one so the test also exercises the level entry's ten-bit
    // lookup. Existing child flip/palette bits are retained and XORed, never reconstructed.
    var definitions = new byte[16];
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(8), 0x0123);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(10), 0x4567);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(12), 0x89ab);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(14), 0xcdef);

    AssertEqual(
        new ExpandedBlockTiles(0x0123, 0x4567, 0x89ab, 0xcdef),
        LevelBlockTilemapExpander.Expand(0x0001, definitions),
        "unflipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles(0x0567, 0x4123, 0x8def, 0xc9ab),
        LevelBlockTilemapExpander.Expand(0x0401, definitions),
        "horizontally flipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles(0x09ab, 0x4def, 0x8123, 0xc567),
        LevelBlockTilemapExpander.Expand(0x0801, definitions),
        "vertically flipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles(0x0def, 0x49ab, 0x8567, 0xc123),
        LevelBlockTilemapExpander.Expand(0x0c01, definitions),
        "doubly flipped block expansion");

    AssertThrows<InvalidDataException>(
        () => LevelBlockTilemapExpander.Expand(0x0002, definitions),
        "block definition bounds check");

    Console.WriteLine("  BG stream: all 16x16 block flip expansions agree.");
}

/// <summary>
/// Verifies the shared row-major BG1/BTS model that will feed bank-$94 collision while
/// continuing to construct the already-verified bank-$80 visual streamer.
/// </summary>
static void VerifyRoomLevelData()
{
    const int width = 3;
    const int height = 2;
    ushort[] foreground = [
        0x0000, 0x1123, 0x8567,
        0xc001, 0xe002, 0xf003,
    ];
    byte[] behavior = [0x00, 0x45, 0x80, 0x11, 0x22, 0x33];
    ushort[] background = [0, 1, 2, 3, 4, 5];
    var definitions = new byte[8 * 4];
    var level = new RoomLevelData(width, height, foreground, behavior, background, definitions);

    RoomCollisionBlock block = level.GetCollisionBlock(blockX: 1, blockY: 1);
    AssertEqual(4, block.Index, "room collision row-major index");
    AssertEqual(0xe002, block.LevelWord, "room collision level word");
    AssertEqual(0x22, block.Behavior, "room collision parallel BTS byte");
    AssertEqual(RoomCollisionType.GrappleBlock, block.CollisionType,
        "room collision high-nibble dispatcher type");
    AssertEqual(2, block.VisualBlockIndex, "room collision visual block index");

    // Pixel (31,17) is block (1,1); shifts must occur before multiplication/indexing.
    AssertEqual(block, level.GetCollisionBlockAtPixel(31, 17), "room pixel-to-block conversion");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(width, 0),
        "room collision rejects X beyond header width");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(0, height),
        "room collision rejects Y beyond header height");

    BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
    AssertTrue(streamer is not null, "room level constructs visual streamer from same allocation");

    Console.WriteLine("  Room level: shared BG1/BTS indexing, collision type, and pixel conversion agree.");
}

/// <summary>
/// Exercises bank-$8F's inline room-state selector bytecode without the private cartridge.
/// </summary>
static void VerifyCartridgeRoomStateSelection()
{
    var bus = new TestAddressSpace();
    const ushort roomPointer = 0x9000;
    const int roomAddress = 0x8f0000 | roomPointer;

    // Fixed eleven-byte header. Only area and door-list values are semantically interesting
    // here; zero dimensions are legal because this test stops before asset construction.
    bus.WriteBytes(roomAddress,
    [
        0x12, 0x03, 0x00, 0x00, 0x00, 0x00, 0x70, 0xa0, 0x00, 0x00, 0x88,
    ]);

    // Ordered selector program: event, boss bit, Morph Ball+missiles, power bombs, finish.
    // Finish's following byte is the inline default state header, so its expected pointer is
    // derived from this exact command layout instead of fabricated by the production code.
    int selector = roomAddress + 11;
    WriteTestWord(bus, selector, RoomStateSelectorCodes.EventHasBeenSet);
    bus.WriteByte(selector + 2, 0x00);
    WriteTestWord(bus, selector + 3, 0x9100);
    WriteTestWord(bus, selector + 5, RoomStateSelectorCodes.BossIsDead);
    bus.WriteByte(selector + 7, 0x04);
    WriteTestWord(bus, selector + 8, 0x9120);
    WriteTestWord(bus, selector + 10, RoomStateSelectorCodes.MorphBallAndMissiles);
    WriteTestWord(bus, selector + 12, 0x9140);
    WriteTestWord(bus, selector + 14, RoomStateSelectorCodes.PowerBombs);
    WriteTestWord(bus, selector + 16, 0x9160);
    WriteTestWord(bus, selector + 18, RoomStateSelectorCodes.Finish);
    const ushort defaultStatePointer = 0x901f;

    // All five state headers may remain zero-filled: State.Pointer alone proves which
    // branch won, and TestAddressSpace returns zero for every unwritten payload byte.
    AssertEqual(defaultStatePointer,
        CartridgeRoomHeader.Load(bus, roomPointer).State.Pointer,
        "room selector default inline state");

    var eventBytes = new byte[Bank80SystemState.EventByteCount];
    eventBytes[0] = 1;
    AssertEqual((ushort)0x9100,
        CartridgeRoomHeader.Load(bus, roomPointer,
            new RoomStateSelectionContext(eventBytes, BossBits.None, false, false)).State.Pointer,
        "room selector event branch");
    AssertEqual((ushort)0x9120,
        CartridgeRoomHeader.Load(bus, roomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), BossBits.AreaTorizo, false, false)).State.Pointer,
        "room selector boss branch");
    AssertEqual((ushort)0x9140,
        CartridgeRoomHeader.Load(bus, roomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), BossBits.None, true, false)).State.Pointer,
        "room selector Morph Ball and missiles branch");
    AssertEqual((ushort)0x9160,
        CartridgeRoomHeader.Load(bus, roomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), BossBits.None, false, true)).State.Pointer,
        "room selector power-bomb branch");

    // The interpreter must return at the first successful command; later facts do not
    // override the event-selected pointer merely because they are also true.
    AssertEqual((ushort)0x9100,
        CartridgeRoomHeader.Load(bus, roomPointer,
            new RoomStateSelectionContext(eventBytes, BossBits.AreaTorizo, true, true)).State.Pointer,
        "room selector preserves cartridge priority");

    // The Tourian-specific routine owns its boss-bit operand in executable code rather
    // than the room stream. Exercise it separately so the catalogued implicit mask cannot
    // accidentally be replaced with the operand-reading behavior of the generic routine.
    WriteTestWord(bus, selector, RoomStateSelectorCodes.MainAreaBossIsDead);
    WriteTestWord(bus, selector + 2, 0x9180);
    WriteTestWord(bus, selector + 4, RoomStateSelectorCodes.Finish);
    AssertEqual((ushort)0x9180,
        CartridgeRoomHeader.Load(bus, roomPointer,
            new RoomStateSelectionContext(
                Array.Empty<byte>(),
                RoomStateSelectorOperands.MainAreaBoss,
                false,
                false)).State.Pointer,
        "room selector main-area boss implicit operand");

    // Known unused callbacks remain deliberately unsupported, while an arbitrary word is
    // diagnosed as unknown. Both must stop at the cartridge boundary instead of falling
    // through to a fabricated default state.
    WriteTestWord(bus, selector, RoomStateSelectorCodes.UnusedDoor);
    AssertThrows<NotSupportedException>(
        () => CartridgeRoomHeader.Load(bus, roomPointer),
        "known unused room selector fails loudly");
    WriteTestWord(bus, selector, 0xdead);
    AssertThrows<InvalidDataException>(
        () => CartridgeRoomHeader.Load(bus, roomPointer),
        "unknown room selector fails loudly");

    Console.WriteLine(
        "  Room states: complete selector catalog, operands, priority, and failures agree.");
}

/// <summary>Checks $80:A9DE-$80:AD17 staging geometry and $80:8CD8 NMI destinations.</summary>
static void VerifyBackgroundTilemapStreamer()
{
    const int width = 32;
    var level = new ushort[width * 32];
    var background = new ushort[level.Length];
    var definitions = new byte[8];
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(0), 0x0001);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(2), 0x0002);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(4), 0x0003);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(6), 0x0004);
    var streamer = new BackgroundTilemapStreamer(width, level, background, definitions);

    // X block $11 selects the second BG1 screen base ($53E0); Y block 5 splits a column
    // into 22 unwrapped words and 10 wrapped words for each of its left/right halves.
    var columnRequest = new BackgroundUpdateRequest(
        BackgroundLayer.Level,
        BackgroundUpdateAxis.Column,
        SourceXBlock: 2,
        SourceYBlock: 3,
        VramXBlock: 0x11,
        VramYBlock: 5);
    TilemapStreamUpdate column = streamer.Build(columnRequest)!
        ?? throw new InvalidOperationException("Non-Mode-7 column was incorrectly skipped.");
    AssertEqual(32, column.FirstHalves.Length, "column left staging words");
    AssertEqual(0x0001, column.FirstHalves[0], "column top-left tile");
    AssertEqual(0x0003, column.FirstHalves[1], "column bottom-left tile");
    AssertEqual(4, column.Segments.Count, "wrapped column DMA count");
    AssertEqual(22, column.Segments[0].WordCount, "column unwrapped word count");
    AssertEqual(10, column.Segments[2].WordCount, "column wrapped word count");
    AssertEqual(0x5542, column.Segments[0].VramWordDestination, "column unwrapped destination");
    AssertEqual(0x5402, column.Segments[2].VramWordDestination, "column wrapped destination");
    AssertEqual(TilemapDmaDirection.Column, column.Segments[0].Direction, "column VMAIN mode");
    var streamedVram = new SnesVram();
    column.ExecuteTo(streamedVram);
    AssertEqual(0x0001, streamedVram.ReadWord(0x5542), "column DMA left top word");
    AssertEqual(0x0002, streamedVram.ReadWord(0x5543), "column DMA right top word");
    AssertEqual(0x0003, streamedVram.ReadWord(0x5562), "column DMA left bottom word");

    // X within-screen 5 yields 22 unwrapped and 12 wrapped row words. The bottom half uses
    // the same destination with bit $20 set, exactly as the NMI routine does.
    var rowRequest = new BackgroundUpdateRequest(
        BackgroundLayer.Level,
        BackgroundUpdateAxis.Row,
        SourceXBlock: 2,
        SourceYBlock: 3,
        VramXBlock: 5,
        VramYBlock: 6);
    TilemapStreamUpdate row = streamer.Build(rowRequest)!
        ?? throw new InvalidOperationException("Non-Mode-7 row was incorrectly skipped.");
    AssertEqual(34, row.FirstHalves.Length, "row top staging words");
    AssertEqual(0x0001, row.FirstHalves[0], "row top-left tile");
    AssertEqual(0x0002, row.FirstHalves[1], "row top-right tile");
    AssertEqual(22, row.Segments[0].WordCount, "row unwrapped word count");
    AssertEqual(12, row.Segments[2].WordCount, "row wrapped word count");
    AssertEqual(0x518a, row.Segments[0].VramWordDestination, "row unwrapped destination");
    AssertEqual(0x51aa, row.Segments[1].VramWordDestination, "row bottom destination");
    AssertEqual(0x5580, row.Segments[2].VramWordDestination, "row wrapped destination");
    AssertEqual(TilemapDmaDirection.Row, row.Segments[0].Direction, "row VMAIN mode");
    streamedVram.Clear();
    row.ExecuteTo(streamedVram);
    AssertEqual(0x0001, streamedVram.ReadWord(0x518a), "row DMA top-left word");
    AssertEqual(0x0002, streamedVram.ReadWord(0x518b), "row DMA top-right word");
    AssertEqual(0x0003, streamedVram.ReadWord(0x51aa), "row DMA bottom-left word");

    // Ceres door $83:AB7C enters a 2x1-screen room with BG2 starting at block ($0C,$04).
    // The native sixteen-block column therefore continues four rows beyond the logical
    // 512-word plane into custom_background's retained allocation. This was the real-route
    // failure found while expanding the horizontal door regression beyond two rooms.
    var shortRoomAllocation = new ushort[RoomLevelMemoryLayout.PrefilledStreamingWordCount];
    Array.Fill(shortRoomAllocation, RoomLevelMemoryLayout.PrefilledLevelWord);
    var shortRoomStreamer = new BackgroundTilemapStreamer(
        width,
        shortRoomAllocation,
        shortRoomAllocation,
        definitions);
    TilemapStreamUpdate ceresDoorColumn = shortRoomStreamer.Build(
        new BackgroundUpdateRequest(
            BackgroundLayer.Background,
            BackgroundUpdateAxis.Column,
            SourceXBlock: 0x000c,
            SourceYBlock: 0x0004,
            VramXBlock: 0,
            VramYBlock: 0))
        ?? throw new InvalidOperationException("Ceres door BG2 column was incorrectly skipped.");
    AssertEqual(32, ceresDoorColumn.FirstHalves.Length,
        "$83:AB7C complete off-room BG2 column allocation");
    AssertEqual((ushort)0x0001, ceresDoorColumn.FirstHalves[^2],
        "$83:AB7C bottom overread retains prefilled block definition");

    AssertEqual<TilemapStreamUpdate?>(null, streamer.Build(rowRequest, mode7Enabled: true), "$80:AB78 Mode 7 return");
    Console.WriteLine("  BG stream: row/column staging splits and NMI destinations agree.");
}

/// <summary>Checks Mode-1 4-bpp BG pixels, palette selection, transparency, and H flip.</summary>
static void VerifyFourBitBackgroundRendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    // Character zero has only its top-left plane-zero bit set, producing color index one.
    var character = new byte[32];
    character[0] = 0x80;
    vram.LoadBytes(0, character);
    cgram.SetColor(2 * 16 + 1, 0x001f); // Full SNES red in BG palette two.

    ushort[] mapEntry = [0x0800]; // Character zero, palette two, no flips.
    vram.ExecuteWordTransfer(mapEntry, destinationWord: 0x5000, wordIncrement: 1);
    Rgba32[] pixels = SnesBgTilemapRenderer.Render4BppViewport(
        vram, cgram, 0x5000, 0, 0, 0, width: 8, height: 8);
    AssertEqual(new Rgba32(255, 0, 0), pixels[0], "4-bpp BG palette pixel");
    AssertEqual(0, pixels[1].A, "4-bpp BG color zero transparency");

    mapEntry[0] |= 0x4000;
    vram.ExecuteWordTransfer(mapEntry, destinationWord: 0x5000, wordIncrement: 1);
    pixels = SnesBgTilemapRenderer.Render4BppViewport(
        vram, cgram, 0x5000, 0, 0, 0, width: 8, height: 1);
    AssertEqual(0, pixels[0].A, "4-bpp H-flip old pixel becomes transparent");
    AssertEqual(new Rgba32(255, 0, 0), pixels[7], "4-bpp H-flip mirrored pixel");

    // BG2SC=$4A means a 32x64-tile map rooted at word $4800. Once VOFS bit eight
    // is set, the PPU selects its second $400-word screen at $4C00 rather than wrapping
    // into the first screen. Landing Site relies on this exact vertical arrangement.
    vram.ExecuteWordTransfer([0x0800], destinationWord: 0x4c00, wordIncrement: 1);
    pixels = SnesBgTilemapRenderer.Render4BppViewport(
        vram, cgram, 0x4800, 0, 0, 0x0100, width: 8, height: 1,
        tilemapWidthInTiles: 32, tilemapHeightInTiles: 64);
    AssertEqual(new Rgba32(255, 0, 0), pixels[0], "4-bpp BGSC vertical second screen");

    // Exercise that same BGSC geometry through the production priority compositor. Its
    // first gameplay scanline is physical Y=32, so VOFS=$E0 selects source Y=$100. A
    // per-line HOFS of eight must then select tile one from the lower vertical screen.
    var gameplayVram = new SnesVram();
    var gameplayCgram = new SnesCgram();
    var gameplayOam = new OamBuffer();
    var redCharacter = new byte[32];
    var blueCharacter = new byte[32];
    redCharacter[0] = 0x80;
    blueCharacter[0] = 0x80;
    // Leave character zero transparent because empty BG1 map words select it above BG2.
    gameplayVram.LoadBytes(0x0020, redCharacter);
    gameplayVram.LoadBytes(0x0040, blueCharacter);
    gameplayCgram.SetColor(2 * 16 + 1, 0x001f);
    gameplayCgram.SetColor(3 * 16 + 1, 0x7c00);
    gameplayVram.ExecuteWordTransfer([0x0801], destinationWord: 0x4800, wordIncrement: 1);
    gameplayVram.ExecuteWordTransfer([0x0801, 0x0c02], destinationWord: 0x4c00, wordIncrement: 1);
    var horizontalByLine = new ushort[SnesGameplayFrameRenderer.Height - SnesGameplayFrameRenderer.HudHeight];
    horizontalByLine[0] = 8;
    Rgba32[] gameplayFrame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
        gameplayVram,
        gameplayCgram,
        gameplayOam,
        bg1HorizontalScroll: 0,
        bg1VerticalScroll: 0,
        bg2HorizontalScroll: 0,
        bg2VerticalScroll: 0x00e0,
        bg2HorizontalScrollByLine: horizontalByLine,
        bg2TilemapWidthInTiles: 32,
        bg2TilemapHeightInTiles: 64);
    AssertEqual(new Rgba32(0, 0, 255),
        gameplayFrame[SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width],
        "ordinary compositor consumes sky HOFS and vertical BGSC screen");

    // The force-blank room fill is exactly 17 level columns when layer-2 X mode is odd.
    var scroll = new BackgroundScrollState
    {
        Layer1XPosition = 0x0120,
        Layer1YPosition = 0x0230,
        Layer2ScrollX = 0x81,
        Layer2ScrollY = 1,
    };
    IReadOnlyList<BackgroundUpdateRequest> initial = scroll.BuildInitialViewportRequests();
    AssertEqual(17, initial.Count, "$80:A176 initial BG1 column count");
    AssertEqual(0x0012, initial[0].SourceXBlock, "initial first source column");
    AssertEqual(0x0022, initial[16].SourceXBlock, "initial seventeenth source column");

    Console.WriteLine("  BG render: 4-bpp pixels, BGSC geometry, and 17-column initial fill agree.");
}

/// <summary>
/// Guards the physical-scanline relationship between the host-composited terrain and live
/// Samus/OAM. The gameplay IRQ hides BG1 behind the HUD; it does not rewind BG1VOFS when
/// BG1 becomes visible again on line 32.
/// </summary>
static void VerifyHostRoomViewportAlignment()
{
    const int roomWidth = 300;
    const int roomHeight = 260;
    const int cameraX = 7;
    const int cameraY = 11;
    var room = new Rgba32[roomWidth * roomHeight];

    // Give every source row a unique red component. The first gameplay output pixel must
    // come from world row cameraY+32; cameraY would expose the old 32-pixel alignment bug.
    for (int y = 0; y < roomHeight; y++)
    {
        for (int x = 0; x < roomWidth; x++)
            room[y * roomWidth + x] = new Rgba32((byte)y, (byte)x, 0);
    }

    var vram = new SnesVram();
    var cgram = new SnesCgram();
    var oam = new OamBuffer();
    Rgba32[] frame = SnesGameplayFrameRenderer.RenderHudRoomAndObjs(
        vram,
        cgram,
        oam,
        room,
        roomWidth,
        roomHeight,
        cameraX,
        cameraY);

    AssertEqual(
        room[(cameraY + SnesGameplayFrameRenderer.HudHeight) * roomWidth + cameraX],
        frame[SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width],
        "host terrain begins at physical scanline 32");
    AssertEqual(
        room[(cameraY + SnesGameplayFrameRenderer.Height - 1) * roomWidth + cameraX + 255],
        frame[^1],
        "host terrain bottom-right physical coordinate");

    Console.WriteLine("  Host terrain: room crop remains aligned with physical BG1 scanlines and live OAM.");
}

/// <summary>
/// Proves that desktop power-bomb composition consumes the same bank-$88 radius frame and
/// half-profile orientation as the original indirect-HDMA window builder.
/// </summary>
static void VerifyPowerBombColorMathWindow()
{
    var bus = new TestAddressSpace();

    // These are the literal `$88:A266-$88:A2A5` horizontal samples and vertical band
    // boundaries used by `$88:8CC6/$8D04/$8D46`. Keeping the fixture independent of the
    // production loop ensures a transposed table or rounded product cannot self-validate.
    bus.WriteBytes(0x88a266, [
        0x00, 0x0c, 0x19, 0x25, 0x31, 0x3e, 0x4a, 0x56,
        0x61, 0x6d, 0x78, 0x83, 0x8e, 0x98, 0xa2, 0xab,
        0xb5, 0xbd, 0xc5, 0xcd, 0xd4, 0xdb, 0xe1, 0xe7,
        0xec, 0xf1, 0xf4, 0xf8, 0xfb, 0xfd, 0xfe, 0xff,
    ]);
    bus.WriteBytes(0x88a286, [
        0xbf, 0xbf, 0xbe, 0xbd, 0xba, 0xb8, 0xb6, 0xb2,
        0xaf, 0xab, 0xa6, 0xa2, 0x9c, 0x96, 0x90, 0x8a,
        0x84, 0x7d, 0x75, 0x6e, 0x66, 0x5e, 0x56, 0x4d,
        0x45, 0x3c, 0x33, 0x2a, 0x20, 0x17, 0x0d, 0x04,
    ]);

    // All sixteen pre-explosion color entries use the same unmistakable fixed color in
    // this fixture. The state still performs its authentic radius-derived table lookup;
    // repeating the value merely keeps these geometry assertions focused and readable.
    for (int color = 0; color < 16; color++)
        bus.WriteBytes(0x889079 + color * 3, [0x01, 0x02, 0x03]);

    const ushort centerX = 100;
    const ushort centerY = 100;
    var explosion = new SamusPowerBombExplosionState();
    explosion.Arm();
    explosion.Spawn(centerX, centerY);

    // Spawn happens after the native HDMA pass. Even though status is already `$8000`,
    // the host must not invent an explosion table on this frame.
    Rgba32[] spawnFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(spawnFrame, bus, explosion, 0, 0);
    AssertEqual(new Rgba32(0, 0, 0, 255),
        spawnFrame[centerY * SnesGameplayFrameRenderer.Width + centerX],
        "power-bomb spawn frame has no premature HDMA window");

    // The first pre-instruction draws radius `$04.00`, then advances the live state to
    // `$34.00`. `$04 * $BF >> 8` gives a two-line vertical extent and `$04 * $FF >> 8`
    // gives a three-pixel center half-width. Testing beyond both extents catches use of
    // the already-updated `$34.00` radius as well as floating-point ellipse substitution.
    explosion.StepFrame(bus);
    AssertEqual(0x0400, explosion.RenderedPreExplosionRadius,
        "power-bomb renderer retains pre-update radius");
    AssertEqual(0x3400, explosion.PreExplosionRadius,
        "power-bomb logic advances next-frame radius");
    Rgba32[] scaledFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(scaledFrame, bus, explosion, 0, 0);
    Rgba32 fixedColor = new(8, 16, 24, 255);
    AssertEqual(fixedColor,
        scaledFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 3],
        "scaled power-bomb center uses truncated final horizontal sample");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        scaledFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 4],
        "scaled power-bomb center excludes first outside pixel");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        scaledFrame[(centerY + 3) * SnesGameplayFrameRenderer.Width + centerX],
        "scaled power-bomb vertical extent uses ROM boundary table");

    // A pre-scaled record is a center-outward half-profile. Native HDMA mirrors it around
    // the explosion Y coordinate; it is not indexed by adding a screen-space midpoint.
    while (explosion.Phase == PowerBombExplosionPhase.PreExplosionWhite)
        explosion.StepFrame(bus);
    bus.WriteBytes(0x889f06, [0x07, 0x03, 0x00]);
    explosion.StepFrame(bus);
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow, explosion.RenderedPhase,
        "first pre-scaled yellow frame is retained for composition");
    Rgba32[] shapeFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(shapeFrame, bus, explosion, 0, 0);
    AssertEqual(fixedColor,
        shapeFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 7],
        "pre-scaled profile byte zero draws center half-width");
    AssertEqual(fixedColor,
        shapeFrame[(centerY + 1) * SnesGameplayFrameRenderer.Width + centerX + 3],
        "pre-scaled profile byte one draws mirrored adjacent line");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        shapeFrame[(centerY + 2) * SnesGameplayFrameRenderer.Width + centerX],
        "pre-scaled zero terminates shape extent");

    Console.WriteLine("  Power bomb: ROM curve bands, rendered-frame timing, and center-outward shapes agree.");
}

static Rgba32[] CreateOpaqueBlackGameplayFrame()
{
    var frame = new Rgba32[
        SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
    Array.Fill(frame, new Rgba32(0, 0, 0, 255));
    return frame;
}

/// <summary>Checks bank-$88 sky fixed-point bands and four queued circular-map rows.</summary>
static void VerifyScrollingSkyState()
{
    AssertTrue(
        ScrollingSkyState.IsLandRoomMain(RoomMainCodePointers.ScrollingSkyLand),
        "$8F:C116 selects land scrolling sky");
    AssertTrue(
        ScrollingSkyState.IsLandRoomMain(RoomMainCodePointers.ScrollingSkyLandZebesTimebombSet),
        "$8F:C120 selects land scrolling sky before quake work");
    AssertTrue(
        !ScrollingSkyState.IsLandRoomMain(0xc11b),
        "$8F:C11B ocean wrapper is not silently treated as land sky");

    var sky = new ScrollingSkyState();
    var writes = new VramWriteQueue();
    sky.ProcessFrame(layer1YPosition: 0x041f, timeIsFrozen: false, writes);

    AssertEqual(0x041f, sky.VerticalScroll, "$88:AFB2 BG2 vertical scroll");
    AssertEqual(4, writes.Entries.Count, "$88:AFA3 four sky transfers");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ad1c0, 0x4820), writes.Entries[0], "sky upper first row");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ad200, 0x4840), writes.Entries[1], "sky upper second row");

    // Chunk index five is the intentional table-adjacency wrap to tilemap zero.
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab1c0, 0x4c20), writes.Entries[2], "sky wrapped lower first row");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab200, 0x4c40), writes.Entries[3], "sky wrapped lower second row");

    // At the cutscene's camera Y=0, the unsigned subtraction produces $FFF0 and Y=$01FE.
    // The 65C816 consequently reads a word at $88:AF9A, 510 bytes beyond $88:AD9C.
    // Populate only the two ROM words this fixture needs: declared chunk zero ($B180) and
    // the actual adjacent instruction bytes interpreted as pointer $ADA6.
    var bank88Rom = new byte[0x048000];
    int chunkZeroOffset = SuperMetroidAddressSpace.ToRomOffset(0x88ad9c);
    bank88Rom[chunkZeroOffset] = 0x80;
    bank88Rom[chunkZeroOffset + 1] = 0xb1;
    int wrappedPointerOffset = SuperMetroidAddressSpace.ToRomOffset(0x88af9a);
    bank88Rom[wrappedPointerOffset] = 0xa6;
    bank88Rom[wrappedPointerOffset + 1] = 0xad;
    var topSky = new ScrollingSkyState(new SuperMetroidAddressSpace(bank88Rom));
    var topWrites = new VramWriteQueue();
    topSky.ProcessFrame(layer1YPosition: 0, timeIsFrozen: false, topWrites);
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab526, 0x4fc0), topWrites.Entries[0],
        "sky Y=0 wrapped ROM pointer read");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab900, 0x4bc0), topWrites.Entries[2],
        "sky Y=0 lower row remains in chunk zero");

    // The $02E0 section aliases data slot eight, which already received the $0238 row's
    // +0.8000. Its additional +0.C000 produces integer 1 after only one frame.
    AssertEqual(1, sky.GetDataSlotPosition(8), "sky aliased fast HDMA slot");
    ushort[] lines = sky.BuildGameplayHorizontalScrolls(layer1YPosition: 0x02c0, lineCount: 1);
    AssertEqual(1, lines[0], "sky scanline resolves aliased HDMA slot");

    int tailBeforeFreeze = writes.TailInBytes;
    sky.ProcessFrame(layer1YPosition: 0x041f, timeIsFrozen: true, writes);
    AssertEqual(false, sky.HdmaEnabled, "frozen sky terminates HDMA table");
    AssertEqual(tailBeforeFreeze, writes.TailInBytes, "frozen sky queues no rows");

    Console.WriteLine("  Sky: HDMA bands, circular uploads, and the Y=0 ROM overread agree.");
}

/// <summary>
/// Exercises bank-$90 firing and movement, bank-$93 animation, and bank-$94 solid collision
/// for an uncharged power beam without sharing implementation code with the production path.
/// </summary>
}
