using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Botwoon's wall block draws: which room-object draw is the wall's own clear, and the block shape it draws, from
/// <see cref="BotwoonWallObjectDrawDefinitions"/>. What it shows is the clear's installed picture (#1557).
/// </summary>
internal static class BotwoonWallObjectDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is the wall's clear.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The frame's block shape; null when the draw is not the wall's.</param>
    /// <returns>Whether the draw is the wall's own frame.</returns>
    /// <exception cref="InvalidOperationException">The draw is a wall frame with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (!RoomObjectDraw.TryFrame(request, out BotwoonWallDraw frame))
        {
            shape = null;
            return false;
        }
        shape = BotwoonWallObjectDrawDefinitions.Shapes.TryGetValue(frame, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(BotwoonWallDraw)} {frame}.");
        return true;
    }
}
