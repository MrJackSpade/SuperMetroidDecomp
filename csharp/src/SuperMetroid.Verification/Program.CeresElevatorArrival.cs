using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies the easy-to-miss graphics-index write in the shared Ceres elevator
    /// initializer at <c>$86:A301</c>.
    /// </summary>
    static void VerifyCeresElevatorArrivalGraphicsIndex()
    {
        var bus = new TestAddressSpace();

        // Only the definition fields consumed by the focused translator are necessary.
        // Values are the retail words at $86:A387 and $86:A395 respectively.
        bus.WriteBytes(0x86a387,
        [
            0xee, 0xa2, 0x28, 0xa3, 0x8d, 0xa2, 0x01, 0x01, 0x00, 0x30,
        ]);
        bus.WriteBytes(0x86a395,
        [
            0x1b, 0xa3, 0x64, 0xa3, 0x99, 0xa2, 0x01, 0x01, 0x00, 0x30,
        ]);

        // One duration/spritemap pair is enough to make each object drawable. The real
        // first-frame pointers are retained so this fixture tests the same dispatch path
        // as the cartridge-backed arrival rather than a host-invented instruction list.
        bus.WriteBytes(0x86a28d, [0x01, 0x00, 0xba, 0xb1]);
        bus.WriteBytes(0x86a299, [0x01, 0x00, 0x6d, 0x84]);

        // Give both one-entry spritemaps conspicuous source attributes. With native
        // graphics index zero, AddEnemyProjectileSpritemap must preserve bytes $34/$A5.
        // A leaked enemy graphics word would add a base tile and OR another OBJ palette.
        byte[] spritemap =
        [
            0x01, 0x00, // One component.
            0x00, 0x00, // X offset zero, small OBJ.
            0x00,       // Y offset zero.
            0x34, 0xa5, // Complete ROM-authored tile/attribute word.
        ];
        bus.WriteBytes(0x8db1ba, spritemap);
        bus.WriteBytes(0x8d846d, spritemap);

        var samus = new SamusState { XPosition = 0x0080, YPosition = 0x0000 };
        var arrival = new CeresElevatorArrivalState(bus, samus);
        arrival.Step(samus);

        var oam = new OamBuffer();
        oam.BeginFrame();
        arrival.Draw(oam, cameraX: 0, cameraY: 0);

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

        Console.WriteLine(
            "  Ceres elevator: shared initializer clears transient enemy tile/palette bits.");
    }
}
