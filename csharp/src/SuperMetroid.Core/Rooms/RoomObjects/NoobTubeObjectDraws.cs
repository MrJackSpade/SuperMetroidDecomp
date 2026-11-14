using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape each of the seven noob tube draws draws (#1862). The glass's own frames are one block at the object's
/// block; the opened and broken frames are rows twelve blocks wide down from it, the rim row's and the floor row's pipe
/// edges solid around air, the floor with a second solid edge block, and the panel rows between air. What each frame
/// shows is its installed picture (#1557).
/// </summary>
internal static class NoobTubeObjectDraws
{
    /// <summary>The blocks across a tube row.</summary>
    private const int RowWidth = 12;

    /// <summary>The row of the tube's floor, below its rim, which is row zero.</summary>
    private const int FloorRow = 5;

    /// <summary>
    /// The block shape of each tube draw: the intact glass is the projectile trigger, shootable; once damaged it is solid,
    /// and the opened frame's block is the solid upper rim. The cleared and opened-rows frames draw their rows from the
    /// rim down; the broken frames clear the object's block and draw the rows down to the floor.
    /// </summary>
    private static readonly Dictionary<NoobTubeDraw, RoomBlockShape> Shapes = new()
    {
        [NoobTubeDraw.Intact] = RoomBlockShapes.Block(RoomCollisionType.ShootableBlock),
        [NoobTubeDraw.Damaged] = RoomBlockShapes.Block(RoomCollisionType.SolidBlock),
        [NoobTubeDraw.Opened] = RoomBlockShapes.Block(RoomCollisionType.SolidBlock),
        [NoobTubeDraw.Cleared] = Rows(0, 1, 2, 3),
        [NoobTubeDraw.OpenedRows] = Rows(0, 1, 2),
        [NoobTubeDraw.BrokenLate] = BrokenRows(4, 5),
        [NoobTubeDraw.BrokenFull] = BrokenRows(3, 4, 5),
    };

    /// <summary>The block shape of <paramref name="request"/> when it is one of the tube's draws.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The tube's blocks; null when the draw is not the tube's.</param>
    /// <returns>Whether the draw is one of the tube's.</returns>
    /// <exception cref="InvalidOperationException">The draw is a tube draw with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<NoobTubeDraw> { Frame: var tube })
        {
            shape = null;
            return false;
        }
        shape = Shapes.TryGetValue(tube, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined NoobTubeDraw {tube}.");
        return true;
    }

    /// <summary>The tube's <paramref name="rows"/>, each twelve blocks wide from the object's column.</summary>
    private static RoomBlockShape Rows(params int[] rows) => new([.. RowCells(rows)]);

    /// <summary>The object's own block opened, and the tube's <paramref name="rows"/> below it.</summary>
    private static RoomBlockShape BrokenRows(params int[] rows) =>
        new([new(Offset16<RoomBlocks>.Zero, RoomCollisionType.Air), .. RowCells(rows)]);

    /// <summary>The cells of the tube's <paramref name="rows"/>, each twelve blocks wide from the object's column.</summary>
    private static List<RoomBlockShapeCell> RowCells(int[] rows)
    {
        var cells = new List<RoomBlockShapeCell>();
        foreach (int row in rows)
        {
            for (int column = 0; column < RowWidth; column++)
            {
                int edgeDistance = Math.Min(column, RowWidth - 1 - column);
                bool solid = (row == 0 && edgeDistance == 0) || (row == FloorRow && edgeDistance <= 1);
                cells.Add(new(new(column, row), solid ? RoomCollisionType.SolidBlock : RoomCollisionType.Air));
            }
        }
        return cells;
    }
}
