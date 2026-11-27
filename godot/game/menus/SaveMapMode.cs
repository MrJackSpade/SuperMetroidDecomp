using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Menus;

/// <summary>
/// The map of where a saved game was saved, shown between the options and the game. After a moment of black the
/// map of the whole planet opens from the middle of the screen: its areas, each named once the game has been saved
/// in it, the one the game was saved in lit. Confirm opens a window from that area's name onto the area's own map,
/// with the place the game was saved at marked and pulsing, the stations found and the ship; the directions move
/// it. Confirm there starts the game; cancel closes the window back to the planet, and cancel on the planet goes
/// back to the options.
/// </summary>
/// <remarks>
/// Its data gives how many levels of darkness a fade has ("fade_levels"); its "entry" (the "black_updates" before
/// the planet shows, the window it then shows through, "left", "right" and "top" with the bottom as far from the
/// screen's foot, and the "speed" each edge opens at); the "world": its "title", the "order" its areas' names are
/// drawn in, and for each of its "areas" the "backdrop" colour, the "foreground" picture, the "glow" laid brightly
/// over it, its name "label" ("lit" and "dim", with where its corner goes), the place the window onto its map opens
/// from ("label_at") and how that window opens ("reveal": the "updates" its edges move for and how far each moves
/// an update, "left", "right", "top" and "bottom", in 65536ths of a pixel); the "room" map: its "backdrop" colour,
/// the "frames" of the areas' maps, how the map is laid out and moved ("map", as <see cref="ScrollingMap"/> reads
/// it), the "area_folder" of the areas' maps, the "highlight" its arrows are coloured by and the "arrows" (as the
/// pause screen's), and the "marker" of where the game was saved (its "backing", and the timeline of its "pulse",
/// each time round which the backing shows or hides); the "return" to the planet (its "setup_updates", how far in
/// from the screen's edges the closing window starts, "inset", and how many "fewer_updates" than the opening it
/// moves for); the "load" (its "prelude_updates" before the fade and "black_updates" after); the "stations" a game
/// can be saved at, by the entry point a game saved there resumes at, each with its "area", where its marker lies
/// on that area's map ("x", "y") and the place the map is first laid about ("anchor"); and its "sounds"
/// ("confirm", "expand", "return", "scroll", "highlight").
/// </remarks>
public partial class SaveMapMode : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    public const string DataFile = "menus/save_map.json";

    /// <summary>What the screen is doing.</summary>
    private enum Phase
    {
        BlackHold,
        Revealing,
        Area,
        PreparingWindow,
        ExpandingWindow,
        InitializingRoom,
        Room,
        AreaReturnRequested,
        OptionsRequested,
        LoadRequested,
    }

    /// <summary>
    /// The window between the planet and an area's map: a rectangle of the screen whose four edges start together and
    /// move apart, each at its own speed and no further than the screen's edge, or start just inside the screen and
    /// move back. Its edges are kept in 65536ths of a pixel.
    /// </summary>
    private sealed class Window
    {
        private readonly DataNode _reveal;
        private readonly Vector2I _screen;
        private long _left, _right, _top, _bottom;
        private int _moves;

        /// <summary>A window opening from <paramref name="from"/>.</summary>
        public Window(DataNode reveal, Vector2I from, Vector2I screen)
        {
            (_reveal, _screen) = (reveal, screen);
            (_left, _right, _top, _bottom) = (Whole(from.X), Whole(from.X), Whole(from.Y), Whole(from.Y));
            _moves = reveal.Int("updates");
        }

        /// <summary>A window closing from <paramref name="inset"/> pixels inside the screen, moving for <paramref name="fewer"/> updates fewer than the opening.</summary>
        public Window(DataNode reveal, Vector2I screen, int inset, int fewer)
        {
            (_reveal, _screen, IsReturning) = (reveal, screen, true);
            (_left, _top, _right, _bottom) = (Whole(inset), Whole(inset), Whole(screen.X - inset), Whole(screen.Y - inset));
            _moves = reveal.Int("updates") - fewer;
            if (_moves <= 0)
                throw reveal.Problem("opens for too few updates to close again");
        }

        /// <summary>Whether this is the closing window.</summary>
        public bool IsReturning { get; }

        /// <summary>Whether the edges have made their last move.</summary>
        public bool IsComplete { get; private set; }

        /// <summary>The part of the screen inside it; of no size when its edges meet or have crossed.</summary>
        public Rect2I Inside
        {
            get
            {
                int left = Pixels(_left), right = Pixels(_right) + 1, top = Pixels(_top), bottom = Pixels(_bottom);
                return right <= left || bottom <= top ? new Rect2I(left, top, 0, 0) : new Rect2I(left, top, right - left, bottom - top);
            }
        }

        /// <summary>Moves every edge by its speed, and says whether that was the last move.</summary>
        public bool Step()
        {
            if (IsComplete)
                return true;
            int left = _reveal.Int("left"), right = _reveal.Int("right"), top = _reveal.Int("top"), bottom = _reveal.Int("bottom");
            if (IsReturning)
            {
                (_left, _right, _top, _bottom) = (_left - left, _right - right, _top - top, _bottom - bottom);
            }
            else
            {
                _left = OnScreen(_left + left, _screen.X);
                _right = OnScreen(_right + right, _screen.X - 1);
                _top = OnScreen(_top + top, _screen.Y);
                _bottom = OnScreen(_bottom + bottom, _screen.Y);
            }

            IsComplete = --_moves == 0;
            return IsComplete;
        }

        private static long Whole(int pixels) => (long)pixels << 16;

        private static int Pixels(long edge) => (int)(edge >> 16);

        /// <summary>An edge kept between the screen's near side and <paramref name="side"/>.</summary>
        private static long OnScreen(long edge, int side) => edge < 0 ? 0 : Math.Min(edge, Whole(side));
    }

    private readonly SaveData _save;
    private readonly DataNode _data;
    private readonly string _area;
    private Progress? _progress;
    private ScreenFade? _fade;
    private Control? _areaClip;
    private Control? _roomClip;
    private Node2D? _areaScene;
    private Node2D? _roomScene;
    private Sprite2D? _foreground;
    private ScrollingMap? _map;
    private Node2D? _marks;
    private Node2D? _arrowMarks;
    private Sprite2D? _backing;
    private CinematicSprite? _pulse;
    private readonly Dictionary<string, Sprite2D> _arrows = new(StringComparer.Ordinal);
    private Phase _phase = Phase.BlackHold;
    private Phase _beganIn = Phase.Area;
    private int _blackLeft;
    private int _revealSteps;
    private Window? _window;
    private Window? _returnWindow;
    private int _waitLength;
    private int _waitLeft;
    private int _dark;
    private int _look;
    private int _lookLeft;
    private bool _showsBacking = true;
    private bool _loadsNext;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    private SaveMapMode(SaveData save, DataNode data)
    {
        (_save, _data) = (save, data);
        _area = data.Get("stations").Get(save.EntryPoint).Text("area");
    }

    /// <summary>
    /// The map of the saved game in <paramref name="session"/>'s slot; or null when the slot holds no game, or holds
    /// one saved at no place the map marks, which starts with no map.
    /// </summary>
    public static SaveMapMode? Of(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.Saves.Has(session.Slot))
            return null;
        SaveData save = session.Saves.Read(session.Slot);
        DataNode data = session.Content.ReadData(DataFile);
        return data.Get("stations").Has(save.EntryPoint) ? new SaveMapMode(save, data) : null;
    }

    /// <summary>
    /// Whether the saved game in <paramref name="session"/>'s slot is one the map shows before it starts: one saved
    /// at a place the map marks. The two games the cartridge saves by itself about Ceres, on arriving there and on
    /// leaving it, are not, and neither is an empty slot.
    /// </summary>
    public static bool Shows(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Saves.Has(session.Slot)
            && session.Content.ReadData(DataFile).Get("stations").Has(session.Saves.Read(session.Slot).EntryPoint);
    }

    private Vector2I Screen => new(_data.Get("room").Get("map").Int("screen_width"), _data.Get("room").Get("map").Int("screen_height"));

    /// <inheritdoc/>
    protected override void Enter()
    {
        _progress = _save.Remembered();

        var layer = new CanvasLayer { Name = "Screen", Layer = 30 };
        AddChild(layer);
        var black = new ColorRect { Name = "Black", Color = Colors.Black };
        black.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(black);
        _areaClip = new Control { Name = "AreaWindow", ClipContents = true, MouseFilter = Control.MouseFilterEnum.Ignore };
        layer.AddChild(_areaClip);
        _areaScene = BuildWorld();
        _areaClip.AddChild(_areaScene);
        _roomClip = new Control { Name = "RoomWindow", ClipContents = true, MouseFilter = Control.MouseFilterEnum.Ignore };
        layer.AddChild(_roomClip);
        _roomScene = BuildRoom();
        _roomClip.AddChild(_roomScene);
        _fade = new ScreenFade { Name = "Fade", Layer = 40 };
        AddChild(_fade);
        _blackLeft = _data.Get("entry").Int("black_updates");
        ShowScene();
    }

    /// <summary>
    /// Builds the planet: the area's colour behind, the planet's picture, the glow of the area over it, and over
    /// those the title and the name of every area the game has been saved in, the one it was saved in lit.
    /// </summary>
    private Node2D BuildWorld()
    {
        DataNode world = _data.Get("world"), area = world.Get("areas").Get(_area);
        var scene = new Node2D { Name = "World" };
        scene.AddChild(new ColorRect { Name = "Backdrop", Color = new Color(area.Text("backdrop")), Size = Screen, MouseFilter = Control.MouseFilterEnum.Ignore });
        _foreground = Picture("Foreground", area.Text("foreground"), 0, 0);
        scene.AddChild(_foreground);
        Sprite2D glow = Picture("Glow", area.Text("glow"), 0, 0);
        glow.Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        _foreground.AddChild(glow);
        DataNode title = world.Get("title");
        scene.AddChild(Picture("Title", title.Text("picture"), title.Int("x"), title.Int("y")));
        Progress progress = _progress ?? throw NotShown();
        foreach (DataNode named in world.List("order"))
        {
            string name = named.AsText();

            // An area is named once the game has been saved somewhere in it, as it has been where it was saved now.
            bool savedIn = name == _area || _data.Get("stations").Entries()
                .Any(station => station.Value.Text("area") == name && progress.Has(SaveData.SavedAtFact(station.Key)));
            if (!savedIn)
                continue;
            DataNode label = world.Get("areas").Get(name).Get("label");
            scene.AddChild(Picture($"Label{name}", label.Text(name == _area ? "lit" : "dim"), label.Int("x"), label.Int("y")));
        }

        return scene;
    }

    /// <summary>
    /// Builds the area's own map: its frame, the map showing through the frame's opening about where the game was
    /// saved, and over them the marker of that place and the arrows.
    /// </summary>
    private Node2D BuildRoom()
    {
        DataNode room = _data.Get("room"), map = room.Get("map");
        var scene = new Node2D { Name = "AreaMap" };
        scene.AddChild(new ColorRect { Name = "Backdrop", Color = new Color(room.Text("backdrop")), Size = Screen, MouseFilter = Control.MouseFilterEnum.Ignore });
        DataNode frame = room.Get("frames").Get(_area);
        var frameAt = new Vector2I(frame.Int("x"), frame.Int("y"));
        if (frame.Text("picture").Length > 0)
            scene.AddChild(Picture("Frame", frame.Text("picture"), frameAt.X, frameAt.Y));
        _marks = new Node2D { Name = "Marks" };
        _arrowMarks = new Node2D { Name = "Arrows" };
        LayMap(scene, frameAt);
        scene.AddChild(_marks);
        DataNode marker = room.Get("marker"), backing = marker.Get("backing");
        _backing = Picture("Backing", backing.Text("picture"), backing.Int("x"), backing.Int("y"));
        _marks.AddChild(_backing);
        LiftMarkers();
        _marks.AddChild(_arrowMarks);
        foreach ((string way, _, _) in ScrollingMap.Ways)
        {
            DataNode arrow = room.Get("arrows").Get(way);
            Sprite2D sprite = Picture($"Arrow{way}", arrow.List("pictures")[0].AsText(), arrow.Int("x"), arrow.Int("y"));
            sprite.Visible = false;
            _arrowMarks.AddChild(sprite);
            _arrows[way] = sprite;
        }

        BeginRoomMap();
        return scene;
    }

    /// <summary>Lays the area's map anew, about the place the game was saved at, under the marks.</summary>
    private void LayMap(Node2D scene, Vector2I frameAt)
    {
        DataNode room = _data.Get("room");
        if (_map is { } old)
        {
            scene.RemoveChild(old);
            old.QueueFree();
        }

        Area area = Area.Read(Session.Content.ReadData($"{room.Text("area_folder")}/{_area}.json"));
        DataNode anchor = _data.Get("stations").Get(_save.EntryPoint).Get("anchor");
        _map = ScrollingMap.Of(
            room.Get("map"), area, _progress ?? throw NotShown(), Session.Content, frameAt, Vector2I.Zero, new Vector2I(anchor.Int("x"), anchor.Int("y")));
        scene.AddChild(_map);
        if (_marks?.GetParent() == scene)
            scene.MoveChild(_map, _marks.GetIndex());
        if (_backing?.GetParent() is not null)
            LiftMarkers();
    }

    /// <summary>
    /// Lays the map's markers over the save's marker and its backing, and under the arrows: the cartridge draws the
    /// ship and the stations over where the game was saved, and the map's labels under it.
    /// </summary>
    private void LiftMarkers() => Map().LiftMarkersOver(_backing ?? throw NotShown());

    /// <summary>Sets the area's map as it is each time it is come to: laid about the place saved at, its marker's pulse and its highlight begun again.</summary>
    private void BeginRoomMap()
    {
        DataNode room = _data.Get("room");
        if (_roomScene is { } scene)
        {
            DataNode frame = room.Get("frames").Get(_area);
            LayMap(scene, new Vector2I(frame.Int("x"), frame.Int("y")));
        }

        Node2D marks = _marks ?? throw NotShown();
        if (_pulse is { } before)
        {
            marks.RemoveChild(before);
            before.QueueFree();
        }

        // The marker shows its first look from the start, and its backing with it.
        _pulse = CinematicSprite.Of(Session.Content, FrameTimeline.Read(room.Get("marker").Get("pulse")), PixelPosition.Origin);
        _pulse.Name = "Pulse";
        marks.AddChild(_pulse);
        marks.MoveChild(_pulse, (_backing ?? throw NotShown()).GetIndex() + 1);
        _pulse.Step();
        _showsBacking = true;
        (_look, _lookLeft) = (0, room.Get("highlight").Int("first_updates"));
        ShowHighlight();
    }

    /// <inheritdoc/>
    public override bool GivesMusicItsTurn => !_loadsNext;

    /// <inheritdoc/>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        // Every update of the old map draws a random number first and gives the sounds their turns, but for the one
        // that waits out the frame of the display before loading.
        if (!_loadsNext)
            Session.OpenUpdate();
        _closing = _loadsNext ? ClosingSoundWork.None : ClosingSoundWork.Usual;
        GameMode next = StepScreen(input);
        if (ReferenceEquals(next, this))
            ShowScene();
        return next;
    }

    /// <summary>Plays one update of the screen.</summary>
    private GameMode StepScreen(GameInput input)
    {
        // Once the load's wait is over, the old game waits out one more update in black, the map no longer moving,
        // before it begins loading.
        if (_loadsNext)
        {
            Session.Options.Save();
            return GameLoad.OfSlot(appears: true);
        }

        DataNode entry = _data.Get("entry");
        if (_phase == Phase.BlackHold)
        {
            if (--_blackLeft == 0)
                _phase = Phase.Revealing;
            return this;
        }

        if (_phase == Phase.Revealing)
        {
            // When the window's top would pass the screen's top, the whole planet shows instead.
            if (entry.Int("top") - (_revealSteps + 1) * entry.Int("speed") < 0)
                _phase = Phase.Area;
            else
                _revealSteps++;
            return this;
        }

        // What the area's map shows moving is moved on before anything is asked of the player.
        Phase before = _phase;
        _beganIn = before;
        ScrollingMap map = Map();
        if (before == Phase.Room)
        {
            foreach ((string way, _, _) in ScrollingMap.Ways)
                _arrows[way].Visible = map.CanScroll(way);
        }

        if (before is Phase.Room or Phase.LoadRequested)
        {
            StepHighlight();
            if ((_pulse ?? throw NotShown()).Step().Contains("completed"))
                _showsBacking = !_showsBacking;
        }

        if (before == Phase.Room && map.StepScroll(input))
            Sound("scroll");
        Navigate(input);
        if (_phase != before)
        {
            (_waitLength, _waitLeft) = _phase switch
            {
                Phase.AreaReturnRequested => Wait(_data.Get("return").Int("setup_updates")),
                Phase.LoadRequested => Wait(_data.Get("load").Int("prelude_updates") + _data.Int("fade_levels") + _data.Get("load").Int("black_updates")),
                _ => Wait(0),
            };
            switch (_phase)
            {
                case Phase.PreparingWindow or Phase.LoadRequested:
                    Sound("confirm");
                    break;
                case Phase.ExpandingWindow:
                    Sound("expand");
                    break;
                case Phase.AreaReturnRequested:
                    Sound("return");
                    break;
                case Phase.Room:
                    BeginRoomMap();
                    break;
            }

            return this;
        }

        switch (_phase)
        {
            case Phase.AreaReturnRequested:
                if (_waitLeft > 0 && --_waitLeft > 0)
                    return this;
                if (_returnWindow is null)
                {
                    DataNode back = _data.Get("return");
                    _returnWindow = new Window(Reveal(), Screen, back.Int("inset"), back.Int("fewer_updates"));
                    Sound("return");
                }
                else if (_returnWindow.Step())
                {
                    _phase = Phase.Area;
                    _returnWindow = null;
                }

                return this;

            case Phase.OptionsRequested:
                // The screen darkens a level an update, and at black the options come back, made anew as the old
                // game makes them.
                _dark++;
                return _dark == _data.Int("fade_levels") ? new OptionsMode() : this;

            case Phase.LoadRequested:
                if (_waitLeft > 0)
                    _waitLeft--;
                if (_waitLength - _waitLeft <= _data.Get("load").Int("prelude_updates"))
                    return this;
                if (_dark < _data.Int("fade_levels"))
                {
                    _dark++;
                    return this;
                }

                if (_waitLeft > 0)
                    return this;
                _loadsNext = true;
                return this;

            default:
                return this;
        }

        static (int Length, int Left) Wait(int updates) => (updates, updates);
    }

    /// <summary>
    /// Takes what the player newly presses. On the planet a direction or the choosing of an item wastes the update;
    /// otherwise cancel goes back to the options and confirm or pause opens the area's map. The window then opens
    /// by itself. On the area's map cancel closes it and confirm or pause starts the game.
    /// </summary>
    private void Navigate(GameInput input)
    {
        bool cancel = input.WasPressed(GameAction.Cancel);
        bool confirm = input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause);
        switch (_phase)
        {
            case Phase.Area:
                if (ScrollingMap.Ways.Any(way => input.WasPressed(way.Action)) || input.WasPressed(Data.Actions.ItemSelect))
                    return;
                if (cancel)
                    _phase = Phase.OptionsRequested;
                else if (confirm)
                    _phase = Phase.PreparingWindow;
                break;

            case Phase.PreparingWindow:
                DataNode at = _data.Get("world").Get("areas").Get(_area).Get("label_at");
                _window = new Window(Reveal(), new Vector2I(at.Int("x"), at.Int("y")), Screen);
                _phase = Phase.ExpandingWindow;
                break;

            case Phase.ExpandingWindow:
                if ((_window ?? throw NotShown()).Step())
                    _phase = Phase.InitializingRoom;
                break;

            case Phase.InitializingRoom:
                _phase = Phase.Room;
                break;

            case Phase.Room:
                if (cancel)
                    _phase = Phase.AreaReturnRequested;
                else if (confirm)
                    _phase = Phase.LoadRequested;
                break;
        }
    }

    /// <summary>How the window between the planet and the area's map moves.</summary>
    private DataNode Reveal() => _data.Get("world").Get("areas").Get(_area).Get("reveal");

    /// <summary>Moves the highlight on by an update: each look lasts its own while, and it sounds as it comes round to the first.</summary>
    private void StepHighlight()
    {
        if (--_lookLeft > 0)
            return;
        IReadOnlyList<DataNode> updates = _data.Get("room").Get("highlight").List("updates");
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
        DataNode arrows = _data.Get("room").Get("arrows");
        foreach ((string way, Sprite2D arrow) in _arrows)
            arrow.Texture = Session.Content.LoadTexture(arrows.Get(way).List("pictures")[_look].AsText());
    }

    /// <summary>
    /// Shows what the screen now shows: black, the planet through its opening window, the planet, the frame of the
    /// area's map through the window opening over the planet, the area's map, or the window closing back.
    /// </summary>
    private void ShowScene()
    {
        Vector2I screen = Screen;
        var whole = new Rect2I(Vector2I.Zero, screen);
        var none = new Rect2I(0, 0, 0, 0);
        DataNode entry = _data.Get("entry");
        ScrollingMap map = Map();
        bool roomStepped = _beganIn == Phase.Room;

        // What the area's map shows: its frame alone, its cells, and what lies over them once it has been moved on.
        void RoomShows(bool cells, bool signs, bool arrows)
        {
            map.ShowsCells = cells;
            map.ShowsSigns = signs;
            (_marks ?? throw NotShown()).Visible = signs;
            (_arrowMarks ?? throw NotShown()).Visible = arrows;
        }

        // The glow lies over the whole of the planet's screen, or only over the planet itself.
        void Glow(bool planetOnly) =>
            (_foreground ?? throw NotShown()).ClipChildren = planetOnly ? CanvasItem.ClipChildrenMode.AndDraw : CanvasItem.ClipChildrenMode.Disabled;

        switch (_phase)
        {
            case Phase.BlackHold:
                Lay(none, none, roomOnTop: true);
                break;

            case Phase.Revealing:
                int left = entry.Int("left") - _revealSteps * entry.Int("speed"), right = entry.Int("right") + _revealSteps * entry.Int("speed");
                int top = entry.Int("top") - _revealSteps * entry.Int("speed");
                Glow(planetOnly: false);
                Lay(new Rect2I(left, top, right + 1 - left, screen.Y - top - top), none, roomOnTop: true);
                break;

            case Phase.Area or Phase.PreparingWindow or Phase.OptionsRequested:
                Glow(planetOnly: false);
                Lay(whole, none, roomOnTop: true);
                break;

            case Phase.ExpandingWindow or Phase.InitializingRoom:
                Window window = _window ?? throw NotShown();
                Glow(planetOnly: true);
                RoomShows(cells: false, signs: false, arrows: false);
                Lay(whole, window.IsComplete ? whole : window.Inside, roomOnTop: true);
                break;

            case Phase.Room or Phase.LoadRequested:
                // The map's first update shows its cells alone; the load shows no arrows once it is under way.
                RoomShows(cells: true, signs: roomStepped || _phase == Phase.LoadRequested, arrows: roomStepped);
                Lay(none, whole, roomOnTop: true);
                break;

            case Phase.AreaReturnRequested when roomStepped:
                RoomShows(cells: true, signs: true, arrows: true);
                Lay(none, whole, roomOnTop: true);
                break;

            case Phase.AreaReturnRequested when _waitLeft == 1:
                // On the last update of the setup the planet shows through the window the screen first opened by.
                Glow(planetOnly: false);
                RoomShows(cells: false, signs: false, arrows: false);
                Lay(new Rect2I(entry.Int("left"), entry.Int("top"), entry.Int("right") + 1 - entry.Int("left"), screen.Y - 2 * entry.Int("top")), whole, roomOnTop: false);
                break;

            case Phase.AreaReturnRequested when _returnWindow is { } closing:
                Glow(planetOnly: false);
                RoomShows(cells: false, signs: false, arrows: false);
                Lay(whole, closing.IsComplete ? none : closing.Inside, roomOnTop: true);
                break;

            case Phase.AreaReturnRequested:
                RoomShows(cells: false, signs: false, arrows: false);
                Lay(none, whole, roomOnTop: true);
                break;

            default:
                throw new InvalidOperationException($"The map of a saved game is doing {_phase}, which is nothing it can do.");
        }

        // The marker lies where the game was saved, on the map as it now lies.
        DataNode station = _data.Get("stations").Get(_save.EntryPoint);
        Vector2I at = map.OnScreen(new Vector2I(station.Int("x"), station.Int("y")));
        CinematicSprite pulse = _pulse ?? throw NotShown();
        pulse.Place = PixelPosition.FromWhole(at.X, at.Y);
        pulse.ShowPlace();
        Sprite2D backing = _backing ?? throw NotShown();
        DataNode placed = _data.Get("room").Get("marker").Get("backing");
        backing.Position = new Vector2(at.X + placed.Int("x"), at.Y + placed.Int("y"));
        backing.Visible = _showsBacking;
        (_fade ?? throw NotShown()).Cover(_dark, _data.Int("fade_levels"));
    }

    /// <summary>Shows the planet through one part of the screen and the area's map through another, one of them over the other; a part of no size shows nothing.</summary>
    private void Lay(Rect2I world, Rect2I room, bool roomOnTop)
    {
        Control areaClip = _areaClip ?? throw NotShown(), roomClip = _roomClip ?? throw NotShown();
        Through(areaClip, _areaScene ?? throw NotShown(), world);
        Through(roomClip, _roomScene ?? throw NotShown(), room);
        Node layer = areaClip.GetParent();
        layer.MoveChild(roomOnTop ? roomClip : areaClip, layer.GetChildCount() - 1);

        static void Through(Control clip, Node2D scene, Rect2I part)
        {
            clip.Visible = part.Size.X > 0 && part.Size.Y > 0;
            clip.Position = part.Position;
            clip.Size = part.Size;
            scene.Position = -part.Position;
        }
    }

    private Sprite2D Picture(string name, string picture, int x, int y) => new()
    {
        Name = name, Texture = Session.Content.LoadTexture(picture), Centered = false, Position = new Vector2(x, y),
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
    };

    private void Sound(string name) => Session.Sounds.Request(_data.Get("sounds").Text(name), channel: "menu");

    private ScrollingMap Map() => _map ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The map of a saved game has not been shown.");
}
