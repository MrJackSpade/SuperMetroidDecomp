/// <summary>Native bank-$90 frame-handler identities used by the reserve CPU comparison.</summary>
internal static class ReserveHandlerTraceDefinitions
{
    /// <summary>$90:E713, Samus_FrameHandlerAlfa_Func13, command-zero locked alpha.</summary>
    public const int LockedAlpha = 0xe713;
    /// <summary>$90:E695, Samus_FrameHandlerAlfa_Func11, normal gameplay alpha.</summary>
    public const int NormalAlpha = 0xe695;
    /// <summary>$90:E8DC, SetContactDamageIndexAndUpdateMinimap, stationary beta without animation.</summary>
    public const int StationaryBeta = 0xe8dc;
    /// <summary>$90:E725, Samus_FrameHandlerBeta_Func17, normal movement/animation beta.</summary>
    public const int NormalBeta = 0xe725;
}
