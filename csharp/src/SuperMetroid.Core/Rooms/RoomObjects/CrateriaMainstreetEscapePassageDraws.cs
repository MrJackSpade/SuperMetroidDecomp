using System.Diagnostics.CodeAnalysis;
using static SuperMetroid.Core.Rooms.RoomObjects.CrateriaMainstreetEscapePassageObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The open-passage draw of Crateria's main street escape and its block shape (#1862).</summary>
internal static class CrateriaMainstreetEscapePassageDraws
{
    /// <summary>The passage's open blocks when <paramref name="request"/> is the passage's draw.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The two open blocks; null when the draw is not the passage's.</param>
    /// <returns>Whether the draw is the passage's.</returns>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        shape = request == ClearPairDraw ? OpenPassage : null;
        return shape is not null;
    }
}
