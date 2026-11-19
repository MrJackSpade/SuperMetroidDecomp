namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The path a Choot swings along as it falls from the top of its jump, from its spawn settings: each swings out to the
/// right, back across to the left and drops (<see cref="ChootFallingPathDefinitions"/>).
/// </summary>
internal enum ChootFallingPath
{
    /// <summary>Swings 17 pixels right and 13 left, a sample each update.</summary>
    Normal,
    /// <summary>Swings 32 pixels right and 24 left, a sample each update.</summary>
    Wide,
    /// <summary>Swings 45 pixels right and 34 left, a sample each update.</summary>
    VeryWide,

    /// <summary>The normal path, holding its rightmost and leftmost samples longer.</summary>
    Slow,

    /// <summary>The normal path, holding its rightmost and leftmost samples longer still.</summary>
    VerySlow,
}
