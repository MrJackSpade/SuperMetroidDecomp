using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Runtime;

/// <summary>
/// Checks the debugger state's index of the objects the game's static members hold indexes only the catalogs a state
/// may reach (#1716), and that the catalogs it asks about an object are every catalog that exposes one of its type.
/// </summary>
internal static class DefinitionIndexVerification
{
    /// <summary>
    /// Catalogs a runtime standing in Landing Site reaches nothing of: bosses, cinematics and the ending. The trail
    /// placements (<see cref="ProjectileTrailPlacementDefinitions"/>) are not among them (#2605): since 4105dddf04 the
    /// catalog holds its frame sets as arrays of <see cref="SamusProjectileFrameId"/>, and the runtime lets an array of
    /// ints, or of any enum stored as an int, be assigned to one. So a capture asks the catalog about each such array the
    /// state holds (Landing Site's sound libraries, enemy identities, areas, story events, scroll states and ints), and
    /// indexes it. It holds none of them, so the capture still names no member of it.
    /// </summary>
    private static readonly Type[] LandingSiteUnreachedCatalogs =
    [
        typeof(KraidAnimations), typeof(MotherBrainAnimations), typeof(PhantoonAnimations), typeof(DraygonAnimations),
        typeof(EndingCreditsPhaseGroups), typeof(CeresDestructionPhaseGroups),
    ];

    /// <summary>
    /// #1716: a debugger state's first capture indexes only the catalogs that may hold what the state reaches. A runtime
    /// standing in Landing Site, captured through an index no capture has used, indexes fewer catalogs than there are and
    /// none of the boss, cinematic and ending catalogs it reaches nothing of; restored through another such index, it
    /// stands in the catalog's own Landing Site, and the restore indexes only catalogs the capture named members of.
    /// </summary>
    internal static void VerifyDebuggerStateIndexesReachedCatalogs()
    {
        SuperMetroidRuntime runtime = RetailRuntimeFixtureVerification.CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;

        var capturing = new DefinitionObjects();
        byte[] captured = GraphDigestVerification.SerializeGraphThrough(runtime, capturing, content);
        DefinitionCatalog[] indexed = Indexed(capturing);
        Console.WriteLine($"Definition index: a Landing Site runtime's first capture indexes {indexed.Length} of {capturing.Catalogs.Count} catalogs.");
        AssertTrue(indexed.Length < capturing.Catalogs.Count, "a Landing Site capture leaves catalogs it reaches nothing of unindexed");
        foreach (Type unreached in LandingSiteUnreachedCatalogs)
        {
            AssertTrue(capturing.Catalogs.Any(catalog => TypeNameOf(catalog) == unreached.FullName), $"{unreached.Name} is a catalog of static members");
            AssertTrue(indexed.All(catalog => TypeNameOf(catalog) != unreached.FullName), $"a Landing Site capture does not index {unreached.Name}");
        }

        var restoring = new DefinitionObjects();
        SuperMetroidRuntime restored = GraphDigestVerification.RestoreGraphThrough<SuperMetroidRuntime>(captured, restoring, content);
        AssertTrue(ReferenceEquals(RoomDefinitions.LandingSite, restored.Room.LoadedRoom.Definition),
            "the restored runtime stands in the catalog's own Landing Site");
        AssertTrue(Indexed(restoring).All(catalog => indexed.Any(captured => captured.Prefix == catalog.Prefix)),
            "a restore indexes only catalogs its capture named members of");
        Console.WriteLine("Definition index: the Landing Site capture restores to the catalog's own room.");
    }

    /// <summary>The namespaces of the enemy and boss catalogs, whose exposure one check covers.</summary>
    private static readonly string[] EnemyNamespaces = ["SuperMetroid.Core.Enemies", "SuperMetroid.Core.Bosses"];

    /// <summary>The namespaces of the room, world and area catalogs, whose exposure one check covers.</summary>
    private static readonly string[] WorldNamespaces =
        ["SuperMetroid.Core.Rooms", "SuperMetroid.Core.World", "SuperMetroid.Core.Areas", "SuperMetroid.Core.Fx"];

    /// <summary>#1716: every enemy and boss catalog's exposed objects are asked of it (<see cref="VerifyCandidatesCoverExposedMembers"/>).</summary>
    internal static void VerifyDefinitionCandidatesCoverEnemyCatalogs() =>
        VerifyCandidatesCoverExposedMembers("enemy and boss", catalog => InNamespaces(catalog, EnemyNamespaces));

    /// <summary>#1716: every room, world and area catalog's exposed objects are asked of it (<see cref="VerifyCandidatesCoverExposedMembers"/>).</summary>
    internal static void VerifyDefinitionCandidatesCoverWorldCatalogs() =>
        VerifyCandidatesCoverExposedMembers("room, world and area", catalog => InNamespaces(catalog, WorldNamespaces));

    /// <summary>#1716: every other catalog's exposed objects are asked of it (<see cref="VerifyCandidatesCoverExposedMembers"/>).</summary>
    internal static void VerifyDefinitionCandidatesCoverOtherCatalogs() =>
        VerifyCandidatesCoverExposedMembers("other", catalog => !InNamespaces(catalog, EnemyNamespaces) && !InNamespaces(catalog, WorldNamespaces));

    /// <summary>Whether <paramref name="catalog"/>'s type lies in one of <paramref name="namespaces"/> or a namespace within one.</summary>
    private static bool InNamespaces(DefinitionCatalog catalog, string[] namespaces) =>
        NamespaceOf(catalog) is { } name && namespaces.Any(root => name == root || name.StartsWith(root + ".", StringComparison.Ordinal));

    /// <summary>The full name of the type whose static members <paramref name="catalog"/> holds: its prefix before the separator.</summary>
    private static string TypeNameOf(DefinitionCatalog catalog) => catalog.Prefix[..^DefinitionCatalog.MemberSeparator.Length];

    /// <summary>The namespace of the type whose static members <paramref name="catalog"/> holds, or none for the global namespace.</summary>
    private static string? NamespaceOf(DefinitionCatalog catalog) =>
        TypeNameOf(catalog).Split('+')[0] is var outer && outer.LastIndexOf('.') is var dot and >= 0 ? outer[..dot] : null;

    /// <summary>
    /// #1716: the index asks a catalog about an object whenever the catalog exposes one of the object's type, so a capture
    /// names every catalog member a game can reach by its path. Every object each catalog <paramref name="covered"/> selects
    /// exposes, walked as its type's generated members hand it to a debugger state (#2602), is of a type whose candidates
    /// include that catalog. Reading the members runs the catalogs' initializers, so the catalogs are checked in three groups.
    /// </summary>
    private static void VerifyCandidatesCoverExposedMembers(string group, Func<DefinitionCatalog, bool> covered)
    {
        var definitions = new DefinitionObjects();
        var candidates = new Dictionary<Type, HashSet<DefinitionCatalog>>();
        int exposedObjects = 0, coveredCatalogs = 0;
        foreach (DefinitionCatalog catalog in definitions.Catalogs.Where(covered))
        {
            coveredCatalogs++;
            foreach (object exposed in Exposed(catalog))
            {
                exposedObjects++;
                Type type = exposed.GetType();
                if (!candidates.TryGetValue(type, out HashSet<DefinitionCatalog>? asked))
                    candidates.Add(type, asked = [.. definitions.Candidates(exposed)]);
                if (!asked.Contains(catalog))
                    throw new InvalidOperationException(
                        $"Verification failed: {TypeNameOf(catalog)} exposes a {type.FullName}, but the index does not ask it about objects of that type.");
            }
        }
        AssertTrue(coveredCatalogs > 0, $"the {group} catalogs are found");
        Console.WriteLine($"Definition index: {exposedObjects} objects exposed by {coveredCatalogs} {group} catalogs are each asked of their catalog.");
    }

    /// <summary>The catalogs <paramref name="definitions"/> has indexed.</summary>
    private static DefinitionCatalog[] Indexed(DefinitionObjects definitions) =>
        [.. definitions.Catalogs.Where(catalog => catalog.BuiltGraph is not null)];

    /// <summary>
    /// Every object other than a string that <paramref name="catalog"/>'s members expose: each member's object, and every
    /// object a debugger state reaches from it through each type's generated members: a field's value, an element, a
    /// dictionary entry's key or value, a loaded part's value, a value type walked through to what it holds.
    /// </summary>
    private static List<object> Exposed(DefinitionCatalog catalog)
    {
        var exposed = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<object>();
        foreach (StateGraphCatalogMember member in catalog.Members)
            if (member.Read() is { } root)
                pending.Push(root);
        var held = new HeldObjects(pending);
        while (pending.Count > 0)
        {
            object value = pending.Pop();
            Type type = value.GetType();
            if (value is string or Delegate or Type || type.IsPrimitive || type.IsEnum)
                continue;
            if (!type.IsValueType)
            {
                if (!seen.Add(value))
                    continue;
                exposed.Add(value);
            }
            StateGraphTypes.Shared.For(value).Visit(value, held);
        }
        return exposed;
    }

    /// <summary>Collects every value an object hands a debugger state's walk.</summary>
    /// <param name="pending">Receives the values.</param>
    private sealed class HeldObjects(Stack<object> pending) : IStateGraphChildren
    {
        /// <inheritdoc/>
        public void Child(StateGraphField field, object? value)
        {
            if (value is not null)
                pending.Push(value);
        }

        /// <inheritdoc/>
        public void Element(int index, object? value)
        {
            if (value is not null)
                pending.Push(value);
        }

        /// <inheritdoc/>
        public void Part(string step, object? value)
        {
            if (value is not null)
                pending.Push(value);
        }
    }

    /// <summary>Runs the definition index checks.</summary>
    [VerificationCommand("--definition-index")]
    internal static int DefinitionIndexCommand()
    {
        Suite(nameof(VerifyDebuggerStateIndexesReachedCatalogs), () => VerifyDebuggerStateIndexesReachedCatalogs());
        Suite(nameof(VerifyDefinitionCandidatesCoverEnemyCatalogs), () => VerifyDefinitionCandidatesCoverEnemyCatalogs());
        Suite(nameof(VerifyDefinitionCandidatesCoverWorldCatalogs), () => VerifyDefinitionCandidatesCoverWorldCatalogs());
        Suite(nameof(VerifyDefinitionCandidatesCoverOtherCatalogs), () => VerifyDefinitionCandidatesCoverOtherCatalogs());
        return 0;
    }
}
