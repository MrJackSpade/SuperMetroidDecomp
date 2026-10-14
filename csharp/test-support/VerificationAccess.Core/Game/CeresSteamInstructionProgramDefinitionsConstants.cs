using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="CeresSteamInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class CeresSteamInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>InstList_CeresSteam_Down_2</c> at $A6:F0C9; seven three-tick
    /// frames loop to the hidden hold at $F0C1.
    /// </summary>
    public const ushort DownActive = 0xf0c9;
    /// <summary>
    /// <c>InstList_CeresSteam_Left_2</c> at $A6:F095; seven three-tick frames
    /// loop to the local hidden hold at $F08D.
    /// </summary>
    public const ushort LeftActive = 0xf095;
    /// <summary>
    /// <c>InstList_CeresSteam_Right_2</c> at $A6:F0FD; seven three-tick
    /// frames loop to the hidden hold at $F0F5.
    /// </summary>
    public const ushort RightActive = 0xf0fd;
    /// <summary>
    /// <c>InstList_CeresSteam_Up_2</c> at $A6:F061 runs seven three-tick
    /// frames and returns to the hidden hold at $F059. The interpreter thus
    /// traverses only the three local states $F04D, $F059, and $F061.
    /// </summary>
    public const ushort UpActive = 0xf061;

    /// <summary>Provides the four active Ceres steam instruction programs used to verify directional variants.</summary>
    extension(CeresSteamInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.DownActive"/>
        internal static ushort DownActive => CeresSteamInstructionProgramDefinitionsConstants.DownActive;
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.LeftActive"/>
        internal static ushort LeftActive => CeresSteamInstructionProgramDefinitionsConstants.LeftActive;
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.RightActive"/>
        internal static ushort RightActive => CeresSteamInstructionProgramDefinitionsConstants.RightActive;
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.UpActive"/>
        internal static ushort UpActive => CeresSteamInstructionProgramDefinitionsConstants.UpActive;
    }
}
