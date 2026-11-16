namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A Rinka's life (#2171): how long it waits before it flies at Samus, how fast it flies and the health it returns with.</summary>
internal static class RinkaLifeDefinitions
{
    /// <summary>The updates a newly fired Rinka waits before it aims at Samus, on the update after them.</summary>
    internal const int AimDelay = 26;

    /// <summary>
    /// The updates a killed special Rinka, one of Mother Brain's spawn-point Rinkas, waits hidden before it respawns, on the
    /// update after them.
    /// </summary>
    internal const int RespawnDelay = 1;

    /// <summary>The health a special Rinka, one of Mother Brain's spawn-point Rinkas, returns with after its death delay.</summary>
    internal const int RespawnHealth = 10;

    /// <summary>The speed a Rinka flies at along its aim: a pixel and an eighth an update.</summary>
    internal static readonly SignedEightEight Speed = SignedEightEight.FromParts(1, 32);
}
