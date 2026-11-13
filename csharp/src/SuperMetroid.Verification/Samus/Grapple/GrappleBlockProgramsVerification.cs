using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.ShotBlockObjectProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail and guarded-runtime checks for respawning/permanent grapple-block PLMs and their installed visuals.</summary>
internal static class GrappleBlockProgramsVerification
{
    /// <summary>Checks all nineteen compiled control addresses and values across the full 16-bit bank-local domain.</summary>
    private static void VerifyGrappleBlockControlMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] addresses = [0xcd6a,0xcd6e,0xcd71,0xcd75,0xcd79,0xcd7d,0xcd81,0xcd85,0xcd89,0xcd8d,0xcd8f,0xcd91,
            0xcda9,0xcdad,0xcdb0,0xcdb4,0xcdb8,0xcdbc,0xcdc0];
        AssertTrue(addresses.SequenceEqual(RoomObjectGrappleBlockProgramDefinitions.MechanicsWordAddresses()), "Grapple native control enumeration");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectGrappleBlockProgramDefinitions.TryReadMechanicsWord((ushort)address,out ushort value);
            AssertEqual(known.Contains((ushort)address),found,"Grapple complete control domain including callback gap");
            AssertEqual(found ? ReadRomWord(rom,0x840000 | address) : (ushort)0,value,"Grapple native timing, sound queue, BTS restoration and deletion");
        }
    }

    /// <summary>Checks the thirteen interleaved draw-pointer words while rejecting non-operand addresses.</summary>
    private static void VerifyGrappleBlockDrawOperandMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] addresses = [0xcd6c,0xcd73,0xcd77,0xcd7b,0xcd7f,0xcd83,0xcd87,0xcd8b,
            0xcdab,0xcdb2,0xcdb6,0xcdba,0xcdbe];
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectGrappleBlockProgramDefinitions.TryReadDrawPointerWord((ushort)address,out ushort value);
            AssertEqual(known.Contains((ushort)address),found,"Grapple complete draw operand domain");
            AssertEqual(found ? ReadRomWord(rom,0x840000 | address) : (ushort)0,value,"Grapple native initial, forward and reverse draw selection");
        }
    }

    /// <summary>Checks both packed sound operands and exact byte-address ownership.</summary>
    private static void VerifyGrappleBlockSoundMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] addresses = [0xcd70,0xcdaf];
        AssertTrue(addresses.SequenceEqual(RoomObjectGrappleBlockProgramDefinitions.MechanicsByteAddresses()), "Grapple native sound enumeration");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectGrappleBlockProgramDefinitions.TryReadMechanicsByte((ushort)address,out byte value);
            AssertEqual(addresses.Contains((ushort)address),found,"Grapple complete packed sound domain");
            AssertEqual(found ? GrappleBlockSoundMappingRomByte(rom, 0x840000 | address) : (byte)0,value,"Grapple native packed sound bytes");
        }
    }

    /// <summary>Reads a cartridge sound byte solely as the compiled-operand oracle.</summary>
    private static byte GrappleBlockSoundMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Compares five one-block draw lists with native geometry, collision words, and terminal offsets.</summary>
    private static void VerifyGrappleBlockPhysicalDrawMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] pointers = [0xa4f9,0xa4ff,0xa505,0xa50b,0xa511];
        var exported = RoomObjectGrappleBlockVisualFiles.All.ToArray();
        AssertTrue(pointers.SequenceEqual(exported.Select(draw => NativeObjectDraws.AddressOf(RoomObjectDraw.Of(draw.Frame)))), "Grapple native draw export order");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool found = GrappleBlockObjectDraws.TryGet(GrappleAt(pointer),out var draw);
            AssertEqual(pointers.Contains((ushort)pointer),found,"Grapple full native draw pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomObjectGrappleBlockDrawDefinitions.DrawList),draw,"Grapple missing draw cleared");
                continue;
            }
            AssertEqual((ushort)pointer,NativeObjectDraws.AddressOf(RoomObjectDraw.Of(draw.Frame)),"Grapple draw identity");
            RoomObjectFrameLayout layout = RoomObjectGrappleBlockVisualFiles.Family.Layouts[RoomObjectDraw.Of(draw.Frame)];
            AssertEqual(1, layout.Runs.Length, "Grapple frame is one run");
            AssertRunHeader(ReadRomWord(rom,0x840000 | pointer),layout.Runs.Span[0].Direction,layout.Runs.Span[0].Count,"Grapple native single-block geometry");
            // What the block shows is its installed picture, which the Grapple picture check compares with the cartridge.
            AssertEqual(RoomLevelWordCodec.Decode(ReadRomWord(rom,0x840000 | (pointer + 2))).Collision,draw.Collision,"Grapple native collision");
            AssertEqual(unchecked((sbyte)GrappleBlockPhysicalDrawMappingRomByte(rom, 0x840000 | (pointer + 4))),layout.NextStart(0).X,"Grapple native terminal X");
            AssertEqual(unchecked((sbyte)GrappleBlockPhysicalDrawMappingRomByte(rom, 0x840000 | (pointer + 5))),layout.NextStart(0).Y,"Grapple native terminal Y");
            AssertEqual(draw,exported.Single(entry => NativeObjectDraws.AddressOf(RoomObjectDraw.Of(entry.Frame)) == pointer),"Grapple exported draw matches calculation");
        }
    }

    /// <summary>The Grapple-block draw the cartridge stores at <paramref name="pointer"/>.</summary>
    private static RoomObjectDraw GrappleAt(int pointer) => NativeObjectDraws.At<GrappleBlockDraw>(pointer);

    /// <summary>The artwork identity of the Grapple-block draw the cartridge stores at <paramref name="pointer"/>.</summary>
    private static string GrappleId(int pointer) =>
        RoomObjectGrappleBlockVisualFiles.VisualId(RoomObjectDraw.FrameOf<GrappleBlockDraw>(GrappleAt(pointer), "Grapple-block"));

    /// <summary>Reads native draw-list terminal offsets as the physical-geometry oracle.</summary>
    private static byte GrappleBlockPhysicalDrawMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Compares the five frames' installed pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    private static void VerifyGrappleBlockStockVisualMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] pointers = [0xa4f9,0xa4ff,0xa505,0xa50b,0xa511];
        LookupEscapeGateStockFrame[] frames = [.. pointers.Select(pointer => new LookupEscapeGateStockFrame(pointer, GrappleId(pointer)))];
        InstalledRoomObjectGrappleBlockVisuals stock = InstalledRoomObjectGrappleBlockVisuals.Stock();
        AssertPicturedAsNative(rom, stock.Images, RoomObjectGrappleBlockVisualFiles.Family, frames, WrittenCells(RoomObjectGrappleBlockVisualFiles.Family));
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!pointers.Contains((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.Image(GrappleAt(pointer)),"Grapple stock rejects unsupported draw identity");
        AssertImportAndRejections(rom, stock.Images, RoomObjectGrappleBlockVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectGrappleBlockVisualDirectory);
    }

    /// <summary>Runs both BTS programs under read guards for 300 frames.</summary>
    internal static void VerifyGrappleBlockPrograms()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyGrappleBlockStockVisualMapping), () => VerifyGrappleBlockStockVisualMapping(rom));
        Suite(nameof(VerifyGrappleBlockControlMapping), () => VerifyGrappleBlockControlMapping(rom));
        Suite(nameof(VerifyGrappleBlockDrawOperandMapping), () => VerifyGrappleBlockDrawOperandMapping(rom));
        Suite(nameof(VerifyGrappleBlockSoundMapping), () => VerifyGrappleBlockSoundMapping(rom));
        var forbidden = new HashSet<int>();
        foreach (ushort address in RoomObjectGrappleBlockProgramDefinitions.MechanicsWordAddresses())
        {
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
        }
        foreach (ushort address in RoomObjectGrappleBlockProgramDefinitions.MechanicsByteAddresses())
            forbidden.Add(0x840000 | address);

        Suite(nameof(VerifyGrappleBlockPhysicalDrawMapping), () => VerifyGrappleBlockPhysicalDrawMapping(rom));
        for (int address = 0xa4f9; address < 0xa517; address++)
            forbidden.Add(0x840000 | address);

        for (byte bts = 1; bts <= 2; bts++)
        {
            GrappleBlockFixture native = NewGrappleBlockFixture(bts);
            GrappleBlockFixture compiled = NewGrappleBlockFixture(bts);
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 300; frame++)
            {
                native.RoomObjects.Step(native.Level);
                compiled.RoomObjects.Step(compiled.Level);
                AssertEqual(native.RoomObjects.SlotTable.ActiveCount, compiled.RoomObjects.SlotTable.ActiveCount,
                    $"Grapple BTS {bts} active count, frame {frame}");
                AssertEqual(native.RoomObjects.Sounds.SoundRequests.Count, compiled.RoomObjects.Sounds.SoundRequests.Count,
                    $"Grapple BTS {bts} sound count, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).Terrain,
                    compiled.Level.GetCollisionBlockByIndex(27).Terrain,
                    $"Grapple BTS {bts} level word, frame {frame}");
                AssertEqual(RoomBlockBehaviorCodec.Encode(native.Level.GetCollisionBlockByIndex(27).Behavior),
                    RoomBlockBehaviorCodec.Encode(compiled.Level.GetCollisionBlockByIndex(27).Behavior),
                    $"Grapple BTS {bts} BTS, frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"Grapple BTS {bts} never rereads compiled control bytes");
            AssertEqual(0, compiled.RoomObjects.SlotTable.ActiveCount,
                $"Grapple BTS {bts} finishes its cartridge timeline");
        }

        Suite(nameof(VerifyGrappleBlockVisualSeparation), () => VerifyGrappleBlockVisualSeparation());
        Suite(nameof(VerifyGrappleBlockVisualInstallation), () => VerifyGrappleBlockVisualInstallation());

        Console.WriteLine($"Grapple-block PLMs: 19 control words, 2 sound bytes, and 5 draw lists match ROM; both programs run with source reads forbidden.");
    }

    /// <summary>Confirms the stock first frame's Grapple collision word and immediate tile, and catalog rejection of missing draws.</summary>
    private static void VerifyGrappleBlockVisualSeparation(
)
    {
        // Run the first PLM draw and capture collision, immediate tile upload, and later camera-streamed tile output.
        GrappleBlockProgramsRenderResult Render(
            InstalledRoomObjectGrappleBlockVisuals visuals)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new RoomTerrainBlock[width * width];
            words[blockIndex] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.GrappleBlock);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new RoomBlockBehavior[words.Length], new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectGrappleBlockVisuals = visuals });
            AssertTrue(roomObjects.TrySpawnBreakableGrappleBlock(level, level.BlockAt(blockIndex), 1),
                "visual test starts the real respawning Grapple PLM");
            roomObjects.Step(level);
            return new GrappleBlockProgramsRenderResult(level.GetCollisionBlockByIndex(blockIndex).Terrain,
                level.DisplayedAppearance(blockIndex),
                level.DisplayedAppearance(blockIndex));
        }

        var stock = Render(InstalledRoomObjectGrappleBlockVisuals.Stock());
        AssertEqual(RoomTerrainBlock.Of(0x0b7, BlockMirroring.None, RoomCollisionType.GrappleBlock), stock.Physical,
            "native first Grapple frame installs Grapple collision");
        AssertEqual(RoomBlockAppearance.Of(0x0b7), stock.Immediate,
            "stock visual block reaches immediate tile upload");
    }

    /// <summary>Checks the extracted stock art and that a corrupt installed visual file fails loudly where it is read.</summary>
    private static void VerifyGrappleBlockVisualInstallation()
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectGrappleBlockVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectGrappleBlockVisualFilesConstants.ValidateStock(
            installation.RoomObjectGrappleBlockVisualDirectory);
        AssertEqual(RoomBlockAppearance.Of(0x0b7),
            installation.LoadRoomObjectGrappleBlockVisuals().Image(RoomObjectDraw.Of(GrappleBlockDraw.Intact)).At(Offset16<RoomBlocks>.Zero),
            "installed Grapple frame matches native stock art");

        string stockPath = Path.Combine(
            installation.RoomObjectGrappleBlockVisualDirectory,
            RoomObjectGrappleBlockVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "corrupt stock");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectGrappleBlockVisuals(),
            "corrupt installed Grapple visuals fail loudly where they are read");
    }

    /// <summary>Constructed room collision and PLM system for one grapple BTS program.</summary>
    internal sealed record GrappleBlockFixture(
        RoomLevelData Level, RoomObjectSystem RoomObjects);

    /// <summary>Spawns one grapple block at cell 27 in an eight-column synthetic room.</summary>
    private static GrappleBlockFixture NewGrappleBlockFixture(byte bts)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new RoomTerrainBlock[width * width];
        words[blockIndex] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.GrappleBlock);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new RoomBlockBehavior[words.Length], new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.TrySpawnBreakableGrappleBlock(level, level.BlockAt(blockIndex), bts),
            $"Grapple BTS {bts} installs its native program");
        return new GrappleBlockFixture(level, roomObjects);
    }

    /// <summary>Runs the grapple-block mechanics and visual checks from their command-line switch.</summary>
    [VerificationCommand("--grapple-block-programs")]
    internal static int GrappleBlockProgramsCommand()
    {
        Suite(nameof(VerifyGrappleBlockPrograms), () => VerifyGrappleBlockPrograms());
        return 0;
    }
}
