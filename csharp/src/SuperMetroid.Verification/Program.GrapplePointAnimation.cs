using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyGrapplePointAnimation(ISnesAddressSpace bus)
    {
        VerifyGrappleRopeGeometry(bus);
        ushort begin = RomDataReader.ReadWordFixedBank(bus, SamusGrappleRomData.Rendering.PointTilePointers);
        ushort end = RomDataReader.ReadWordFixedBank(bus, SamusGrappleRomData.Rendering.PointTilePointers + 2);
        var grapple = new SamusGrappleState { Phase = GrapplePhase.Firing, PointAnimationTimer = 5 };
        ushort expectedTimer = 5, expectedPointer = begin;
        for (int tick = 0; tick < 48; tick++)
        {
            expectedTimer = unchecked((ushort)(expectedTimer - 1));
            if ((short)expectedTimer < 0)
            {
                expectedTimer = 5;
                expectedPointer = unchecked((ushort)(expectedPointer + 512));
                if ((short)(expectedPointer - end) >= 0) expectedPointer = begin;
            }
            var queue = new VramWriteQueue();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, new OamBuffer(), queue, 0, 0);
            AssertEqual(expectedTimer, grapple.PointAnimationTimer, "Grapple endpoint timer matches native DEC/BPL");
            AssertEqual(0x9a0000 | expectedPointer, queue.Entries[0].SourceAddress, $"Grapple endpoint follows four-frame native stride at tick {tick}");
            AssertEqual(2, queue.Entries.Count, "Zero-length rope still queues native endpoint and segment tiles");
        }
        foreach (ushort timer in new ushort[] { 0, 1, 5, 32768, 32769, 65535 })
        for (int frame = 0; frame < 256; frame++)
        {
            grapple.PointAnimationFrame = (byte)frame; grapple.PointAnimationTimer = timer;
            expectedPointer = unchecked((ushort)(begin + frame * 512));
            expectedTimer = unchecked((ushort)(timer - 1));
            if ((short)expectedTimer < 0)
            {
                expectedTimer = 5; expectedPointer = unchecked((ushort)(expectedPointer + 512));
                if (unchecked((short)(expectedPointer - end)) >= 0) expectedPointer = begin;
            }
            var queue = new VramWriteQueue();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, new OamBuffer(), queue, 0, 0);
            AssertEqual(expectedTimer, grapple.PointAnimationTimer, "Endpoint boundary timer matches native signed decrement");
            AssertEqual(0x9a0000 | expectedPointer, queue.Entries[0].SourceAddress, "Endpoint ordinal retains native wrapped pointer arithmetic");
        }
        Console.WriteLine("Grapple endpoint animation: two four-frame cycles and 1536 restored frame/timer boundaries preserve native upload source and six-call cadence.");
    }
}
