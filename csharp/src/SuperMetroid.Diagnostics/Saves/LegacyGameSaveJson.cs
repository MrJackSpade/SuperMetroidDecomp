using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The one-way upgrade of older game saves to the schema Core reads: schema one (#1421) carried the cartridge's whole
/// SRAM image as raw pages, whose remaining fields are read back through <see cref="SaveRamCodec"/>; schema two (#1475)
/// listed bosses, used stations and acquired maps by the cartridge's table positions, which become lists by area; schema
/// three (#1523) kept the cartridge's loading-state word, which becomes the startup route it selects; schema four
/// (#1527) listed the Zebetite count as three event bits, which become a count, and named an event after its number;
/// schema five (#1553) listed event-table bits no story event uses by number, and the Chozo orbs, item locations and
/// doors by room-data number, which become the locations and doors they number; schema six (#1605) kept the HUD
/// selection and reserve-tank mode as numbers, which become their names, and two settings the game never reads; schema
/// seven (#1691) kept the count of item set-ups, which nothing in the game reads; schema eight (#1723) named the
/// checkpoint and used stations by their numbers in each area's station list, which become the stations' names; schema
/// nine (#1778) could list the item no item occupies, which is not carried; schema ten (#2103) named item locations and
/// doors after the disassembly's abbreviated room labels, which become the rooms' spelled-out names; schema eleven
/// (#2140) kept no history of a power-on that found no valid save, which becomes none; schema twelve (#2364) kept a
/// reserve-missile count nothing in the game reads, which is not carried; schema thirteen (#2491) named some item
/// locations after the room state that first places them, which become their rooms' names; schema fourteen (#2501) named
/// some doors after the room states that place them, which become their rooms' names; schema fifteen (#2560) kept the save
/// file's history as two marks, which become the history they mark.
/// </summary>
public static class LegacyGameSaveJson
{
    /// <summary>How the legacy pages' records are named.</summary>
    private static readonly JsonSerializerOptions PageOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };

    /// <summary>Reads a game save of any supported schema, upgrading a schema-one save first.</summary>
    /// <param name="json">The save's JSON text.</param>
    /// <param name="sourceName">Where the save came from, named in errors.</param>
    /// <param name="migrated">Whether the save was an older schema, which its owner rewrites.</param>
    public static GameSaveJsonDocument Deserialize(string json, string sourceName, out bool migrated)
    {
        Ensure.NotNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        string current;
        int sourceSchemaVersion;
        try
        {
            current = Upgrade(json, out sourceSchemaVersion);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Game save '{sourceName}' is malformed at {exception.Path ?? "the root"}: " + exception.Message, exception);
        }
        catch (InvalidDataException exception) when (!exception.Message.Contains(sourceName, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Game save '{sourceName}' is invalid: {exception.Message}", exception);
        }
        migrated = sourceSchemaVersion < GameSaveJsonFormat.SchemaVersion;
        return GameSaveJsonCodec.Deserialize(current, sourceName);
    }

    /// <summary>One-way upgrade of schemas one through fourteen to the current schema; the current schema is returned unchanged.</summary>
    private static string Upgrade(string json, out int sourceSchemaVersion)
    {
        JsonObject root = JsonNode.Parse(json) as JsonObject ??
            throw new InvalidDataException("Game save must be a JSON object.");
        if (root["schemaVersion"] is not JsonValue version || !version.TryGetValue(out sourceSchemaVersion))
            throw new InvalidDataException("schemaVersion must be an integer");
        if (sourceSchemaVersion >= GameSaveJsonFormat.SchemaVersion) return json;
        if (sourceSchemaVersion < SchemaTwo)
            UpgradeSchemaOne(root);
        if (sourceSchemaVersion <= SchemaTwo)
            UpgradeSchemaTwo(root);
        if (sourceSchemaVersion <= SchemaThree)
            UpgradeSchemaThree(root);
        if (sourceSchemaVersion <= SchemaFour)
            UpgradeSchemaFour(root);
        if (sourceSchemaVersion <= SchemaFive)
            UpgradeSchemaFive(root);
        if (sourceSchemaVersion <= SchemaSix)
            UpgradeSchemaSix(root);
        // Schema one kept the count only in its preserved image, which its upgrade does not read the count from.
        if (sourceSchemaVersion is >= SchemaTwo and <= SchemaSeven)
            UpgradeSchemaSeven(root);
        if (sourceSchemaVersion <= SchemaEight)
            UpgradeSchemaEight(root);
        if (sourceSchemaVersion <= SchemaNine)
            UpgradeSchemaNine(root);
        if (sourceSchemaVersion <= SchemaTen)
            UpgradeSchemaTen(root);
        // Schema one's upgrade reads the save file's history from its preserved image.
        if (sourceSchemaVersion is >= SchemaTwo and <= SchemaEleven)
            UpgradeSchemaEleven(root);
        // Schema one's upgrade does not read the count from its preserved image.
        if (sourceSchemaVersion is >= SchemaTwo and <= SchemaTwelve)
            UpgradeSchemaTwelve(root);
        if (sourceSchemaVersion <= SchemaThirteen)
            UpgradeSchemaThirteen(root);
        if (sourceSchemaVersion <= SchemaFourteen)
            UpgradeSchemaFourteen(root);
        if (sourceSchemaVersion <= SchemaFifteen)
            UpgradeSchemaFifteen(root);
        root["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        return root.ToJsonString();
    }

    /// <summary>The last schema that kept no history of a power-on that found no valid save.</summary>
    private const int SchemaEleven = 11;

    /// <summary>The last schema that kept a reserve-missile count.</summary>
    private const int SchemaTwelve = 12;

    /// <summary>The last schema that named some item locations after the room state that first places them.</summary>
    private const int SchemaThirteen = 13;

    /// <summary>The last schema that named some doors after the room states that place them.</summary>
    private const int SchemaFourteen = 14;

    /// <summary>The last schema that kept the save file's history as two marks.</summary>
    private const int SchemaFifteen = 15;

    /// <summary>
    /// Schema fifteen to sixteen (#2560): schemas one through fifteen kept the save file's history as two marks, a completed
    /// game and a power-on that found no valid save, which never held together; the upgraded file names the history they mark.
    /// </summary>
    private static void UpgradeSchemaFifteen(JsonObject root)
    {
        SaveFileHistory history = (TakeMark(root, "gameCompleted"), TakeMark(root, "powerOnFoundNoValidSave")) switch
        {
            (false, false) => SaveFileHistory.None,
            (true, false) => SaveFileHistory.GameCompleted,
            (false, true) => SaveFileHistory.PowerOnFoundNoValidSave,
            (true, true) => throw new InvalidDataException("gameCompleted and powerOnFoundNoValidSave cannot both hold"),
        };
        root["history"] = EnumName(history);
    }

    /// <summary>Removes and returns an older save's true-or-false property.</summary>
    private static bool TakeMark(JsonObject root, string name)
    {
        bool value = root[name] is JsonValue mark && mark.TryGetValue(out bool parsed) ? parsed
            : throw new InvalidDataException($"{name} must be true or false");
        root.Remove(name);
        return value;
    }

    /// <summary>
    /// The doors schemas six through fourteen named after the room states that place them, by that name, each mapped to its
    /// room's name and, where two doors of the room would share that, the grey door's condition, its block or, for the
    /// Climb's escape, the state that places it.
    /// </summary>
    private static readonly Dictionary<string, string> SchemaFourteenDoorNames = new(StringComparer.Ordinal)
    {
        ["LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombsGreenDoorFacingLeft"] = nameof(LockedDoorDefinitions.LandingSiteGreenDoorFacingLeft),
        ["LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombsYellowDoorFacingLeft"] = nameof(LockedDoorDefinitions.LandingSiteYellowDoorFacingLeft),
        ["LandingSiteWhenZebesTimebombSetGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.LandingSiteGreyDoorFacingRight),
        ["LandingSiteWhenZebesTimebombSetGreyDoorFacingLeftAt142x70"] = nameof(LockedDoorDefinitions.LandingSiteGreyDoorFacingLeftAt142x70),
        ["LandingSiteWhenZebesTimebombSetGreyDoorFacingLeftAt142x22"] = nameof(LockedDoorDefinitions.LandingSiteGreyDoorFacingLeftAt142x22),
        ["ParlorDefaultOrWhenZebesAwakeRedDoorFacingLeft"] = nameof(LockedDoorDefinitions.ParlorRedDoorFacingLeft),
        ["ParlorWhenZebesTimebombSetGreyDoorFacingRightAt1x6"] = nameof(LockedDoorDefinitions.ParlorGreyDoorFacingRightAt1x6),
        ["ParlorWhenZebesTimebombSetGreyDoorFacingRightAt17x38"] = nameof(LockedDoorDefinitions.ParlorGreyDoorFacingRightAt17x38),
        ["ParlorWhenZebesTimebombSetGreyDoorFacingRightAt17x54"] = nameof(LockedDoorDefinitions.ParlorGreyDoorFacingRightAt17x54),
        ["ParlorWhenZebesTimebombSetGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.ParlorGreyDoorFacingLeft),
        ["ParlorWhenZebesTimebombSetGreyDoorFacingUp"] = nameof(LockedDoorDefinitions.ParlorGreyDoorFacingUp),
        ["ClimbDefaultOrWhenZebesAwakeGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.ClimbGreyDoorFacingLeftAt46x6),
        ["ClimbDefaultOrWhenZebesAwakeGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.ClimbGreyDoorFacingRight),
        ["ClimbDefaultOrWhenZebesAwakeYellowDoorFacingLeft"] = nameof(LockedDoorDefinitions.ClimbYellowDoorFacingLeft),
        ["ClimbWhenZebesTimebombSetGreyDoorFacingLeftAt46x6"] = nameof(LockedDoorDefinitions.ClimbGreyDoorFacingLeftAt46x6WhenZebesTimebombSet),
        ["ClimbWhenZebesTimebombSetGreyDoorFacingLeftAt46x118"] = nameof(LockedDoorDefinitions.ClimbGreyDoorFacingLeftAt46x118),
        ["ClimbWhenZebesTimebombSetGreyDoorFacingLeftAt30x134"] = nameof(LockedDoorDefinitions.ClimbGreyDoorFacingLeftAt30x134),
        ["ClimbWhenZebesTimebombSetGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.ClimbGreyDoorFacingRightWhenZebesTimebombSet),
        ["PitDefaultGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.PitGreyDoorFacingRightNeverOpening),
        ["PitWithMorphBallAndMissilesGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.PitGreyDoorFacingLeft),
        ["PitWithMorphBallAndMissilesGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.PitGreyDoorFacingRightUntilEnemiesKilled),
        ["BombTorizoDefaultOrWhenAreaTorizoDeadBombTorizoGreyDoor"] = nameof(LockedDoorDefinitions.BombTorizoGreyDoor),
        ["BombTorizoWhenZebesTimebombSetGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.BombTorizoGreyDoorFacingRight),
        ["FlywayDefaultOrWhenAreaTorizoDeadRedDoorFacingLeft"] = nameof(LockedDoorDefinitions.FlywayRedDoorFacingLeft),
        ["MorphBallWhenZebesAwakeGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.MorphBallGreyDoorFacingRight),
        ["AtticDefaultGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.AtticGreyDoorFacingRightUntilAreaBossDead),
        ["AtticDefaultGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.AtticGreyDoorFacingLeftUntilAreaBossDead),
        ["BasementDefaultEyeDoorFacingLeft"] = nameof(LockedDoorDefinitions.BasementEyeDoorFacingLeft),
        ["BowlingAlleyWhenAreaBossDeadGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.BowlingAlleyGreyDoorFacingRight),
        ["AtticWhenAreaBossDeadGreyDoorFacingRight"] = nameof(LockedDoorDefinitions.AtticGreyDoorFacingRightUntilEnemiesKilled),
        ["AtticWhenAreaBossDeadGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.AtticGreyDoorFacingLeftUntilEnemiesKilled),
        ["AtticWhenAreaBossDeadGreyDoorFacingUp"] = nameof(LockedDoorDefinitions.AtticGreyDoorFacingUp),
        ["ElectricDeathWhenAreaBossDeadRedDoorFacingRight"] = nameof(LockedDoorDefinitions.ElectricDeathRedDoorFacingRight),
    };

    /// <summary>
    /// Schema fourteen to fifteen (#2501): schemas six through fourteen named some doors after the room states that place
    /// them; each becomes its room's name, and every other name is kept as it was.
    /// </summary>
    private static void UpgradeSchemaFourteen(JsonObject root)
    {
        foreach ((JsonObject progression, int index) in Progressions(root))
            progression["openedDoors"] = RenamedPlacements(progression, "openedDoors", index, SchemaFourteenDoorNames);
    }

    /// <summary>The item locations schemas six through thirteen named after the room state that first places them, by that name.</summary>
    private static readonly Dictionary<string, string> SchemaThirteenItemLocationNames = new(StringComparer.Ordinal)
    {
        ["PitWithMorphBallAndMissilesExposedMissileTank"] = nameof(ItemLocationDefinitions.PitExposedMissileTank),
        ["BombTorizoDefaultOrWhenAreaTorizoDeadChozoBombs"] = nameof(ItemLocationDefinitions.BombTorizoChozoBombs),
        ["FinalMissileWhenZebesAwakeExposedMissileTank"] = nameof(ItemLocationDefinitions.FinalMissileExposedMissileTank),
        ["MorphBallDefaultExposedMorphBall"] = nameof(ItemLocationDefinitions.MorphBallExposedMorphBall),
        ["MorphBallWhenZebesAwakeExposedPowerBombTank"] = nameof(ItemLocationDefinitions.MorphBallExposedPowerBombTank),
        ["BowlingAlleyWhenAreaBossDeadChozoReserveTank"] = nameof(ItemLocationDefinitions.BowlingAlleyChozoReserveTank),
        ["BowlingAlleyWhenAreaBossDeadExposedMissileTank"] = nameof(ItemLocationDefinitions.BowlingAlleyExposedMissileTank),
        ["AssemblyLineWhenAreaBossDeadExposedMissileTank"] = nameof(ItemLocationDefinitions.AssemblyLineExposedMissileTank),
        ["WreckedShipEnergyTankWhenAreaBossDeadExposedEnergyTank"] = nameof(ItemLocationDefinitions.WreckedShipEnergyTankExposedEnergyTank),
        ["WreckedShipWestSuperWhenAreaBossDeadExposedSuperMissileTank"] = nameof(ItemLocationDefinitions.WreckedShipWestSuperExposedSuperMissileTank),
        ["WreckedShipEastSuperWhenAreaBossDeadExposedSuperMissileTank"] = nameof(ItemLocationDefinitions.WreckedShipEastSuperExposedSuperMissileTank),
        ["GravitySuitWhenAreaBossDeadChozoGravitySuit"] = nameof(ItemLocationDefinitions.GravitySuitChozoGravitySuit),
    };

    /// <summary>
    /// Schema thirteen to fourteen (#2491): schemas six through thirteen named some item locations after the room state that
    /// first places them; each becomes its room's name, which every state of the room shares, and every other name is kept as
    /// it was; the schema-fourteen upgrade renames the doors.
    /// </summary>
    private static void UpgradeSchemaThirteen(JsonObject root)
    {
        foreach ((JsonObject progression, int index) in Progressions(root))
            progression["collectedItems"] = RenamedPlacements(progression, "collectedItems", index, SchemaThirteenItemLocationNames);
    }

    /// <summary>
    /// Schema twelve to thirteen (#2364): schemas two through twelve kept each slot's reserve-missile count, which nothing in
    /// the game reads; the upgraded slot drops it.
    /// </summary>
    private static void UpgradeSchemaTwelve(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject resources = slot["resources"] as JsonObject ?? throw new InvalidDataException($"slots[{index}].resources is required");
            resources.Remove("reserveMissiles");
        }
    }

    /// <summary>
    /// Schema eleven to twelve (#2140): schemas two through eleven kept completion alone and lost a power-on that found no
    /// valid save from the save file's history; the upgraded file holds no such power-on.
    /// </summary>
    private static void UpgradeSchemaEleven(JsonObject root) => root["powerOnFoundNoValidSave"] = false;

    /// <summary>The last schema that kept the count of item set-ups.</summary>
    private const int SchemaSeven = 7;

    /// <summary>The schema that named stations by their numbers in each area's station list.</summary>
    private const int SchemaEight = 8;

    /// <summary>The last schema that could list the item no item occupies.</summary>
    private const int SchemaNine = 9;

    /// <summary>The last schema that named item locations and doors after the disassembly's abbreviated room labels.</summary>
    private const int SchemaTen = 10;

    /// <summary>
    /// The item locations schemas six through ten named after an abbreviated room label, by that name, each mapped to the
    /// name schemas eleven through thirteen gave it; the schema-thirteen upgrade renames those that named a room state.
    /// </summary>
    private static readonly Dictionary<string, string> SchemaTenItemLocationNames = new(StringComparer.Ordinal)
    {
        ["BlueBrinstarETankExposedMissileTank"] = nameof(ItemLocationDefinitions.BlueBrinstarEnergyTankExposedMissileTank),
        ["BlueBrinstarETankShotBlockEnergyTank"] = nameof(ItemLocationDefinitions.BlueBrinstarEnergyTankShotBlockEnergyTank),
        ["BotwoonETankExposedEnergyTank"] = nameof(ItemLocationDefinitions.BotwoonEnergyTankExposedEnergyTank),
        ["EtecoonETankExposedEnergyTank"] = nameof(ItemLocationDefinitions.EtecoonEnergyTankExposedEnergyTank),
        ["GauntletETankExposedEnergyTank"] = nameof(ItemLocationDefinitions.GauntletEnergyTankExposedEnergyTank),
        ["HiJumpETankExposedEnergyTank"] = nameof(ItemLocationDefinitions.HiJumpEnergyTankExposedEnergyTank),
        ["HiJumpETankExposedMissileTank"] = nameof(ItemLocationDefinitions.HiJumpEnergyTankExposedMissileTank),
        ["HopperETankExposedEnergyTank"] = nameof(ItemLocationDefinitions.HopperEnergyTankExposedEnergyTank),
        ["LNEscapePowerBombsExposedPowerBombTank"] = nameof(ItemLocationDefinitions.LowerNorfairEscapePowerBombsExposedPowerBombTank),
        ["LNFirefleaExposedEnergyTank"] = nameof(ItemLocationDefinitions.LowerNorfairFirefleaExposedEnergyTank),
        ["LNSpringBallMazeExposedMissileTank"] = nameof(ItemLocationDefinitions.LowerNorfairSpringBallMazeExposedMissileTank),
        ["PostCrocJumpExposedMissileTank"] = nameof(ItemLocationDefinitions.PostCrocomireJumpExposedMissileTank),
        ["PostCrocMissileExposedMissileTank"] = nameof(ItemLocationDefinitions.PostCrocomireMissileExposedMissileTank),
        ["PostCrocPowerBombsExposedPowerBombTank"] = nameof(ItemLocationDefinitions.PostCrocomirePowerBombsExposedPowerBombTank),
        ["RidleyETankShotBlockEnergyTank"] = nameof(ItemLocationDefinitions.RidleyEnergyTankShotBlockEnergyTank),
        ["WSEastSuperWhenAreaBossDeadExposedSuperMissileTank"] = "WreckedShipEastSuperWhenAreaBossDeadExposedSuperMissileTank",
        ["WSWestSuperWhenAreaBossDeadExposedSuperMissileTank"] = "WreckedShipWestSuperWhenAreaBossDeadExposedSuperMissileTank",
        ["WarehouseETankShotBlockEnergyTank"] = nameof(ItemLocationDefinitions.WarehouseEnergyTankShotBlockEnergyTank),
        ["WaterwayETankExposedEnergyTank"] = nameof(ItemLocationDefinitions.WaterwayEnergyTankExposedEnergyTank),
        ["WreckedShipETankWhenAreaBossDeadExposedEnergyTank"] = "WreckedShipEnergyTankWhenAreaBossDeadExposedEnergyTank",
    };

    /// <summary>The doors schemas six through ten named after an abbreviated room label, by that name.</summary>
    private static readonly Dictionary<string, string> SchemaTenDoorNames = new(StringComparer.Ordinal)
    {
        ["EtecoonETankGreenDoorFacingRight"] = nameof(LockedDoorDefinitions.EtecoonEnergyTankGreenDoorFacingRight),
        ["HiJumpETankGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.HiJumpEnergyTankGreyDoorFacingLeft),
        ["LNFarmingEyeDoorFacingRight"] = nameof(LockedDoorDefinitions.LowerNorfairFarmingEyeDoorFacingRight),
        ["MaridiaElevRedDoorFacingLeft"] = nameof(LockedDoorDefinitions.MaridiaElevatorRedDoorFacingLeft),
        ["PostCrocFarmingRedDoorFacingRight"] = nameof(LockedDoorDefinitions.PostCrocomireFarmingRedDoorFacingRight),
        ["PostCrocShaftRedDoorFacingLeft"] = nameof(LockedDoorDefinitions.PostCrocomireShaftRedDoorFacingLeft),
        ["RedBinstarElevYellowDoorFacingDown"] = nameof(LockedDoorDefinitions.RedBrinstarElevatorYellowDoorFacingDown),
        ["WarehouseETankGreyDoorFacingLeft"] = nameof(LockedDoorDefinitions.WarehouseEnergyTankGreyDoorFacingLeft),
    };

    /// <summary>
    /// Schema ten to eleven (#2103): schemas six through ten named the item locations and doors of some rooms after the
    /// disassembly's abbreviated room labels (<c>LN</c>, <c>ETank</c>, <c>Elev</c> and the misspelt <c>RedBinstarElev</c>
    /// among them); each becomes its room's spelled-out name, and every other name is kept as it was.
    /// </summary>
    private static void UpgradeSchemaTen(JsonObject root)
    {
        foreach ((JsonObject progression, int index) in Progressions(root))
        {
            progression["collectedItems"] = RenamedPlacements(progression, "collectedItems", index, SchemaTenItemLocationNames);
            progression["openedDoors"] = RenamedPlacements(progression, "openedDoors", index, SchemaTenDoorNames);
        }
    }

    /// <summary>A slot's list of placement names, each renamed when <paramref name="renamed"/> lists it, in its order.</summary>
    private static JsonArray RenamedPlacements(JsonObject progression, string list, int slot, Dictionary<string, string> renamed)
    {
        JsonArray names = progression[list] as JsonArray ??
            throw new InvalidDataException($"slots[{slot}].progression.{list} is required");
        var current = new JsonArray();
        foreach (JsonNode? entry in names)
        {
            string name = entry is JsonValue value && value.TryGetValue(out string? text) ? text
                : throw new InvalidDataException($"slots[{slot}].progression.{list} lists an entry that is not a string");
            current.Add(renamed.TryGetValue(name, out string? spelledOut) ? spelledOut : name);
        }
        return current;
    }

    /// <summary>The name schemas one through nine wrote for the equipment bit no item records.</summary>
    private const string UnassignedItemName = "unassignedItem";

    /// <summary>The name the current schema writes for a slot holding no item.</summary>
    private const string NoItemName = "none";

    /// <summary>
    /// Schema nine to ten (#1778): schemas one through nine could list <c>unassignedItem</c> among a slot's collected and
    /// equipped items, the equipment bit Golden Torizo's item code set and nothing in the game reads; like schema six's
    /// unread words, it is not carried, and a list it alone made up becomes no item.
    /// </summary>
    private static void UpgradeSchemaNine(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject inventory = slot["inventory"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].inventory is required");
            foreach (string list in new[] { "collectedItems", "equippedItems" })
            {
                string items = inventory[list] is JsonValue value && value.TryGetValue(out string? text) ? text
                    : throw new InvalidDataException($"slots[{index}].inventory.{list} must be a string");
                string[] kept = [.. items.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Where(item => !string.Equals(item, UnassignedItemName, StringComparison.OrdinalIgnoreCase))];
                inventory[list] = kept.Length == 0 ? NoItemName : string.Join(", ", kept);
            }
        }
    }

    /// <summary>
    /// Schema eight to nine (#1723): schemas one through eight named a slot's checkpoint by its area and its number in the
    /// area's station list, and listed each area's used stations by those numbers, one entry per area in area order; both
    /// become the stations' names, the used stations in the order <see cref="LoadStationDefinitions.Usable"/> lists them. A
    /// checkpoint number that names no load station is rejected, as schema eight's reader rejected it. A used-station
    /// number that names no station Samus uses, which a glitched beam's overwrite of the record could set while saves
    /// kept every number, names nothing the game reads and, as schema eight's reader dropped it, is not carried.
    /// </summary>
    private static void UpgradeSchemaEight(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            string checkpointPath = $"slots[{index}].checkpoint";
            JsonObject checkpoint = slot["checkpoint"] as JsonObject ?? throw new InvalidDataException($"{checkpointPath} is required");
            AreaId checkpointArea = SchemaEightArea(checkpoint["area"], $"{checkpointPath}.area");
            slot["checkpoint"] = SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Decode(
                checkpointArea, SchemaEightInteger(checkpoint["saveStation"], $"{checkpointPath}.saveStation"), checkpointPath).Name;

            JsonObject progression = slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required");
            string usedPath = $"slots[{index}].progression.usedStations";
            JsonArray areas = Take(progression, "usedStations", index);
            if (areas.Count != AreaIds.All.Count)
                throw new InvalidDataException($"{usedPath} must contain {AreaIds.All.Count} areas");
            var used = new HashSet<LoadStation>();
            for (int position = 0; position < areas.Count; position++)
            {
                JsonObject entry = areas[position] as JsonObject ??
                    throw new InvalidDataException($"{usedPath}[{position}] must be an object");
                AreaId area = SchemaEightArea(entry["area"], $"{usedPath}[{position}].area");
                if (area != AreaIds.All[position])
                    throw new InvalidDataException($"{usedPath}[{position}] is out of area order");
                JsonArray numbers = entry["stations"] as JsonArray ??
                    throw new InvalidDataException($"{usedPath} of {area} is required");
                int previous = -1;
                foreach (JsonNode? listed in numbers)
                {
                    int number = SchemaEightInteger(listed, $"{usedPath} of {area}");
                    if (number <= previous)
                        throw new InvalidDataException($"{usedPath} of {area} must be strictly increasing without duplicates");
                    previous = number;
                    if (SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Find(area, number) is { } station &&
                        LoadStationDefinitions.Usable.Contains(station))
                        used.Add(station);
                }
            }
            var named = new JsonArray();
            foreach (LoadStation station in LoadStationDefinitions.Usable.Where(used.Contains))
                named.Add(station.Name);
            progression["usedStations"] = named;
        }
    }

    /// <summary>The area a schema-eight entry names, as the schema's reader named areas.</summary>
    private static AreaId SchemaEightArea(JsonNode? node, string property)
    {
        string name = node is JsonValue value && value.TryGetValue(out string? text) ? text
            : throw new InvalidDataException($"{property} must be an area name");
        foreach (AreaId area in AreaIds.All)
        {
            if (string.Equals(EnumName(area), name, StringComparison.OrdinalIgnoreCase))
                return area;
        }
        throw new InvalidDataException($"{property} '{name}' names no area");
    }

    /// <summary>A schema-eight integer.</summary>
    private static int SchemaEightInteger(JsonNode? node, string property) =>
        node is JsonValue value && value.TryGetValue(out int number) ? number
            : throw new InvalidDataException($"{property} must be an integer");

    /// <summary>The schema that listed item locations and doors by room-data number.</summary>
    private const int SchemaFive = 5;

    /// <summary>
    /// Schema six to seven (#1605): schema six kept each slot's HUD selection as its place in the selection ring and its
    /// reserve-tank mode as none, auto or manual counted from zero (<see cref="SaveSlotNumbering"/>); both become their
    /// names, and a number naming no selection or mode is rejected, as schema six's loader rejected it. Schema six also
    /// carried <c>cartridgeDebugFlag</c>, the debug build's Samus-placement setting, and <c>newFileMarker</c>, a word a
    /// new file sets; no retail routine reads either, so, like schema two's padding, they are not carried.
    /// </summary>
    private static void UpgradeSchemaSix(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            slot["hudItem"] = EnumName(SaveSlotNumbering.SavedHudItem(TakeInteger(slot, "hudItem", index)));
            slot["reserveMode"] = EnumName(SaveSlotNumbering.SavedReserveMode(TakeInteger(slot, "reserveMode", index)));
            _ = TakeInteger(slot, "cartridgeDebugFlag", index);
            _ = TakeInteger(slot, "newFileMarker", index);
        }
    }

    /// <summary>The schema that kept the HUD selection and reserve-tank mode as numbers.</summary>
    private const int SchemaSix = 6;

    /// <summary>
    /// Schema seven to eight (#1691): schemas two through seven carried <c>loadedItemCount</c>, the count of item set-ups
    /// the cartridge kept and nothing in the game reads; like schema six's unread words, it is not carried.
    /// </summary>
    private static void UpgradeSchemaSeven(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            _ = TakeInteger(slot, "loadedItemCount", index);
        }
    }

    /// <summary>Removes and returns an older slot's integer property.</summary>
    private static int TakeInteger(JsonObject slot, string name, int index)
    {
        int value = slot[name] is JsonValue number && number.TryGetValue(out int parsed) ? parsed
            : throw new InvalidDataException($"slots[{index}].{name} must be an integer");
        slot.Remove(name);
        return value;
    }

    /// <summary>The name the current schema writes for <paramref name="value"/>.</summary>
    private static string EnumName<T>(T value) where T : struct, Enum => JsonNamingPolicy.CamelCase.ConvertName(value.ToString());

    /// <summary>The schema that kept the cartridge's loading-state word.</summary>
    private const int SchemaThree = 3;

    /// <summary>The schema that listed the Zebetite count as three event bits.</summary>
    private const int SchemaFour = 4;

    /// <summary>
    /// Schema five to six (#1553): schema five listed a bit of the event table that no story event uses as its number,
    /// <c>0xNN</c>, and listed the Chozo orbs, item locations and doors by the numbers room data gives them. The item
    /// locations and doors become the names of the locations and doors those numbers name. Nothing in the game reads an
    /// unnamed event bit, a Chozo orb, or an item or door number that no retail placement has (only SpaceTime's
    /// overwrite, a tester assist or an imported save image could set one), so, like schema two's padding, they are not
    /// carried; every named event, item location and door is kept as it was.
    /// </summary>
    private static void UpgradeSchemaFive(JsonObject root)
    {
        foreach ((JsonObject progression, int index) in Progressions(root))
        {
            JsonArray events = progression["events"] as JsonArray ??
                throw new InvalidDataException($"slots[{index}].progression.events is required");
            var named = new JsonArray();
            foreach (JsonNode? entry in events)
            {
                string name = entry is JsonValue value && value.TryGetValue(out string? text) ? text
                    : throw new InvalidDataException($"slots[{index}].progression.events lists an event that is not a string");
                if (!name.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    named.Add(name);
            }
            progression["events"] = named;
            if (progression["brokenChozoOrbs"] is not JsonArray)
                throw new InvalidDataException($"slots[{index}].progression.brokenChozoOrbs is required");
            progression.Remove("brokenChozoOrbs");
            progression["collectedItems"] = PlacementNames(progression, "collectedItems", index,
                number => SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.TryItem(number, out var location) ? location.Name : null);
            progression["openedDoors"] = PlacementNames(progression, "openedDoors", index,
                number => SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.TryDoor(number, out var door) ? door.Name : null);
        }
    }

    /// <summary>
    /// The names of the placements a schema-five list numbers, in its order; a number no retail placement has names
    /// nothing the game reads and is not carried.
    /// </summary>
    private static JsonArray PlacementNames(JsonObject progression, string list, int slot, Func<int, string?> nameOf)
    {
        var names = new JsonArray();
        foreach (int number in Numbers(Take(progression, list, slot), slot))
        {
            if (nameOf(number) is { } name)
                names.Add(name);
        }
        return names;
    }

    /// <summary>Each saved slot's progression object and the slot's index.</summary>
    private static List<SlotProgression> Progressions(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        var progressions = new List<SlotProgression>();
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            progressions.Add(new SlotProgression(slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required"), index));
        }
        return progressions;
    }

    /// <summary>One saved slot's progression object.</summary>
    /// <param name="Progression">The slot's progression object.</param>
    /// <param name="Index">The slot's index.</param>
    private sealed record SlotProgression(JsonObject Progression, int Index);

    /// <summary>The event names through which schema four listed the Zebetite counter's bits, lowest bit first.</summary>
    private static readonly string[] ZebetiteCounterEventNames =
        ["ZebetiteDestroyedBit0", "ZebetiteDestroyedBit1", "ZebetiteDestroyedBit2"];

    /// <summary>The name schema four gave the event schema five names <see cref="StoryEvent.BabyMetroidEncounterMusicSkipped"/>.</summary>
    private const string BabyMetroidEncounterMusicSkippedSchemaFourName = "Unused14";

    /// <summary>The name schema five and every later schema save the Baby Metroid's skipped encounter music by, its installed key.</summary>
    private const string BabyMetroidEncounterMusicSkippedSavedName = "ShitroidEncounterMusicSkipped";

    /// <summary>
    /// The name schema five and every later schema give that event, its installed key, which renaming the member left as it
    /// was (#2552).
    /// </summary>
    private const string BabyMetroidEncounterMusicSkippedSavedName = "ShitroidEncounterMusicSkipped";

    /// <summary>
    /// Schema four to five (#1527): schema four listed the Zebetite count as the three events
    /// <c>ZebetiteDestroyedBit0</c> through <c>ZebetiteDestroyedBit2</c>, the bits of the cartridge's counter; they leave
    /// a slot's events and become its <c>zebetitesDestroyed</c> count, read as the cartridge's table codec reads the
    /// counter. The event schema four called <c>Unused14</c> is renamed to <see cref="StoryEvent.BabyMetroidEncounterMusicSkipped"/>.
    /// </summary>
    private static void UpgradeSchemaFour(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject progression = slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required");
            if (progression.ContainsKey("zebetitesDestroyed"))
                throw new InvalidDataException($"slots[{index}].progression.zebetitesDestroyed is not part of schema four");
            JsonArray events = progression["events"] as JsonArray ??
                throw new InvalidDataException($"slots[{index}].progression.events is required");
            var upgraded = new JsonArray();
            int counter = 0;
            foreach (JsonNode? entry in events)
            {
                string name = entry is JsonValue value && value.TryGetValue(out string? text) ? text
                    : throw new InvalidDataException($"slots[{index}].progression.events lists an event that is not a string");
                int bit = Array.FindIndex(ZebetiteCounterEventNames,
                    counterName => string.Equals(counterName, name, StringComparison.OrdinalIgnoreCase));
                if (bit >= 0)
                    counter |= 1 << bit;
                else if (string.Equals(name, BabyMetroidEncounterMusicSkippedSchemaFourName, StringComparison.OrdinalIgnoreCase))
                    upgraded.Add(BabyMetroidEncounterMusicSkippedSavedName);
                else
                    upgraded.Add(name);
            }
            progression["events"] = upgraded;
            progression["zebetitesDestroyed"] = SuperMetroid.AssetExtraction.Progression.ProgressionTableCodec.ZebetitesDestroyed(counter);
        }
    }

    /// <summary>
    /// Schema three to four (#1523): a slot's <c>loadingGameState</c>, the cartridge's dispatcher word, becomes the
    /// <c>startupRoute</c> it selects; a word the dispatcher does not name resumes the main game, as the cartridge does.
    /// A slot without the word resumes the main game.
    /// </summary>
    private static void UpgradeSchemaThree(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is not JsonObject slot)
                continue;
            SaveStartupRoute route = SaveStartupRoute.MainGame;
            if (slot["loadingGameState"] is JsonNode word)
            {
                int value = word is JsonValue number && number.TryGetValue(out int parsed) ? parsed
                    : throw new InvalidDataException($"slots[{index}].loadingGameState must be an integer");
                slot.Remove("loadingGameState");
                route = SuperMetroid.AssetExtraction.Progression.SaveStartupRouteCodec.Route(value);
            }
            slot["startupRoute"] = JsonNamingPolicy.CamelCase.ConvertName(route.ToString());
        }
    }

    /// <summary>The schema that listed progress as bit positions of the cartridge's tables.</summary>
    private const int SchemaTwo = 2;

    /// <summary>Bits a schema-two station or map table byte held.</summary>
    private const int BitsPerTableByte = 8;

    /// <summary>Bits a schema-two used-station table gave each area: one per number its station list can give a station.</summary>
    private const int UsedStationBitsPerArea = 16;

    /// <summary>
    /// Schema two to three (#1475): schema two listed bosses by the cartridge's eight area slots, used stations as bit
    /// positions of the sixteen-byte station table and acquired maps as bit positions of the twelve-byte map-station
    /// table. Schema three lists each by area: the station table's bit <c>n</c> is station <c>n % 16</c> of area
    /// <c>n / 16</c>, and an area's map is acquired when any bit of its map-station byte is set. The eighth area slot,
    /// which names no area, and the map-station table's padding bytes hold nothing the game reads and are not carried.
    /// The orb, item and door lists are renamed; their numbers are already room-data numbers.
    /// </summary>
    private static void UpgradeSchemaTwo(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject progression = slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required");
            JsonArray bosses = Take(progression, "bossFlagsByAreaIndex", index);
            JsonArray stations = Take(progression, "usedSaveStationBits", index);
            JsonArray maps = Take(progression, "acquiredMapStationBits", index);
            progression["brokenChozoOrbs"] = Take(progression, "roomChozoBits", index);
            progression["collectedItems"] = Take(progression, "collectedItemBits", index);
            progression["openedDoors"] = Take(progression, "openedDoorBits", index);
            var defeated = new JsonArray();
            var used = new JsonArray();
            var acquired = new JsonArray();
            for (int area = 0; area < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; area++)
            {
                string name = JsonNamingPolicy.CamelCase.ConvertName(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area).ToString());
                defeated.Add(new JsonObject
                {
                    ["area"] = name,
                    ["bosses"] = bosses.Count > area ? bosses[area]?.DeepClone() : null,
                });
                var areaStations = new JsonArray();
                bool mapAcquired = false;
                // Each area's word holds a bit per number its station list can give a station; the bits of numbers that
                // name no station Samus uses were never read, and the current schema lists stations only.
                foreach (int bit in Numbers(stations, index))
                {
                    if (bit / UsedStationBitsPerArea == area &&
                        SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Find(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area), bit % UsedStationBitsPerArea) is { } station &&
                        LoadStationDefinitions.Usable.Contains(station))
                        areaStations.Add(bit % UsedStationBitsPerArea);
                }
                foreach (int bit in Numbers(maps, index))
                    mapAcquired |= bit / BitsPerTableByte == area;
                used.Add(new JsonObject { ["area"] = name, ["stations"] = areaStations });
                if (mapAcquired)
                    acquired.Add(name);
            }
            progression["defeatedBosses"] = defeated;
            progression["usedStations"] = used;
            progression["acquiredMaps"] = acquired;
        }
    }

    /// <summary>Removes and returns a schema-two progression list.</summary>
    private static JsonArray Take(JsonObject progression, string name, int slot)
    {
        JsonArray list = progression[name] as JsonArray ??
            throw new InvalidDataException($"slots[{slot}].progression.{name} is required");
        progression.Remove(name);
        return list;
    }

    /// <summary>The integers a schema-two bit-position list holds.</summary>
    private static IEnumerable<int> Numbers(JsonArray list, int slot) =>
        list.Select(entry => entry is JsonValue value && value.TryGetValue(out int number)
            ? number
            : throw new InvalidDataException($"slots[{slot}].progression lists a bit position that is not an integer"));

    /// <summary>Schema one to two: reads the fields schema one left in its preserved image back through the codec.</summary>
    private static void UpgradeSchemaOne(JsonObject root)
    {
        JsonArray pages = root["preservedUntranslatedSram"] as JsonArray ??
            throw new InvalidDataException("Legacy preservedUntranslatedSram is required.");
        byte[] sram = DecodeLegacyPages(pages);
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        if (slots.Count != SaveRamLayout.SlotCount)
            throw new InvalidDataException("slots must contain exactly three entries");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            slot["japaneseText"] = Word(SaveRamLayout.JapaneseTextOffset) != 0;
            ushort Word(int offset) => SaveRamCodec.ReadSlotWord(sram, index, offset);
        }
        SaveFileHistory history = SaveRamCodec.ReadHistory(sram);
        root["gameCompleted"] = history == SaveFileHistory.GameCompleted;
        root["powerOnFoundNoValidSave"] = history == SaveFileHistory.PowerOnFoundNoValidSave;
        root.Remove("preservedUntranslatedSram");
    }

    /// <summary>Bytes per page of the save image schema one preserved; no later schema has pages.</summary>
    private const int PreservationPageSize = 256;

    /// <summary>Validates the older page-preservation format before upgrading named save data.</summary>
    private static byte[] DecodeLegacyPages(JsonArray pages)
    {
        int size = PreservationPageSize;
        int expectedPages = SaveRamLayout.ImageByteCount / size;
        if (pages.Count != expectedPages)
            throw new InvalidDataException($"preservedUntranslatedSram must contain {expectedPages} pages");
        var sram = new byte[SaveRamLayout.ImageByteCount];
        for (int page = 0; page < pages.Count; page++)
        {
            var entry = pages[page]?.Deserialize<LegacySramPage>(PageOptions) ??
                throw new InvalidDataException($"preservedUntranslatedSram[{page}] is null");
            if (!string.Equals(entry.Offset, $"0x{page * size:X4}", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"preservedUntranslatedSram[{page}].offset must be 0x{page * size:X4}");
            byte[] bytes;
            try { bytes = Convert.FromHexString(entry.Bytes ?? throw new InvalidDataException($"preservedUntranslatedSram[{page}].bytes is required")); }
            catch (FormatException error) { throw new InvalidDataException($"preservedUntranslatedSram[{page}].bytes is not hexadecimal", error); }
            if (bytes.Length != size) throw new InvalidDataException($"preservedUntranslatedSram[{page}].bytes must decode to {size} bytes");
            bytes.CopyTo(sram, page * size);
        }
        return sram;
    }

    /// <summary>One fixed-size raw SRAM page in a legacy save awaiting schema upgrade.</summary>
    private sealed record LegacySramPage
    {
        /// <summary>Expected hexadecimal SRAM page origin used to verify ordering.</summary>
        public required string Offset { get; init; }
        /// <summary>Hexadecimal page bytes decoded into the legacy SRAM image.</summary>
        public required string Bytes { get; init; }
    }
}
