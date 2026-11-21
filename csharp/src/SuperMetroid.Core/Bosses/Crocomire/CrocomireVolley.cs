namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire's mouth volley aims each shot.</summary>
public static partial class CrocomireVolley
{
    /// <summary>
    /// The gradient the volley's shot fired after <paramref name="shotsSpat"/> earlier ones is fired along, while the volley
    /// has shots left: the authored gradient at its place in <see cref="CrocomireProjectileDefinitions.AuthoredGradients"/>,
    /// or, for the last shot, <paramref name="last"/>'s; once every shot is spat, none.
    /// </summary>
    /// <param name="shotsSpat">The shots the volley has spat, counted up from none as each is fired.</param>
    /// <param name="last">The installed gradient the volley's last shot is fired along.</param>
    /// <param name="gradient">The next shot's gradient, when the volley has one left.</param>
    /// <returns>Whether the volley has a shot left.</returns>
    public static bool TryNextGradient(int shotsSpat, CrocomireLastShotGradient last, out int gradient)
    {
        if (shotsSpat < CrocomireProjectileDefinitions.AuthoredGradients.Length)
        {
            gradient = CrocomireProjectileDefinitions.AuthoredGradients[shotsSpat];
            return true;
        }
        if (shotsSpat == CrocomireProjectileDefinitions.AuthoredGradients.Length)
        {
            gradient = Ensure.NotNull(last).Gradient;
            return true;
        }
        gradient = 0;
        return false;
    }
}
