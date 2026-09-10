/// <summary>Measured 16.16 positions from the pinned original-CPU crouch-jump fixture.</summary>
internal static class CrouchJumpFixtureData
{
    /// <summary>Standing center on the synthetic row-48 floor after its grounding scan.</summary>
    public const uint StandingLaunchY = 0x02ebffff;

    /// <summary>Normal-jump apex in air, also reached with Gravity Suit underwater.</summary>
    private const uint AirApex = 0x027ce7ff;

    /// <summary>Hi-Jump apex in air, also reached with Gravity Suit underwater.</summary>
    private const uint AirHiJumpApex = 0x02446bff;

    /// <summary>Normal-jump apex while submerged without Gravity Suit.</summary>
    private const uint WaterApex = 0x02ba1fff;

    /// <summary>Hi-Jump apex while submerged without Gravity Suit.</summary>
    private const uint WaterHiJumpApex = 0x0286bfff;

    public static uint StandingApex(bool underwaterPhysics, bool hiJump) =>
        (underwaterPhysics, hiJump) switch
        {
            (false, false) => AirApex,
            (false, true) => AirHiJumpApex,
            (true, false) => WaterApex,
            (true, true) => WaterHiJumpApex,
        };
}
