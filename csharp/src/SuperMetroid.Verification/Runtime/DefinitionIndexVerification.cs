using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Runtime;

/// <summary>
/// Checks the debugger state's index of the objects the game's static members hold (#2619): it holds the catalogs whose
/// type initializers have run, each catalog marking itself as its initializer runs, and a catalog initialized after the
/// index was first used is indexed by the next lookup.
/// </summary>
internal static class DefinitionIndexVerification
{
    /// <summary>
    /// #2619: a runtime standing in Landing Site, captured through an index no capture has used and restored through
    /// another, stands in the catalog's own Landing Site: the capture names the room by its path among the initialized
    /// catalogs, and the restore resolves it to the catalog's member.
    /// </summary>
    internal static void VerifyDebuggerStateRestoresInitializedCatalogRoom()
    {
        SuperMetroidRuntime runtime = RetailRuntimeFixtureVerification.CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;

        byte[] captured = GraphDigestVerification.SerializeGraphThrough(runtime, new DefinitionObjects(StateGraphTypes.Shared), content);
        SuperMetroidRuntime restored = GraphDigestVerification.RestoreGraphThrough<SuperMetroidRuntime>(
            captured, new DefinitionObjects(StateGraphTypes.Shared), content);
        AssertTrue(ReferenceEquals(RoomDefinitions.LandingSite, restored.Room.LoadedRoom.Definition),
            "the restored runtime stands in the catalog's own Landing Site");
        Console.WriteLine("Definition index: the Landing Site capture restores to the catalog's own room.");
    }

    /// <summary>
    /// #2619: a catalog not yet initialized when an index was first used marks itself as its members are first read, and the
    /// same index's next lookup names its member by a path that resolves to the member's own object.
    /// </summary>
    internal static void VerifyDefinitionIndexAddsLaterInitializedCatalogs()
    {
        var definitions = new DefinitionObjects(StateGraphTypes.Shared);
        AssertTrue(!definitions.TryGetPath(new object(), out _), "a bare object is never named");
        DefinitionCatalog? later = null;
        (string Name, object Root) member = default;
        foreach (DefinitionCatalog catalog in definitions.Catalogs)
        {
            if (StateGraphCatalogInitializations.Initialized.Contains(catalog.Type))
                continue;
            (string Name, object Root)[] roots = catalog.Roots();
            AssertTrue(StateGraphCatalogInitializations.Initialized.Contains(catalog.Type),
                $"{catalog.Type.FullName} marks itself initialized as its members are first read");
            if (roots.Length == 0)
                continue;
            later = catalog;
            member = roots[0];
            break;
        }
        AssertTrue(later is not null, "a catalog holding an object is not yet initialized when the index is first used");
        AssertTrue(definitions.TryGetPath(member.Root, out string? path), $"{member.Name}, initialized after the index was first used, is named");
        AssertTrue(ReferenceEquals(member.Root, definitions.Resolve(path!)), $"{member.Name}'s path resolves to its own object");
        Console.WriteLine($"Definition index: {member.Name}, initialized after the index's first lookup, is named as {path}.");
    }

    /// <summary>Runs the definition index checks.</summary>
    [VerificationCommand("--definition-index")]
    internal static int DefinitionIndexCommand()
    {
        Suite(nameof(VerifyDebuggerStateRestoresInitializedCatalogRoom), () => VerifyDebuggerStateRestoresInitializedCatalogRoom());
        Suite(nameof(VerifyDefinitionIndexAddsLaterInitializedCatalogs), () => VerifyDefinitionIndexAddsLaterInitializedCatalogs());
        return 0;
    }
}
