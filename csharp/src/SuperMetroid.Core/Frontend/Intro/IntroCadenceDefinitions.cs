namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The counter cadences and phases of the intro (#1606): the dancing baby's cries, Mother Brain's flash and shake, and the laboratory's pan.</summary>
internal static partial class IntroCadenceDefinitions
{
    /// <summary>
    /// The counts of updates danced on which the discovered baby Metroid cries: each period's first place, the last as its
    /// crying dance ends.
    /// </summary>
    internal static readonly UpdateCadence BabyCriesInDance = new(64, 0);

    /// <summary>
    /// The phase of the intro Mother Brain's hurt flash timer on which she shows her own colours: on its odd counts,
    /// so she flashes white on the even ones.
    /// </summary>
    internal static readonly UpdateAlternation MotherBrainHurtShowsOwnColours = new(1);

    /// <summary>
    /// The phase of the cinematic frame counter on which the intro Mother Brain's room shakes up: on odd updates; on
    /// even ones it shakes down.
    /// </summary>
    internal static readonly UpdateAlternation MotherBrainRoomShakesUp = new(1);

    /// <summary>
    /// The updates left of the crossfade into the laboratory on which the delivery and examination scenes pan the
    /// laboratory one pixel with the baby: every fourth, down to its last.
    /// </summary>
    internal static readonly UpdateCadence LaboratoryStep = new(4, 0);

    /// <summary>
    /// The updates left of a crossfade, its last included, on which it takes a step: every fourth counted back from its
    /// last, the last among them, so its thirty-two steps end on that last update.
    /// </summary>
    internal static readonly UpdateCadence CrossfadeStep = new(4, 1);
}
