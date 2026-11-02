using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The elevator platform's appearance: a cell's selected appearance, from the authored appearances that differ from
/// the compiled frames, and the identity of the selected frames.
/// </summary>
internal static class ElevatorPlatformArtwork
{
    /// <summary>Hashes each frame's selected appearances run by run, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmElevatorPlatformVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmElevatorPlatformVisualCatalog), content =>
        {
            var buffer = new RoomBlockAppearance[4];
            foreach (var frame in ElevatorPlatformDraws.DrawLists.OrderBy(item => ElevatorPlatformDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                ElevatorPlatformDraws.TryDescribe(frame.Draw, out var shape);
                content.AppendName("frame", ElevatorPlatformDraws.VisualId(frame.Draw));
                content.Append("runs", shape.RunCount);
                for (int run = 0; run < shape.RunCount; run++)
                {
                    int count = shape.BlockCount(run);
                    for (int cell = 0; cell < count; cell++)
                        buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell) ??
                            throw new InvalidDataException($"Elevator-platform visuals lack frame {frame.Draw}.");
                    content.AppendAppearances("blocks", buffer.AsSpan(0, count));
                }
            }
        });

    /// <summary>Resolves one elevator-platform block appearance without replacing its compiled terrain block or draw offset.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(IReadOnlyDictionary<PlmDraw, RoomBlockAppearance[][]> customAppearances, PlmDraw draw, int runIndex, int cellIndex)
    {
        if (!ElevatorPlatformDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)cellIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(cellIndex));
        return customAppearances.TryGetValue(draw, out var runs)
            ? runs[runIndex][cellIndex] : shape.BlockAt(runIndex, cellIndex).Appearance;
    }
}
