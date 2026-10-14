namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$AA animation lists used by Bomb and Golden Torizo reactions.</summary>
internal static class TorizoInstructionLists
{
    /// <summary>Bomb Torizo airborne list facing left at $AA:BC78.</summary>
    public const ushort AirborneFacingLeft = 0xbc78;
    /// <summary>Bomb Torizo alternate landing list facing right at $AA:BD0E.</summary>
    public const ushort AlternateLandingFacingRight = 0xbd0e;
    /// <summary>Bomb Torizo landing list facing left at $AA:BDD8.</summary>
    public const ushort LandingFacingLeft = 0xbdd8;
    /// <summary>Bomb Torizo landing list facing right at $AA:B962.</summary>
    public const ushort LandingFacingRight = 0xb962;
    /// <summary>Bomb Torizo airborne list facing right at $AA:C0F2.</summary>
    public const ushort AirborneFacingRight = 0xc0f2;
    /// <summary>Bomb Torizo alternate landing list facing left at $AA:C188.</summary>
    public const ushort AlternateLandingFacingLeft = 0xc188;
    /// <summary>InstList_GoldenTorizo_CaughtSuper_FacingLeft_LeftLegFwd at $AA:CDE1.</summary>
    public const ushort CaughtSuperFacingLeftLeftLegForward = 0xcde1;
    /// <summary>InstList_GoldenTorizo_CaughtSuper_FacingLeft_RightLegFwd at $AA:CE43.</summary>
    public const ushort CaughtSuperFacingLeftRightLegForward = 0xce43;
    /// <summary>InstList_GoldenTorizo_CaughtSuper_FacingRight_RightLegFwd at $AA:CEA5.</summary>
    public const ushort CaughtSuperFacingRightRightLegForward = 0xcea5;
    /// <summary>InstList_GoldenTorizo_CaughtSuper_FacingRight_LeftLegFwd at $AA:CEFF.</summary>
    public const ushort CaughtSuperFacingRightLeftLegForward = 0xceff;
    /// <summary>Golden Torizo caught-missile reaction facing left at $AA:D1F1.</summary>
    public const ushort CaughtMissileFacingLeft = 0xd1f1;
    /// <summary>Golden Torizo caught-missile reaction facing right at $AA:D2AD.</summary>
    public const ushort CaughtMissileFacingRight = 0xd2ad;
    /// <summary>Golden Torizo turning-left list at $AA:D203; this landing-path alias is retained for callers.</summary>
    public const ushort GoldenLandingFacingRight = 0xd203;
    /// <summary>Golden Torizo turning-right list at $AA:D2BF; this landing-path alias is retained for callers.</summary>
    public const ushort GoldenLandingFacingLeft = 0xd2bf;
}
