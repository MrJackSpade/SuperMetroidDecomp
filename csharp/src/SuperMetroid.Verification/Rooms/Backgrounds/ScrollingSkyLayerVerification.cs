using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>Checks the scrolling sky's published bands (#1347, #1687).</summary>
internal static class ScrollingSkyLayerVerification
{
    /// <summary>
    /// Steps the sky through ordinary and frozen frames; each frame's published sky places its bands against the camera and
    /// carries, for every band, the offset its drift's speed accumulates to, with the bottom band still and frozen frames
    /// leaving every offset unchanged.
    /// </summary>
    internal static void VerifyPublishedSkyBands()
    {
        var sky = new ScrollingSkyState(ScrollingSkyChunkTable.Ocean);
        var expected = ScrollingSkyBandDefinitions.Drifts.ToDictionary(drift => drift, _ => default(FixedCoordinate));
        int frozenFrames = 0;
        for (int frame = 0; frame < 300; frame++)
        {
            bool frozen = frame % 37 is >= 30 and < 34;
            ushort cameraY = (ushort)(frame * 3);
            sky.ProcessFrame(cameraY, frozen);
            if (!frozen)
                foreach (SkyDrift drift in ScrollingSkyBandDefinitions.Drifts)
                    expected[drift] = expected[drift].MovedBy(drift.Speed);
            else
                frozenFrames++;

            ScrollingSkyLayer layer = sky.Publish(cameraY)
                ?? throw new InvalidOperationException("The configured sky publishes its layer.");
            AssertEqual((int)cameraY, layer.CameraRow, $"frame {frame} places the bands against the camera");
            AssertEqual(!frozen, layer.ShowsDrift, $"frame {frame} shows the drift only when time runs");
            AssertEqual(ScrollingSkyBandDefinitions.Bands.Length, layer.Bands.Length, $"frame {frame} publishes every band");
            for (int band = 0; band < layer.Bands.Length; band++)
            {
                SkyBand definition = ScrollingSkyBandDefinitions.Bands[band];
                AssertEqual(definition.TopRow, layer.Bands[band].FirstRow, $"frame {frame} band {band} top");
                AssertEqual(band + 1 < layer.Bands.Length ? ScrollingSkyBandDefinitions.Bands[band + 1].TopRow : ScrollingSkyBandDefinitions.EndRow,
                    layer.Bands[band].EndRow, $"frame {frame} band {band} end");
                AssertEqual((int)expected[definition.Drift].Whole, layer.Bands[band].Offset, $"frame {frame} band {band} offset");
            }
            AssertEqual(0, layer.Bands[^1].Offset, $"frame {frame} keeps the bottom band still");
        }
        AssertTrue(frozenFrames > 0 && expected.Values.Any(phase => phase.Whole != 0),
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
