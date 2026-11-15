
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Verification access to <see cref="MamaTurtleShellContourDefinitions"/> members production does not use.</summary>
internal static class MamaTurtleShellContourDefinitionsAccess
{
    /// <summary>Reads raw shell-contour offsets used by Mama Turtle collision comparisons.</summary>
    extension(MamaTurtleShellContourDefinitions)
    {
        /// <summary>
        /// Returns one offset by its place in the cartridge's table for cartridge parity checks: the table holds the left
        /// contour, then the right.
        /// </summary>
        internal static int GetRawOffset(int index)
        {
            if ((uint)index >= MamaTurtleShellContourDefinitions.EntryCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return index < MamaTurtleShellContourDefinitions.HalfWidth
                ? MamaTurtleShellContourDefinitions.LeftOfOrigin[index]
                : MamaTurtleShellContourDefinitions.RightOfOrigin[index - MamaTurtleShellContourDefinitions.HalfWidth];
        }
    }
}
