namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire's mouth volley aims each shot.</summary>
public static class CrocomireVolley
{
    /// <summary>
    /// The gradient the volley's shot fired after <paramref name="shotsSpat"/> earlier ones is fired along: the authored
    /// gradient at its place in <see cref="CrocomireProjectileDefinitions.AuthoredGradients"/>, or, for the last shot,
    /// <paramref name="last"/>'s.
    /// </summary>
    /// <param name="shotsSpat">The shots the volley has spat before this one, 0 through one less than <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>.</param>
    /// <param name="last">The installed gradient the volley's last shot is fired along.</param>
    /// <exception cref="InvalidDataException">The volley has no such shot.</exception>
    public static int GradientOf(int shotsSpat, CrocomireTenthShotGradient last)
    {
        if (shotsSpat is < 0 or >= CrocomireProjectileDefinitions.VolleyShotCount)
            throw new InvalidDataException(
                $"Crocomire's volley has no shot after {shotsSpat}; it spits {CrocomireProjectileDefinitions.VolleyShotCount}.");
        return shotsSpat < CrocomireProjectileDefinitions.AuthoredGradients.Length
            ? CrocomireProjectileDefinitions.AuthoredGradients[shotsSpat]
            : Ensure.NotNull(last).Gradient;
    }
}
