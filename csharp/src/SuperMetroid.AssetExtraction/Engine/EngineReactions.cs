using System.Text.Json.Nodes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Turns the old game's table of what each weapon does to an enemy into the Godot game's list of reactions. The old
/// table has an entry for every beam Samus can fire, for a charged shot, for each missile and bomb and for each way
/// her body can strike. A hit in the Godot game carries every kind that is true of it, and the reaction that applies
/// is the one naming the most of them; so the list is found by writing a reaction for every hit Samus can deliver and
/// then taking away every reaction, and every kind in one, that the rest answer for just as the table does.
/// </summary>
internal static partial class EngineReactions
{
    /// <summary>What a hit does: how many halves of its damage are taken, and what else follows.</summary>
    /// <param name="Halves">The damage taken, in halves of the hit's.</param>
    /// <param name="Kills">Whether the hit destroys the enemy whatever its health.</param>
    /// <param name="Freezes">Whether it freezes the enemy every time.</param>
    /// <param name="FreezesInsteadOfKilling">Whether it freezes the enemy in place of destroying it.</param>
    /// <param name="Unhittable">How many updates the enemy cannot be hit after taking the damage.</param>
    private readonly partial record struct Outcome(
        int Halves, bool Kills = false, bool Freezes = false, bool FreezesInsteadOfKilling = false, int Unhittable = 0, bool Passes = false);

    /// <summary>What a hit with no reaction does in the Godot game: its whole damage and nothing more.</summary>
    private static readonly Outcome Unanswered = new(2);

    private static readonly Outcome Nothing = new(0);

    /// <summary>
    /// What the old game's reaction that does nothing comes to: the hit takes no notice of the enemy, and what
    /// delivered it goes on unspent. A dud is not that: it does nothing to the enemy and spends the shot.
    /// </summary>
    private static readonly Outcome Passed = new(0, Passes: true);

    /// <summary>
    /// The damage the grapple beam's hit carries. The old grapple takes no health from anything it touches: it stops
    /// on an enemy, holds it, or destroys it outright. A reaction can only take a share or a multiple of a hit's damage,
    /// so the hit carries the least damage there is, and the enemies it destroys take it as many times over as they have
    /// health; every enemy's reactions answer it, none by its plain damage.
    /// </summary>
    internal const int GrappleHitDamage = 1;

    /// <summary>The reactions of the enemy the old game defines as <paramref name="definition"/>.</summary>
    internal static JsonArray Of(RoomEnemyDefinition definition)
    {
        EnemyVulnerabilities table = definition.Vulnerability;
        // An enemy whose shots are duds, or pass it by, is harmed by none of them whatever its table says. Shots pass
        // one whose reaction is to do nothing, and the two species whose own reaction lets a shot fly on.
        bool shotsPass = ReferenceEquals(definition.Shot, EnemyReaction.Ignore)
            || ReferenceEquals(definition.Shot, SuperMetroid.Core.Enemies.FaceBlocks.FaceBlockReactions.Shot)
            || ReferenceEquals(definition.Shot, SuperMetroid.Core.Enemies.Sparks.SparkReactions.Shot);
        bool shotsHarm = !definition.Shot.IsDud && !shotsPass;
        Outcome spared = shotsPass ? Passed : Nothing;
        bool touchHarms = !ReferenceEquals(definition.Touch, EnemyReaction.Ignore);
        bool powerBombHarms = !ReferenceEquals(definition.PowerBombReaction, EnemyReaction.Ignore);

        var hits = new List<(string[] Kinds, Outcome Outcome)>();
        void Beams(string[] family, EnemyBeamFamilyVulnerability beams)
        {
            (string[] Kinds, EnemyWeaponVulnerability Beam)[] combined =
            [
                ([], beams.Plain), (["wave"], beams.Wave), (["ice"], beams.Ice), (["ice", "wave"], beams.IceWave),
            ];
            foreach ((string[] kinds, EnemyWeaponVulnerability beam) in combined)
            {
                bool ice = kinds.Contains("ice"), plasma = family.Contains("plasma");
                hits.Add((["beam", .. family, .. kinds], shotsHarm ? OfBeam(beam, beam.Multiplier, ice, plasma) : spared));
                // A charged shot takes the charged beam's damage, and still freezes as the beam it is does.
                hits.Add((["beam", "charged", .. family, .. kinds], shotsHarm ? OfBeam(beam, table.ChargedBeam.Multiplier, ice, plasma) : spared));
            }
        }

        Beams([], table.Power);
        Beams(["spazer"], table.Spazer);
        Beams(["plasma"], table.Plasma);
        hits.Add((["missile"], shotsHarm ? new Outcome(Halves(table.Ordnance.Missile.Multiplier)) : spared));
        hits.Add((["super_missile"], shotsHarm ? new Outcome(Halves(table.Ordnance.SuperMissile.Multiplier)) : spared));
        hits.Add((["bomb"], shotsHarm ? new Outcome(Halves(table.Ordnance.Bomb.Multiplier)) : spared));
        int powerBomb = powerBombHarms ? Halves(table.Ordnance.PowerBomb.Multiplier) : 0;
        hits.Add((["power_bomb"], powerBombHarms
            ? new Outcome(powerBomb, Unhittable: powerBomb > 0 ? EnemyCollisionDefinitions.PowerBombInvincibilityUpdates : 0)
            : Passed));
        hits.Add((["speed_boost"], touchHarms ? new Outcome(Halves(table.Contact.SpeedBooster.Multiplier)) : Passed));
        hits.Add((["shinespark"], touchHarms ? new Outcome(Halves(table.Contact.Shinespark.Multiplier)) : Passed));
        hits.Add((["screw_attack"], touchHarms ? new Outcome(Halves(table.Contact.ScrewAttack.Multiplier)) : Passed));
        hits.Add((["charged_spin"], touchHarms ? new Outcome(Halves(table.Contact.PseudoScrew.Multiplier)) : Passed));

        // The grapple beam destroys some enemies outright, goes on past those that have no reaction to it as if they
        // were not there, and does nothing to the rest but stop on them.
        hits.Add((["grapple"], definition.GrappleResponse.BeamReaction switch
        {
            GrappleEnemyReaction.Kill => new Outcome(0, Kills: true),
            GrappleEnemyReaction.None => Passed,
            _ => Nothing,
        }));

        var reactions = hits.Select(hit => (Kinds: hit.Kinds.ToList(), hit.Outcome)).ToList();
        bool AnswersAll() => hits.All(hit => Answer(reactions, hit.Kinds) == hit.Outcome);
        if (!AnswersAll())
            throw new InvalidDataException("A reaction for every hit does not answer every hit as the old table does.");

        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int index = reactions.Count - 1; index >= 0; index--)
            {
                (List<string> Kinds, Outcome Outcome) taken = reactions[index];
                reactions.RemoveAt(index);
                if (AnswersAll())
                {
                    changed = true;
                    continue;
                }

                reactions.Insert(index, taken);
                foreach (string kind in taken.Kinds.ToArray())
                {
                    // A reaction always names a kind: one naming none would answer hits no table speaks of.
                    if (taken.Kinds.Count == 1)
                        break;
                    taken.Kinds.Remove(kind);
                    if (reactions.Count(other => other.Kinds.ToHashSet().SetEquals(taken.Kinds)) == 1 && AnswersAll())
                        changed = true;
                    else
                        taken.Kinds.Add(kind);
                }
            }
        }

        var written = new JsonArray();
        foreach ((List<string> kinds, Outcome outcome) in reactions)
        {
            var kindsWritten = new JsonArray();
            foreach (string kind in kinds.Order(StringComparer.Ordinal))
                kindsWritten.Add(kind);
            var reaction = new JsonObject { ["kinds"] = kindsWritten, ["damage"] = Damage(outcome, definition) };
            if (outcome.Freezes || outcome.FreezesInsteadOfKilling)
            {
                reaction["status"] = "frozen";
                reaction["status_when"] = outcome.Freezes ? "always" : "instead_of_dying";
                reaction["status_updates"] = EnemyCollisionDefinitions.FreezeUpdates;
                reaction["status_unhittable_updates"] = EnemyCollisionDefinitions.FreezeInvincibilityUpdates;
            }

            if (outcome.Unhittable > 0)
                reaction["unhittable_updates"] = outcome.Unhittable;
            if (outcome.Passes)
                reaction["passes"] = true;
            written.Add(reaction);
        }

        return written;
    }

    /// <summary>
    /// What a beam does: one the table says freezes does only that, and any other does its damage, freezes in place of
    /// destroying when it is ice and the table does not forbid it, and leaves the enemy unhittable a while when it is plasma.
    /// </summary>
    private static Outcome OfBeam(EnemyWeaponVulnerability beam, WeaponDamageFactor damage, bool ice, bool plasma)
    {
        if (beam.FreezesWithoutDamage)
            return new Outcome(0, Freezes: true);
        int halves = Halves(damage);
        return halves == 0
            ? Nothing
            : new Outcome(halves, FreezesInsteadOfKilling: ice && !beam.PreventsLethalFreeze,
                Unhittable: plasma ? EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates : 0);
    }

    /// <summary>How many halves of a hit's damage a factor takes.</summary>
    private static int Halves(WeaponDamageFactor factor) => factor.DamageFrom(2);

    /// <summary>
    /// What the Godot game answers a hit of <paramref name="kinds"/> with, of <paramref name="reactions"/>: the first
    /// of those naming the most kinds among those whose kinds the hit all carries.
    /// </summary>
    private static Outcome Answer(List<(List<string> Kinds, Outcome Outcome)> reactions, string[] kinds)
    {
        (List<string> Kinds, Outcome Outcome)? best = null;
        foreach ((List<string> Kinds, Outcome Outcome) reaction in reactions)
        {
            if (!reaction.Kinds.All(kinds.Contains))
                continue;
            if (best is null || reaction.Kinds.Count > best.Value.Kinds.Count)
                best = reaction;
        }

        return best?.Outcome ?? Unanswered;
    }

    /// <summary>The damage of an outcome, as a reaction writes it.</summary>
    private static JsonNode Damage(Outcome outcome, RoomEnemyDefinition definition)
    {
        // The grapple's hit, taken as many times over as it takes to reach the enemy's health, destroys it.
        if (outcome.Kills)
            return JsonValue.Create(Math.Max(1, (definition.Health + GrappleHitDamage - 1) / GrappleHitDamage));
        return outcome.Halves switch
        {
            0 => "none",
            1 => "half",
            2 => "full",
            4 => "double",
            int halves when halves % 2 == 0 => JsonValue.Create(halves / 2),
            int halves => throw new InvalidDataException($"The old table takes {halves} halves of a hit's damage, which a reaction cannot say."),
        };
    }
}
