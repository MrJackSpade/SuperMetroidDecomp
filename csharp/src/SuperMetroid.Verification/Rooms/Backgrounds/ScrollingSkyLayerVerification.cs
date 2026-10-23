using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>Checks the scrolling sky's published parallax layer (#1347).</summary>
internal static class ScrollingSkyLayerVerification
{
    /// <summary>
    /// Steps the sky through ordinary and frozen frames; each frame's published layer carries the band offsets the
    /// 23 band speeds accumulate to, with the bottom band stationary and frozen frames leaving every offset unchanged.
    /// </summary>
    internal static void VerifyPublishedSkyBands()
    {
        var sky = new ScrollingSkyState(ScrollingSkyChunkTable.Ocean);
        var expected = new uint[RoomFxRomData.ScrollingSky.DataSlotCount];
        int frozenFrames = 0;
        for (int frame = 0; frame < 300; frame++)
        {
            bool frozen = frame % 37 is >= 30 and < 34;
            ushort cameraY = (ushort)(frame * 3);
            sky.ProcessFrame(cameraY, frozen);
            if (!frozen)
            {
                for (int band = 0; band < RoomFxRomData.ScrollingSky.SectionCount; band++)
                {
                    SkyScrollSection section = RoomFxRomData.ScrollingSky.GetSection(band);
                    expected[section.DataSlot] = unchecked(expected[section.DataSlot] + (uint)section.Velocity.RawValue);
                }
                expected[^1] = 0;
            }
            else
                frozenFrames++;

            ScrollingSkyLayer layer = sky.Publish(cameraY)
                ?? throw new InvalidOperationException("The configured sky publishes its layer.");
            AssertEqual(cameraY, layer.VerticalScroll, $"frame {frame} positions the bands from the camera");
            AssertEqual(!frozen, layer.BandsShown, $"frame {frame} shows its bands only when time runs");
            AssertEqual(expected.Length, layer.BandOffsets.Length, $"frame {frame} publishes all 23 band offsets");
            for (int slot = 0; slot < expected.Length; slot++)
                AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)expected[slot])), layer.BandOffsets[slot],
                    $"frame {frame} band slot {slot} offset");
            AssertEqual(0, layer.BandOffsets[^1], $"frame {frame} keeps the bottom band stationary");
        }
        AssertTrue(frozenFrames > 0 && expected.Take(expected.Length - 1).Any(value => value != 0),
            "the fixture covers frozen frames and moving bands");
        Console.WriteLine($"  Scrolling sky: 300 published frames ({frozenFrames} frozen) carry the accumulated band offsets.");
    }

    /// <summary>Runs the scrolling-sky layer check.</summary>
    [VerificationCommand("--scrolling-sky-layer")]
    internal static int ScrollingSkyLayerCommand()
    {
        Suite(nameof(VerifyPublishedSkyBands), () => VerifyPublishedSkyBands());
        return 0;
    }
}
