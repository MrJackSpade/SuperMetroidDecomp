namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's body, which hangs below its balloon (#1472, #1519).</summary>
/// <param name="Contact">Whether Samus's touch and shots reach the body, as its placement says.</param>
internal sealed record PowampBodySpawnSettings(PowampContact Contact) : PowampSpawnSettings(Contact);
