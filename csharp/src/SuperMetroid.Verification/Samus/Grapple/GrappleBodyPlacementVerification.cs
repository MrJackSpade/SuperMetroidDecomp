using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
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
        var samus = (new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
        byte[] json = GrappleSwingFrameExtractor.Extract(rom);
        var stock = GrappleSwingFrameFile.Load(new MemoryStream(json));
        foreach (bool installed in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var g = samus.Grapple;
            samus.Artwork.GrappleSwingFrames = installed ? InstalledFixtures.Projectiles.Grapple.SwingFrames : stock;
            samus.ChangePoseKeepingAnimation(left ? SamusPoseId.GrappleSwingLeftPose : SamusPoseId.GrappleSwingRightPose);
            // Anchors range over four screens on each axis, at least a rope and a body inside the room's top and left edges,
            // as a swing's are: its anchor and the body line it sweeps lie in the room.
            g.Anchor = g.Anchor.WithX(AnchorMargin + (raw & AnchorSpread)); g.Anchor = g.Anchor.WithY(AnchorMargin + (~raw & AnchorSpread));
            g.RopeLength = (ushort)(raw & 127);
            g.ValidatedAnchor = (raw & 1) != 0 ? GrappleAnchorKind.Block : null;
            // The physical rope uses Angle; the body mapping uses the remembered mirror.
            g.Angle = RotationAngleConstants.FromRaw(unchecked((ushort)(raw ^ 0x4080)));
            g.MirroredAngle = RotationAngleConstants.FromRaw((ushort)raw);
            byte nativeFrame = GrappleBodyPlacementRomByte(rom, 0x9bc1c2 + RotationAngleConstants.FromRaw(unchecked((ushort)raw)).ToTrigonometryAngle().RawValue);
            int table = left ? 0x9bc2c2 : 0x9bc302;
            sbyte x = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2));
            sbyte y = unchecked((sbyte)GrappleBodyPlacementRomByte(rom, table + nativeFrame * 2 + 1));
            position(samus, g);
            AssertEqual(unchecked((ushort)(g.RopeStart.X + x)), samus.Position.X.Whole, "Swing art cannot move Grapple collision-body X");
            AssertEqual(unchecked((ushort)(g.RopeStart.Y + y)), samus.Position.Y.Whole, "Swing art cannot move Grapple collision-body Y");
            AssertEqual(nativeFrame, samus.AnimationNumber, "Native swing visual frame");
            AssertEqual(15, samus.AnimationFrameTimer.RawValue, "Native swing animation timer");
            AssertEqual(g.RopeStart.X, g.FlareOrigin.X, "Swing Flare follows physical Start X");
            AssertEqual(g.RopeStart.Y, g.FlareOrigin.Y, "Swing Flare follows physical Start Y");
        }
        foreach (string invalid in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1,\"frames\":[]}" })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameFile.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid))), "Invalid swing-frame JSON rejected");
        foreach (int invalid in new[] { -1, 32 })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameFile.Load(new MemoryStream(GrappleSwingFrameFile.Write(new() { Version = GrappleSwingFrameFile.Version, Frames = Enumerable.Repeat(invalid, GrappleSwingDirection.Count).ToArray() }))), "Invalid swing-frame index rejected");
        AssertThrows<InvalidDataException>(() => GrappleSwingFrameFile.Load(new MemoryStream(GrappleSwingFrameFile.Write(new() { Version = GrappleSwingFrameFile.Version, Frames = new int[GrappleSwingDirection.Count - 1] }))), "Incomplete swing-frame table rejected");
        Console.WriteLine("Grapple body placement: 262144 extracted/installed updates preserve native frames and physical offsets across mirror angle, facing and anchors across the room.");
    }

    /// <summary>The nearest an anchor lies to the room's top or left edge: past the longest rope swept here and the body's farthest offset.</summary>
    private const int AnchorMargin = 256;

    /// <summary>The spread of anchors beyond the margin, as a mask of the swept word.</summary>
    private const int AnchorSpread = 1023;

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
