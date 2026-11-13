namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The enemy-library sounds the Sidehoppers' animations queue.</summary>
internal static class HopperSoundDefinitions
{
    /// <summary>The hop sound, queued as a Sidehopper's jumping animation starts.</summary>
    internal static readonly SoundEffectId Hop = EnemySound.SidehopperJump;

    /// <summary>The landing sound, queued as a Sidehopper's landed animation starts.</summary>
    internal static readonly SoundEffectId Land = EnemySound.SidehopperLanding;
}
