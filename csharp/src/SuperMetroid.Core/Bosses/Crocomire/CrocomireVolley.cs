namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire's mouth volley aims each shot.</summary>
public static class CrocomireVolley
{
    /// <summary>
    /// Whether <paramref name="shot"/> is the volley's last, which reads its gradient past the authored ones from the
    /// installed <see cref="CrocomireTenthShotGradient"/>.
    /// </summary>
    /// <param name="shot">The shot's place in the volley, 1 through <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>.</param>
    public static bool IsLastShot(int shot) => shot == CrocomireProjectileDefinitions.VolleyShotCount;

    /// <summary>
    /// The authored gradient the volley's <paramref name="shot"/> is fired along: each shot reads the gradient its place in
    /// the volley names, so the shots before the last cycle through
    /// <see cref="CrocomireProjectileDefinitions.GradientCycle"/> from its second gradient.
    /// </summary>
    /// <param name="shot">The shot's place in the volley, 1 through <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>.</param>
    /// <exception cref="InvalidDataException">The shot is the last, or the volley has no such shot.</exception>
    public static int AuthoredGradient(int shot)
    {
        if (shot is < 1 or > CrocomireProjectileDefinitions.VolleyShotCount)
            throw new InvalidDataException(
                $"Crocomire's volley has no shot {shot}; it spits shots 1 through {CrocomireProjectileDefinitions.VolleyShotCount}.");
        if (IsLastShot(shot))
            throw new InvalidDataException("Crocomire's last shot reads its gradient past the authored ones.");
        return CrocomireProjectileDefinitions.GradientCycle[shot % CrocomireProjectileDefinitions.GradientCycle.Length];
    }
}
