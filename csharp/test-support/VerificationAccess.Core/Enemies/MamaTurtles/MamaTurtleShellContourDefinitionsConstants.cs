
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Cartridge values of <see cref="MamaTurtleShellContourDefinitions"/> that only verification reads.</summary>
internal static class MamaTurtleShellContourDefinitionsConstants
{
    /// <summary>Number of signed words in the two 24-pixel contour halves.</summary>
    public const int EntryCount = 48;
    /// <summary>First signed contour word at <c>$A2:8E80</c>.</summary>
    public const int SourceAddress = 0xa28e80;

    /// <summary>Provides the entry count and source address for Mama Turtle shell contour comparison.</summary>
    extension(MamaTurtleShellContourDefinitions)
    {
        /// <inheritdoc cref="MamaTurtleShellContourDefinitionsConstants.EntryCount"/>
        internal static int EntryCount => MamaTurtleShellContourDefinitionsConstants.EntryCount;
        /// <inheritdoc cref="MamaTurtleShellContourDefinitionsConstants.SourceAddress"/>
        internal static int SourceAddress => MamaTurtleShellContourDefinitionsConstants.SourceAddress;
    }
}
