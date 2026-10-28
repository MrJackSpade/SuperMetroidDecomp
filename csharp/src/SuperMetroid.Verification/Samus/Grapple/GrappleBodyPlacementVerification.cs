using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that editable swing frames affect display while native grapple body offsets remain physical.</summary>
internal static class GrappleBodyPlacementVerification
{
    /// <summary>Compares installed and edited swing art across mirrored angles, facing, anchor wrap, and pendulum updates.</summary>
    internal static void VerifyGrappleBodyPlacement(SuperMetroidAddressSpace rom)
    {
        var position = new Action<SamusState, SamusGrappleState>(SamusGrappleSwing.PositionSamusFromPendulum);
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
            Version = 1, Frames = Enumerable.Range(0, 256).Select(angle => (stock.Resolve(new ByteAngle((byte)angle)) + 11) & 31).ToArray(),
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
            g.Angle = RotationAngle.FromRaw(unchecked((ushort)(raw ^ 0x4080)));
            g.MirroredAngle = RotationAngle.FromRaw((ushort)raw);
            byte nativeFrame = GrappleBodyPlacementRomByte(rom, 0x9bc1c2 + RotationAngle.FromRaw(unchecked((ushort)raw)).TableIndex);
            int table = left ? 0x9bc2c2 : 0x9bc302;
            sbyte x = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2));
            sbyte y = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2 + 1));
            position(samus, g);
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
            SamusGrappleMovement.ConnectUnobstructedSwing(bus, native, new Position16<RoomPixels>(512, 512), 32, RotationAngle.FromTableIndex(unchecked((byte)angle)), 128, faceRight);
            SamusGrappleMovement.ConnectUnobstructedSwing(bus, changed, new Position16<RoomPixels>(512, 512), 32, RotationAngle.FromTableIndex(unchecked((byte)angle)), 128, faceRight);
            for (int tick = 0; tick < 16; tick++)
            {
                ControllerButton input = (SuperMetroid.Core.Input.ControllerButton.X | (tick < 8 ? SuperMetroid.Core.Input.ControllerButton.Left : SuperMetroid.Core.Input.ControllerButton.Right));
                AssertEqual(SamusGrappleMovement.Step(new SamusTerrain { Level = empty }, native, input, 0), SamusGrappleMovement.Step(new SamusTerrain { Level = empty }, changed, input, 0), "Edited swing frames preserve actual pendulum results");
                AssertEqual((native.AnimationFrame + 11) & 31, changed.AnimationFrame, "Edited frame reaches actual pendulum updater");
                AssertEqual(native.AnimationFrameTimer.RawValue, changed.AnimationFrameTimer.RawValue, "Visual mapping cannot change swing timing");
                int displayed = changed.AnimationFrame;
                SamusPresentation.SetGrappleSwingAnimationFrame(changed, native.AnimationFrame);
                AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(changed)), "Entire swing simulation state matches except displayed frame");
                SamusPresentation.SetGrappleSwingAnimationFrame(changed, displayed);
            }
        }
        Console.WriteLine("Grapple body placement: 524288 extracted/installed updates preserve physical offsets independently of edited JSON frames, mirror angle, facing and anchor wrapping; installed selector ROM reads forbidden.");
    }

    /// <summary>Reads native frame and body-offset tables for the independent placement oracle.</summary>
    private static byte GrappleBodyPlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the grapple placement comparison with the retail cartridge image.</summary>
    [VerificationCommand("--grapple-swing-frames")]
    internal static int GrappleSwingFramesCommand()
    {
        Suite(nameof(VerifyGrappleBodyPlacement), () => VerifyGrappleBodyPlacement(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
