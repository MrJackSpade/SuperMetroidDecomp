using static SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockPlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The crumble draw list and artwork names of the Speed Booster blocks.</summary>
internal static class SpeedBoosterBlockPlmDraws
{
    /// <summary>The shared one-block crumble draw for crumble image <paramref name="frame"/> (0..3).</summary>
    internal static PlmDraw CrumbleDrawList(int frame) =>
        ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, frame);

    /// <summary>Accepts only the bomb-reveal draw pointer and rejects unrelated PLM drawings.</summary>
    internal static string VisualId(PlmDraw request) => request == BombRevealDraw
        ? BombRevealVisualId
        : throw new InvalidDataException($"Speed Booster draw {request} has no visual ID.");

    /// <summary>Resolves the one exported visual identity to its calculated draw record.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId([BombReveal], _ => BombRevealVisualId, id, out list);
}
