using System.Globalization;

namespace Fixture;

/// <summary>A palette phase and its name.</summary>
/// <param name="Phase">The phase.</param>
/// <param name="Name">The name.</param>
public sealed record PaletteName(int Phase, string Name);
