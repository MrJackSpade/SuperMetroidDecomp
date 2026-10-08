using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="CompiledEnemyVisualSelectors"/>; never linked by player hosts.</summary>
internal static class CompiledEnemyVisualSelectorsTooling
{
    internal static int CalculatedCount => BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount + BotwoonInstructionProgramDefinitions.PresentationWordCount + BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount + BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount + BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordCount + BeetomInstructionProgramDefinitionsTooling.PresentationWordCount + PlatformInstructionProgramDefinitionsTooling.PresentationWordCount + ElevatorInstructionProgramDefinitionsTooling.PresentationWordCount + GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordCount + HorizontalShutterInstructionProgramDefinitionsTooling.PresentationWordCount + (VerticalShutterInstructionProgramDefinitionsTooling.PresentationWordCount - 1) + AlcoonFireballInstructionProgramDefinitions.PresentationWordCount + FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount + BoyonInstructionProgramDefinitionsTooling.PresentationWordCount +
        BoulderInstructionProgramDefinitionsTooling.PresentationWordCount +
        FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount +
        KraidNailInstructionProgramDefinitions.PresentationWordCount + PhantoonInstructionProgramDefinitions.PresentationWordCount +
        FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount + AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount + AtomicInstructionProgramDefinitionsTooling.PresentationWordCount;
    internal static int Count => CompiledEnemyVisualSelectors.Entries.Length + CalculatedCount;
    internal static CompiledEnemyVisualSelector At(int index)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        // Calculated entries are ordered by native bank/address. Their merged rank
        // is their own ordinal plus the number of literal entries before them.
        int low = 0, high = CalculatedCount;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            CompiledEnemyVisualSelector candidate = CalculatedAt(middle);
            int literalLow = 0, literalHigh = CompiledEnemyVisualSelectors.Entries.Length;
            while (literalLow < literalHigh)
            {
                int literalMiddle = literalLow + (literalHigh - literalLow) / 2;
                if (CompiledEnemyVisualSelectors.Entries[literalMiddle].Address < candidate.Address) literalLow = literalMiddle + 1;
                else literalHigh = literalMiddle;
            }
            int rank = literalLow + middle;
            if (rank == index) return candidate;
            if (rank < index) low = middle + 1;
            else high = middle;
        }
        return CompiledEnemyVisualSelectors.Entries[index - low];
    }
    internal static CompiledEnemyVisualSelector CalculatedAt(int index)
    {
        if (index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operand));
        }
        index -= AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
        if (index < BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt(operand));
        }
        index -= BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;
        if (index < BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.BombTorizoStatueFrameAt(operand));
        }
        index -= BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount;
        if (index < FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operand));
        }
        index -= FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;
        if (index < BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BotwoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.BotwoonProjectileFrameAt(operand));
        }
        index -= BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount;
        if (index < BoyonInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = BoyonInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa20000 | operand, EnemySpritemapDefinitions.BoyonFrameAt(operand));
        }
        index -= BoyonInstructionProgramDefinitionsTooling.PresentationWordCount;
        int growingCount = GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < growingCount + HorizontalShutterInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = index < growingCount
                ? GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordAddress(index)
                : HorizontalShutterInstructionProgramDefinitionsTooling.PresentationWordAddress(index - growingCount);
            return new(0xa20000 | operand, ShutterVisualDefinitions.PointerAt(operand));
        }
        index -= growingCount + HorizontalShutterInstructionProgramDefinitionsTooling.PresentationWordCount;
        int kamerCount = VerticalShutterInstructionProgramDefinitionsTooling.PresentationWordCount - 1;
        if (index < kamerCount)
        {
            ushort operand = VerticalShutterInstructionProgramDefinitionsTooling.PresentationWordAddress(index + 1);
            return new(0xa20000 | operand, EnemySpritemapDefinitions.KamerPlatformFrameAt(operand));
        }
        index -= kamerCount;
        if (index < ElevatorInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = ElevatorInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa30000 | operand, EnemySpritemapDefinitions.ElevatorFrameAt(operand));
        }
        index -= ElevatorInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < PlatformInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = PlatformInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa30000 | operand, TripperKamerVisualDefinitions.FrameAt(operand));
        }
        index -= PlatformInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < BoulderInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = BoulderInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa60000 | operand, EnemySpritemapDefinitions.BoulderFrameAt(operand));
        }
        index -= BoulderInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = FakeKraidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa60000 | operand, KraidVisualDefinitions.FrameAt(RoomEnemySystem.FakeKraidDefinition, operand));
        }
        index -= FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < KraidNailInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa70000 | operand, KraidVisualDefinitions.FrameAt(RoomEnemySystem.KraidGoodNailDefinition, operand));
        }
        index -= KraidNailInstructionProgramDefinitions.PresentationWordCount;
        if (index < PhantoonInstructionProgramDefinitions.PresentationWordCount)
            return new(0xa70000 | PhantoonInstructionProgramDefinitions.PresentationWordAddress(index), PhantoonInstructionProgramDefinitions.PresentationFrame(index));
        index -= PhantoonInstructionProgramDefinitions.PresentationWordCount;
        if (index < FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.FuneNamiheFrameAt(operand));
        }
        index -= FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < BeetomInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = BeetomInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.BeetomFrameAt(operand));
        }
        index -= BeetomInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = AlcoonInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AlcoonFrameAt(operand));
        }
        index -= AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < AtomicInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = AtomicInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AtomicFrameAt(operand));
        }
        index -= AtomicInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, BlueBrinstarFaceBlockVisualDefinitions.FrameAt(operand));
        }
        index -= BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordCount;
        ushort botwoonOperand = BotwoonInstructionProgramDefinitions.PresentationWordAddress(index);
        return new(0xb30000 | botwoonOperand, BotwoonVisualDefinitions.FrameAt(botwoonOperand));
    }
}
