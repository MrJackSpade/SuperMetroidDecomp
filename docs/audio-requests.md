# Audio requests (SME6446)

Core asks the audio layer for things in the game's terms. A request names what it wants by a domain identity; it never
carries the port the original program wrote, the address an upload was read from, or the raw byte a library port held
(v0.9.0 manifesto, section 3, "Audio").

## Target shape

The requests are the game's own operations: play a music track, stop the music, queue a sound in a library, change the
music data set. A track, a sound and a music data set are identities: names whose backing values are arbitrary and
read by nothing, decoded once at export by the clip renderer in AssetExtraction. A change of music data set is a
clip-set switch; the downtime the original upload imposed on sound requests is a behaviour of the sound acceptance
clock (`docs/compatibility-layer.md`), not an upload of bytes. When each library accepts a request, and what it reports
back, is the clock's business, written in its own types.

## The rule

`AudioRequestAnalyzer` reports, in the `SuperMetroid.Core` assembly only, every instance field or property of an audio
request whose type is an integral number (or a nullable, array or single-argument collection of one), or an enum named
with the camel-case word `Port`, `Bank` or `Address` (with plurals).

An audio request is a type whose name ends with the camel-case word `Request` or `Command` and that is declared in an
`Audio` namespace or whose name includes `Audio`, `Music` or `Sound`; every type nested in or derived from one is a
request too. A member that overrides another is reported once, at the declaration it overrides. Static members and
constants are not carried by a request and are not read.

## Exemplar

```csharp
// Before: a port write carrying the byte the library port held, and an upload named by its bank.
internal sealed record WritePort(ApuPort InputPort, int Value) : CartridgeAudioCommand;
internal sealed record Upload(AudioBank AudioSet) : CartridgeAudioCommand;

// After: the operation the game asked for, by identity.
public sealed record QueueSound(SoundEffect Sound) : AudioRequest;
public sealed record ChangeMusicDataSet(MusicDataSet DataSet) : AudioRequest;
```

## Allowance

None. A value that is genuinely a quantity of the request (a delay, a queue limit) is a domain value type named for
what it measures, never an integer named to slip past the rule. No baseline, allowlist, count, severity change or scope
change (project manifesto, section 6).
