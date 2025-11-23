param(
    [switch]$DryRun
)

$branches = @(
    "feature/http-only-cookies",
    "vulnerability/idor",
    "vulnerability/path-traversal",
    "vulnerability/sql-injection",
    "vulnerability/xss-dom",
    "vulnerability/xss-persistent"
)

function Invoke-GitCommand {
    param([string]$Command)
    
    if ($DryRun) {
        Write-Host "[DRY RUN] $Command" -ForegroundColor Yellow
        return 0
    }
    else {
        Write-Host "$ $Command" -ForegroundColor DarkGray
        $parts = $Command -split " "
        $cmd = $parts[0]
        $cmdArgs = $parts[1..($parts.Length-1)]
        $p = Start-Process -FilePath $cmd -ArgumentList $cmdArgs -Wait -PassThru -NoNewWindow
        return $p.ExitCode
    }
}

$currentBranch = git rev-parse --abbrev-ref HEAD

Write-Host "Starting rebase workflow..." -ForegroundColor Cyan
if ($DryRun) {
    Write-Host "*** DRY RUN MODE - No commands will be executed ***" -ForegroundColor Yellow
}
Write-Host "Current branch: $currentBranch" -ForegroundColor Yellow

foreach ($branch in $branches) {
    Write-Host "`nProcessing: $branch" -ForegroundColor Cyan
    
    $exitCode = Invoke-GitCommand "git checkout $branch"
    if ($exitCode -ne 0) {
        Write-Host "Failed to checkout $branch" -ForegroundColor Red
        continue
    }
    
    if ($DryRun) {
        Write-Host "[DRY RUN] git rebase main" -ForegroundColor Yellow
    }
    else {
        Write-Host "$ git rebase main" -ForegroundColor DarkGray
        $rebaseOutput = git rebase main 2>&1
        $exitCode = $LASTEXITCODE
        Write-Host $rebaseOutput
        
        if ($exitCode -ne 0) {
            Write-Host "Rebase conflict on $branch - stopping here" -ForegroundColor Red
            Write-Host "Resolve conflicts, then run: git rebase --continue" -ForegroundColor Yellow
            exit 1
        }
    }
    
    $exitCode = Invoke-GitCommand "git push origin $branch --force-with-lease"
    if ($exitCode -ne 0) {
        Write-Host "Failed to push $branch" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✓ Completed $branch" -ForegroundColor Green
}

Invoke-GitCommand "git checkout $currentBranch"
Write-Host "`nAll branches rebased and pushed!" -ForegroundColor Green
Write-Host "Returned to: $currentBranch" -ForegroundColor Yellow
