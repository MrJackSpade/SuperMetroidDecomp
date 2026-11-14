namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What an eruption step does to the collision part's width. Its values are arbitrary.</summary>
internal enum HibashiStepWidth
{
    /// <summary>The collision part keeps the half-width it has.</summary>
    Kept,

    /// <summary>The collision part gets its eight-pixel half-width back, as the eruption's first step does.</summary>
    Restored,
}
