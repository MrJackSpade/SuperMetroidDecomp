using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks that Crocomire's melt publishes through bound characters each frame, not VRAM writes (#1355).</summary>
internal static class CrocomireMeltPublicationVerification
{
    /// <summary>
    /// A constructed Crocomire melting in his room erases one column per frame on the cartridge's cadence, and each
    /// accepted NMI publishes the masked characters that frame bound, while VRAM itself never receives them.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks the melting records' native VRAM words")]
    internal static void VerifyCrocomireMeltPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
        CrocomireBehavior crocomire = runtime.Enemies.CrocomireBehavior;
        CrocomireEnemyState state = crocomire.Access.Crocomire ?? throw new InvalidOperationException("Crocomire's room loads Crocomire.");
        CrocomireDeathState death = crocomire.Access.CrocomireDeath ?? throw new InvalidOperationException("Crocomire's room loads his death state.");
        // The death sequence's order: the melting tilemap loads, then the pass's graphics, then the dissolve begins.
        crocomire.Melting.LoadFirstCrocomireMeltingTilemap(state);
        death.MeltingTableOffset = (ushort)CrocomireMeltingHeader.First;
        crocomire.Melting.InitializeCrocomireMeltingGraphics(state);
        crocomire.Melting.BeginCrocomireMelting(state);

        int frames = 0;
        for (int frame = 0; frame < 12; frame++)
        {
            byte[] heightsBefore = death.MeltingColumnHeights.ToArray();
            int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
            if (!CrocomireMeltingTransferDefinitions.TryUpload(recordOffset, out CrocomireMeltingUpload upload))
                AssertTrue(CrocomireMeltingTransferDefinitions.TryUpload(death.MeltingTableOffset, out upload), "a melt pass restarts at its first record");
            crocomire.Melting.RunCrocomireMelting(state, null);
            AssertTrue(!heightsBefore.AsSpan().SequenceEqual(death.MeltingColumnHeights), $"melt frame {frame} erases a column");

            runtime.FrameBoundary.BeginAcceptedFrame(0);
            ReadOnlySpan<byte> published = runtime.PublishDisplayFrame().Memory.Vram;
            int source = upload.SourceWord - CrocomireMeltingTransferDefinitions.ScratchStart;
            ReadOnlySpan<byte> melted = death.MeltingGraphics.Slice(source, upload.ByteCount);
            AssertTrue(published.Slice(upload.DestinationWord * 2, upload.ByteCount).SequenceEqual(melted),
                $"melt frame {frame} publishes its masked characters");
            AssertTrue(!runtime.Vram.Bytes.Slice(upload.DestinationWord * 2, upload.ByteCount).SequenceEqual(melted),
                $"melt frame {frame} writes no characters into VRAM");
            frames++;
        }
        Console.WriteLine($"  Crocomire melt: {frames} frames each erase a column and publish its masked characters.");
    }

    /// <summary>Runs the Crocomire melt publication check.</summary>
    [VerificationCommand("--crocomire-melt")]
    internal static int CrocomireMeltCommand()
    {
        Suite(nameof(VerifyCrocomireMeltPublication), () => VerifyCrocomireMeltPublication());
        return 0;
    }
}
