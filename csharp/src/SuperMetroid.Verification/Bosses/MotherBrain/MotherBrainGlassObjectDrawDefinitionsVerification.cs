using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupGlassLayoutsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainGlassStockVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassObjectProgramVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World;

/// <summary>Checks compiled Mother Brain glass draw layouts through the guarded production PLM path.</summary>
internal static class MotherBrainGlassObjectDrawDefinitionsVerification
{
    /// <summary>Compares eleven physical draw lists with cartridge data and verifies guarded production draws write the collision blocks intact.</summary>
    internal static void VerifyMotherBrainGlassObjectDrawDefinitions(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyMotherBrainGlassObjectProgram), () => VerifyMotherBrainGlassObjectProgram(rom));
        Suite(nameof(VerifyGlassLayoutGeometry), () => VerifyGlassLayoutGeometry(rom));
        Suite(nameof(VerifyGlassLayoutCollision), () => VerifyGlassLayoutCollision(rom));

        RoomObjectFrameLayout[] lists =
            RoomObjectMotherBrainGlassVisualFiles.Family.Layouts.Values.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        AssertEqual(11, lists.Length,
            "Mother Brain glass program selects eleven distinct physical draw lists");
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        foreach (RoomObjectFrameLayout list in lists)
            VerifyMotherBrainGlassNativeDrawPath(rom, bank84, lists, list);
        Suite(nameof(VerifyMotherBrainGlassVisualInstallation), () => VerifyMotherBrainGlassVisualInstallation(rom));
        Console.WriteLine(
            "  Mother Brain glass PLM: 11 guarded native layouts preserve physical blocks.");
    }

    /// <summary>
    /// Draws one selected glass layout from its installed picture in a synthetic room while denying reads of its original
    /// cartridge payload, and requires the room to hold the terrain the cartridge's draw list writes.
    /// </summary>
    private static void VerifyMotherBrainGlassNativeDrawPath(
        CartridgeImportAddressSpace rom,
        byte[] bank84,
        RoomObjectFrameLayout[] lists,
        RoomObjectFrameLayout selected)
    {
        const int width = 32;
        const int height = 16;
        const int originX = 9;
        const int originY = 5;
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        SeedRoomObjectPopulation(bus, 0x9000,
        [
            unchecked((byte)RoomObjectNativeWords.Word(RoomObjectHeaderId.MotherBrainGlass)),
            LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(RoomObjectHeaderId.MotherBrainGlass)),
            originX, originY, 0x00, 0x80, 0x00, 0x00,
        ]);
        // The production control list is compiled and immutable. Draw each layout through the
        // production timed-frame draw instead of mutating a cartridge operand execution no longer reads.
        var guarded = NewMotherBrainGlassDrawReadGuard(bus, lists);
        var level = CreateEmptyRoom(new(width, height));
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Tourian), RoomObjectPopulationImporter.Read(guarded, 0x9000), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false, IsBossDefeated = () => false, IsMiniBossDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } } }),
            $"Mother Brain glass loads for draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4}");
        AssertTrue(roomObjects.MotherBrainGlassWasLoaded,
            $"glass header retains its PLM owner for draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4}");
        roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<MotherBrainGlassDraw>(RoomObjectDrawAddressCodec.Of(selected.Draw)));

        var expected = new Dictionary<int, RoomTerrainBlock>();
        foreach (KeyValuePair<Offset16<RoomBlocks>, RoomTerrainBlock> cell in
                 InstalledFramePictureAssertions.NativeTerrain(rom, RoomObjectDrawAddressCodec.Of(selected.Draw), out _))
            expected.Add((originY + cell.Key.Y) * width + originX + cell.Key.X, cell.Value);
        foreach ((int index, RoomTerrainBlock block) in expected)
            AssertEqual(block, level.GetCollisionBlockByIndex(index).Terrain,
                $"glass draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"glass draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4} avoids source payload reads");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyMotherBrainGlassVisualInstallation(
        CartridgeImportAddressSpace rom)
    {
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectMotherBrainGlassVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectMotherBrainGlassVisualFilesConstants.ValidateStock(
                installation.RoomObjectMotherBrainGlassVisualDirectory);
            VerifyMotherBrainGlassStockMapping(rom, installation.LoadRoomObjectMotherBrainGlassVisuals());
            string stockPath = Path.Combine(
                installation.RoomObjectMotherBrainGlassVisualDirectory,
                RoomObjectMotherBrainGlassVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomObjectMotherBrainGlassVisuals(),
                "tampered glass stock fails loudly where it is read");
        }
    }

    /// <summary>Blocks cartridge reads across every compiled glass draw list's run headers and tile words.</summary>
    private static CartridgeReadGuard NewMotherBrainGlassDrawReadGuard(
        ISnesAddressSpace source, RoomObjectFrameLayout[] lists) =>
        new CartridgeReadGuard(source).Forbid(address => lists.Any(list =>
            {
                int first = 0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw);
                int length = list.Runs.Span.Sum(run =>
                    4 + run.Count * 2);
                return address >= first && address < first + length;
            }),
            address => $"Mother Brain glass reread draw payload ${address:X6}.");

}
