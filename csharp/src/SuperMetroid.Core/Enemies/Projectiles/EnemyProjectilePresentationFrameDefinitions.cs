using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.FallingSparks.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Presentation-only bank-$86 spritemap operands already identified by translated
/// enemy-projectile programs. Their bank-$8D targets and OAM parts are extracted as
/// editable artwork; no timing, collision, callback, or motion value lives here.
/// </summary>
internal static class EnemyProjectilePresentationFrameDefinitions
{
    // Each generation is a complete historical set of named visual operands. Their
    // order matches the published artwork schema, so an older override can be merged
    // by operand identity without assuming that the sorted current catalog is a prefix.
    /// <summary>Published artwork schema boundaries used to merge older projectile overrides by operand identity.</summary>
    private enum CatalogGeneration
    {
        /// <summary>Base projectile families before Golden Torizo's super missile and eye beam.</summary>
        PreGoldenTorizo,
        /// <summary>Adds Golden Torizo's super missile and eye beam frames.</summary>
        PreGoldenTorizoEgg,
        /// <summary>Adds Golden Torizo egg frames.</summary>
        PreTorizoEffects,
        /// <summary>Adds Torizo drool, swipe, boom, dust, explosion, and orb effects.</summary>
        PreGenericEnemyDeath,
        /// <summary>Adds enemy pickup and generic death frames.</summary>
        PreEnvironmentAndAttack,
        /// <summary>Adds environmental and common attack effects.</summary>
        PreMotherBrainAndStatue,
        /// <summary>Adds Mother Brain and Bomb Torizo statue projectile frames.</summary>
        PreWorkRobot,
        /// <summary>Adds Work Robot laser frames.</summary>
        PrePolypRock,
        /// <summary>Adds the Polyp Rock visual operand.</summary>
        Current,
    }

    /// <summary>Base schema frame set retained for pre-Golden-Torizo overrides.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGoldenTorizoFrames =
        Build(CatalogGeneration.PreGoldenTorizo);
    /// <summary>Historical frame set including Golden Torizo's super missile and eye beam.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGoldenTorizoEggFrames =
        Build(CatalogGeneration.PreGoldenTorizoEgg);
    /// <summary>Historical frame set including Golden Torizo egg artwork.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreTorizoEffectsFrames =
        Build(CatalogGeneration.PreTorizoEffects);
    /// <summary>Historical frame set including the Torizo effect family.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGenericEnemyDeathFrames =
        Build(CatalogGeneration.PreGenericEnemyDeath);
    /// <summary>Historical frame set including pickups and generic enemy deaths.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreEnvironmentAndAttackFrames =
        Build(CatalogGeneration.PreEnvironmentAndAttack);
    /// <summary>Historical frame set including environmental and attack effects.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreMotherBrainAndStatueFrames =
        Build(CatalogGeneration.PreMotherBrainAndStatue);
    /// <summary>Historical frame set including Mother Brain and statue projectiles.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreWorkRobotFrames =
        Build(CatalogGeneration.PreWorkRobot);
    /// <summary>Current sorted visual-operand set used by production catalog lookup.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] Frames =
        Build(CatalogGeneration.Current);
    /// <summary>Historical frame set including Work Robot laser and excluding Polyp Rock.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PrePolypRockFrames =
        Build(CatalogGeneration.PrePolypRock);

    /// <summary>Current projectile visual operands sorted by bank-$86 address.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> All => Frames;
    /// <summary>Schema set before the Polyp Rock projectile frame was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PrePolypRock =>
        PrePolypRockFrames;
    /// <summary>Base schema set before Golden Torizo projectile art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGoldenTorizo =>
        PreGoldenTorizoFrames;
    /// <summary>Schema set before Golden Torizo egg art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGoldenTorizoEgg =>
        PreGoldenTorizoEggFrames;
    /// <summary>Schema set before the broader Torizo effect family was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreTorizoEffects =>
        PreTorizoEffectsFrames;
    /// <summary>Schema set before generic enemy pickup and death art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGenericEnemyDeath =>
        PreGenericEnemyDeathFrames;
    /// <summary>Schema set before environmental and attack effect art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreEnvironmentAndAttack =>
        PreEnvironmentAndAttackFrames;
    /// <summary>Schema set before Mother Brain and statue projectile art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreMotherBrainAndStatue =>
        PreMotherBrainAndStatueFrames;
    /// <summary>Schema set before Work Robot laser art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreWorkRobot =>
        PreWorkRobotFrames;

    /// <summary>Checks whether a bank-$86 visual operand belongs to the current published artwork catalog.</summary>
    internal static bool Contains(ushort operandAddress) =>
        Array.BinarySearch(Frames,
            new EnemyProjectilePresentationFrameDefinition(operandAddress, string.Empty),
            AddressComparer.Instance) >= 0;

    /// <summary>Collects visual operands through a selected schema generation, retaining the first name for shared physical frames.</summary>
    private static EnemyProjectilePresentationFrameDefinition[] Build(
        CatalogGeneration generation)
    {
        var frames = new Dictionary<ushort, string>();
        foreach (EnemyProjectilePresentationFrameDefinition frame in
                 EnemyProjectileInstructionMechanicsDefinitions.VisualFrames)
            frames.Add(frame.OperandAddress, frame.Name);

        static void Add(Dictionary<ushort, string> target, string family, int count,
            Func<int, ushort> addressAt)
        {
            for (int index = 0; index < count; index++)
            {
                ushort address = addressAt(index);
                // Different producers can reuse one physical bank-$86 frame. Its first
                // family name is the editable identity; later owners share that artwork.
                target.TryAdd(address, $"{family}_frame_{index:D2}");
            }
        }

        // Torizo's families install their typed frames in each family's native operand order.
        static void AddTorizo(Dictionary<ushort, string> target, string family, TorizoProjectileFamily torizoFamily)
        {
            IReadOnlyList<TorizoProjectileFrame> torizoFrames = TorizoProjectileVisualDefinitions.Frames(torizoFamily);
            Add(target, family, torizoFrames.Count, index => TorizoProjectileVisualDefinitions.PresentationKey(torizoFrames[index]));
        }

        Add(frames, "ceres_ridley", RidleyFireballVisualDefinitions.ProjectileFrameCount,
            index => RidleyFireballVisualDefinitions.PresentationKey(RidleyFireballVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "old_tourian_escape_wall_explosion", OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordCount,
            OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordAddress);
        Add(frames, "cacatac_spike", CacatacProjectileInstructionProgramDefinitions.PresentationWordCount,
            CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "botwoon", BotwoonVisualDefinitions.ProjectileFrameCount,
            index => BotwoonVisualDefinitions.ProjectilePresentationKey(BotwoonVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "crocomire", CrocomireProjectileVisualDefinitions.ProjectileFrameCount,
            index => CrocomireProjectileVisualDefinitions.PresentationKey(CrocomireProjectileVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "draygon", DraygonProjectileVisualDefinitions.ProjectileFrameCount,
            index => DraygonProjectileVisualDefinitions.PresentationKey(DraygonProjectileVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "downward_gate", DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount,
            DownwardGateProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "eye_door", EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount,
            EyeDoorProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "fake_kraid", FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
            FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "kago_bug", KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount,
            KagoBugProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "kraid_rock", KraidRockVisualDefinitions.ProjectileFrameCount,
            index => KraidRockVisualDefinitions.PresentationKey(KraidRockVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "noob_tube", NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount,
            NoobTubeProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "nuclear_waffle", NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount,
            NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "phantoon", PhantoonFlameVisualDefinitions.ProjectileFrameCount,
            index => PhantoonFlameVisualDefinitions.PresentationKey(PhantoonFlameVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "shaktool", ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount,
            ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "space_pirate", SpacePirateProjectileInstructionProgramDefinitions.PresentationWordCount,
            SpacePirateProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "spore_spawn", SporeSpawnProjectileVisualDefinitions.ProjectileFrameCount,
            index => SporeSpawnProjectileVisualDefinitions.PresentationKey(SporeSpawnProjectileVisualDefinitions.ProjectileFrames[index]));
        Add(frames, "stoke", StokeProjectileInstructionProgramDefinitions.PresentationWordCount,
            StokeProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "tourian_statue", TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount,
            TourianStatueProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "yapping_maw", YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount,
            YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "alcoon_fireball", AlcoonFireballFrameDefinitions.PoseCount,
            index => AlcoonFireballFrameDefinitions.PresentationKey(new(index)));
        if (generation >= CatalogGeneration.PreGoldenTorizoEgg)
        {
            AddTorizo(frames, "golden_torizo_super_missile", TorizoProjectileFamily.SuperMissile);
            AddTorizo(frames, "golden_torizo_eye_beam", TorizoProjectileFamily.EyeBeam);
        }
        if (generation >= CatalogGeneration.PreTorizoEffects)
        {
            AddTorizo(frames, "golden_torizo_egg", TorizoProjectileFamily.Egg);
        }
        if (generation >= CatalogGeneration.PreGenericEnemyDeath)
        {
            AddTorizo(frames, "torizo_drool", TorizoProjectileFamily.Drool);
            AddTorizo(frames, "torizo_explosive_swipe", TorizoProjectileFamily.Swipe);
            AddTorizo(frames, "torizo_sonic_boom", TorizoProjectileFamily.SonicBoom);
            AddTorizo(frames, "torizo_landing_dust", TorizoProjectileFamily.LandingDust);
            AddTorizo(frames, "torizo_explosion", TorizoProjectileFamily.Explosion);
            AddTorizo(frames, "torizo_chozo_orb", TorizoProjectileFamily.ChozoOrb);
        }
        if (generation >= CatalogGeneration.PreEnvironmentAndAttack)
        {
            Add(frames, "enemy_pickup",
                EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
                EnemyPickupInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "enemy_death",
                EnemyDeathInstructionProgramDefinitions.PresentationWordCount,
                EnemyDeathInstructionProgramDefinitions.PresentationWordAddress);
        }
        if (generation >= CatalogGeneration.PreMotherBrainAndStatue)
        {
            Add(frames, "ceres_falling_debris", CeresFallingDebrisVisualDefinitions.Frames.Count,
                index => CeresFallingDebrisVisualDefinitions.PresentationKey(CeresFallingDebrisVisualDefinitions.Frames[index]));
            Add(frames, "save_station_electricity",
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount,
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "gunship_dust", GunshipDustVisualDefinitions.Frames.Count,
                index => GunshipDustVisualDefinitions.PresentationKey(GunshipDustVisualDefinitions.Frames[index]));
            Add(frames, "falling_spark",
                FallingSparkFrameDefinitions.PoseCount,
                index => FallingSparkFrameDefinitions.PresentationKey(new(index)));
            Add(frames, "magdollite_lava",
                MagdolliteLavaFrameDefinitions.PoseCount,
                index => MagdolliteLavaFrameDefinitions.PresentationKey(new(index)));
            Add(frames, "chozo_tourian_dust",
                ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount,
                ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "eye_door_sweat",
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "ki_hunter_acid_spit",
                KiHunterAcidSpitFrameDefinitions.PoseCount,
                index => KiHunterAcidSpitFrameDefinitions.PresentationKey(new(index)));
            Add(frames, "fune_namihe_fireball",
                FuneNamiheFireballFrameDefinitions.PoseCount,
                index => FuneNamiheFireballFrameDefinitions.PresentationKey(new(index)));
            Add(frames, "dragon_fireball",
                DragonFireballFrameDefinitions.PoseCount,
                index => DragonFireballFrameDefinitions.PresentationKey(new(index)));
            Add(frames, "powamp_spike",
                PowampSpikeFrameDefinitions.PoseCount,
                index => PowampSpikeFrameDefinitions.PresentationKey(new(index)));
        }
        if (generation >= CatalogGeneration.PreWorkRobot)
        {
            AddTorizo(frames, "bomb_torizo_statue_fragment", TorizoProjectileFamily.StatueFragment);
            Add(frames, "mother_brain_glass", MotherBrainGlassVisualDefinitions.ProjectileFrameCount,
                index => MotherBrainGlassVisualDefinitions.PresentationKey(MotherBrainGlassVisualDefinitions.ProjectileFrames[index]));
            Add(frames, "mother_brain_hand_beam", MotherBrainHandBeamVisualDefinitions.ProjectileFrameCount,
                index => MotherBrainHandBeamVisualDefinitions.PresentationKey(MotherBrainHandBeamVisualDefinitions.ProjectileFrames[index]));
            Add(frames, "mother_brain_top_tube",
                MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "mother_brain_turret",
                MotherBrainTurretVisualDefinitions.ProjectileFrameCount,
                index => MotherBrainTurretVisualDefinitions.PresentationKey(MotherBrainTurretVisualDefinitions.ProjectileFrames[index]));
        }
        if (generation >= CatalogGeneration.PrePolypRock)
        {
            Add(frames, "work_robot_laser",
                WorkRobotLaserFrameDefinitions.PoseCount,
                index => WorkRobotLaserFrameDefinitions.PresentationKey(new(index)));
        }
        if (generation >= CatalogGeneration.Current)
            frames.Add(PolypRockFrameDefinitions.PresentationKey(new(0)), "polyp_rock_00");

        return frames.OrderBy(entry => entry.Key)
            .Select(entry => new EnemyProjectilePresentationFrameDefinition(entry.Key, entry.Value))
            .ToArray();
    }

    /// <summary>Orders frame definitions by physical bank-$86 operand address for binary search.</summary>
    private sealed class AddressComparer : IComparer<EnemyProjectilePresentationFrameDefinition>
    {
        /// <summary>Shared stateless address comparer for current-catalog lookups.</summary>
        internal static readonly AddressComparer Instance = new();
        /// <summary>Compares only operand addresses; family names do not affect lookup identity.</summary>
        public int Compare(EnemyProjectilePresentationFrameDefinition left,
            EnemyProjectilePresentationFrameDefinition right) =>
            left.OperandAddress.CompareTo(right.OperandAddress);
    }
}
