namespace SuperMetroid.Core.Game;

/// <summary>
/// Presentation-only bank-$86 spritemap operands already identified by translated
/// enemy-projectile programs. Their bank-$8D targets and OAM parts are extracted as
/// editable artwork; no timing, collision, callback, or motion value lives here.
/// </summary>
internal static class EnemyProjectilePresentationFrameDefinitions
{
    private static readonly EnemyProjectilePresentationFrameDefinition[] Frames = Build();

    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> All => Frames;

    internal static bool Contains(ushort operandAddress) =>
        Array.BinarySearch(Frames,
            new EnemyProjectilePresentationFrameDefinition(operandAddress, string.Empty),
            AddressComparer.Instance) >= 0;

    private static EnemyProjectilePresentationFrameDefinition[] Build()
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
