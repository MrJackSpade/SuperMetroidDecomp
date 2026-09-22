[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$userProfilePath = [Environment]::GetFolderPath(
    [Environment+SpecialFolder]::UserProfile)
$rulesDirectory = Join-Path $userProfilePath '.codex\rules'
$rulesFile = Join-Path $rulesDirectory 'default.rules'
$utf8WithoutBom = [Text.UTF8Encoding]::new($false)

$pattern =
    'pattern = ["gh", "issue", "comment", "--repo", ' +
    '"MrJackSpade/SuperMetroidDecomp"]'
$rule = @'
prefix_rule(
    pattern = ["gh", "issue", "comment", "--repo", "MrJackSpade/SuperMetroidDecomp"],
    decision = "allow",
    justification = "The repository owner authorizes implementation, verification, and diagnostic comments on this project's GitHub issues.",
    match = [
        "gh issue comment --repo MrJackSpade/SuperMetroidDecomp 536 --body-file .codex-issue-comment-536.md",
    ],
    not_match = [
        "gh issue comment --repo another/repository 536 --body summary",
    ],
)
'@

[IO.Directory]::CreateDirectory($rulesDirectory) | Out-Null

$existing = if ([IO.File]::Exists($rulesFile)) {
    [IO.File]::ReadAllText($rulesFile)
}
else {
    ''
}

if ($existing.IndexOf($pattern, [StringComparison]::Ordinal) -ge 0) {
    Write-Host "Codex GitHub issue-comment rule is already installed in $rulesFile"
    Write-Host 'Restart Codex if it has not been restarted since the rule was added.'
    exit 0
}

$separator = if ($existing.Length -eq 0) {
    ''
}
elseif ($existing.EndsWith("`n", [StringComparison]::Ordinal)) {
    "`n"
}
else {
    "`r`n`r`n"
}

[IO.File]::AppendAllText(
    $rulesFile,
    $separator + $rule + "`r`n",
    $utf8WithoutBom)

Write-Host "Installed the scoped Codex GitHub issue-comment rule in $rulesFile"
Write-Host 'Restart Codex so it loads the updated user-level rules.'
