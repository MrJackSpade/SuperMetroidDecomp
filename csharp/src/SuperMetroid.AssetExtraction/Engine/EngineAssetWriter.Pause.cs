using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
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

        MapFraming framing = MapFramingDefinitions.Pause;
        const int cell = GameplayHudDefinitions.MinimapSquarePixels;
        _tree.WriteData("pause/pause.json", new JsonObject
        {
            ["backdrop"] = Colour(screens.Backdrops.Pause),
            ["backdrops"] = backdrops,
            ["frame"] = new JsonArray(Placed(PauseFramePictures.ButtonRowsFrame)),
            ["buttons"] = buttons,
            ["map"] = new JsonObject
            {
                ["x"] = framing.LeftEdge, ["y"] = framing.TopEdge, ["columns"] = (framing.RightEdge - framing.LeftEdge) / cell,
                ["rows"] = (framing.BottomEdge - framing.TopEdge) / cell,
            },
            ["area_folder"] = "areas",
            ["sounds"] = new JsonObject
            {
                ["move"] = EngineNames.Sound(SamusSound.MenuCursor),
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
                ["controls"] = controls,
                ["wireframes"] = wireframes,
                ["reserve"] = new JsonObject
                {
                    ["absent"] = Placed(PauseEquipmentPictures.ReserveAbsentFrame),
                    ["arrow_none"] = Placed(PauseEquipmentPictures.ReserveArrowFrame(ShownReserveArrow.NoReserves)),
                    ["tank"] = Placed(PauseEquipmentPictures.ReserveTankFrame),
                    ["arrow"] = Placed(PauseEquipmentPictures.ReserveArrowFrame(ShownReserveArrow.Dim)),
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
