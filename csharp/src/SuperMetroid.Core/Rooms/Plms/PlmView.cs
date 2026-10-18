namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The BG1 view a PLM tile write reflects: the layer-1 position and the BG1 X offset (#633).</summary>
public readonly record struct PlmView
{
    /// <summary>Layer1 position.</summary>
    public required Position16<RoomPixels> Layer1Position { get; init; }

    /// <summary>Bg1 x offset.</summary>
    public required int Bg1XOffset
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
}
