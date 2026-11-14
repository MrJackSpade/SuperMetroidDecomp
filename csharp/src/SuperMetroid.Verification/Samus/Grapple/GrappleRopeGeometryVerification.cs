using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Grapple.GrappleEndpointGeometryVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Grapple rope's chain of pictures against native angle tables, and its one animation phase.</summary>
internal static class GrappleRopeGeometryVerification
{
    /// <summary>The first rope character on the grapple's page: the rope's cycle of pictures follows it.</summary>
    private const int FirstRopeCharacter = 0x21;

    /// <summary>
    /// Compares real drawing with the native fractional accumulator across directions, origins, and rope lengths, and
    /// checks each segment's picture follows the rope's one animation phase.
    /// </summary>
    internal static void VerifyGrappleRopeGeometry(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyGrappleEndpointGeometry), () => VerifyGrappleEndpointGeometry());
        Suite(nameof(VerifyGrappleRopeAnimation), VerifyGrappleRopeAnimation);
        // Exercise the actual drawing entry point on a freshly fired rope. Expected positions emulate the native
        // overlapping-word accumulator, using cartridge samples independently of compiled tables.
        int cases = 0;
        foreach (int direction in new[] { 1, -1 })
        foreach (int origin in new[] { 128, 4, 252 })
        foreach (int length in new[] { 120, 1, 7, 8, 16, 128, 136 })
        {
            var grapple = new SamusGrappleState
            {
                Phase = GrapplePhase.Firing, RopeLength = length,
                BeamStart = new((ushort)origin, 128),
                Anchor = new((ushort)(origin + direction * 100), 28),
                PointAnimationTimer = WrappingCountdown.Start(5),
            };
            // Equal-magnitude up/right and up/left vectors select $20/$E0.
            int angle = direction > 0 ? 32 : 224;
            int dx = (short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa0b443 + angle * 2) * 2048;
            int dy = (short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa0b3c3 + angle * 2) * 2048;
            int x = unchecked(SixteenSixteen.FromWhole(origin - 4)), y = 124 << 16;
            // The rope is a chain of one picture per eight pixels of its length, at least one, each published wherever it
            // lies for the display to clip (#1587); a rope past sixteen segments keeps going (#1588). The OAM a segment
            // stands for keeps its position's low byte on each axis.
            var expected = new List<GrappleRopeGeometryExpected>();
            int count = Math.Max(1, length / 8);
            for (int segment = 0; segment < count; segment++)
            {
                ushort sx = (ushort)SixteenSixteenWords.WholeOf(unchecked((uint)x)), sy = (ushort)SixteenSixteenWords.WholeOf(unchecked((uint)y));
                expected.Add(new GrappleRopeGeometryExpected(unchecked((byte)sx), unchecked((byte)sy)));
                x = unchecked(x + dx); y = unchecked(y + dy);
            }
            var oamFrame = new SpriteFrameBuffer();
            SamusGrappleBeamDrawing.DrawConnectedBeam(grapple, oamFrame, new(0, 0), InstalledFixtures.GrappleSheet);
            OamBuffer oam = oamFrame.FinalizeAsOam();
            string context = $"direction {direction}, origin {origin}, length {length}";
            AssertEqual((expected.Count + 1) * 4, oam.LastFinalizedSpriteCount * 4, "Every rope segment and the endpoint are published: " + context);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i].X, oam.LowTable[i * 4], "Native fractional rope X: " + context);
                AssertEqual(expected[i].Y, oam.LowTable[i * 4 + 1], "Native fractional rope Y: " + context);
                // A fresh rope shows its opening pattern the whole way along: the cycle's last picture at the muzzle,
                // each segment beyond the one before.
                AssertEqual((byte)(FirstRopeCharacter + (3 - i % 4)), oam.LowTable[i * 4 + 2], "Rope picture follows the opening pattern: " + context);
            }
            AssertEqual(GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay - 1, grapple.RopeAnimationTimer,
                "The rope's one animation counts the draw: " + context);
            cases++;
        }
        Console.WriteLine($"Grapple rope geometry: {cases} diagonal, viewport-edge, short and long cases publish the whole chain at its native positions in its opening pattern.");
    }

    /// <summary>
    /// Draws a fresh rope repeatedly, lengthening it partway, and checks every segment, those that appear later
    /// included, shows the picture of the rope's one phase: the first step after six draws and one every five after.
    /// </summary>
    private static void VerifyGrappleRopeAnimation()
    {
        var grapple = new SamusGrappleState
        {
            Phase = GrapplePhase.ConnectedLocked, BeamStart = new(128, 128), Anchor = new(128, 28),
            PointAnimationTimer = WrappingCountdown.Start(5),
        };
        for (int draw = 1; draw <= 24; draw++)
        {
            grapple.RopeLength = draw < 8 ? 8 : 40;
            OamBuffer oam = SpriteFrameBuffer.DrawnOam(sprites =>
                SamusGrappleBeamDrawing.DrawConnectedBeam(grapple, sprites, new(0, 0), InstalledFixtures.GrappleSheet));
            int steps = draw < 6 ? 0 : 1 + (draw - 6) / 5;
            int segments = grapple.RopeLength / 8;
            AssertEqual((segments + 1) * 4, oam.LastFinalizedSpriteCount * 4, $"Rope of {segments} segments and endpoint drawn on draw {draw}");
            for (int segment = 0; segment < segments; segment++)
                AssertEqual((byte)(FirstRopeCharacter + (steps + 3 - segment % 4) % 4), oam.LowTable[segment * 4 + 2],
                    $"Rope segment {segment} shows the rope's one phase on draw {draw}");
        }
        Console.WriteLine("Grapple rope animation: the whole rope steps on together, segments shown after lengthening included.");
    }

    /// <summary>Runs the focused Grapple rope geometry check against the retail ROM.</summary>
    [VerificationCommand("--grapple-rope-geometry")]
    internal static int GrappleRopeGeometryCommand()
    {
        Suite(nameof(VerifyGrappleRopeGeometry), () => VerifyGrappleRopeGeometry(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
