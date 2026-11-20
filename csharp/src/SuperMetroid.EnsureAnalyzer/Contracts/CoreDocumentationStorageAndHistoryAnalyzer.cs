using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// SME6479 (#2550) widens SME6454's storage, original-program and port-narration vocabularies to every phrasing the
/// #1423 review found in <c>SuperMetroid.Core</c>'s documentation: outside hardware owners, no comment tells a value by its
/// storage's range or unit, reasons about its storage's wrap, narrates the original's program flow, routines, tables,
/// records, flags, installed animations or loaded colours, describes the display hardware, or tells the port's own
/// reasoning and history. Core's documentation says what the game does: how far a thing moves, which update a thing
/// happens on, what a picture shows, how often a chance is taken. Each comment is read with its markup removed and its
/// line breaks joined, and a run of line comments, one under another, is read as one text for a phrase that runs on from
/// one line to the next. One diagnostic per comment; there is no allowance list. A <c>[CompatibilityService]</c> type,
/// which reproduces hardware behaviour, and an <c>[EncodingOwner]</c> codec describe the encoding they own. See
/// <c>docs/core-vocabulary.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CoreDocumentationStorageAndHistoryAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core comment tells storage, the original program or the port's own history.</summary>
    public const string DocumentationStorageAndHistoryId = "SME6479";

    /// <summary>
    /// A value told by its storage's range or by a machine number: a frame counter's or a field's range, a screen coordinate's
    /// signed range, a share of the 65536 random samples, a sample of a number or more, a bit of a value, a storage width, the
    /// finest change a speed keeps and a value told as nonzero or negative (<c>the short frame counter, 0 through 255</c>,
    /// <c>the lowest 16384 of the 65536 samples</c>, <c>its lowest bit</c>).
    /// </summary>
    private static readonly Regex StorageRangeProse = new(
        @"\b(?:short frame counter|frame phase)[^.;]{0,40}\b(?:0|zero) through 255\b" +
        @"|\b256-value field, 0 through 255\b|\b0 through 4095\b|\bwhole pixels, 0 through 127\b" +
        @"|\blong frame counter[^.;]{0,30}\b0 through 65535\b|\b0 through 65535 as the frame clock\b" +
        @"|\b(?:screen (?:X|Y)|coordinate), -32768 through 32767\b|\b65536-update wrap\b" +
        @"|\bthe sample is 32768 or more\b|\boccupy 0-127\b|\bright-space 0-127\b" +
        @"|\bof the 65536 (?:random )?samples\b|\b(?:often|minus 224) \(?65312\b|\bnear 65535\b|\b\d+-(?:value" +
        @"|update|step) (?:field|cycle|turn|frame counter)\b|\bsample of \d+ or more\b|\bdivisible by four\b" +
        @"|\ban odd count of\s+sends\b|\bits (?:lowest|highest) bit\b|\bThe lowest bit of\b" +
        @"|\bremainder below 256\b|\bevery speed under 256 pixels\b|\b31,745\b|\bstorage width\b" +
        @"|\bthe finest change a speed keeps\b|\bzero fractional speed\b|\bonly when it is nonzero\b" +
        @"|\bnonzero dive choice\b|\bsigned speed turns negative\b|\bdifference from the minimum is negative\b" +
        @"|\bwhole speed so large\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A unit of the original's storage: 256ths, 768ths and 512ths of a pixel, a turn or a step, counts over 256, 128ths of
    /// the whole, a fixed-point bias, a projectile pool's size, colours counted as the original stored them and a value's
    /// overflow (<c>in 256ths of a pixel</c>, <c>24/256</c>, <c>fifteen nontransparent colours</c>, <c>the fraction's
    /// overflow</c>).
    /// </summary>
    private static readonly Regex StorageUnitProse = new(
        @"\bin (?:signed )?(?:256|768|512)ths (?:of a (?:pixel|turn|step)|before|in the|each)\b|\b(?:256" +
        @"|768)ths of a (?:pixel|turn|step)\b|\bwhole 256ths\b|\bsize in 256ths\b|\bits 768ths\b" +
        @"|\bcounted as 256ths\b|\b128ths of the whole\b|\bfiner-than-256ths\b|\btangent to 256ths\b" +
        @"|(?<![\w/])(?:24|8|160|240)/256\b|\bsigned 1/256\b|\+1\.0 bias\b|\bits speed in 256ths\b" +
        @"|\bof the angle\'s 256ths\b|\bsixteen-projectile\b|\beight-colou?r\b|\bsixteen-colour palette\b" +
        @"|\b(?:fifteen|three) (?:fading |nontransparent )?colou?rs\b|\bThe ghost's sixteen colours\b" +
        @"|\bthe transparent colou?r\b|\bX and Y words\b|\bthree of the brain's colours\b" +
        @"|\b16 times the column\b|\bsplit planet velocity\b|\bthe fraction's overflow\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Reasoning about a value's storage range: a count that never nears a wrap, a value far inside its range, a few thousand
    /// units of rest, a room at most fifty screens tall, a speed that stays below a number of pixels and comparisons told as
    /// signed (<c>stays far inside its range</c>, <c>at most fifty screens</c>, <c>The comparison is signed</c>).
    /// </summary>
    private static readonly Regex RangeReasoningProse = new(
        @"\bnears? a wrap\b|\bwrap of its count\b|\bwraps past (?:the room|65535)\b|\bwrapping at 65536\b" +
        @"|\bwithout unsigned wraparound\b|\bfar inside (?:its|the) (?:velocity's |speed's )?range\b" +
        @"|\bcarries it out of range\b|\bpasses the speed's range\b|\bnear the end of its range\b" +
        @"|\bfew thousand units\b|\bat most fifty screens\b|\bstays? below \d+ pixels an update\b" +
        @"|\bthe clamps compare signed\b|\bThe comparison is signed\b|\bvalid wrapped coordinates\b" +
        @"|\bthe frame clock wraps it\b|\bThe samples lie within\b|\bthe up-zero angle convention\b" +
        @"|\bonly whether it lies in the second half\b|\bwould pass 65535\b|\bat most 32768\b" +
        @"|\bcontinues past the field\'s last value\b|\b(?:a later|whose next) step throws\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's program flow: a fall-through, a direct or early return, an entry, a tail, a branch named by its test,
    /// a shared delete, a loop's order and an unreachable path (<c>falls directly into</c>, <c>returns early</c>, <c>main/hurt
    /// entry</c>, <c>the frame tail</c>, <c>is never reached</c>).
    /// </summary>
    private static readonly Regex ProgramFlowProse = new(
        @"\bfalls? (?:directly )?into\b|\bfalls? through (?:here|into|to)\b|\bStand-up falls through\b" +
        @"|\bfell through\b|\bfalling through here\b|\breturns? (?:directly|early)\b" +
        @"|\bThe game returns (?:before|directly)\b|\bearly-return\b|\bliteral early branch\b" +
        @"|\breturns into the following\b|\bsame-frame jump back\b|\bthat returns at once\b" +
        @"|\bEvery reaction that returns\b|\blookup returns at once\b|\bmain/hurt entry\b" +
        @"|\bthe common body main\b|\bclosing tail\b|\bboss-owned tail\b|\bthe frame tail\b" +
        @"|\bsuspended frame tail\b|\binput phase's (?:head|tail)\b|\bfollowing inert step\b|\b(?:non-negative" +
        @"|left|deceleration|state-selection|slow three-pixel|target-step equality) (?:path|branch)\b" +
        @"|\bindependent branches\b|\bthe same branch used\b|\bdrawing branches\b" +
        @"|\bbranch into pose-definition\b|\bacid-height branches\b|\binitial flame-rain branch\b" +
        @"|\bcontrol-flow\b|\bsame long cleanup\b|\bthe following shared delete\b" +
        @"|\bthe next step is the shared delete\b|\bshow entry, installing\b" +
        @"|\bsteps the hand beam's phase on\b|\bimmediately call their shared setup\b" +
        @"|\bimmediately after reading\b|\bresumes its own cursor\b|\bpassed their places in the list\b" +
        @"|\bWith objects drawn first\b|\binterruption owns\b|\bpose/handler initialization\b" +
        @"|\bnot executed by the Baby\b|\btests the holes from the last\b|\bthe call it runs out on\b" +
        @"|\bgrows on call\b|\bthen asks for the spawn\'s facing\b|\bjump, and fall targets\b" +
        @"|\bis never reached\b|\bfull cooldown in phase\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's routines and engine parts named in prose: preludes, movers, initialization AI, collision walks, setups,
    /// lookups, fallbacks, checks and handlers (<c>the projectile prelude</c>, <c>the common mover</c>, <c>init AI</c>,
    /// <c>the pose-definition fallback</c>).
    /// </summary>
    private static readonly Regex RoutineNameProse = new(
        @"\b(?:collision|projectile|frame-hitbox|ordinary projectile|shared projectile) preludes?\b" +
        @"|\b(?:common|velocity|enemy velocity) mover\b|\bMain AI and its mover\b|\binit[- ]AI\b" +
        @"|\bunconditional (?:enemy|death call)\b|\bcollision walks?\b|\bshared rectangle walk\b" +
        @"|\bthe sparkle setup\b|\bone-shot special-block setup\b|\bpicture-load sequence\b" +
        @"|\bquarter-turn sine lookup\b|\bselects the sine sample\b|\bby the game's sine\b" +
        @"|\bthe game's multiply\b|\bslope-processing\b|\bslope-height row\b|\bThe selector result\b" +
        @"|\bfiring selector\b|\bscroll-boundary checks\b|\bthe speed calculation always\b" +
        @"|\bthe sub-speed test\b|\bgrapple-phase comparison\b|\bpose-definition fallback\b" +
        @"|\breach the input fallback\b|\blookup failure\b|\bthe liquid-flag update\b" +
        @"|\bknockback check, knockback movement\b|\bThe zero test shares\b|\bnegative-result clamp\b" +
        @"|\bthe calling update\b|\bat the collision call\b|\bThe auxiliary Samus\b|\bproperty flicker\b" +
        @"|\bits point collision\b|\bthe game's (?:own handling|wrap|below-zero|partwise|music queues|grapple" +
        @"|X and Y movement|wait tests|vertical-collision)\b|\bdirection-aware (?:X mover|displacement path)\b" +
        @"|\bcalling the horizontal mover\b|\bGolden Torizo's code\b|\bbinds corpse rotting\b|\bcolour-math\b" +
        @"|\bthe rainbow colours' handler\b|\bshared geometry from\b|\bthe game's timed draws\b" +
        @"|\bits delete did\b|\bmovement-index mutations\b|\breads past the Power Bomb\b|\bbeyond its table\b" +
        @"|\bpositive half-wave\b|\bhalf-wave deterministically\b|\bgates only on red\b" +
        @"|\breplaces the horizontal result\b|\bwould have cleared its enable\b|\bfrom the room main's\b" +
        @"|\bone-frame request\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Installing animations and handlers and loading or copying colours and pictures: an animation installed, colours a setup
    /// loads, a colour copy, a frame that wrote colours, an effect's records and the display's maps, pages, brightness and
    /// images (<c>installs the laser head animation</c>, <c>which his initialization loads</c>, <c>the runaway colour
    /// copy</c>, <c>master brightness</c>).
    /// </summary>
    private static readonly Regex InstallationAndColourLoadProse = new(
        @"\binstall(?:s|ing)\b(?:\s+[\w/-]+){0,4}?\s+(?:animation|loop|handoff)\b" +
        @"|\binstalls cooldown artwork\b|\battachment state\b|\binstalls the first page's setup\b" +
        @"|\binstalled the handoff\b|\binstalls (?:and starts|a sprite|the final|a flying)\b" +
        @"|\binstalled light-beam\b|\binstalled only for\b|\breinstalls the\b|\bwhich (?:its|his" +
        @"|her) setup installs\b|\bcrack setup installs\b|\binstalled by the shard\b|\bwhich (?:his|her" +
        @"|its) (?:initialization|setup|opening|room's entry) loads\b|\b(?:initialization|opening) loads\b" +
        @"|\beach encounter loads\b|\beach colour load of the\b|\bits colour load is not\b" +
        @"|\bsuit-colour reload\b|\bcolours his setup loads\b|\bbefore any (?:suit|beam) is loaded\b" +
        @"|\bfiring colours are loaded\b|\bcomplete load of\b|\b(?:runaway )?colour copy\b" +
        @"|\bfirst colour pair is copied\b|\bsuppress its normal copy\b|\bcopied an authored shade\b" +
        @"|\boverwrote (?:her|its|their)\b|\bframes that wrote it\b|\bcolours it writes\b" +
        @"|\bcolours written then\b|\bthe records of\b|\brecords of \(|\bwhich loads them beside\b" +
        @"|\binstalls the record\'s block\b|\bshows the record\b|\blast record\b|\bthe record they showed\b" +
        @"|\bloads the rear map\b|\brear-view map\b|\bplanet-reveal (?:map|page)\b" +
        @"|\breplaces the station maps\b|\bscheduled melting graphics\b|\bforces blank\b|\bmaster brightness\b" +
        @"|\bwithout rasterization\b|\bdisplay image\b|\bover the (?:room's |arena's )foreground\b" +
        @"|\bforeground cell\b|\bover the room's front tiles\b|\bThe pause's setup loads\b" +
        @"|\bsecond gameplay (?:room )?page\b|\bdestination sprite buffer\b|\bthe order it loads\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The display hardware in gameplay prose: sprite priorities, sprite records and entries, background layers, frames and
    /// offsets, a frame drawn partly as sprites, room graphics and tile cells (<c>the low-priority frames</c>, <c>sprite
    /// entries</c>, <c>drawn partly as a background layer</c>).
    /// </summary>
    private static readonly Regex DisplayHardwareProse = new(
        @"\blow-priority frames\b|\bEqual-priority overlap\b|\bSame-priority sprite\b|\barm-cannon priority\b" +
        @"|\bsprite (?:records?|attributes|entries)\b|\bas a background layer\b|\bbackground frames?\b" +
        @"|\bthe background wave's\b|\bbackground distortion\b|\bnegative background vertical offset\b" +
        @"|\bBackground offset latched\b|\bcurrent background position\b|\bbackground text object\b" +
        @"|\bdistorted background\b|\bdescending background\b|\bdraws partly\b|\bpartly as sprites\b" +
        @"|\bdrawn as sprites\b|\bon his boss body layer\b|\btheir sprite parts\b|\broom-graphics\b" +
        @"|\blayer scroll copies\b|\bthe upper half becomes\b|\bin several groups and\b|\bmap-memory owners\b" +
        @"|\b(?:icon's|template's) cells\b|\bleaves its cells blank\b|\bthe timer's sprite pictures\b" +
        @"|\bbackground's per-frame steps\b|\bdecorative render sprites\b|\bover the beams' upper colours\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Storage named for what the original kept: flags, set bits and statuses, latches, records, population entries, frame
    /// counters and timers, halves and pairs of a value, block types and parameters, writes and stores, controller masks and
    /// the game told as keeping, writing or comparing a value (<c>the animating flag</c>, <c>the load-station record</c>,
    /// <c>rewrites only the block's type and parameter</c>).
    /// </summary>
    private static readonly Regex StorageRecordProse = new(
        @"\b(?:animating|collision|freeze|momentum|falling|escape|single) flag\b" +
        @"|\bequips the [\w -]+ (?:equipment|beam) flag\b|\bsets the flag\b|\bset bits?\b|\bindependent bits\b" +
        @"|\bexploration bits\b|\bThe random bits\b|\bstatus is set\b|\bany set status\b|\bthe zero status\b" +
        @"|\b(?:sets|clears) this latch\b|\bWithout this latch\b|\bthe charge-sound latch\b" +
        @"|\bload-station record\b|\bpopulation entr(?:y names|ies)\b|\bpopulation's frame timer\b" +
        @"|\bpopulation gave it\b|\bprimary \w+ population entry\b|\bprojectile fields\b" +
        @"|\bProjectile records\b|\bhitbox record\b|\bturn records\b|\bworkspace\b|\b(?:short" +
        @"|long) frame counter\b|\bframe counter's count\b|\bframe timer set to one\b" +
        @"|\bframe timer as the spawn\b|\bprojectile's frame timer\b|\bone-tick frame countdown\b" +
        @"|\bthe halves of the random\b|\btwo halves exchanged\b|\bthe working value\b" +
        @"|\bworking value the room mains\b|\bextra pair\b|\bnumeric run-speed pair\b|\bmomentum pair\b" +
        @"|\bunowned pair\b|\bthe pair is cleared\b|\bThe game keeps (?:only|one)\b|\bThe game writes the\b" +
        @"|\bthe game shares\b|\bas the game clears\b|\bThe game divides\b|\bThe game compares the speed\b" +
        @"|\bcopy of it\b|\becho storage\b|\bexternal-displacement values\b|\bthese two global values\b" +
        @"|\bshared charge/grapple\b|\bthe still-set\b|\breuses shinespark state\b" +
        @"|\btype and (?:its )?parameter\b|\bblock's type\b|\brewrites? (?:only|nothing but)\b" +
        @"|\bexact same collision write\b|\bcontrol writes\b|\broom writes\b|\bwrites these values\b" +
        @"|\breads the same values\b|\bEvery store\b|\bstores the counter\b|\bthe held controller mask\b" +
        @"|\bphysical controller pair\b|\bautomatic controller sample\b|\bwhich nothing else reads\b" +
        @"|\bnothing it holds is inherited\b|\bthe order of the writes is unobservable\b" +
        @"|\bThe music data set and track\b|\bboss flags\b|\bleftover number\b|\bstreams in\b" +
        @"|\bthe shared gradual-change counter\b|\bsaved exploration bits\b|\bwhole word\b|\bfraction word\b" +
        @"|\bthe edge word\b|\bthat word at\b|\blow byte\b|\bhigh byte\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's tables, entries and allocation: an area's own tables, rows and entries, reads past an end, record and
    /// object orders, allocation order and pools, an unconnected region and an enemy told by its place in the original's
    /// list (<c>the area's own tables</c>, <c>past-the-end</c>, <c>allocation order</c>, <c>a normal second enemy</c>).
    /// </summary>
    private static readonly Regex TableAndAllocationProse = new(
        @"\b(?:area's own|areas' own|own) tables?\b|\bown table gives\b|\bits own row in every policy\b" +
        @"|\bpolicy's rows\b|\bpast-the-end\b|\bpartial or past\b|\bthe entry the game reads\b" +
        @"|\bcontour entry\b|\bpast the beam selectors\b|\bfollows the beam sound lists\b" +
        @"|\bthe same entries of\b|\breveal tables\b|\bstored curve\b|\brecords store\b" +
        @"|\broom-authored record order\b|\bdescending room object order\b" +
        @"|\bin the order of their population\b|\ballocation (?:order|attempts?|size)\b|\bshared-pool\b" +
        @"|\bthe pool gives\b|\bcommon or its area's list\b|\bunconnected region\b|\bunrelated data\b" +
        @"|\bwhat lies past a list\b|\bnot part of the game's design\b|\bnumbered six ways\b" +
        @"|\bThe original calls\b|\bthe original names\b|\bIn the original\b|\ba normal second enemy\b" +
        @"|\bfour separate enemies\b|\bhighest occupied projectile\b|\bfour solid-enemy collisions\b" +
        @"|\bprojectiles of this room\b|\bthe level data that followed\b|\bselects the right entry\b" +
        @"|\btimer-eight\b|\bread a damage value\b|\bfrom an earlier occupant\b|\bprivate projectile's\b" +
        @"|\bthe same enemy frame\b|\bits final entry\b|\bcollision loop\b" +
        @"|\btwo short trigonometric products\b|\bwithout velocity integration\b|\bzero hitbox radii\b" +
        @"|\bcopies the complete definition\b|\beach segment\'s copy\b|\bpseudo-door leads to no room\b" +
        @"|\bclears only the velocity\'s whole\b|\bpose-input latches\b|\bthe target-step\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The port's reasoning, history and self-narration: what it does deliberately or intentionally, what it models, what it
    /// would break or invent, instructions to its developers and what it replaced (<c>deliberately owns only</c>, <c>Models
    /// the</c>, <c>Without this</c>, <c>Do not turn a protected</c>, <c>rather than a saved perch</c>).
    /// </summary>
    private static readonly Regex PortReasoningProse = new(
        @"\bdeliberately (?:owns|not|skips|absent|replaces|starts|returns|alternates|carries|outside|separate" +
        @"|cue-driven|a delta)\b|\bremain deliberately\b|\bDeliberately sends\b|\bintentionally (?:pure" +
        @"|does not|omits|leave|remain)\b|\bLiteral movement\b|\bliteral comparisons\b|\bModels the\b" +
        @"|\bthis model\b|\bthe model does not\b|\bport draws\b|\bthe model's constructor\b" +
        @"|\bnot an invented\b|\binvent(?:s|ed|ing)? (?:another|gameplay|bespoke)\b" +
        @"|\bis deliberately not invented\b|\baccidentally\b|\bsilently (?:treating|refresh|reinterpret)\b" +
        @"|\bescape hatch\b|\bnarrow (?:method|operation)\b|\bconsistency check\b|\bDo not (?:turn a protected" +
        @"|infer an airborne|advance here|route it)\b|\bdo not rerun controls\b|\bmust not accidentally\b" +
        @"|\bmust not end\b|\bmust remain untouched\b|\bKeep this after\b|\bKeep it distinct\b" +
        @"|\bKeeping this (?:as|predicate)\b|\bKeeping the list\b|\bkeeping them here prevents\b" +
        @"|\bPreserve (?:each|their per-phase)\b|\bRetain the bounded\b|\bReuse the ordinary\b" +
        @"|\bWithout this\b|\bOmitting that\b|\bSkipping this\b|\bReinitializing frame\b" +
        @"|\breplacing it with a main-AI timer\b|\bwould (?:re-lock|invent gameplay|enable boost)\b" +
        @"|\bloses (?:its first tick|the one-frame)\b|\ba stale request\b|\bcorrupt (?:displacement|cyclic" +
        @"|reserve)\b|\beasy to miss\b|\bDespite its traditional\b|\bapplication-owned\b" +
        @"|\bthe old landing movement\b|\bthe game explicitly lists\b|\bexplicitly declines\b" +
        @"|\bexplicitly supplies\b|\bnot claimed\b|\bReachable third-phase\b|\bnot the game's own phases\b" +
        @"|\bC# runtime shell\b|\bframe-steppable\b|\banalyzer SME\d+\b|\bthis consumer now includes\b" +
        @"|\bexplicit producer\b|\bprematurely clears\b|\bminus-one marker\b|\bGenerated from the animations\b" +
        @"|\bwould disagree by one\b|\brather than (?:targeting a saved|a saved perch|by painting" +
        @"|a phase of it|silently)\b|\ba mark beside the action\b|\bthe apparent Y sequence\b" +
        @"|\bReapplying it\b|\baligning the travel axis instead\b|\breplaces the outdoors test\b" +
        @"|\bthe original's countdown\b|\bmomentum-retention setups\b|\bwould make the game read\b" +
        @"|\bthat can diverge\b|\bthe exact enemy-place side effect\b|\bwhose only work nothing reads\b" +
        @"|\bstale dash speed\b|\bstale index\b|\bwould change every climb\b|\bthe second draw writing\b" +
        @"|\bcan consume every\b|\bPuromi/|\bMisc dust\b|\bmisc-dust\b|\bwould change nothing\b" +
        @"|\bnever assign the authored\b|\bexactly as the game does\b|\bexactly where the original\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The animation segment template and the original's drawing history: a segment per animation a spawn, branch or wake
    /// enters, a picture's second showing, what the game read or drew twice and the original's labels (<c>a spawn, branch or
    /// wake enters</c>, <c>the second showing</c>, <c>The game draws the closed</c>).
    /// </summary>
    private static readonly Regex SegmentAndDrawHistoryProse = new(
        @"\ba spawn, branch or [\w ]+ (?:enters|starts)\b|\bas its list shows it\b|\bthe second showing\b" +
        @"|\bThe game read it\b|\bthe game draws it\b|\bThe game draws the closed\b|\bno boss-body part\b" +
        @"|\bstages copied\b|\bcounts past zero\b|\bauthored to last\b|\bauthored length is never shown\b" +
        @"|\bThe game authors\b|\bShitroid actor\b|\bThe Shitroid\b|\bEvery object keeps its own timer\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    /// <summary>Every vocabulary the rule reads, in the order a comment is tested against them.</summary>
    private static readonly ImmutableArray<Regex> Vocabulary =
    [
        StorageRangeProse,
        StorageUnitProse,
        RangeReasoningProse,
        ProgramFlowProse,
        RoutineNameProse,
        InstallationAndColourLoadProse,
        DisplayHardwareProse,
        StorageRecordProse,
        TableAndAllocationProse,
        PortReasoningProse,
        SegmentAndDrawHistoryProse,
    ];

    private static readonly DiagnosticDescriptor Rule = new(
        DocumentationStorageAndHistoryId,
        "Core documentation tells storage, the original program or the port's history",
        "Core documentation tells storage, the original program or the port's history ('{0}'); say what the game does",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core's documentation tells the game: what a thing does, when and how far, never the bytes, bits, words, " +
            "ranges and units the original stored, the original's routines, tables and program flow, the display hardware, " +
            "or the port's own reasoning and history (#2550, docs/core-vocabulary.md).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSyntaxTreeAction(AnalyzeTree);
        });
    }

    /// <summary>Reports each comment of a tree, and each run of line comments, that tells the vocabularies' prose.</summary>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
        Microsoft.CodeAnalysis.Text.SourceText source = context.Tree.GetText(context.CancellationToken);
        var run = new List<SyntaxTrivia>();
        foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: false))
        {
            if (!IsComment(trivia))
                continue;
            if (!CoreDocumentationVocabularyAnalyzer.IsInHardwareOwner(trivia.Token.Parent) &&
                Finding(trivia.ToFullString()) is { } word)
                context.ReportDiagnostic(Diagnostic.Create(Rule, trivia.GetLocation(), word));
            if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                continue;
            if (run.Count > 0 && !CoreDocumentationVocabularyAnalyzer.OnTheNextLine(source, run[run.Count - 1], trivia))
            {
                ReportLineCommentRun(context, run);
                run.Clear();
            }
            run.Add(trivia);
        }
        ReportLineCommentRun(context, run);
    }

    /// <summary>Whether <paramref name="trivia"/> is a line, block or documentation comment.</summary>
    private static bool IsComment(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
        trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
        trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);

    /// <summary>
    /// Reports a run of two or more line comments once when its lines, read as one text, tell the vocabularies' prose in a
    /// phrase that runs from one line onto the next; a phrase within one line is the single comment's, and is not reported
    /// twice.
    /// </summary>
    private static void ReportLineCommentRun(SyntaxTreeAnalysisContext context, List<SyntaxTrivia> run)
    {
        if (run.Count < 2 || CoreDocumentationVocabularyAnalyzer.IsInHardwareOwner(run[0].Token.Parent))
            return;
        var joined = new System.Text.StringBuilder();
        var lineStarts = new List<int>();
        foreach (SyntaxTrivia comment in run)
        {
            if (joined.Length > 0)
                joined.Append(' ');
            lineStarts.Add(joined.Length);
            joined.Append(comment.ToString().Substring(2).Trim());
        }
        string text = joined.ToString();
        foreach (Regex vocabulary in Vocabulary)
        {
            foreach (Match match in vocabulary.Matches(text))
            {
                if (lineStarts.Any(start => start > match.Index && start < match.Index + match.Length))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, run[0].GetLocation(), match.Value));
                    return;
                }
            }
        }
    }

    /// <summary>The first phrase of <paramref name="comment"/>'s prose any vocabulary reads, or null when it has none.</summary>
    private static string? Finding(string comment)
    {
        string text = CoreDocumentationVocabularyAnalyzer.CommentLineBreak.Replace(
            CoreDocumentationVocabularyAnalyzer.Markup.Replace(comment, " "), " ");
        foreach (Regex vocabulary in Vocabulary)
        {
            if (vocabulary.Match(text) is { Success: true } match)
                return match.Value;
        }
        return null;
    }
}
