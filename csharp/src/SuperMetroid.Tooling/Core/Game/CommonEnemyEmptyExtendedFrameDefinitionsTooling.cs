namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CommonEnemyEmptyExtendedFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitionsTooling
{
    /// <summary>Enemy banks carrying the common $804D/$804F/$8059 frame records.</summary>
    internal static IEnumerable<byte> SupportedBanks
    {
        get
        {
            yield return 0xa0;
            for (byte bank = 0xa2; bank <= 0xaa; bank++) yield return bank;
            for (byte bank = 0xb2; bank <= 0xb3; bank++) yield return bank;
        }
    }
}
