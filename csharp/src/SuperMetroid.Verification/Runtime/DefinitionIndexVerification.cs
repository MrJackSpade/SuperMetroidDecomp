using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
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
    /// <summary>Catalogs a runtime standing in Landing Site reaches nothing of: bosses, cinematics and the ending.</summary>
    private static readonly Type[] LandingSiteUnreachedCatalogs =
    [
        typeof(KraidPhaseGroups), typeof(MotherBrainAnimations), typeof(PhantoonAnimations), typeof(DraygonAnimations),
        typeof(EndingCreditsPhaseGroups), typeof(CeresDestructionPhaseGroups), typeof(ProjectileTrailPlacementDefinitions),
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
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;

        var capturing = new DefinitionObjects();
        byte[] captured = GraphDigestVerification.SerializeGraphThrough(runtime, capturing, content);
        DefinitionCatalog[] indexed = Indexed(capturing);
        Console.WriteLine($"Definition index: a Landing Site runtime's first capture indexes {indexed.Length} of {capturing.Catalogs.Count} catalogs.");
        AssertTrue(indexed.Length < capturing.Catalogs.Count, "a Landing Site capture leaves catalogs it reaches nothing of unindexed");
        foreach (Type unreached in LandingSiteUnreachedCatalogs)
        {
            AssertTrue(capturing.Catalogs.Any(catalog => catalog.Type == unreached), $"{unreached.Name} is a catalog of static members");
            AssertTrue(indexed.All(catalog => catalog.Type != unreached), $"a Landing Site capture does not index {unreached.Name}");
        }

        var restoring = new DefinitionObjects();
        SuperMetroidRuntime restored = GraphDigestVerification.RestoreGraphThrough<SuperMetroidRuntime>(captured, restoring, content);
        AssertTrue(ReferenceEquals(RoomDefinitions.LandingSite, restored.Room.LoadedRoom.Definition),
            "the restored runtime stands in the catalog's own Landing Site");
        AssertTrue(Indexed(restoring).All(catalog => indexed.Any(captured => captured.Type == catalog.Type)),
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
        catalog.Type.Namespace is { } name && namespaces.Any(root => name == root || name.StartsWith(root + ".", StringComparison.Ordinal));

    /// <summary>
    /// #1716: the index asks a catalog about an object whenever the catalog exposes one of the object's type, so a capture
    /// names every catalog member a game can reach by its path. Every object each catalog <paramref name="covered"/> selects
    /// exposes, walked as <see cref="DefinitionExposure"/> describes, is of a type whose candidates include that catalog.
    /// Reading the members runs the catalogs' initializers, so the catalogs are checked in three groups.
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
                    candidates.Add(type, asked = [.. definitions.Candidates(type)]);
                if (!asked.Contains(catalog))
                    throw new InvalidOperationException(
                        $"Verification failed: {catalog.Type.FullName} exposes a {type.FullName}, but the index does not ask it about objects of that type.");
            }
        }
        AssertTrue(coveredCatalogs > 0, $"the {group} catalogs are found");
        Console.WriteLine($"Definition index: {exposedObjects} objects exposed by {coveredCatalogs} {group} catalogs are each asked of their catalog.");
    }

    /// <summary>The catalogs <paramref name="definitions"/> has indexed.</summary>
    private static DefinitionCatalog[] Indexed(DefinitionObjects definitions) =>
        [.. definitions.Catalogs.Where(catalog => catalog.graph is not null)];

    /// <summary>
    /// Every object other than a string that <paramref name="catalog"/>'s members expose: each member's object; what a
    /// type of the game's assembly or a value type holds in its fields; an array's elements; and what another assembly's
    /// class exposes: the elements it enumerates and its comparers, a lazily loaded part's value once loaded, or the
    /// fields declared with one of its type arguments.
    /// </summary>
    [ReflectionAudit("the definition index check walks what each catalog's members expose, field by field")]
    private static List<object> Exposed(DefinitionCatalog catalog)
    {
        var exposed = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<object>();
        foreach (FieldInfo member in catalog.Members)
            if (member.GetValue(null) is { } root)
                pending.Push(root);
        while (pending.Count > 0)
        {
            object value = pending.Pop();
            Type type = value.GetType();
            if (value is string or Pointer || type.IsPrimitive || type.IsEnum)
                continue;
            if (!type.IsValueType)
            {
                if (!seen.Add(value))
                    continue;
                exposed.Add(value);
            }
            if (value is Delegate or MemberInfo)
                continue;
            foreach (object held in Held(value, type, catalog.Type.Assembly))
                pending.Push(held);
        }
        return exposed;
    }

    /// <summary>The objects <paramref name="value"/> of <paramref name="type"/> exposes, as <see cref="Exposed"/> describes.</summary>
    [ReflectionAudit("the definition index check walks what each catalog's members expose, field by field")]
    private static IEnumerable<object> Held(object value, Type type, Assembly game)
    {
        if (value is Array array)
        {
            // An array of values holding no reference exposes nothing but values.
            if (!HoldsReferences(Ensure.NotNull(type.GetElementType())))
                yield break;
            foreach (object? element in array)
                if (element is not null)
                    yield return element;
            yield break;
        }
        if (type.Assembly == game || type.IsValueType)
        {
            foreach (FieldInfo field in Fields(type))
                if (!field.FieldType.IsPointer && field.GetValue(value) is { } held)
                    yield return held;
            yield break;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Lazy<>))
        {
            if (type.GetProperty(nameof(Lazy<object>.IsValueCreated))?.GetValue(value) is true &&
                type.GetProperty(nameof(Lazy<object>.Value))?.GetValue(value) is { } loaded)
                yield return loaded;
            yield break;
        }
        if (value is System.Collections.IEnumerable elements)
        {
            foreach (object? element in elements)
                if (element is not null)
                    yield return element;
            foreach (string comparer in (string[])["Comparer", "KeyComparer", "ValueComparer"])
                if (type.GetProperty(comparer, BindingFlags.Instance | BindingFlags.Public) is { } property &&
                    property.GetIndexParameters().Length == 0 && property.GetValue(value) is { } held)
                    yield return held;
            yield break;
        }
        Type[] arguments = type.IsGenericType ? type.GetGenericArguments() : [];
        foreach (FieldInfo field in Fields(type))
            if (arguments.Contains(field.FieldType) && field.GetValue(value) is { } held)
                yield return held;
    }

    /// <summary>Every instance field of <paramref name="type"/> and its bases, read once per type.</summary>
    [ReflectionAudit("the definition index check walks what each catalog's members expose, field by field")]
    private static FieldInfo[] Fields(Type type)
    {
        if (fields.TryGetValue(type, out FieldInfo[]? known))
            return known;
        var declaredFields = new List<FieldInfo>();
        for (Type? current = type; current is not null; current = current.BaseType)
            declaredFields.AddRange(current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        FieldInfo[] all = [.. declaredFields];
        fields.Add(type, all);
        return all;
    }

    /// <summary>Each type's instance fields, as <see cref="Fields"/> reads them.</summary>
    private static readonly Dictionary<Type, FieldInfo[]> fields = [];

    /// <summary>Whether values of <paramref name="type"/> can hold a reference; a reference type always can.</summary>
    [ReflectionAudit("the definition index check skips arrays of values that hold no reference")]
    private static bool HoldsReferences(Type type) => !type.IsValueType ||
        (typeof(System.Runtime.CompilerServices.RuntimeHelpers)
            .GetMethod(nameof(System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences))
            ?? throw new InvalidOperationException("The runtime cannot tell which types hold references."))
            .MakeGenericMethod(type).Invoke(null, null) is true;

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
