namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One oscillation of the hidden wall's rumble (#1879): the wall's vertical offset swings between a return offset and
/// an extreme above it.
/// </summary>
/// <param name="ReturnOffset">The offset each swing starts from and returns to, in pixels; positive is down.</param>
/// <param name="ExtremeOffset">The offset each swing reaches, in pixels; negative is up.</param>
/// <param name="Repeats">How many more swings follow the first before the next oscillation starts.</param>
/// <param name="Speed">The pixels the offset moves each update toward the offset it is heading for.</param>
internal readonly record struct CrocomireRumbleOscillation(int ReturnOffset, int ExtremeOffset, int Repeats, int Speed);
