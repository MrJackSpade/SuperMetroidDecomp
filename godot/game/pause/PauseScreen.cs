using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// The pause screen, laid over play. Play goes on while the screen darkens for it and again while the screen
/// brightens after it: through both this steps play itself, without the pause button, and gives way at once to
/// whatever play hands over to, but for a door or the time up begun on the last update of either, which go on as the
/// cartridge's do. Between them every sound stops, the screen is black for an update, the pause
/// screen brightens, is used, and darkens, and the screen is black for an update more. The strip across the top of
/// play stays over it throughout.
/// It has two pages. The map page shows the map of the area Samus is in, through the opening of the page's frame;
/// the directions move it. The equipment page shows what she owns, each thing lit when it is switched on, with a
/// selector on one of them that the directions move, and confirm switches the thing the selector is on once it has
/// moved, so a press on the update it moves switches what it moved onto. Her reserves are there too: whether they
/// refill by themselves, and, when they do not, a line on which confirm starts them pouring into her energy, which
/// then goes on by itself until she is full or they are empty and leaves them empty either way. Angle up turns from
/// the map to the equipment, angle down back, and pause leaves, each once it has been held a while.
/// </summary>
/// <remarks>
/// Its data gives its "backdrop" colour; the picture behind everything for each area ("backdrops"); the pictures
/// of its "frame"; its three "buttons" ("map", "exit", "samus"), each "lit" or "dim"; its "fade" (how many "levels"
/// of darkness there are, the updates it waits before each step of a fade ("wait"), and before each step of the
/// fade out of a page ("page_out_wait")); "held_repeat_delay", the updates the buttons held must stay the same
/// before one of them turns the page or leaves, on the update after; its "map", a plane behind the
/// page that the directions move: the size of a "cell" and of the screen ("screen_width", "screen_height"); the
/// "opening" of the frame it shows through ("x", "y", "width", "height", in the backdrop's picture); the edges of the part of the screen its bounds are kept
/// to ("left_edge", "right_edge", "top_edge", "bottom_edge"); how it first lies ("centre_column", across which the
/// middle of what shows is put; "centre_line" and "row_middle_above_centre", down; "here_leftmost",
/// "here_rightmost" and "here_top", which the sign of where Samus is must not lie beyond; "lowest_map_top"); the
/// cells its bounds are taken to be while none of it shows ("unrevealed"); how far a press moves it
/// ("scroll_step"), how many updates that takes ("scroll_updates") and on which of them it moves
/// ("scroll_moves_at"); the "highlight" its arrows are coloured by (the "updates" each of its looks lasts, the
/// first of them "first_updates" the first time; it goes round on every update the screen is in use, and sounds
/// each time it comes round); the "arrows" that show which ways it can be moved ("left", "right", "up", "down",
/// each a place and its "pictures", one for each look of the highlight); and "indicator", the timeline of the sign of where Samus is, its pictures placed from
/// the corner of her cell; the "area_folder" of the areas' maps; its "sounds" ("move", "switch", "mode", "page",
/// "leave", "scroll", "highlight", "pour", and "pour_every", the updates from one pouring sound to the next); and its
/// "equipment" page: the "base" picture, the "hyper" beam's picture, the "controls" (each the "equipment" it
/// switches, the "side" of the page it is on, the "group" it is in ("beams", "suits" or "boots", each a column the
/// selector moves along, in the order given), its picture switched "on" and "off" with where that goes, and the
/// place of its "selector"), the "selector" (the "updates" each of its looks lasts, the first of them
/// "first_updates" each time the page is come to, and its "looks" for "reserves", "beams", "suits" and "boots":
/// for each look a picture and where its corner lies from the selector's place), the suit the selector goes to
/// from the beams when it is sent right
/// ("beams_right_enters"), the "wireframes" of her suit by whether she wears the Varia suit and the high
/// jump boots ("plain", "hi_jump", "varia", "varia_hi_jump"), and her "reserve": the pictures shown with none
/// ("absent", "arrow_none"), and with some the "tank", the "arrow" (dim; "arrow_lit" while the selector is on the
/// pouring line; and "arrow_pulse" while it is on the line of how they refill and they refill by themselves: its
/// place and "pictures", each shown "updates_each" updates of the game's clock in turn), the "bar" (a little tank for each she has:
/// the pictures of a "full" one, of one by how many steps full it is ("fills", from none), and of the "end_cap"
/// after the last; their "places"; the energy a tank holds ("energy_per_tank") and a step is ("energy_per_step");
/// and that a tank fewer than "flicker_below_steps" steps full shows its next step on and off, "flicker_updates"
/// each way), the mode ("auto", "manual"), three "digits"
/// (each a place and ten pictures), the places of the selector on its two lines ("mode_selector",
/// "transfer_selector") and how much pours an update ("pour_per_update").
/// Every picture is given with where its corner goes.
/// </remarks>
public partial class PauseScreen : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    public string DataFile { get; set; } = "pause/pause.json";

    /// <summary>What the pause is doing.</summary>
    private enum Phase
    {
        Darkening,
        Preparing,
        FadingIn,
        InUse,
        FadingOut,
        Restoring,
        Brightening,
    }

    /// <summary>How far a turn from one page to the other has got.</summary>
    private enum PageTurn
    {
        None,
        FadingOut,
        Loading,
        FadingIn,
    }

    /// <summary>Which of the buttons along the bottom are lit besides the way out.</summary>
    private enum Lit
    {
        OnMapPage,
        OnEquipmentPage,
        Leaving,
    }

    /// <summary>One thing the selector can rest on: a piece of equipment, or one of the two lines of her reserves.</summary>
    private sealed record Line(string Equipment, string Group, DataNode Selector);

    private const string ReserveMode = "reserve_mode", ReserveTransfer = "reserve_transfer", ReserveGroup = "reserves";

    private readonly PlayingMode _playing;
    private readonly List<Line> _controls = [];
    private DataNode? _data;
    private ScreenFade? _fade;
    private CanvasLayer? _layer;
    private Node2D? _frame;
    private Node2D? _page;
    private Node2D? _marks;
    private ScrollingMap? _map;
    private CinematicSprite? _indicator;
    private Sprite2D? _selector;
    private Sprite2D? _reserveArrow;
    private int _hudLayer;
    private Phase _phase;
    private PageTurn _turn;
    private Lit _lit;
    private int _levels;
    private int _pageDark;
    private int _fadeWaitLeft;
    private Vector2I _here;
    private readonly Dictionary<string, Sprite2D> _arrows = new(StringComparer.Ordinal);
    private int _look;
    private int _lookLeft;
    private int _selectorLook;
    private int _selectorLeft;
    /// <summary>The held buttons that repeat on this update, which the game keeps from one pause to the next.</summary>
    private IReadOnlySet<GameAction> _repeating => Session.Service<PauseHeldButtons>().Repeating;

    private bool _showsMap = true;
    private string _chosen = "";
    private int? _pourLeft;
    private readonly Dictionary<string, List<(string Equipment, bool Had)>> _columns = new(StringComparer.Ordinal);
    private bool _beamsClosed;

    /// <summary>The pause screen, laid over <paramref name="playing"/>.</summary>
    public PauseScreen(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    private Player Samus => Session.Player;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _fade = new ScreenFade { Name = "Fade", Layer = 20 };
        AddChild(_fade);
        _levels = _data.Get("fade").Int("levels");
        _fade.Cover(0, _levels);

        // The darkening goes on from however dark the screen was left, at the pause's own pace from here.
        int wait = _data.Get("fade").Int("wait");
        Session.Darkness.SetPace(wait, wait);
        _phase = Phase.Darkening;
    }

    /// <summary>
    /// The terms play goes on beneath the darkening and the brightening on, as the cartridge has it: the pause is not
    /// asked for again, and Samus with no energy left does not begin to die until play is its own again, though her
    /// reserves may still save her.
    /// </summary>
    private static readonly PlayTerms BeneathThePause = new() { MayPause = false, PlayerMayDie = false };

    /// <summary>
    /// Whether her reserves saved Samus on the update of play just played beneath the pause. The refill takes the
    /// update from the pause, as it does from anything else the update began: play is its own again.
    /// </summary>
    private bool SavedBeneath => _playing.Room.EnergySettled == EmptyEnergy.Saved;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        switch (_phase)
        {
            case Phase.Darkening:
            {
                // The darkening takes its step whatever the update of play began, and the screen is left that dark:
                // through the refill that takes the update from the pause, or the door or the time up it is handed to.
                GameMode? next = _playing.PlayBeneath(input, BeneathThePause);
                bool black = StepFade(darker: true);
                if (SavedBeneath && next is null)
                    return _playing;
                if (black)
                    Session.Darkness.SetPace(0, 0);
                if (next is not null)
                    return black ? AfterDarkening(next) : next;
                if (black)
                    _phase = Phase.Preparing;
                return this;
            }

            case Phase.Preparing:
                // As in the old game, the liquid and the statues still move before this update's random number is drawn.
                Session.Player.NoteHeld(input);
                _playing.Room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                Session.Sounds.StopAll();
                Prepare();
                BeginFade(dark: Session.Darkness.Levels);
                _phase = Phase.FadingIn;
                return this;

            case Phase.FadingIn:
                // Each update of the old pause screen, and the one that restores play, draws a random number first.
                // On each of them, from its preparing on, what is held is noted for Samus, who plays none of them.
                Session.Player.NoteHeld(input);
                Session.OpenUpdate();
                StepAnimations(inUse: false);
                if (StepFade(darker: false))
                    _phase = Phase.InUse;
                return this;

            case Phase.InUse:
                Session.Player.NoteHeld(input);
                Session.OpenUpdate();

                // The Godot game's own: item select opens the tester's menu, laid over the screen as it stands.
                if (_turn == PageTurn.None && input.WasPressed(Actions.ItemSelect))
                    return new Assists.DebugMenuMode(this);
                if (StepInUse(input))
                {
                    BeginFade(dark: 0);
                    _phase = Phase.FadingOut;
                }

                return this;

            case Phase.FadingOut:
                Session.Player.NoteHeld(input);
                Session.OpenUpdate();
                StepAnimations(inUse: false);
                if (StepFade(darker: true))
                    _phase = Phase.Restoring;
                return this;

            case Phase.Restoring:
                Session.Player.NoteHeld(input);
                Session.OpenUpdate();
                Restore();

                // Resuming disarms an elevator's door and leaves its ride as it stands: a ride down that reaches its
                // door disarmed begins the door's passage without the wait.
                ElevatorJourney.Of(Session).Disarm();
                BeginFade(dark: Session.Darkness.Levels);
                _phase = Phase.Brightening;
                return this;

            case Phase.Brightening:
            {
                // The brightening takes its step too, whatever the update of play began.
                GameMode? next = _playing.PlayBeneath(input, BeneathThePause);
                bool lit = StepFade(darker: false);
                if (lit)
                {
                    Session.Darkness.SetPace(0, 0);
                    return AfterBrightening(next);
                }

                if (next is not null)
                    return next;
                return SavedBeneath ? _playing : this;
            }

            default:
                throw new InvalidOperationException($"The pause is doing {_phase}, which is nothing it can do.");
        }
    }

    /// <summary>
    /// Back from the tester's menu, the map shows what it is now set to show, and the equipment page what she now
    /// has, the selector where it lies as the page is come to.
    /// </summary>
    protected internal override void Resume()
    {
        _map?.Reveal(Assists.TesterAssists.Of(Session).Now(Session).MapReveal);
        if (_phase == Phase.InUse && !_showsMap)
        {
            ChooseFirst();
            ShowEquipment();
        }
    }

    /// <summary>
    /// What play began on the update the darkening went black goes on from the black screen, as on the cartridge: a
    /// time up at its fade to black, with no white-out, and a door at the step after its first, so an elevator's door
    /// has no wait. Anything else is handed over as it is.
    /// </summary>
    private GameMode AfterDarkening(GameMode next)
    {
        if (next is TimeUpMode timeUp)
            timeUp.BeginAtBlackOut();
        else if (next is DoorTransitionMode)
            ElevatorJourney.Of(Session).Disarm();
        return next;
    }

    /// <summary>
    /// On the update the brightening is done play is its own again over whatever that update began, as on the
    /// cartridge: a door entered stays entered and is gone through at the end of the next update, and a time up is
    /// asked for again on the next update only on Zebes, as the escape does there. Anything else is handed over.
    /// </summary>
    private GameMode AfterBrightening(GameMode? next)
    {
        if (next is DoorTransitionMode or TimeUpMode)
        {
            // The mode play made has not taken control, and play names what it stood for again.
            next.Free();
            return _playing;
        }

        return next ?? _playing;
    }

    /// <summary>Sets the screen so dark, with the wait before the first step of the fade from there.</summary>
    private void BeginFade(int dark) => Session.Darkness.Restart(dark, Data().Get("fade").Int("wait"));

    /// <summary>Plays one update of the fade of the whole screen, a level at a time after its wait, and says whether it is done.</summary>
    private bool StepFade(bool darker) => darker ? Session.Darkness.Darken() : Session.Darkness.Brighten();

    /// <summary>Shows the page as dark as its turn has it. The whole screen's darkness is the session's, drawn over it.</summary>
    private void ShowDarkness() => (_fade ?? throw NotShown()).Cover(_pageDark, _levels);

    /// <summary>Builds the pause screen under the black, on its map page, and puts the strip across the top of play over it.</summary>
    private void Prepare()
    {
        DataNode data = Data();
        _layer = new CanvasLayer { Name = "Screen", Layer = 10 };
        AddChild(_layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _layer.AddChild(backdrop);
        _frame = new Node2D { Name = "Frame" };
        _layer.AddChild(_frame);

        // The map lies in the plane of the backdrop's picture, whose opening it shows through, about where Samus is.
        // A room that is in no area has no map to show, and the screen is then its equipment page alone.
        Room room = _playing.Room;
        DataNode map = data.Get("map");
        bool hasMap = room.Area.Length > 0;
        if (hasMap)
        {
            Area area = Area.Read(Session.Content.ReadData($"{data.Text("area_folder")}/{room.Area}.json"));
            DataNode picture = data.Get("backdrops").Get(room.Area);
            var plane = new Vector2I(picture.Int("x"), picture.Int("y"));
            _here = new Vector2I(
                room.MapCell.X + Samus.Position.X.Whole / room.MapCellSize.X,
                room.MapCell.Y + Samus.Position.Y.Whole / room.MapCellSize.Y) * map.Int("cell");
            _map = ScrollingMap.Of(map, area, Session.Progress, Session.Content, plane, plane, _here);
            _map.Reveal(Assists.TesterAssists.Of(Session).Now(Session).MapReveal);
            _layer.AddChild(_map);
        }

        _page = new Node2D { Name = "Page" };
        _layer.AddChild(_page);
        _marks = new Node2D { Name = "Marks" };
        _layer.AddChild(_marks);
        if (_playing.Hud is { } hud)
        {
            _hudLayer = hud.Layer;
            hud.Layer = _layer.Layer + 1;
        }

        (_look, _lookLeft) = (0, map.Get("highlight").Int("first_updates"));
        foreach ((string way, _, _) in ScrollingMap.Ways)
        {
            DataNode arrow = map.Get("arrows").Get(way);
            var sprite = new Sprite2D
            {
                Name = $"Arrow{way}", Centered = false, Position = new Vector2(arrow.Int("x"), arrow.Int("y")), Visible = false,
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            };
            _marks.AddChild(sprite);
            _arrows[way] = sprite;
        }

        ShowHighlight();

        // The sign of where Samus is shows its first look from the start.
        _indicator = CinematicSprite.Of(Session.Content, FrameTimeline.Read(map.Get("indicator")), PixelPosition.Origin);
        _indicator.Name = "Here";
        _marks.AddChild(_indicator);
        _indicator.Step();

        _lit = hasMap ? Lit.OnMapPage : Lit.OnEquipmentPage;
        ShowFrame();
        ChooseFirst();
        ShowPage(hasMap);
    }

    /// <summary>Takes the pause screen away under the black, and puts the strip across the top of play back where it was.</summary>
    private void Restore()
    {
        // What the pages switched on or off is put right about her before play shows again.
        (Session.Player as global::Game.Samus)?.ReconcileAfterPause();

        // The pause is over: a station that held her through it lets her go.
        Session.EndPause();
        if (_playing.Hud is { } hud)
            hud.Layer = _hudLayer;
        CanvasLayer layer = _layer ?? throw NotShown();
        RemoveChild(layer);
        layer.QueueFree();
        (_layer, _frame, _page, _marks, _map, _indicator, _selector, _reserveArrow) = (null, null, null, null, null, null, null, null);
        _arrows.Clear();
    }

    /// <summary>
    /// Plays one update of the screen in use, and says whether it is being left. A page being turned takes nothing
    /// from the player. The map page looks for the turn to the equipment, then for leaving, then moves the map; the
    /// equipment page plays its selector and switches first, then looks for the turn to the map and for leaving.
    /// </summary>
    private bool StepInUse(GameInput input)
    {
        StepAnimations(inUse: true);
        StepHeldRepeat(input);
        if (_showsMap && _turn == PageTurn.None)
        {
            foreach ((string way, _, _) in ScrollingMap.Ways)
                _arrows[way].Visible = Map().CanScroll(way);
        }

        if (StepPageTurn())
            return false;
        if (_showsMap)
        {
            if (_repeating.Contains(Actions.AngleUp))
                BeginPageTurn(Lit.OnEquipmentPage);
            bool leaving = Leaves();
            if (Map().StepScroll(input))
                Sound("scroll");
            PlaceIndicator();
            return leaving;
        }

        StepEquipment(input);
        if (_repeating.Contains(Actions.AngleDown) && _map is not null)
            BeginPageTurn(Lit.OnMapPage);
        return Leaves();
    }

    /// <summary>
    /// Works out which of the actions held count as repeating this update: those held, and not newly pressed, once
    /// what is held so has stayed the same for more updates than the delay, and on every update after until it changes.
    /// </summary>
    private void StepHeldRepeat(GameInput input) => Session.Service<PauseHeldButtons>().Take(input, Data().Int("held_repeat_delay"));

    /// <summary>Whether pause, held long enough, leaves the screen; the buttons then show only the way out lit.</summary>
    private bool Leaves()
    {
        if (!_repeating.Contains(GameAction.Pause))
            return false;
        Sound("leave");
        _lit = Lit.Leaving;
        ShowFrame();
        return true;
    }

    /// <summary>Begins the turn to the other page: its button is lit at once, and the page showing begins to darken.</summary>
    private void BeginPageTurn(Lit lit)
    {
        Sound("page");
        _lit = lit;
        ShowFrame();
        _turn = PageTurn.FadingOut;
        (_pageDark, _fadeWaitLeft) = (0, Data().Get("fade").Int("page_out_wait"));
    }

    /// <summary>
    /// Plays one update of a page being turned, and says whether one was: the page showing darkens a level after each
    /// wait, the other page is put in its place over one black update, and that brightens a level after each wait.
    /// </summary>
    private bool StepPageTurn()
    {
        DataNode fade = Data().Get("fade");
        switch (_turn)
        {
            case PageTurn.None:
                return false;

            case PageTurn.FadingOut:
                if (StepPageFade(darker: true, fade.Int("page_out_wait")))
                    _turn = PageTurn.Loading;
                return true;

            case PageTurn.Loading:
                if (_lit == Lit.OnEquipmentPage)
                    ChooseFirst();
                ShowPage(map: _lit == Lit.OnMapPage);
                _turn = PageTurn.FadingIn;
                _fadeWaitLeft = fade.Int("wait");
                return true;

            case PageTurn.FadingIn:
                if (StepPageFade(darker: false, fade.Int("wait")))
                    _turn = PageTurn.None;
                return true;

            default:
                throw new InvalidOperationException($"A page of the pause screen is being turned as {_turn}, which is no way to turn one.");
        }
    }

    /// <summary>Plays one update of the fade of a page being turned, and says whether it is done.</summary>
    private bool StepPageFade(bool darker, int wait)
    {
        if (_fadeWaitLeft > 0)
        {
            _fadeWaitLeft--;
        }
        else
        {
            _fadeWaitLeft = wait;
            _pageDark = Math.Clamp(_pageDark + (darker ? 1 : -1), 0, _levels);
            ShowDarkness();
        }

        return _pageDark == (darker ? _levels : 0);
    }

    /// <summary>
    /// Moves on what the screen shows moving, once an update. The highlight goes round only while the screen is in
    /// use, and sounds as it comes round; the arrows hide, to be shown again by the map page in use; the sign of
    /// where Samus is plays on the map page, the selector on the equipment page.
    /// </summary>
    private void StepAnimations(bool inUse)
    {
        foreach (Sprite2D arrow in _arrows.Values)
            arrow.Visible = false;
        if (inUse)
            StepHighlight();
        if (_showsMap)
        {
            _indicator?.Step();
            return;
        }

        StepSelector();
        ShowReserveArrow();

        // A tank of her reserves that is nearly empty flickers, and the page is shown again each time it changes.
        DataNode bar = Data().Get("equipment").Get("reserve").Get("bar");
        if (FlickeringSteps(bar) is not null && Session.Clock.ShortUpdates % bar.Int("flicker_updates") == 0)
            ShowEquipment();
    }

    /// <summary>Moves the highlight on by an update: each look lasts its own while, and it sounds as it comes round to the first.</summary>
    private void StepHighlight()
    {
        if (--_lookLeft > 0)
            return;
        IReadOnlyList<DataNode> updates = Data().Get("map").Get("highlight").List("updates");
        _look++;
        if (_look == updates.Count)
        {
            _look = 0;
            Sound("highlight");
        }

        _lookLeft = updates[_look].AsInt();
        ShowHighlight();
    }

    /// <summary>Shows every arrow in the highlight's look.</summary>
    private void ShowHighlight()
    {
        DataNode arrows = Data().Get("map").Get("arrows");
        foreach ((string way, Sprite2D arrow) in _arrows)
            arrow.Texture = Session.Content.LoadTexture(arrows.Get(way).List("pictures")[_look].AsText());
    }

    /// <summary>Plays one update of the equipment page: the selector moves, and confirm switches or pours.</summary>
    private void StepEquipment(GameInput input)
    {
        if (_controls.Count == 0)
            return;
        Line chosen = Chosen();
        bool confirm = input.WasPressed(GameAction.Confirm);
        string before = _chosen;
        bool changed = false;
        if (chosen.Group == ReserveGroup)
        {
            // Her reserves answer to confirm before the selector moves off them.
            changed = StepReserves(confirm, chosen);
            MoveSelector(input, Chosen());
        }
        else
        {
            // Anything else is switched after the selector has moved: a press on the update it moves switches what it
            // moved onto, by the rules of the column it came from.
            MoveSelector(input, chosen);
            if (confirm && !(chosen.Group == "beams" && _columns["beams"].All(beam => !beam.Had)))
            {
                Line target = Chosen();
                Sound("switch");
                if (target.Group != ReserveGroup)
                {
                    Switch(target.Equipment, keepsBeamsApart: chosen.Group == "beams");
                    changed = true;
                }
            }
        }

        if (changed)
            _playing.Hud?.Refresh(Session, _playing.Room);
        if (changed || _chosen != before)
            ShowEquipment();
    }

    /// <summary>
    /// Plays one update of her reserves under the selector, and says whether anything of hers changed. On the line of
    /// how they refill, confirm changes it. On the pouring line, confirm starts the pouring, which then goes on every
    /// update the selector is there: a part of her reserves goes into her energy, with a sound every so often, until
    /// she is full or they have run out; either way they are then empty and the selector goes back to the line above.
    /// </summary>
    private bool StepReserves(bool confirm, Line chosen)
    {
        Inventory inventory = Samus.Inventory;
        if (chosen.Equipment == ReserveMode)
        {
            if (!confirm || inventory.Capacity(Amounts.ReserveEnergy) == 0)
                return false;
            Sound("mode");
            inventory.Switch(Equipment.ReserveTank, !inventory.IsOn(Equipment.ReserveTank));
            return true;
        }

        DataNode sounds = Data().Get("sounds");
        int every = sounds.Int("pour_every");
        if (_pourLeft is not { } left)
        {
            if (!confirm)
                return false;
            left = (inventory.Amount(Amounts.ReserveEnergy) + every - 1) / every * every;
        }

        if (left % every == 0)
            Sound("pour");
        _pourLeft = Math.Max(0, left - 1);

        int pour = Data().Get("equipment").Get("reserve").Int("pour_per_update");
        int energy = inventory.Amount(Inventory.Energy) + pour, most = inventory.Capacity(Inventory.Energy);
        if (energy < most)
        {
            // Her reserves may fall short of a whole pouring; what they lack comes back off her energy.
            int reserves = inventory.Amount(Amounts.ReserveEnergy) - pour;
            if (reserves > 0)
            {
                inventory.Restore(Inventory.Energy, energy);
                inventory.Restore(Amounts.ReserveEnergy, reserves);
                return true;
            }

            energy += reserves;
        }

        inventory.Restore(Inventory.Energy, Math.Min(energy, most));
        inventory.Restore(Amounts.ReserveEnergy, 0);
        _pourLeft = null;
        _chosen = ReserveMode;
        return true;
    }

    /// <summary>
    /// Moves the selector as the directions pressed send it. It goes up and down its own
    /// column, passing what she has not got; off the end of a column, or sent across, it goes where the page has that
    /// column lead: from the beams up to her reserves and right to the suits or, failing them, the boots; from the
    /// suits down to the boots and left to her reserves or the beams; from the boots up to the suits and left to the
    /// last beam or her reserves; from her reserves down to the beams and right to the suits or the boots.
    /// </summary>
    private void MoveSelector(GameInput input, Line chosen)
    {
        bool left = input.WasPressed(GameAction.Left), right = input.WasPressed(GameAction.Right);
        bool up = input.WasPressed(GameAction.Up), down = input.WasPressed(GameAction.Down);
        Inventory inventory = Samus.Inventory;
        if (chosen.Group == ReserveGroup)
        {
            if (right)
            {
                if (down || !SelectFrom("suits", 0, 1))
                    SelectFrom("boots", 0, 1);
            }
            else if (up)
            {
                if (chosen.Equipment != ReserveMode)
                    Select(ReserveMode);
            }
            else if (down)
            {
                // The pouring line is there only while her reserves do not refill by themselves.
                if (chosen.Equipment == ReserveTransfer || inventory.IsOn(Equipment.ReserveTank))
                {
                    SelectFrom("beams", 0, 1);
                }
                else if (inventory.Amount(Amounts.ReserveEnergy) == 0)
                {
                    // With nothing to pour, the selector only passes the pouring line on its way to the beams.
                    _chosen = ReserveTransfer;
                    SelectFrom("beams", 0, 1);
                }
                else
                {
                    Select(ReserveTransfer);
                }
            }

            return;
        }

        string group = chosen.Group;
        int place = _columns[group].FindIndex(entry => entry.Equipment == chosen.Equipment);
        switch (group)
        {
            case "beams":
                if (right)
                {
                    int entry = up ? 0 : _columns["suits"].FindIndex(suit => suit.Equipment == Data().Get("equipment").Text("beams_right_enters"));
                    if (!SelectFrom("suits", entry, 1) && !up)
                        SelectFrom("boots", 0, 1);
                }
                else if (down)
                {
                    SelectFrom(group, place + 1, 1);
                }
                else if (up && !SelectFrom(group, place - 1, -1))
                {
                    SelectReserves();
                }

                break;
            case "suits":
                if (left)
                {
                    if (down || !SelectReserves())
                        SelectFrom("beams", 0, 1);
                }
                else if (up)
                {
                    SelectFrom(group, place - 1, -1);
                }
                else if (down && !SelectFrom(group, place + 1, 1))
                {
                    SelectFrom("boots", 0, 1);
                }

                break;
            case "boots":
                if (left)
                {
                    if (up || !SelectFrom("beams", _columns["beams"].Count - 1, -1))
                        SelectReserves();
                }
                else if (down)
                {
                    SelectFrom(group, place + 1, 1);
                }
                else if (up && !SelectFrom(group, place - 1, -1))
                {
                    SelectFrom("suits", _columns["suits"].Count - 1, -1);
                }

                break;
            default:
                throw new InvalidOperationException($"The pause screen's equipment has no column \"{group}\".");
        }
    }

    /// <summary>
    /// Puts the selector on the first thing she has in a column, looking from a place in it up or down, and says
    /// whether there was one. The beams are closed while she has the hyper beam. Looking down the suits past one she
    /// has not got never finds the last of them, as the old game's search does not.
    /// </summary>
    private bool SelectFrom(string group, int start, int step)
    {
        List<(string Equipment, bool Had)> column = _columns[group];
        if (group == "beams" && _beamsClosed)
            return false;
        for (int place = start; place >= 0 && place < column.Count; place += step)
        {
            if (!column[place].Had)
            {
                if (group == "suits" && step > 0 && place + step >= column.Count - 1)
                    return false;
                continue;
            }

            Select(column[place].Equipment);
            return true;
        }

        return false;
    }

    /// <summary>Puts the selector on the first line of her reserves, if she has any, and says whether she has.</summary>
    private bool SelectReserves()
    {
        if (Samus.Inventory.Capacity(Amounts.ReserveEnergy) == 0)
            return false;
        Select(ReserveMode);
        return true;
    }

    /// <summary>Puts the selector on a line, with the sound of it moving.</summary>
    private void Select(string line)
    {
        _chosen = line;
        Sound("move");
    }

    /// <summary>The line the selector is on.</summary>
    private Line Chosen() => _controls.Find(line => line.Equipment == _chosen)
        ?? throw new InvalidOperationException($"The pause screen's selector is on \"{_chosen}\", which the page does not show.");

    /// <summary>Puts the selector where it lies as the equipment page is come to: on her reserves if she has any, else on the first thing she has.</summary>
    private void ChooseFirst()
    {
        Inventory inventory = Samus.Inventory;
        _chosen = ReserveMode;
        if (inventory.Capacity(Amounts.ReserveEnergy) > 0)
            return;
        // As in the old game it may start on a beam she has though the hyper beam closes the beams.
        foreach (DataNode control in Data().Get("equipment").List("controls"))
        {
            if (!inventory.Owns(control.Text("equipment")))
                continue;
            _chosen = control.Text("equipment");
            return;
        }
    }

    /// <summary>Whether she has what a control switches, and it can be switched: the beams cannot while she has the hyper beam.</summary>
    private bool Has(DataNode control)
    {
        Inventory inventory = Samus.Inventory;
        DataNode page = Data().Get("equipment");
        bool closed = inventory.IsOn(Equipment.HyperBeam) && control.Text("side") == page.Text("hyper_side") && control.Flag("beam");
        return inventory.Owns(control.Text("equipment")) && !closed;
    }

    /// <summary>Shows the backdrop of the area, the page's frame and its buttons, lit as they now are.</summary>
    private void ShowFrame()
    {
        DataNode data = Data();
        Node2D frame = _frame ?? throw NotShown();
        foreach (Node child in frame.GetChildren())
        {
            frame.RemoveChild(child);
            child.QueueFree();
        }

        Room room = _playing.Room;
        if (room.Area.Length > 0)
            Show(data.Get("backdrops").Get(room.Area), parent: frame);
        foreach (DataNode picture in data.List("frame"))
            Show(picture, parent: frame);

        // The way out is always lit; the button of the page that is not showing is lit until the screen is left.
        string other = _lit switch { Lit.OnMapPage => "samus", Lit.OnEquipmentPage => "map", _ => "" };
        foreach ((string name, DataNode button) in data.Get("buttons").Entries())
            Show(button, button.Text(name == "exit" || name == other ? "lit" : "dim"), frame);
    }

    /// <summary>Shows one of the two pages.</summary>
    private void ShowPage(bool map)
    {
        DataNode data = Data();
        Node2D page = _page ?? throw NotShown();
        _showsMap = map;
        foreach (Node child in page.GetChildren())
        {
            page.RemoveChild(child);
            child.QueueFree();
        }

        (_selector, _reserveArrow) = (null, null);
        (_marks ?? throw NotShown()).Visible = map;
        if (_map is not null)
            _map.Visible = map;
        if (map)
        {
            PlaceIndicator();
            return;
        }

        // The selector begins again on its first look each time the page is come to.
        (_selectorLook, _selectorLeft) = (0, data.Get("equipment").Get("selector").Int("first_updates"));
        ShowEquipment();
    }

    /// <summary>The map behind the page.</summary>
    private ScrollingMap Map() => _map ?? throw new InvalidOperationException("The pause screen has no map: the room is in no area.");

    /// <summary>Puts the sign of where Samus is over her cell of the map, where the map now lies.</summary>
    private void PlaceIndicator()
    {
        if (_indicator is not { } indicator)
            return;
        Vector2I here = Map().OnScreen(_here);
        indicator.Place = PixelPosition.FromWhole(here.X, here.Y);
        indicator.ShowPlace();
    }

    /// <summary>Shows the equipment page as her inventory now stands, with the selector where it is.</summary>
    private void ShowEquipment()
    {
        DataNode page = Data().Get("equipment");
        Node2D shown = _page ?? throw NotShown();
        if (shown.GetNodeOrNull<Node2D>("Equipment") is { } old)
        {
            shown.RemoveChild(old);
            old.QueueFree();
        }

        var equipment = new Node2D { Name = "Equipment" };
        shown.AddChild(equipment);
        Inventory inventory = Samus.Inventory;
        Show(page.Get("base"), parent: equipment);
        _controls.Clear();

        // Her reserves: with none, their place says so; with some, how they refill and how much they hold.
        DataNode reserve = page.Get("reserve");
        int held = inventory.Amount(Amounts.ReserveEnergy);
        if (inventory.Capacity(Amounts.ReserveEnergy) == 0)
        {
            Show(reserve.Get("absent"), parent: equipment);
        }
        else
        {
            bool auto = inventory.IsOn(Equipment.ReserveTank);
            Show(reserve.Get(auto ? "auto" : "manual"), parent: equipment);
            Show(reserve.Get("tank"), parent: equipment);
            IReadOnlyList<DataNode> digits = reserve.List("digits");
            int divisor = 1;
            for (int place = 1; place < digits.Count; place++)
                divisor *= 10;
            foreach (DataNode digit in digits)
            {
                Show(digit, digit.List("pictures")[held / divisor % 10].AsText(), equipment);
                divisor /= 10;
            }

            // The bar: the full tanks, the one part filled, the empty ones, and the cap after the last.
            DataNode bar = reserve.Get("bar");
            IReadOnlyList<DataNode> places = bar.List("places");
            IReadOnlyList<DataNode> fills = bar.List("fills");
            int perTank = bar.Int("energy_per_tank"), perStep = bar.Int("energy_per_step");
            int tank = 0;
            for (; tank < held / perTank; tank++)
                Show(places[tank], bar.Text("full"), equipment);
            if (held % perTank != 0)
            {
                // One that is nearly empty shows its next step for a while and hides it for as long, by the game's clock.
                int steps = held % perTank / perStep;
                if (FlickeringSteps(bar) is { } flickering)
                    steps = flickering;
                Show(places[tank++], fills[steps].AsText(), equipment);
            }

            for (; tank < inventory.Capacity(Amounts.ReserveEnergy) / perTank; tank++)
                Show(places[tank], fills[0].AsText(), equipment);
            Show(places[tank], bar.Text("end_cap"), equipment);

            _controls.Add(new Line(ReserveMode, ReserveGroup, reserve.Get("mode_selector")));
            if (!auto)
                _controls.Add(new Line(ReserveTransfer, ReserveGroup, reserve.Get("transfer_selector")));
        }

        // The hyper beam stands in the place of the beams, which cannot then be switched.
        bool hyper = inventory.IsOn(Equipment.HyperBeam);
        if (hyper)
            Show(page.Get("hyper"), parent: equipment);
        _beamsClosed = hyper;
        _columns.Clear();
        foreach (DataNode control in page.List("controls"))
        {
            string name = control.Text("equipment"), group = control.Text("group");
            if (!_columns.TryGetValue(group, out List<(string Equipment, bool Had)>? column))
                _columns[group] = column = [];
            // A beam the hyper beam closes is not shown and no search finds it, but the selector may start on it.
            bool owns = inventory.Owns(name);
            column.Add((name, owns));
            if (!owns)
                continue;
            if (Has(control))
                Show(control, control.Text(inventory.IsOn(name) ? "on" : "off"), equipment);
            _controls.Add(new Line(name, group, control.Get("selector")));
        }

        string wireframe = (inventory.IsOn(Equipment.VariaSuit), inventory.IsOn(Equipment.HiJumpBoots)) switch
        {
            (false, false) => "plain",
            (false, true) => "hi_jump",
            (true, false) => "varia",
            (true, true) => "varia_hi_jump",
        };
        Show(page.Get("wireframes").Get(wireframe), parent: equipment);

        // The arrow of her reserves and the selector lie over the rest, and are shown anew as they change.
        _reserveArrow = new Sprite2D { Name = "ReserveArrow", Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        equipment.AddChild(_reserveArrow);
        _selector = new Sprite2D { Name = "Selector", Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        equipment.AddChild(_selector);

        // A line that is no longer shown, as the pouring line is not once her reserves refill by themselves, gives the selector to the first.
        if (_controls.Count > 0 && _controls.All(line => line.Equipment != _chosen))
            _chosen = _controls[0].Equipment;
        ShowReserveArrow();
        ShowSelector();
    }

    /// <summary>
    /// How many steps full the part-filled tank of her reserves shows now, when it is one that flickers: its next
    /// step for a while, then not for as long, by the game's clock. Null when no tank flickers.
    /// </summary>
    private int? FlickeringSteps(DataNode bar)
    {
        Inventory inventory = Samus.Inventory;
        if (inventory.Capacity(Amounts.ReserveEnergy) == 0)
            return null;
        int rest = inventory.Amount(Amounts.ReserveEnergy) % bar.Int("energy_per_tank"), perStep = bar.Int("energy_per_step");
        int steps = rest / perStep;
        if (rest == 0 || rest % perStep == 0 || steps >= bar.Int("flicker_below_steps"))
            return null;
        // The flicker and the arrow's pulse go by the short count, which a door's scroll starts over.
        return Session.Clock.ShortUpdates / bar.Int("flicker_updates") % 2 == 0 ? steps + 1 : steps;
    }

    /// <summary>
    /// Shows the arrow of her reserves: as the page has it with none; pulsing while the selector is on the line of
    /// how they refill and they refill by themselves; lit while it is on the pouring line; dim otherwise.
    /// </summary>
    private void ShowReserveArrow()
    {
        if (_reserveArrow is not { } arrow)
            return;
        DataNode reserve = Data().Get("equipment").Get("reserve");
        Inventory inventory = Samus.Inventory;
        DataNode placed;
        string picture;
        if (inventory.Capacity(Amounts.ReserveEnergy) == 0)
        {
            placed = reserve.Get("arrow_none");
            picture = placed.Text("picture");
        }
        else if (_chosen == ReserveMode && inventory.IsOn(Equipment.ReserveTank))
        {
            placed = reserve.Get("arrow_pulse");
            IReadOnlyList<DataNode> pictures = placed.List("pictures");
            picture = pictures[Session.Clock.ShortUpdates / placed.Int("updates_each") % pictures.Count].AsText();
        }
        else
        {
            placed = reserve.Get(_chosen == ReserveTransfer ? "arrow_lit" : "arrow");
            picture = placed.Text("picture");
        }

        arrow.Visible = picture.Length > 0;
        if (!arrow.Visible)
            return;
        arrow.Texture = Session.Content.LoadTexture(picture);
        arrow.Position = new Vector2(placed.Int("x"), placed.Int("y"));
    }

    /// <summary>Moves the selector's look on by an update, while there is anything for it to be on.</summary>
    private void StepSelector()
    {
        if (_controls.Count == 0 || --_selectorLeft > 0)
            return;
        IReadOnlyList<DataNode> updates = Data().Get("equipment").Get("selector").List("updates");
        _selectorLook = (_selectorLook + 1) % updates.Count;
        _selectorLeft = updates[_selectorLook].AsInt();
        ShowSelector();
    }

    /// <summary>Shows the selector on the line it is on, in the look it has come to for that line's column.</summary>
    private void ShowSelector()
    {
        if (_selector is not { } selector)
            return;
        selector.Visible = _controls.Count > 0;
        if (!selector.Visible)
            return;
        Line chosen = Chosen();
        DataNode look = Data().Get("equipment").Get("selector").Get("looks").List(chosen.Group)[_selectorLook];
        selector.Texture = Session.Content.LoadTexture(look.Text("picture"));
        selector.Position = new Vector2(chosen.Selector.Int("x") + look.Int("x"), chosen.Selector.Int("y") + look.Int("y"));
    }

    /// <summary>
    /// Switches a piece of equipment on or off. Switched from the beams' own column, the spazer and the plasma beam
    /// do not go together: one on turns the other off. Switched on the update the selector came onto it from another
    /// column, they are left as they are.
    /// </summary>
    private void Switch(string equipment, bool keepsBeamsApart)
    {
        Inventory inventory = Samus.Inventory;
        bool on = !inventory.IsOn(equipment);
        inventory.Switch(equipment, on);
        if (!on || !keepsBeamsApart)
            return;
        if (equipment == Equipment.Spazer && inventory.IsOn(Equipment.PlasmaBeam))
            inventory.Switch(Equipment.PlasmaBeam, false);
        else if (equipment == Equipment.PlasmaBeam && inventory.IsOn(Equipment.Spazer))
            inventory.Switch(Equipment.Spazer, false);
    }

    /// <summary>Shows a picture the data places, or another picture at that place; an empty picture shows nothing.</summary>
    private void Show(DataNode placed, string? picture = null, Node2D? parent = null)
    {
        picture ??= placed.Text("picture");
        if (picture.Length == 0)
            return;
        (parent ?? _page ?? throw NotShown()).AddChild(new Sprite2D
        {
            Texture = Session.Content.LoadTexture(picture), Centered = false, Position = new Vector2(placed.Int("x"), placed.Int("y")),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        });
    }

    private void Sound(string name) => Session.Sounds.Request(Data().Get("sounds").Text(name), channel: "menu");

    private DataNode Data() => _data ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The pause screen has not been shown.");
}
