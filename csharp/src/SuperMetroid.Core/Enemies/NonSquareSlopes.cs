namespace SuperMetroid.Core.Enemies;

/// <summary>How horizontal enemy movement treats non-square slopes.</summary>
internal enum NonSquareSlopes
{
    /// <summary>They do not block movement.</summary>
    Ignored,

    /// <summary>They block movement like walls.</summary>
    Walls,
}
