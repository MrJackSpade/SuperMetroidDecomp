using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The draw of one frame of a family (#1402, #2034).</summary>
/// <typeparam name="TFrame">The family's frame: an enum of its frames, or a record of what varies between them.</typeparam>
/// <param name="Family">The family the frame belongs to, which draws it.</param>
/// <param name="Frame">The frame.</param>
internal sealed partial record RoomObjectFrameDraw<TFrame>(RoomObjectDrawFamily<TFrame> Family, TFrame Frame) : RoomObjectDraw
    where TFrame : struct
{
    /// <inheritdoc/>
    internal override void Draw(RoomLevelData level, Position16<RoomBlocks> origin, IRoomObjectDrawing drawing) =>
        Family.Draw(level, origin, drawing, this);

    /// <summary>The family's name and the frame, as diagnostics name the draw.</summary>
    public override string ToString() => $"{Family.Name} {Frame}";
}
