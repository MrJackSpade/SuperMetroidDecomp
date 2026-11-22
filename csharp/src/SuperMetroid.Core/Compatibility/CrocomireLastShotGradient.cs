namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The gradient Crocomire's last spit shot is fired along (#1500, #2464). A volley fires nine shots; the first eight are
/// aimed by the gradients authored for them (<see cref="Bosses.Crocomire.CrocomireProjectileDefinitions.AuthoredGradients"/>),
/// and the game reads the last shot's gradient from the data that follows them. The import pipeline decodes that gradient
/// once from the player's game data; it aims the volley's last shot steeply, unlike any authored gradient.
/// </summary>
/// <remarks>
/// Consulted as the last shot of each volley spawns, which keeps the gradient its aim turns into a velocity on the update after.
/// Evidence: the spit attack advances its shot counter by two from zero while it is below eighteen, spawning each shot
/// with the new count, and the shot's aiming routine indexes the nine-word gradient table with the count; the volley
/// check decodes the word after the table from the cartridge and compares it with this service.
/// </remarks>
[CompatibilityService("Crocomire's last spit shot is aimed by the gradient read past the volley's nine")]
public sealed partial class CrocomireLastShotGradient
{
    /// <summary>Keeps the decoded gradient.</summary>
    /// <param name="gradient">The aim's vertical component, paired with 64 pixels to the left, -32768 through 32767.</param>
    /// <exception cref="ArgumentOutOfRangeException">The gradient lies outside -32768 through 32767.</exception>
    public CrocomireLastShotGradient(int gradient) =>
        Gradient = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, gradient);

    /// <summary>The aim's vertical component, paired with 64 pixels to the left, -32768 through 32767.</summary>
    public int Gradient { get; }
}
