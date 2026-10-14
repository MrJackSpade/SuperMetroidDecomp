using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuperMetroid.Verification.Samus.Grapple.GrappleRopeGeometryVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks grapple endpoint tile uploads against native pointer and timer behavior.</summary>
internal static class GrapplePointAnimationVerification
{
    /// <summary>Checks repeated frame stride and restored frame/timer boundaries, including wrapped pointer arithmetic.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple point tile pointers")]
    internal static void VerifyGrapplePointAnimation(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyGrappleRopeGeometry), () => VerifyGrappleRopeGeometry(bus));
        ushort begin = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusGrappleRomData.Rendering.PointTilePointers);
        ushort end = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusGrappleRomData.Rendering.PointTilePointers + 2);
        var grapple = new SamusGrappleState { Phase = GrapplePhase.Firing, PointAnimationTimer = NativeCountdown.Start(5) };
        ushort expectedTimer = 5, expectedPointer = begin;
        for (int tick = 0; tick < 48; tick++)
        {
            NativeWordCounterStep expectedStep = NativeWordCounter.Decrement(expectedTimer);
            expectedTimer = expectedStep.Value;
            if (expectedStep.IsNegative)
            {
                expectedTimer = 5;
                expectedPointer = unchecked((ushort)(expectedPointer + 512));
                if ((short)(expectedPointer - end) >= 0) expectedPointer = begin;
            }
            var queue = new VramWriteQueue();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, new OamBuffer(), queue, new(0, 0));
            AssertEqual(expectedTimer, grapple.PointAnimationTimer.RawValue, "Grapple endpoint timer matches native DEC/BPL");
            AssertEqual(0x9a0000 | expectedPointer, queue.Entries[0].SourceAddress, $"Grapple endpoint follows four-frame native stride at tick {tick}");
            AssertEqual(2, queue.Entries.Count, "Zero-length rope still queues native endpoint and segment tiles");
        }
        foreach (ushort timer in new ushort[] { 0, 1, 5, 32768, 32769, 65535 })
        for (int frame = 0; frame < 256; frame++)
        {
            grapple.PointAnimationFrame = (byte)frame; grapple.PointAnimationTimer = NativeCountdown.FromRaw(timer);
            expectedPointer = unchecked((ushort)(begin + frame * 512));
            NativeWordCounterStep expectedStep = NativeWordCounter.Decrement(timer);
            expectedTimer = expectedStep.Value;
            if (expectedStep.IsNegative)
            {
                expectedTimer = 5; expectedPointer = unchecked((ushort)(expectedPointer + 512));
                if (unchecked((short)(expectedPointer - end)) >= 0) expectedPointer = begin;
            }
            var queue = new VramWriteQueue();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, new OamBuffer(), queue, new(0, 0));
            AssertEqual(expectedTimer, grapple.PointAnimationTimer.RawValue, "Endpoint boundary timer matches native signed decrement");
            AssertEqual(0x9a0000 | expectedPointer, queue.Entries[0].SourceAddress, "Endpoint ordinal retains native wrapped pointer arithmetic");
        }
        Console.WriteLine("Grapple endpoint animation: two four-frame cycles and 1536 restored frame/timer boundaries preserve native upload source and six-call cadence.");
    }
}
