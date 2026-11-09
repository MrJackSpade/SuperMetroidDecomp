using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo statue's slope access opening or closing (#1554): it draws the access once and, as it leaves
/// the room on its second update, turns the statue room's spikes into slopes or its slopes back into spikes.
/// </summary>
internal sealed class ChozoSlopeAccessObject : TerrainChangeObject
{
    /// <summary>The terrain change the access makes to the statue room's slope as it ends.</summary>
    private readonly Action<RoomLevelData> _slopeChange;

    /// <summary>Creates the access with its drawn change and its slope change.</summary>
    private ChozoSlopeAccessObject(TerrainChange change, Action<RoomLevelData> slopeChange) : base(change) =>
        _slopeChange = slopeChange;

    /// <summary>The access opening: the spikes become slopes.</summary>
    internal static ChozoSlopeAccessObject Clearing() =>
        new(ChozoStatueObjectDefinitions.ClearSlopeAccess, ChozoStatueObjects.TransformSpikesToSlopes);

    /// <summary>The access closing: the slopes become spikes again.</summary>
    internal static ChozoSlopeAccessObject Blocking() =>
        new(ChozoStatueObjectDefinitions.BlockSlopeAccess, ChozoStatueObjects.RevertSlopesToSpikes);

    /// <inheritdoc/>
    private protected override void End(RoomObjectStepContext context) => _slopeChange(context.Level);
}
