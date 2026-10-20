namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A compiled door cap draw: one four-word run in its direction.</summary>
internal interface IPlmCapDrawShape
{
    /// <summary>The run's native direction-and-count word.</summary>
    ushort DirectionAndCountWord();

    /// <summary>The level word at <paramref name="cell"/> of the run.</summary>
    ushort WordAt(int cell);
}
