using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Harness;

/// <summary>Checks the #624 span queries and layered-snapshot operations: results, ordering, exceptions and allocations.</summary>
internal static class SpanQueriesVerification
{
    /// <summary>
    /// #624: each span query matches its <see cref="Enumerable"/> counterpart, scalar queries allocate nothing with a
    /// static predicate, owned-array operations allocate only their result, and a snapshot without a layer kind keeps
    /// the other layers, memory, object selection and brightness.
    /// </summary>
    internal static void VerifySpanQueries()
    {
        int[] values = [4, 7, 1, 7, 9];
        ReadOnlySpan<int> span = values;
        AssertEqual(Enumerable.Any(values, x => x > 8), span.Any(x => x > 8), "Any");
        AssertEqual(Enumerable.All(values, x => x > 0), span.All(x => x > 0), "All");
        AssertEqual(Enumerable.Count(values, x => x == 7), span.Count(x => x == 7), "Count");
        AssertEqual(Enumerable.First(values, x => x > 4), span.First(x => x > 4), "First keeps element order");
        AssertEqual(Enumerable.Single(values, x => x == 9), span.Single(x => x == 9), "Single");
        AssertEqual(Enumerable.Sum(values, x => x), span.Sum(x => x), "Sum");
        AssertThrows<InvalidOperationException>(() => ((ReadOnlySpan<int>)values).First(x => x > 100), "First with no match throws as Enumerable does");
        AssertThrows<InvalidOperationException>(() => ((ReadOnlySpan<int>)values).Single(x => x == 7), "Single with two matches throws");
        AssertThrows<InvalidOperationException>(() => ((ReadOnlySpan<int>)values).Single(x => x == 100), "Single with no match throws");
        int[] large = [int.MaxValue, 1];
        AssertThrows<OverflowException>(() => ((ReadOnlySpan<int>)large).Sum(x => x), "Sum is checked as Enumerable.Sum is");
        AssertTrue(span.SelectToArray(x => x * 2).SequenceEqual(Enumerable.Select(values, x => x * 2).ToArray()), "SelectToArray");
        AssertTrue(span.SelectToArray((x, index) => x + index).SequenceEqual(Enumerable.Select(values, (x, index) => x + index).ToArray()), "Indexed SelectToArray");
        AssertTrue(span.WhereToArray(x => x == 7).SequenceEqual(Enumerable.Where(values, x => x == 7).ToArray()), "WhereToArray");
        AssertTrue(span.WhereToArray((_, index) => index % 2 == 0).SequenceEqual(new[] { 4, 1, 9 }), "Indexed WhereToArray");
        AssertTrue(span.ConcatToArray([2, 3]).SequenceEqual(new[] { 4, 7, 1, 7, 9, 2, 3 }), "ConcatToArray");
        AssertEqual(2, span.CountDifferences([4, 0, 1, 0]), "CountDifferences pairs up to the shorter span, as Zip does");
        AssertEqual(4, span.CountDistinct(), "CountDistinct");
        int[] distinct = [3, 5, 8];
        var keyed = ((ReadOnlySpan<int>)distinct).ToDictionary(x => x * 10, x => x);
        AssertTrue(keyed.Count == 3 && keyed[50] == 5, "ToDictionary keys each element");
        AssertThrows<ArgumentException>(() => ((ReadOnlySpan<int>)values).ToDictionary(x => x), "ToDictionary rejects a duplicate key as Enumerable does");

        // Allocation: scalar queries with static predicates read the span in place.
        // The first pass creates each static lambda's cached delegate once; the second measures the queries alone.
        long scalarBytes = 0;
        int result = 0;
        for (int pass = 0; pass < 2; pass++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            result = (span.Any(static x => x > 8) ? 1 : 0) + (span.All(static x => x > 0) ? 1 : 0) +
                span.Count(static x => x == 7) + span.Sum(static x => x) + span.First(static x => x > 4);
            scalarBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        }
        AssertEqual(39, result, "scalar span query results");
        AssertEqual(0L, scalarBytes, "scalar span queries allocate nothing");
        Func<int, int> twice = static x => x * 2;
        _ = span.SelectToArray(twice);
        long selectBefore = GC.GetAllocatedBytesForCurrentThread();
        int[] doubled = span.SelectToArray(twice);
        long selectBytes = GC.GetAllocatedBytesForCurrentThread() - selectBefore;
        // The reference allocation: one array of the result's length, measured the same way.
        long arrayBefore = GC.GetAllocatedBytesForCurrentThread();
        int[] reference = new int[values.Length];
        long arrayBytes = GC.GetAllocatedBytesForCurrentThread() - arrayBefore;
        AssertTrue(selectBytes == arrayBytes && doubled.Length == reference.Length,
            $"SelectToArray allocates only its result ({selectBytes} bytes; one array is {arrayBytes})");

        // A snapshot without a layer kind keeps everything else, and LayersOf finds a kind in order.
        var cgram = new SnesCgram();
        var oam = new OamBuffer();
        oam.BeginFrame();
        oam.FinalizeFrame();
        var memory = FramePaintConstants.CaptureFrontend(new SnesVram(), cgram, oam, 3);
        var red = new FixedColorAddRenderLayer(31, 0, 0);
        var objects = new SpritePriorityRenderLayer(2);
        var blue = new FixedColorAddRenderLayer(0, 0, 31);
        var snapshot = new LayeredScene(memory, new RenderLayer[] { red, objects, blue }, 15);
        LayeredScene without = snapshot.Without<FixedColorAddRenderLayer>();
        AssertTrue(without.Layers.SequenceEqual(new RenderLayer[] { objects }), "Without removes only the selected kind");
        AssertTrue(ReferenceEquals(snapshot.Paint, without.Paint) && without.Brightness == 15,
            "Without shares memory and brightness");
        AssertTrue(snapshot.Layers.SequenceEqual(new RenderLayer[] { red, objects, blue }), "Without leaves the source snapshot unchanged");
        AssertTrue(snapshot.LayersOf<FixedColorAddRenderLayer>().SequenceEqual(new[] { red, blue }), "LayersOf keeps application order");
        AssertEqual(0, without.LayersOf<FixedColorAddRenderLayer>().Length, "LayersOf finds none after Without");
        Console.WriteLine("#624 span queries: Enumerable-equivalent results and exceptions, zero-allocation scalar queries, owned results only, snapshot layer removal.");
    }

    /// <summary>Runs the focused span-query checks from the CLI.</summary>
    [VerificationCommand("--span-queries")]
    internal static int SpanQueriesCommand()
    {
        Suite(nameof(VerifySpanQueries), () => VerifySpanQueries());
        return 0;
    }
}
