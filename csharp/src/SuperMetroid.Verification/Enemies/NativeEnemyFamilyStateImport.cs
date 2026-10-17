using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Translates a native snapshot's enemy variable words into the typed state each enemy family owns. The generic
/// snapshot import writes every slot's words first; this codec then gives each family its state from them, rejecting
/// a word outside the range the family's own updates can produce.
/// </summary>
internal static class NativeEnemyFamilyStateImport
{
    /// <summary>Translates the recorded variable words of the enemy in <paramref name="slot"/> into its family's state.</summary>
    /// <param name="enemies">The enemy system whose family states receive the values.</param>
    /// <param name="slot">The imported slot, its variable and parameter words already written.</param>
    internal static void Import(RoomEnemySystem enemies, RoomEnemySlot slot)
    {
        int index = slot.SlotIndex;
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingHeight = new Position16<RoomPixels>(0, slot.VariableA);
            platform.RightSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableB);
            platform.LeftSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableE), slot.VariableD);
            platform.YSpeedTableIndex = Index(slot.VariableF, platform.MaximumYSpeedTableIndex, "platform acceleration index");
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedTableIndex = Index(slot.VariableB, short.MaxValue, "Beetom speed index");
            beetom.Function = slot.VariableC == 0 ? null : (BeetomEnemyFunction)slot.VariableC;
            beetom.FunctionTimer = NativeCountdown.FromRaw(slot.VariableD);
            beetom.ButtonCounter = NativeCountdown.FromRaw(slot.VariableE);
            beetom.PreviousController1Input = (SnesButton)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool && enemies.ShaktoolSegments[index] is { } segment)
        {
            segment.TargetAngle = SnesAngle.FromRaw(slot.VariableA);
            segment.OrbitAngle = SnesAngle.FromRaw(slot.VariableB);
            segment.AngularVelocity = Index(slot.VariableC, 1020, "Shaktool angular velocity");
            ImportShaktoolOrientation(segment, slot.Parameter2 >> 1, slot.VariableD);
            segment.OwnerSlotIndex = SlotIndex(slot.VariableE, "Shaktool owner");
            segment.PreInstruction = slot.VariableF == 0 ? null : (ShaktoolPreInstruction)slot.VariableF;
        }
        if (enemies.ChootStates[index] is { } choot)
        {
            choot.Function = slot.VariableA == 0 ? null : (ChootEnemyFunction)slot.VariableA;
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedTableIndex = unchecked((short)slot.VariableB) >> 8;
            choot.FallingPatternFrame = (slot.VariableC & 0xff) == 0
                ? slot.VariableC >> 8
                : throw new InvalidDataException($"Recorded Choot falling cursor ${slot.VariableC:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = NativeCountdown.FromRaw(slot.VariableD);
            choot.FallingPath = slot.VariableE == 0 ? null : ClosedNativeWords.Decode<ChootFallingPath>(slot.VariableE, "recorded Choot falling pattern");
            choot.FallingPatternYDistance = slot.VariableF;
        }
        if (enemies.StokeStates[index] is { } stoke)
        {
            stoke.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableB), slot.VariableA);
            stoke.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableD), slot.VariableC);
            stoke.Direction = (StokeDirection)slot.VariableE;
            stoke.Function = slot.VariableF == 0 ? null : (StokeAiFunction)slot.VariableF;
        }
        if (enemies.SkulteraStates[index] is { } skultera)
        {
            skultera.Function = slot.VariableA == 0 ? null : (SkulteraEnemyFunction)slot.VariableA;
            skultera.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableB);
            skultera.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableE), slot.VariableD);
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(slot.VariableF));
        }
        if (slot.EntityState<MultiviolaEntity, MultiviolaEnemyState>() is { } multiviola)
            multiviola.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableC, slot.VariableD, slot.VariableE, slot.VariableF);
        if (slot.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SnesSignedEightEight(slot.VariableA), new SnesSignedEightEight(slot.VariableB));
            crawler.Function = slot.VariableF == 0 ? null : (CrawlerEnemyFunction)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz && slot.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SnesSignedEightEight(slot.VariableD), new SnesSignedEightEight(slot.VariableC));
            holtz.Function = slot.VariableF == 0 ? null : (LowerNorfairRioEnemyFunction)slot.VariableF;
            holtz.IsFollower = (slot.Parameter1 & EnemyParameterMasks.FollowerBit) != 0;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape && slot.SpeciesState<EscapeEtecoonEnemyState>() is { } escape)
        {
            escape.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(slot.VariableA), default);
            escape.PreInstruction = slot.VariableF == 0 ? null : (EscapeEtecoonPreInstruction)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock && slot.SpeciesState<BlueBrinstarFaceBlockEnemyState>() is { } faceBlock)
        {
            faceBlock.Activated = slot.VariableA != 0;
            faceBlock.LastHorizontalDelta = slot.VariableB;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Puromi && slot.SpeciesState<NuclearWaffleEnemyState>() is { } waffle)
        {
            waffle.Function = slot.VariableA == 0 ? null : (NuclearWaffleEnemyFunction)slot.VariableA;
            waffle.WaitingTimer = NativeCountdown.FromRaw(slot.VariableB);
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks && slot.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Function = slot.VariableA == 0 ? null : (PolypEnemyFunction)slot.VariableA;
            polyp.CooldownTimer = NativeCountdown.FromRaw(slot.VariableB);
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp && slot.SpeciesState<PowampEnemyState>() is { } powamp)
        {
            // The halves reuse the same words for different values.
            if (powamp.IsBalloon)
            {
                powamp.BalloonSpawn = new(slot.VariableA, slot.VariableB);
                powamp.BalloonGrappleTravelDistance = slot.VariableD;
            }
            else
            {
                powamp.YVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableA), slot.VariableB);
                powamp.WiggleIndex = Index(slot.VariableC, 11, "Powamp wiggle phase");
                powamp.WiggleTimer = NativeCountdown.FromRaw(slot.VariableD);
                powamp.FunctionTimer = NativeCountdown.FromRaw(slot.VariableE);
            }
            powamp.Function = slot.VariableF == 0 ? null : (PowampEnemyFunction)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle && slot.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            mama.Function = slot.VariableA == 0 ? null : (MamaTurtleAiFunction)slot.VariableA;
            // Only the horizontal velocity's whole pixels live in the record; its fraction and the vertical velocity do not.
            mama.Velocity = mama.Velocity.WithX(mama.Velocity.X.WithWholePixels(unchecked((short)slot.VariableE)));
            mama.AsleepFlag = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (enemies.HibashiStates[index] is { } hibashi)
        {
            hibashi.Function = slot.VariableA == 0 ? null : (HibashiEnemyFunction)slot.VariableA;
            hibashi.InactiveTimer = NativeCountdown.FromRaw(slot.VariableB);
            hibashi.ActivityFinished = slot.VariableC != 0;
            hibashi.SpawnHeight = new Position16<RoomPixels>(0, slot.VariableD);
        }
        if (enemies.AtomicStates[index] is { } atomic)
        {
            atomic.VerticalMovement = slot.VariableA == 0 ? null : (AtomicVerticalMovement)slot.VariableA;
            atomic.HorizontalMovement = slot.VariableB == 0 ? null : (AtomicHorizontalMovement)slot.VariableB;
        }
        if (enemies.AlcoonStates[index] is { } alcoon)
        {
            alcoon.Function = slot.VariableA == 0 ? null : (AlcoonEnemyFunction)slot.VariableA;
            alcoon.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableD, 0, slot.VariableB, slot.VariableC);
            alcoon.SpawnPosition = alcoon.SpawnPosition.WithY(slot.VariableF);
        }
        if (enemies.DeadTourianCorpses[index] is { } corpse)
        {
            corpse.Function = DeadTourianCorpseFunctionOf(corpse.Species, slot.VariableA);
            corpse.PreRotDelayCounter = slot.VariableB;
        }
        if (enemies.RioStates[index] is { } rio)
        {
            rio.SavedHorizontalVelocity = new SnesSignedEightEight(slot.VariableA);
            rio.Function = slot.VariableB == 0 ? null : (RioEnemyFunction)slot.VariableB;
            rio.Velocity = new(new SnesSignedEightEight(slot.VariableD), new SnesSignedEightEight(slot.VariableC));
            rio.AnimationFinished = slot.VariableE != 0;
        }
        if (slot.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is { } norfairRio)
        {
            norfairRio.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableA));
            norfairRio.Function = slot.VariableF == 0 ? null : (NorfairRioEnemyFunction)slot.VariableF;
        }
        if (enemies.WaverStates[index] is { } waver)
        {
            waver.XVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableB), slot.VariableA);
            waver.CurrentInstructionListIndex = (WaverAnimationSelector)slot.VariableC;
            // Only the phase word's low byte is ever read.
            waver.Angle = SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(slot.VariableD));
            waver.SpinFinished = slot.VariableE != 0;
            waver.RequestedInstructionListIndex = (WaverAnimationSelector)slot.VariableF;
        }
        if (slot.EntityState<FliesEntity, FlyEnemyState>() is { } fly)
        {
            fly.RetreatTimer = NativeCountdown.FromRaw(slot.VariableA);
            fly.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableC));
            fly.TargetHeight = new Position16<RoomPixels>(0, slot.VariableD);
            // The phase word is an even byte offset into the 256-entry table, nine bits wide.
            fly.Angle = slot.VariableE % 2 == 0
                ? SnesAngle.FromTableIndex(checked((byte)(Index(slot.VariableE, 510, "fly phase") / 2)))
                : throw new InvalidDataException($"Recorded fly phase ${slot.VariableE:X4} is odd.");
            fly.Function = slot.VariableF == 0 ? null : (FlyEnemyFunction)slot.VariableF;
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.FacingLeft = (slot.VariableA & 0x8000) != 0;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.FunctionTimer = NativeCountdown.FromRaw(slot.VariableD);
            dragon.AttackCounter = NativeCountdown.FromRaw(slot.VariableD);
            dragon.Function = slot.VariableF == 0 ? null : (DragonEnemyFunction)slot.VariableF;
        }
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingHeight = new Position16<RoomPixels>(0, slot.VariableA);
        if (enemies.FuneNamiheStates[index] is { } funeNamihe)
        {
            funeNamihe.Function = slot.VariableB == 0 ? null : (FuneNamiheEnemyFunction)slot.VariableB;
            funeNamihe.YProximity = Index(slot.VariableC, byte.MaxValue, "Namihe wake distance");
            funeNamihe.VariantIndex = Index(slot.VariableD, 15, "Fune/Namihe variant");
            funeNamihe.CooldownTimer = slot.VariableE;
            funeNamihe.CooldownTime = Index(slot.VariableF, byte.MaxValue, "Fune cooldown time");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(slot.VariableA);
            if (namihe != funeNamihe.IsNamihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${slot.VariableA:X4} names the other species.");
            funeNamihe.ShowsActiveAnimation = active;
            funeNamihe.FacingRight = right;
        }
        if (slot.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
            slot.SpeciesState<RipperVariantEnemyState>() is { } ripperVariant)
        {
            ripperVariant.XVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableD), slot.VariableC);
            ripperVariant.SpeedTableByteOffset = slot.VariableE;
        }
        if (enemies.NorfairLavaJumpingEnemyStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SnesSignedEightEight(slot.VariableC);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(slot.VariableD, slot.VariableE);
            lavaJumper.Function = slot.VariableF == 0 ? null : (NorfairLavaJumpingEnemyFunction)slot.VariableF;
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = NativeCountdown.FromRaw(slot.VariableA);
            metaree.Function = slot.VariableB == 0 ? null : (MetareeEnemyFunction)slot.VariableB;
            metaree.RequestedInstructionListIndex = (SkreeMetareeAnimationPhase)slot.VariableC;
            metaree.InstalledInstructionListIndex = (SkreeMetareeAnimationPhase)slot.VariableD;
            metaree.AttackReady = slot.VariableE != 0;
            metaree.YVelocity = Index(slot.VariableF, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Function = slot.VariableB == 0 ? null : (SparkEnemyFunction)slot.VariableB;
            spark.BaseFunctionTime = slot.VariableE;
            spark.FunctionTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedTableIndex = Index(slot.VariableA, short.MaxValue, "Puyo speed index");
            puyo.HopCooldownTimer = NativeCountdown.FromRaw(slot.VariableB);
            puyo.Function = slot.VariableC == 0 ? null : (PuyoEnemyFunction)slot.VariableC;
            puyo.AirborneFunction = slot.VariableD == 0 ? null : (PuyoAirborneFunction)slot.VariableD;
            puyo.HopRecord = slot.VariableE % PuyoHopDefinitions.RecordSize == 0
                ? Index(slot.VariableE, 6 * PuyoHopDefinitions.RecordSize, "Puyo hop record offset") / PuyoHopDefinitions.RecordSize
                : throw new InvalidDataException($"Recorded Puyo hop record offset ${slot.VariableE:X4} is not a record.");
        }
    }

    /// <summary>
    /// Splits a Shaktool segment's shared orientation word by the segment's role: the centre's heading bucket and
    /// reversal bit, or the final saw's curl progress; every other segment keeps it zero.
    /// </summary>
    /// <param name="segment">The segment's state.</param>
    /// <param name="segmentIndex">The segment's place in the chain, zero through six.</param>
    /// <param name="word">The recorded orientation word.</param>
    private static void ImportShaktoolOrientation(ShaktoolSegmentState segment, int segmentIndex, ushort word)
    {
        segment.ResetOrientation();
        switch (segmentIndex)
        {
            case 3:
                segment.OrientationReversed = (word & 0x8000) != 0;
                int bucket = word & 0x7fff;
                if ((bucket & 0x1f) != 0 || bucket > 0xe0)
                    throw new InvalidDataException($"Recorded Shaktool centre orientation ${word:X4} has no heading bucket.");
                segment.OrientationBucket = bucket;
                return;
            case 6:
                segment.CurlProgress = Index(word, 0xf3fb, "Shaktool curl progress");
                return;
            default:
                if (word != 0)
                    throw new InvalidDataException($"Recorded Shaktool segment {segmentIndex} orientation ${word:X4} is not zero.");
                return;
        }
    }

    /// <summary>Decodes a recorded enemy byte index into the slot it names.</summary>
    /// <param name="word">The recorded byte index, a multiple of the record size.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static int SlotIndex(ushort word, string meaning) =>
        word % RoomEnemySystem.NativeSlotSize == 0 && word / RoomEnemySystem.NativeSlotSize < RoomEnemySystem.MaximumEnemyCount
            ? word / RoomEnemySystem.NativeSlotSize
            : throw new InvalidDataException($"Recorded {meaning} ${word:X4} is not an enemy slot.");

    /// <summary>Decodes a dead Tourian corpse's recorded bank-$A9 main-AI function word into its phase.</summary>
    /// <param name="species">The corpse's species, whose wait, pre-rot and rotting routines are its own.</param>
    /// <param name="word">The recorded function word; zero is an emptied record.</param>
    private static DeadTourianCorpseFunction? DeadTourianCorpseFunctionOf(DeadTourianCorpseSpecies species, ushort word) =>
        (species, word) switch
        {
            (_, 0) => null,
            (_, 0xda63) => DeadTourianCorpseFunction.Finished,
            (DeadTourianCorpseSpecies.Zoomer, 0xda69) or (DeadTourianCorpseSpecies.Ripper, 0xda73) or (DeadTourianCorpseSpecies.Skree, 0xda6e) =>
                DeadTourianCorpseFunction.Waiting,
            (DeadTourianCorpseSpecies.Zoomer, 0xda94) or (DeadTourianCorpseSpecies.Ripper, 0xda99) or (DeadTourianCorpseSpecies.Skree, 0xda9e) =>
                DeadTourianCorpseFunction.PreRotDelay,
            (DeadTourianCorpseSpecies.Zoomer, 0xdad0) or (DeadTourianCorpseSpecies.Ripper, 0xdae6) or (DeadTourianCorpseSpecies.Skree, 0xdafc) =>
                DeadTourianCorpseFunction.Rotting,
            _ => throw new InvalidDataException($"Recorded dead {species} function ${word:X4} is not one of its routines."),
        };

    /// <summary>Decodes a recorded index word that the family's updates keep within 0 through <paramref name="maximum"/>.</summary>
    /// <param name="word">The recorded word.</param>
    /// <param name="maximum">The largest value the family produces.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static int Index(ushort word, int maximum, string meaning) =>
        word <= maximum ? word : throw new InvalidDataException($"Recorded {meaning} {word} exceeds {maximum}.");
}
