using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using static GraphDigestVerification;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

internal static class GrappleBodyPlacementVerification
{
    internal static void VerifyGrappleBodyPlacement(SuperMetroidAddressSpace rom)
    {
        var position = new Action<ISnesAddressSpace, SamusState, SamusGrappleState>(SamusGrappleMovement.PositionSamusFromPendulum);
        CartridgeReadGuard bus = new CartridgeReadGuard(rom)
            .Forbid(address => address is >= 0x9bc1c2 and < 0x9bc2c2,
                _ => "Installed swing art still reads the ROM selector.")
            .Forbid(address => address is >= 0x9bc2c2 and < 0x9bc342,
                address => $"Compiled Grapple body offset read ROM ${address:X6}.");
        var samus = PrepareRetailSamusFixture(new SamusState());
        byte[] json = GrappleSwingFrameExtractor.Extract(rom);
        var stock = GrappleSwingFrameCatalog.Load(new MemoryStream(json));
        var edited = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameCatalog.Write(new()
        {
            Version = 1, Frames = Enumerable.Range(0, 256).Select(angle => (stock.Resolve((byte)angle) + 11) & 31).ToArray(),
        })));
        foreach (bool installed in new[] { false, true })
        foreach (bool replaceArt in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var g = samus.Grapple;
            g.SwingFrames = replaceArt ? edited : installed ? InstalledFixtures.Projectiles.GrappleTiles.SwingFrames : stock;
            samus.Pose = left ? SamusPoseId.GrappleSwingLeftPose : SamusPoseId.GrappleSwingRightPose;
            g.Anchor = g.Anchor.WithX((ushort)raw); g.Anchor = g.Anchor.WithY(unchecked((ushort)~raw));
            g.RopeLength = (ushort)(raw & 127);
            g.ValidateAnchorBlock = (raw & 1) != 0;
            // The physical rope uses Angle; the body mapping uses the remembered mirror.
            g.Angle = SnesAngle.FromRaw(unchecked((ushort)(raw ^ 0x4080)));
            g.MirroredAngle = SnesAngle.FromRaw((ushort)raw);
            byte nativeFrame = GrappleBodyPlacementRomByte(rom, 0x9bc1c2 + SnesAngle.FromRaw(unchecked((ushort)raw)).TableIndex);
            int table = left ? 0x9bc2c2 : 0x9bc302;
            sbyte x = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2));
            sbyte y = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2 + 1));
            position(bus, samus, g);
            AssertEqual(unchecked((ushort)(g.RopeStart.X + x)), samus.Position.X.Whole, "Art override cannot move Grapple collision-body X");
            AssertEqual(unchecked((ushort)(g.RopeStart.Y + y)), samus.Position.Y.Whole, "Art override cannot move Grapple collision-body Y");
            AssertEqual(replaceArt ? (nativeFrame + 11) & 31 : nativeFrame, samus.AnimationFrame, "Visual frame override still applies");
            AssertEqual(15, samus.AnimationFrameTimer.RawValue, "Native swing animation timer");
            AssertEqual(g.RopeStart.X, g.BeamStart.X, "Swing Flare follows physical Start X");
            AssertEqual(g.RopeStart.Y, g.BeamStart.Y, "Swing Flare follows physical Start Y");
        }
        foreach (string invalid in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1,\"frames\":[]}" })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid))), "Invalid swing-frame JSON rejected");
        foreach (int invalid in new[] { -1, 32 })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameCatalog.Write(new() { Version = 1, Frames = Enumerable.Repeat(invalid, 256).ToArray() }), "Invalid swing-frame index rejected");
        AssertThrows<InvalidDataException>(() => GrappleSwingFrameCatalog.Write(new() { Version = 1, Frames = new int[255] }), "Incomplete swing-frame table rejected");
        var empty = CreateRoom(new(64, 64), new ushort[4096], new byte[4096]);
        foreach (bool faceRight in new[] { false, true })
        for (int angle = 0; angle < 256; angle += 32)
        {
            var native = PrepareRetailSamusFixture(new SamusState()); var changed = PrepareRetailSamusFixture(new SamusState());
            native.Grapple.SwingFrames = stock; changed.Grapple.SwingFrames = edited;
            SamusGrappleMovement.ConnectUnobstructedSwing(bus, native, new Position16<RoomPixels>(512, 512), 32, SnesAngle.FromTableIndex(unchecked((byte)angle)), 128, faceRight);
            SamusGrappleMovement.ConnectUnobstructedSwing(bus, changed, new Position16<RoomPixels>(512, 512), 32, SnesAngle.FromTableIndex(unchecked((byte)angle)), 128, faceRight);
            for (int tick = 0; tick < 16; tick++)
            {
                ushort input = (ushort)(SuperMetroid.Core.Input.SnesButton.X | (tick < 8 ? SuperMetroid.Core.Input.SnesButton.Left : SuperMetroid.Core.Input.SnesButton.Right));
                AssertEqual(SamusGrappleMovement.Step(new SamusTerrain { Bus = bus, Level = empty }, native, input, 0), SamusGrappleMovement.Step(new SamusTerrain { Bus = bus, Level = empty }, changed, input, 0), "Edited swing frames preserve actual pendulum results");
                AssertEqual((native.AnimationFrame + 11) & 31, changed.AnimationFrame, "Edited frame reaches actual pendulum updater");
                AssertEqual(native.AnimationFrameTimer.RawValue, changed.AnimationFrameTimer.RawValue, "Visual mapping cannot change swing timing");
                ushort displayed = changed.AnimationFrame;
                changed.Presentation.SetGrappleSwingAnimationFrame(native.AnimationFrame);
                AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(changed)), "Entire swing simulation state matches except displayed frame");
                changed.Presentation.SetGrappleSwingAnimationFrame(displayed);
            }
        }
        Console.WriteLine("Grapple body placement: 524288 extracted/installed updates preserve physical offsets independently of edited JSON frames, mirror angle, facing and anchor wrapping; installed selector ROM reads forbidden.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple body placement")]
    private static byte GrappleBodyPlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    [VerificationCommand("--grapple-swing-frames")]
    internal static int GrappleSwingFramesCommand()
    {
        Suite(nameof(VerifyGrappleBodyPlacement), () => VerifyGrappleBodyPlacement(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
