# Ensure validation

Use `SuperMetroid.Core.Ensure` for reusable caller-supplied value checks whose failure
can be explained by a generic argument exception.

```csharp
using SuperMetroid.Core;

width = Ensure.GreaterThanZero(width);
tilemapWidthInTiles = Ensure.OneOf(tilemapWidthInTiles, 32, 64);
output = Ensure.LengthEqual(output, checked(width * height));
mode = Ensure.IsDefined(options.MapReveal);
```

Every operation returns its input value. `CallerArgumentExpression` supplies the
parameter name from the *value expression*, including a property or indexer such
as `options.MapReveal` or `options.Modes[0]`. Do not manually write `nameof(options)`
or cast a numeric input to fit an overload.

`Between` and `BetweenInclusive` include both endpoints. `BetweenExclusive`
excludes both. All comparison operations reject NaN; their return type is the
same as the numeric input type. The public numeric overloads cover signed and
unsigned integral types through 128 bits, native integers, `BigInteger`, `Half`,
`float`, `double`, and `decimal`.
`IsDefined<TEnum>` uses the typed `Enum.IsDefined` operation and is for ordinary
closed enums. It rejects unnamed `[Flags]` combinations; do not apply it to a
flags domain without proving that exact rule is intended.

Keep parser errors, corrupt-state checks, cartridge invariants, and validation
requiring domain context at the owning boundary. Such checks often use
`InvalidDataException`, `InvalidOperationException`, or a detailed diagnostic
message and do not become `Ensure` calls merely because they compare numbers.

## Analyzer

`SME6201` suggests an `Ensure` operation for standard argument throw helpers
and simple immediate-throw null, numeric, enum, allowed-value, equality, and
length/count guards. `SME6202` catches redundant casts or manual parameter names
in `Ensure.IsDefined` calls. Both are warnings while the existing guards are
migrated; `WarningsNotAsErrors` keeps them advisory despite the repository's
general warnings-as-errors setting. The analyzer deliberately leaves compound
conditions and custom state or parser exceptions alone.

When a new reusable rule appears, add the operation to `Ensure` with a
type-preserving return value, caller-expression capture, documented boundaries,
and focused verification. Then extend the analyzer's semantic guard matcher and
its positive and negative fixtures. Do not add a text-only rule that matches
unrelated domain behavior.

Run `dotnet run --project csharp/src/SuperMetroid.EnsureVerification --no-restore`
to verify the API and analyzer. The analyzer project uses the Roslyn assemblies
shipped with the selected .NET SDK, so no new NuGet package is required.
