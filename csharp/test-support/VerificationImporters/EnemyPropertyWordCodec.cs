using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between an enemy's typed <see cref="EnemyContact"/>, <see cref="EnemyProcessing"/>,
/// <see cref="EnemyPresence"/>, frame-hitbox collision and new-frame flag and the two property words the cartridge
/// stores them in (#1472, #1549), for the checks, fixtures and native-snapshot imports that compare with native memory.
/// The members' values are arbitrary: each property-word bit is named here against its member of the three enums, and
/// the extra-property word's two bits against the two flags; every conversion walks these tables bit by bit. The property
/// word's low byte is not an engine property: a few species keep their own settings there, which the species' settings
/// and state hold typed.
/// </summary>
[EncodingOwner("enemy property and extra-property words")]
internal static class EnemyPropertyWordCodec
{
    /// <summary>The property word's bits that hold engine properties: its high byte.</summary>
    private const int EngineBits = 0xff00;

    /// <summary>Each contact flag's bit in the property word.</summary>
    private static readonly Dictionary<EnemyContact, int> ContactBits = new()
    {
        [EnemyContact.IgnoresSamus] = 1 << 10,
        [EnemyContact.BlocksPlasmaBeam] = 1 << 12,
        [EnemyContact.SolidToSamus] = 1 << 15,
    };

    /// <summary>Each processing flag's bit in the property word.</summary>
    private static readonly Dictionary<EnemyProcessing, int> ProcessingBits = new()
    {
        [EnemyProcessing.OffScreen] = 1 << 11,
        [EnemyProcessing.Animates] = 1 << 13,
    };

    /// <summary>Each presence flag's bit in the property word.</summary>
    private static readonly Dictionary<EnemyPresence, int> PresenceBits = new()
    {
        [EnemyPresence.Hidden] = 1 << 8,
        [EnemyPresence.Deleted] = 1 << 9,
        [EnemyPresence.RespawnsIfKilled] = 1 << 14,
    };

    /// <summary>The extra-property word's bit for <see cref="RoomEnemySlot.CollidesByFrameHitboxes"/>.</summary>
    private const int CollidesByFrameHitboxesBit = 1 << 2;

    /// <summary>The extra-property word's bit for <see cref="EnemyAnimationClock.ShowsNewFrame"/>.</summary>
    private const int NewInstructionFrameBit = 1 << 15;

    /// <summary>The extra-property word's named bits.</summary>
    private const int ExtraBits = CollidesByFrameHitboxesBit | NewInstructionFrameBit;

    /// <summary>The property word holding these engine properties, with a clear low byte.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A value holds a bit no member names.</exception>
    public static int EncodePropertyWord(EnemyContact contact, EnemyProcessing processing, EnemyPresence presence) =>
        Encode(contact) | Encode(processing) | Encode(presence);

    /// <summary>The property word holding <paramref name="state"/>'s engine properties, with a clear low byte.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A part holds a bit no member names.</exception>
    public static int EncodePropertyWord(EnemyPropertyState state) =>
        EncodePropertyWord(state.Contact, state.Processing, state.Presence);

    /// <summary>The property word holding <paramref name="slot"/>'s engine properties, with a clear low byte.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A part holds a bit no member names.</exception>
    public static int EncodePropertyWord(RoomEnemySlot slot) => EncodePropertyWord(EnemyPropertyState.Of(slot));

    /// <summary>The extra-property word holding <paramref name="state"/>'s two flags.</summary>
    public static int EncodeExtraPropertyWord(EnemyPropertyState state) =>
        (state.CollidesByFrameHitboxes ? CollidesByFrameHitboxesBit : 0) | (state.NewInstructionFrame ? NewInstructionFrameBit : 0);

    /// <summary>The extra-property word holding <paramref name="slot"/>'s two flags.</summary>
    public static int EncodeExtraPropertyWord(RoomEnemySlot slot) => EncodeExtraPropertyWord(EnemyPropertyState.Of(slot));

    /// <summary>The state a pair of stored words holds, every set bit of each named.</summary>
    /// <exception cref="InvalidDataException">
    /// The property word's low byte holds species settings, or the extra-property word holds a bit no flag names.
    /// </exception>
    public static EnemyPropertyState Decode(int propertyWord, int extraPropertyWord) =>
        SpeciesBitsOf(propertyWord) == 0
            ? DecodeWithSpeciesBits(propertyWord, extraPropertyWord)
            : throw new InvalidDataException($"Enemy property word ${propertyWord:X4} holds species settings in its low byte.");

    /// <summary>
    /// The state a pair of stored words holds, leaving the species settings of the property word's low byte to the
    /// species' import; every set bit of the extra-property word is named.
    /// </summary>
    /// <exception cref="InvalidDataException">The extra-property word holds a bit no flag names.</exception>
    public static EnemyPropertyState DecodeWithSpeciesBits(int propertyWord, int extraPropertyWord) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, extraPropertyWord) & ~ExtraBits) == 0
            ? EngineStateOf(propertyWord, extraPropertyWord)
            : throw new InvalidDataException(
                $"Enemy extra-property word ${extraPropertyWord:X4} holds a bit no additional property names.");

    /// <summary>
    /// The engine state of a pair of stored words as native memory holds them: the species settings of the property
    /// word's low byte are left to the species' import, and extra-property bits no port behaviour reads are dropped.
    /// </summary>
    public static EnemyPropertyState EngineStateOf(int propertyWord, int extraPropertyWord)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, propertyWord);
        Ensure.BetweenInclusive(0, ushort.MaxValue, extraPropertyWord);
        EnemyContact contact = EnemyContact.None;
        foreach (KeyValuePair<EnemyContact, int> entry in ContactBits)
            if ((propertyWord & entry.Value) != 0)
                contact |= entry.Key;
        EnemyProcessing processing = EnemyProcessing.None;
        foreach (KeyValuePair<EnemyProcessing, int> entry in ProcessingBits)
            if ((propertyWord & entry.Value) != 0)
                processing |= entry.Key;
        EnemyPresence presence = EnemyPresence.None;
        foreach (KeyValuePair<EnemyPresence, int> entry in PresenceBits)
            if ((propertyWord & entry.Value) != 0)
                presence |= entry.Key;
        return new EnemyPropertyState(contact, processing, presence,
            CollidesByFrameHitboxes: (extraPropertyWord & CollidesByFrameHitboxesBit) != 0,
            NewInstructionFrame: (extraPropertyWord & NewInstructionFrameBit) != 0);
    }

    /// <summary>Stores on <paramref name="slot"/> the state a pair of stored words holds, every set bit of each named.</summary>
    /// <exception cref="InvalidDataException">
    /// The property word's low byte holds species settings, or the extra-property word holds a bit no flag names.
    /// </exception>
    public static void DecodeInto(RoomEnemySlot slot, int propertyWord, int extraPropertyWord) =>
        Decode(propertyWord, extraPropertyWord).ApplyTo(slot);

    /// <summary>
    /// Stores on <paramref name="slot"/> the contact, processing and presence a stored property word holds, every set bit
    /// named, leaving the slot's extended-frame and new-frame flags as they are.
    /// </summary>
    /// <exception cref="InvalidDataException">The word's low byte holds species settings.</exception>
    public static void DecodePropertyWordInto(RoomEnemySlot slot, int propertyWord)
    {
        ArgumentNullException.ThrowIfNull(slot);
        EnemyPropertyState state = Decode(propertyWord, extraPropertyWord: 0);
        slot.Contact = state.Contact;
        slot.Processing = state.Processing;
        slot.Presence = state.Presence;
    }

    /// <summary>The species settings a stored property word keeps in its low byte.</summary>
    public static int SpeciesBitsOf(int propertyWord) => Ensure.BetweenInclusive(0, ushort.MaxValue, propertyWord) & ~EngineBits;

    /// <summary>The property-word bits of these contact flags.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="contact"/> holds a bit no member names.</exception>
    private static int Encode(EnemyContact contact)
    {
        int word = 0;
        EnemyContact encoded = EnemyContact.None;
        foreach (KeyValuePair<EnemyContact, int> entry in ContactBits)
        {
            if ((contact & entry.Key) == EnemyContact.None)
                continue;
            word |= entry.Value;
            encoded |= entry.Key;
        }
        return encoded == contact
            ? word
            : throw new ArgumentOutOfRangeException(nameof(contact), contact, "The value holds a bit no contact flag names.");
    }

    /// <summary>The property-word bits of these processing flags.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="processing"/> holds a bit no member names.</exception>
    private static int Encode(EnemyProcessing processing)
    {
        int word = 0;
        EnemyProcessing encoded = EnemyProcessing.None;
        foreach (KeyValuePair<EnemyProcessing, int> entry in ProcessingBits)
        {
            if ((processing & entry.Key) == EnemyProcessing.None)
                continue;
            word |= entry.Value;
            encoded |= entry.Key;
        }
        return encoded == processing
            ? word
            : throw new ArgumentOutOfRangeException(nameof(processing), processing, "The value holds a bit no processing flag names.");
    }

    /// <summary>The property-word bits of these presence flags.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="presence"/> holds a bit no member names.</exception>
    private static int Encode(EnemyPresence presence)
    {
        int word = 0;
        EnemyPresence encoded = EnemyPresence.None;
        foreach (KeyValuePair<EnemyPresence, int> entry in PresenceBits)
        {
            if ((presence & entry.Key) == EnemyPresence.None)
                continue;
            word |= entry.Value;
            encoded |= entry.Key;
        }
        return encoded == presence
            ? word
            : throw new ArgumentOutOfRangeException(nameof(presence), presence, "The value holds a bit no presence flag names.");
    }
}
