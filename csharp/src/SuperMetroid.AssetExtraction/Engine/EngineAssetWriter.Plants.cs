using System.Text.Json.Nodes;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The plants of Brinstar that seize Samus: what each kind does, from the old game's own definitions.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the plants' data: for the one in a floor and the one in a ceiling, its looks, its bites and its timing.</summary>
    /// <exception cref="InvalidDataException">A plant's mouth is not the shape this knows how to say.</exception>
    private void WriteSamusEaters()
    {
        var data = new JsonObject();
        foreach ((string name, SpecialAirReaction air) in new[] { ("floor", SpecialAirReaction.FloorPlant), ("ceiling", SpecialAirReaction.CeilingPlant) })
        {
            SamusEaterObjectDefinition plant = EngineLayouts.PlantOf(air) ?? throw new InvalidDataException($"The air {air} is no plant.");

            // Its mouth is the block that seizes and the blocks beside it that stand for that block.
            RoomBlockShape waiting = SamusEaterObjectDraws.Family.Shape(plant.Idle);
            if (waiting.Cells.Count(cell => cell.Collision == RoomCollisionType.SpecialAir) != 1
                || waiting.Cells.Single(cell => cell.Collision == RoomCollisionType.SpecialAir).Cell != new Offset16<RoomBlocks>(0, 0))
                throw new InvalidDataException($"The plant {air} does not seize at the one block it is placed by.");
            int reachLeft = 0;
            while (waiting.Cells.Any(cell => cell.Cell == new Offset16<RoomBlocks>(-(reachLeft + 1), 0) && cell.Collision == RoomCollisionType.HorizontalExtension))
                reachLeft++;
            if (waiting.Cells.Count(cell => cell.Collision == RoomCollisionType.HorizontalExtension) != reachLeft)
                throw new InvalidDataException($"The mouth of the plant {air} reaches somewhere other than to the left of its block.");

            var chew = new JsonArray();
            foreach (SamusEaterChewFrame frame in plant.Chew)
            {
                chew.Add(new JsonObject
                {
                    ["look"] = frame.Image == plant.Idle ? "" : EngineLayouts.PlantLook(frame.Image),
                    ["updates"] = frame.Updates,
                    ["bites"] = frame.BiteBefore is SamusEaterBite.Damage or SamusEaterBite.SoundAndDamage,
                    ["sound"] = frame.BiteBefore == SamusEaterBite.SoundAndDamage,
                });
            }

            data[name] = new JsonObject
            {
                ["block"] = EngineLayouts.PlantBlockFile(air),
                ["reach_left"] = reachLeft,

                // She is held a pixel out of the plant from where she was: down from one in a ceiling, up from one in a floor.
                ["held_offset"] = plant.Side == VerticalDirection.Up ? 1 : -1,
                ["chew"] = chew,
                ["chews"] = plant.Chews,
                ["released"] = plant.Released == plant.Idle ? "" : EngineLayouts.PlantLook(plant.Released),
            };
        }

        data["release_updates"] = SamusEaterObjectDefinitions.ReleaseDuration;
        data["damage_hundredths"] = Hundredths(SamusEaterObjectDefinitions.ChewDamage);
        data["chew_sound"] = EngineNames.Sound(SamusEaterObjectDefinitions.ChewSound);
        data["held_invincible_updates"] = SamusEaterObjectDefinitions.HeldInvincibilityFrames;
        data["released_invincible_updates"] = SamusEaterObjectDefinitions.ReleaseInvincibilityFrames;
        _tree.WriteData("objects/samus_eater.json", data);
    }
}
