namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The counter cadences and phases of the intro (#1606): the dancing baby's cries, Mother Brain's flash and shake, and the laboratory's pan.</summary>
internal static class IntroCadenceDefinitions
{
    /// <summary>
    /// The counts of updates danced on which the discovered baby Metroid cries: every 64th, so it cries at 64 and at
    /// 128, when its dance ends.
    /// </summary>
    internal static readonly UpdateCadence BabyCriesEverySixtyFourthDanceUpdate = new(64, 0);

    /// <summary>
    /// The phase of the intro Mother Brain's hurt flash timer on which she shows her own colours: set on its odd counts,
    /// so she flashes white on the even ones.
    /// </summary>
    internal static readonly UpdateCounterField MotherBrainHurtShowsOwnColours = new(0, 1);

    /// <summary>
    /// The phase of the cinematic frame counter on which the intro Mother Brain's room shakes up: set on odd updates; on
    /// even ones it shakes down.
    /// </summary>
    internal static readonly UpdateCounterField MotherBrainRoomShakesUp = new(0, 1);

    /// <summary>
    /// The updates left of the crossfade into the laboratory on which the delivery and examination scenes pan the
    /// laboratory one pixel with the baby: every fourth, down to its last.
    /// </summary>
    internal static readonly UpdateCadence LaboratoryStepEveryFourthUpdate = new(4, 0);

    /// <summary>
    /// The updates left of a crossfade, its last included, on which it takes a step: every fourth counted back from its
    /// last, the last among them, so its thirty-two steps end on that last update.
    /// </summary>
    internal static readonly UpdateCadence CrossfadeStepEveryFourthUpdate = new(4, 1);
}
