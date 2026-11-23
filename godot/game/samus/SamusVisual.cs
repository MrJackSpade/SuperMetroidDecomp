using Engine.Actors;
using Engine.Actors.Components;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// Shows Samus: the timeline for what she is doing, the way she faces and where she aims, in the suit she wears, with
/// her flicker after a hit and her glows. It is stepped once per update, after she has acted, and only shows; nothing
/// in play reads it.
/// </summary>
/// <remarks>
/// Her timelines are named for her look, then the way she faces, then her aim: "standing_right_up". Where she has no
/// timeline of the whole name, the one without the aim is used, and failing that the one of the look alone. In a
/// picture's path, "{suit}" stands for the suit she wears: "power", "varia" or "gravity".
/// </remarks>
public partial class SamusVisual : Node2D
{
    /// <summary>The tint while a shine is stored.</summary>
    [Export]
    public Color StoredShineTint { get; set; } = new(1.4f, 1.4f, 1.0f);

    /// <summary>The tint while she is boosting or shinesparking.</summary>
    [Export]
    public Color BoostTint { get; set; } = new(1.0f, 1.3f, 1.6f);

    /// <summary>The tint while her beam is fully charged.</summary>
    [Export]
    public Color ChargedTint { get; set; } = new(1.3f, 1.3f, 1.3f);

    /// <summary>How many colours the glow of the hyper beam runs through before it comes round.</summary>
    [Export]
    public int GlowSteps { get; set; } = 12;

    /// <summary>How strongly that glow colours her, from none to wholly.</summary>
    [Export]
    public float GlowStrength { get; set; } = 0.6f;

    /// <summary>How bright that glow is.</summary>
    [Export]
    public float GlowBrightness { get; set; } = 1.4f;

    private TimelinePlayer? _player;
    private string _playing = "";

    /// <summary>Shows <paramref name="samus"/> as she is at the end of this update.</summary>
    public void Show(Samus samus)
    {
        ActorVisual? picture = GetNodeOrNull<ActorVisual>("Picture");
        if (picture is null)
            return;

        string name = TimelineFor(samus);
        if (name.Length == 0)
        {
            picture.Show("");
            return;
        }

        if (name != _playing || _player is null)
        {
            _playing = name;
            _player = new TimelinePlayer(samus.Timelines.Get(name));
        }
        else
        {
            _player.Step();
        }

        string suit = samus.Inventory.IsOn(Equipment.GravitySuit) ? "gravity" : samus.Inventory.IsOn(Equipment.VariaSuit) ? "varia" : "power";
        picture.Show(_player.Current.Picture.Replace("{suit}", suit, StringComparison.Ordinal));

        // After a hit she flickers: shown on one update and hidden on the next.
        picture.SetShown(!samus.IsInvincible || samus.InvincibleUpdatesLeft % 2 == 0);
        picture.Tint(
            samus.State is DrainedState { Glows: true } drained ? GlowTint(drained.GlowStep)
            : samus.State is ShinesparkState || samus.IsBoosting ? BoostTint
            : samus.HasStoredShine ? StoredShineTint
            : samus.Weapons.IsCharged ? ChargedTint
            : Colors.White);
    }

    /// <summary>The colour of one step of the glow that runs through every colour.</summary>
    private Color GlowTint(int step) =>
        Color.FromHsv((float)(step % GlowSteps) / GlowSteps, GlowStrength, 1f) * new Color(GlowBrightness, GlowBrightness, GlowBrightness);

    /// <summary>The name of the timeline that shows her now, or empty when her data has none for it.</summary>
    private static string TimelineFor(Samus samus)
    {
        string look = SamusLooks.Of(samus);
        string facing = samus.Facing == Facing.Right ? "right" : "left";
        string aim = Samus.AimName(samus.Aim);
        string[] candidates = [$"{look}_{facing}_{aim}", $"{look}_{facing}", look];
        return candidates.FirstOrDefault(samus.Timelines.Has) ?? "";
    }
}
