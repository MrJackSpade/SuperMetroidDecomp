using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between a typed <see cref="RoomEnemyPlacement"/> and the words of a native sixteen-byte population
/// entry (#1472), for the checks that compare compiled populations with the cartridge and the fixtures that seed native
/// records. The definition pointer is the caller's: this type takes and gives the resolved identity.
/// </summary>
[EncodingOwner("room enemy population entry words")]
internal static class RoomEnemyPopulationRecordCodec
{
    /// <summary>The typed entry the words of a native population entry describe.</summary>
    /// <exception cref="InvalidDataException">A word holds a setting or property the entry cannot carry.</exception>
    public static RoomEnemyPlacement Decode(
        EnemyDefinitionId enemy, int x, int y, int initialization, int properties, int extraProperties,
        int parameter1, int parameter2)
    {
        bool extraIsSettings = EnemySpawnSettingsCodec.TakesExtraPropertySettings(enemy);
        var words = new PopulationWords(initialization, EnemyPropertyWordCodec.SpeciesBitsOf(properties),
            extraIsSettings ? extraProperties : 0, parameter1, parameter2);
        EnemySpawnSettings settings = EnemySpawnSettingsCodec.Decode(enemy, words, new Position16<RoomPixels>(checked((ushort)x), checked((ushort)y)));
        if (words.SpeciesPropertyBits != 0 && EnemySpawnSettingsCodec.Encode(settings).SpeciesPropertyBits == 0)
            throw new InvalidDataException($"Enemy {enemy}'s property word ${properties:X4} holds species bits its settings do not take.");
        EnemyPropertyState state = EnemyPropertyWordCodec.DecodeWithSpeciesBits(properties, extraIsSettings ? 0 : extraProperties);
        if (state.NewInstructionFrame)
        {
            throw new InvalidDataException(
                $"Enemy {enemy}'s extra-property word ${extraProperties:X4} sets the new-frame flag, which an authored placement never sets.");
        }
        return new RoomEnemyPlacement(
            enemy,
            PlacingOf(enemy, new Position16<RoomPixels>(checked((ushort)x), checked((ushort)y))),
            state.Contact,
            state.Processing,
            state.Presence,
            state.CollidesByFrameHitboxes,
            settings);
    }

    /// <summary>
    /// Where an entry of <paramref name="enemy"/> whose position words are <paramref name="position"/> places it: a
    /// Zebetite's entry is placed by its setup, which stands it where its generation stands, and its words hold the room
    /// origin; every other entry at its words' position.
    /// </summary>
    /// <exception cref="InvalidDataException">A Zebetite's entry holds a position other than the room origin.</exception>
    public static EnemyPlacing PlacingOf(EnemyDefinitionId enemy, Position16<RoomPixels> position)
    {
        if (enemy != EnemyDefinitionId.Zebetite)
            return EnemyPlacing.At(position);
        if (position != Position16<RoomPixels>.Zero)
            throw new InvalidDataException($"A Zebetite's population entry holds position {position}, where its setup places it.");
        return EnemyPlacing.BySetup;
    }

    /// <summary>The position words of an entry placed by <paramref name="placing"/>: an entry its setup places holds the room origin.</summary>
    public static Position16<RoomPixels> PositionWordsOf(EnemyPlacing placing) => Ensure.NotNull(placing) switch
    {
        EnemyPlacedAt at => at.Position,
        EnemyPlacedBySetup => Position16<RoomPixels>.Zero,
        _ => throw new InvalidDataException($"Undefined {nameof(EnemyPlacing)} {placing}."),
    };

    /// <summary>The words of a native population entry for a typed entry, after the definition pointer.</summary>
    public static PopulationEntryWords Encode(RoomEnemyPlacement record)
    {
        PopulationWords settings = EnemySpawnSettingsCodec.Encode(record.DefinitionId, record.SpawnSettings);
        Position16<RoomPixels> position = PositionWordsOf(record.Placing);
        return new PopulationEntryWords(
            position.X, position.Y, settings.Initialization,
            EnemyPropertyWordCodec.EncodePropertyWord(record.Contact, record.Processing, record.Presence) | settings.SpeciesPropertyBits,
            EnemyPropertyWordCodec.EncodeExtraPropertyWord(
                EnemyPropertyState.None with { CollidesByFrameHitboxes = record.CollidesByFrameHitboxes }) | settings.ExtraProperties,
            settings.Parameter1, settings.Parameter2);
    }
}
