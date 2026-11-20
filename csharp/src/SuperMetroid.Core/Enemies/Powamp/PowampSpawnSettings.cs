namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>What a Powamp placement is, its balloon or its body, and whether the half refuses Samus's touch and shots (#1472, #1519, #2474).</summary>
/// <param name="Contact">Whether Samus's touch and shots reach the half, as its placement says.</param>
internal abstract record PowampSpawnSettings(PowampContact Contact) : EnemySpawnSettings;
