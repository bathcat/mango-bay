# Test Coverage Report

This project uses [Coverlet](https://github.com/coverlet-coverage/coverlet) for code coverage collection and [ReportGenerator](https://github.com/danielpalme/ReportGenerator) for HTML report generation.

## Quick Start

### First Time Setup

```pwsh
cd projects/api
dotnet tool restore
```

This installs the local tools defined in `.config/dotnet-tools.json`. No global tool installation required!

### Generate Coverage Report

```pwsh
./scripts/coverage.ps1
```

This will:

1. Clean previous coverage data
2. Run all tests with coverage collection
3. Generate an HTML report
4. Open the report in your default browser

## Script Options

```pwsh
./scripts/coverage.ps1 [options]

Options:
  -SkipClean    Keep previous coverage data (faster for iterative work)
  -Build        Build the solution before running tests
  -Verbose      Show detailed test output
  -SkipOpen     Don't open the report in browser (useful for CI)
```

## Examples

```pwsh
./scripts/coverage.ps1 -Build -Verbose

./scripts/coverage.ps1 -SkipClean -SkipOpen

./scripts/coverage.ps1 -Build
```

## Report Location

The HTML report is generated at: `projects/api/coverage-report/index.html`

## CI/CD Usage

For automated builds, use:

```pwsh
./scripts/coverage.ps1 -SkipOpen
```

## What Gets Measured?

The script measures code coverage for:

-   MBC.Core
-   MBC.Endpoints
-   MBC.Persistence
-   MBC.Services
-   MBC.Payments.Client

Test projects and generated code are automatically excluded from coverage calculations.

## Troubleshooting

**No coverage files found?**

-   Ensure `coverlet.collector` is installed in test projects (already configured in `tests/Directory.Build.props`)

**Tool not found?**

-   Run `dotnet tool restore` from the `projects/api` directory

**Tests failing?**

-   Run `dotnet test` to see detailed test failure information
-   Use `./scripts/coverage.ps1 -Verbose` for more test output
