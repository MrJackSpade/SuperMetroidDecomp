namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Whether a Ki-Hunter starts without its wings (#1472).</summary>
/// <param name="Wingless">Whether it starts without wings; its wing placement starts deleted.</param>
internal sealed record KiHunterSpawnSettings(bool Wingless) : EnemySpawnSettings;
