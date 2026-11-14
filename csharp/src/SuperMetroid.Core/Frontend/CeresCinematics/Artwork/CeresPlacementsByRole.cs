namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Keys a Ceres scene's installed actor placements by the role each places (#1768).</summary>
internal static class CeresPlacementsByRole
{
    /// <summary>
    /// The placements keyed by role, requiring exactly one placement for every role of <typeparamref name="TRole"/> and
    /// no other.
    /// </summary>
    /// <param name="placements">The installed placements, in any order.</param>
    /// <param name="roleOf">The role a placement places.</param>
    /// <param name="scene">The scene the layout belongs to, named in the failure.</param>
    /// <exception cref="InvalidDataException">A placement is missing, names a role twice or names no role of the scene.</exception>
    internal static Dictionary<TRole, TPlacement> Index<TRole, TPlacement>(IEnumerable<TPlacement> placements,
        Func<TPlacement, TRole> roleOf, string scene)
        where TRole : struct, Enum
        where TPlacement : class
    {
        Ensure.NotNull(placements);
        Ensure.NotNull(roleOf);
        var byRole = new Dictionary<TRole, TPlacement>();
        foreach (TPlacement placement in placements)
        {
            if (placement is null)
                throw new InvalidDataException($"{scene} holds an empty actor placement.");
            TRole role = roleOf(placement);
            if (!Enum.IsDefined(role))
                throw new InvalidDataException($"{scene} places an actor of unknown role {role}.");
            if (!byRole.TryAdd(role, placement))
                throw new InvalidDataException($"{scene} places its {role} actor twice.");
        }
        foreach (TRole role in Enum.GetValues<TRole>())
        {
            if (!byRole.ContainsKey(role))
                throw new InvalidDataException($"{scene} does not place its {role} actor.");
        }
        return byRole;
    }

    /// <summary>The placement of <paramref name="role"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a role of the scene.</exception>
    internal static TPlacement Of<TRole, TPlacement>(IReadOnlyDictionary<TRole, TPlacement> placements, TRole role)
        where TRole : struct, Enum
        where TPlacement : class =>
        placements.TryGetValue(role, out TPlacement? placement) ? placement : throw new ArgumentOutOfRangeException(nameof(role));
}
