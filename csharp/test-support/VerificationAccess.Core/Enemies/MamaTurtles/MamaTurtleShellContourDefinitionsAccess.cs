
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Verification access to <see cref="MamaTurtleShellContourDefinitions"/> members production does not use.</summary>
internal static class MamaTurtleShellContourDefinitionsAccess
{
    /// <summary>Reads raw shell-contour offsets used by Mama Turtle collision comparisons.</summary>
    extension(MamaTurtleShellContourDefinitions)
    {
        /// <summary>Returns one raw word by native table index for cartridge parity checks.</summary>
        internal static short GetRawOffset(int index)
        {
            if ((uint)index >= MamaTurtleShellContourDefinitions.EntryCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return MamaTurtleShellContourDefinitions.Offsets[index];
        }
    }
}
