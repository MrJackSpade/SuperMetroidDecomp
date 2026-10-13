[CmdletBinding()]
param()
# Ensures the pinned public reference sources that verification reads exist in this checkout:
#   upstream-disassembly  InsaneFirebat/sm_disassembly (annotated 65816 disassembly)
#   upstream-sm           snesrev/sm (native C reconstruction)
# Pins are recorded in README.md. Both directories are gitignored. A worktree clones from the
# main checkout's copy when one exists, so only the first fetch touches the network.
# Run from the repository root.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$sources = @(
    [pscustomobject]@{ Path = 'upstream-disassembly'; Url = 'https://github.com/InsaneFirebat/sm_disassembly'; Commit = '362be646929cf8e483f692b73a6561cfc2dc1d0d' }
    [pscustomobject]@{ Path = 'upstream-sm';          Url = 'https://github.com/snesrev/sm';                 Commit = '578f90b3cc49557bb70060ad033bb90b8cf8ac50' }
)

# The main checkout is the parent of the shared git directory; worktrees reuse its clones.
$common = (git rev-parse --path-format=absolute --git-common-dir).Trim()
$mainRoot = Split-Path -Parent $common

foreach ($source in $sources) {
    if (Test-Path -LiteralPath (Join-Path $source.Path '.git')) {
        $head = (git -C $source.Path rev-parse HEAD).Trim()
        if ($head -eq $source.Commit) { continue }
    }
    else {
        $local = Join-Path $mainRoot $source.Path
        $origin = if ((Test-Path -LiteralPath (Join-Path $local '.git')) -and
            ((Resolve-Path -LiteralPath $local).Path -ne (Join-Path (Get-Location) $source.Path))) { $local } else { $source.Url }
        git clone --quiet $origin $source.Path
    }
    # A fresh clone already has the commit; an older one may need it fetched first.
    git -C $source.Path cat-file -e "$($source.Commit)^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0) { git -C $source.Path fetch --quiet origin $source.Commit }
    git -C $source.Path checkout --quiet --detach $source.Commit
    "Reference source $($source.Path) at $($source.Commit)"
}
