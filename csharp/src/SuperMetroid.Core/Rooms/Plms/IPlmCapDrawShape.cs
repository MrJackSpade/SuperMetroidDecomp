using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A compiled door cap draw: one four-block run in its direction.</summary>
internal interface IPlmCapDrawShape
{
    /// <summary>The direction the cap's run writes its four blocks in.</summary>
    PlmRunDirection Direction { get; }

    /// <summary>The terrain at <paramref name="cell"/> of the run.</summary>
    RoomTerrainBlock BlockAt(int cell);
}
