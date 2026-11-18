using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Samus.Grapple.GrappleRopeGeometryVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks grapple endpoint tile selection against native pointer and timer behavior.</summary>
internal static class GrapplePointAnimationVerification
{
    /// <summary>
    /// Checks the endpoint's frames and countdown over two of its four-frame cycles against the native pointer stride and
    /// DEC/BPL countdown (#2388: the endpoint plays its animation, six draws a frame).
    /// </summary>
    internal static void VerifyGrapplePointAnimation(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyGrappleRopeGeometry), () => VerifyGrappleRopeGeometry(bus));
        ushort begin = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleBeamDefinitions.Rendering.PointTilePointers);
        ushort end = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleBeamDefinitions.Rendering.PointTilePointers + 2);
        var grapple = new SamusGrappleState(global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Phase = GrapplePhase.Firing };
        ushort expectedTimer = 5, expectedPointer = begin;
        for (int tick = 0; tick < 48; tick++)
        {
            CountdownStep expectedStep = WrappingCounter.Decrement(expectedTimer);
            expectedTimer = expectedStep.Value;
            if (expectedStep.IsNegative)
            {
                expectedTimer = 5;
                expectedPointer = unchecked((ushort)(expectedPointer + 512));
                if ((short)(expectedPointer - end) >= 0) expectedPointer = begin;
            }
            SamusGrappleBeamDrawing.DrawConnectedBeam(grapple, new SpriteFrameBuffer(), new(0, 0), InstalledFixtures.GrappleSheet);
            AssertEqual(expectedTimer, grapple.PointAnimationTimer.RawValue, "Grapple endpoint timer matches native DEC/BPL");
            AssertEqual(0x9a0000 | expectedPointer, GrapplePointAnimationNative.SourceAddress(grapple.PointAnimationFrame),
                $"Grapple endpoint follows four-frame native stride at tick {tick}");
        }
        Console.WriteLine("Grapple endpoint animation: two four-frame cycles preserve native endpoint selection and six-call cadence.");
    }
}
