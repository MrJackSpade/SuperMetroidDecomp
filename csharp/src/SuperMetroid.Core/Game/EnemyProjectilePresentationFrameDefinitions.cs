namespace SuperMetroid.Core.Game;

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
    private enum CatalogGeneration
    {
        PreGoldenTorizo,
        PreGoldenTorizoEgg,
        PreTorizoEffects,
        PreGenericEnemyDeath,
        PreEnvironmentAndAttack,
        PreMotherBrainAndStatue,
        PreWorkRobot,
        PrePolypRock,
        Current,
    }

    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGoldenTorizoFrames =
        Build(CatalogGeneration.PreGoldenTorizo);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGoldenTorizoEggFrames =
        Build(CatalogGeneration.PreGoldenTorizoEgg);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreTorizoEffectsFrames =
        Build(CatalogGeneration.PreTorizoEffects);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGenericEnemyDeathFrames =
        Build(CatalogGeneration.PreGenericEnemyDeath);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreEnvironmentAndAttackFrames =
        Build(CatalogGeneration.PreEnvironmentAndAttack);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreMotherBrainAndStatueFrames =
        Build(CatalogGeneration.PreMotherBrainAndStatue);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreWorkRobotFrames =
        Build(CatalogGeneration.PreWorkRobot);
    private static readonly EnemyProjectilePresentationFrameDefinition[] Frames =
        Build(CatalogGeneration.Current);
    private static readonly EnemyProjectilePresentationFrameDefinition[] PrePolypRockFrames =
        Build(CatalogGeneration.PrePolypRock);

    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> All => Frames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PrePolypRock =>
        PrePolypRockFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGoldenTorizo =>
        PreGoldenTorizoFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGoldenTorizoEgg =>
        PreGoldenTorizoEggFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreTorizoEffects =>
        PreTorizoEffectsFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGenericEnemyDeath =>
        PreGenericEnemyDeathFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreEnvironmentAndAttack =>
        PreEnvironmentAndAttackFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreMotherBrainAndStatue =>
        PreMotherBrainAndStatueFrames;
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreWorkRobot =>
        PreWorkRobotFrames;

    internal static bool Contains(ushort operandAddress) =>
        Array.BinarySearch(Frames,
            new EnemyProjectilePresentationFrameDefinition(operandAddress, string.Empty),
            AddressComparer.Instance) >= 0;

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

        Add(frames, "ceres_ridley", CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordCount,
            CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "cacatac_spike", CacatacProjectileInstructionProgramDefinitions.PresentationWordCount,
            CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "botwoon", BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount,
            BotwoonProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "crocomire", CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount,
            CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "draygon", DraygonProjectileInstructionProgramDefinitions.PresentationWordCount,
            DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "downward_gate", DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount,
            DownwardGateProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "eye_door", EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount,
            EyeDoorProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "fake_kraid", FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
            FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "kago_bug", KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount,
            KagoBugProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "kraid_rock", KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount,
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "noob_tube", NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount,
            NoobTubeProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "nuclear_waffle", NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount,
            NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "phantoon", PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount,
            PhantoonProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "shaktool", ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount,
            ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "space_pirate", SpacePirateProjectileInstructionProgramDefinitions.PresentationWordCount,
            SpacePirateProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "spore_spawn", SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount,
            SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "stoke", StokeProjectileInstructionProgramDefinitions.PresentationWordCount,
            StokeProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "tourian_statue", TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount,
            TourianStatueProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "yapping_maw", YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount,
            YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress);
        Add(frames, "alcoon_fireball", AlcoonFireballInstructionProgramDefinitions.PresentationWordCount,
            AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress);
        if (generation >= CatalogGeneration.PreGoldenTorizoEgg)
        {
            Add(frames, "golden_torizo_super_missile",
                GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount,
                GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "golden_torizo_eye_beam",
                GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount,
                GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress);
        }
        if (generation >= CatalogGeneration.PreTorizoEffects)
        {
            Add(frames, "golden_torizo_egg",
                GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount,
                GoldenTorizoEggInstructionProgramDefinitions.PresentationWordAddress);
        }
        if (generation >= CatalogGeneration.PreGenericEnemyDeath)
        {
            Add(frames, "torizo_drool",
                BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount,
                BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "torizo_explosive_swipe",
                TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount,
                TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "torizo_sonic_boom",
                TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount,
                TorizoSonicBoomInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "torizo_landing_dust",
                TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount,
                TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "torizo_explosion",
                TorizoExplosionInstructionProgramDefinitions.PresentationWordCount,
                TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "torizo_chozo_orb",
                TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount,
                TorizoChozoOrbInstructionProgramDefinitions.PresentationWordAddress);
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
            Add(frames, "ceres_falling_debris",
                CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount,
                CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "save_station_electricity",
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount,
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "gunship_dust",
                GunshipDustInstructionProgramDefinitions.PresentationWordCount,
                GunshipDustInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "falling_spark",
                FallingSparkInstructionProgramDefinitions.PresentationWordCount,
                FallingSparkInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "magdollite_lava",
                MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount,
                MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "chozo_tourian_dust",
                ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount,
                ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "eye_door_sweat",
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "ki_hunter_acid_spit",
                KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount,
                KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "fune_namihe_fireball",
                FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount,
                FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "dragon_fireball",
                DragonFireballInstructionProgramDefinitions.PresentationWordCount,
                DragonFireballInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "powamp_spike",
                PowampSpikeInstructionProgramDefinitions.PresentationWordCount,
                PowampSpikeInstructionProgramDefinitions.PresentationWordAddress);
        }
        if (generation >= CatalogGeneration.PreWorkRobot)
        {
            Add(frames, "bomb_torizo_statue_fragment",
                BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount,
                BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "mother_brain_glass",
                MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "mother_brain_hand_beam",
                MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "mother_brain_top_tube",
                MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress);
            Add(frames, "mother_brain_turret",
                MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress);
        }
        if (generation >= CatalogGeneration.PrePolypRock)
        {
            Add(frames, "work_robot_laser",
                WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount,
                WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress);
        }
        if (generation >= CatalogGeneration.Current)
            frames.Add(PolypRockInstructionProgramDefinitions.PresentationWord, "polyp_rock_00");

        return frames.OrderBy(entry => entry.Key)
            .Select(entry => new EnemyProjectilePresentationFrameDefinition(entry.Key, entry.Value))
            .ToArray();
    }

    private sealed class AddressComparer : IComparer<EnemyProjectilePresentationFrameDefinition>
    {
        internal static readonly AddressComparer Instance = new();
        public int Compare(EnemyProjectilePresentationFrameDefinition left,
            EnemyProjectilePresentationFrameDefinition right) =>
            left.OperandAddress.CompareTo(right.OperandAddress);
    }
}
