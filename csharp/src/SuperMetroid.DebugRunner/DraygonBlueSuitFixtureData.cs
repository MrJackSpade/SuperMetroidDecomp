/// <summary>Cartridge identities and constructed program sentinels for the lethal eye-hit fixture.</summary>
internal static class DraygonBlueSuitFixtureData
{
    /// <summary>Bank $A5 extended spritemap at $A3BB, with the vulnerable eye rectangle at body center.</summary>
    public const ushort EyeHitSpritemap = 0xa3bb;

    /// <summary>Nonzero $9000 instruction sentinel for an active projectile; collision-only probing never executes it.</summary>
    public const ushort UnexecutedProjectileProgram = 0x9000;
}
