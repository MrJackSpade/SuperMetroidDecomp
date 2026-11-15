namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The five Choot falling paths.
/// Its values are arbitrary.
/// </summary>
internal enum ChootFallingPath
{
    /// <summary>Normal falling motion.</summary>
    Normal,
    /// <summary>Wide falling motion.</summary>
    Wide,
    /// <summary>Very wide falling motion.</summary>
    VeryWide,

    /// <summary>The normal path, holding its rightmost and leftmost samples eleven updates each.</summary>
    Slow,

    /// <summary>The normal path, holding its rightmost and leftmost samples thirty-one updates each.</summary>
    VerySlow,
}
