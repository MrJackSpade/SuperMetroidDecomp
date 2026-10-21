namespace SuperMetroid.Verification.Enemies.Spike;

/// <summary>An underwater reserve spike suit freeze and launch timing, and whether it retains the suit.</summary>
/// <param name="FreezeAfter">The frame the freeze starts after.</param>
/// <param name="LaunchAfter">The frame the launch follows after.</param>
/// <param name="RetainsSuit">Whether the suit is retained.</param>
internal readonly record struct SpikeSuitReserveCase(int FreezeAfter, int LaunchAfter, bool RetainsSuit);
