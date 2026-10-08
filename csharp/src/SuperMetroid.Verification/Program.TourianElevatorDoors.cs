using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyTourianElevatorDoors()
    {
        // Reports #1181-1184: real Tourian list/BTS values at the production collision seam.
        foreach (byte pose in new byte[] { 1, 2, 9, 10, 0x25, 0x26 })
        {
            CheckCollision(0xdad5, 2, pose, horizontal: false, elevator: true);
            CheckCollision(0xdad5, 3, pose, horizontal: true, elevator: false);
        }
        // The same omitted native sentinel also terminates the Maridia elevator list.
        CheckCollision(0xd332, 3, 1, horizontal: false, elevator: true);

        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort listPointer in new ushort[] { 0xdad5, 0xd332 })
        {
            AssertEqual(4, DoorDefinitionsTooling.GetList(listPointer).DoorPointers.Length,
                "native elevator list has four entries");
            for (byte index = 0; index < 4; index++)
            {
                ushort pointer = ReadVerificationWord(bus, 0x8f0000 | (listPointer + index * 2));
                var native = CartridgeDoorHeaderImporter.Load(bus, pointer);
                AssertEqual(native, DoorDefinitions.Resolve(listPointer, index), "native door record");
                AssertEqual(native, DoorDefinitions.Resolve(listPointer, (byte)(index | 0x80)),
                    "native high-bit BTS mask");
            }
            AssertThrows<InvalidDataException>(() => DoorDefinitions.Resolve(listPointer, 4),
                "genuinely invalid BTS still rejected");
        }
        Console.WriteLine("Tourian/Maridia elevator doors: native four-entry lists and headers match; elevator clipping/pose gating and save-door transition pass without cartridge reads.");

        static void CheckCollision(ushort list, byte bts, byte pose, bool horizontal, bool elevator)
        {
            ushort[] blocks = new ushort[16];
            byte[] behaviors = new byte[16];
            int target = horizontal ? 6 : 9;
            blocks[target] = 0x9000;
            behaviors[target] = bts;
            var level = new RoomLevelData(4, 4, blocks, behaviors, new ushort[16], [], doorListPointer: list);
            var state = new SamusKinematicsState
            {
                XPosition = 24, YPosition = 24, XRadius = 7, YRadius = 7, CollisionPose = pose,
            };
            var bus = new DoorNoReadAddressSpace();
            BlockMoveResult result = horizontal
                ? SamusBlockCollision.MoveHorizontal(bus, level, state, 4 << 16)
                : SamusBlockCollision.MoveVertical(bus, level, state, 4 << 16, scanLeftToRight: true);
            AssertEqual(elevator, result.Collided, "native solid elevator / passable physical door");
            AssertEqual(elevator && pose < 9, level.ConsumeElevatorDoorContact(), "native elevator pose gating");
            if (elevator)
                AssertTrue(level.PendingDoorTransition is null, "sentinel must not start a room transition");
            else
                AssertEqual((ushort)0xa99c, level.PendingDoorTransition!.Pointer, "correct Tourian save-room door");
        }
    }
}
