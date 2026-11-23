using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import's reading of a room-object draw's identity (#2034): the installed frame files key each family's pictures by
/// the draw, and the import tells which family's frame a draw is when it lists, names and checks the frames. The game draws
/// each frame through its own family and never asks.
/// </summary>
internal static partial class RoomObjectDrawIdentity
{
    /// <summary>Whether <paramref name="draw"/> is a frame of <typeparamref name="TFrame"/>, and that frame.</summary>
    internal static bool TryFrame<TFrame>(RoomObjectDraw draw, out TFrame frame) where TFrame : struct
    {
        if (draw is RoomObjectFrameDraw<TFrame> { Frame: var found })
        {
            frame = found;
            return true;
        }
        frame = default;
        return false;
    }

    /// <summary>The frame of <paramref name="draw"/>, which must be one of <typeparamref name="TFrame"/>'s.</summary>
    /// <param name="draw">The draw.</param>
    /// <param name="family">The family's name, for the rejection.</param>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is another family's.</exception>
    internal static TFrame FrameOf<TFrame>(RoomObjectDraw draw, string family) where TFrame : struct =>
        TryFrame(draw, out TFrame frame) ? frame : throw new InvalidDataException($"{draw} is not a {family} draw.");

    /// <summary>Whether <paramref name="draw"/> is one of <paramref name="family"/>'s frames, and the block shape it draws.</summary>
    internal static bool TryShape<TFrame>(RoomObjectDrawFamily<TFrame> family, RoomObjectDraw draw, [MaybeNullWhen(false)] out RoomBlockShape shape)
        where TFrame : struct
    {
        Ensure.NotNull(family);
        if (draw is RoomObjectFrameDraw<TFrame> typed && typed.Family == family)
        {
            shape = family.Shape(typed.Frame);
            return true;
        }
        shape = null;
        return false;
    }
}
