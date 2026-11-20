using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The encounters of the room's bosses that the room itself reads (#2194): Crocomire's, which his room's shaking main and
/// his melt's display read; Phantoon's, whose body wave and visibility the room steps and whose display the frame
/// latches; and Mother Brain's, which the room's display, colours, escape text and her brain's late draw read. Each boss
/// attaches its encounter as its body sets up, a body that sets up again replacing it, and the room keeps it for its
/// lifetime, so those reads go on after the boss's own place is gone. No boss's part reads it: each part holds the
/// encounter it joined.
/// </summary>
[RoomLifetime]
internal sealed class RoomBossEncounters
{
    /// <summary>The room's Crocomire, once his body has set up; none in a room without him.</summary>
    internal CrocomireEnemyState? Crocomire { get; private set; }

    /// <summary>The room's Phantoon, once his body has set up; none in a room without him.</summary>
    internal PhantoonEnemyState? Phantoon { get; private set; }

    /// <summary>The room's Mother Brain, once her body has set up; none in a room without her.</summary>
    internal MotherBrainEnemyState? MotherBrain { get; private set; }

    /// <summary>Crocomire's body sets up: the room reads <paramref name="crocomire"/> from now on.</summary>
    internal void Attach(CrocomireEnemyState crocomire)
    {
        Crocomire = Ensure.NotNull(crocomire);
        foreach (Action<CrocomireEnemyState> joins in _crocomireJoins)
            joins(crocomire);
    }

    /// <summary>
    /// What waits on the room's Crocomire, composed with him as his body sets up (#2486, #2554): the room's rumble, which its
    /// main composes before the room's enemies load.
    /// </summary>
    private readonly List<Action<CrocomireEnemyState>> _crocomireJoins = [];

    /// <summary>Has <paramref name="joins"/> run with the room's Crocomire as his body sets up.</summary>
    internal void WhenCrocomireJoins(Action<CrocomireEnemyState> joins) => _crocomireJoins.Add(Ensure.NotNull(joins));

    /// <summary>
    /// What waits on the room's Kraid, composed with him as his body finishes setting up (#2554): the Kraid demo's phase
    /// wait, which the demo composes before the room's enemies load.
    /// </summary>
    private readonly List<Action<KraidEnemyState>> _kraidSetUps = [];

    /// <summary>Has <paramref name="setUp"/> run with the room's Kraid as his body finishes setting up.</summary>
    internal void WhenKraidSetsUp(Action<KraidEnemyState> setUp) => _kraidSetUps.Add(Ensure.NotNull(setUp));

    /// <summary>Kraid's body has finished setting up the encounter <paramref name="kraid"/>: what waits on him runs.</summary>
    internal void KraidSetUp(KraidEnemyState kraid)
    {
        Ensure.NotNull(kraid);
        foreach (Action<KraidEnemyState> setUp in _kraidSetUps)
            setUp(kraid);
    }

    /// <summary>Phantoon's body sets up: the room reads <paramref name="phantoon"/> from now on.</summary>
    internal void Attach(PhantoonEnemyState phantoon) => Phantoon = Ensure.NotNull(phantoon);

    /// <summary>Mother Brain's body sets up: the room reads <paramref name="motherBrain"/> from now on.</summary>
    internal void Attach(MotherBrainEnemyState motherBrain) => MotherBrain = Ensure.NotNull(motherBrain);
}
