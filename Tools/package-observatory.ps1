param([switch]$PrepareOnly)
. "$PSScriptRoot/common.ps1"
$release=Join-Path $script:Root 'Builds/Windows/Observatory-Release'
Copy-Item -LiteralPath (Join-Path $script:Root 'Design/CelestialMaker.ico') -Destination $release
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ObservatoryPerformance/Create Desktop Shortcut.ps1') -Destination $release
$build=Get-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/build-release.json') -Raw | ConvertFrom-Json
$player=Get-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/player-headless.json') -Raw | ConvertFrom-Json
if(!$player.pass -or $player.version -ne '2.8.2'){throw 'A passing headless release check is required'}
if($build.result -ne 'Succeeded' -or $build.errors -ne 0 -or !(Test-Path -LiteralPath (Join-Path $release 'CelestialMaker.exe'))){throw 'Successful Observatory build required'}
@'
Celestial Maker 2.8.2 - Observatory

Extract this entire folder and run CelestialMaker.exe.
Create Desktop Shortcut.ps1 creates a shortcut with the original Celestial icon.
The executable also contains the icon. Keep the ICO beside the executable.
The game interface and number formatting use English.
Your existing Celestial Maker saves are loaded from the same local profile.

New in 2.8.2: distinct, smoothly increasing research prices and chapter-local
process upgrades. Building prices separate the entry process from advanced units.
Thin purchase bars are yellow while saving, green when the actual purchase can
execute. Buildings respect reserves; research still requires prerequisites.
The Research view suggests one next discovery. Automation introduces Energy,
Matter and Time plans in plain language. Existing owned items and saves remain.

New in 2.8.1: smoother UI presentation with cached orbital scenery and viewport
visibility, a stable pinned-goal status, and an optional five-step autobuyer lesson
on a separate practice economy. Reopen the lesson in Automation at any time.
Your first Prestige and the first recovery Prestige after Ascension/Genesis earn
one point at a lower lifetime-production threshold. Packages still cost points;
no free buyer is granted. Major resets start with stronger basic production.
The first project stage is four hours of base work; later stages retain the
geometric curve. Existing work and saves are preserved when loaded.

New in 2.8: chapter autobuyers purchased with Prestige Points, brighter UI,
animated Overview, continuous production and progressive building/navigation
discovery. Each chapter buyer costs 1 point, survives Prestige and resets on
Ascension/Genesis. Older free buy flags no longer grant automation.

Also included: engineering modules, goal sequences, six-hour plan comparisons,
varied challenge rules and readiness trials, expedition discoveries, universe
specializations, repeatable practice lessons and a cosmic chronicle. Optional
endgame scenarios use separate local save slots. See Automation and Cosmos.

Live resource counters update every rendered frame (60 FPS default, 120 optional).
High-resolution text atlases and art scale down; bitmap art never scales up.
Use the left rail for Overview, Production, Development, Automation, Cosmos
and Journal. Settings includes text scaling (100-200%), Night/Dawn appearance,
reduced motion, save export and import. Existing 80% settings remain supported.
Center view tabs have individual accent colors and crisp pictograms. The current
view also has a bold label and underline; the same cues appear in compact Views.
Top-bar resources have stable colors, small vector icons and tinted meter fields.
Their names remain visible, and live amounts use high-contrast neutral text.
Undiscovered resources and reset currencies stay hidden. Reset points appear
when their first reset is available or a save records prior discovery, and
remain visible after spending them. Locked resets still explain their next goal.
Research and Upgrades use compact rows: click a name to expand details, compare
production, favorite it or pin a goal. Requirements and approximate waiting times
use your selected economy. Ctrl+K opens search; Alt+1-6 switches categories;
F1 shows shortcuts. Tab selects a control; Enter/Space activates it; Escape closes
a dialog. Narrow windows expose navigation through Menu / Find and Views.

Filters, searches and favorites are saved. Pin one research/upgrade goal for an
in-game readiness notification. Turn notifications off in Settings if desired.
Research plans include prerequisites and are saved per economy. Automatic queue
execution requires Temporal workshop and must be enabled explicitly. It respects
reserves and construction funding and pauses after resets. Pending prerequisites
are restored after a reset. Manual planning is available from the start.

Production continues in the background. Review automatic reset rules before
enabling them. Observatory diagnostics explain bottlenecks and link to their source.
Flows trace producers, protected stock and consumers. Context guides remain available.
Save complete operations profiles, compare strategic focus and forecast reset recovery
on a private simulation copy. Settings includes local restore points (12 retained).
The cosmos illustration and earned appearances follow real discoveries and projects.

After your first Prestige, Automation > Development Plan enables a persistent
Energy, Matter or Time strategy for six-hour visits. It manages routing, development
points, chapter autobuyers, focus, laws and construction, and repeats discovered
resets. After higher resets it can repeat Prestige to fund paid buyers.
The first Prestige, Ascension and Genesis remain your decision. Existing saves
start with the plan OFF. Stop the plan before manually managing its settings.
Guided routing now restores affordable basic research when no queue is pending.

Offline time replays every 1/60-second tick, including automation and parallel
economies. A progress counter shows catch-up; long absences take real processing
time. The pending cursor survives save/reload with no absence cap. A restore point
is created before catch-up; automatic resets during catch-up share that point.

Scripted active, hourly and six-hour strategies with Development Plan target
roughly 400 simulated hours. Visit frequency, chosen strategy and reserves matter.
These are simulation results, not a promise for every player.
'@ | Set-Content -LiteralPath (Join-Path $release 'README.txt') -Encoding utf8
if($PrepareOnly){return}
$zipPath=Join-Path $script:Root 'Builds/CelestialMaker-Observatory-Windows.zip'
$parts=@(Get-ChildItem -LiteralPath $release | Where-Object {$_.Name -notlike '*BackUpThisFolder*'} | ForEach-Object {$_.FullName})
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream=[IO.File]::Open($zipPath,[IO.FileMode]::Create)
$archive=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create,$false)
try{
 foreach($part in $parts){$item=Get-Item -LiteralPath $part;$files=if($item.PSIsContainer){Get-ChildItem -LiteralPath $part -File -Recurse}else{@($item)}
  foreach($file in $files){$relative=$file.FullName.Substring($release.Length+1).Replace('\','/');[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$relative,[IO.Compression.CompressionLevel]::Optimal)|Out-Null}
 }
}finally{$archive.Dispose();$stream.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
try {
 $names=@($zip.Entries | ForEach-Object FullName)
 foreach($required in @('CelestialMaker.exe','CelestialMaker.ico','Create Desktop Shortcut.ps1','UnityPlayer.dll','CelestialMaker_Data/Managed/CelestialMaker.Runtime.dll')){if($names -notcontains $required){throw "Missing payload: $required"}}
 $verified=0
 foreach($entry in $zip.Entries){if(!$entry.Name){continue};$candidate=Join-Path $release $entry.FullName;$sourceHash=(Get-FileHash -LiteralPath $candidate -Algorithm SHA256).Hash;$stream=$entry.Open();$sha=[Security.Cryptography.SHA256]::Create();try{$packedHash=([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','')}finally{$sha.Dispose();$stream.Dispose()};if($sourceHash -ne $packedHash){throw "Archive content differs: $($entry.FullName)"};$verified++}
}finally{$zip.Dispose()}
$before=Get-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/saves-before.json') -Raw | ConvertFrom-Json
$saves=@($before | ForEach-Object {[pscustomobject]@{path=$_.path;unchanged=((Get-FileHash -LiteralPath $_.path).Hash -eq $_.sha256)}})
if(@($saves | Where-Object {!$_.unchanged}).Count){throw 'User saves changed during this task; inspect before delivery'}
$saves | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/save-integrity.json')
[pscustomobject]@{version='2.8.2';path=$zipPath;bytes=(Get-Item -LiteralPath $zipPath).Length;sha256=(Get-FileHash -LiteralPath $zipPath).Hash;verifiedEntries=$verified;userSavesUnchanged=$saves.Count;build=$build.result;errors=$build.errors;desktopControlled=$false;screenCaptured=$false;playerLaunched=$true;visiblePlayerLaunched=$false;graphicsEnabled=$false;headlessPlayerPassed=$player.pass;visual='OBSERVED: offscreen Unity URP UI fixtures; desktop interaction and FPS unverified';utc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/artifact.json')
