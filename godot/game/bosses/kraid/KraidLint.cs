using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the three spikes in Kraid's belly. Once Kraid stands at its full height each, in its turn, pushes out of
/// the belly, flashes as it gathers, and fires across the room, to be loaded again after a while. A spike in flight
/// is a platform Samus can ride; its point knocks her back and hurts her, once for each time it comes out. A touch
/// of Kraid's arm sets the lowest one off at once.
/// </summary>
/// <remarks>It plays "budding" until Kraid has grown and "grown" after. As it gathers it shows, every other update,
/// the colours of its hurt flash ("hurt_colours") in place of Kraid's.</remarks>
public partial class KraidLint : Enemy
{
    /// <summary>How far below Kraid's place it sits, in pixels; above, when less than nothing.</summary>
    [Export]
    public int Height { get; set; }

    /// <summary>How far right of Kraid's place it comes out, in pixels; left, when less than nothing.</summary>
    [Export]
    public int EmergeOffset { get; set; }

    /// <summary>How many updates after Kraid has grown it first comes out.</summary>
    [Export]
    public int InitialDelay { get; set; } = 64;

    /// <summary>Whether it is the one a touch of Kraid's arm sets off.</summary>
    [Export]
    public bool FiresWhenArmTouched { get; set; }

    /// <summary>How far it pushes out of the belly before it gathers, in pixels.</summary>
    [Export, DataValue("emerge_pixels")]
    public int EmergePixels { get; set; } = 32;

    /// <summary>How many updates it gathers for.</summary>
    [Export, DataValue("charge_updates")]
    public int ChargeUpdates { get; set; } = 30;

    /// <summary>How fast it flies, as a number of pixels an update.</summary>
    [Export, DataValue("fire_speed")]
    public double FireSpeed { get; set; } = 3.5;

    /// <summary>Left of here, in the room's pixels, it can no longer hurt.</summary>
    [Export, DataValue("harmless_x")]
    public int HarmlessX { get; set; } = 56;

    /// <summary>Left of here it is gone, to be loaded again.</summary>
    [Export, DataValue("return_x")]
    public int ReturnX { get; set; } = 32;

    /// <summary>How many updates pass before a fired one comes out again.</summary>
    [Export, DataValue("reload_updates")]
    public int ReloadUpdates { get; set; } = 300;

    /// <summary>The sound of its firing, by name.</summary>
    [Export, DataValue("fire_sound")]
    public string FireSound { get; set; } = "";

    /// <summary>How far its point lies from its place, in pixels; left, when less than nothing.</summary>
    [Export, DataValue("point_x")]
    public int PointX { get; set; } = -26;

    private enum Phase
    {
        Inactive,
        Align,
        Produce,
        Charge,
        Fire,
    }

    private Kraid? _kraid;
    private Phase _phase;
    private int _wait;
    private int _travel;
    private int _placedAt = -1;
    private bool _harmless;
    private bool _shown = true;
    private bool _begun;
    private bool _hiddenForGrowth;

    /// <summary>The colours of Kraid's parts, as Kraid last gave them.</summary>
    private ColourChange? _kraidColours;

    /// <summary>The colours last shown on it.</summary>
    private ColourChange? _coloursShown;

    /// <summary>The colours of its hurt flash, which it shows as it gathers.</summary>
    private ColourChange _flashColours = ColourChange.None;

    /// <summary>Whether it shows its flash's colours this update, in place of Kraid's.</summary>
    private bool _flashing;

    /// <summary>
    /// A shot lands on it while it is out and has not yet struck Samus, and is a dud there: it is harmed by none, and
    /// stands between her shots and Kraid. Harmless again, or hidden, it is met by nothing.
    /// </summary>
    public override bool IsHitBy(HitSide side) => !_harmless && _shown && base.IsHitBy(side);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _flashColours = ColourChange.Read(data.Get("hurt_colours"));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Play("budding");
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // Its point hurts by its own rule; the rest of it is only a platform.
    }

    /// <summary>
    /// Shows it in the colours of Kraid's parts at once, in place of those it showed before, unless it shows its
    /// flash's this update. A change is worked out for it by the picture it shows, so while it shows none it is shown
    /// none.
    /// </summary>
    public void ShowColours(ColourChange colours)
    {
        ArgumentNullException.ThrowIfNull(colours);
        _kraidColours = colours;
        ShowColours();
    }

    /// <summary>Shows it in its flash's colours or in Kraid's, whichever it shows this update.</summary>
    private void ShowColours()
    {
        ColourChange? colours = _flashing ? _flashColours : _kraidColours;
        if (colours is null || Visual is not { } visual || visual.Picture.Length == 0 || ReferenceEquals(_coloursShown, colours))
            return;
        colours.ShowOn(visual, nameof(Kraid));
        _coloursShown = colours;
    }

    /// <summary>Has it show its flash's colours, or Kraid's again.</summary>
    private void SetFlashing(bool flashing)
    {
        _flashing = flashing;
        ShowColours();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_kraid is null)
        {
            _kraid = Room.Enemies.OfType<Kraid>().FirstOrDefault()
                ?? throw new InvalidOperationException($"The spike \"{Name}\" is in a room with no Kraid.");
            if (FiresWhenArmTouched)
                _kraid.ArmTouched += () => _phase = Phase.Fire;
        }

        Kraid kraid = _kraid;
        if (!IsInstanceValid(kraid) || kraid.IsFinished)
        {
            Die();
            return;
        }

        if (_placedAt != kraid.LintsPlaced)
        {
            _placedAt = kraid.LintsPlaced;
            Position = new PixelPosition(Position.X, Pixels.FromWhole(kraid.Position.Y.Whole + Height));
        }

        if (kraid.HasBegunGrowing && !_hiddenForGrowth)
        {
            // As Kraid begins to rise to its full height its spikes go out of sight, until each next comes out.
            _hiddenForGrowth = true;
            SetShown(false);
        }

        if (kraid.HasGrown && !_begun)
        {
            _begun = true;
            Play("grown");
            (_phase, _wait, _travel) = (Phase.Align, InitialDelay, 0);
        }

        StrikeSamus();
        int halfWidth = Body is { } body ? body.Rect.Width / 2 : 0;
        int emerged = EmergeOffset + kraid.Position.X.Whole - _travel;
        switch (_phase)
        {
            case Phase.Inactive:
                break;

            case Phase.Align:
                SetX(kraid.Position.X.Whole - halfWidth);
                if (--_wait <= 0)
                    _phase = Phase.Produce;
                break;

            case Phase.Produce:
                _harmless = false;
                SetShown(true);
                SetX(emerged);
                if (++_travel >= EmergePixels)
                    (_phase, _wait) = (Phase.Charge, ChargeUpdates);
                break;

            case Phase.Charge:
                // On the updates with an even count left it shows the flash's colours, and Kraid's on the others.
                SetFlashing(_wait % 2 == 0);
                SetX(emerged);
                if (--_wait > 0)
                    break;
                _phase = Phase.Fire;
                Sound(FireSound);
                break;

            case Phase.Fire:
                MoveCarrying(new PixelOffset(-Pixels.FromNumber(FireSpeed), Pixels.Zero));
                if (Position.X.Whole < HarmlessX)
                    _harmless = true;
                if (Position.X.Whole >= ReturnX)
                    break;
                SetShown(false);
                (_phase, _wait, _travel) = (Phase.Align, ReloadUpdates, 0);
                break;

            default:
                throw new InvalidOperationException($"Kraid's spike has no phase {_phase}.");
        }
    }

    /// <summary>Its point, meeting Samus, knocks her back and hurts her, and then is harmless until it comes out again.</summary>
    private void StrikeSamus()
    {
        Player samus = Room.Player;
        if (_harmless || !_shown || samus.IsInvincible)
            return;
        int point = Position.X.Whole + PointX;
        PixelRect body = samus.BodyRect;
        if (body.Right < point || body.Left >= point || body.Bottom - Position.Y.Whole < -2 || body.Top - Position.Y.Whole >= 4)
            return;
        ShiftPlayer(PixelOffset.FromWhole(-(body.Width / 2 + 17), 0));
        samus.TakeHit(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Direction = PixelOffset.FromWhole(-1, 0), Source = this }, samus.Hurtbox);
        _harmless = true;
    }

    /// <summary>Shows or hides it; hidden, it is no platform either.</summary>
    private void SetShown(bool shown)
    {
        _shown = shown;
        Visual?.SetShown(shown);
        if (Solid is { } solid)
            solid.Enabled = shown;
    }

    private void SetX(int x) => Position = new PixelPosition(Pixels.FromWhole(x), Position.Y);
}
