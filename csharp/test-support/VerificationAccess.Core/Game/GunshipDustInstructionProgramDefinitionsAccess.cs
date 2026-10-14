using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GunshipDustInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GunshipDustInstructionProgramDefinitionsAccess
{
    /// <summary>Selects the gunship dust animation entry from its native spawn parameter.</summary>
    extension(GunshipDustInstructionProgramDefinitions)
    {
        /// <summary>Maps the even dust variant parameter to one of six native entry programs.</summary>
        internal static ushort InitialForParameter(ushort parameter) => parameter switch
        {
            0 => GunshipDustInstructionProgramDefinitions.Index0,
            2 => GunshipDustInstructionProgramDefinitions.Index2,
            4 => GunshipDustInstructionProgramDefinitions.Index4,
            6 => GunshipDustInstructionProgramDefinitions.Index6,
            8 => GunshipDustInstructionProgramDefinitions.Index8,
            10 => GunshipDustInstructionProgramDefinitions.IndexA,
            _ => throw new ArgumentOutOfRangeException(
                "parameter", parameter, "Gunship dust parameter must be 0,2,4,6,8,A."),
        };
    }
}
