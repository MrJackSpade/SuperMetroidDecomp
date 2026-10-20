namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A compiled PLM draw described run by run, from which its draw list is built.</summary>
internal interface IPlmRunDrawShape
{
    /// <summary>The number of runs the draw writes.</summary>
    int RunCount { get; }

    /// <summary>The number of level words in run <paramref name="run"/>.</summary>
    int WordCount(int run);

    /// <summary>The level word at <paramref name="cell"/> of run <paramref name="run"/>.</summary>
    ushort WordAt(int run, int cell);

    /// <summary>The horizontal block step after run <paramref name="run"/>.</summary>
    sbyte NextX(int run);

    /// <summary>The vertical block step after run <paramref name="run"/>.</summary>
    sbyte NextY(int run);
}
