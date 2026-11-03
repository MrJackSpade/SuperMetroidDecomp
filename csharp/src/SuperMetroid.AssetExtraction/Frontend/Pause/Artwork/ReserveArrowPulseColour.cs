namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>One bevel colour of one phase of the reserve arrow's pulse.</summary>
/// <param name="Frame">The pulse phase.</param>
/// <param name="Bevel">The bevel whose colour it is.</param>
internal readonly record struct ReserveArrowPulseColour(int Frame, ReserveArrowBevel Bevel);
