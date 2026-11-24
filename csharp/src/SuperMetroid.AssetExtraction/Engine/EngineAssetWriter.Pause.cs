using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The pause screen: its backdrop and buttons, where its map shows, and the equipment page with everything Samus can switch.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The Godot game's name for one of the things Samus carries that the old game keeps as a flag.</summary>
    private static string EquipmentName(SamusEquipmentFlags item) => item switch
    {
        SamusEquipmentFlags.VariaSuit => "varia_suit",
        SamusEquipmentFlags.SpringBall => "spring_ball",
        SamusEquipmentFlags.MorphBall => "morph_ball",
        SamusEquipmentFlags.ScrewAttack => "screw_attack",
        SamusEquipmentFlags.GravitySuit => "gravity_suit",
        SamusEquipmentFlags.HiJumpBoots => "hi_jump_boots",
        SamusEquipmentFlags.SpaceJump => "space_jump",
        SamusEquipmentFlags.Bombs => "bombs",
        SamusEquipmentFlags.SpeedBooster => "speed_booster",
        SamusEquipmentFlags.GrappleBeam => "grapple_beam",
        SamusEquipmentFlags.XrayScope => "xray_scope",
        _ => throw new InvalidDataException($"The Godot game has no one name for {item}."),
    };

    /// <summary>The Godot game's name for one of Samus's beams.</summary>
    private static string BeamName(SamusBeamFlags beam) => beam switch
    {
        SamusBeamFlags.Charge => "charge_beam",
        SamusBeamFlags.Ice => "ice_beam",
        SamusBeamFlags.Wave => "wave_beam",
        SamusBeamFlags.Spazer => "spazer",
        SamusBeamFlags.Plasma => "plasma_beam",
        _ => throw new InvalidDataException($"The Godot game has no one name for {beam}."),
    };

    /// <summary>Writes the pause screen's data file and pictures.</summary>
    private void WritePause()
    {
        const string folder = "pause";
        const SpriteSheetId sheet = SpriteSheetId.PausePictures;
        InstalledScreenContent screens = _installation.LoadMaps();

        // A picture of the screen lies with the point the old game keeps for it at the screen's corner. One the
        // installation lists as showing nothing has no picture.
        JsonObject Placed(string frame)
        {
            if (_sheets.SpriteIsBlank(sheet, frame))
                return new JsonObject { ["picture"] = "", ["x"] = 0, ["y"] = 0 };
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(sheet, frame);
            JsonObject at = At(-originX, -originY);
            at["picture"] = _sheets.WriteSprite(_tree, sheet, frame, $"{folder}/{EngineSheets.FileName(frame)}.png");
            return at;
        }

        // A selector lies with its own point at the anchor of what it selects.
        JsonObject Selector(PauseEquipmentControl control)
        {
            string frame = PauseSelectorAnchors.SheetFrameName(screens.PauseSelectors.Looks[0].FrameOf(control.Category));
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.PauseSprites, frame);
            JsonObject at = At(screens.PauseSelectors.Anchor(control).X - originX, screens.PauseSelectors.Anchor(control).Y - originY);
            at["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.PauseSprites, frame, $"{folder}/{EngineSheets.FileName(frame)}.png");
            return at;
        }

        var backdrops = new JsonObject();
        foreach (AreaId area in AreaIds.All)
            backdrops[Named(area)] = Placed(PauseFramePictures.BackdropFrame(area));

        var buttons = new JsonObject();
        foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
        {
            JsonObject lit = Placed(PauseFramePictures.ButtonFrame(label, PauseButtonLabelShade.Lit));
            JsonObject dim = Placed(PauseFramePictures.ButtonFrame(label, PauseButtonLabelShade.Dim));
            if (lit["x"]!.GetValue<int>() != dim["x"]!.GetValue<int>() || lit["y"]!.GetValue<int>() != dim["y"]!.GetValue<int>())
                throw new InvalidDataException($"The pause screen's {label} button lies in two places, lit and dim.");
            buttons[Named(label)] = new JsonObject
            {
                ["lit"] = lit["picture"]!.GetValue<string>(), ["dim"] = dim["picture"]!.GetValue<string>(), ["x"] = lit["x"]!.GetValue<int>(),
                ["y"] = lit["y"]!.GetValue<int>(),
            };
        }

        // The things she can switch: the beams on the left of the page, under her reserves; her suits and boots on the right.
        var controls = new JsonArray();
        foreach (PauseUpgradeControl control in PauseEquipmentControls.Upgrades)
        {
            JsonObject on = Placed(PauseEquipmentPictures.LabelFrame(control.Label, PauseEquipmentLabelState.Enabled));
            JsonObject off = Placed(PauseEquipmentPictures.LabelFrame(control.Label, PauseEquipmentLabelState.Disabled));
            bool beam = control.Category == PauseEquipmentCategory.Beams;
            controls.Add(new JsonObject
            {
                ["equipment"] = control.Upgrade switch
                {
                    BeamPauseUpgrade beamUpgrade => BeamName(beamUpgrade.Beam),
                    ItemPauseUpgrade item => EquipmentName(item.Item),
                    _ => throw new InvalidDataException($"The pause screen switches {control.Upgrade}, which this does not know."),
                },
                ["side"] = beam ? "left" : "right",
                ["beam"] = beam,
                // The column of the page it is in, which the selector moves along and between.
                ["group"] = Named(control.Category),
                ["on"] = on["picture"]!.GetValue<string>(),
                ["off"] = off["picture"]!.GetValue<string>(),
                ["x"] = on["x"]!.GetValue<int>(),
                ["y"] = on["y"]!.GetValue<int>(),
                ["selector"] = Selector(control),
            });
        }

        var digits = new JsonArray();
        for (int place = 0; place < new ShownReserveDisplay(ReserveTankMode.Auto, 0).Digits().Length; place++)
        {
            JsonObject at = Placed(PauseEquipmentPictures.ReserveDigitFrame(place, 0));
            var pictures = new JsonArray();
            for (int digit = 0; digit < GameplayHudDefinitions.Numerals; digit++)
                pictures.Add(Placed(PauseEquipmentPictures.ReserveDigitFrame(place, digit))["picture"]!.GetValue<string>());
            digits.Add(new JsonObject { ["x"] = at["x"]!.GetValue<int>(), ["y"] = at["y"]!.GetValue<int>(), ["pictures"] = pictures });
        }

        var wireframes = new JsonObject();
        foreach (PauseWireframe wireframe in PauseWireframe.All)
        {
            string name = (wireframe.Suit, wireframe.Boots) switch
            {
                (PauseWireframeSuit.PowerSuit, PauseWireframeBoots.RegularBoots) => "plain",
                (PauseWireframeSuit.PowerSuit, PauseWireframeBoots.HiJumpBoots) => "hi_jump",
                (PauseWireframeSuit.VariaSuit, PauseWireframeBoots.RegularBoots) => "varia",
                (PauseWireframeSuit.VariaSuit, PauseWireframeBoots.HiJumpBoots) => "varia_hi_jump",
                _ => throw new InvalidDataException($"The pause screen draws her as {wireframe}, which this does not know."),
            };
            wireframes[name] = Placed(PauseEquipmentPictures.WireframeFrame(wireframe));
        }

        // The sign of where Samus is on the map: a ring that swells and shrinks, about the cell she is in.
        var pulse = new JsonArray();
        foreach (SuperMetroid.Core.Animation.AnimationBeat<MapSpriteId, MapIndicatorCue> beat in PauseMapIndicatorAnimation.Segments.Beats(MapIndicatorSegment.Pulse))
        {
            if (beat is not SuperMetroid.Core.Animation.AnimationBeat<MapSpriteId, MapIndicatorCue>.Frame shown)
                continue;
            string frame = MapSpriteDefinitions.Names[shown.Shown];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            pulse.Add(new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(
                    _tree, SpriteSheetId.MapSprites, frame, $"pause/map_{EngineSheets.FileName(frame)}.png",
                    MapSpriteShades.Drawn(MapSpriteScreen.Pause, MapSpriteColouring.Markers).Key),
                ["updates"] = shown.Duration,
                ["offset"] = At(-originX, -originY),
            });
        }

        // The bar of her reserves: a little tank for each she has, full, empty or part filled, and a cap after the last.
        string Bar(PauseReserveTankVisual visual) => _sheets.WriteSprite(
            _tree, SpriteSheetId.PauseSprites, PauseReserveTankParts.SheetFrameName(PauseReserveTankDefinitions.InstalledNames.Of(visual)),
            $"pause/reserve_bar_{Named(visual)}.png");
        var barPlaces = new JsonArray();
        for (int place = 0; place < PauseReserveTankDefinitions.AnchorCount; place++)
        {
            var anchor = screens.PauseReserveTanks.Anchor(place);
            barPlaces.Add(At(anchor.X, anchor.Y));
        }

        var bar = new JsonObject
        {
            ["full"] = Bar(PauseReserveTankVisual.Full),
            ["end_cap"] = Bar(PauseReserveTankVisual.EndCap),
            // A tank by how many steps full it is, from none.
            ["fills"] = new JsonArray([.. PauseReserveTankDefinitions.FillLevels.Select(level => (JsonNode)Bar(level))]),
            ["places"] = barPlaces,
            ["energy_per_tank"] = SamusEnergyDefinitions.EnergyPerTank,
            ["energy_per_step"] = PauseReserveEnergyDefinitions.EnergyPerFillStep,
            ["flicker_below_steps"] = PauseReserveEnergyDefinitions.FlickerFillStepLimit,
            ["flicker_updates"] = PauseCadenceDefinitions.ReserveFlickerStepHidden.CountsEachWay,
        };

        MapFraming framing = MapFramingDefinitions.Pause;
        const int cell = GameplayHudDefinitions.MinimapSquarePixels;

        // The arrows that say which ways the map can be moved, their colours going round as the old highlight does.
        var arrows = new JsonObject();
        foreach (MapScrollDirection direction in MapScrollControls.Directions)
        {
            string frame = MapSpriteDefinitions.Names[MapArrowDefinitions.ArrowPictures[direction]];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            var looks = new JsonArray();
            foreach (MapHighlightFrame look in screens.Maps.HighlightCycle.Frames)
            {
                looks.Add(new JsonObject
                {
                    ["picture"] = _sheets.WriteSprite(
                        _tree, SpriteSheetId.MapSprites, frame, $"pause/map_{EngineSheets.FileName(frame)}_{look.Frame}.png", MapSpriteShades.Highlight(look).Key),
                    ["updates"] = screens.Maps.HighlightCycle.Duration(look),
                });
            }

            var at = screens.Maps.Arrows.Get(direction).Position;
            arrows[Named(direction)] = new JsonObject
            {
                ["x"] = at.X,
                ["y"] = at.Y + framing.ArrowDrop,
                ["timeline"] = new JsonObject { ["steps"] = looks, ["loops_to"] = 0, ["offset"] = At(-originX, -originY) },
            };
        }
        _tree.WriteData("pause/pause.json", new JsonObject
        {
            ["backdrop"] = Colour(screens.Backdrops.Pause),
            ["backdrops"] = backdrops,
            ["frame"] = new JsonArray(Placed(PauseFramePictures.ButtonRowsFrame)),
            ["buttons"] = buttons,
            ["map"] = new JsonObject
            {
                // The map is a plane behind the page, moved under it. These say where it first lies and how far it moves.
                ["cell"] = cell,
                ["screen_width"] = SuperMetroid.Core.Spatial.ScreenGeometry.ScreenWidthPixels,
                ["screen_height"] = SuperMetroid.Core.Spatial.ScreenGeometry.ScreenHeightPixels,
                ["centre_line"] = framing.CentreLine,
                ["left_edge"] = framing.LeftEdge,
                ["right_edge"] = framing.RightEdge,
                ["top_edge"] = framing.TopEdge,
                ["bottom_edge"] = framing.BottomEdge,
                ["centre_column"] = MapFramingDefinitions.CentreColumn,
                ["row_middle_above_centre"] = MapFramingDefinitions.RowMiddleAboveCentredLine,
                ["here_leftmost"] = MapFramingDefinitions.MarkerLeftmostColumn,
                ["here_rightmost"] = MapFramingDefinitions.MarkerRightmostColumn,
                ["here_top"] = MapFramingDefinitions.MarkerTopLine,
                ["lowest_map_top"] = MapFramingDefinitions.LowestMapTop,
                // The cells its bounds are taken to be while none of it shows.
                ["unrevealed"] = new JsonObject
                {
                    ["left"] = MapFramingDefinitions.UnrevealedLeftColumn, ["right"] = MapFramingDefinitions.UnrevealedRightColumn,
                    ["top"] = MapFramingDefinitions.UnrevealedTopRow, ["bottom"] = MapFramingDefinitions.UnrevealedBottomRow,
                },
                ["scroll_step"] = MapScrollDefinitions.StepPixels,
                ["scroll_updates"] = MapScrollDefinitions.StepTicks,
                ["scroll_moves_at"] = MapScrollDefinitions.PulseTick,
                ["arrows"] = arrows,
                ["indicator"] = new JsonObject { ["steps"] = pulse, ["loops_to"] = 0 },
            },
            ["area_folder"] = "areas",
            ["sounds"] = new JsonObject
            {
                ["move"] = EngineNames.Sound(SamusSound.MenuCursor),
                ["scroll"] = EngineNames.Sound(SamusSound.MapScroll),
                ["switch"] = EngineNames.Sound(SamusSound.MenuConfirm),
                ["page"] = EngineNames.Sound(SamusSound.MenuConfirm),
                ["pour"] = EngineNames.Sound(PauseReserveEnergyDefinitions.RefillSound),
                ["pour_every"] = PauseReserveEnergyDefinitions.RefillSoundInterval,
            },
            ["equipment"] = new JsonObject
            {
                ["base"] = Placed(PauseEquipmentPictures.BaseFrame),
                ["hyper"] = Placed(PauseEquipmentPictures.HyperBeamsFrame),
                ["hyper_side"] = "left",
                // The suit the selector goes to from the beams when it is sent right.
                ["beams_right_enters"] = EquipmentName(((ItemPauseUpgrade)PauseEquipmentControls.BeamsRightSuitEntry.Upgrade).Item),
                ["controls"] = controls,
                ["wireframes"] = wireframes,
                ["reserve"] = new JsonObject
                {
                    ["absent"] = Placed(PauseEquipmentPictures.ReserveAbsentFrame),
                    ["arrow_none"] = Placed(PauseEquipmentPictures.ReserveArrowFrame(ShownReserveArrow.NoReserves)),
                    ["tank"] = Placed(PauseEquipmentPictures.ReserveTankFrame),
                    ["arrow"] = Placed(PauseEquipmentPictures.ReserveArrowFrame(ShownReserveArrow.Dim)),
                    ["bar"] = bar,
                    ["auto"] = Placed(PauseEquipmentPictures.ReserveModeFrame(ReserveTankMode.Auto)),
                    ["manual"] = Placed(PauseEquipmentPictures.ReserveModeFrame(ReserveTankMode.Manual)),
                    ["digits"] = digits,
                    ["mode_selector"] = Selector(PauseEquipmentControls.ReserveMode),
                    ["transfer_selector"] = Selector(PauseEquipmentControls.ReserveTransfer),
                },
            },
        });
        if (PauseEquipmentRules.ReserveEnergyPerFrame != 1)
            throw new InvalidDataException("The old game pours more than one of her reserves an update, where the Godot pause screen pours one.");
    }
}
