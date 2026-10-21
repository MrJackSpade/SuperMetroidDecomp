using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>
/// Stock identities for installed enemy compositions. These are visual frame selections,
/// not editable instruction timers, AI callbacks, collision or hitbox definitions.
/// </summary>
internal static class EnemySpritemapDefinitions
{

    /// <summary>Revision 68 prefix before Ridley's eight breakup body frames.</summary>
    internal const int PreRidleyBreakupFrameCount = 1404;
    /// <summary>Revision 67 prefix before the Kzan and Polyp single-frame compositions.</summary>
    internal const int PreSingleFrameFrameCount = 1402;
    /// <summary>Revision 66 prefix before Puromi/Nuclear Waffle head frames.</summary>
    internal const int PreNuclearWaffleFrameCount = 1394;
    /// <summary>Revision 65 prefix before Kraid's two belly-lint frames.</summary>
    internal const int PreKraidLintFrameCount = 1392;
    /// <summary>Revision 64 prefix before audited environmental and Tourian artwork additions.</summary>
    internal const int PreAuditOrdinaryFrameCount = 1339;
    /// <summary>Revision 63 prefix before friendly-animal compositions.</summary>
    internal const int PreFriendlyAnimalFrameCount = 1257;
    /// <summary>Revision 62 prefix before Zero crawler artwork.</summary>
    internal const int PreZeroFrameCount = 1241;
    /// <summary>Revision 61 prefix before the tatori family.</summary>
    internal const int PreMamaTurtleFrameCount = 1212;
    /// <summary>Revision 60 prefix before gunship display compositions.</summary>
    internal const int PreGunshipFrameCount = 1199;
    /// <summary>Revision 59 prefix before Botwoon visual frames.</summary>
    internal const int PreBotwoonFrameCount = 1183;
    /// <summary>Revision 58 prefix before Yard enemy visual frames.</summary>
    internal const int PreYardFrameCount = 1079;
    /// <summary>Revision 57 prefix before work-robot visual frames.</summary>
    internal const int PreWorkRobotFrameCount = 1052;
    /// <summary>Revision 56 prefix before Evir visual frames.</summary>
    internal const int PreEvirFrameCount = 1028;
    /// <summary>Revision 55 prefix before Mochtroid visual frames.</summary>
    internal const int PreMochtroidFrameCount = 1022;
    /// <summary>Revision 54 prefix before Tourian corpse artwork.</summary>
    internal const int PreDeadTourianCorpseFrameCount = 1009;
    /// <summary>Revision 53 prefix before stationary dead Torizo artwork.</summary>
    internal const int PreDeadTorizoStationaryFrameCount = 1008;
    /// <summary>Revision 52 prefix before Rinka visual frames.</summary>
    internal const int PreRinkaFrameCount = 1003;
    /// <summary>Revision 51 prefix before Viola visual frames.</summary>
    internal const int PreViolaFrameCount = 995;
    /// <summary>Revision 50 prefix before Chozo statue artwork.</summary>
    internal const int PreChozoStatueFrameCount = 969;
    /// <summary>Revision 49 prefix before Norfair lava-jumper frames.</summary>
    internal const int PreNorfairLavaJumperFrameCount = 958;
    /// <summary>Revision 48 prefix before Multiviola visual frames.</summary>
    internal const int PreMultiviolaFrameCount = 950;
    /// <summary>Revision 47 prefix before Dragon enemy visual frames.</summary>
    internal const int PreDragonFrameCount = 938;
    /// <summary>Revision 46 prefix before Tripper/Kamer compositions.</summary>
    internal const int PreTripperKamerFrameCount = 920;
    /// <summary>Revision 45 prefix before Shaktool visual frames.</summary>
    internal const int PreShaktoolFrameCount = 905;
    /// <summary>Revision 44 prefix before Metroid visual frames.</summary>
    internal const int PreMetroidFrameCount = 901;
    /// <summary>Revision 43 prefix before shutter compositions.</summary>
    internal const int PreShutterFrameCount = 896;
    /// <summary>Revision 42 prefix before morph-ball eye artwork.</summary>
    internal const int PreMorphBallEyeFrameCount = 874;
    /// <summary>Revision 41 prefix before face-block visual frames.</summary>
    internal const int PreFaceBlockFrameCount = 869;
    /// <summary>Revision 40 prefix before Kago visual frames.</summary>
    internal const int PreKagoFrameCount = 866;
    /// <summary>Revision 39 prefix before Fly visual frames.</summary>
    internal const int PreFlyFrameCount = 862;
    /// <summary>Revision 38 prefix before Sciser visual frames.</summary>
    internal const int PreSciserFrameCount = 850;
    /// <summary>Revision 37 prefix before supplementary Ridley artwork.</summary>
    internal const int PreRidleySupplementFrameCount = 819;
    /// <summary>Revision 36 prefix before dead Torizo artwork.</summary>
    internal const int PreDeadTorizoFrameCount = 818;
    /// <summary>Revision 35 prefix before Mother Brain visual frames.</summary>
    internal const int PreMotherBrainFrameCount = 800;
    /// <summary>Revision 34 retained-frame prefix before Ki Hunter artwork.</summary>
    internal const int PreKiHunterFrameCount = 759;
    /// <summary>Revision 33 retained-frame prefix before Yapping Maw artwork.</summary>
    internal const int PreYappingMawFrameCount = 735;
    /// <summary>Revision 32 prefix before room sprite-object compositions.</summary>
    internal const int PreRoomSpriteObjectFrameCount = 472;
    /// <summary>Revision 31 prefix before Draygon breath frames.</summary>
    internal const int PreDraygonBreathFrameCount = 463;
    /// <summary>Revision 30 prefix before Draygon intro frames.</summary>
    internal const int PreDraygonIntroFrameCount = 459;
    /// <summary>Revision 29 prefix before elevator visual frames.</summary>
    internal const int PreElevatorFrameCount = 457;
    /// <summary>Revision 28 prefix before Kamer platform frames.</summary>
    internal const int PreKamerFrameCount = 453;
    /// <summary>Revision 27 prefix before Fune/Namihe frames.</summary>
    internal const int PreFuneNamiheFrameCount = 431;
    /// <summary>Revision 26 prefix before Sbug frames.</summary>
    internal const int PreSbugFrameCount = 407;
    /// <summary>Revision 25 prefix before horizontal Zoomer frames.</summary>
    internal const int PreHZoomerFrameCount = 387;
    /// <summary>Revision 24 prefix before Choot frames.</summary>
    internal const int PreChootFrameCount = 383;
    /// <summary>Revision 23 prefix before Hopper frames.</summary>
    internal const int PreHopperFrameCount = 359;
    /// <summary>Revision 22 prefix before Beetom frames.</summary>
    internal const int PreBeetomFrameCount = 337;
    /// <summary>Revision 21 prefix before Alcoon frames.</summary>
    internal const int PreAlcoonFrameCount = 319;
    /// <summary>Revision 20 prefix before Bull frames.</summary>
    internal const int PreBullFrameCount = 316;
    /// <summary>Revision 19 prefix before Puyo frames.</summary>
    internal const int PrePuyoFrameCount = 308;
    /// <summary>Revision 18 prefix before Norfair Rio frames.</summary>
    internal const int PreNorfairRioFrameCount = 288;
    /// <summary>Revision 17 prefix before Lower Norfair Rio frames.</summary>
    internal const int PreLowerNorfairRioFrameCount = 270;
    /// <summary>Revision 16 prefix before ordinary Rio frames.</summary>
    internal const int PreRioFrameCount = 262;
    /// <summary>Revision 15 prefix before Ceres baby frames.</summary>
    internal const int PreCeresBabyFrameCount = 259;
    /// <summary>Revision 14 prefix before Ceres door frames.</summary>
    internal const int PreCeresDoorFrameCount = 244;
    /// <summary>Revision 13 frame count before editable display bindings; the next revision changes metadata without adding frames.</summary>
    internal const int PreDisplayBindingsFrameCount = 244;
    /// <summary>Revision 12 prefix before Magdollite frames.</summary>
    internal const int PreMagdolliteFrameCount = 215;
    /// <summary>Revision 11 prefix before Fireflea frames.</summary>
    internal const int PreFirefleaFrameCount = 194;
    /// <summary>Revision 10 prefix before Ripper frames.</summary>
    internal const int PreRipperFrameCount = 180;
    /// <summary>Revision 9 prefix before Owtch/Stoke frames.</summary>
    internal const int PreOwtchStokeFrameCount = 167;
    /// <summary>Revision 8 retained frame count for older composition documents.</summary>
    internal const int PreviousFrameCount = 145;
    /// <summary>Revision 7 retained frame count for older composition documents.</summary>
    internal const int PriorFrameCount = 101;
    /// <summary>Revision 6 retained frame count for older composition documents.</summary>
    internal const int EarlierFrameCount = 79;
    /// <summary>Revision 5 retained frame count for older composition documents.</summary>
    internal const int IntermediateFrameCount = 69;
    /// <summary>Revision 4 retained frame count for the oldest accepted composition document.</summary>
    internal const int LegacyFrameCount = 47;
    /// <summary>Installed document containing editable enemy composition frames and bindings.</summary>
    internal const string FileName = "enemy-compositions.json";
    /// <summary>Bank $A2 containing Boyon's native spritemap pointers.</summary>
    internal const byte BoyonBank = 0xa2;
    /// <summary>Bank $A2 used to resolve ordinary Rio visual frames.</summary>
    internal const byte RioBank = 0xa2;
    /// <summary>Bank $A2 used to resolve Lower Norfair Rio frames.</summary>
    internal const byte LowerNorfairRioBank = 0xa2;
    /// <summary>Bank $A2 used to resolve Norfair Rio frames.</summary>
    internal const byte NorfairRioBank = 0xa2;
    /// <summary>Bank $A2 containing Puyo composition pointers.</summary>
    internal const byte PuyoBank = 0xa2;
    /// <summary>Bank $A8 containing Bull composition pointers.</summary>
    internal const byte BullBank = 0xa8;
    /// <summary>Bank $A8 containing Alcoon composition pointers.</summary>
    internal const byte AlcoonBank = 0xa8;
    /// <summary>Bank $A8 containing Beetom composition pointers.</summary>
    internal const byte BeetomBank = 0xa8;
    /// <summary>Bank $A3 containing Hopper composition pointers.</summary>
    internal const byte HopperBank = 0xa3;
    /// <summary>Bank $A2 containing Choot composition pointers.</summary>
    internal const byte ChootBank = 0xa2;
    /// <summary>Bank $A3 containing horizontal Zoomer composition pointers.</summary>
    internal const byte HZoomerBank = 0xa3;
    /// <summary>Bank $A3 containing Sbug composition pointers.</summary>
    internal const byte SbugBank = 0xa3;
    /// <summary>Bank $A8 containing Fune and Namihe composition pointers.</summary>
    internal const byte FuneNamiheBank = 0xa8;
    /// <summary>Bank $A2 containing Kamer platform composition pointers.</summary>
    internal const byte KamerPlatformBank = 0xa2;
    /// <summary>Bank $A3 containing elevator composition pointers.</summary>
    internal const byte ElevatorBank = 0xa3;
    /// <summary>Bank $B4 containing room sprite-object composition pointers.</summary>
    internal const byte RoomSpriteObjectBank = 0xb4;
    /// <summary>Bank $A3 containing Skultera composition pointers.</summary>
    internal const byte SkulteraBank = 0xa3;
    /// <summary>Bank $A3 containing Waver composition pointers.</summary>
    internal const byte WaverBank = 0xa3;
    /// <summary>Bank $A3 containing Zoa composition pointers.</summary>
    internal const byte ZoaBank = 0xa3;
    /// <summary>Bank $A3 containing Skree and Metaree composition pointers.</summary>
    internal const byte SkreeMetareeBank = 0xa3;
    /// <summary>Bank $B3 containing pipe-bug composition pointers.</summary>
    internal const byte PipeBugBank = 0xb3;
    /// <summary>Bank $A6 containing Fake Kraid composition pointers.</summary>
    internal const byte FakeKraidBank = 0xa6;
    /// <summary>Bank $A7 containing Kraid nail composition pointers.</summary>
    internal const byte KraidNailBank = 0xa7;
    /// <summary>Bank $A2 containing Owtch and Stoke composition pointers.</summary>
    internal const byte OwtchStokeBank = 0xa2;
    /// <summary>Bank $A2 containing Ripper composition pointers.</summary>
    internal const byte RipperBank = 0xa2;
    /// <summary>Bank $A3 containing Fireflea composition pointers.</summary>
    internal const byte FirefleaBank = 0xa3;
    /// <summary>Bank $A8 containing Magdollite composition pointers.</summary>
    internal const byte MagdolliteBank = 0xa8;
    /// <summary>Bank $A6 containing boulder composition pointers.</summary>
    internal const byte BoulderBank = 0xa6;
    /// <summary>Bank $A8 containing Atomic composition pointers.</summary>
    internal const byte AtomicBank = 0xa8;

    /// <summary>Two-byte part count followed by five bytes per native OAM part.</summary>
    internal static int NativeFrameBytes(int parts) => 2 + 5 * parts;

    /// <summary>
    /// Appends each later enemy family's frames after the historical prefix. Additional bank-$B4
    /// sprite-object targets come from compiled selectors shared by 62 programs.
    /// </summary>
    private static IEnumerable<EnemySpritemapDefinition> EnumerateFrames()
    {
        foreach (var frame in EnemyNamedSpritemapFrameDefinitions.NamedFrames())
            yield return frame;
        foreach (var frame in RoomSpriteObjectVisualDefinitions.AdditionalFrames(EnemyNamedSpritemapFrameDefinitions.NamedFrames()))
            yield return frame;
        foreach (var frame in YappingMawVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in KiHunterVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MotherBrainVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in DeadTorizoArtworkDefinitions.Frames())
            yield return frame;
        foreach (var frame in RidleySupplementalVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in SciserVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in FlyVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in KagoVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in BlueBrinstarFaceBlockVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MorphBallEyeVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ShutterVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MetroidVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ShaktoolVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in TripperKamerVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in DragonVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MultiviolaVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in NorfairLavaJumperVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ChozoStatueVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ViolaVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in RinkaVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in DeadTorizoArtworkDefinitions.StationaryFrames())
            yield return frame;
        foreach (var frame in DeadTourianCorpseVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MochtroidVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in EvirVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in WorkRobotVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in YardVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in BotwoonVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in GunshipVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in MamaTurtleVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ZeroVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in FriendlyAnimalVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in HibashiVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ZebetiteVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in WreckedShipGhostVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in PowampVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in SparkVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in ShitroidVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in KraidLintVisualDefinitions.Frames())
            yield return frame;
        foreach (var frame in NuclearWaffleVisualDefinitions.Frames())
            yield return frame;
        yield return SingleFrameEnemyVisualDefinitions.Kzan;
        yield return SingleFrameEnemyVisualDefinitions.Polyp;
        foreach (var frame in RidleyBreakupVisualDefinitions.Legs) yield return frame;
        foreach (var frame in RidleyBreakupVisualDefinitions.Torso) yield return frame;
        foreach (var frame in RidleyBreakupVisualDefinitions.Head) yield return frame;
        foreach (var frame in RidleyBreakupVisualDefinitions.Claw) yield return frame;
    }

    /// <summary>Lazy, schema-ordered stock frame catalog used to validate and bind installed compositions.</summary>
    internal static FrameList Frames { get; } = new();

    /// <summary>Exposes the complete stock frame sequence with checked index and range access.</summary>
    internal sealed class FrameList : IReadOnlyList<EnemySpritemapDefinition>
    {
        /// <summary>Materializes the ordered catalogs only when a consumer first needs them.</summary>
        /// <remarks>The frame list is a fixed derivation of the definitions; derive it once, not per access.</remarks>
        private readonly Lazy<EnemySpritemapDefinition[]> frames = new(() => EnumerateFrames().ToArray());
        /// <summary>Total named and appended frames in the current installed schema.</summary>
        public int Count => frames.Value.Length;
        /// <summary>Array-compatible length for internal catalog consumers.</summary>
        internal int Length => Count;
        /// <summary>Reads one current-schema frame with an explicit bounds check.</summary>
        public EnemySpritemapDefinition this[int index] => (uint)index < Count
            ? frames.Value[index] : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Copies a contiguous schema prefix or family range for compatibility checks.</summary>
        internal EnemySpritemapDefinition[] this[Range range] => frames.Value[range];
        /// <summary>Traverses the materialized frame sequence in installed-schema order.</summary>
        public IEnumerator<EnemySpritemapDefinition> GetEnumerator() => ((IEnumerable<EnemySpritemapDefinition>)frames.Value).GetEnumerator();
        /// <summary>Uses the same ordered traversal for non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
