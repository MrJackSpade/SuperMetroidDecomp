
namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Cartridge values of <see cref="SbugInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class SbugInstructionProgramDefinitionsConstants
{
    /// <summary><c>$A3:A0E9</c>, down-facing animation loop.</summary>
    public const ushort Down = 0xa0e9;
    /// <summary><c>$A3:A0D5</c>, down-left-facing animation loop.</summary>
    public const ushort DownLeft = 0xa0d5;
    /// <summary><c>$A3:A0FD</c>, down-right-facing animation loop.</summary>
    public const ushort DownRight = 0xa0fd;
    /// <summary><c>$A3:A0C1</c>, left-facing animation loop.</summary>
    public const ushort Left = 0xa0c1;
    /// <summary><c>$A3:A099</c>, up-facing animation loop.</summary>
    public const ushort Up = 0xa099;
    /// <summary><c>$A3:A0AD</c>, up-left-facing animation loop.</summary>
    public const ushort UpLeft = 0xa0ad;
    /// <summary><c>$A3:A085</c>, up-right-facing animation loop.</summary>
    public const ushort UpRight = 0xa085;

    /// <summary>Provides directional Sbug instruction programs for native movement animation checks.</summary>
    extension(SbugInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.Down"/>
        internal static ushort Down => SbugInstructionProgramDefinitionsConstants.Down;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.DownLeft"/>
        internal static ushort DownLeft => SbugInstructionProgramDefinitionsConstants.DownLeft;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.DownRight"/>
        internal static ushort DownRight => SbugInstructionProgramDefinitionsConstants.DownRight;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.Left"/>
        internal static ushort Left => SbugInstructionProgramDefinitionsConstants.Left;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.Up"/>
        internal static ushort Up => SbugInstructionProgramDefinitionsConstants.Up;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.UpLeft"/>
        internal static ushort UpLeft => SbugInstructionProgramDefinitionsConstants.UpLeft;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.UpRight"/>
        internal static ushort UpRight => SbugInstructionProgramDefinitionsConstants.UpRight;
    }
}
