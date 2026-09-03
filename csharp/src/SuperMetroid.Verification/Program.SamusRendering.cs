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

/// <summary>Samus rendering, arm cannon, visor, hurt palette, and pose fixture verification.</summary>
static void VerifySamusRenderingSlice()
{
    var bus = new TestAddressSpace();
    SeedPoseOneSamusData(bus);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        AnimationFrame = 0,
        XPosition = 0x0480,
        YPosition = 0x0086,
    };
    var cgram = new SnesCgram();
    var vram = new SnesVram();
    var oam = new OamBuffer();

        SamusState.LoadPowerSuitPalette(bus, cgram);
    AssertEqual(0x3800, cgram.Colors[192], "Samus power-suit palette color zero at CGRAM 192");
    AssertEqual(0x000d, cgram.Colors[207], "Samus power-suit palette color fifteen at CGRAM 207");

    samus.PrimeGraphics(bus);
    AssertEqual(0x92d0b0, samus.TileTransfers.TopDefinitionAddress, "pose 1 frame 0 top tile definition");
    AssertEqual(0x92d1c8, samus.TileTransfers.BottomDefinitionAddress, "pose 1 frame 0 bottom tile definition");
    AssertTrue(samus.TileTransfers.TopTransferEnabled, "Samus top tile DMA flag");
    AssertTrue(samus.TileTransfers.BottomTransferEnabled, "Samus bottom tile DMA flag");

    samus.TileTransfers.TransferToVram(bus, vram);
    AssertEqual(0x10, vram.ReadByte(0xc000), "Samus top part 1 reaches VRAM word $6000");
    AssertEqual(0x20, vram.ReadByte(0xc200), "Samus top part 2 reaches VRAM word $6100");
    AssertEqual(0x30, vram.ReadByte(0xc100), "Samus bottom part 1 reaches VRAM word $6080");
    AssertEqual(0x40, vram.ReadByte(0xc300), "Samus bottom part 2 reaches VRAM word $6180");

    samus.InitializeAnimation(bus);
    AssertEqual(10, samus.AnimationFrameTimer, "pose 1 initial animation delay");

    // Frames 0-3 each last ten calls. The fifth byte is command $F6, which returns a
    // healthy Samus to frame zero rather than ever exposing command index four as art.
    for (int tick = 0; tick < 9; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual(0, samus.AnimationFrame, "standing frame remains zero for first nine ticks");
    AssertEqual(1, samus.AnimationFrameTimer, "standing timer reaches one before advance");
    samus.AnimateNoFx(bus);
    AssertEqual(1, samus.AnimationFrame, "standing frame advances on tenth tick");
    AssertEqual(10, samus.AnimationFrameTimer, "next standing frame reloads ten ticks");
    for (int tick = 0; tick < 30; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual(0, samus.AnimationFrame, "healthy $F6 command loops standing animation");
    AssertEqual(0xf6, samus.LastAnimationDelayCommand!.Value, "healthy standing loop command");

    // Below 30 energy, the same command enters frames 5-8. Command $FE,$04 then subtracts
    // four byte positions and loops that faster eight-tick breathing sequence.
    samus.Health = 29;
    samus.InitializeAnimation(bus);
    for (int tick = 0; tick < 40; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual(5, samus.AnimationFrame, "low-health $F6 enters alternate sequence");
    AssertEqual(8, samus.AnimationFrameTimer, "low-health sequence uses eight-tick delay");
    for (int tick = 0; tick < 32; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual(5, samus.AnimationFrame, "$FE,$04 loops low-health standing sequence");
    AssertEqual(0xfe, samus.LastAnimationDelayCommand!.Value, "low-health backward-loop command");

    // Complete the otherwise-unused `$FA/$FC` instruction slots with direct bytecode
    // fixtures. These poses are not admitted gameplay routes, but they are valid retail
    // interpreter records and must preserve the same command-three publication seam.
    const byte unusedInstructionPose = 0x20;
    WriteTestWord(bus, 0x91b010 + unusedInstructionPose * 2, 0xf000);
    var unusedInstructionSamus = new SamusState { Pose = unusedInstructionPose };

    // `$FA gg aa` selects `gg` only when both vertical speed halves are zero.
    bus.WriteBytes(0x91f000, [0x01, 0xfa, 0x31, 0x32]);
    unusedInstructionSamus.InitializeAnimation(bus);
    unusedInstructionSamus.AnimateNoFx(bus);
    AssertEqual(0xfa, unusedInstructionSamus.LastAnimationDelayCommand!.Value,
        "unused vertical-speed instruction reaches `$FA`");
    AssertEqual((byte?)0x31, unusedInstructionSamus.PendingTransitionalPose,
        "$FA` zero Y speed selects first pose byte");
    unusedInstructionSamus.Kinematics.YSubspeed = 1;
    unusedInstructionSamus.InitializeAnimation(bus);
    unusedInstructionSamus.AnimateNoFx(bus);
    AssertEqual((byte?)0x32, unusedInstructionSamus.PendingTransitionalPose,
        "$FA` nonzero Y subspeed selects second pose byte");

    // `$FC eeee gg aa` uses a little-endian equipment word and distinct pose bytes.
    bus.WriteBytes(0x91f000, [0x01, 0xfc, 0x02, 0x00, 0x41, 0x42]);
    unusedInstructionSamus.Kinematics.YSubspeed = 0;
    unusedInstructionSamus.EquippedItems = 0;
    unusedInstructionSamus.InitializeAnimation(bus);
    unusedInstructionSamus.AnimateNoFx(bus);
    AssertEqual(0xfc, unusedInstructionSamus.LastAnimationDelayCommand!.Value,
        "unused equipment instruction reaches `$FC`");
    AssertEqual((byte?)0x41, unusedInstructionSamus.PendingTransitionalPose,
        "$FC` unequipped branch selects first pose byte");
    unusedInstructionSamus.EquippedItems = 0x0002;
    unusedInstructionSamus.InitializeAnimation(bus);
    unusedInstructionSamus.AnimateNoFx(bus);
    AssertEqual((byte?)0x42, unusedInstructionSamus.PendingTransitionalPose,
        "$FC` equipped branch selects second pose byte");

    // Restore the ordinary debugger scenario before verifying frame-zero drawing below.
    samus.Health = 99;
    samus.InitializeAnimation(bus);

    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();

    // Pose $01's graphics Y offset is six, so world (1152,134) becomes origin (128,128).
    AssertEqual(128, samus.SpritemapXPosition, "Samus default screen X calculation");
    AssertEqual(128, samus.SpritemapYPosition, "Samus signed graphics-Y offset calculation");
    AssertEqual(0x019a, samus.TopSpritemapIndex, "Samus pose 1 top spritemap index");
    AssertEqual(0x04aa, samus.BottomSpritemapIndex, "Samus pose 1 bottom spritemap index");
    AssertEqual(7, oam.LastFinalizedSpriteCount, "Samus pose 1 emits four top and three bottom OBJs");

    OamEntry firstTop = oam.GetEntry(0);
    AssertEqual(121, firstTop.X, "Samus top OBJ signed X offset");
    AssertEqual(120, firstTop.Y, "Samus top OBJ signed Y offset");
    AssertTrue(firstTop.IsLarge, "Samus top OBJ preserves ROM size bit");
    AssertEqual(4, firstTop.Palette, "Samus top OBJ preserves ROM palette");
    AssertEqual(2, firstTop.Priority, "Samus top OBJ preserves ROM priority");
    AssertEqual(0, firstTop.TileNumber, "Samus top OBJ tile number");

    OamEntry firstBottom = oam.GetEntry(4);
    AssertEqual(113, firstBottom.X, "Samus bottom OBJ signed X offset");
    AssertEqual(144, firstBottom.Y, "Samus bottom OBJ positive Y offset");
    AssertEqual(8, firstBottom.TileNumber, "Samus bottom OBJ tile number");

    // The verified ordinary transition applies pose $09 after movement/animation and before
    // drawing. Its movement type one uses default position math and always draws both halves.
    SeedPoseNineSamusData(bus);
    samus.ApplyStandingRightToRunningRight(bus);
    AssertEqual(0x09, samus.Pose, "standing-right transition applies running-right pose");
    AssertEqual(0, samus.AnimationFrame, "running transition resets animation frame");
    AssertEqual(2, samus.AnimationFrameTimer, "running pose first delay byte");
    AssertEqual(21, samus.Kinematics.YRadius, "running pose refreshes collision radius");

    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x00f9, samus.TopSpritemapIndex, "running pose top spritemap index");
    AssertEqual(0x00e3, samus.BottomSpritemapIndex, "running pose bottom spritemap index");
    AssertEqual(2, oam.LastFinalizedSpriteCount, "synthetic running pose draws top and bottom pieces");

    samus.ApplyRunningRightToStandingRight(bus);
    AssertEqual(0x01, samus.Pose, "running no-button fallback applies standing-right pose");
    AssertEqual(0, samus.AnimationFrame, "standing fallback resets animation frame");
    AssertEqual(10, samus.AnimationFrameTimer, "standing fallback reloads frame-zero delay");

    // `$90:868D` inserts one direct small-OBJ write between pose `$00`'s ordinary top and
    // bottom spritemaps. This record is easy to lose in a high-level “draw both halves”
    // abstraction, so verify its exact OAM order, coordinates, size, and attribute word.
    SeedForwardFacingSamusData(bus);
    samus.EquippedItems = 0;
    samus.HorizontalSpeed.BaseSpeed = 3;
    samus.Kinematics.YSpeed = 2;
    samus.ApplyForwardFacingPoseSetup(bus);
    AssertEqual(SamusPoseIds.ForwardFacingPowerSuitPose, samus.Pose, "no suit selects power forward pose");
    AssertEqual(24, samus.Kinematics.YRadius, "power forward setup reads radius 24");
    AssertEqual(8, samus.AnimationFrameTimer, "power forward setup reads delay eight");
    AssertEqual(0, samus.HorizontalSpeed.BaseSpeed, "forward setup clears base X speed");
    AssertEqual(0, samus.Kinematics.YSpeed, "forward setup clears Y speed");
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x0002, samus.TopSpritemapIndex, "power-suit forward top spritemap index");
    AssertEqual(0x0062, samus.BottomSpritemapIndex, "power-suit forward bottom spritemap index");
    AssertEqual(3, oam.LastFinalizedSpriteCount, "power-suit forward top/chest/bottom OAM order");
    OamEntry chestCover = oam.GetEntry(1);
    AssertEqual(121, chestCover.X, "forward chest-cover X is Samus screen X minus seven");
    AssertEqual(117, chestCover.Y, "forward chest-cover Y is Samus screen Y minus seventeen");
    AssertEqual(0x21, chestCover.TileNumber, "forward chest-cover tile number");
    AssertEqual(4, chestCover.Palette, "forward chest-cover palette");
    AssertEqual(3, chestCover.Priority, "forward chest-cover priority");
    AssertTrue(!chestCover.IsLarge, "forward chest-cover is one small OBJ");

    // `$9B` uses dedicated suited art and therefore must not inherit `$00`'s chest patch.
    samus.EquippedItems = 0x0001;
    samus.ApplyForwardFacingPoseSetup(bus);
    AssertEqual(SamusPoseIds.ForwardFacingSuitedPose, samus.Pose, "Varia selects suited forward pose");
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x00c2, samus.TopSpritemapIndex, "suited forward top spritemap index");
    AssertEqual(0x0122, samus.BottomSpritemapIndex, "suited forward bottom spritemap index");
    AssertEqual(2, oam.LastFinalizedSpriteCount, "suited forward emits no power-suit chest patch");

    // `$90:85E2` suppresses body OAM on odd invincibility frames but still calls the
    // bank-$92 tile-definition selector. This is deliberately not folded into the arm-
    // cannon test: `$90:C663` has a similar-looking but stricter condition of its own.
    samus.InvincibilityTimer = 1;
    samus.KnockbackTimer = 0;
    samus.TileTransfers.ClearTransferFlags();
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0, nmiFrameCounter: 1);
    oam.FinalizeFrame();
    AssertEqual(0, oam.LastFinalizedSpriteCount,
        "odd invincibility frame suppresses complete Samus body");
    AssertTrue(samus.TileTransfers.TopTransferEnabled,
        "hidden invincibility frame still selects top graphics DMA");
    AssertTrue(!samus.TileTransfers.BottomTransferEnabled,
        "hidden suited-forward frame preserves its bank-$92 bottom-set FF sentinel");

    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0, nmiFrameCounter: 2);
    oam.FinalizeFrame();
    AssertEqual(2, oam.LastFinalizedSpriteCount,
        "even invincibility frame draws complete Samus body");

    // Knockback and shine are two independent OR terms in the native branch. Prove each
    // on an odd frame so neither can pass accidentally through the even-NMI condition.
    samus.KnockbackTimer = 1;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0, nmiFrameCounter: 1);
    oam.FinalizeFrame();
    AssertEqual(2, oam.LastFinalizedSpriteCount,
        "knockback overrides odd-frame invincibility blink");

    samus.KnockbackTimer = 0;
    AssertTrue(samus.Shinespark.TryStoreFromSpeedBooster(0x0400),
        "test fixture installs nonzero native shine timer");
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0, nmiFrameCounter: 1);
    oam.FinalizeFrame();
    AssertEqual(2, oam.LastFinalizedSpriteCount,
        "shine timer overrides odd-frame invincibility blink");

    // Give every synthetic spritemap index used below one visible OBJ. Top and bottom base
    // indices are both zero, so finalized OAM count becomes a direct witness: one means the
    // top-only branch, two means the native bottom selector accepted that pose/frame.
    bus.WriteBytes(0x92e000, [1, 0, 0, 0, 0, 0x00, 0x28]);
    for (int index = 0; index <= 3; index++)
        WriteTestWord(bus, 0x92808d + index * 2, 0xe000);
    bus.WriteBytes(0x908d80, [
        0xf8, 0x00, 0xf8, 0x00,
        0xfc, 0xfe, 0xfc, 0xfe,
        0x00, 0x00, 0x00, 0x00,
        0xfc, 0x00, 0xfc, 0x00,
        0x05, 0x04, 0x05, 0x04,
        0x00, 0x00, 0x00, 0x00,
    ]);

    // Reset the unrelated flicker inputs before exercising the complete `$90:86EE/$870C`
    // bottom-half matrix. The named cases cover every comparison boundary in the native
    // code rather than checking only the common morph pose that first exposed the gap.
    samus.InvincibilityTimer = 0;
    samus.KnockbackTimer = 0;
    foreach ((byte pose, byte movementType, ushort frame, int expectedSprites, string name) in new[]
    {
        ((byte)0xd7, (byte)0x0a, (ushort)0, 1, "$D7 frame zero top-only"),
        ((byte)0xd8, (byte)0x0a, (ushort)2, 1, "$D8 frame two top-only"),
        ((byte)0xd7, (byte)0x0a, (ushort)3, 2, "$D7 frame three split"),
        ((byte)0x35, (byte)0x0f, (ushort)0, 2, "$35 basic crouch transition split"),
        ((byte)0x37, (byte)0x0f, (ushort)0, 1, "$37 morph transition top-only"),
        ((byte)0x3d, (byte)0x0f, (ushort)1, 1, "$3D unmorph transition top-only"),
        ((byte)0xdb, (byte)0x0f, (ushort)0, 2, "$DB frame-zero split"),
        ((byte)0xdc, (byte)0x0f, (ushort)1, 1, "$DC nonzero frame top-only"),
        ((byte)0xdd, (byte)0x0f, (ushort)1, 1, "$DD pre-frame-two top-only"),
        ((byte)0xf0, (byte)0x0f, (ushort)2, 2, "$F0 frame-two split"),
        ((byte)0xf1, (byte)0x0f, (ushort)0, 2, "$F1 aimed transition always split"),
        ((byte)0x60, (byte)0x07, (ushort)0, 1, "unused type-seven top-only"),
        ((byte)0x61, (byte)0x09, (ushort)0, 1, "unused type-nine top-only"),
        ((byte)0x62, (byte)0x0b, (ushort)0, 2, "unused type-B split"),
        ((byte)0x63, (byte)0x0c, (ushort)0, 2, "unused type-C split"),
        ((byte)0x65, (byte)0x0d, (ushort)0, 2, "unused type-D pose $65 frame-zero split"),
        ((byte)0x66, (byte)0x0d, (ushort)1, 1, "unused type-D pose $66 later top-only"),
        ((byte)0x67, (byte)0x0d, (ushort)1, 2, "other unused type-D pose remains split"),
    })
    {
        WritePoseDefinition(bus, pose, [8, movementType, 0xff, 0xff, 0, 0, 16, 0]);
        WriteTestWord(bus, 0x929263 + pose * 2, 0);
        WriteTestWord(bus, 0x92945d + pose * 2, 0);
        samus.Pose = pose;
        samus.AnimationFrame = frame;
        oam.BeginFrame();
        samus.Draw(bus, oam, layer1X: samus.XPosition, layer1Y: samus.YPosition);
        oam.FinalizeFrame();
        AssertEqual(expectedSprites, oam.LastFinalizedSpriteCount, name);
    }

    // `$90:864E` contains exactly 28 bottom-half handlers, and the complete retail
    // `$91:B629-$BD0F` pose table never stores a movement byte above `$1B`. A synthetic
    // larger value is malformed metadata, not an untranslated draw selector.
    WritePoseDefinition(bus, 0x64, [8, 0x1c, 0xff, 0xff, 0, 0, 16, 0]);
    samus.Pose = 0x64;
    samus.AnimationFrame = 0;
    AssertThrows<InvalidDataException>(
        () => samus.ReadMovementType(bus),
        "movement-type API rejects a discriminator outside the complete retail domain");
    oam.BeginFrame();
    AssertThrows<InvalidDataException>(
        () => samus.Draw(bus, oam, layer1X: samus.XPosition, layer1Y: samus.YPosition),
        "movement type beyond complete bottom-half table is rejected as malformed pose data");

    // Positioning must consume those same ROM bytes instead of duplicating a convenient
    // host switch. `$37` proves signed -4/-2 values; unused `$39` proves that the native
    // zero record remains admitted and does not fall through to its nonzero pose offset.
    samus.Pose = 0x37;
    samus.YPosition = 0x0086;
    samus.AnimationFrame = 0;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: samus.XPosition, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x0082, samus.SpritemapYPosition,
        "morph transition frame zero reads signed minus-four table byte");
    samus.AnimationFrame = 1;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: samus.XPosition, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x0084, samus.SpritemapYPosition,
        "morph transition frame one reads signed minus-two table byte");

    WritePoseDefinition(bus, 0x39, [8, 0x0f, 0xff, 0xff, 9, 0, 16, 0]);
    WriteTestWord(bus, 0x929263 + 0x39 * 2, 0);
    WriteTestWord(bus, 0x92945d + 0x39 * 2, 0);
    samus.Pose = 0x39;
    samus.AnimationFrame = 0;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: samus.XPosition, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x0086, samus.SpritemapYPosition,
        "unused transition pose reads native zero instead of generic graphics offset");

    // Standing's position selector has two special families. Front-view frames zero/one
    // remain generic, but frame two and later use Y-1. Keep frame two here because pose `$00`
    // already carries a deliberately different graphics offset in the fixture above.
    samus.Pose = SamusPoseIds.ForwardFacingPowerSuitPose;
    samus.AnimationFrame = 2;
    samus.YPosition = 0x0086;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x0085, samus.SpritemapYPosition,
        "front-facing frame two uses fixed one-pixel graphics offset");

    // Landing's `$90:8D28` table is byte-packed but read by a 16-bit unaligned LDA. The
    // next frame's byte becomes the high half of the subtraction. Its low byte produces the
    // expected on-screen nudge; retaining the wrapped high byte proves this is the actual
    // 65816 operation rather than a visually plausible host-only byte lookup.
    bus.WriteBytes(0x908d28, [
        3, 6, 0, 0,
        3, 6, 0, 0,
        3, 3, 6, 0,
        3, 3, 6, 0,
        0,
    ]);
    WritePoseDefinition(bus, 0xa4, [8, 0, 0xff, 0xff, 9, 0, 21, 0]);
    WriteTestWord(bus, 0x929263 + 0xa4 * 2, 0);
    WriteTestWord(bus, 0x92945d + 0xa4 * 2, 0);
    samus.Pose = 0xa4;
    samus.AnimationFrame = 0;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0xfa83, samus.SpritemapYPosition,
        "normal-jump landing frame zero preserves unaligned word subtraction");
    AssertEqual(0x83, oam.GetEntry(0).Y,
        "landing OAM exposes low-byte three-pixel visual nudge");

    samus.AnimationFrame = 1;
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual(0x0080, samus.SpritemapYPosition,
        "normal-jump landing frame one reads overlapping 0006 word");

    // Ceres status bit `$8000` makes `$90:8C1F` borrow bank `$8B`'s Mode 7 point
    // transform before applying the ordinary pose offset. A 90-degree matrix around
    // ($0480,$0080) maps (+8,+6) to (-6,+8). The body must use that temporary point while
    // collision-visible Samus coordinates remain byte-for-byte unchanged afterward.
    samus.Pose = SamusPoseIds.FacingRightNormalPose;
    samus.AnimationFrame = 0;
    samus.XPosition = 0x0488;
    samus.YPosition = 0x0086;
    var quarterTurn = new SamusMode7Transform(
        MatrixA: 0x0000,
        MatrixB: 0x0100,
        MatrixC: 0xff00,
        CenterX: 0x0480,
        CenterY: 0x0080);
    oam.BeginFrame();
    samus.Draw(
        bus,
        oam,
        layer1X: 0x0400,
        layer1Y: 0,
        mode7Transform: quarterTurn);
    oam.FinalizeFrame();
    AssertEqual(0x007a, samus.SpritemapXPosition,
        "Mode 7 quarter-turn rotates Samus render X around M7X");
    AssertEqual(0x0082, samus.SpritemapYPosition,
        "Mode 7 quarter-turn applies pose graphics offset after rotated Y");
    AssertEqual(0x0488, samus.XPosition,
        "Mode 7 body calculation restores physical Samus X");
    AssertEqual(0x0086, samus.YPosition,
        "Mode 7 body calculation restores physical Samus Y");

    // Exercise negative products and a coordinate wrap independently of the convenient
    // quarter-turn result. The explicit reference operations mirror `$8B:8A52`'s two
    // word-sized accumulators and catch a host implementation that retains extra precision.
    var wrappedMatrix = new SamusMode7Transform(
        MatrixA: 0xff80,
        MatrixB: 0x0180,
        MatrixC: 0xfe80,
        CenterX: 0xfff0,
        CenterY: 0x0010);
    SamusMode7Point wrapped = wrappedMatrix.Transform(0x0010, 0xffe0);
    AssertEqual(0x0028, wrapped.X,
        "Mode 7 transform preserves signed-product and center-X word wrap");
    AssertEqual(0x0058, wrapped.Y,
        "Mode 7 transform preserves signed-product and center-Y word wrap");

    Console.WriteLine("  Samus: body art, Mode 7 position, bottom rules, tile DMA, OAM, and invincibility flicker agree.");
}

/// <summary>
/// Walks the complete `$90:C519-$C790` arm-cannon cover lifetime. The fixture keeps the
/// native three-level pose/direction/frame pointer topology intact, so a plausible-looking
/// host animation cannot pass by substituting invented timing or a fixed bitmap.
/// </summary>
static void VerifySamusArmCannon()
{
    var bus = new TestAddressSpace();
    SeedPoseOneSamusData(bus);

    // The retail item table says missiles open the cover while no HUD item closes it.
    // Entries two through five are included to catch a shifted or shortened lookup even
    // though this focused timeline switches only between entries zero and one.
    bus.WriteBytes(0x90c7d9, [0, 1, 1, 0, 1, 0]);

    // Pose $01 points to a compact normal record: selector two, draw mode two (after the
    // body), then signed X/Y pairs. Animation frame zero therefore uses (+7,-3).
    WriteTestWord(bus, 0x90c7e1, 0xd000);
    bus.WriteBytes(0x90d000, [0x02, 0x02, 0x07, 0xfd, 0x09, 0xfb]);

    // Selector two's real attribute word names small OBJ tile $1F, palette four, priority
    // two. Its four-word tile list reserves entry zero and supplies frames one through
    // three in bank $9A; each draw uploads exactly one 32-byte 4bpp tile.
    WriteTestWord(bus, 0x90c795, 0x281f);
    WriteTestWord(bus, 0x90c7a9, 0xd100);
    WriteTestWord(bus, 0x90d100, 0x0000);
    WriteTestWord(bus, 0x90d102, 0x8120);
    WriteTestWord(bus, 0x90d104, 0x8140);
    WriteTestWord(bus, 0x90d106, 0x8160);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        AnimationFrame = 0,
        XPosition = 0x0480,
        YPosition = 0x0086,
        SelectedHudItem = 0,
    };

    // The HUD producer requires two identical samples. Merely changing selection sets the
    // native toggle word to one; only the following stable frame is allowed to transition.
    SamusArmCannonUpdateResult closedSampleOne = samus.ArmCannon.Update(bus, samus);
    SamusArmCannonUpdateResult closedSampleTwo = samus.ArmCannon.Update(bus, samus);
    AssertEqual(0, closedSampleOne.FrameAfter,
        "first closed HUD sample leaves cannon invisible");
    AssertEqual(0, closedSampleTwo.FrameAfter,
        "second closed HUD sample agrees with already-closed state");
    AssertEqual(2, samus.ArmCannon.ToggleFlag,
        "stable HUD selection saturates arm-cannon toggle at two");
    AssertEqual(2, closedSampleTwo.DrawingMode,
        "pose record publishes after-body arm-cannon draw mode");

    samus.SelectedHudItem = 1;
    SamusArmCannonUpdateResult selectionChanged = samus.ArmCannon.Update(bus, samus);
    AssertTrue(selectionChanged.HudItemChanged && !selectionChanged.TransitionStarted,
        "new missile selection waits one stable HUD frame");
    AssertEqual(0, selectionChanged.FrameAfter,
        "selection-change frame keeps cannon closed");

    SamusArmCannonUpdateResult openingOne = samus.ArmCannon.Update(bus, samus);
    AssertTrue(openingOne.TransitionStarted, "second missile sample begins opening");
    AssertEqual(1, openingOne.FrameAfter,
        "opening starts at zero and advances to frame one in the same call");
    AssertEqual(1, openingOne.OpenFlag, "missile selection stores open flag one");
    AssertEqual(1, openingOne.CloseFlag, "opening frame one retains transition flag");

    SamusArmCannonUpdateResult openingTwo = samus.ArmCannon.Update(bus, samus);
    SamusArmCannonUpdateResult openingThree = samus.ArmCannon.Update(bus, samus);
    AssertEqual(2, openingTwo.FrameAfter, "opening advances to cover frame two");
    AssertEqual(1, openingTwo.CloseFlag, "frame two remains transitional");
    AssertEqual(3, openingThree.FrameAfter, "opening clamps at cover frame three");
    AssertEqual(0, openingThree.CloseFlag, "fully open cover clears transition flag");

    // Rewind is intentionally unnecessary: selecting frame one in the public state is not
    // possible, which protects the model from debugger-only invalid combinations. The
    // fully-open draw still proves the same selector route and frame-indexed third tile.
    var oam = new OamBuffer();
    var vramWrites = new VramWriteQueue();
    oam.BeginFrame();
    SamusArmCannonDrawResult draw = samus.ArmCannon.Draw(
        bus, oam, vramWrites, samus, layer1X: 0x0400, layer1Y: 0, nmiFrameCounter: 0);
    AssertTrue(draw.SpriteWritten && draw.TileUploadQueued,
        "open cover emits one OBJ and queues its tile upload");
    AssertEqual(2, draw.DirectionSelector, "pose record selects direction two");
    AssertEqual(0x281f, draw.Attributes, "direction two uses retail OAM attributes");
    AssertEqual(0x8160, draw.TileSource, "frame three indexes third cover tile");
    AssertEqual(135, draw.ScreenX, "cover X includes signed pose offset and camera");
    AssertEqual(125, draw.ScreenY,
        "cover Y includes signed offset, graphics origin, and camera");
    AssertEqual(4, oam.NextByteOffset, "cover consumes exactly one four-byte OAM record");
    OamEntry cover = oam.GetEntry(0);
    AssertEqual(135, cover.X, "cover OAM X");
    AssertEqual(125, cover.Y, "cover OAM Y");
    AssertEqual(0x1f, cover.TileNumber, "cover OAM tile slot");
    AssertEqual(4, cover.Palette, "cover OAM palette");
    AssertEqual(2, cover.Priority, "cover OAM priority");
    AssertTrue(!cover.IsLarge, "arm-cannon cover is a small OBJ");
    AssertEqual(new VramWriteEntry(0x20, 0x9a8160, 0x61f0), vramWrites.Entries[0],
        "cover queues native bank-$9A tile DMA to VRAM $61F0");

    // Odd invincibility frames return before both OAM and DMA. An off-screen coordinate,
    // in contrast, suppresses only the OBJ: native code still refreshes the shared tile.
    samus.InvincibilityTimer = 1;
    var flickerOam = new OamBuffer();
    var flickerWrites = new VramWriteQueue();
    flickerOam.BeginFrame();
    SamusArmCannonDrawResult flicker = samus.ArmCannon.Draw(
        bus, flickerOam, flickerWrites, samus, 0x0400, 0, nmiFrameCounter: 1);
    AssertTrue(!flicker.SpriteWritten && !flicker.TileUploadQueued,
        "odd invincibility frame suppresses cover OBJ and DMA");
    AssertEqual(0, flickerWrites.Entries.Count, "flicker return leaves VRAM queue untouched");

    samus.InvincibilityTimer = 0;
    var clippedOam = new OamBuffer();
    var clippedWrites = new VramWriteQueue();
    clippedOam.BeginFrame();
    SamusArmCannonDrawResult clipped = samus.ArmCannon.Draw(
        bus, clippedOam, clippedWrites, samus, layer1X: 0x0500, layer1Y: 0, nmiFrameCounter: 0);
    AssertTrue(!clipped.SpriteWritten && clipped.TileUploadQueued,
        "off-screen cover omits OAM but retains tile DMA");
    AssertEqual(1, clippedWrites.Entries.Count, "clipped cover still has one VRAM transfer");

    // Closing mirrors opening but starts from synthetic frame four. The same stable-sample
    // call decrements immediately to three, followed by two, one, and invisible zero.
    samus.SelectedHudItem = 0;
    SamusArmCannonUpdateResult closeChanged = samus.ArmCannon.Update(bus, samus);
    AssertTrue(closeChanged.HudItemChanged && !closeChanged.TransitionStarted,
        "item deselection also waits one stable frame");
    SamusArmCannonUpdateResult closingThree = samus.ArmCannon.Update(bus, samus);
    SamusArmCannonUpdateResult closingTwo = samus.ArmCannon.Update(bus, samus);
    SamusArmCannonUpdateResult closingOne = samus.ArmCannon.Update(bus, samus);
    SamusArmCannonUpdateResult closingZero = samus.ArmCannon.Update(bus, samus);
    AssertTrue(closingThree.TransitionStarted, "second empty-item sample begins closing");
    AssertEqual(3, closingThree.FrameAfter, "closing begins visibly at frame three");
    AssertEqual(2, closingTwo.FrameAfter, "closing decrements to frame two");
    AssertEqual(1, closingOne.FrameAfter, "closing decrements to frame one");
    AssertEqual(0, closingZero.FrameAfter, "closing reaches invisible frame zero");
    AssertEqual(0, closingZero.CloseFlag, "fully closed cover clears transition flag");

    Console.WriteLine(
        "  Samus arm cannon: HUD debounce, open/close cadence, OAM, clipping, flicker, and tile DMA agree.");
}

/// <summary>
/// Proves the byte-aliased `$0A72/$0A73` visor timer, all three room-cycle colors, normal-
/// room reset, X-ray exclusion, and the inactive-charge call seam.
/// </summary>
static void VerifySamusVisorPalette()
{
    var bus = new TestAddressSpace();
    var cgram = new SnesCgram();
    ushort[] colors = [0x1000, 0x1001, 0x1002, 0x2000, 0x2001, 0x2002];
    WriteTestWords(bus, 0x9ba3c0, colors);

    var state = new SamusVisorPaletteState();
    cgram.SetColor(196, 0x7777);
    SamusVisorPaletteStepResult normal = state.Update(
        bus, cgram, specialSamusPaletteType: 0, layerBlendingDefaultConfig: 2);
    AssertEqual(SamusVisorPaletteAction.ResetForNormalRoom, normal.Action,
        "ordinary room resets visor animation");
    AssertEqual(0x0601, state.PackedTimerIndex,
        "ordinary room primes timer one and table offset six");
    AssertEqual(0x7777, cgram.Colors[196],
        "ordinary room reset does not overwrite current visor color");

    // The first `$28` call decrements timer one to zero and immediately copies offset six.
    SamusVisorPaletteStepResult first = state.Update(bus, cgram, 0, 0x0028);
    AssertEqual(SamusVisorPaletteAction.ColorWritten, first.Action,
        "backdrop room writes first visor color immediately");
    AssertEqual((byte?)6, first.SourceByteOffset, "first visor source is table offset six");
    AssertEqual(0x2000, cgram.Colors[196], "first backdrop visor color");
    AssertEqual(0x0805, state.PackedTimerIndex,
        "first write reloads five and advances packed offset to eight");

    // Four calls retain timers 4/3/2/1. The fifth reaches zero, writes, and reloads five.
    for (ushort expectedTimer = 4; expectedTimer >= 1; expectedTimer--)
    {
        SamusVisorPaletteStepResult countdown = state.Update(bus, cgram, 0, 0x002a);
        AssertEqual(SamusVisorPaletteAction.Countdown, countdown.Action,
            $"visor countdown timer {expectedTimer}");
        AssertEqual(unchecked((byte)expectedTimer), state.Timer,
            $"visor packed low byte reaches {expectedTimer}");
        if (expectedTimer == 1)
            break;
    }
    SamusVisorPaletteStepResult second = state.Update(bus, cgram, 0, 0x002a);
    AssertEqual((byte?)8, second.SourceByteOffset, "second visor source is table offset eight");
    AssertEqual(0x2001, cgram.Colors[196], "second backdrop visor color");
    AssertEqual(0x0a05, state.PackedTimerIndex,
        "second write advances packed offset to ten");

    for (int call = 0; call < 5; call++)
        state.Update(bus, cgram, 0, 0x0028);
    AssertEqual(0x2002, cgram.Colors[196], "third backdrop visor color");
    AssertEqual(0x0605, state.PackedTimerIndex,
        "third write wraps only to room-cycle offset six");

    ushort packedBeforeXray = state.PackedTimerIndex;
    cgram.SetColor(196, 0x3456);
    SamusVisorPaletteStepResult xray = state.Update(bus, cgram, 8, 0x0028);
    AssertEqual(SamusVisorPaletteAction.SuppressedByXray, xray.Action,
        "X-ray special handler suppresses ordinary visor cycle");
    AssertEqual(packedBeforeXray, state.PackedTimerIndex,
        "X-ray suppression freezes both packed bytes");
    AssertEqual(0x3456, cgram.Colors[196],
        "X-ray suppression preserves its independently owned color");

    // `HandleBeamChargePalettes` reaches the visor only through its no-charge branch.
    // This integration assertion prevents the exact state machine from becoming orphaned.
    var integratedSamus = new SamusState();
    var projectiles = new SamusProjectileSystem();
    SamusBeamChargePaletteStepResult charge = projectiles.UpdateBeamChargePalette(
        bus, cgram, integratedSamus, layerBlendingDefaultConfig: 0x0028);
    AssertEqual(SamusBeamChargePaletteAction.Inactive, charge.Action,
        "inactive charging retains beam-palette result");
    AssertEqual(SamusVisorPaletteAction.ColorWritten,
        projectiles.LastVisorPaletteStep.Action,
        "inactive charging falls through to visor handler");
    AssertEqual(0x2000, cgram.Colors[196],
        "integrated visor call reads bank-$9B room-cycle color");

    Console.WriteLine(
        "  Samus visor: packed timer/index, backdrop cycle, normal reset, and X-ray exclusion agree.");
}

/// <summary>
/// Exercises the complete ordinary `$91:D8AA-$D953` hurt-counter lifetime with diagnostic
/// palette records. This intentionally verifies every call rather than three hand-picked
/// frames: an off-by-one at seven, forty, or sixty would otherwise look visually plausible.
/// </summary>
static void VerifySamusHurtFlashPalette()
{
    var bus = new TestAddressSpace();
    var cgram = new SnesCgram();

    // Power/Varia/Gravity table entries are bank-$9B pointers. Use Gravity in the primary
    // run and seed all sixteen words distinctly so a partial ten-color copy cannot pass.
    WriteTestWord(bus, 0x91d727, 0x9400);
    WriteTestWord(bus, 0x91d729, 0x9440);
    WriteTestWord(bus, 0x91d72b, 0x9480);
    for (int color = 0; color < 16; color++)
    {
        WriteTestWord(bus, 0x9b9400 + color * 2, unchecked((ushort)(0x0100 + color)));
        WriteTestWord(bus, 0x9b9440 + color * 2, unchecked((ushort)(0x0200 + color)));
        WriteTestWord(bus, 0x9b9480 + color * 2, unchecked((ushort)(0x0300 + color)));
        WriteTestWord(bus, 0x9ba380 + color * 2, unchecked((ushort)(0x4000 + color)));
        WriteTestWord(bus, 0x9ba3a0 + color * 2, unchecked((ushort)(0x5000 + color)));
    }

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        EquippedItems = 0x0021, // Both suit bits prove Gravity's native precedence.
        HurtFlashCounter = 1,
    };

    int hurtPaletteCalls = 0;
    int normalPaletteCalls = 0;
    int untouchedCalls = 0;
    for (int call = 1; call <= 59; call++)
    {
        SamusHurtFlashPaletteStepResult step = SamusHurtFlashPalette.Update(
            bus, cgram, samus, controllerInput: 0);
        AssertEqual(call, step.CounterBefore,
            $"hurt palette call {call} reads pre-increment counter");

        if (call <= 6 && (call & 1) != 0)
        {
            hurtPaletteCalls++;
            AssertEqual(SamusHurtFlashPaletteAction.HurtFlash, step.Action,
                $"odd hurt call {call} selects fixed flash palette");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(unchecked((ushort)(0x4000 + color)), cgram.Colors[192 + color],
                    $"hurt call {call} copies flash color {color}");
            }
        }
        else if (call <= 6)
        {
            normalPaletteCalls++;
            AssertEqual(SamusHurtFlashPaletteAction.NormalSuitRestore, step.Action,
                $"even hurt call {call} restores equipment palette");
            AssertEqual(0x9b9480, step.PaletteAddress!.Value,
                $"hurt call {call} gives Gravity priority over Varia");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(unchecked((ushort)(0x0300 + color)), cgram.Colors[192 + color],
                    $"hurt call {call} copies Gravity color {color}");
            }
        }
        else
        {
            untouchedCalls++;
            AssertEqual(SamusHurtFlashPaletteAction.NoPaletteChange, step.Action,
                $"hurt call {call} preserves the existing palette");
        }

        if (call == 2)
        {
            AssertTrue(step.HurtSoundQueued, "hurt call two publishes impact SFX");
            AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x35), 6),
                samus.LiquidPhysics.SoundRequests[^1],
                "hurt impact uses library one sound $35 maximum six");
        }
    }

    AssertEqual(3, hurtPaletteCalls, "hurt lifetime has three flash writes");
    AssertEqual(3, normalPaletteCalls, "hurt lifetime has three suit restores");
    AssertEqual(53, untouchedCalls, "hurt lifetime has fifty-three preserving calls");
    AssertEqual(0, samus.HurtFlashCounter,
        "hurt counter clears when call fifty-nine increments it to sixty");
    AssertEqual(1, samus.LiquidPhysics.SoundRequests.Count,
        "ordinary hurt lifetime queues impact sound only once");

    // Cinematic call two suppresses impact audio and uses the dedicated intro palette in
    // place of equipment-selected colors. Odd call one remains the common hurt palette.
    var cinematic = new SamusState { HurtFlashCounter = 2, EquippedItems = 0x0020 };
    cinematic.LiquidPhysics.CinematicFunctionActive = true;
    SamusHurtFlashPaletteStepResult intro = SamusHurtFlashPalette.Update(
        bus, cgram, cinematic, controllerInput: 0);
    AssertEqual(SamusHurtFlashPaletteAction.IntroRestore, intro.Action,
        "cinematic even hurt call selects intro palette");
    AssertTrue(!intro.HurtSoundQueued, "cinematic hurt call suppresses impact SFX");
    AssertEqual(0, cinematic.LiquidPhysics.SoundRequests.Count,
        "cinematic hurt call leaves sound queue empty");
    for (int color = 0; color < 16; color++)
    {
        AssertEqual(unchecked((ushort)(0x5000 + color)), cgram.Colors[192 + color],
            $"cinematic restore copies intro color {color}");
    }

    // Counter forty calls command `$1C` for spin/wall-jump movement. A Screw Attack pose
    // must select `$33`; using the ROM movement-type byte keeps this a real dispatcher test.
    WritePoseDefinition(bus, SamusPoseIds.ScrewAttackRightPose,
        [0x08, 0x03, 0, 0, 0, 0, 0x15, 0]);
    var spinning = new SamusState
    {
        Pose = SamusPoseIds.ScrewAttackRightPose,
        HurtFlashCounter = 39,
    };
    SamusHurtFlashPaletteStepResult spinRecovery = SamusHurtFlashPalette.Update(
        bus, cgram, spinning, controllerInput: 0);
    AssertEqual(SamusHurtFlashRecoveryAction.ScrewAttackSound, spinRecovery.Recovery,
        "counter forty restores Screw Attack sound");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x33), 9),
        spinning.LiquidPhysics.SoundRequests[^1],
        "Screw Attack recovery uses library one maximum nine");

    // A non-spinning charged shot arms the native one-word latch. The post-draw consumer
    // queues `$41` only while Shoot is still held, then clears the latch in either case.
    WritePoseDefinition(bus, SamusPoseIds.FacingRightNormalPose,
        [0x08, 0x00, 0, 0, 0, 0, 0x15, 0]);
    var charging = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        HurtFlashCounter = 39,
        ProjectileFlareCounter = 0x10,
    };
    SamusHurtFlashPaletteStepResult chargeRecovery = SamusHurtFlashPalette.Update(
        bus, cgram, charging, (ushort)SnesButton.X);
    AssertEqual(SamusHurtFlashRecoveryAction.ResumeChargingBeamRequested,
        chargeRecovery.Recovery,
        "counter forty arms charging-beam recovery");
    AssertEqual(1, charging.ResumeChargingBeamSoundFlag,
        "charging recovery publishes native flag one");
    AssertTrue(SamusHurtFlashPalette.ConsumeResumeChargingBeamSound(
            charging, (ushort)SnesButton.X),
        "post-draw handler queues held charging sound");
    AssertEqual(0, charging.ResumeChargingBeamSoundFlag,
        "post-draw handler clears charging recovery flag");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x41), 9),
        charging.LiquidPhysics.SoundRequests[^1],
        "charging recovery queues library one sound $41 maximum nine");

    // Grapple's pointer comparison accepts wall-grab release because its native handler is
    // still below `$C856`; cancel-pending is exactly the cutoff and must remain silent.
    var grapple = new SamusState { HurtFlashCounter = 39 };
    grapple.Grapple.Phase = GrapplePhase.WallGrabRelease;
    SamusHurtFlashPaletteStepResult grappleRecovery = SamusHurtFlashPalette.Update(
        bus, cgram, grapple, controllerInput: 0);
    AssertEqual(SamusHurtFlashRecoveryAction.GrappleSound, grappleRecovery.Recovery,
        "counter forty restores pre-cancel grapple sound");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x06), 9),
        grapple.LiquidPhysics.SoundRequests[^1],
        "grapple recovery uses library one sound six maximum nine");

    var cancelledGrapple = new SamusState { HurtFlashCounter = 39 };
    cancelledGrapple.Grapple.Phase = GrapplePhase.CancelPending;
    SamusHurtFlashPaletteStepResult cancelledRecovery = SamusHurtFlashPalette.Update(
        bus, cgram, cancelledGrapple, controllerInput: 0);
    AssertEqual(SamusHurtFlashRecoveryAction.None, cancelledRecovery.Recovery,
        "grapple cancel cutoff suppresses recovery sound");
    AssertEqual(0, cancelledGrapple.LiquidPhysics.SoundRequests.Count,
        "cancelled grapple leaves sound queue empty");

    Console.WriteLine("  Samus hurt flash: full palette lifetime and impact/recovery sounds agree.");
}

/// <summary>
/// Constructs the exact pointer topology and meaningful bytes for retail pose $01 frame 0.
/// The graphics payload uses diagnostic values because correctness here is about routing;
/// the DebugRunner separately executes the same code against the user's actual cartridge.
/// </summary>
static void SeedPoseOneSamusData(TestAddressSpace bus)
{
    // Pose definition $91:B631: facing right, standing movement type, +6 graphics Y.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);

    // Pose 1's delay-table pointer and its complete healthy/low-health bytecode stream.
    WriteTestWord(bus, 0x91b012, 0xb298);
    bus.WriteBytes(0x91b298, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6, 0x08, 0x08, 0x08, 0x08, 0xfe, 0x04]);

    // Pose-to-spritemap base indices and the two pointer-table entries they resolve.
    WriteTestWord(bus, 0x929265, 0x019a);
    WriteTestWord(bus, 0x92945f, 0x04aa);
    WriteTestWord(bus, 0x9283c1, 0xa072);
    WriteTestWord(bus, 0x9289e1, 0xadbb);

    // Top spritemap $92:A072: four pieces. The assembler macro ORs size bit $8000
    // into X values such as $43F9, yielding the deliberately odd encoded word $C3F9.
    bus.WriteBytes(0x92a072, [
        0x04, 0x00,
        0xf9, 0xc3, 0xf8, 0x00, 0x28,
        0xf9, 0xc3, 0xf0, 0x02, 0x28,
        0x0a, 0x00, 0xfd, 0x04, 0x28,
        0x02, 0x00, 0xfd, 0x05, 0x28,
    ]);

    // Bottom spritemap $92:ADBB: three large pieces using tile names $08/$0A/$0C.
    bus.WriteBytes(0x92adbb, [
        0x03, 0x00,
        0xf1, 0xc3, 0x10, 0x08, 0x28,
        0xf9, 0xc3, 0x10, 0x0a, 0x28,
        0xf9, 0xc3, 0x00, 0x0c, 0x28,
    ]);

    // Pose 1 -> animation list $DB48; frame 0 -> top set 7/position C and bottom
    // set 0/position 6. Their list pointers plus position*7 produce D0B0 and D1C8.
    WriteTestWord(bus, 0x92d950, 0xdb48);
    bus.WriteBytes(0x92db48, [0x07, 0x0c, 0x00, 0x06]);
    WriteTestWord(bus, 0x92d92c, 0xd05c);
    WriteTestWord(bus, 0x92d938, 0xd19e);
    bus.WriteBytes(0x92d0b0, [0x00, 0xe1, 0x9c, 0xc0, 0x00, 0x80, 0x00]);
    bus.WriteBytes(0x92d1c8, [0x20, 0x88, 0x9d, 0xc0, 0x00, 0xc0, 0x00]);

    // Distinct first bytes prove each of the four fixed NMI destinations independently.
    bus.WriteByte(0x9ce100, 0x10);
    bus.WriteByte(0x9ce1c0, 0x20);
    bus.WriteByte(0x9d8820, 0x30);
    bus.WriteByte(0x9d88e0, 0x40);

    ushort[] powerSuitColors = [
        0x3800, 0x0108, 0x03bd, 0x1405, 0x3be0, 0x21a8, 0x579f, 0x4ad2,
        0x3a4e, 0x00bb, 0x02b5, 0x016b, 0x0252, 0x1104, 0x0074, 0x000d,
    ];
    WriteTestWords(bus, 0x9b9400, powerSuitColors);
}

/// <summary>
/// Seeds the two front-view pose records with compact diagnostic spritemaps. The base
/// indices, pose metadata, and raw chest-cover attributes are retail values; the one-piece
/// body maps keep this verifier focused on routing because DebugRunner covers real artwork.
/// </summary>
static void SeedForwardFacingSamusData(TestAddressSpace bus)
{
    // Both records face neither left nor right, use standing movement type zero, have an
    // eight-pixel graphics-origin offset, and use the front-view radius of 24 pixels.
    WritePoseDefinition(bus, 0, [0x00, 0x00, 0xff, 0xff, 0x08, 0x00, 0x18, 0x00]);
    bus.WriteBytes(0x91bb01, [0x00, 0x00, 0xff, 0xff, 0x08, 0x00, 0x18, 0x00]);

    // `$00/$9B` share `$91:B56F`: eight ticks on frame zero followed by command `$FF`.
    WriteTestWord(bus, 0x91b010, 0xb56f);
    WriteTestWord(bus, 0x91b146, 0xb56f);
    bus.WriteBytes(0x91b56f, [0x08, 0xff]);

    // Retail frame-zero bases: `$00` top/bottom `$0002/$0062`, `$9B` `$00C2/$0122`.
    WriteTestWord(bus, 0x929263, 0x0002);
    WriteTestWord(bus, 0x92945d, 0x0062);
    WriteTestWord(bus, 0x929399, 0x00c2);
    WriteTestWord(bus, 0x929593, 0x0122);

    // Point those four indices at one-piece diagnostic maps. The direct `$3821` chest
    // record is not included here: production Draw must append it independently.
    WriteTestWord(bus, 0x928091, 0xa200);
    WriteTestWord(bus, 0x928151, 0xa210);
    WriteTestWord(bus, 0x928211, 0xa220);
    WriteTestWord(bus, 0x9282d1, 0xa230);
    bus.WriteBytes(0x92a200, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28]);
    bus.WriteBytes(0x92a210, [0x01, 0x00, 0x00, 0x00, 0x10, 0x08, 0x28]);
    bus.WriteBytes(0x92a220, [0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x28]);
    bus.WriteBytes(0x92a230, [0x01, 0x00, 0x00, 0x00, 0x10, 0x09, 0x28]);

    // Draw's final tile-selection step follows each pose's four-byte frame record. Zeroed
    // set pointers are safe because this verifier does not execute the resulting DMA.
    WriteTestWord(bus, 0x92d94e, 0xe000);
    WriteTestWord(bus, 0x92da84, 0xe004);
    bus.WriteBytes(0x92e000, [0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00]);
}

/// <summary>Seeds the retail pose-$09 pointer topology with compact diagnostic spritemaps.</summary>
static void SeedPoseNineSamusData(TestAddressSpace bus)
{
    // Pose $09: facing right, movement type one, new-pose-unless-buttons $01, +6 graphics
    // offset, and radius 21. These are the eight retail bytes at $91:B671.
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);

    // Ten alternating 2/3-tick running frames followed by command $FF back to frame zero.
    WriteTestWord(bus, 0x91b022, 0xb20a);
    bus.WriteBytes(0x91b20a, [0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0xff]);

    // Retail frame-zero bases are top $00F9 and bottom $00E3. Their pointer slots select
    // compact one-piece diagnostic maps; production DebugRunner reads the real maps.
    WriteTestWord(bus, 0x929275, 0x00f9);
    WriteTestWord(bus, 0x92946f, 0x00e3);
    WriteTestWord(bus, 0x92827f, 0xa100);
    WriteTestWord(bus, 0x928253, 0xa108);
    bus.WriteBytes(0x92a100, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28]);
    bus.WriteBytes(0x92a108, [0x01, 0x00, 0x00, 0x08, 0x08, 0x28, 0x28]);

    // Reuse pose one's already-seeded diagnostic tile definitions by making pose $09 frame
    // zero choose the same top set/position and bottom set/position record.
    WriteTestWord(bus, 0x92d960, 0xdc48);
    bus.WriteBytes(0x92dc48, [0x07, 0x0c, 0x00, 0x06]);
}

/// <summary>
/// Checks required-new/required-held masks, extra-button acceptance, ROM priority order,
/// terminators, and same-pose suppression in the bank-$91 prospective-pose lookup.
/// </summary>
}
