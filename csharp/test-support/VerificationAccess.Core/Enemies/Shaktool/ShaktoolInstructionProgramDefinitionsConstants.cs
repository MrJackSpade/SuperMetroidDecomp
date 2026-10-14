
namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Cartridge values of <see cref="ShaktoolInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ShaktoolInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_Shaktool_Head_AimingDownLeft</c> at $AA:DADC.</summary>
    public const ushort HeadAimingDownLeft = 0xdadc;
    /// <summary><c>InstList_Shaktool_Head_AimingDownRight</c> at $AA:DACC.</summary>
    public const ushort HeadAimingDownRight = 0xdacc;
    /// <summary><c>InstList_Shaktool_Head_AimingRight</c> at $AA:DAC4.</summary>
    public const ushort HeadAimingRight = 0xdac4;
    /// <summary><c>InstList_Shaktool_Head_AimingUp</c> at $AA:DAB4.</summary>
    public const ushort HeadAimingUp = 0xdab4;
    /// <summary><c>InstList_Shaktool_Head_AimingUpLeft</c> at $AA:DAAC.</summary>
    public const ushort HeadAimingUpLeft = 0xdaac;
    /// <summary><c>InstList_Shaktool_Head_AimingUpRight</c> at $AA:DABC.</summary>
    public const ushort HeadAimingUpRight = 0xdabc;

    /// <summary>Provides Shaktool head-aiming programs for each native facing direction.</summary>
    extension(ShaktoolInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownLeft"/>
        internal static ushort HeadAimingDownLeft => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownLeft;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownRight"/>
        internal static ushort HeadAimingDownRight => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownRight;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingRight"/>
        internal static ushort HeadAimingRight => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingRight;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUp"/>
        internal static ushort HeadAimingUp => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUp;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpLeft"/>
        internal static ushort HeadAimingUpLeft => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpLeft;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpRight"/>
        internal static ushort HeadAimingUpRight => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpRight;
    }
}
