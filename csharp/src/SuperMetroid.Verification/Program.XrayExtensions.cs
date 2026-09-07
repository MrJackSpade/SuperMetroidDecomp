using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyXrayExtensions()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        RoomLevelData Create() => new(8, 8, Enumerable.Repeat((ushort)0x8000, 64).ToArray(),
            new byte[64], new ushort[64], Array.Empty<byte>());
        void Set(RoomLevelData level, int index, RoomCollisionType type, byte bts)
        {
            level.SetPlmForegroundEntry(index, (ushort)((int)type << 12));
            level.SetPlmBehavior(index, bts);
        }
        var level = Create();
        Set(level, 18, RoomCollisionType.HorizontalExtension, 0xff);
        Set(level, 17, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual((ushort?)XrayRevealCodePointers.BlankMetatile, XrayRevealExtensions.Resolve(bus, level, 18),
            "initial negative horizontal BTS reaches scroll-trigger blank");
        Set(level, 17, RoomCollisionType.ShootableBlock, 0);
        AssertTrue(XrayRevealExtensions.Resolve(bus, level, 18) is null,
            "X-ray extension does not inherit a shot-block reveal like a collision extension");
        Set(level, 18, RoomCollisionType.VerticalExtension, 1);
        Set(level, 26, RoomCollisionType.HorizontalExtension, 1);
        Set(level, 34, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual((ushort?)XrayRevealCodePointers.BlankMetatile, XrayRevealExtensions.Resolve(bus, level, 18),
            "positive horizontal BTS reached vertically retains the vertical loop");
        Set(level, 26, RoomCollisionType.HorizontalExtension, 0xff);
        Set(level, 25, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual((ushort?)XrayRevealCodePointers.BlankMetatile, XrayRevealExtensions.Resolve(bus, level, 18),
            "negative horizontal BTS switches from Y to X");
        Set(level, 26, RoomCollisionType.VerticalExtension, 0xfe);
        Set(level, 10, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual((ushort?)XrayRevealCodePointers.BlankMetatile, XrayRevealExtensions.Resolve(bus, level, 18),
            "subsequent vertical BTS stays unsigned and hardware multiply uses low Y byte");
        Set(level, 0, RoomCollisionType.HorizontalExtension, 0xff);
        AssertEqual((ushort?)XrayRevealCodePointers.BlankMetatile, XrayRevealExtensions.Resolve(bus, level, 0),
            "negative coordinate exit emits native blank");
        Set(level, 0, RoomCollisionType.HorizontalExtension, 0);
        AssertTrue(XrayRevealExtensions.Resolve(bus, level, 0) is null, "zero BTS retains original art");
        Set(level, 0, RoomCollisionType.HorizontalExtension, 1);
        Set(level, 1, RoomCollisionType.HorizontalExtension, 0xff);
        bool cycleRejected = false;
        try { XrayRevealExtensions.Resolve(bus, level, 0); }
        catch (InvalidDataException e) when (e.Message.Contains("Cyclic")) { cycleRejected = true; }
        AssertTrue(cycleRejected, "cyclic authored extensions report an error instead of hanging");
        Console.WriteLine("  X-ray extensions: signed entry, asymmetric axis routing, byte multiplication, terminal filtering and cycle detection agree.");
    }
}
