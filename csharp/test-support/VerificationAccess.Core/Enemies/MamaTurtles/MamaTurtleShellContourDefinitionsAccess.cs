
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Verification access to <see cref="MamaTurtleShellContourDefinitions"/> members production does not use.</summary>
internal static class MamaTurtleShellContourDefinitionsAccess
{
    /// <summary>Reads raw shell-contour offsets used by Mama Turtle collision comparisons.</summary>
    extension(MamaTurtleShellContourDefinitions)
    {
        /// <summary>
        /// Returns one offset by its place in the cartridge's table for cartridge parity checks: the table holds the left
        /// contour, then the right from distance zero. The right contour's word for distance zero is never read, an actor
        /// at her origin taking the left contour, so the port keeps no offset for it.
        /// </summary>
        internal static int GetRawOffset(int index)
        {
            if ((uint)index >= MamaTurtleShellContourDefinitions.EntryCount || index == MamaTurtleShellContourDefinitions.UnreadRightOfOriginWord)
                throw new ArgumentOutOfRangeException(nameof(index));
            return index < MamaTurtleShellContourDefinitions.HalfWidth
                ? MamaTurtleShellContourDefinitions.LeftOfOrigin[index]
                : MamaTurtleShellContourDefinitions.RightOfOrigin[index - MamaTurtleShellContourDefinitions.HalfWidth - 1];
        }

        /// <summary>The cartridge table's place of the right contour's word for distance zero, which nothing reads.</summary>
        internal static int UnreadRightOfOriginWord => MamaTurtleShellContourDefinitions.HalfWidth;
    }
}
