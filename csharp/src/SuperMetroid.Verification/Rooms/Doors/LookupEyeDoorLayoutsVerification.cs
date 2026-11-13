using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks eye-door PLM geometry, collision, and visual words against all native draw lists.</summary>
internal static class LookupEyeDoorLayoutsVerification
{
    /// <summary>Checks exact pointer ownership, run direction, and physical placement for 24 layouts.</summary>
    internal static void VerifyEyeDoorLayoutGeometry(CartridgeImportAddressSpace rom) => VerifyEyeDoorLayoutField(rom,0);
    /// <summary>Checks the collision nibbles of all forty native eye-door cells.</summary>
    internal static void VerifyEyeDoorLayoutCollision(CartridgeImportAddressSpace rom) => VerifyEyeDoorLayoutField(rom,1);
    /// <summary>Checks metatile and flip bits through direct, exported, and named draw views.</summary>
    internal static void VerifyEyeDoorLayoutVisuals(CartridgeImportAddressSpace rom) => VerifyEyeDoorLayoutField(rom,2);

    /// <summary>Walks native eye-door lists independently and compares one selected field class with compiled descriptors.</summary>
    private static void VerifyEyeDoorLayoutField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9bf7,0x9c03,0x9c0b,0x9c13,0x9c1b,0x9c23,0x9c2b,0x9c31,0x9c37,0x9c3d,0x9c43,0x9c49,
            0x9c4f,0x9c5b,0x9c63,0x9c6b,0x9c73,0x9c7b,0x9c83,0x9c89,0x9c8f,0x9c95,0x9c9b,0x9ca1];
        var exported = RoomObjectEyeDoorVisualFiles.All.ToArray();
        AssertEqual(24,exported.Length,"Eye door original layout count");
        InstalledRoomObjectEyeDoorVisuals? pictures = field == 2 ? InstalledRoomObjectEyeDoorVisuals.Stock() : null;
        if (field == 0)
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                bool owned = pointers.Contains((ushort)raw);
                AssertEqual(owned,EyeDoorDraws.TryDescribe(NativeObjectDraws.At<EyeDoorDraws.Draw>((ushort)raw),out var shape),"Eye door complete shape domain");
                AssertEqual(owned,RoomObjectEyeDoorVisualFiles.TryGet(NativeObjectDraws.At<EyeDoorDraws.Draw>((ushort)raw),out var dto),"Eye door complete DTO domain");
                if (!owned)
                {
                    AssertEqual(default(EyeDoorDraws.Draw),shape,"Eye door rejected shape");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList),dto,"Eye door rejected DTO");
                }
            }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            EyeDoorDraws.TryDescribe(NativeObjectDraws.At<EyeDoorDraws.Draw>(pointer),out var shape);
            RoomObjectEyeDoorVisualFiles.TryGet(NativeObjectDraws.At<EyeDoorDraws.Draw>(pointer),out var direct);
            var views = new List<RoomObjectShotBlockDrawDefinitions.DrawList> {direct,exported[index]};
            if (pointer != 0x9bf7)
            {
                string id = RoomObjectEyeDoorVisualFiles.VisualId(NativeObjectDraws.At<EyeDoorDraws.Draw>(pointer));
                AssertTrue(RoomObjectEyeDoorVisualFiles.TryGet(RoomObjectEyeDoorVisualFiles.Family.Ids.Single(pair => pair.Value == id).Key,out var named),"Eye door reverse layout");
                views.Add(named);
            }
            ushort header = ReadSamusEaterObjectWord(rom,0x840000 | pointer);
            int count = header & 0x7fff;
            bool vertical = (header & 0x8000) != 0;
            if (field == 0)
            {
                AssertEqual(pointer,RoomObjectDrawAddressCodec.Of(exported[index].Draw),"Eye door original layout order");
                AssertEqual(count,shape.BlockCount,"Eye door original cell count");
                AssertEqual(vertical,shape.Vertical,"Eye door original direction");
                AssertEqual((ushort)0,ReadSamusEaterObjectWord(rom,0x840000 | (pointer + 2 + count * 2)),"Eye door native terminator");
                foreach (int bad in new[] {int.MinValue,-1,count,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(bad),"Eye door cell bounds");
            }
            foreach (var view in views)
            {
                AssertEqual(pointer,RoomObjectDrawAddressCodec.Of(view.Draw),"Eye door selected DTO pointer");
                AssertEqual(1,view.Runs.Length,"Eye door single run");
                var run = view.Runs.Span[0];
                if (field == 0)
                {
                    AssertRunHeader(header,run,"Eye door DTO direction/count");
                    AssertEqual(count,run.Count,"Eye door DTO length");
                    AssertEqual(0,(int)RoomObjectRunContinuationCodecConstants.X(view, 0),"Eye door terminal X");
                    AssertEqual(0,(int)RoomObjectRunContinuationCodecConstants.Y(view, 0),"Eye door terminal Y");
                }
                else
                    for (int cell = 0; cell < count; cell++)
                    {
                        RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom,0x840000 | (pointer + 2 + cell * 2)));
                        if (field == 1)
                        {
                            AssertEqual(expected.Collision,shape.CollisionAt(cell),"Eye door native calculated collision");
                            AssertEqual(expected.Collision,run.Collisions.Span[cell],"Eye door native DTO collision");
                        }
                        else
                            // Core holds no appearance: what the cell shows is the door's installed picture.
                            AssertEqual(expected.Appearance,pictures!.Image(view.Draw).At(run.CellAt(cell)),"Eye door installed picture shows the cartridge's block");

                    }
            }
            if (field == 0)
            {
                var level = CreateRoom(new(32, 16),new RoomTerrainBlock[512],new RoomBlockBehavior[512]);
                var roomObjects = RoomObjectFixtures.Unbound();
                var bus = new TestAddressSpace();
                SeedSoleElevatorPlatformObject(bus, 0x9400, new(3, 4), 0xafb6);
                AssertEqual(1,RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria), SuperMetroid.TestSupport.VerificationImporters.RoomObjectPopulationImporter.Read(bus,0x9400), new RoomObjectServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),"Eye door draw dispatcher fixture");
                roomObjects.DrawSoleObjectFrameForVerification(level, NativeObjectDraws.At<EyeDoorDraws.Draw>(pointer));
                for (int cell = 0; cell < count; cell++)
                    AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom,0x840000 | (pointer + 2 + cell * 2))),
                        level.GetCollisionBlock(new(checked((ushort)(3 + (vertical ? 0 : cell))), checked((ushort)(4 + (vertical ? cell : 0))))).Terrain,
                        "Eye door production draw native word and position");
            }
            cells += count;
        }
        AssertEqual(40,cells,"Eye door complete native cell domain");
    }
}
