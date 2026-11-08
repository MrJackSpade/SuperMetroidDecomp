
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Verification access to <see cref="MamaTurtleShellContourDefinitions"/> members production does not use.</summary>
internal static class MamaTurtleShellContourDefinitionsAccess
{
    /// <summary>Reads raw shell-contour offsets used by Mama Turtle collision comparisons.</summary>
    extension(MamaTurtleShellContourDefinitions)
    {
        /// <summary>Returns one offset by its table index for cartridge parity checks.</summary>
        internal static int GetRawOffset(int index)
        {
            if ((uint)index >= MamaTurtleShellContourDefinitions.EntryCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return MamaTurtleShellContourDefinitions.Offsets[index];
        }
    }
}
