using Fixture.Shapes;

namespace Fixture.Shared;

/// <summary>
/// A file both projects compile. Neither needs <c>System.Text</c>, so it goes. Fixture.csproj imports
/// <c>Fixture.Shapes</c> globally and reports its using here as unnecessary, but Other.csproj needs it, so it is no
/// finding and stays.
/// </summary>
internal static class Corners
{
    /// <summary>The corners of a square.</summary>
    internal static int Of(Square square) => square.Corners;
}
