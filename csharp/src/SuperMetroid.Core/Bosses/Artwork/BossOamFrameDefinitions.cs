namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>Selected Ridley, Draygon and Spore Spawn OAM roots calculated from
/// extended-record geometry: two count bytes plus eight bytes per component.</summary>
internal static class BossOamFrameDefinitions
{
    /// <summary>Side-facing Ridley body frames in this visual catalog.</summary>
    internal const int RidleyFrameCount = 11;
    /// <summary>Draygon OAM component frames, separate from its BG2-only poses.</summary>
    internal const int DraygonOamFrameCount = 48;
    /// <summary>Spore Spawn OAM frames selected by its animation instructions.</summary>
    internal const int SporeSpawnOamFrameCount = 12;
}
