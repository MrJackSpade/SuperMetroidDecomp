# Design Cleanup Ticket Triage Handoff

Date: 2026-09-14

## Current objective

The user is reviewing the project and sending design-pattern concerns as they notice them. For each concern, maintain the GitHub cleanup backlog in `MrJackSpade/SuperMetroidDecomp`.

Do not assume every observation needs a new issue. Treat related observations as additions to an existing cleanup umbrella. Update that issue with the new restriction, example, migration requirement, or acceptance criterion. Create a separate issue only when the concern has a materially different design remedy or requires distinct analyzer logic.

Every issue created through this workflow must have both labels:

- `cleanup`
- `deferred`

The `cleanup` label now exists with description `Refactoring and maintainability cleanup` and color `5319E7`.

When a prohibited pattern can be detected reliably, the issue must explicitly require a new blocking analyzer. The analyzer should run in normal CI/builds at error severity, include positive and negative analyzer tests, avoid false positives for legitimate domain-specific code, and preferably supply a semantics-preserving code fix.

## Active cleanup umbrella

### Issue #620

Title: `Centralize reusable validation in Ensure and enforce it with a blocking analyzer`

URL: https://github.com/MrJackSpade/SuperMetroidDecomp/issues/620

Labels: `cleanup`, `deferred`

The issue requires a shared core `Ensure` API for reusable argument/value validation that can use generic messages. Examples supplied by the user:

```csharp
Ensure.NotNull(value);
Ensure.GreaterThanZero(value);
Ensure.Between(0, 64, value);

checkedValue = Ensure.GreaterThanZero(inputValue);
```

Important settled requirements:

- Each operation returns the validated value.
- Each operation captures the caller's value expression with `CallerArgumentExpression` or an equivalent supported mechanism and uses it as the exception parameter name.
- Generic guards use the semantically appropriate standard argument exception and consistent generic messages.
- The API covers reusable null, sign/zero, range, allowed-set membership, equality, length, and count checks across applicable reference, string, numeric, collection/span, and enum/domain types.
- Boundary semantics must be explicit and tested; `Between` must not have undocumented inclusive/exclusive behavior.
- Domain invariants, corrupt-state checks, and validation requiring bespoke context remain outside `Ensure`.
- Numeric callers must never cast just to fit an `Ensure` signature.
- Provide public overloads for every supported numeric type. Each overload accepts and returns the original type without narrowing, widening, truncating, changing signedness, or obscuring the captured expression. Shared private implementation is allowed only if the public call remains cast-free and type-preserving.
- Migrate eligible guards repository-wide, including `ArgumentNullException.ThrowIfNull` and recognizable handwritten generic guards.
- The motivating example is `csharp/src/SuperMetroid.Core/Rendering/SnesBgTilemapRenderer.cs`, with a parallel reference implementation in `csharp/src/SuperMetroid.Verification/ViewportReference.cs`.
- Add a blocking analyzer for direct `ThrowIfNull` calls and recognizable inline null/sign/range/membership/equality/length/count guards that immediately throw standard argument exceptions.
- Analyzer exclusions must preserve domain-specific validation and custom-context checks.
- Analyzer tests and, where safe, a code fix are required.

If the user supplies more validation-related restrictions or examples, update #620 rather than creating another issue unless the requested rule cannot reasonably live under this API/analyzer umbrella.

## Earlier completed report

Issue #619, `Validate Puromi fire-snake spawns in Lower Norfair Pillar Room`, was created and labeled `bug`:

https://github.com/MrJackSpade/SuperMetroidDecomp/issues/619

Its affected version was updated from unknown to `v0.3.4`, which was the latest published production release when checked. This issue is separate from the design-cleanup stream.

## Resume procedure

For each new user observation:

1. Identify the concrete pattern and locate representative repository examples with `rg` when useful.
2. Search all GitHub issues for an existing matching or umbrella cleanup ticket.
3. Update an existing issue when the remedy belongs under the same abstraction or analyzer policy; otherwise create a new `cleanup` + `deferred` issue.
4. Preserve the user's exact constraints as enforceable requirements and acceptance criteria rather than soft suggestions.
5. Require a blocking analyzer only where detection can be expressed cleanly and with acceptable false-positive behavior.
6. Report the issue number/link and summarize what was added.

GitHub operations have been performed with authenticated `gh` CLI access. No source implementation was requested as part of this ticket-triage task.

## Workspace caution

At handoff time, the worktree already contained unrelated modified and untracked files involving grapple assets, verification work, movement fixtures, and issue-specific fixtures. These changes predated this handoff and belong to other work. Do not modify, stage, discard, or include them in commits for this task.

The repository instructions require commits and pushes for completed repository changes. After every successful push, follow `tools/discord-updates.md` and drain all unposted published commits using `pwsh -File tools/discord-updates.ps1 next` until it returns `caught-up`.
