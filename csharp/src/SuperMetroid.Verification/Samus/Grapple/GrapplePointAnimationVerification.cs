using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Grapple.GrappleRopeGeometryVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks grapple endpoint tile selection against native pointer and timer behavior.</summary>
internal static class GrapplePointAnimationVerification
{
    /// <summary>Checks repeated frame stride and restored frame/timer boundaries, including wrapped pointer arithmetic.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple point tile pointers")]
    internal static void VerifyGrapplePointAnimation(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyGrappleRopeGeometry), () => VerifyGrappleRopeGeometry(bus));
        ushort begin = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleBeamDefinitions.Rendering.PointTilePointers);
        ushort end = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GrappleBeamDefinitions.Rendering.PointTilePointers + 2);
        var grapple = new SamusGrappleState { Phase = GrapplePhase.Firing, PointAnimationTimer = WrappingCountdown.Start(5) };
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
            AssertEqual(0x9a0000 | expectedPointer, GrapplePointAnimationDefinitions.SourceAddress(grapple.PointAnimationFrame),
                $"Grapple endpoint follows four-frame native stride at tick {tick}");
        }
        foreach (ushort timer in new ushort[] { 0, 1, 5, 32768, 32769, 65535 })
        for (int frame = 0; frame < 256; frame++)
        {
            grapple.PointAnimationFrame = (byte)frame; grapple.PointAnimationTimer = WrappingCountdown.FromRaw(timer);
            expectedPointer = unchecked((ushort)(begin + frame * 512));
            CountdownStep expectedStep = WrappingCounter.Decrement(timer);
            expectedTimer = expectedStep.Value;
            if (expectedStep.IsNegative)
            {
                expectedTimer = 5; expectedPointer = unchecked((ushort)(expectedPointer + 512));
                if (unchecked((short)(expectedPointer - end)) >= 0) expectedPointer = begin;
            }
            // Restored frames past the four installed endpoint tiles exist only in corrupted state; drawing them
            // fails loudly, so this checks the animation arithmetic itself.
            GrapplePointAnimationDefinitions.Advance(grapple);
            AssertEqual(expectedTimer, grapple.PointAnimationTimer.RawValue, "Endpoint boundary timer matches native signed decrement");
            AssertEqual(0x9a0000 | expectedPointer, GrapplePointAnimationDefinitions.SourceAddress(grapple.PointAnimationFrame),
                "Endpoint ordinal retains native wrapped pointer arithmetic");
        }
        Console.WriteLine("Grapple endpoint animation: two four-frame cycles and 1536 restored frame/timer boundaries preserve native endpoint selection and six-call cadence.");
    }
}
