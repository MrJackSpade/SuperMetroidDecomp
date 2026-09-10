[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$signed = -not [string]::IsNullOrWhiteSpace($env:ANDROID_KEYSTORE_BASE64)
if (-not $signed -and $env:RELEASE_EVENT -eq 'push') {
    throw 'Tagged APK releases require the ANDROID_KEYSTORE_BASE64 and Android signing secrets. See docs/releases.md.'
}
$signingArguments = @()
$keyFile = Join-Path ([IO.Path]::GetTempPath()) ('supermetroid-signing-' + [guid]::NewGuid().ToString('N') + '.keystore')
try {
    if ($signed) {
        foreach ($name in 'ANDROID_KEY_ALIAS', 'ANDROID_KEY_PASSWORD', 'ANDROID_STORE_PASSWORD') {
            if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) { throw "Missing secret $name." }
        }
        [IO.File]::WriteAllBytes($keyFile, [Convert]::FromBase64String($env:ANDROID_KEYSTORE_BASE64))
        $signingArguments = @('-p:AndroidKeyStore=true', "-p:AndroidSigningKeyStore=$keyFile",
            "-p:AndroidSigningKeyAlias=$env:ANDROID_KEY_ALIAS", '-p:AndroidSigningKeyPass=env:ANDROID_KEY_PASSWORD',
            '-p:AndroidSigningStorePass=env:ANDROID_STORE_PASSWORD')
    }
    # Manual validation may use an ephemeral debug key; tag publication never does.
    dotnet publish csharp/src/SuperMetroid.Android -c Release -f net10.0-android `
        "-p:ApplicationDisplayVersion=$env:RELEASE_VERSION" "-p:ApplicationVersion=$env:RELEASE_CODE" `
        "-p:AndroidSdkDirectory=$env:ANDROID_HOME" "-p:JavaSdkDirectory=$env:JAVA_HOME" @signingArguments
    $apk = 'csharp/src/SuperMetroid.Android/bin/Release/net10.0-android/android-arm64/publish/org.supermetroid.csharp.testing-Signed.apk'
    python tools/release.py package-android $apk out/artifacts
}
finally {
    # Only the exact temporary file created above is removed, never a supplied keystore.
    if (Test-Path -LiteralPath $keyFile) { Remove-Item -LiteralPath $keyFile }
}
