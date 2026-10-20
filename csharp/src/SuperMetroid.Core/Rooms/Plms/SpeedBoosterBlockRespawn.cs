namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Whether a crumbled speed-booster block comes back.</summary>
public enum SpeedBoosterBlockRespawn
{
    /// <summary>The block stays gone.</summary>
    Permanent,

    /// <summary>The block reappears after a delay.</summary>
    Respawns,
}
