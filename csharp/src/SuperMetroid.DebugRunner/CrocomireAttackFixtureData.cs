/// <summary>Native and synthetic identities used by the Crocomire attack-entry regression.</summary>
internal static class CrocomireAttackFixtureData
{
    /// <summary>$A4:BB96, first timed spritemap in the five-record projectile volley loop.</summary>
    public const int VolleyTimedRecords = 0xa4bb96;
    /// <summary>Nonzero test-only live-shot marker; collision consumes it before any instruction fetch.</summary>
    public const ushort SyntheticLiveInstruction = 0x9000;
}
