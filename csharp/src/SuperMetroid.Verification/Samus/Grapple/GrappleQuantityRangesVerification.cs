using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>
/// Checks the grapple quantities stored as ordinary integers: the flare counter's saturation through the production
/// beam draw matches the cartridge's signed word test, and each quantity rejects values outside its stated range.
/// </summary>
internal static class GrappleQuantityRangesVerification
{
    /// <summary>
    /// Drives the production beam draw from flare counters around the saturation point and compares each result
    /// with the cartridge's word arithmetic: increment while the signed difference from 120 is negative.
    /// </summary>
    internal static void VerifyGrappleFlareCounterSaturation()
    {
        foreach (int start in new[] { 0, 1, 118, 119, 120 })
        {
            var grapple = new SamusGrappleState
            {
                Phase = GrapplePhase.Firing,
                RopeLength = 0,
                FlareCounter = start,
                PointAnimationTimer = NativeCountdown.Start(5),
            };
            SamusGrappleBeamDrawing.DrawConnectedBeam(grapple, new SpriteFrameBuffer(), new(0, 0),
                SpriteFrameTestArtwork.BlankGrapple, standardObjects: SpriteFrameTestArtwork.StandardObjects);
            ushort word = (ushort)start;
            if (unchecked((short)(word - 120)) < 0)
                word = unchecked((ushort)(word + 1));
            AssertEqual(word, grapple.FlareCounter, $"grapple flare counter from {start} saturates as the native word does");
        }
        Console.WriteLine("Grapple flare counter: production saturation at 120 matches the native signed word test.");
    }

    /// <summary>Checks that rope length, flare counter and endpoint frame accept their range limits and reject the values beyond them.</summary>
    internal static void VerifyGrappleQuantityRanges()
    {
        var grapple = new SamusGrappleState { RopeLength = short.MaxValue, FlareCounter = short.MaxValue, PointAnimationFrame = byte.MaxValue };
        AssertEqual(short.MaxValue, grapple.RopeLength, "rope length accepts its largest stated value");
        AssertEqual(short.MaxValue, grapple.FlareCounter, "flare counter accepts its largest stated value");
        AssertEqual(byte.MaxValue, grapple.PointAnimationFrame, "endpoint frame accepts its largest restored value");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.RopeLength = -1; }, "rope length rejects a negative length");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.RopeLength = short.MaxValue + 1; },
            "rope length rejects a length whose signed test would differ");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.FlareCounter = -1; }, "flare counter rejects a negative count");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.FlareCounter = short.MaxValue + 1; },
            "flare counter rejects a count whose signed test would differ");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.PointAnimationFrame = -1; }, "endpoint frame rejects a negative frame");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.PointAnimationFrame = byte.MaxValue + 1; },
            "endpoint frame rejects a frame no restored state holds");
        Console.WriteLine("Grapple quantity ranges: rope length, flare counter and endpoint frame keep their stated ranges.");
    }

    /// <summary>Runs the grapple quantity saturation and range checks.</summary>
    [VerificationCommand("--grapple-quantity-ranges")]
    internal static int GrappleQuantityRangesCommand()
    {
        Suite(nameof(VerifyGrappleFlareCounterSaturation), VerifyGrappleFlareCounterSaturation);
        Suite(nameof(VerifyGrappleQuantityRanges), VerifyGrappleQuantityRanges);
        return 0;
    }
}
