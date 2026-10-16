using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Harness;

/// <summary>Checks the #623 required-value wrappers and representative lifecycle contracts.</summary>
internal static class NullContractsVerification
{
    /// <summary>A public constant for <see cref="RequiredReflection.RequiredConstant{T}"/>.</summary>
    public const ushort ReflectedConstant = 4660;

    /// <summary>
    /// #623: each wrapper returns the present value and fails with its named exception and context when the value is
    /// absent; lifecycle accessors fail before their state exists instead of handing back null.
    /// </summary>
    internal static void VerifyNullContracts()
    {
        // RequiredJson: a JSON null document is a failure, a present document is returned.
        AssertEqual(7, RequiredJson.Deserialize<int[]>("[7]")[0], "RequiredJson returns a present document");
        var missingDocument = AssertThrows<JsonException>(() => RequiredJson.Deserialize<int[]>("null"), "RequiredJson rejects a null document");
        AssertTrue(missingDocument.Message.Contains("Int32[]", StringComparison.Ordinal), "RequiredJson names the required type");
        AssertTrue(RequiredJson.Parse("{}") is JsonObject, "RequiredJson parses a present root");
        AssertThrows<JsonException>(() => RequiredJson.Parse("null"), "RequiredJson rejects a null root");

        // RequiredJsonNodes: absent or null properties and elements fail at their path.
        JsonNode document = RequiredJson.Parse("""{"frames":[{"x":1},null],"empty":null}""");
        AssertEqual(1, document.Required("frames").Required(0).Required("x").GetValue<int>(), "Required returns present nodes");
        var missingProperty = AssertThrows<KeyNotFoundException>(() => document.Required("absent"), "Required rejects an absent property");
        AssertTrue(missingProperty.Message.Contains("'absent'", StringComparison.Ordinal), "Required names the absent property");
        AssertThrows<KeyNotFoundException>(() => document.Required("empty"), "Required rejects a null property");
        var missingElement = AssertThrows<KeyNotFoundException>(() => document.Required("frames").Required(1), "Required rejects a null element");
        AssertTrue(missingElement.Message.Contains("$.frames", StringComparison.Ordinal), "Required names the element's array path");
        AssertThrows<KeyNotFoundException>(() => document.Required("frames").AsArray().RequiredElements().ToArray(),
            "RequiredElements rejects a null element");

        // RequiredReflection: present public members and constants resolve; absent ones fail with the member named.
        AssertEqual(nameof(GetType), typeof(object).GetRequiredMethod(nameof(GetType)).Name, "GetRequiredMethod returns the member");
        AssertEqual(nameof(string.Length), typeof(string).GetRequiredProperty(nameof(string.Length)).Name, "GetRequiredProperty returns the member");
        var missingMethod = AssertThrows<MissingMethodException>(() => typeof(string).GetRequiredMethod("NoSuchMethod"), "GetRequiredMethod rejects an absent method");
        AssertTrue(missingMethod.Message.Contains("NoSuchMethod", StringComparison.Ordinal), "GetRequiredMethod names the method");
        AssertThrows<MissingFieldException>(() => typeof(string).GetRequiredField("NoSuchField"), "GetRequiredField rejects an absent field");
        AssertThrows<MissingMemberException>(() => typeof(string).GetRequiredProperty("NoSuchProperty"), "GetRequiredProperty rejects an absent property");
        var constant = typeof(NullContractsVerification).GetRequiredField(nameof(ReflectedConstant));
        AssertEqual(ReflectedConstant, constant.RequiredConstant<ushort>(), "RequiredConstant reads a typed constant");
        AssertThrows<InvalidOperationException>(() => constant.RequiredConstant<int>(), "RequiredConstant rejects a constant of another type");
        AssertTrue(RequiredReflection.TrustedPlatformAssemblies().Length > 0, "the host publishes trusted platform assemblies");

        // Lifecycle state: absent until created, and a checked accessor fails rather than returning null.
        var room = new ActiveRoomState();
        AssertThrows<InvalidOperationException>(() => _ = room.LoadedLevelData, "room level data requires a loaded room");
        AssertThrows<InvalidOperationException>(() => _ = room.LoadedCamera, "room camera requires a loaded room");
        var player = new SamusFrameState();
        AssertThrows<InvalidOperationException>(() => _ = player.IntroducedSamus, "gameplay Samus requires her introduction");
        player.Samus = new SamusState();
        AssertTrue(ReferenceEquals(player.Samus, player.IntroducedSamus), "the introduced Samus is the attached actor");
        var phantoon = new PhantoonEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry));
        AssertThrows<InvalidOperationException>(() => _ = phantoon.Eye, "Phantoon's eye requires part initialization");
        var enemies = new RoomEnemySystem();
        AssertThrows<InvalidOperationException>(() => _ = enemies.Presentation.Cgram, "enemy palettes require bound CGRAM");
        var game = new SuperMetroidGame(SuperMetroidAddressSpace.CreateWithoutCartridge());
        AssertThrows<InvalidOperationException>(() => _ = game.Session.GameplayRuntime, "the frontend has no gameplay runtime before a game loads");
        Console.WriteLine("#623 null contracts: JSON, node, reflection and lifecycle wrappers return present values and fail with context when absent.");
    }

    /// <summary>Runs the focused null-contract checks from the CLI.</summary>
    [VerificationCommand("--null-contracts")]
    internal static int NullContractsCommand()
    {
        Suite(nameof(VerifyNullContracts), () => VerifyNullContracts());
        return 0;
    }
}
