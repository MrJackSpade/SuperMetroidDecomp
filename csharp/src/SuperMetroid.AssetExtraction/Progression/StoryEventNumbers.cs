using SuperMetroid.Core;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The single conversion between a <see cref="StoryEvent"/> and the number the cartridge gives that story event: its bit
/// in the saved event table, and the operand room data and object programs carry. The progression table codec, the
/// legacy save translation and the checks that compare with native state convert through it; nothing in the game knows
/// these numbers.
/// </summary>
[EncodingOwner("story event numbering")]
public static class StoryEventNumbers
{
    /// <summary>How many numbers the event table holds, 0 through 63.</summary>
    public const int NumberCount = 64;

    /// <summary>
    /// The events in the order the cartridge numbers them, from zero; null where a number belongs to no story event.
    /// Numbers three through five are such numbers: the cartridge keeps the Zebetite count
    /// (<see cref="PersistentProgress.ZebetitesDestroyed"/>) there, which <see cref="ProgressionTableCodec"/> reads and
    /// writes. Numbers from <see cref="Numbered"/>'s length up are used by no story event.
    /// </summary>
    private static readonly StoryEvent?[] Numbered =
    [
        StoryEvent.ZebesAwake, StoryEvent.ShitroidAteSidehopper, StoryEvent.MotherBrainGlassDestroyed,
        null, null, null,
        StoryEvent.PhantoonStatueGrey, StoryEvent.RidleyStatueGrey, StoryEvent.DraygonStatueGrey,
        StoryEvent.KraidStatueGrey, StoryEvent.TourianUnlocked, StoryEvent.MaridiaNoobTubeBroken,
        StoryEvent.LowerNorfairChozoLoweredAcid, StoryEvent.ShaktoolClearedPath, StoryEvent.ZebesTimebombSet,
        StoryEvent.CrittersEscaped, StoryEvent.FirstMetroidHallCleared, StoryEvent.FirstMetroidShaftCleared,
        StoryEvent.SecondMetroidHallCleared, StoryEvent.SecondMetroidShaftCleared, StoryEvent.ShitroidEncounterMusicSkipped,
        StoryEvent.OutranSpeedBoosterLavaquake,
    ];

    /// <summary>The number the cartridge gives <paramref name="storyEvent"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="storyEvent"/> is not a defined story event.</exception>
    public static int Number(this StoryEvent storyEvent) =>
        Array.IndexOf(Numbered, storyEvent) is var number and >= 0
            ? number
            : throw new ArgumentOutOfRangeException(nameof(storyEvent), storyEvent, "The event has no cartridge number.");

    /// <summary>The story event the cartridge numbers <paramref name="number"/>, when one uses it.</summary>
    /// <param name="number">A number of the event table.</param>
    /// <param name="storyEvent">The story event numbered so, when there is one.</param>
    /// <returns>False for a number of the table that no story event uses.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the event table.</exception>
    public static bool TryStoryEvent(int number, out StoryEvent storyEvent)
    {
        _ = Ensure.BetweenInclusive(0, NumberCount - 1, number);
        StoryEvent? named = number < Numbered.Length ? Numbered[number] : null;
        storyEvent = named ?? default;
        return named is not null;
    }

    /// <summary>The story event the cartridge numbers <paramref name="number"/>, which room data and object programs carry.</summary>
    /// <exception cref="InvalidDataException">No story event uses <paramref name="number"/>.</exception>
    public static StoryEvent Of(int number) => TryStoryEvent(number, out StoryEvent storyEvent)
        ? storyEvent
        : throw new InvalidDataException($"Event number {number} names no story event.");
}
