namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The enemy-library sounds the Sidehoppers' programs queue.</summary>
internal static class HopperSoundDefinitions
{
    /// <summary>The hop sound, queued as a Sidehopper's jumping program starts.</summary>
    internal static readonly SoundEffectId Hop = EnemySound.SidehopperJump;

    /// <summary>The landing sound, queued as a Sidehopper's landed program starts.</summary>
    internal static readonly SoundEffectId Land = EnemySound.SidehopperLanding;
}
