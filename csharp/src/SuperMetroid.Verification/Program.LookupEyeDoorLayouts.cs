using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEyeDoorLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyEyeDoorLayoutField(rom,0);
    private static void VerifyEyeDoorLayoutCollision(SuperMetroidAddressSpace rom) => VerifyEyeDoorLayoutField(rom,1);
    private static void VerifyEyeDoorLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyEyeDoorLayoutField(rom,2);

    private static void VerifyEyeDoorLayoutField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9bf7,0x9c03,0x9c0b,0x9c13,0x9c1b,0x9c23,0x9c2b,0x9c31,0x9c37,0x9c3d,0x9c43,0x9c49,
            0x9c4f,0x9c5b,0x9c63,0x9c6b,0x9c73,0x9c7b,0x9c83,0x9c89,0x9c8f,0x9c95,0x9c9b,0x9ca1];
        var exported = EyeDoorPlmDrawDefinitions.All.ToArray();
        AssertEqual(24,exported.Length,"Eye door original layout count");
        if (field == 0)
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                bool owned = pointers.Contains((ushort)raw);
                AssertEqual(owned,EyeDoorPlmDrawDefinitions.TryDescribe((ushort)raw,out var shape),"Eye door complete shape domain");
                AssertEqual(owned,EyeDoorPlmDrawDefinitions.TryGet((ushort)raw,out var dto),"Eye door complete DTO domain");
                if (!owned)
                {
                    AssertEqual(default(EyeDoorPlmDrawDefinitions.Draw),shape,"Eye door rejected shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList),dto,"Eye door rejected DTO");
                }
            }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            EyeDoorPlmDrawDefinitions.TryDescribe(pointer,out var shape);
            EyeDoorPlmDrawDefinitions.TryGet(pointer,out var direct);
            var views = new List<RoomPlmShotBlockDrawDefinitions.DrawList> {direct,exported[index]};
            if (pointer != 0x9bf7)
            {
                AssertTrue(EyeDoorPlmDrawDefinitions.TryGetByVisualId(EyeDoorPlmDrawDefinitions.VisualId(pointer),out var named),"Eye door reverse layout");
                views.Add(named);
            }
            ushort header = ReadSamusEaterPlmWord(rom,0x840000 | pointer);
            int count = header & 0x7fff;
            bool vertical = (header & 0x8000) != 0;
            if (field == 0)
            {
                AssertEqual(pointer,exported[index].Pointer,"Eye door original layout order");
                AssertEqual(count,shape.WordCount,"Eye door original cell count");
                AssertEqual(vertical,shape.Vertical,"Eye door original direction");
                AssertEqual((ushort)0,ReadSamusEaterPlmWord(rom,0x840000 | (pointer + 2 + count * 2)),"Eye door native terminator");
                foreach (int bad in new[] {int.MinValue,-1,count,int.MaxValue})
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(bad),"Eye door cell bounds");
            }
            foreach (var view in views)
            {
                AssertEqual(pointer,view.Pointer,"Eye door selected DTO pointer");
                AssertEqual(1,view.Runs.Length,"Eye door single run");
                var run = view.Runs.Span[0];
                if (field == 0)
                {
                    AssertEqual(header,run.DirectionAndCount,"Eye door DTO direction/count");
                    AssertEqual(count,run.LevelWords.Length,"Eye door DTO length");
                    AssertEqual(0,(int)run.NextX,"Eye door terminal X");
                    AssertEqual(0,(int)run.NextY,"Eye door terminal Y");
                }
                else
                    for (int cell = 0; cell < count; cell++)
                    {
                        int mask = field == 1 ? 0xf000 : 0xfff;
                        int expected = ReadSamusEaterPlmWord(rom,0x840000 | (pointer + 2 + cell * 2)) & mask;
                        AssertEqual(expected,shape.WordAt(cell) & mask,"Eye door native calculated field");
                        AssertEqual(expected,run.LevelWords.Span[cell] & mask,"Eye door native DTO field");
                    }
            }
            if (field == 0)
            {
                var level = CreateRoom(32,16,new ushort[512],new byte[512],blockDefinitions:new byte[0x400 * 8]);
                var plms = new RoomPlmSystem();
                var bus = new TestAddressSpace();
                bus.WriteBytes(0x8f9400,[0x0b,0xb7,3,4,0,0,0,0]);
                WriteWord(bus,0x84b70d,0xafb6);
                AssertEqual(1,plms.LoadRoomPopulation(bus,level,level.CreateBackgroundStreamer(),new SnesVram(),
                    SuperMetroid.AssetExtraction.RoomPlmPopulationImporter.Read(bus,0x9400),new Bank80SystemState(),
                    AreaId.Crateria,() => null,() => false),"Eye door draw dispatcher fixture");
                plms.DrawSolePlmFrameForVerification(bus, level, level.CreateBackgroundStreamer(), pointer);
                for (int cell = 0; cell < count; cell++)
                    AssertEqual(ReadSamusEaterPlmWord(rom,0x840000 | (pointer + 2 + cell * 2)),
                        level.GetCollisionBlock(3 + (vertical ? 0 : cell),4 + (vertical ? cell : 0)).LevelWord,
                        "Eye door production draw native word and position");
            }
            cells += count;
        }
        AssertEqual(40,cells,"Eye door complete native cell domain");
    }
}