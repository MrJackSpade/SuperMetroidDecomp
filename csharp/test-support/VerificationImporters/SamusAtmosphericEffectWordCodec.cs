using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec of the cartridge's four atmospheric effect places (#1470, #1556). Each place holds a word with the effect in
/// its high byte, numbered in the order of the cartridge's per-effect tables, and the animation frame in its low byte, and
/// a timer whose values from 32769 up (and zero, which wraps there) wait hidden until it counts down to exactly 32768, which
/// shows the effect. The game holds Samus's particles as a list and keeps of the places only the waits her breath bubbles
/// and lava spray keep; this is the one place the packing, the timer's hidden range and the place numbers are known, for
/// imports and checks of native state.
/// </summary>
[EncodingOwner("Samus atmospheric effect places")]
internal static class SamusAtmosphericEffectWordCodec
{
    /// <summary>How many places the cartridge keeps.</summary>
    public const int PlaceCount = 4;

    /// <summary>The bits of the word's animation frame, below its effect.</summary>
    private const int FrameBits = 8;

    /// <summary>The timer value that shows a hidden effect; a timer above it waits hidden.</summary>
    private const int HiddenTimerShows = 32768;

    /// <summary>The place whose occupant holds up breath bubbles: the bubble producer tests that it is empty.</summary>
    private const int BubblePlace = 2;

    /// <summary>The place whose occupant can hold up lava spray: the spray producer tests one bit of its effect number.</summary>
    private const int SprayLeadPlace = 0;

    /// <summary>The effect number that bit tests, which the spray, the bubbles and both dusts set.</summary>
    private const int SprayHoldingEffectBit = 4;

    /// <summary>The first place whose lava spray drifts left; the places before it drift right.</summary>
    private const int FirstLeftDriftingPlace = 2;

    /// <summary>The effect number two, whose sprite list the game lacks and which no producer starts.</summary>
    private const int UnstartedEffect = 2;

    /// <summary>The particle kinds by the word's effect number; number zero is an empty place.</summary>
    private static readonly SamusParticleKind?[] EffectOrder =
    [
        null, SamusParticleKind.SurfaceSplash, null, SamusParticleKind.DivingSplash, SamusParticleKind.LavaSpray,
        SamusParticleKind.Bubbles, SamusParticleKind.Dust, SamusParticleKind.Dust,
    ];

    /// <summary>The kind of particle a place word records, or none for an empty place.</summary>
    /// <param name="word">An atmospheric effect place word.</param>
    /// <exception cref="InvalidDataException">The high byte names no effect, or the effect no producer starts.</exception>
    public static SamusParticleKind? DecodeKind(int word)
    {
        int number = EffectNumber(word);
        if (number == UnstartedEffect)
            throw new InvalidDataException($"Atmospheric effect word ${word:X4} names the effect no producer starts.");
        return EffectOrder[number];
    }

    /// <summary>The animation step a place word records.</summary>
    /// <param name="word">An atmospheric effect place word.</param>
    public static int DecodeStep(int word) => word & ((1 << FrameBits) - 1);

    /// <summary>How a place's timer starts its particle: shown with its updates left, or hidden for its updates.</summary>
    /// <param name="timer">An atmospheric effect place timer, 0 through 65535.</param>
    public static SamusParticleStart DecodeStart(int timer)
    {
        if (timer is < 0 or > ushort.MaxValue)
            throw new InvalidDataException($"Atmospheric effect timer {timer} is outside sixteen bits.");
        if (timer is >= 1 and <= HiddenTimerShows)
            return SamusParticleStart.Shown(timer);
        // Zero counts down through 65535 before reaching the hidden range's end.
        return SamusParticleStart.Hidden(timer == 0 ? HiddenTimerShows : timer - HiddenTimerShows);
    }

    /// <summary>
    /// Restores one place of the cartridge's state: its particle, if any, joins <paramref name="particles"/>, and the bubble
    /// place and the spray's leading place hold up <paramref name="waits"/> for as long as their particles show.
    /// </summary>
    /// <param name="particles">Samus's particles; places are restored in order, so later places draw in front.</param>
    /// <param name="waits">Samus's breath bubble and lava spray waits, cleared before the first place is restored.</param>
    /// <param name="place">The place, 0 through 3.</param>
    /// <param name="word">The place's effect word.</param>
    /// <param name="timer">The place's timer.</param>
    /// <param name="position">The place's position in whole room pixels.</param>
    public static void Restore(SamusParticles particles, BreathBubbleAndLavaSprayWaits waits, int place, int word, int timer,
        Position16<RoomPixels> position)
    {
        Ensure.NotNull(particles);
        Ensure.NotNull(waits);
        if (DecodeKind(word) is not SamusParticleKind kind)
            return;
        SamusParticleStart start = DecodeStart(timer);
        int step = DecodeStep(word);
        particles.Add(new SamusParticle(kind, step, start, position, Drift(kind, place)));
        int updates = SamusParticleAnimations.UpdatesUntilFinished(kind, step, start);
        if (Place(place) == BubblePlace)
            waits.HoldBubbles(updates);
        if (place == SprayLeadPlace && HoldsSpray(word))
            waits.HoldSpray(updates);
    }

    /// <summary>The breath bubbles' wait the cartridge's bubble place implies: its particle's updates left, or zero.</summary>
    /// <param name="word">The bubble place's effect word.</param>
    /// <param name="timer">The bubble place's timer.</param>
    public static int BubbleWait(int word, int timer) => UpdatesLeft(word, timer);

    /// <summary>The lava spray's wait the cartridge's leading place implies: a holding particle's updates left, or zero.</summary>
    /// <param name="word">The leading place's effect word.</param>
    /// <param name="timer">The leading place's timer.</param>
    public static int SprayWait(int word, int timer) => HoldsSpray(word) ? UpdatesLeft(word, timer) : 0;

    /// <summary>The place the bubble producer tests.</summary>
    public static int BubbleWaitPlace => BubblePlace;

    /// <summary>The place the spray producer tests.</summary>
    public static int SprayWaitPlace => SprayLeadPlace;

    /// <summary>The updates a place's particle still shows, or zero for an empty place.</summary>
    private static int UpdatesLeft(int word, int timer) => DecodeKind(word) is SamusParticleKind kind
        ? SamusParticleAnimations.UpdatesUntilFinished(kind, DecodeStep(word), DecodeStart(timer))
        : 0;

    /// <summary>Whether a place word's effect sets the bit the spray producer tests.</summary>
    private static bool HoldsSpray(int word) => (EffectNumber(word) & SprayHoldingEffectBit) != 0;

    /// <summary>The drift of a particle restored from <paramref name="place"/>.</summary>
    private static Offset16<RoomPixels> Drift(SamusParticleKind kind, int place) => kind switch
    {
        SamusParticleKind.Dust => SamusAtmosphericAnimationDefinitions.DustDrift,
        SamusParticleKind.LavaSpray => new Offset16<RoomPixels>(Place(place) < FirstLeftDriftingPlace ? 1 : -1,
            SamusAtmosphericAnimationDefinitions.LavaSprayRise),
        SamusParticleKind.SurfaceSplash or SamusParticleKind.DivingSplash or SamusParticleKind.Bubbles => Offset16<RoomPixels>.Zero,
        _ => throw new InvalidDataException($"Undefined particle kind {kind}."),
    };

    /// <summary>A place number, 0 through 3.</summary>
    private static int Place(int place) => place is >= 0 and < PlaceCount
        ? place
        : throw new ArgumentOutOfRangeException(nameof(place), place, "The cartridge keeps four atmospheric effect places.");

    /// <summary>The effect number of a place word's high byte.</summary>
    private static int EffectNumber(int word) =>
        (word >> FrameBits) is var number and >= 0 && number < EffectOrder.Length
            ? number
            : throw new InvalidDataException($"Atmospheric effect word ${word:X4} names no effect.");
}
