using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The shades the import pre-colours Mother Brain's encounter in (#1512): each colour load of the encounter over her
/// body, brain, legs and spawned objects and over her room's pictures, which each owner lays in the order it loads them.
/// </summary>
public static class MotherBrainShades
{
    /// <summary>Her body, brain and rear legs in health band <paramref name="band"/>, zero through three.</summary>
    public static PictureShade Health(int band) => new("mother-brain-health", band);

    /// <summary>Her body, brain and rear legs at rainbow phase <paramref name="frame"/>.</summary>
    public static PictureShade Rainbow(int frame) => new("mother-brain-rainbow", frame);

    /// <summary>Her body, brain and rear legs restored after the rainbow beam.</summary>
    public static PictureShade Restored { get; } = new("mother-brain-restored", 0);

    /// <summary>Her body, brain and drained rear legs at step <paramref name="frame"/> of the Baby's drain to grey.</summary>
    public static PictureShade ToGrey(int frame) => new("mother-brain-to-grey", frame);

    /// <summary>Her body, brain and drained rear legs at step <paramref name="frame"/> of her revival from grey.</summary>
    public static PictureShade FromGrey(int frame) => new("mother-brain-from-grey", frame);

    /// <summary>Her brain's three fading colours at step <paramref name="frame"/> of the fake death's fade to grey.</summary>
    public static PictureShade FakeDeathToGrey(int frame) => new("mother-brain-fake-death-to-grey", frame);

    /// <summary>Her brain's three fading colours at step <paramref name="frame"/> of the fake death's revival.</summary>
    public static PictureShade FakeDeathFromGrey(int frame) => new("mother-brain-fake-death-from-grey", frame);

    /// <summary>Her phase-two attack and rear-leg colours, which her ascent loads.</summary>
    public static PictureShade PhaseTwo { get; } = new("mother-brain-phase-two", 0);

    /// <summary>The glass shards' and tube projectiles' colours her room's entry loads.</summary>
    public static PictureShade RoomEntry { get; } = new("mother-brain-room-entry", 0);

    /// <summary>Her body, brain and rear legs at step <paramref name="frame"/> of her death's body fade.</summary>
    public static PictureShade DeathBody(int frame) => new("mother-brain-death-body", frame);

    /// <summary>Her detached brain, drawn in the corpse's colours, at step <paramref name="frame"/> of its fade to grey.</summary>
    public static PictureShade CorpseFade(int frame) => new("mother-brain-corpse-fade", frame);

    /// <summary>Her room at step <paramref name="frame"/> of the fake death's flash.</summary>
    public static PictureShade Flash(int frame) => new("mother-brain-room-flash", frame);

    /// <summary>Her room's steady grey colours after the flash stops.</summary>
    public static PictureShade FinalRoom { get; } = new("mother-brain-final-room", 0);

    /// <summary>Her room's lights cleared to black as the Baby dies.</summary>
    public static PictureShade LightsOut { get; } = new("mother-brain-lights-out", 0);

    /// <summary>Her room's lights at step <paramref name="frame"/> of their recovery, zero through six.</summary>
    public static PictureShade RecoveryLights(int frame) => new("mother-brain-recovery-lights", frame);

    /// <summary>The Baby's colours the cutscene loads as it arrives.</summary>
    public static PictureShade BabyArrives { get; } = new("mother-brain-baby-arrives", 0);

    /// <summary>The Baby at shade <paramref name="shade"/>, one through six, of its fade to black.</summary>
    public static PictureShade BabyFade(int shade) => new("mother-brain-baby-fade", shade);
}
