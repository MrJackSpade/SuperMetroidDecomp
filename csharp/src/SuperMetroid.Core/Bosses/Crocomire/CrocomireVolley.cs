namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire's mouth volley aims each shot.</summary>
public static class CrocomireVolley
{
    /// <summary>
    /// Whether a shot's spawn parameter selects the volley's tenth shot, whose gradient is read past the nine authored
    /// ones from the installed <see cref="CrocomireTenthShotGradient"/>. The fight emits only 2, 4, ..., 18.
    /// </summary>
    /// <param name="parameter">The shot's spawn parameter.</param>
    public static bool IsTenthShot(int parameter) => parameter / 2 == CrocomireProjectileDefinitions.AuthoredShotCount;

    /// <summary>
    /// The authored gradient a shot's spawn parameter selects: the parameter halved indexes the volley, whose first nine
    /// shots cycle through <see cref="CrocomireProjectileDefinitions.GradientCycle"/>.
    /// </summary>
    /// <param name="parameter">The shot's spawn parameter.</param>
    /// <exception cref="InvalidDataException">The parameter selects the tenth shot or a later one.</exception>
    public static int AuthoredGradient(int parameter)
    {
        int shot = parameter / 2;
        if (shot < CrocomireProjectileDefinitions.AuthoredShotCount)
            return CrocomireProjectileDefinitions.GradientCycle[shot % CrocomireProjectileDefinitions.GradientCycle.Length];
        if (shot == CrocomireProjectileDefinitions.AuthoredShotCount)
            throw new InvalidDataException("Crocomire's tenth shot reads its gradient past the authored nine.");

        // Rejected, not read: the spit attack counts its shots up by two from zero and stops once the count reaches
        // eighteen, so no shot is spawned with a later parameter.
        throw new InvalidDataException($"Crocomire gradient selector {parameter} is outside the volley, which ends at its tenth shot.");
    }
}
