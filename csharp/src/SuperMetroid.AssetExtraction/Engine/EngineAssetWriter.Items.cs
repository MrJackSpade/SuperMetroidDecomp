using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The items: what each grants, its message, and its two looks, which it shows by turns. A tank looks as the room's
/// own blocks draw it; anything else has a picture of its own, which is on the sheets of the sets of graphics of the
/// rooms that hold it. And the shell some items are held in, which any weapon bursts.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The name the Godot game asks for one of the old game's messages by.</summary>
    internal static string MessageName(SuperMetroid.Core.Rooms.GameplayMessageId message) => EngineSheets.FileName(message.ToString());

    /// <summary>The name the Godot game asks for one of the old game's pieces of music by.</summary>
    internal static string MusicName(MusicTrack track) => EngineSheets.FileName(track.ToString());

    /// <summary>Writes every item's type and looks, and the shell.</summary>
    private void WriteItems()
    {
        const string directory = GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName;
        static string Frame(CollectibleDraw draw) => EngineSheets.FileName(draw.ToString()).Replace('_', '-');

        foreach ((InWorldCollectibleKind kind, CollectibleDefinition item) in CollectibleDefinitions.Items)
        {
            string name = EngineSheets.FileName(kind.ToString());

            // What the old game's grant does, in the Godot game's words. A tank raises a capacity; an energy tank
            // also fills her energy, and a reserve tank gives none of what it holds and switches the reserves to be
            // used by themselves the first time. A beam that cannot be fired with another switches that one off.
            (string Equipment, string Expands, int By, int? Adds, bool Fills, string[] SwitchesOff) grant = kind switch
            {
                InWorldCollectibleKind.EnergyTank => ("", "energy", SamusEnergyDefinitions.EnergyPerTank, null, true, []),
                InWorldCollectibleKind.ReserveTank => ("reserve_tank", "reserve_energy", SamusEnergyDefinitions.EnergyPerTank, 0, false, []),
                InWorldCollectibleKind.MissileTank => ("", "missiles", CollectibleCapacityDefinitions.MissileTankMissiles, null, false, []),
                InWorldCollectibleKind.SuperMissileTank => ("", "super_missiles", CollectibleCapacityDefinitions.SuperMissileTankSuperMissiles, null, false, []),
                InWorldCollectibleKind.PowerBombTank => ("", "power_bombs", CollectibleCapacityDefinitions.PowerBombTankPowerBombs, null, false, []),
                InWorldCollectibleKind.SpazerBeam => ("spazer", "", 0, null, false, ["plasma_beam"]),
                InWorldCollectibleKind.PlasmaBeam => ("plasma_beam", "", 0, null, false, ["spazer"]),
                InWorldCollectibleKind.Bombs or InWorldCollectibleKind.HiJumpBoots or InWorldCollectibleKind.SpeedBooster
                    or InWorldCollectibleKind.SpringBall or InWorldCollectibleKind.XrayScope or InWorldCollectibleKind.GrappleBeam
                    or InWorldCollectibleKind.SpaceJump or InWorldCollectibleKind.ScrewAttack or InWorldCollectibleKind.MorphBall
                    or InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit or InWorldCollectibleKind.ChargeBeam
                    or InWorldCollectibleKind.IceBeam or InWorldCollectibleKind.WaveBeam => (name, "", 0, null, false, []),
                _ => throw new InvalidDataException($"The item {kind} grants something this does not know."),
            };

            string Look(CollectibleImage image)
            {
                CollectibleLook look = image.What is CollectibleImage.VisibleLook visible
                    ? visible.Look
                    : throw new InvalidDataException("An item lying in the open shows something that is not one of its looks.");
                return item.Picture == CollectiblePictureSource.SharedTankBlocks
                    ? RoomObjects.Picture(directory, Frame(CollectibleObjectDraws.TankFrame(kind, look).Frame), "items")
                    : RoomObjects.SheetPicture(RoomBlockPictureNames.OfCollectible(new CollectiblePicture(kind, look)), "items", $"{name}_{look}");
            }

            string looks = $"items/{name}_looks.json";
            _tree.WriteData(looks, EngineTimelines.Of(
                CollectibleSequenceAnimations.Segments, CollectibleSequenceAnimations.Exposed, Look, static (CollectiblePhase _) => null));

            var type = new JsonObject
            {
                ["name"] = name,
                ["message"] = MessageName(item.Message),
                ["message_hold_updates"] = GameplayMessageBoxDefinitions.Messages[item.Message].MinimumDisplayFrames
                    ?? throw new InvalidDataException($"The message of the item {kind} has no time it stays up."),
                ["fanfare"] = MusicName(AudioQueueDefinitions.PermanentItemTrack),
                ["timeline"] = looks,
            };
            if (grant.Equipment.Length > 0)
                type["grants_equipment"] = grant.Equipment;
            if (grant.Expands.Length > 0)
            {
                type["expands"] = grant.Expands;
                type["expands_by"] = grant.By;
                if (grant.Adds is { } given)
                    type["adds"] = given;
                if (grant.Fills)
                    type["fills"] = true;
            }

            if (grant.SwitchesOff.Length > 0)
                type["switches_off"] = new JsonArray([.. grant.SwitchesOff.Select(off => (JsonNode)off)]);
            _tree.WriteData($"items/{name}.json", type);
        }

        // The shell: an orb that pulses until a shot bursts it. Nothing of it is remembered: it is whole again each
        // time its room is entered, for as long as its item has not been taken. The first hit that lands bursts it, so
        // it never shows itself hurt and has no look for that.
        string Orb(CollectibleImage image)
        {
            CollectibleDraw draw = image.What is CollectibleImage.DrawnImage { Draw: RoomObjectFrameDraw<CollectibleDraw> framed }
                ? framed.Frame
                : throw new InvalidDataException("An item's shell shows something that is not a frame of its own.");
            return draw == CollectibleDraw.Empty ? "" : RoomObjects.Picture(directory, Frame(draw), "objects/item_shell");
        }

        // As it bursts the shell is in the way only while it shows its burst: the frames of it that show nothing are
        // air on the cartridge, and the burst's own frame a solid block.
        static JsonObject Bursting(JsonObject timeline)
        {
            foreach (JsonNode? step in timeline["steps"]!.AsArray())
                step!["cues"] = new JsonArray(step["picture"]!.GetValue<string>().Length == 0 ? "passable" : "blocking");
            return timeline;
        }

        const int block = EngineRoomObjects.Block;
        var burstBy = new JsonObject();
        foreach (JsonNode? weapon in EngineLayouts.AnyWeapon())
            burstBy[weapon!.GetValue<string>()] = 1;
        new EngineObjectFile(_tree, "objects/item_shell.json", "Barrier")
            .Value("health", 1)
            .Value("any_hit", 0)
            .Group("damaged_by", burstBy)
            .Collider("Solid", 0, 0, block, block)
            .Collider("Hurtbox", 0, 0, block, block)
            .Timeline("closed", EngineTimelines.Of(
                CollectibleSequenceAnimations.Segments, CollectibleSequenceAnimations.OrbPulsing, Orb, static (CollectiblePhase _) => null))
            .Timeline("opening", Bursting(EngineTimelines.Of(
                CollectibleSequenceAnimations.Segments, CollectibleSequenceAnimations.OrbBurst, Orb, static (CollectiblePhase _) => null)))
            .Timeline("open", new JsonObject { ["steps"] = new JsonArray(new JsonObject { ["picture"] = "", ["updates"] = 1 }), ["loops_to"] = 0 })
            .Write();
    }
}
