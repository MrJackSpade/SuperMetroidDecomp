using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The five one-block draw lists used by breakable-Grapple-block PLMs. These are
/// complete native level words, not merely tile references: the initial frame
/// installs Grapple collision and the later frames install air collision.
/// </summary>
internal static class RoomPlmGrappleBlockDrawDefinitions
{

    /// <summary>A single Grapple-block draw with the level word it installs.</summary>
    internal readonly record struct DrawList(GrappleBlockDraw Frame, ushort LevelWord)
    {
        /// <summary>Native one-word row, followed by signed zero X/Y termination.</summary>
        internal const ushort DirectionAndCount = 1;
        /// <summary>The one-block record leaves the next draw position in its current column.</summary>
        internal const sbyte NextX = 0;
        /// <summary>The one-block record leaves the next draw position in its current row.</summary>
        internal const sbyte NextY = 0;
    }

    /// <summary>Enumerates the initial Grapple block, three breakup frames, and blank air in native order.</summary>
    internal static IEnumerable<DrawList> All
    {
        get
        {
            foreach (GrappleBlockDraw frame in Enum.GetValues<GrappleBlockDraw>())
                yield return new(frame, LevelWord(frame).Raw);
        }
    }

    /// <summary>
    /// The level word a draw installs: the Grapple block, the three consecutive breakup tiles as air, and blank air.
    /// </summary>
    private static RoomLevelWord LevelWord(GrappleBlockDraw frame) => frame switch
    {
        GrappleBlockDraw.Intact => RoomLevelWord.Create(0x0b7, LevelBlockFlipFlags.None, RoomCollisionType.GrappleBlock),
        GrappleBlockDraw.Break0 => RoomLevelWord.Create(0x053, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        GrappleBlockDraw.Break1 => RoomLevelWord.Create(0x054, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        GrappleBlockDraw.Break2 => RoomLevelWord.Create(0x055, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        GrappleBlockDraw.Blank => RoomLevelWord.Create(0x0ff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        _ => throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}."),
    };

    /// <summary>The published artwork identity of a Grapple-block draw.</summary>
    internal static string VisualId(GrappleBlockDraw frame) => frame switch
    {
        GrappleBlockDraw.Intact => "intact",
        GrappleBlockDraw.Break0 => "break-0",
        GrappleBlockDraw.Break1 => "break-1",
        GrappleBlockDraw.Break2 => "break-2",
        GrappleBlockDraw.Blank => "blank",
        _ => throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}."),
    };

    /// <summary>Finds the Grapple-block draw whose artwork identity is <paramref name="id"/>.</summary>
    internal static bool TryGetByVisualId(string? id, out DrawList draw) =>
        PlmDraw.TryFindByVisualId(All, candidate => VisualId(candidate.Frame), id, out draw);

    /// <summary>Resolves one of the five Grapple-block draws; draws of other families produce a default record.</summary>
    internal static bool TryGet(PlmDraw request, out DrawList definition)
    {
        if (request is not PlmFrameDraw<GrappleBlockDraw> { Frame: var frame })
        {
            definition = default;
            return false;
        }
        definition = new(frame, LevelWord(frame).Raw);
        return true;
    }
}
