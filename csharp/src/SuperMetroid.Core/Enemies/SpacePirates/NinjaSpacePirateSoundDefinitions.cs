namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The enemy-library sounds the ninja Space Pirates' animation cues play.</summary>
internal static class NinjaSpacePirateSoundDefinitions
{
    /// <summary>The Space Pirate attack sound, played as a claw is thrown and as a divekick descends.</summary>
    internal static readonly SoundEffectId Attack = EnemySound.ShotSpacePirate;

    /// <summary>The spin-jump whirl, played at the start of each spin loop.</summary>
    internal static readonly SoundEffectId SpinJump = EnemySound.AlcoonSpit;
}
