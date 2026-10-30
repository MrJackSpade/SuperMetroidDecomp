using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of both mirrored, three-component eye doors: the draw each typed eye-door frame shows, their physical
/// cells from <see cref="EyeDoorPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class EyeDoorDraws
{
    /// <summary>The draw <paramref name="frame"/> shows for <paramref name="component"/> facing <paramref name="orientation"/>.</summary>
    internal static PlmDraw DrawList(EyeDoorOrientation orientation, EyeDoorComponent component, EyeDoorPlmFrame frame)
    {
        bool left = orientation switch
        {
            EyeDoorOrientation.Left => true,
            EyeDoorOrientation.Right => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        Component part = component switch
        {
            EyeDoorComponent.Door or EyeDoorComponent.Eye => Component.Middle,
            EyeDoorComponent.Bottom => Component.Bottom,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorComponent)} {component}."),
        };
        return frame switch
        {
            EyeDoorPlmFrame.Eye0 => PlmDraw.Of(new Draw(Component.Eye, 0, left)),
            EyeDoorPlmFrame.Eye1 => PlmDraw.Of(new Draw(Component.Eye, 1, left)),
            EyeDoorPlmFrame.Eye2 => PlmDraw.Of(new Draw(Component.Eye, 2, left)),
            EyeDoorPlmFrame.Eye3 => PlmDraw.Of(new Draw(Component.Eye, 3, left)),
            EyeDoorPlmFrame.Eye4 => PlmDraw.Of(new Draw(Component.Eye, 4, left)),
            EyeDoorPlmFrame.OpeningClear => PlmDraw.Of(new Draw(Component.Clear, 0, left)),
            // The eye flashes to the closed blue cap of its facing.
            EyeDoorPlmFrame.BlueCap => BlueDoorCaps.Alias(left ? ColoredDoorOrientation.Left : ColoredDoorOrientation.Right),
            EyeDoorPlmFrame.Pose0 => PlmDraw.Of(new Draw(part, 0, left)),
            EyeDoorPlmFrame.Pose1 => PlmDraw.Of(new Draw(part, 1, left)),
            EyeDoorPlmFrame.Pose2 => PlmDraw.Of(new Draw(part, 2, left)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorPlmFrame)} {frame}."),
        };
    }

    /// <summary>Physical segment affected by one eye-door draw.</summary>
    internal enum Component
    {
        /// <summary>Animated two-block eye with its vulnerable collision phase.</summary>
        Eye,
        /// <summary>Middle solid door component.</summary>
        Middle,
        /// <summary>Bottom solid door component.</summary>
        Bottom,
        /// <summary>Four-block clearing frame after the eye opens.</summary>
        Clear,
    }

    /// <summary>One eye-door draw: its component, animation frame and side.</summary>
    /// <param name="Part">The component the draw writes.</param>
    /// <param name="Frame">The component's animation frame.</param>
    /// <param name="Left">Whether the draw belongs to the left-facing door; the two clears differ only in this.</param>
    [IndependentFlag("which of the two mirrored eye doors a draw belongs to, a fact of the draw")]
    internal readonly record struct Draw(Component Part, [property: IntegerQuantity("eye animation frame index")] int Frame, bool Left)
    {
        /// <summary>Whether the run writes cells vertically rather than horizontally.</summary>
        internal bool Vertical => Part is Component.Eye or Component.Clear;
        /// <summary>Gets the cell count for an eye, middle, bottom, or clear component.</summary>
        internal int BlockCount => EyeDoorPlmDrawDefinitions.Cells[this].Count;
        /// <summary>The terrain of <paramref name="cell"/> in run order.</summary>
        internal RoomTerrainBlock BlockAt(int cell)
        {
            IReadOnlyList<RoomTerrainBlock> cells = EyeDoorPlmDrawDefinitions.Cells[this];
            if ((uint)cell >= (uint)cells.Count) throw new ArgumentOutOfRangeException(nameof(cell));
            return cells[cell];
        }
    }

    /// <summary>Enumerates both sides' clear, eye, middle, and bottom draws, left side first.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (bool left in new[] { true, false })
            {
                TryGet(PlmDraw.Of(new Draw(Component.Clear, 0, left)), out var clear);
                yield return clear;
                foreach (Component part in new[] { Component.Eye, Component.Middle, Component.Bottom })
                    for (int frame = 0; frame < EyeDoorPlmDrawDefinitions.FrameCounts[part]; frame++)
                    {
                        TryGet(PlmDraw.Of(new Draw(part, frame, left)), out var value);
                        yield return value;
                    }
            }
        }
    }

    /// <summary>The mirrored clear shares one visual identity, preserving existing 23-frame overrides.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable =>
        All.Where(draw => draw.Draw != EyeDoorPlmDrawDefinitions.MirroredOpeningClear);

    /// <summary>Maps the mirrored opening clear to its shared editable visual source.</summary>
    internal static PlmDraw VisualSource(PlmDraw request) =>
        request == EyeDoorPlmDrawDefinitions.MirroredOpeningClear ? EyeDoorPlmDrawDefinitions.SharedClear : request;

    /// <summary>Accepts an eye-door draw whose frame lies within its component's animation.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw frame) => (uint)frame.Frame < (uint)EyeDoorPlmDrawDefinitions.FrameCounts[frame.Part], out draw);

    /// <summary>Materializes a native-shaped run for artwork tooling from the physical draw.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!TryDescribe(request, out var draw)) { list = default; return false; }
        var blocks = new RoomTerrainBlock[draw.BlockCount];
        for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = draw.BlockAt(cell);
        list = new(request, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(draw.Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, blocks, new(0, 0)),
        });
        return true;
    }

    /// <summary>Gets a stable side, component, and frame identifier for an editable draw.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!TryDescribe(request, out var draw) || request == EyeDoorPlmDrawDefinitions.MirroredOpeningClear)
            throw new InvalidDataException($"Eye-door draw {request} has no visual ID.");
        string component = draw.Part switch
        {
            Component.Clear => "clear",
            Component.Eye => "eye",
            Component.Middle => "middle",
            Component.Bottom => "bottom",
            _ => throw new InvalidOperationException($"Undefined {nameof(Component)} {draw.Part}."),
        };
        return draw.Part == Component.Clear ? "left-eye-clear" : $"{(draw.Left ? "left" : "right")}-{component}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact editable visual identifier to its draw.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(Editable, VisualId, id, out list);
}
