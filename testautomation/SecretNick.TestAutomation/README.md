# SecretNick Test Automation Framework

.NET 9 + Playwright + Reqnroll BDD framework for API and UI testing (Angular/React frontends).

## Quick Start

### Prerequisites

1. **.NET SDK** (version from `Tests.csproj` `<TargetFramework>`) - https://dotnet.microsoft.com/download
2. **Docker & Docker Compose**
3. **IDE with Reqnroll extension:**
   - **Visual Studio 2022**: Install [Reqnroll for Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=Reqnroll.ReqnrollForVisualStudio2022)
     - Extensions → Manage Extensions → Search "Reqnroll" → Download → Restart
   - **VS Code**: Install [Cucumber extension](https://marketplace.visualstudio.com/items?itemName=CucumberOpen.cucumber-official)
     ```json
     // .vscode/settings.json
     {
       "cucumber.glue": ["testautomation/SecretNick.TestAutomation/Tests/**/*.cs"],
       "cucumber.features": ["testautomation/SecretNick.TestAutomation/Tests/**/*.feature"]
     }
     ```
   - **Rider**: Install Reqnroll plugin from Marketplace → Restart

### Installation

```bash
# Clone repo
git clone <repo-url>
cd <repo-root>

# Restore test packages
dotnet restore testautomation/SecretNick.TestAutomation/Tests/Tests.csproj

# Install Playwright browsers
# Replace net9.0 with your actual TargetFramework (e.g., net8.0, net9.0)
pwsh testautomation/SecretNick.TestAutomation/Tests/bin/Debug/net9.0/playwright.ps1 install chromium
```

### Start Application

```bash
# From repository root, create .env file with your values
cat > .env << EOF
PSQL_USER=admin
PSQL_PASSWORD=your_password
PSQL_DB=itm
CONNECTIONSTRINGS__DBCONNECTIONSTRING=Host=db;Port=5432;Database=itm;Username=admin;Password=your_password
EOF

# Start Docker services (compose.yml is in repo root)
docker compose up -d

# Wait for services
sleep 30
curl http://localhost:8080/health
```

Services run on (default ports, configure in compose.yml):
- Backend API: `http://localhost:8080`
- Angular UI: `http://localhost:8081`
- React UI: `http://localhost:8082`
- PostgreSQL: `localhost:5432`

## Project Structure

```
<repo-root>/
├── compose.yml            # Docker services configuration
├── .env                   # Environment variables
└── testautomation/
    └── SecretNick.TestAutomation/
        ├── SecretNick.TestAutomation.sln
        ├── README.md
        └── Tests/
            ├── Tests.csproj
            ├── ImplicitUsings.cs
            ├── reqnroll.json
            ├── Api/
            │   ├── Clients/      # API clients (Room, User, System)
            │   ├── Models/       # Request/Response DTOs
            │   └── Steps/        # API step definitions
            ├── Ui/
            │   ├── Pages/        # Page Object Models
            │   └── Steps/        # UI step definitions
            ├── Core/
            │   ├── Configuration/ # appsettings.*.json + ConfigManager
            │   └── Drivers/      # BrowserDriver, ApiDriver
            ├── Features/
            │   ├── Api/          # API feature files
            │   └── Ui/           # UI feature files (@ui tag)
            └── Hooks/
                └── TestHooks.cs  # Setup/Teardown + Screenshots
```

## Configuration

### Creating Custom Build Configurations

You can create custom configurations for different environments, browsers, or test setups:

1. **Create configuration file** in `testautomation/SecretNick.TestAutomation/Tests/Core/Configuration/`:
   ```
   appsettings.{YourConfigName}.json
   ```

2. **Define your settings** (example: `appsettings.Staging_Firefox.json`):
   ```json
   {
     "BaseUrls": {
       "Ui": "http://staging.example.com:3000",
       "Api": "http://staging-api.example.com/"
     },
     "Browser": {
       "Type": "firefox",
       "Headless": true,
       "SlowMo": 100,
       "DefaultTimeout": 45000,
       "ViewportWidth": 1920,
       "ViewportHeight": 1080
     },
     "TestRun": {
       "DefaultTimeout": 45000
     },
     "Logging": {
       "Level": "Debug",
       "FilePath": "logs/test-run-{Date}.log"
     }
   }
   ```

3. **Run tests** with your configuration:
   ```bash
   cd testautomation/SecretNick.TestAutomation/Tests
   dotnet test -c Staging_Firefox
   ```

### Configuration Parameters Reference

**BaseUrls:**
- `Ui` - Frontend application URL (Angular/React)
- `Api` - Backend API base URL

**Browser:**
- `Type` - Browser engine: `chromium`, `firefox`, `webkit`
- `Headless` - Run without UI: `true` (CI/CD) or `false` (local debugging)
- `SlowMo` - Delay between actions in ms (0 = no delay, 500 = debug mode)
- `DefaultTimeout` - Maximum wait time for elements/actions in ms
- `ViewportWidth` / `ViewportHeight` - Browser window size in pixels

**TestRun:**
- `DefaultTimeout` - Global timeout for test scenarios in ms

**Logging:**
- `Level` - Log detail: `Information`, `Debug`, `Warning`, `Error`
- `FilePath` - Log file location (supports `{Date}` placeholder)

### Built-in Configurations

| Configuration       | Frontend | Browser  | Headless | URL                   |
|---------------------|----------|----------|----------|-----------------------|
| `Angular_Headless`  | Angular  | Chromium | Yes      | http://localhost:8081 |
| `Angular`           | Angular  | Chromium | No       | http://localhost:8081 |
| `React_Headless`    | React    | Chromium | Yes      | http://localhost:8082 |
| `React`             | React    | Chromium | No       | http://localhost:8082 |

### Configuration Priority (high to low)

1. **CLI parameters** (highest)
2. **Environment variables** (prefix: `TEST_`)
3. **appsettings.{Configuration}.json** (lowest)

### Override via CLI Parameters

```bash
# From repository root
cd testautomation/SecretNick.TestAutomation/Tests

# Override base URL
dotnet test -c Angular_Headless -- NUnit.TestParameter.BaseUrls:Ui=http://192.168.1.100:8081

# Override browser
dotnet test -c React -- NUnit.TestParameter.Browser:Type=firefox

# Disable headless
dotnet test -c Angular_Headless -- NUnit.TestParameter.Browser:Headless=false

# Multiple overrides
dotnet test -c React -- \
  NUnit.TestParameter.BaseUrls:Ui=http://staging.example.com \
  NUnit.TestParameter.Browser:SlowMo=500
```

### Override via Environment Variables

```bash
# Linux/macOS
export TEST_BaseUrls__Ui="http://192.168.1.100:8081"
export TEST_Browser__Headless="false"
export TEST_Browser__Type="firefox"
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test -c Angular_Headless

# Windows PowerShell
$env:TEST_BaseUrls__Ui = "http://192.168.1.100:8081"
$env:TEST_Browser__Headless = "false"
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test -c Angular_Headless
```

**Note:** Use double underscore `__` for nested properties.

### Override via .runsettings File

`custom.runsettings`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <TestRunParameters>
    <Parameter name="BaseUrls:Ui" value="http://staging.example.com" />
    <Parameter name="Browser:Headless" value="true" />
    <Parameter name="Browser:Type" value="firefox" />
  </TestRunParameters>
</RunSettings>
```

```bash
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test -c Angular_Headless --settings custom.runsettings
```

## Running Tests

**Note:** All test commands should be run from `testautomation/SecretNick.TestAutomation/Tests/` directory.

### Run All Tests

```bash
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test -c Angular_Headless
dotnet test -c React
```

### Run API Tests Only

```bash
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test -c Angular_Headless --filter "TestCategory=api"
```

### Run UI Tests Only

```bash
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test -c React --filter "TestCategory=ui"
```

### Filter by Multiple Tags

```bash
cd testautomation/SecretNick.TestAutomation/Tests

# AND operator
dotnet test --filter "TestCategory=api&TestCategory=positive"

# OR operator
dotnet test --filter "TestCategory=done|TestCategory=automated"
```

### Run Specific Feature

```bash
cd testautomation/SecretNick.TestAutomation/Tests
dotnet test --filter "FullyQualifiedName~UserManagement"
```

## Parallelization Configuration

Configure test parallelization in `testautomation/SecretNick.TestAutomation/Tests/ImplicitUsings.cs`:

```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]
```

**Parameters:**
- `ParallelScope.Fixtures` - Test fixtures (feature files) run in parallel, scenarios within features run sequentially
- `ParallelScope.Children` - All tests run in parallel (use with caution for UI tests)
- `LevelOfParallelism(N)` - Maximum N parallel threads (adjust based on CPU cores and resource availability)

**Recommendations:**
- Local: `LevelOfParallelism(2-4)` for smooth debugging
- CI/CD: `LevelOfParallelism(4-8)` for faster execution
- UI tests: Lower parallelism to avoid browser resource conflicts

## Reqnroll Report Configuration

Configure report generation in `testautomation/SecretNick.TestAutomation/Tests/reqnroll.json`:

```json
{
  "language": {
    "feature": "en"
  },
  "bindingCulture": {
    "name": "en-US"
  },
  "trace": {
    "traceSuccessfulSteps": true,
    "traceTimings": true,
    "minTracedDuration": "0:0:0.1",
    "stepDefinitionSkeletonStyle": "RegexAttribute"
  },
  "generator": {
    "allowDebugGeneratedFiles": false,
    "allowRowTests": true,
    "generateAsyncTests": false,
    "addNonParallelizableMarkerForTags": []
  },
  "runtime": {
    "stopAtFirstError": false,
    "missingOrPendingStepsOutcome": "Inconclusive"
  },
  "plugins": [
    {
      "type": "Generator",
      "name": "Reqnroll.xUnit.ReqnrollPlugin"
    }
  ]
}
```

**Key settings:**
- `traceSuccessfulSteps` - Include passed steps in reports (set `false` for shorter reports)
- `traceTimings` - Show step execution times
- `stopAtFirstError` - Stop test run on first failure (`true` for faster debugging)
- `missingOrPendingStepsOutcome` - How to treat unimplemented steps: `Inconclusive`, `Ignore`, `Pending`

**Report generation:**
Reports are generated automatically after test run in:
```
testautomation/SecretNick.TestAutomation/Tests/bin/{Configuration}/net9.0/reqnroll_report.html
```

## CI/CD Pipelines

### Network Configuration for Docker Tests

**CRITICAL:** When running tests in CI/CD pipelines, tests and application containers must communicate. Two approaches:

**Approach 1: Host Network Mode (Recommended for CI/CD)**
```yaml
# In pipeline, start services with host network
docker compose up -d

# Tests access services via localhost
# URLs in appsettings: http://localhost:8080, http://localhost:8081, etc.
```

**Approach 2: Docker Network**
```yaml
# Add tests to same network as application
docker network create test-network
docker compose up -d
docker run --network test-network <test-container>

# In appsettings, use service names:
# http://backend:8080, http://angular:80, http://react:80
```

### GitHub Actions

`.github/workflows/test.yml`:
```yaml
name: E2E Tests

on:
  push:
    branches: [main, develop]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    env:
      PSQL_PASSWORD: "12345"
    strategy:
      fail-fast: false
      matrix:
        config: [Angular_Headless, React_Headless]
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'  # Match your TargetFramework
      
      - name: Create .env
        run: |
          cat > .env << EOF
          PSQL_USER=admin
          PSQL_PASSWORD=$PSQL_PASSWORD
          PSQL_DB=itm
          CONNECTIONSTRINGS__DBCONNECTIONSTRING=Host=db;Port=5432;Database=itm;Username=admin;Password=$PSQL_PASSWORD
          EOF
      
      - name: Start Docker services
        run: |
          docker compose up -d
          sleep 30
      
      - name: Wait for services
        run: |
          timeout 60 bash -c 'until curl -f http://localhost:8080/swagger/index.html; do sleep done'
          timeout 60 bash -c 'until curl -f http://localhost:8081; do sleep 2; done'
          timeout 60 bash -c 'until curl -f http://localhost:8082; do sleep 2; done'
      
      - name: Verify network connectivity
        run: |
          echo "=== Docker containers ==="
          docker compose ps
          echo "=== Backend API ==="
          curl -v http://localhost:8080/swagger/index.html
          echo "=== Angular UI ==="
          curl -I http://localhost:8081
          echo "=== React UI ==="
          curl -I http://localhost:8082
      
      - name: Restore dependencies
        working-directory: testautomation/SecretNick.TestAutomation/Tests
        run: dotnet restore Tests.csproj
      
      - name: Build tests
        working-directory: testautomation/SecretNick.TestAutomation/Tests
        run: dotnet build Tests.csproj -c ${{ matrix.config }} --no-restore
      
      - name: Install Playwright
        working-directory: testautomation/SecretNick.TestAutomation/Tests
        run: |
          # Adjust path based on your TargetFramework (netX.0)
          pwsh bin/${{ matrix.config }}/net9.0/playwright.ps1 install --with-deps chromium
      
      - name: Run tests
        working-directory: testautomation/SecretNick.TestAutomation/Tests
        run: |
          # Tests run on host and access Docker services via localhost
          dotnet test Tests.csproj -c ${{ matrix.config }} --no-build --logger trx
        env:
          # Override URLs if needed (defaults use localhost)
          TEST_BaseUrls__Api: "http://localhost:8080/"
          TEST_BaseUrls__Ui: "http://localhost:${{ matrix.config == 'Angular_Headless' && '8081' || '8082' }}"
      
      - name: Upload artifacts
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results-${{ matrix.config }}
          path: |
            testautomation/SecretNick.TestAutomation/Tests/bin/${{ matrix.config }}/net*/reqnroll_report.html
            testautomation/SecretNick.TestAutomation/Tests/bin/${{ matrix.config }}/net*/Screenshots/
            testautomation/SecretNick.TestAutomation/Tests/TestResults/*.trx
            testautomation/SecretNick.TestAutomation/Tests/logs/
          retention-days: 30
      
      - name: Cleanup
        if: always()
        run: docker compose down -v
```

### Azure DevOps

`azure-pipelines.yml`:
```yaml
trigger:
  branches:
    include: [main, develop]

pool:
  vmImage: 'ubuntu-latest'

strategy:
  matrix:
    Angular:
      buildConfig: 'Angular_Headless'
      frontendPort: '8081'
    React:
      buildConfig: 'React_Headless'
      frontendPort: '8082'

steps:
- task: UseDotNet@2
  inputs:
    version: '9.0.x'  # Match your TargetFramework

- script: |
    cat > .env << EOF
    PSQL_USER=admin
    PSQL_PASSWORD=$(PSQL_PASSWORD)
    PSQL_DB=itm
    CONNECTIONSTRINGS__DBCONNECTIONSTRING=Host=db;Port=5432;Database=itm;Username=admin;Password=$(PSQL_PASSWORD)
    EOF
    docker compose up -d
    sleep 30
  displayName: 'Start services'

- script: |
    timeout 60 bash -c 'until curl -f http://localhost:8080/swagger/index.html; do sleep 2; done'
    timeout 60 bash -c 'until curl -f http://localhost:$(frontendPort); do sleep 2; done'
    echo "=== Services ready ==="
    docker compose ps
    curl -v http://localhost:8080/swagger/index.html
    curl -I http://localhost:$(frontendPort)
  displayName: 'Wait and verify services'

- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
    projects: 'testautomation/SecretNick.TestAutomation/Tests/Tests.csproj'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: 'testautomation/SecretNick.TestAutomation/Tests/Tests.csproj'
    arguments: '-c $(buildConfig) --no-restore'

- script: |
    cd testautomation/SecretNick.TestAutomation/Tests
    pwsh bin/Debug/net*/playwright.ps1 install --with-deps chromium
  displayName: 'Install Playwright'

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'testautomation/SecretNick.TestAutomation/Tests/Tests.csproj'
    arguments: '-c $(buildConfig) --no-build --logger trx'
  displayName: 'Run tests'
  env:
    TEST_BaseUrls__Api: 'http://localhost:8080/'
    TEST_BaseUrls__Ui: 'http://localhost:$(frontendPort)'

- task: PublishTestResults@2
  condition: always()
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/TestResults/*.trx'

- task: PublishBuildArtifacts@1
  condition: always()
  inputs:
    PathtoPublish: 'testautomation/SecretNick.TestAutomation/Tests/bin/$(buildConfig)/net*/'
    ArtifactName: 'test-results-$(buildConfig)'

- script: docker compose down -v
  condition: always()
  displayName: 'Cleanup'
```

### AWS CodeBuild

`buildspec.yml`:
```yaml
version: 0.2

env:
  variables:
    PSQL_USER: admin
    PSQL_DB: itm
  secrets-manager:
    PSQL_PASSWORD: "your-secret-name:password-key"

phases:
  pre_build:
    commands:
      - wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
      - chmod +x dotnet-install.sh
      - ./dotnet-install.sh --channel 9.0
      - export PATH="$HOME/.dotnet:$PATH"
      
      - |
        cat > .env << EOF
        PSQL_USER=$PSQL_USER
        PSQL_PASSWORD=$PSQL_PASSWORD
        PSQL_DB=$PSQL_DB
        CONNECTIONSTRINGS__DBCONNECTIONSTRING=Host=db;Port=5432;Database=$PSQL_DB;Username=$PSQL_USER;Password=$PSQL_PASSWORD
        EOF
      
      - docker compose up -d
      - sleep 30
      - timeout 60 bash -c 'until curl -f http://localhost:8080/swagger/index.html; do sleep 2; done'
      - timeout 60 bash -c 'until curl -f http://localhost:8081; do sleep 2; done'
      - timeout 60 bash -c 'until curl -f http://localhost:8082; do sleep 2; done'
      - |
        echo "=== Docker services status ==="
        docker compose ps
        echo "=== Testing connectivity ==="
        curl -v http://localhost:8080/swagger/index.html
        curl -I http://localhost:8081
        curl -I http://localhost:8082
  
  build:
    commands:
      - cd testautomation/SecretNick.TestAutomation/Tests
      - dotnet restore Tests.csproj
      - dotnet build Tests.csproj -c Angular_Headless --no-restore
      - pwsh bin/Debug/net*/playwright.ps1 install --with-deps chromium
      - |
        # Tests access services via localhost
        export TEST_BaseUrls__Api="http://localhost:8080/"
        export TEST_BaseUrls__Ui="http://localhost:8081"
        dotnet test Tests.csproj -c Angular_Headless --no-build --logger trx
  
  post_build:
    commands:
      - cd $CODEBUILD_SRC_DIR
      - docker compose down -v

artifacts:
  files:
    - 'testautomation/SecretNick.TestAutomation/Tests/bin/Angular_Headless/net*/reqnroll_report.html'
    - 'testautomation/SecretNick.TestAutomation/Tests/bin/Angular_Headless/net*/Screenshots/**/*'
    - 'testautomation/SecretNick.TestAutomation/Tests/TestResults/**/*.trx'
    - 'testautomation/SecretNick.TestAutomation/Tests/logs/**/*'
  name: test-results-$(date +%Y%m%d-%H%M%S)

reports:
  test-results:
    files: ['testautomation/SecretNick.TestAutomation/Tests/TestResults/**/*.trx']
    file-format: 'VisualStudioTrx'
```

## Reports & Artifacts

### Generated Artifacts

After test execution (replace `netX.0` with your TargetFramework version):
```
testautomation/SecretNick.TestAutomation/Tests/bin/{Configuration}/netX.0/
├── reqnroll_report.html          # HTML test report
├── Screenshots/                   # Failure screenshots (UI tests only)
│   └── {ScenarioName}_{Timestamp}.png
└── logs/
    └── test-run-{Date}.log       # Execution logs
```

### Viewing Reports Locally

```bash
# From repository root

# Open HTML report (adjust netX.0 to your version)
start testautomation/SecretNick.TestAutomation/Tests/bin/Angular_Headless/net9.0/reqnroll_report.html  # Windows
open testautomation/SecretNick.TestAutomation/Tests/bin/Angular_Headless/net9.0/reqnroll_report.html   # macOS
xdg-open testautomation/SecretNick.TestAutomation/Tests/bin/Angular_Headless/net9.0/reqnroll_report.html  # Linux

# View logs
cat testautomation/SecretNick.TestAutomation/Tests/logs/test-run-*.log
```

## Troubleshooting

**Port conflicts:**
```bash
# From repository root
docker compose down -v
# Change ports in compose.yml if needed
```

**Tests can't reach application in CI/CD:**
```bash
# Verify containers are running
docker compose ps

# Check container logs
docker compose logs backend
docker compose logs angular
docker compose logs react

# Test connectivity from host
curl http://localhost:8080/swagger/index.html
curl http://localhost:8081
curl http://localhost:8082

# Ensure appsettings use correct URLs (localhost:PORT in CI/CD)
```

**Playwright browser missing:**
```bash
cd testautomation/SecretNick.TestAutomation/Tests
# Adjust path to your TargetFramework version (netX.0)
pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps chromium
```

**Config not loading:**
- Verify `appsettings.{Configuration}.json` exists in `bin/{Configuration}/netX.0/`
- Check `Tests.csproj` has `<Content Include>` for config files
- Rebuild solution: `dotnet build -c {Configuration}`

**Tests hang:**
- Check `DefaultTimeout` in appsettings
- Verify application is accessible: `curl http://localhost:8080/swagger/index.html`
- Check Docker container logs: `docker compose logs`

**Docker services not starting:**
```bash
# From repository root
docker compose down -v
docker compose up -d
docker compose ps  # Check status
docker compose logs  # View logs
```

**Network issues in Docker:**
```bash
# Inspect Docker networks
docker network ls
docker network inspect bridge

# Check if containers are on same network
docker inspect <container-name> | grep NetworkMode
```
