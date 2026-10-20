namespace SuperMetroid.Core.Enemies;

/// <summary>Whether a lethal touch plays the generic death animation.</summary>
internal enum TouchDeathAnimation
{
    /// <summary>The generic death animation plays.</summary>
    Play,

    /// <summary>The species handles the death itself.</summary>
    Skip,
}
