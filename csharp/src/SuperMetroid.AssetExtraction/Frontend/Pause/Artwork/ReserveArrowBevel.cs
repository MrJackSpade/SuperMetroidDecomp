namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One of the reserve-tank arrow's two bevel colours, which its pulse animates in mirrored ramps.</summary>
internal enum ReserveArrowBevel
{
    /// <summary>The darker bevel colour while the arrow is solid; its pulse starts from the lighter solid colour.</summary>
    Shade,

    /// <summary>The lighter bevel colour while the arrow is solid; its pulse starts from the darker solid colour.</summary>
    Light,
}
