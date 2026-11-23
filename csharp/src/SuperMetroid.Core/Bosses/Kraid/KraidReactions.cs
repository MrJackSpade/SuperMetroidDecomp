using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The reactions of Kraid's own that its frames' hitboxes and its parts' definitions name (#1759, #2558). Only Kraid's
/// own behaviour tells them apart.
/// </summary>
internal static partial class KraidReactions
{
    /// <summary>The touch reaction of Kraid's body and feet: the hitboxes his arm's part and his foot hold over his body (#2221).</summary>
    internal static EnemyReaction BodyTouch => SpeciesOwnReactions.KraidBodyTouch;

    /// <summary>The arm's touch reaction, which pushes Samus away.</summary>
    internal static EnemyReaction ArmTouch => SpeciesOwnReactions.KraidArmTouch;

    /// <summary>The arm's shot reaction.</summary>
    internal static EnemyReaction ArmShot { get; } = EnemyReaction.OfSpecies("Kraid.ArmShot");

    /// <summary>The touch reaction of his nails' own, which their definitions name (#2558).</summary>
    internal static EnemyReaction NailTouch => SpeciesOwnReactions.KraidNailTouch;

    /// <summary>Fake Kraid's own touch reaction, which his definition names (#2558).</summary>
    internal static EnemyReaction FakeKraidTouch => SpeciesOwnReactions.KraidFakeKraidTouch;

    /// <summary>Fake Kraid's own shot reaction, which his definition names (#2558).</summary>
    internal static EnemyReaction FakeKraidShot => SpeciesOwnReactions.KraidFakeKraidShot;
}
