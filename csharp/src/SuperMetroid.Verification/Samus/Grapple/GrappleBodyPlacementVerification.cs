using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed swing frames select the native display frame while native grapple body offsets remain physical.</summary>
internal static class GrappleBodyPlacementVerification
{
    /// <summary>Compares extracted and installed swing art across mirrored angles, facing and anchor wrap.</summary>
    internal static void VerifyGrappleBodyPlacement(CartridgeImportAddressSpace rom)
    {
        var position = new Action<SamusState, SamusGrappleState>(SamusGrappleSwing.PositionSamusFromPendulum);
        var samus = PrepareRetailSamusFixture(new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()));
        byte[] json = GrappleSwingFrameExtractor.Extract(rom);
        var stock = GrappleSwingFrameFile.Load(new MemoryStream(json));
        foreach (bool installed in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var g = samus.Grapple;
            g.SwingFrames = installed ? InstalledFixtures.Projectiles.Grapple.SwingFrames : stock;
            samus.Pose = left ? SamusPoseId.GrappleSwingLeftPose : SamusPoseId.GrappleSwingRightPose;
            g.Anchor = g.Anchor.WithX((ushort)raw); g.Anchor = g.Anchor.WithY(unchecked((ushort)~raw));
            g.RopeLength = (ushort)(raw & 127);
            g.ValidateAnchorBlock = (raw & 1) != 0;
            // The physical rope uses Angle; the body mapping uses the remembered mirror.
            g.Angle = RotationAngle.FromRaw(unchecked((ushort)(raw ^ 0x4080)));
            g.MirroredAngle = RotationAngle.FromRaw((ushort)raw);
            byte nativeFrame = GrappleBodyPlacementRomByte(rom, 0x9bc1c2 + RotationAngle.FromRaw(unchecked((ushort)raw)).ToTrigonometryAngle().RawValue);
            int table = left ? 0x9bc2c2 : 0x9bc302;
            sbyte x = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2));
            sbyte y = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2 + 1));
            position(samus, g);
            AssertEqual(unchecked((ushort)(g.RopeStart.X + x)), samus.Position.X.Whole, "Swing art cannot move Grapple collision-body X");
            AssertEqual(unchecked((ushort)(g.RopeStart.Y + y)), samus.Position.Y.Whole, "Swing art cannot move Grapple collision-body Y");
            AssertEqual(nativeFrame, samus.AnimationFrame, "Native swing visual frame");
            AssertEqual(15, samus.AnimationFrameTimer.RawValue, "Native swing animation timer");
            AssertEqual(g.RopeStart.X, g.BeamStart.X, "Swing Flare follows physical Start X");
            AssertEqual(g.RopeStart.Y, g.BeamStart.Y, "Swing Flare follows physical Start Y");
        }
        foreach (string invalid in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1,\"frames\":[]}" })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameFile.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid))), "Invalid swing-frame JSON rejected");
        foreach (int invalid in new[] { -1, 32 })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameFile.Load(new MemoryStream(GrappleSwingFrameFile.Write(new() { Version = 1, Frames = Enumerable.Repeat(invalid, 256).ToArray() }))), "Invalid swing-frame index rejected");
        AssertThrows<InvalidDataException>(() => GrappleSwingFrameFile.Load(new MemoryStream(GrappleSwingFrameFile.Write(new() { Version = 1, Frames = new int[255] }))), "Incomplete swing-frame table rejected");
        Console.WriteLine("Grapple body placement: 262144 extracted/installed updates preserve native frames and physical offsets across mirror angle, facing and anchor wrapping.");
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
