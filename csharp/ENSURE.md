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

## Adding an operation

When a new reusable rule appears, add the operation to `Ensure` with a
type-preserving return value, caller-expression capture, documented boundaries,
and focused verification. Do not add a text-only rule that matches unrelated domain behavior.
