namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The one reader of where a room's population lists its enemies relative to each other (#1542, #2556). A population lists
/// every member of a multipart enemy together, in the order its <see cref="EnemyComposite{TRole}"/> names their roles: a
/// Phantoon's eye, tentacles and mouth after his body, a Powamp's balloon before its body. A member, as it is placed, asks
/// here for the member that plays another role and keeps the place it is given; no behaviour reaches another enemy by its
/// place in the list. A reference names the place, not its current occupant, so a partner that has died or been deleted
/// is still the one reached, as the game expects. The rules that reproduce a reach by placement itself (a struck part's
/// follow-up, the gunship pad's borrowed height) read the places after and before an enemy here too.
/// </summary>
[EncodingOwner("a room population's listing of its enemies: the members of a multipart enemy together, and the places after and before an enemy")]
internal static partial class EnemyCompositeListing
{
    /// <summary>The member of <paramref name="self"/>'s composite that plays <paramref name="role"/>.</summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="self">The member looking for its partner.</param>
    /// <param name="composite">How the population lists the composite.</param>
    /// <param name="selfRole">The role <paramref name="self"/> plays, which the composite names once.</param>
    /// <param name="role">The partner's role, which the composite names once.</param>
    /// <exception cref="InvalidOperationException">The composite does not name either role once.</exception>
    /// <exception cref="InvalidDataException">The population lists no enemy where the partner belongs.</exception>
    internal static RoomEnemyPlace Member<TRole>(this EnemyPlaces enemies, RoomEnemyPlace self, EnemyComposite<TRole> composite, TRole selfRole, TRole role)
        where TRole : notnull =>
        RequireListed(enemies, self, OnlyPlaceOf(composite, role) - OnlyPlaceOf(composite, selfRole), selfRole);

    /// <summary>
    /// Every member of <paramref name="self"/>'s composite that plays <paramref name="role"/>, in the order the population
    /// lists them.
    /// </summary>
    /// <param name="enemies">The room's enemy places.</param>
    /// <param name="self">The member looking for its partners.</param>
    /// <param name="composite">How the population lists the composite.</param>
    /// <param name="selfRole">The role <paramref name="self"/> plays, which the composite names once.</param>
    /// <param name="role">The partners' role.</param>
    /// <exception cref="InvalidOperationException">The composite does not name <paramref name="selfRole"/> once.</exception>
    /// <exception cref="InvalidDataException">The population lists no enemy where a partner belongs.</exception>
    internal static RoomEnemyPlace[] Members<TRole>(this EnemyPlaces enemies, RoomEnemyPlace self, EnemyComposite<TRole> composite, TRole selfRole, TRole role)
        where TRole : notnull
    {
        int own = OnlyPlaceOf(composite, selfRole);
        var members = new List<RoomEnemyPlace>();
        for (int listed = 0; listed < composite.ListedRoles.Count; listed++)
        {
            if (EqualityComparer<TRole>.Default.Equals(composite.ListedRoles[listed], role))
            {
                members.Add(RequireListed(enemies, self, listed - own, selfRole));
            }
        }
        return [.. members];
    }

    /// <summary>
    /// The member listed right before <paramref name="self"/> in its composite, or null for the composite's first member,
    /// which has none.
    /// </summary>
    /// <exception cref="InvalidOperationException">The composite does not name <paramref name="selfRole"/> once.</exception>
    /// <exception cref="InvalidDataException">The population lists no enemy where the partner belongs.</exception>
    internal static RoomEnemyPlace? MemberBefore<TRole>(this EnemyPlaces enemies, RoomEnemyPlace self, EnemyComposite<TRole> composite, TRole selfRole)
        where TRole : notnull =>
        OnlyPlaceOf(composite, selfRole) == 0 ? null : RequireListed(enemies, self, -1, selfRole);

    /// <summary>
    /// The member listed right after <paramref name="self"/> in its composite, or null for the composite's last member,
    /// which has none.
    /// </summary>
    /// <exception cref="InvalidOperationException">The composite does not name <paramref name="selfRole"/> once.</exception>
    /// <exception cref="InvalidDataException">The population lists no enemy where the partner belongs.</exception>
    internal static RoomEnemyPlace? MemberAfter<TRole>(this EnemyPlaces enemies, RoomEnemyPlace self, EnemyComposite<TRole> composite, TRole selfRole)
        where TRole : notnull =>
        OnlyPlaceOf(composite, selfRole) == composite.ListedRoles.Count - 1 ? null : RequireListed(enemies, self, 1, selfRole);

    /// <summary>
    /// The enemies the room lists after <paramref name="enemy"/>, by how many places after it, one or more, for a rule that
    /// reproduces a reach by placement (<see cref="FollowUpAfterStruckPart"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">An asked place lies past the room's last.</exception>
    internal static Func<int, RoomEnemyPlace> PlacesAfter(this EnemyPlaces enemies, RoomEnemyPlace enemy) =>
        distance => RequireListed(enemies, enemy, Ensure.BetweenInclusive(1, EnemyPlaceOrder.Capacity - 1, distance), enemy.Occupant?.DefinitionId);

    /// <summary>
    /// Every enemy place the room lists after <paramref name="enemy"/>, nearest first, and none for the last place, for a
    /// rule that reproduces a reach by placement as far as the room's places go
    /// (<see cref="FollowUpAfterStruckPart.RecipientsWithin"/>).
    /// </summary>
    internal static IEnumerable<RoomEnemyPlace> AllPlacesAfter(this EnemyPlaces enemies, RoomEnemyPlace enemy) =>
        Ensure.NotNull(enemies).InUpdateOrder.Skip(PlacesThrough(enemy));

    /// <summary>
    /// The enemies the room lists before <paramref name="enemy"/>, by how many places before it, one or more, for a rule
    /// that reproduces a reach by placement (<see cref="GunshipPadBorrowedHeight"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">An asked place lies before the room's first.</exception>
    internal static Func<int, RoomEnemyPlace> PlacesBefore(this EnemyPlaces enemies, RoomEnemyPlace enemy) =>
        distance => RequireListed(enemies, enemy, -Ensure.BetweenInclusive(1, EnemyPlaceOrder.Capacity - 1, distance), enemy.Occupant?.DefinitionId);

    /// <summary>
    /// Whether room loading has initialized <paramref name="enemy"/> (<see cref="IRoomLoadProgress"/>). Loading
    /// initializes the room's population in the order it lists it, so its progress is measured by that order.
    /// </summary>
    /// <param name="progress">How far room loading has progressed.</param>
    /// <param name="enemy">The enemy whose initialization is asked about.</param>
    internal static bool HasInitialized(this IRoomLoadProgress progress, RoomEnemyPlace enemy) =>
        Ensure.NotNull(progress).HasInitializedEnemyPlace(Ensure.NotNull(enemy).Ordinal);

    /// <summary>How many places, from the first, reach as far as <paramref name="enemy"/>'s, its own included.</summary>
    internal static int PlacesThrough(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).Ordinal + 1;

    /// <summary>
    /// Sets up the new occupant of <paramref name="enemy"/>'s place from the place's previous occupant
    /// (<see cref="PlaceInheritance{TOccupant}"/>).
    /// </summary>
    internal static TOccupant Inherit<TOccupant>(this PlaceInheritance<TOccupant> inheritance, RoomEnemyPlace enemy, Func<TOccupant?, TOccupant> occupy)
        where TOccupant : class =>
        Ensure.NotNull(inheritance).Inherit(Ensure.NotNull(enemy).Ordinal, occupy);

    /// <summary>
    /// Whether a Magdollite arm in <paramref name="arm"/>'s place grows one final segment on reaching its full height
    /// (<see cref="MagdolliteApexGrowth"/>).
    /// </summary>
    internal static bool GrowsFinalSegment(this MagdolliteApexGrowth growth, RoomEnemyPlace arm) =>
        Ensure.NotNull(growth).GrowsFinalSegment(Ensure.NotNull(arm).Ordinal);

    /// <summary>The asking of a loading-time read about enemy places, answered by their initialization progress.</summary>
    /// <param name="progress">How far room loading has progressed.</param>
    internal static Func<RoomEnemyPlace, bool> InitializedPlaces(this IRoomLoadProgress progress) =>
        Ensure.NotNull(progress).HasInitialized;

    /// <summary>The place <paramref name="offset"/> places after (or, negative, before) <paramref name="self"/>, required.</summary>
    /// <exception cref="InvalidDataException">The population lists no enemy there.</exception>
    private static RoomEnemyPlace RequireListed<TRole>(EnemyPlaces enemies, RoomEnemyPlace self, int offset, TRole selfRole)
    {
        Ensure.NotNull(enemies);
        if (!IsListed(self, offset))
            throw new InvalidDataException($"Enemy {self.Ordinal} ({self.Occupant?.DefinitionId}), the {selfRole}, has no enemy listed {offset} places from it.");
        return enemies.InUpdateOrder[self.Ordinal + offset];
    }

    /// <summary>Whether a room place lies <paramref name="offset"/> places after (or, negative, before) <paramref name="self"/>.</summary>
    private static bool IsListed(RoomEnemyPlace self, int offset)
    {
        int place = Ensure.NotNull(self).Ordinal + offset;
        return place is >= 0 and < EnemyPlaceOrder.Capacity;
    }

    /// <summary>Where the composite lists the one member that plays <paramref name="role"/>.</summary>
    /// <exception cref="InvalidOperationException">The composite does not name the role exactly once.</exception>
    private static int OnlyPlaceOf<TRole>(EnemyComposite<TRole> composite, TRole role) where TRole : notnull
    {
        Ensure.NotNull(composite);
        int? found = null;
        for (int listed = 0; listed < composite.ListedRoles.Count; listed++)
        {
            if (!EqualityComparer<TRole>.Default.Equals(composite.ListedRoles[listed], role))
                continue;
            if (found is not null)
                throw new InvalidOperationException($"The composite lists {role} more than once; its members find it by role.");
            found = listed;
        }
        return found ?? throw new InvalidOperationException($"The composite lists no {role}.");
    }
}
