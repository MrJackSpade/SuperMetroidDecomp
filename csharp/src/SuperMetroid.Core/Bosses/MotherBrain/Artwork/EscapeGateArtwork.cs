using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's escape gate: a block's selected appearance, from the authored overrides or the
/// compiled terrain block, and the identity of the selected frames.
/// </summary>
internal static class EscapeGateArtwork
{
    /// <summary>Hashes every gate frame's selected appearances, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmEscapeGateVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmEscapeGateVisualCatalog), content =>
        {
            var appearances = new RoomBlockAppearance[4];
            foreach (var frame in MotherBrainEscapeGateDraws.All.OrderBy(item => MotherBrainEscapeGateDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int row = 0; row < appearances.Length; row++)
                    appearances[row] = visuals.GetAppearance(frame.Draw, row) ??
                        throw new InvalidDataException($"Escape-gate visuals lack frame {frame.Draw}.");
                content.AppendName("frame", MotherBrainEscapeGateDraws.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });

    /// <summary>Resolves one escape-gate block's selected appearance independently of its compiled solid collision.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported escape-gate frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row ordinal is outside the four-block run.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customBlocks, PlmDraw draw, int blockIndex)
    {
        if (!MotherBrainEscapeGateDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Escape-gate visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
    }
}
