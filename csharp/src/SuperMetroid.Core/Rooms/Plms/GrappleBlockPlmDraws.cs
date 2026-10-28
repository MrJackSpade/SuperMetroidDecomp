using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmGrappleBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The level word and artwork name of each Grapple-block draw.</summary>
internal static class GrappleBlockPlmDraws
{
    /// <summary>
    /// The level word a draw installs: the Grapple block, the three consecutive breakup tiles as air, and blank air.
    /// </summary>
    private static RoomLevelWord LevelWord(GrappleBlockDraw frame) =>
        LevelWords.TryGetValue(frame, out RoomLevelWord value) ? value : throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}.");

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

    /// <summary>Enumerates the initial Grapple block, three breakup frames, and blank air in native order.</summary>
    internal static IEnumerable<DrawList> All
    {
        get
        {
            foreach (GrappleBlockDraw frame in Enum.GetValues<GrappleBlockDraw>())
                yield return new(frame, LevelWord(frame));
        }
    }
}
