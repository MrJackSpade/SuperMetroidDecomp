namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>A door or item location and the number room data gives it.</summary>
/// <typeparam name="T">The kind of placement.</typeparam>
/// <param name="Number">The number room data and the saved tables give the placement.</param>
/// <param name="Placement">The placement.</param>
internal sealed record NumberedPlacement<T>(int Number, T Placement)
    where T : class;
