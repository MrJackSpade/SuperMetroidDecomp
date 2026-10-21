using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Enemies.EnemyEffectsFixtureVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;

/// <summary>Checks that replacement effect artwork changes pixels without changing Crocomire or Mother Brain timing.</summary>
internal static class EnemyEffectIsolationVerification
{
    /// <summary>Checks the two statically identified melt graphs, not a search for runtime reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyCrocomireEffectIsolation()
    {
        var baseline = new CrocomireEffectFixture(MeltEffectArtwork(0x55));
        var edited = new CrocomireEffectFixture(MeltEffectArtwork(0xaa, seed: 3));
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            bool first = pass.HeaderOffset == (ushort)CrocomireMeltingHeader.First;
            int source = (int)(first ? CrocomireMeltingTilemapAddress.FirstTilemap : CrocomireMeltingTilemapAddress.SecondTilemap);
            baseline.InitializeMap(source); edited.InitializeMap(source);
            AssertCrocomireEffectMechanics(baseline, edited, "melt map replacement");
            AssertTrue(!baseline.Effect.Bg2WorkingTilemap.SequenceEqual(edited.Effect.Bg2WorkingTilemap), "authored melt map reaches scratch");
            AssertTrue(!baseline.Vram.ImageEquals(edited.Vram), "authored melt map reaches VRAM");

            byte[] tail = baseline.Effect.MeltingGraphics[CrocomireMeltingArtwork.UsedByteCount(pass)..].ToArray();
            baseline.Call(c => c.Melting.InitializeCrocomireMeltingGraphics); edited.Call(c => c.Melting.InitializeCrocomireMeltingGraphics);
            AssertCrocomireEffectMechanics(baseline, edited, "melt graphics replacement");
            AssertTrue(!baseline.Effect.MeltingGraphics.SequenceEqual(edited.Effect.MeltingGraphics), "authored melt pixels reach scratch");
            AssertTrue(tail.SequenceEqual(baseline.Effect.MeltingGraphics[CrocomireMeltingArtwork.UsedByteCount(pass)..]),
                "native overlapping copies retain the unwritten scratch tail");
            CrocomireDeathPhase phase = baseline.Actor.DeathSequenceIndex;
            for (int record = 0; record <= pass.Uploads.Length; record++)
            {
                baseline.Call(c => c.Melting.UploadNextCrocomireMeltingGraphicsSlice); edited.Call(c => c.Melting.UploadNextCrocomireMeltingGraphicsSlice);
                AssertCrocomireEffectMechanics(baseline, edited, "melt upload " + record);
                AssertEqual(record == pass.Uploads.Length ? phase.Next() : phase, baseline.Actor.DeathSequenceIndex,
                    "only the native transfer terminator advances the phase");
                if (record < pass.Uploads.Length)
                {
                    CrocomireMeltingUpload upload = pass.Uploads[record];
                    int offset = upload.SourceWord - 0x4000;
                    AssertTrue(baseline.Vram.Bytes.Slice(upload.DestinationWord * 2, upload.ByteCount)
                        .SequenceEqual(baseline.Effect.MeltingGraphics.Slice(offset, upload.ByteCount)), "exact scheduled VRAM upload");
                }
            }

            baseline.Call(c => c.Melting.BeginCrocomireMelting); edited.Call(c => c.Melting.BeginCrocomireMelting);
            phase = baseline.Actor.DeathSequenceIndex;
            // The compiled slope starts at 256, grows by 384 to 20480, then completes on
            // the following call. This explicit bound comes from the source, not a probe.
            int calls = (20480 - 256 + 383) / 384 + 1;
            int initialSmokeObjects = baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive);
            for (int call = 1; call <= calls; call++)
            {
                baseline.Dissolve(); edited.Dissolve();
                AssertCrocomireEffectMechanics(baseline, edited, "melt dissolve call " + call);
                AssertEqual(call < calls, baseline.Effect.MeltingHdmaActive, "exact HDMA lifetime");
                AssertEqual(call == calls ? phase.Next() : phase, baseline.Actor.DeathSequenceIndex, "exact dissolve phase handoff");
                AssertEqual((ushort)Math.Min(call - 1, CrocomireMeltingDefinitions.ColumnCount),
                    baseline.Effect.MeltingColumnCursor, "fixed erase-column cadence");
                AssertEqual(initialSmokeObjects + (call + 5) / 6,
                    baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive), "fixed acid-smoke cadence");
                for (int column = 0; column < CrocomireMeltingDefinitions.ColumnCount; column++)
                    AssertEqual((byte)(column < call ? 48 : 0),
                        baseline.Effect.MeltingColumnHeights[CrocomireMeltingDefinitions.SelectColumn(column)], "exact erased-column height");
            }
            AssertEqual(pass.NextHeaderOffset, baseline.Effect.MeltingTableOffset, "exact next-pass header");
            baseline.Call(c => c.Melting.FinishCrocomireMeltingPass); edited.Call(c => c.Melting.FinishCrocomireMeltingPass);
            AssertCrocomireEffectMechanics(baseline, edited, "finished melt pass");
            AssertTrue(baseline.Effect.Bg2WorkingTilemap.SequenceEqual(edited.Effect.Bg2WorkingTilemap), "both visual variants clear the completed melt tilemap");
            AssertTrue(baseline.Vram.Bytes.Slice(EnemyBg2FrameLayout.VramBase * 2, 2048)
                .SequenceEqual(edited.Vram.Bytes.Slice(EnemyBg2FrameLayout.VramBase * 2, 2048)), "exact cleared BG2 upload");
        }
        Console.WriteLine("  Crocomire replacement: both maps/images, exact uploads, 54 calls per dissolve, erase/HDMA/smoke/phase timing unchanged.");
    }

    /// <summary>Compares two corpse images through all 118 rot calls, checking row completion and RAM-to-VRAM transfers.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyMotherBrainRotIsolation()
    {
        var baselineEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var editedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        EnemyStagingSurfaces baseline = baselineEnemies.Room.Staging;
        EnemyStagingSurfaces edited = editedEnemies.Room.Staging;
        var baselineRot = new MotherBrainCorpseRottingState();
        var editedRot = new MotherBrainCorpseRottingState();
        baselineRot.Initialize(baseline, CorpseEffectArtwork(0x55));
        editedRot.Initialize(edited, CorpseEffectArtwork(0xaa));
        AssertTrue(!baseline.RottingHeap.Bytes.SequenceEqual(edited.RottingHeap.Bytes), "different installed corpse pixels reach the rotting heap");
        Action<MotherBrainSpriteTileTransferRequest> baselineTransfer = baselineEnemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer;
        Action<MotherBrainSpriteTileTransferRequest> editedTransfer = editedEnemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer;
        int finished = 0;
        // Entry i waits 2*i calls and needs max(1,ceil(i/2)) destructive moves. The last
        // entry therefore completes on 94+24=118; different pixels cannot alter that.
        for (int call = 1; call <= 118; call++)
        {
            ushort seed = unchecked((ushort)(call * 37));
            short[] rowYBefore = Enumerable.Range(0, MotherBrainCorpseRottingState.EntryCount)
                .Select(index => MotherBrainCorpseRottingState.ReadEntry(baseline, index).YOffset)
                .ToArray();
            var expected = baselineRot.Step(baseline, new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            var actual = editedRot.Step(edited, new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            AssertEqual(call < 118, expected.StillRotting, "exact corpse lifetime");
            AssertEqual(expected.StillRotting, actual.StillRotting, "corpse lifetime independent of pixels");
            AssertAnimationValues(baselineRot, editedRot, "corpse counters at call " + call);
            AssertEqual(call < 118 ? 6 : 0, expected.VramTransfers.Count, "six uploads on each active call, none on completion");
            AssertTrue(expected.VramTransfers.SequenceEqual(actual.VramTransfers), "same corpse upload schedule");
            AssertTrue(expected.DustRequests.SequenceEqual(actual.DustRequests), "same completion dust and audio");
            // Rows finished by this call, in table order: `$DBDF` marks a completed row
            // `$FFFF`, except the final row, which completes as the call returns carry clear.
            int[] finishedRows = Enumerable.Range(0, MotherBrainCorpseRottingState.EntryCount)
                .Where(index => rowYBefore[index] >= 0 &&
                    (MotherBrainCorpseRottingState.ReadEntry(baseline, index).YOffset < 0 ||
                        (!expected.StillRotting && index == MotherBrainCorpseRottingState.EntryCount - 1)))
                .ToArray();
            AssertEqual(finishedRows.Length, expected.DustRequests.Count, "one dust request per completed corpse row");
            foreach ((MotherBrainCorpseDustRequest dust, int index) in expected.DustRequests.Zip(finishedRows))
            {
                AssertEqual(2 * index + Math.Max(1, (index + 1) / 2), call, "exact per-row completion call");
                AssertEqual(unchecked((ushort)(100 + (seed & 31) - 16)), dust.Position.X, "native dust X and sampled RNG");
                AssertEqual((ushort)136, dust.Position.Y, "native dust Y");
                AssertEqual((call & 7) == 0, dust.SoundEffectQueued, "native dust sound cadence");
                finished++;
            }
            for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
                AssertEqual(MotherBrainCorpseRottingState.ReadEntry(baseline, index),
                    MotherBrainCorpseRottingState.ReadEntry(edited, index), "every corpse row's Y/delay is independent of artwork");
            int artEnd = MotherBrainCorpseRottingState.GraphicsBufferSize;
            AssertTrue(baseline.RottingHeap.Bytes[artEnd..].SequenceEqual(edited.RottingHeap.Bytes[artEnd..]) &&
                baseline.Bg2.Bytes.SequenceEqual(edited.Bg2.Bytes), "corpse art changes only its graphics buffer");
            foreach (var request in expected.VramTransfers) baselineTransfer(request);
            foreach (var request in actual.VramTransfers) editedTransfer(request);
            SnesVram shown = ObjCharacterVramImage.Of(baselineEnemies.Room.Session.ObjCharacters, new EnemyTileTransfers(InstalledFixtures.EnemyArtwork));
            foreach (var request in expected.VramTransfers)
            {
                int source = checked((int)request.SourceAddress) - MotherBrainCorpseRottingState.GraphicsBufferAddress;
                AssertTrue(shown.Bytes.Slice(request.VramDestination * 2, request.Size)
                    .SequenceEqual(baseline.RottingHeap.Bytes.Slice(source, request.Size)), "production staging DMA uploads current rot pixels");
            }
        }
        AssertEqual(MotherBrainCorpseRottingState.EntryCount, finished, "each of the 48 completion callbacks fires exactly once");
        AssertEqual(118u, baselineRot.ProcessCallCount, "fixed corpse completion clock");
        Console.WriteLine("  Mother Brain replacement: 118 exact calls, 48 row completions, 702 RAM DMA uploads, dust/audio timing unchanged.");
    }

    /// <summary>Runs the Crocomire melt and Mother Brain corpse artwork isolation checks.</summary>
    [VerificationCommand("--enemy-effect-isolation")]
    internal static int EnemyEffectIsolationCommand()
    {
        Suite(nameof(VerifyCrocomireEffectIsolation), () => VerifyCrocomireEffectIsolation());
        Suite(nameof(VerifyMotherBrainRotIsolation), () => VerifyMotherBrainRotIsolation());
        return 0;
    }
}
