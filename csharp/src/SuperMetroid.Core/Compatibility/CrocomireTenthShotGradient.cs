namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The gradient Crocomire's tenth spit shot is fired along (#1500). A volley fires up to nine shots, each aimed by the
/// gradient its place in the volley selects, and the gradients are authored for the first nine places; the volley
/// counts its shots from the second place, so its last shot selects the tenth place and the game reads its
/// gradient from the data that follows the nine. The import pipeline decodes that gradient once from the player's game
/// data; it aims the volley's last shot steeply, unlike any authored gradient.
/// </summary>
/// <remarks>
/// Consulted by the last shot of each volley on the update after it spawns, when the shot turns its aim into a velocity.
/// Evidence: the spit attack advances its shot counter by two from zero while it is below eighteen, spawning each shot
/// with the new count, and the shot's aiming routine indexes the nine-word gradient table with the count; the volley
/// check decodes the word after the table from the cartridge and compares it with this service.
/// </remarks>
[CompatibilityService("Crocomire's last spit shot is aimed by the gradient read past the volley's nine")]
public sealed class CrocomireTenthShotGradient
{
    /// <summary>Keeps the decoded gradient.</summary>
    /// <param name="gradient">The aim's vertical component, paired with 64 pixels to the left, -32768 through 32767.</param>
    /// <exception cref="ArgumentOutOfRangeException">The gradient lies outside -32768 through 32767.</exception>
    public CrocomireTenthShotGradient(int gradient) =>
        Gradient = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, gradient);

    /// <summary>The aim's vertical component, paired with 64 pixels to the left, -32768 through 32767.</summary>
    public int Gradient { get; }
}
