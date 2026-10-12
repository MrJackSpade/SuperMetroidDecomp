# Ensure validation

Use `SuperMetroid.Core.Ensure` for reusable caller-supplied value checks whose failure
can be explained by a generic argument exception.

```csharp
using SuperMetroid.Core;

this.bus = Ensure.NotNull(bus);
Ensure.AtLeastZero(byteCount);
index = Ensure.BetweenInclusive(0, 7, index);
Ensure.LengthEqual(colors, 16);
```

Every operation returns its input value. `CallerArgumentExpression` supplies the
parameter name from the *value expression*, including a property such as
`parts.Clouds`. Do not manually write a parameter name or cast a numeric input to
fit an overload.

The operations are `NotNull` (reference types), `AtLeastZero` (`int` and `long`),
`BetweenInclusive` (`int`, both endpoints included) and `LengthEqual` (anything
convertible to a read-only span). Add an operation or overload only with a
type-preserving return value, caller-expression capture and focused verification.

Keep parser errors, corrupt-state checks, cartridge invariants, and validation
requiring domain context at the owning boundary. Such checks often use
`InvalidDataException`, `InvalidOperationException`, or a detailed diagnostic
message and do not become `Ensure` calls merely because they compare numbers.

## Analyzer

`SME6201` reports standard argument throw helpers (`ThrowIfNull`, `ThrowIfNegative`)
and simple immediate-throw null, sign, range and length guards on a visible member's
parameters. `SME6202` reports an `Ensure` call that passes its parameter name by hand
or casts the validated value to fit an overload. Both are build errors (#677). The
analyzer deliberately leaves compound conditions, private routines' state checks and
custom state or parser exceptions alone.

The IDE code fix rewrites each recognized guard to its `Ensure` operation when the
value's type fits exactly, and removes a manual parameter name. A value that does not
fit (for example a `long` range or an unconstrained generic) stays diagnostic-only:
add the missing typed overload instead of casting.

When a new reusable rule appears, add the operation to `Ensure` with a
type-preserving return value, caller-expression capture, documented boundaries,
and focused verification. Then extend the analyzer's semantic guard matcher and
its positive and negative fixtures. Do not add a text-only rule that matches
unrelated domain behavior.

Run `dotnet run --project csharp/src/SuperMetroid.EnsureVerification --no-restore`
to verify the API and analyzer. The analyzer project uses the Roslyn assemblies
shipped with the selected .NET SDK, so no new NuGet package is required.
