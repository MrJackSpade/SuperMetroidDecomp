using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Ceres elevator arrival graphics indexing and projectile OAM against native definitions.</summary>
internal static class CeresElevatorArrivalVerification
{
    /// <summary>
    /// Verifies the easy-to-miss graphics-index write in the shared Ceres elevator
    /// initializer at <c>$86:A301</c>.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyCeresElevatorArrivalGraphicsIndex()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledDefinitions), () => VerifyCompiledDefinitions(rom));
        var memory = new TestAddressSpace();

        // Give both one-entry spritemaps conspicuous source attributes. With native
        // graphics index zero, AddEnemyProjectileSpritemap must preserve bytes $34/$A5.
        // A leaked enemy graphics word would add a base tile and OR another OBJ palette.
        var part = new SpriteVisualPart
        {
            OffsetX = 0, OffsetY = 0, TileColumn = 4, TileRow = 19,
            Size = 8, Priority = 2, Palette = 2, FlipX = false, FlipY = true,
        };
        var document = new EnemyProjectileSpritemapDocument
        {
            Version = (int)EnemyProjectileSpritemapVersion.Current,
            Frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(
                frame => frame.Name,
                frame => frame.Pointer is 0xb1ba or 0xb1d0 or 0x846d
                    ? new[] { part } : Array.Empty<SpriteVisualPart>()),
            ProgramFrames = EnemyProjectilePresentationFrameDefinitions.All.ToDictionary(
                frame => frame.Name, _ => Array.Empty<SpriteVisualPart>()),
        };
        var artwork = EnemyProjectileSpritemapCatalog.Load(new MemoryStream(
            EnemyProjectileSpritemapCatalog.Write(document)));
        var bus = NewCeresArrivalDefinitionReadGuard(memory);

        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0000)) };
        var arrival = new CeresElevatorArrivalState(bus, samus, artwork);
        arrival.Step(samus);

        var oam = new OamBuffer();
        oam.BeginFrame();
        arrival.Draw(oam, camera: Position16<RoomPixels>.Zero);

        VerificationAssert.AssertEqual(8, oam.NextByteOffset,
            "Ceres pad and level-data concealer each emitted one OBJ");
        VerificationAssert.AssertEqual(0x34, oam.LowTable[2],
            "Ceres moving pad retained its room-graphics tile byte");
        VerificationAssert.AssertEqual(0xa5, oam.LowTable[3],
            "Ceres moving pad retained its room-graphics attribute byte");
        VerificationAssert.AssertEqual(0x34, oam.LowTable[6],
            "Ceres level-data concealer retained its room-graphics tile byte");
        VerificationAssert.AssertEqual(0xa5, oam.LowTable[7],
            "Ceres level-data concealer retained its room-graphics attribute byte");

        int frames = 1;
        while (!arrival.IsComplete && frames++ < 200)
            arrival.Step(samus);
        VerificationAssert.AssertTrue(arrival.IsComplete,
            "Ceres arrival's compiled instruction programs delete both projectiles");
        VerificationAssert.AssertEqual(
            CeresElevatorArrivalDefinitions.LandingSamusY,
            samus.Position.Y.Whole,
            "Ceres arrival retains the native landing coordinate");

        Console.WriteLine(
            "  Ceres elevator: definitions/programs match cartridge data, run without bank-$86 reads, and retain native graphics/landing behavior.");
    }

    /// <summary>Matches compiled arrival projectiles and their instruction words to the retail bank-$86 source.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres elevator arrival definitions")]
    private static void VerifyCompiledDefinitions(ISnesAddressSpace rom)
    {

        foreach (CeresElevatorProjectileDefinition definition in new[]
        {
            CeresElevatorArrivalDefinitions.MovingPad,
            CeresElevatorArrivalDefinitions.StationaryPlatform,
        })
        {
            int address = CeresElevatorArrivalDefinitions.BankBase | definition.DefinitionPointer;
            VerificationAssert.AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), definition.InitialInstruction,
                "Ceres elevator compiled instruction-list identity");
        }

        ushort[] pointers = [0xa28b, 0xa28d, 0xa291, 0xa295, 0xa299, 0xa29d];
        foreach (ushort pointer in pointers)
        {
            CeresElevatorProjectileInstruction instruction =
                CeresElevatorArrivalDefinitions.ReadInstruction(pointer);
            int address = CeresElevatorArrivalDefinitions.BankBase | pointer;
            switch (instruction.Operation)
            {
                case CeresElevatorProjectileOperation.Frame:
                    VerificationAssert.AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), instruction.Duration,
                        "Ceres elevator compiled frame duration");
                    VerificationAssert.AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)), instruction.SpritemapPointer,
                        "Ceres elevator compiled spritemap identity");
                    VerificationAssert.AssertEqual(unchecked((ushort)(pointer + 4)), instruction.NextInstruction,
                        "Ceres elevator compiled frame continuation");
                    break;
                case CeresElevatorProjectileOperation.Delete:
                    VerificationAssert.AssertEqual(CeresElevatorArrivalDefinitions.DeleteOpcode, CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                        "Ceres elevator compiled delete opcode");
                    break;
                case CeresElevatorProjectileOperation.Goto:
                    VerificationAssert.AssertEqual(CeresElevatorArrivalDefinitions.GotoOpcode, CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                        "Ceres elevator compiled goto opcode");
                    VerificationAssert.AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)), instruction.NextInstruction,
                        "Ceres elevator compiled goto target");
                    break;
            }
        }
    }

    /// <summary>Forbids compiled Ceres arrival definition and instruction bytes during playback.</summary>
    private static CartridgeReadGuard NewCeresArrivalDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86a28b and < 0x86a2a1 or >= 0x86a387 and < 0x86a3a3,
                address => $"Ceres arrival read compiled bank-$86 byte ${address:X6}.");

    /// <summary>Runs the Ceres elevator arrival graphics-index and landing checks.</summary>
    [VerificationCommand("--ceres-elevator-arrival-definitions")]
    internal static int CeresElevatorArrivalDefinitionsCommand()
    {
        Suite(nameof(VerifyCeresElevatorArrivalGraphicsIndex), () => VerifyCeresElevatorArrivalGraphicsIndex());
        return 0;
    }
}
