namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What Phantoon's mouth tells the blending owner to do with Phantoon's layer while Phantoon is not semi-transparent.
/// </summary>
public enum PhantoonLayerControl
{
    /// <summary>Hide Phantoon's layer: the state a part's initialization and a kill leave.</summary>
    Hidden = 1,

    /// <summary>Leave the room's own blending, as the wavy fade-in and the first round set.</summary>
    RoomDefault,

    /// <summary>Hide Phantoon's layer and remove the blending owner for good, as the death graphics clear sets.</summary>
    Removed,
}
