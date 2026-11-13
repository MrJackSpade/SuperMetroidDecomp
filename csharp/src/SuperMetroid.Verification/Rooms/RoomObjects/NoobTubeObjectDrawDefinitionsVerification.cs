using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupNoobTubeStockVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupTubeDrawsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
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
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Noob Tube draw geometry and editable visual tiles through production PLM drawing.</summary>
internal static class NoobTubeObjectDrawDefinitionsVerification
{
    /// <summary>Compares seven physical draw lists and their guarded production draws.</summary>
    internal static void VerifyNoobTubeObjectDrawDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyTubeGeometry), () => VerifyTubeGeometry(rom));
        Suite(nameof(VerifyTubeCollision), () => VerifyTubeCollision(rom));
        Suite(nameof(VerifyTubeVisuals), () => VerifyTubeVisuals(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            RoomObjectNoobTubeVisualFiles.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomObjectShotBlockDrawDefinitions.DrawList cleared = lists.Single(draw => RoomObjectNoobTubeVisualFiles.VisualId(draw.Draw) == "cleared");
        AssertEqual(0x98e3, RoomObjectDrawAddressCodec.Of(cleared.Draw), "the cleared tube is the draw the cartridge stores at $98E3");
        AssertEqual(InstalledFramePictureAssertions.NativePicture(rom, 0x98e3, out _)[cleared.Runs.Span[2].Start],
            InstalledRoomObjectNoobTubeVisuals.Stock().Image(NativeObjectDraws.At<NoobTubeDraw>(0x98e3)).At(cleared.Runs.Span[2].Start),
            "stock n00b-tube catalog retains the native row tile");
        AssertEqual(7, lists.Length,
            "n00b-tube program selects seven distinct physical draw lists");
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList list in lists)
            VerifyNoobTubeNativeDrawPath(bank84, lists, list);
        Suite(nameof(VerifyNoobTubeVisualInstallation), () => VerifyNoobTubeVisualInstallation(rom));
        Console.WriteLine(
            "  N00b-tube PLM: seven guarded native layouts preserve physical blocks.");
    }

    /// <summary>Draws one native layout through the guarded PLM path and compares resulting blocks and streamed art.</summary>
    private static void VerifyNoobTubeNativeDrawPath(
        byte[] bank84,
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists,
        RoomObjectShotBlockDrawDefinitions.DrawList selected)
    {
        const int width = 16;
        const int height = 16;
        const int originX = 2;
        const int originY = 2;
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        SeedRoomObjectPopulation(bus, 0x9400, 
        [
            unchecked((byte)RoomObjectNativeWords.Word(RoomObjectHeaderId.NoobTube)),
            LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(RoomObjectHeaderId.NoobTube)),
            originX, originY, 0, 0, 0, 0,
        ]);
        // The retail control stream is compiled and immutable. Draw each physical layout through
        // the production timed-frame draw without patching that catalog.
        var guarded = NewNoobTubeDrawReadGuard(bus, lists);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new RoomTerrainBlock[width * height], new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Maridia), RoomObjectPopulationImporter.Read(guarded, 0x9400), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } } }),
            $"n00b tube loads for draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4}");
        roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<NoobTubeDraw>(RoomObjectDrawAddressCodec.Of(selected.Draw)));

        RoomObjectFrameImage picture = InstalledRoomObjectNoobTubeVisuals.Stock().Image(selected.Draw);
        var expected = new Dictionary<int, RoomTerrainBlock>();
        foreach (RoomObjectShotBlockDrawDefinitions.Run run in selected.Runs.Span)
        {
            int entryX = originX + run.Start.X;
            int entryY = originY + run.Start.Y;
            bool vertical = run.Direction == RoomObjectRunDirection.Vertical;
            for (int block = 0; block < run.Count; block++)
            {
                int x = entryX + (vertical ? 0 : block);
                int y = entryY + (vertical ? block : 0);
                // Each cell collides as the draw's geometry says and shows the tube's installed picture there.
                expected.Add(y * width + x, new RoomTerrainBlock(picture.At(run.CellAt(block)), run.Collisions.Span[block]));

            }
        }
        foreach ((int index, RoomTerrainBlock block) in expected)
            AssertEqual(block, level.GetCollisionBlockByIndex(index).Terrain,
                $"n00b-tube draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"n00b-tube draw ${RoomObjectDrawAddressCodec.Of(selected.Draw):X4} avoids source payload reads");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyNoobTubeVisualInstallation(CartridgeImportAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectNoobTubeVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectNoobTubeVisualFilesConstants.ValidateStock(
            installation.RoomObjectNoobTubeVisualDirectory);
        VerifyNoobTubeStockMapping(rom, installation.LoadRoomObjectNoobTubeVisuals());
        string stockPath = Path.Combine(
            installation.RoomObjectNoobTubeVisualDirectory,
            RoomObjectNoobTubeVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectNoobTubeVisuals(),
            "tampered tube stock fails loudly where it is read");
    }

    /// <summary>Rejects runtime reads of all seven compiled bank 84 draw payload ranges.</summary>
    private static CartridgeReadGuard NewNoobTubeDrawReadGuard(
        ISnesAddressSpace source, RoomObjectShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source).Forbid(address => lists.Any(list =>
            {
                int first = 0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw);
                int length = list.Runs.Span.Sum(run =>
                    4 + run.Count * 2);
                return address >= first && address < first + length;
            }),
            address => $"N00b tube reread draw payload ${address:X6}.");
}
