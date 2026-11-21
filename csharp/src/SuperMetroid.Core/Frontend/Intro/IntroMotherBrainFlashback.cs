using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The intro's Mother Brain flashback while it runs (#2554): the Samus its demo drives, Mother Brain's sprite and the
/// explosions her fourth hit spawns, the Rinkas, the flashback's collision room, its own progression and its scripted
/// input. Its setup creates it whole and the discovery's setup releases it whole, so nothing of it is read outside it.
/// </summary>
/// <param name="samus">The Samus the flashback's demo drives.</param>
/// <param name="motherBrain">Mother Brain's sprite.</param>
/// <param name="rinkas">The flashback's Rinkas.</param>
/// <param name="terrain">The flashback's collision room.</param>
/// <param name="progression">The flashback's own progression, which its shots act on; the demo's shots never change the game's.</param>
/// <param name="demoInput">The flashback's scripted controller input.</param>
[RoomLifetime]
internal sealed class IntroMotherBrainFlashback(
    SamusState samus,
    IntroMotherBrainSpriteState motherBrain,
    IntroRinkaSystem rinkas,
    PlainTerrain terrain,
    SharedGameState progression,
    DemoInputState demoInput)
{
    /// <summary>The Samus the flashback's demo drives.</summary>
    internal SamusState Samus { get; } = Ensure.NotNull(samus);

    /// <summary>Mother Brain's sprite.</summary>
    internal IntroMotherBrainSpriteState MotherBrain { get; } = Ensure.NotNull(motherBrain);

    /// <summary>The explosions Mother Brain's fourth hit spawns; none before it.</summary>
    internal IntroMotherBrainExplosionSystem? Explosions { get; set; }

    /// <summary>The flashback's Rinkas.</summary>
    internal IntroRinkaSystem Rinkas { get; } = Ensure.NotNull(rinkas);

    /// <summary>The flashback's collision room.</summary>
    internal PlainTerrain Terrain { get; } = Ensure.NotNull(terrain);

    /// <summary>The flashback's own progression, which its shots act on.</summary>
    internal SharedGameState Progression { get; } = Ensure.NotNull(progression);

    /// <summary>The flashback's scripted controller input.</summary>
    internal DemoInputState DemoInput { get; } = Ensure.NotNull(demoInput);
}
