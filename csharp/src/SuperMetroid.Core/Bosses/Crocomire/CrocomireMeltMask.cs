using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// What Crocomire's melt image hides at a step of its dissolve (#1652): each dissolving cell of the image hides the pixel
/// columns of its group that the steps its band last showed have erased. The image is stored once and shown through this.
/// </summary>
internal static class CrocomireMeltMask
{
    /// <summary>
    /// The pixels <paramref name="image"/>'s picture hides once each band has shown the step <paramref name="bandSteps"/>
    /// gives it: step <c>s</c> erases pixel <c>s</c> modulo eight of group <see cref="CrocomireMeltDissolveDefinitions.ColumnOrder"/>
    /// <c>[s]</c> divided by eight, in every band the dissolve erases.
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame shows no picture, or a band's step is not one of the dissolve's.</exception>
    internal static HiddenPictureColumns Of(BossBodyFrame image, ReadOnlySpan<int> bandSteps)
    {
        SpriteSheetFrame picture = image.Picture
            ?? throw new InvalidOperationException($"Crocomire's melt image {image.Name} shows no picture.");
        int columns = picture.Size.X / HiddenPictureColumns.CellPixels, rows = picture.Size.Y / HiddenPictureColumns.CellPixels;
        var cells = new CellColumns[columns * rows];
        ReadOnlySpan<byte> order = CrocomireMeltDissolveDefinitions.ColumnOrder;
        foreach (DissolvingCell cell in image.Dissolving)
        {
            if (cell.Band >= CrocomireMeltDissolveDefinitions.ErasedBands)
                continue;
            int steps = bandSteps[cell.Band];
            if ((uint)steps > (uint)order.Length)
                throw new InvalidOperationException($"Crocomire's melt band {cell.Band} shows step {steps}, which the dissolve does not take.");
            CellColumns hidden = CellColumns.None;
            for (int step = 0; step < steps; step++)
            {
                if (order[step] / CrocomireMeltDissolveDefinitions.GroupColumns == cell.Group)
                    hidden |= HiddenPictureColumns.Columns[step % CrocomireMeltDissolveDefinitions.GroupColumns];
            }
            cells[(cell.Row * columns) + cell.Column] = hidden;
        }
        return new HiddenPictureColumns(columns, rows, ImmutableArray.Create(cells));
    }
}
