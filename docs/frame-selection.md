# Frame selection, not tile uploads (SME6445)

Under pre-assembled artwork, every tile upload a Core behaviour performed (a corpse rotting, Crocomire melting, the
Wrecked Ship treadmill turning, the escape timer's digits, a typewriter line, Samus's death sequence) is "show frame N
of this sheet" or "show this part image". This note describes the target shape and the rule that detects what is left
of the upload model (#1406, v0.9.0 manifesto).

## Target shape

Asset extraction produces each animated tile sequence, rot, melt, typewriter and overlay as frames of a sheet or as part
images, with a semantic identity. A behaviour selects the frame or part at the same update it used to queue the
transfer, and the selection is published through the display model. Where the transfer's timing is gameplay-observable
(a selection that lands one update later because the upload waited for the frame boundary), the delay is kept
explicitly at the selection site and named for what it does in the game, not as a queue.

Nothing in Core holds a video-memory destination, a transfer size or source offset, or a list of pending uploads.

## The rule

`TransferWordAnalyzer` reports, in the `SuperMetroid.Core` assembly only, every field (constants included), property,
positional record component and ordinary method whose name contains the camel-case word `Transfer`, `Upload` or
`Destination` and whose type (or return type) holds integers: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`
or `ulong`, a nullable of one, or an array, sequence or dictionary value of them.

Matching is by whole camel-case word, ignoring case, with the same word splitter as the Core vocabulary rule
(`docs/core-vocabulary.md`): `Transfers`, `Transferred`, `Transferring`, `Uploads`, `Uploaded`, `Uploading` and
`Destinations` are the same words, `DMATransferLength` contains `Transfer`, and `Transference`, `Uploader` and
`Destinationless` are not reported.

A member typed as a domain identity is the replacement, not a finding: a `CorpseFrame TransferFrame` or an enum-typed
movement destination is not reported. Parameters and locals are not members; the members that hold them are what the
rule reads. Overrides keep the name of the member they override.

## Exemplar

```csharp
// Before: the behaviour queues a copy of rows of character data into video memory.
internal readonly record struct DeadMonsterVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset, ushort EncodedVramDestination);

// After: the behaviour selects which picture of the corpse is shown on this update.
internal CorpseFrame Frame { get; private set; }
```

## Allowance

There is no allowance list. A site is justified individually only when the reason would not be equally true of a
thousand other sites. No baseline, allowlist, count, severity change or scope change (project manifesto, section 6).

## Status

The rule ships at error severity with the ticket's work list as its findings. Count the findings with the census
(`docs/analyzer-cleanup.md`); the ticket is not done while the census reports any.
