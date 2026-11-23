namespace SuperMetroid.Core.Fx;

/// <summary>What a special-air block's sand does to Samus inside it.</summary>
internal enum SpecialAirInsideSand
{
    /// <summary>The block holds no sand.</summary>
    None,

    /// <summary>Maridia's quicksand surface: it cancels running momentum and holds or sinks Samus by her vertical direction.</summary>
    Surface,

    /// <summary>Submerging quicksand or a sandfall, which pulls Samus down.</summary>
    Sinks,
}
