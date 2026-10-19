using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the Ceres steam vents' programs (#641): the bank-$A6 spritemap each
/// <see cref="CeresSteamFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CeresSteamFrameDefinitions
{

    /// <summary>The installed frame each <see cref="CeresSteamFrame"/> shows.</summary>
    internal static EnemyFrameTable<CeresSteamFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.CeresSteamOamF142 },
        { new(Pose: 1), EnemyFrameId.CeresSteamOamF14C },
        { new(Pose: 10), EnemyFrameId.CeresSteamOamF1A6 },
        { new(Pose: 11), EnemyFrameId.CeresSteamOamF1B0 },
        { new(Pose: 12), EnemyFrameId.CeresSteamOamF1BA },
        { new(Pose: 13), EnemyFrameId.CeresSteamOamF1C4 },
        { new(Pose: 14), EnemyFrameId.CeresSteamOamF1CE },
        { new(Pose: 15), EnemyFrameId.CeresSteamOamF1D8 },
        { new(Pose: 16), EnemyFrameId.CeresSteamOamF1E2 },
        { new(Pose: 17), EnemyFrameId.CeresSteamOamF1EC },
        { new(Pose: 18), EnemyFrameId.CeresSteamOamF1F6 },
        { new(Pose: 19), EnemyFrameId.CeresSteamOamF200 },
        { new(Pose: 2), EnemyFrameId.CeresSteamOamF156 },
        { new(Pose: 20), EnemyFrameId.CeresSteamOamF20A },
        { new(Pose: 21), EnemyFrameId.CeresSteamOamF214 },
        { new(Pose: 22), EnemyFrameId.CeresSteamOamF21E },
        { new(Pose: 23), EnemyFrameId.CeresSteamOamF228 },
        { new(Pose: 24), EnemyFrameId.CeresSteamOamF232 },
        { new(Pose: 25), EnemyFrameId.CeresSteamOamF23C },
        { new(Pose: 26), EnemyFrameId.CeresSteamOamF246 },
        { new(Pose: 27), EnemyFrameId.CeresSteamOamF250 },
        { new(Pose: 3), EnemyFrameId.CeresSteamOamF160 },
        { new(Pose: 4), EnemyFrameId.CeresSteamOamF16A },
        { new(Pose: 5), EnemyFrameId.CeresSteamOamF174 },
        { new(Pose: 6), EnemyFrameId.CeresSteamOamF17E },
        { new(Pose: 7), EnemyFrameId.CeresSteamOamF188 },
        { new(Pose: 8), EnemyFrameId.CeresSteamOamF192 },
        { new(Pose: 9), EnemyFrameId.CeresSteamOamF19C },
    };
}
