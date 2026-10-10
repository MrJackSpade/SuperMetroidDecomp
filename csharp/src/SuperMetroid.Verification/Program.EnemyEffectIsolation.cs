using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the two statically identified melt graphs, not a search for runtime reads.</summary>
    private static void VerifyCrocomireEffectIsolation()
    {
        var baseline = new CrocomireEffectFixture(MeltEffectArtwork(0x55));
        var edited = new CrocomireEffectFixture(MeltEffectArtwork(0xaa, seed: 3));
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            bool first = pass.HeaderOffset == CrocomireMeltingTransferDefinitions.FirstHeaderOffset;
            int source = first ? CrocomireMeltingArtworkAddresses.FirstTilemap : CrocomireMeltingArtworkAddresses.SecondTilemap;
            baseline.InitializeMap(source); edited.InitializeMap(source);
            AssertCrocomireEffectMechanics(baseline, edited, "melt map replacement");
            AssertTrue(!baseline.Effect.Bg2WorkingTilemap.SequenceEqual(edited.Effect.Bg2WorkingTilemap), "authored melt map reaches scratch");
            AssertTrue(!baseline.Vram.Bytes.SequenceEqual(edited.Vram.Bytes), "authored melt map reaches VRAM");

            byte[] tail = baseline.Effect.MeltingGraphics[CrocomireMeltingArtwork.UsedByteCount(pass)..].ToArray();
            baseline.Call("InitializeCrocomireMeltingGraphics"); edited.Call("InitializeCrocomireMeltingGraphics");
            AssertCrocomireEffectMechanics(baseline, edited, "melt graphics replacement");
            AssertTrue(!baseline.Effect.MeltingGraphics.SequenceEqual(edited.Effect.MeltingGraphics), "authored melt pixels reach scratch");
            AssertTrue(tail.AsSpan().SequenceEqual(baseline.Effect.MeltingGraphics[CrocomireMeltingArtwork.UsedByteCount(pass)..]),
                "native overlapping copies retain the unwritten scratch tail");
            ushort phase = baseline.Actor.DeathSequenceIndex;
            for (int record = 0; record <= pass.Uploads.Length; record++)
            {
                baseline.Call("UploadNextCrocomireMeltingGraphicsSlice"); edited.Call("UploadNextCrocomireMeltingGraphicsSlice");
                AssertCrocomireEffectMechanics(baseline, edited, "melt upload " + record);
                AssertEqual((ushort)(phase + (record == pass.Uploads.Length ? 2 : 0)), baseline.Actor.DeathSequenceIndex,
                    "only the native transfer terminator advances the phase");
                if (record < pass.Uploads.Length)
                {
                    CrocomireMeltingUpload upload = pass.Uploads[record];
                    int offset = upload.SourceWord - 0x4000;
                    AssertTrue(baseline.Vram.Bytes.Slice(upload.DestinationWord * 2, upload.ByteCount)
                        .SequenceEqual(baseline.Effect.MeltingGraphics.Slice(offset, upload.ByteCount)), "exact scheduled VRAM upload");
                }
            }

            baseline.Call("BeginCrocomireMelting"); edited.Call("BeginCrocomireMelting");
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
                AssertEqual((ushort)(phase + (call == calls ? 2 : 0)), baseline.Actor.DeathSequenceIndex, "exact dissolve phase handoff");
                AssertEqual((ushort)Math.Min(call - 1, CrocomireMeltingDefinitions.ColumnCount),
                    baseline.Effect.MeltingColumnCursor, "fixed erase-column cadence");
                AssertEqual(initialSmokeObjects + (call + 5) / 6,
                    baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive), "fixed acid-smoke cadence");
                for (int column = 0; column < CrocomireMeltingDefinitions.ColumnCount; column++)
                    AssertEqual((byte)(column < call ? 48 : 0),
                        baseline.Effect.MeltingColumnHeights[CrocomireMeltingDefinitions.SelectColumn(column)], "exact erased-column height");
            }
            AssertEqual(pass.NextHeaderOffset, baseline.Effect.MeltingTableOffset, "exact next-pass header");
            baseline.Call("FinishCrocomireMeltingPass"); edited.Call("FinishCrocomireMeltingPass");
            AssertCrocomireEffectMechanics(baseline, edited, "finished melt pass");
            AssertTrue(baseline.Effect.Bg2WorkingTilemap.SequenceEqual(edited.Effect.Bg2WorkingTilemap), "both visual variants clear the completed melt tilemap");
            AssertTrue(baseline.Vram.Bytes.Slice(EnemyBg2FrameLayout.VramBase * 2, 2048)
                .SequenceEqual(edited.Vram.Bytes.Slice(EnemyBg2FrameLayout.VramBase * 2, 2048)), "exact cleared BG2 upload");
        }
        Console.WriteLine("  Crocomire replacement: both maps/images, exact uploads, 54 calls per dissolve, erase/HDMA/smoke/phase timing unchanged.");
    }

    /// <summary>
    /// Verifies that replacing Mother Brain's corpse artwork changes its graphics buffer without changing
    /// the rot timeline, row completions, DMA schedule, or completion effects.
    /// </summary>
    private static void VerifyMotherBrainRotIsolation()
    {
        var baseline = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var edited = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var baselineRot = new MotherBrainCorpseRottingState();
        var editedRot = new MotherBrainCorpseRottingState();
        baselineRot.Initialize(baseline, CorpseEffectArtwork(0x55));
        editedRot.Initialize(edited, CorpseEffectArtwork(0xaa));
        AssertTrue(!baseline.WorkRam.SequenceEqual(edited.WorkRam), "different installed corpse pixels reach WRAM");
        var baselineVram = new SnesVram(); var editedVram = new SnesVram();
        Action<MotherBrainSpriteTileTransferRequest> baselineTransfer = CorpseRamTransfer(baseline, baselineVram);
        Action<MotherBrainSpriteTileTransferRequest> editedTransfer = CorpseRamTransfer(edited, editedVram);
        int finished = 0;
        // Entry i waits 2*i calls and needs max(1,ceil(i/2)) destructive moves. The last
        // entry therefore completes on 94+24=118; different pixels cannot alter that.
        for (int call = 1; call <= 118; call++)
        {
            ushort seed = unchecked((ushort)(call * 37));
            short[] rowYBefore = Enumerable.Range(0, MotherBrainCorpseRottingState.EntryCount)
                .Select(index => MotherBrainCorpseRottingState.ReadEntry(baseline, index).YOffset)
                .ToArray();
            var expected = baselineRot.Step(baseline, baseline, 100, 120, seed, (ushort)call);
            var actual = editedRot.Step(edited, edited, 100, 120, seed, (ushort)call);
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
                AssertEqual(unchecked((ushort)(100 + (seed & 31) - 16)), dust.XPosition, "native dust X and sampled RNG");
                AssertEqual((ushort)136, dust.YPosition, "native dust Y");
                AssertEqual((call & 7) == 0, dust.SoundEffectQueued, "native dust sound cadence");
                finished++;
            }
            for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
                AssertEqual(MotherBrainCorpseRottingState.ReadEntry(baseline, index),
                    MotherBrainCorpseRottingState.ReadEntry(edited, index), "every corpse row's Y/delay is independent of artwork");
            int artStart = MotherBrainCorpseRottingState.GraphicsBufferAddress & 0x1ffff;
            int artEnd = artStart + MotherBrainCorpseRottingState.GraphicsBufferSize;
            AssertTrue(baseline.WorkRam[..artStart].SequenceEqual(edited.WorkRam[..artStart]) &&
                baseline.WorkRam[artEnd..].SequenceEqual(edited.WorkRam[artEnd..]), "corpse art changes only its graphics buffer");
            foreach (var request in expected.VramTransfers) baselineTransfer(request);
            foreach (var request in actual.VramTransfers) editedTransfer(request);
            foreach (var request in expected.VramTransfers)
            {
                int source = checked((int)request.SourceAddress) & 0x1ffff;
                AssertTrue(baselineVram.Bytes.Slice(request.VramDestination * 2, request.Size)
                    .SequenceEqual(baseline.WorkRam.Slice(source, request.Size)), "production mutable-RAM DMA uploads current rot pixels");
            }
        }
        AssertEqual(MotherBrainCorpseRottingState.EntryCount, finished, "each of the 48 completion callbacks fires exactly once");
        AssertEqual(118u, baselineRot.ProcessCallCount, "fixed corpse completion clock");
        Console.WriteLine("  Mother Brain replacement: 118 exact calls, 48 row completions, 702 RAM DMA uploads, dust/audio timing unchanged.");
    }

    /// <summary>
    /// Creates the production corpse-tile transfer callback against the supplied memory and VRAM instances.
    /// </summary>
    /// <param name="memory">The address space containing the mutable corpse graphics buffer.</param>
    /// <param name="vram">The VRAM receiving tile data requested by Mother Brain's corpse effect.</param>
    /// <returns>A callback that applies each corpse sprite-tile transfer request.</returns>
    private static Action<MotherBrainSpriteTileTransferRequest> CorpseRamTransfer(SuperMetroidAddressSpace memory, SnesVram vram)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, memory);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        return typeof(RoomEnemySystem).GetMethod("ApplyMotherBrainRainbowTileTransfer", flags)!
            .CreateDelegate<Action<MotherBrainSpriteTileTransferRequest>>(enemies);
    }
}
