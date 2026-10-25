namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire's mouth volley aims each shot.</summary>
public static class CrocomireVolley
{
    /// <summary>
    /// The gradient a shot's spawn parameter selects: the parameter halved indexes the volley, whose first nine shots
    /// cycle through <see cref="CrocomireProjectileDefinitions.GradientCycle"/> and whose tenth reads
    /// <see cref="CrocomireProjectileDefinitions.FinalShotOverreadGradient"/>. The fight emits only 2, 4, ..., 18.
    /// </summary>
    /// <exception cref="InvalidDataException">The parameter selects a shot past the tenth.</exception>
    public static short GradientForSpawnParameter(int parameter)
    {
        int shot = parameter / 2;
        if (shot < CrocomireProjectileDefinitions.AuthoredShotCount)
            return CrocomireProjectileDefinitions.GradientCycle[shot % CrocomireProjectileDefinitions.GradientCycle.Length];
        if (shot == CrocomireProjectileDefinitions.AuthoredShotCount)
            return CrocomireProjectileDefinitions.FinalShotOverreadGradient;
        throw new InvalidDataException($"Crocomire gradient selector {parameter} is outside the authored volley and its known final overread.");
    }
}
