namespace SuperMetroid.Core.Fx;

using SuperMetroid.Core.Progression;

/// <summary>Cartridge values of <see cref="AreaBlockAnimationDefinitions"/> that only verification reads.</summary>
internal static class AreaBlockAnimationDefinitionsConstants
{
    /// <summary>
    /// The cartridge stores an eighth non-retail list after the seven typed areas.
    /// Production <see cref="AreaBlockAnimations.Read(AreaId, int)"/> deliberately accepts only typed retail
    /// areas, while verification retains this row so every native table word is audited.
    /// </summary>
    public const int NativeAreaCount = 8;
    /// <summary>
    /// Native <c>kArea_AnimtilesObjectListPtrs</c> at <c>$83:AC56</c>; eight words point
    /// to the eight native area lists below.
    /// </summary>
    public const int NativeListPointerTable = 0x83ac56;

    /// <summary>Provides the cartridge area count and block-animation list table location used to check area imports.</summary>
    extension(AreaBlockAnimationDefinitions)
    {
        /// <inheritdoc cref="AreaBlockAnimationDefinitionsConstants.NativeAreaCount"/>
        internal static int NativeAreaCount => AreaBlockAnimationDefinitionsConstants.NativeAreaCount;
        /// <inheritdoc cref="AreaBlockAnimationDefinitionsConstants.NativeListPointerTable"/>
        internal static int NativeListPointerTable => AreaBlockAnimationDefinitionsConstants.NativeListPointerTable;
    }
}
