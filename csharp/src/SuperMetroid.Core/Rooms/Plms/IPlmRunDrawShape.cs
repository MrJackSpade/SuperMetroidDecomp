using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A compiled PLM draw described run by run, from which its draw list is built.</summary>
internal interface IPlmRunDrawShape
{
    /// <summary>The number of runs the draw writes.</summary>
    int RunCount { get; }

    /// <summary>The number of blocks in run <paramref name="run"/>.</summary>
    int BlockCount(int run);

    /// <summary>The terrain at <paramref name="cell"/> of run <paramref name="run"/>.</summary>
    RoomTerrainBlock BlockAt(int run, int cell);

    /// <summary>The horizontal block step after run <paramref name="run"/>.</summary>
    int NextX(int run);

    /// <summary>The vertical block step after run <paramref name="run"/>.</summary>
    int NextY(int run);
}
