# Core vocabulary (SME6438)

`SuperMetroid.Core` is a C# game. Its identifiers say what things are in the game, never which chip, memory or
cartridge structure the original program stored them in. This is the rule that detects what is left of the machine
(v0.9.0 manifesto, section 5); every v0.9.0 removal ticket drives its words to zero.

## Target shape

A reviewer reading Core cannot tell from its names that it was ported from the SNES. Sprites are frames of an actor's
sheet, not OAM entries; colours are owned colours and colour effects, not CGRAM slots; a backdrop is an image, not a
VRAM plane; a sound request is a request to a sound library, not an APU port write; an identity is a name, not a
pointer or a bank. Hardware behaviour the game genuinely needs (the timing of a sound acknowledgement, how far the door
loader has progressed, a counter that wraps) lives in a compatibility service named for what it does for the game.

## The rule

`CoreVocabularyAnalyzer` reports, in the `SuperMetroid.Core` assembly only, every declared identifier whose camel-case
words include a hardware word: types, type parameters, fields, properties, methods, events, parameters, locals, local
functions, query range variables, namespace segments and file names.

The words, kept in one place in the analyzer (`CoreVocabularyAnalyzer.HardwareWords`):

`Snes`, `Vram`, `Cgram`, `Oam`, `Obj`, `Dma`, `Hdma`, `Nmi`, `Apu`, `Spc`, `Dsp`, `Wram`, `Ppu`, `Native`, `Rom`, `Bank`,
`Pointer`, `Address`, `Cartridge`, `Mode7`, `Register`.

Matching is by whole camel-case word, ignoring case, with plural and verb endings (`Banks`, `Addresses`, `Banked`,
`Addressing`). `Object` is not `Obj`, `Roman` is not `Rom`, `Registry` is not `Register`. A run of capitals is one word
(`OAMBuffer` is `OAM` + `Buffer`), and a digit stays with the word before it (`Mode7`).

The ticket also names `Tilemap` where it means a VRAM plane. That meaning cannot be told from the word itself (a
metatile layer of a room is a tilemap in any remake), so the rule does not list it; the display-model tickets remove the
VRAM-plane uses by replacing the types, and the v0.9.0 validation pass reads for any that remain.

Members that override or implement a member declared outside Core keep the name the base declares. A record's
positional parameter and the property it declares are reported once.

## Exemplar

```csharp
// Before: the identity is a ROM pointer and the colour is a palette slot.
internal static SnesSpritemapPointer KraidHeadPointer => new(0xA7, 0x8DD2);
internal static int CgramIndex(int row, int column) => row * 16 + column;

// After: the identity is a name, and the colour belongs to the thing that owns it.
internal static KraidFrame Head => KraidFrame.Head;
internal OwnedColor ClawGlow { get; }
```

## Allowance

There is no identifier allowance. A type marked `[CompatibilityService]` (#1401) may say in its XML documentation which
hardware behaviour it reproduces (for example "as the SPC port handshake did"); documentation is not an identifier, so
the rule never reads it. Every other occurrence is renamed for the game or removed with the thing it named. No baseline,
allowlist, count, severity change or scope change (project manifesto, section 6).

## Status

The rule is the milestone-wide detector: it ships at error severity with the whole v0.9.0 work list as its findings and
stays red until the last removal lands. Count the findings with the census (`docs/analyzer-cleanup.md`); while it is
red, build verification binaries with `-p:RunAnalyzers=false` and rely on the census for every SME rule.
