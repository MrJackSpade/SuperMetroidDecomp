namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One timed colour record; the import pre-colours the sheets each shows from its installed colours.</summary>
/// <param name="Record">The record, in the order the programs show them.</param>
public readonly record struct PaletteFxFrame(int Record);
