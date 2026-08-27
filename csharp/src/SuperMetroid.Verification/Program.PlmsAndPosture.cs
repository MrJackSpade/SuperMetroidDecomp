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

/// <summary>Breakable grapple PLMs and Samus posture verification.</summary>
static void VerifyBreakableGrapplePlms()
{
    var bus = new TestAddressSpace();

    // These bytes are the literal $84:CD6A and $84:CDA9 instruction streams. Production
    // code interprets these ROM words; the fixture does not inject a friendly C# timeline.
    bus.WriteBytes(0x84cd6a, [
        0xf0, 0x00, 0xf9, 0xa4,
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0xff, 0xa4,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x06, 0x00, 0x11, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0xff, 0xa4,
        0x93, 0xcd,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cda9, [
        0x78, 0x00, 0xf9, 0xa4,
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0xff, 0xa4,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x01, 0x00, 0x11, 0xa5,
        0xbc, 0x86,
    ]);

    // Each native draw instruction is `{one block, complete level word, terminator}`.
    bus.WriteBytes(0x84a4f9, [0x01, 0x00, 0xb7, 0xe0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4ff, [0x01, 0x00, 0x53, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a505, [0x01, 0x00, 0x54, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a50b, [0x01, 0x00, 0x55, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a511, [0x01, 0x00, 0xff, 0x00, 0x00, 0x00]);

    const int width = 8;
    const int height = 8;
    const int blockIndex = 3 * width + 3;
    var definitions = new byte[0x400 * 8];
    WriteDefinitionWord(definitions, 0xb7, 0, 0x1111);
    WriteDefinitionWord(definitions, 0xb7, 1, 0x2222);
    WriteDefinitionWord(definitions, 0xb7, 2, 0x3333);
    WriteDefinitionWord(definitions, 0xb7, 3, 0x4444);

    static RoomLevelData CreateLevel(byte behavior, byte[] blockDefinitions)
    {
        var foreground = new ushort[width * height];
        var bts = new byte[foreground.Length];
        foreground[blockIndex] = 0xe123;
        bts[blockIndex] = behavior;
        return CreateRoom(width, height, foreground, bts, blockDefinitions: blockDefinitions);
    }

    static void StepMany(
        RoomPlmSystem plms,
        TestAddressSpace addressSpace,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int count)
    {
        for (int frame = 0; frame < count; frame++)
            plms.Step(addressSpace, level, streamer, 0, 0, 0);
    }

    // BTS one waits 240 handler calls, breaks through four exact visual words, reverses
    // through the same words, restores both the saved level word and BTS one, then deletes
    // on the following pass because DrawPLMBlock deliberately seeded timer one.
    RoomLevelData respawning = CreateLevel(1, definitions);
    BackgroundTilemapStreamer respawningStreamer = respawning.CreateBackgroundStreamer();
    var respawningPlms = new RoomPlmSystem();
    AssertTrue(respawningPlms.TrySpawnBreakableGrappleBlock(respawning, blockIndex, 1),
        "respawning grapple PLM occupies a native slot");
    AssertEqual((byte)0, respawning.GetCollisionBlockByIndex(blockIndex).Behavior,
        "CFB5 clears breakable grapple BTS immediately");
    AssertEqual((ushort)0xe123, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "CFB5 retains original level word until handler");

    IReadOnlyList<PlmTilemapUpdate> firstDraw = respawningPlms.Step(
        bus, respawning, respawningStreamer, 0, 0, 0);
    AssertEqual((ushort)0xe0b7, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "connection-frame PLM pass draws grapple frame zero");
    AssertEqual(1, firstDraw.Count, "visible grapple mutation emits one VRAM redraw");
    AssertEqual((ushort)0x50c6, firstDraw[0].TopRowDestination,
        "PLM redraw targets block (3,3) in left BG1 ring screen");
    AssertEqual((ushort)0x1111, firstDraw[0].TopRow[0],
        "PLM redraw expands ROM-selected visual block definition");

    StepMany(respawningPlms, bus, respawning, respawningStreamer, 239);
    AssertEqual((ushort)0xe0b7, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning block remains grapple terrain through timer 240 minus one");
    respawningPlms.Step(bus, respawning, respawningStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "timer 240 expiry draws first air frame");
    AssertEqual(1, respawningPlms.SoundRequests.Count,
        "break transition queues one sound request");
    AssertEqual(new PlmSoundRequest(2, 0x0a, 6), respawningPlms.SoundRequests[0],
        "break transition uses library two sound $0A with maximum six");

    StepMany(respawningPlms, bus, respawning, respawningStreamer, 4);
    AssertEqual((ushort)0x0054, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning break frame one advances after four");
    StepMany(respawningPlms, bus, respawning, respawningStreamer, 4);
    AssertEqual((ushort)0x0055, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning break frame two advances after four");
    StepMany(respawningPlms, bus, respawning, respawningStreamer, 6);
    AssertEqual((ushort)0x00ff, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning break reaches blank frame after six");
    StepMany(respawningPlms, bus, respawning, respawningStreamer, 4 + 4 + 4 + 4);
    AssertEqual((ushort)0xe123, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "DrawPLMBlock restores the complete saved grapple word");
    AssertEqual((byte)1, respawning.GetCollisionBlockByIndex(blockIndex).Behavior,
        "respawning instruction restores BTS one before terrain");
    AssertEqual(1, respawningPlms.ActiveCount,
        "restoration pass retains PLM for timer-one delete delay");
    respawningPlms.Step(bus, respawning, respawningStreamer, 0, 0, 0);
    AssertEqual(0, respawningPlms.ActiveCount,
        "restored grapple PLM deletes on following handler pass");

    // BTS two uses the shorter 120-frame delay and never restores the saved word/BTS.
    RoomLevelData permanent = CreateLevel(2, definitions);
    BackgroundTilemapStreamer permanentStreamer = permanent.CreateBackgroundStreamer();
    var permanentPlms = new RoomPlmSystem();
    AssertTrue(permanentPlms.TrySpawnBreakableGrappleBlock(permanent, blockIndex, 2),
        "nonrespawning grapple PLM occupies a native slot");
    permanentPlms.Step(bus, permanent, permanentStreamer, 0, 0, 0);
    StepMany(permanentPlms, bus, permanent, permanentStreamer, 119);
    AssertEqual((ushort)0xe0b7, permanent.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "nonrespawning block retains grapple terrain through timer 120 minus one");
    permanentPlms.Step(bus, permanent, permanentStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053, permanent.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "nonrespawning timer 120 begins break sequence");
    StepMany(permanentPlms, bus, permanent, permanentStreamer, 4 + 4 + 4);
    AssertEqual((ushort)0x00ff, permanent.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "nonrespawning sequence ends on blank-air visual word");
    permanentPlms.Step(bus, permanent, permanentStreamer, 0, 0, 0);
    AssertEqual(0, permanentPlms.ActiveCount,
        "nonrespawning sequence deletes one frame after timer-one blank draw");
    AssertEqual((byte)0, permanent.GetCollisionBlockByIndex(blockIndex).Behavior,
        "nonrespawning sequence leaves cleared BTS");

    // The following bytes are the exact BTS-3/BTS-6 collision heads and their shared
    // bank-$84 tails. Together they exercise every structural feature that the bomb-block
    // family adds over grapple: queue-cap three, GotoY, a signed-offset second row, a true
    // vertical record, linked-block restoration, and the respawning/permanent split.
    bus.WriteBytes(0x84ccb7, [0x46, 0x8c, 0x06, 0x24, 0x87, 0xc1, 0xcc]);
    bus.WriteBytes(0x84ccc1, [
        0x04, 0x00, 0x9d, 0xa3,
        0x04, 0x00, 0xad, 0xa3,
        0x04, 0x00, 0xbd, 0xa3,
        0x80, 0x01, 0xcd, 0xa3,
        0x04, 0x00, 0xbd, 0xa3,
        0x04, 0x00, 0xad, 0xa3,
        0x04, 0x00, 0x9d, 0xa3,
        0x01, 0x00, 0xd7, 0xa4,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cd1b, [0x46, 0x8c, 0x06, 0x24, 0x87, 0x25, 0xcd]);
    bus.WriteBytes(0x84cd25, [
        0x04, 0x00, 0x7d, 0xa3,
        0x04, 0x00, 0x85, 0xa3,
        0x04, 0x00, 0x8d, 0xa3,
        0x01, 0x00, 0x95, 0xa3,
        0xbc, 0x86,
    ]);

    // `$A39D-$A3DC`: 2x2 animation draw records. Each begins with a two-word horizontal
    // row, then signed offset bytes `{0,+1}`, another two-word row, and a zero terminator.
    static void Write2x2Draw(TestAddressSpace fixtureBus, int address, ushort levelWord)
    {
        fixtureBus.WriteBytes(address, [
            0x02, 0x00,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x01,
            0x02, 0x00,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x00,
        ]);
    }
    Write2x2Draw(bus, 0x84a39d, 0x0053);
    Write2x2Draw(bus, 0x84a3ad, 0x0054);
    Write2x2Draw(bus, 0x84a3bd, 0x0055);
    Write2x2Draw(bus, 0x84a3cd, 0x00ff);
    bus.WriteBytes(0x84a4d7, [
        0x02, 0x00, 0x58, 0xf0, 0x58, 0x50,
        0x00, 0x01,
        0x02, 0x00, 0x58, 0xd0, 0x58, 0xd0,
        0x00, 0x00,
    ]);

    // `$A37D-$A39C`: vertical two-word records used by 1x2 blocks. The high count bit is
    // significant—the second level word advances by room width, never by one column.
    static void Write1x2Draw(TestAddressSpace fixtureBus, int address, ushort levelWord)
    {
        fixtureBus.WriteBytes(address, [
            0x02, 0x80,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x00,
        ]);
    }
    Write1x2Draw(bus, 0x84a37d, 0x0053);
    Write1x2Draw(bus, 0x84a385, 0x0054);
    Write1x2Draw(bus, 0x84a38d, 0x0055);
    Write1x2Draw(bus, 0x84a395, 0x00ff);

    static RoomLevelData CreateBombLevel(byte behavior, byte[] blockDefinitions)
    {
        var foreground = new ushort[width * height];
        var bts = new byte[foreground.Length];
        foreground[blockIndex] = 0xf321;
        bts[blockIndex] = behavior;
        return CreateRoom(width, height, foreground, bts, blockDefinitions: blockDefinitions);
    }

    RoomLevelData respawning2x2 = CreateBombLevel(3, definitions);
    BackgroundTilemapStreamer respawning2x2Streamer =
        respawning2x2.CreateBackgroundStreamer();
    var respawning2x2Plms = new RoomPlmSystem();
    AssertTrue(respawning2x2Plms.TrySpawnCollisionBombBlock(
        respawning2x2, blockIndex, behavior: 3),
        "BTS-3 collision setup occupies a native PLM slot");
    AssertEqual((ushort)0x0321, respawning2x2.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "CE83 synchronously removes only the type-F collision nibble");
    IReadOnlyList<PlmTilemapUpdate> first2x2Draw = respawning2x2Plms.Step(
        bus, respawning2x2, respawning2x2Streamer, 0, 0, 0);
    AssertEqual(new PlmSoundRequest(2, 0x06, 3), respawning2x2Plms.SoundRequests[0],
        "collision break queues library-two sound six with native maximum three");
    AssertEqual(4, first2x2Draw.Count,
        "2x2 draw record emits one debugger-visible redraw for each mutated level word");
    AssertEqual((ushort)0x0053,
        respawning2x2.GetCollisionBlock(3, 3).LevelWord,
        "2x2 first row begins at PLM origin");
    AssertEqual((ushort)0x0053,
        respawning2x2.GetCollisionBlock(4, 4).LevelWord,
        "signed {0,+1} record draws the 2x2 lower-right block");

    StepMany(respawning2x2Plms, bus, respawning2x2, respawning2x2Streamer, 12);
    AssertEqual((ushort)0x00ff, respawning2x2.GetCollisionBlock(4, 4).LevelWord,
        "BTS-3 reaches its four-block blank frame after three four-frame transitions");
    StepMany(respawning2x2Plms, bus, respawning2x2, respawning2x2Streamer, 384 + 12);
    AssertEqual((ushort)0xf058, respawning2x2.GetCollisionBlock(3, 3).LevelWord,
        "BTS-3 restores the type-F parent after the exact 384-frame blank hold");
    AssertEqual((ushort)0x5058, respawning2x2.GetCollisionBlock(4, 3).LevelWord,
        "BTS-3 restores the type-5 horizontal extension");
    AssertEqual((ushort)0xd058, respawning2x2.GetCollisionBlock(3, 4).LevelWord,
        "BTS-3 restores the type-D vertical extension row");
    AssertEqual(1, respawning2x2Plms.ActiveCount,
        "timer-one restored frame keeps BTS-3 PLM alive through this handler pass");
    respawning2x2Plms.Step(bus, respawning2x2, respawning2x2Streamer, 0, 0, 0);
    AssertEqual(0, respawning2x2Plms.ActiveCount,
        "BTS-3 deletes on the handler pass after linked-block restoration");

    RoomLevelData permanent1x2 = CreateBombLevel(6, definitions);
    BackgroundTilemapStreamer permanent1x2Streamer =
        permanent1x2.CreateBackgroundStreamer();
    var permanent1x2Plms = new RoomPlmSystem();
    AssertTrue(permanent1x2Plms.TrySpawnCollisionBombBlock(
        permanent1x2, blockIndex, behavior: 6),
        "BTS-6 collision setup occupies a native PLM slot");
    permanent1x2Plms.Step(bus, permanent1x2, permanent1x2Streamer, 0, 0, 0);
    AssertEqual((ushort)0x0053, permanent1x2.GetCollisionBlock(3, 3).LevelWord,
        "vertical draw writes BTS-6 origin");
    AssertEqual((ushort)0x0053, permanent1x2.GetCollisionBlock(3, 4).LevelWord,
        "vertical draw advances its second word by one room row");
    AssertEqual((ushort)0, permanent1x2.GetCollisionBlock(4, 3).LevelWord,
        "vertical draw does not accidentally advance into the neighboring column");
    StepMany(permanent1x2Plms, bus, permanent1x2, permanent1x2Streamer, 12);
    AssertEqual((ushort)0x00ff, permanent1x2.GetCollisionBlock(3, 4).LevelWord,
        "permanent BTS-6 finishes on two vertical blank-air words");
    permanent1x2Plms.Step(bus, permanent1x2, permanent1x2Streamer, 0, 0, 0);
    AssertEqual(0, permanent1x2Plms.ActiveCount,
        "permanent BTS-6 deletes one frame after its timer-one blank draw");

    Console.WriteLine("  Movement PLMs: grapple and collision-bomb ROM timing, multi-block terrain, sound, VRAM, and respawn agree.");
}

static void WriteDefinitionWord(byte[] definitions, int block, int tile, ushort value)
{
    int offset = block * 8 + tile * 2;
    definitions[offset] = unchecked((byte)value);
    definitions[offset + 1] = unchecked((byte)(value >> 8));
}

static void VerifySamusPostureMovement()
{
    var bus = new TestAddressSpace();
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]); // $02
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    bus.WriteBytes(0x91b649, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]); // $04
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    bus.WriteBytes(0x91b659, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]); // $06
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    bus.WriteBytes(0x91b669, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]); // $08
    bus.WriteBytes(0x91b761, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]); // $27
    bus.WriteBytes(0x91b769, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]); // $28
    bus.WriteBytes(0x91b7d1, [0x08, 0x0f, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]); // $35
    bus.WriteBytes(0x91b801, [0x08, 0x0f, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $3B
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b889, [0x04, 0x02, 0xff, 0x07, 0x03, 0x00, 0x13, 0x00]); // $4C
    bus.WriteBytes(0x91b9b1, [0x08, 0x05, 0x27, 0x01, 0x00, 0x00, 0x10, 0x00]); // $71
    bus.WriteBytes(0x91b9b9, [0x04, 0x05, 0x28, 0x08, 0x00, 0x00, 0x10, 0x00]); // $72
    bus.WriteBytes(0x91b9c1, [0x08, 0x05, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]); // $73
    bus.WriteBytes(0x91b9c9, [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]); // $74
    bus.WriteBytes(0x91ba51, [0x08, 0x05, 0x27, 0x00, 0x00, 0x00, 0x10, 0x00]); // $85
    bus.WriteBytes(0x91ba59, [0x04, 0x05, 0x28, 0x09, 0x00, 0x00, 0x10, 0x00]); // $86

    // These twelve literal records prove the aimed transition art retains command seven's
    // radius semantics: `$F1-$F6` already carry radius 16, while `$F7-$FC` carry radius 21.
    byte[] aimedCrouchTransitions = [0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6];
    byte[] aimedStandTransitions = [0xf7, 0xf8, 0xf9, 0xfa, 0xfb, 0xfc];
    byte[] transitionDirections = [0x08, 0x04, 0x08, 0x04, 0x08, 0x04];
    byte[] transitionShots = [0x00, 0x09, 0x01, 0x08, 0x03, 0x06];
    for (int index = 0; index < 6; index++)
    {
        int crouchAddress = 0x91b629 + aimedCrouchTransitions[index] * 8;
        bus.WriteBytes(crouchAddress, [
            transitionDirections[index], 0x0f, 0xff, transitionShots[index],
            0x08, 0x00, 0x10, 0x00,
        ]);
        int standAddress = 0x91b629 + aimedStandTransitions[index] * 8;
        bus.WriteBytes(standAddress, [
            transitionDirections[index], 0x0f, 0xff, transitionShots[index],
            0x03, 0x00, 0x15, 0x00,
        ]);
    }
    WriteTestWord(bus, 0x91b012, 0xc100);
    WriteTestWord(bus, 0x91b05e, 0xc110);
    WriteTestWord(bus, 0x91b07a, 0xc120);
    WriteTestWord(bus, 0x91b086, 0xc130);
    bus.WriteBytes(0x91c100, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6]);
    bus.WriteBytes(0x91c110, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6]);
    bus.WriteBytes(0x91c120, [0x02, 0xfd, 0x27]);
    bus.WriteBytes(0x91c130, [0x02, 0xfd, 0x01]);
    WriteTestWord(bus, 0x91b010 + SamusState.NeutralJumpTransitionRightPose * 2, 0xc140);
    WriteTestWord(bus, 0x91b010 + SamusState.NeutralJumpTransitionLeftPose * 2, 0xc150);
    bus.WriteBytes(0x91c140, [0x01, 0xfd, SamusState.NeutralJumpRightPose]);
    bus.WriteBytes(0x91c150, [0x01, 0xfd, SamusState.NeutralJumpLeftPose]);

    // Dry-air table-zero values used by Make_Samus_Jump and normal-air gravity. Keeping
    // these as literal ROM words makes a crouch jump observable beyond merely changing pose.
    bus.WriteBytes(0x909eb9, [0x04, 0x00]);
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]);
    bus.WriteBytes(0x909ea1, [0x00, 0x1c]);
    bus.WriteBytes(0x909ea7, [0x00, 0x00]);

    // Give every aimed transition its own command-$FD stream. Distinct stream pointers
    // catch accidental pose reuse; the literal target arrays mirror `$91:B518-$91:B53B`.
    byte[] aimedCrouchTargets = [0x85, 0x86, 0x71, 0x72, 0x73, 0x74];
    byte[] aimedStandTargets = [0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
    for (int index = 0; index < 6; index++)
    {
        ushort crouchStream = unchecked((ushort)(0xc200 + index * 0x10));
        WriteTestWord(bus, 0x91b010 + aimedCrouchTransitions[index] * 2, crouchStream);
        bus.WriteBytes(0x910000 | crouchStream, [0x02, 0xfd, aimedCrouchTargets[index]]);
        ushort standStream = unchecked((ushort)(0xc260 + index * 0x10));
        WriteTestWord(bus, 0x91b010 + aimedStandTransitions[index] * 2, standStream);
        bus.WriteBytes(0x910000 | standStream, [0x02, 0xfd, aimedStandTargets[index]]);
    }
    byte[] stablePosturePoses = [
        0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x28, 0x71, 0x72, 0x73, 0x74, 0x85, 0x86,
    ];
    for (int index = 0; index < stablePosturePoses.Length; index++)
    {
        ushort stream = unchecked((ushort)(0xc300 + index * 0x10));
        WriteTestWord(bus, 0x91b010 + stablePosturePoses[index] * 2, stream);
        bus.WriteBytes(0x910000 | stream, [0x10, 0xff]);
    }

    const int width = 8;
    const int height = 8;
    var floor = new ushort[width * height];
    for (int x = 0; x < width; x++)
        floor[4 * width + x] = 0x8000;
    RoomLevelData level = CreateRoom(
        width, height, floor, new byte[floor.Length]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 43, // standing bottom is pixel 63, immediately above row-four floor
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    AssertTrue(
        samus.TryApplyPostureTransition(
            bus, level, SamusState.CrouchingTransitionRightPose, nmiFrameCounter: 0),
        "standing begins crouch transition");
    AssertEqual((ushort)16, samus.Kinematics.YRadius, "crouch transition radius");
    AssertEqual((ushort)48, samus.YPosition, "command seven moves crouch center down five");

    for (int tick = 0; tick < 2; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((byte)0xfd, samus.LastAnimationDelayCommand!.Value, "crouch transition reaches FD");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "crouch FD applies");
    AssertEqual((byte)0x27, samus.Pose, "crouch transition target");
    GroundedMovementResult crouchFrame = SamusPostureMovement.StepCrouching(
        bus, level, samus, nmiFrameCounter: 0);
    AssertTrue(crouchFrame.Vertical.Collided, "crouch performs grounded probe");

    AssertTrue(
        samus.TryApplyPostureTransition(
            bus, level, SamusState.StandingTransitionRightPose, nmiFrameCounter: 1),
        "crouch begins standing transition");
    AssertEqual((ushort)21, samus.Kinematics.YRadius, "standing transition radius");
    AssertEqual((ushort)43, samus.YPosition, "floor-constrained expansion moves center up five");
    for (int tick = 0; tick < 2; tick++)
        samus.AnimateNoFx(bus);
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "standing FD applies");
    AssertEqual((byte)0x01, samus.Pose, "standing transition target");

    // Exercise all six facing/direction variants through both radius-changing halves. This
    // is intentionally a table test because the retail transition tables route every one
    // through the same two native collision commands but different `$FD` targets.
    for (int index = 0; index < 6; index++)
    {
        bool facesLeft = (index & 1) != 0;
        var aimedSamus = new SamusState
        {
            Pose = facesLeft ? SamusState.FacingLeftNormalPose : SamusState.FacingRightNormalPose,
            XPosition = 48,
            YPosition = 43,
        };
        aimedSamus.RefreshCollisionRadii(bus);
        aimedSamus.InitializeAnimation(bus);
        AssertTrue(
            aimedSamus.TryApplyPostureTransition(
                bus, level, aimedCrouchTransitions[index], unchecked((ushort)index)),
            $"aimed crouch transition ${aimedCrouchTransitions[index]:X2} begins");
        AssertEqual((ushort)16, aimedSamus.Kinematics.YRadius, "aimed crouch transition radius");
        AssertEqual((ushort)48, aimedSamus.YPosition, "aimed crouch keeps feet aligned");
        SamusPostureMovement.StepCrouchStandTransition(
            bus, level, aimedSamus, unchecked((ushort)index));
        aimedSamus.AnimateNoFx(bus);
        aimedSamus.AnimateNoFx(bus);
        AssertTrue(aimedSamus.ApplyPendingVerifiedAnimationTransition(bus), "aimed crouch FD applies");
        AssertEqual(aimedCrouchTargets[index], aimedSamus.Pose, "aimed crouch FD target");
        AssertEqual(
            facesLeft ? (byte)0x28 : (byte)0x27,
            aimedSamus.ReadNoInputFallbackPose(bus),
            "aimed crouch definition fallback");
        GroundedMovementResult aimedCrouchFrame = SamusPostureMovement.StepCrouching(
            bus, level, aimedSamus, unchecked((ushort)index));
        AssertTrue(aimedCrouchFrame.Vertical.Collided, "aimed crouch remains grounded");

        AssertTrue(
            aimedSamus.TryApplyPostureTransition(
                bus, level, aimedStandTransitions[index], unchecked((ushort)index)),
            $"aimed stand transition ${aimedStandTransitions[index]:X2} begins");
        AssertEqual((ushort)21, aimedSamus.Kinematics.YRadius, "aimed standing transition radius");
        AssertEqual((ushort)43, aimedSamus.YPosition, "aimed stand keeps feet aligned");
        aimedSamus.AnimateNoFx(bus);
        aimedSamus.AnimateNoFx(bus);
        AssertTrue(aimedSamus.ApplyPendingVerifiedAnimationTransition(bus), "aimed stand FD applies");
        AssertEqual(aimedStandTargets[index], aimedSamus.Pose, "aimed stand FD target");
    }

    // Equal-radius live shoulder changes never route through pose-change collision. Verify
    // both facing families and the definition-byte-two return to ordinary crouch.
    var crouchAim = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    crouchAim.RefreshCollisionRadii(bus);
    crouchAim.InitializeAnimation(bus);
    foreach (byte target in new byte[] {
        SamusState.CrouchingAimUpRightPose,
        SamusState.CrouchingAimDiagonalUpRightPose,
        SamusState.CrouchingAimDiagonalDownRightPose,
        SamusState.CrouchingRightPose,
    })
    {
        crouchAim.ApplyGroundedAimTransition(bus, target);
        AssertEqual((ushort)16, crouchAim.Kinematics.YRadius, $"right crouch aim ${target:X2} radius");
    }
    AssertThrows<NotSupportedException>(
        () => crouchAim.ApplyGroundedAimTransition(bus, SamusState.CrouchingAimUpLeftPose),
        "crouch aim cannot cross facing families");

    // Releasing Down while retaining the facing direction matches `$91:A6A0`'s direct
    // `$27 -> $01` record. This is not the `$3B` standing animation: radius expansion and
    // floor alignment happen immediately at the post-input pose-change seam.
    var directStand = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    directStand.RefreshCollisionRadii(bus);
    directStand.InitializeAnimation(bus);
    AssertTrue(
        directStand.TryApplyDirectCrouchToStandingTransition(
            bus, level, SamusState.FacingRightNormalPose, nmiFrameCounter: 0),
        "direct crouch-to-standing record applies");
    AssertEqual((byte)0x01, directStand.Pose, "direct crouch exit target");
    AssertEqual((ushort)21, directStand.Kinematics.YRadius, "direct crouch exit radius");
    AssertEqual((ushort)43, directStand.YPosition, "direct crouch exit keeps feet aligned");

    var directStandLeft = new SamusState
    {
        Pose = SamusState.CrouchingLeftPose,
        XPosition = 48,
        YPosition = 48,
    };
    directStandLeft.RefreshCollisionRadii(bus);
    directStandLeft.InitializeAnimation(bus);
    AssertTrue(
        directStandLeft.TryApplyDirectCrouchToStandingTransition(
            bus, level, SamusState.FacingLeftNormalPose, nmiFrameCounter: 1),
        "mirrored direct crouch-to-standing record applies");
    AssertEqual((byte)0x02, directStandLeft.Pose, "mirrored direct crouch exit target");
    AssertEqual((ushort)43, directStandLeft.YPosition, "mirrored direct crouch exit alignment");

    // `$91:FC66` first accepts the 16 -> 19 radius expansion, moving the center up three
    // against the floor, then subtracts ten more pixels only when PreviousPose is exactly
    // ordinary crouch `$27/$28`. Make_Samus_Jump follows with the ROM's 4.E000 velocity.
    var crouchJump = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    crouchJump.RefreshCollisionRadii(bus);
    crouchJump.InitializeAnimation(bus);
    AssertTrue(
        crouchJump.TryApplyCrouchJumpTransition(
            bus, level, SamusState.NeutralJumpTransitionRightPose, nmiFrameCounter: 0),
        "ordinary crouch jump applies");
    AssertEqual((byte)0x4b, crouchJump.Pose, "ordinary crouch jump transition pose");
    AssertEqual((ushort)19, crouchJump.Kinematics.YRadius, "ordinary crouch jump radius");
    AssertEqual((ushort)35, crouchJump.YPosition, "ordinary crouch jump collision plus FC8A offset");
    AssertEqual((ushort)4, crouchJump.Kinematics.YSpeed, "ordinary crouch jump Y speed");
    AssertEqual((ushort)0xe000, crouchJump.Kinematics.YSubspeed, "ordinary crouch jump Y subspeed");
    AssertEqual((ushort)1, crouchJump.Kinematics.YDirection, "ordinary crouch jump rises");

    var crouchJumpLeft = new SamusState
    {
        Pose = SamusState.CrouchingLeftPose,
        XPosition = 48,
        YPosition = 48,
    };
    crouchJumpLeft.RefreshCollisionRadii(bus);
    crouchJumpLeft.InitializeAnimation(bus);
    AssertTrue(
        crouchJumpLeft.TryApplyCrouchJumpTransition(
            bus, level, SamusState.NeutralJumpTransitionLeftPose, nmiFrameCounter: 1),
        "mirrored ordinary crouch jump applies");
    AssertEqual((byte)0x4c, crouchJumpLeft.Pose, "mirrored crouch jump transition pose");
    AssertEqual((ushort)35, crouchJumpLeft.YPosition, "mirrored crouch jump Y adjustment");

    // The native literal-pose comparison intentionally excludes aimed crouches. They use
    // the same `$4B` art and jump velocity but receive only the three-pixel floor-alignment
    // adjustment from pose-change collision.
    var aimedCrouchJump = new SamusState
    {
        Pose = SamusState.CrouchingAimDiagonalUpRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    aimedCrouchJump.RefreshCollisionRadii(bus);
    aimedCrouchJump.InitializeAnimation(bus);
    AssertTrue(
        aimedCrouchJump.TryApplyCrouchJumpTransition(
            bus, level, SamusState.NeutralJumpTransitionRightPose, nmiFrameCounter: 1),
        "aimed crouch jump applies");
    AssertEqual((ushort)45, aimedCrouchJump.YPosition, "aimed crouch jump omits FC8A offset");

    // Ceiling row one ends at pixel 31. A crouched body occupies 32..63 exactly, while a
    // standing body would need 27..63. Both five-pixel probes collide, so native pose
    // collision rejects the larger pose and retains crouch.
    var tunnelBlocks = (ushort[])floor.Clone();
    for (int x = 0; x < width; x++)
        tunnelBlocks[1 * width + x] = 0x8000;
    RoomLevelData tunnel = CreateRoom(
        width, height, tunnelBlocks, new byte[tunnelBlocks.Length]);
    var tunnelSamus = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    tunnelSamus.RefreshCollisionRadii(bus);
    tunnelSamus.InitializeAnimation(bus);
    AssertTrue(
        !tunnelSamus.TryApplyPostureTransition(
            bus, tunnel, SamusState.StandingTransitionRightPose, nmiFrameCounter: 0),
        "low tunnel rejects standing radius expansion");
    AssertEqual((byte)0x27, tunnelSamus.Pose, "rejected stand retains crouch pose");
    AssertEqual((ushort)48, tunnelSamus.YPosition, "rejected stand preserves center Y");

    var tunnelJump = new SamusState
    {
        Pose = SamusState.CrouchingAimUpRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    tunnelJump.RefreshCollisionRadii(bus);
    tunnelJump.InitializeAnimation(bus);
    AssertTrue(
        !tunnelJump.TryApplyCrouchJumpTransition(
            bus, tunnel, SamusState.NeutralJumpTransitionRightPose, nmiFrameCounter: 1),
        "low tunnel rejects crouch-jump radius expansion");
    AssertEqual((byte)0x27, tunnelJump.Pose, "boxed aimed jump falls back to ordinary crouch");
    AssertEqual((ushort)16, tunnelJump.Kinematics.YRadius, "boxed aimed jump retains crouch radius");
    AssertEqual((ushort)0, tunnelJump.Kinematics.YSpeed, "boxed aimed jump does not call Make_Samus_Jump");

    Console.WriteLine("  Samus posture: animated/direct exits, crouch jumps, radii, movement, FD targets, and low-ceiling fallback agree.");
}

/// <summary>
/// Verifies all six stationary aim poses against their literal pose-definition bytes,
/// movement-type-zero grounding, same-facing transitions, and no-controller fallbacks.
/// </summary>
}
