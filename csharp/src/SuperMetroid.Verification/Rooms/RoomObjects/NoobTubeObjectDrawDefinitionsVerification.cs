using SuperMetroid.Core;
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
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;

/// <summary>Checks Noob Tube draw geometry and editable visual tiles through production PLM drawing.</summary>
internal static class NoobTubeObjectDrawDefinitionsVerification
{
    /// <summary>Compares the five physical draw lists and their guarded production draws.</summary>
    internal static void VerifyNoobTubeObjectDrawDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyTubeGeometry), () => VerifyTubeGeometry(rom));
        Suite(nameof(VerifyTubeCollision), () => VerifyTubeCollision(rom));
        Suite(nameof(VerifyTubeVisuals), () => VerifyTubeVisuals(rom));

        RoomObjectDrawLists.DrawList[] lists =
            RoomObjectNoobTubeVisualFiles.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomObjectDrawLists.DrawList upperRows = lists.Single(draw => RoomObjectNoobTubeVisualFiles.VisualId(draw.Draw) == "upper-rows-open");
        AssertEqual(0x9991, RoomObjectDrawAddressCodec.Of(upperRows.Draw), "the tube's opened upper rows are the draw the cartridge stores at $9991");
        AssertEqual(InstalledFramePictureAssertions.NativePicture(rom, 0x9991, out _)[upperRows.Runs.Span[2].Start],
            InstalledRoomObjectNoobTubeVisuals.Stock().Image(NativeObjectDraws.At<NoobTubeDraw>(0x9991)).At(upperRows.Runs.Span[2].Start),
            "stock n00b-tube catalog retains the native row tile");
        AssertEqual(5, lists.Length,
            "the n00b tube shows five distinct physical draw lists");
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        foreach (RoomObjectDrawLists.DrawList list in lists)
            VerifyNoobTubeNativeDrawPath(bank84, lists, list);
        Suite(nameof(VerifyNoobTubeVisualInstallation), () => VerifyNoobTubeVisualInstallation(rom));
        Console.WriteLine(
            "  N00b-tube PLM: five guarded native layouts preserve physical blocks.");
    }

    /// <summary>Draws one native layout through the guarded PLM path and compares resulting blocks and streamed art.</summary>
    private static void VerifyNoobTubeNativeDrawPath(
        byte[] bank84,
        RoomObjectDrawLists.DrawList[] lists,
        RoomObjectDrawLists.DrawList selected)
    {
        const int width = 16;
        const int height = 16;
        const int originX = 2;
        const int originY = 2;
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        SeedRoomObjectPopulation(bus, 0x9400, 
        [
            unchecked((byte)RoomObjectNativeWords.Word(RoomObjectKind.NoobTube)),
            LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(RoomObjectKind.NoobTube)),
            originX, originY, 0, 0, 0, 0,
        ]);
        // The retail control stream is compiled and immutable. Draw each physical layout through
        // the production timed-frame draw without patching that catalog.
        var guarded = NewNoobTubeDrawReadGuard(bus, lists);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new RoomTerrainBlock[width * height], new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid(), []);
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Maridia), RoomObjectPopulationImporter.Read(guarded, 0x9400), RoomObjectServiceFixtures.Uninstalled with { Samus = new FixtureRoomSamus(() => new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } } });
        roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<NoobTubeDraw>(RoomObjectDrawAddressCodec.Of(selected.Draw)));

        RoomObjectFrameImage picture = InstalledRoomObjectNoobTubeVisuals.Stock().Image(selected.Draw);
        var expected = new Dictionary<int, RoomTerrainBlock>();
        foreach (RoomObjectDrawLists.Run run in selected.Runs.Span)
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

    /// <summary>Rejects runtime reads of the five compiled bank 84 draw payload ranges.</summary>
    private static CartridgeReadGuard NewNoobTubeDrawReadGuard(
        ISnesAddressSpace source, RoomObjectDrawLists.DrawList[] lists) =>
        new CartridgeReadGuard(source).Forbid(address => lists.Any(list =>
            {
                int first = 0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw);
                int length = list.Runs.Span.Sum(run =>
                    4 + run.Count * 2);
                return address >= first && address < first + length;
            }),
            address => $"N00b tube reread draw payload ${address:X6}.");
}
