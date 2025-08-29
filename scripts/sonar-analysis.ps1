# Script para ejecutar análisis de SonarQube en Windows (PowerShell)
# Autor: Claude Code Assistant
# Descripción: Ejecuta análisis completo de código con SonarQube

param(
    [string]$ProjectKey = "dotnet-ecuador-api",
    [string]$SonarUrl = "http://localhost:9000",
    [string]$Token = ""
)

Write-Host "🔍 Iniciando análisis de SonarQube para DotNet Ecuador API" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Verificar que SonarQube esté ejecutándose
Write-Host "📡 Verificando conexión a SonarQube..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$SonarUrl/api/system/status" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ SonarQube está ejecutándose correctamente" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Error: SonarQube no está disponible en $SonarUrl" -ForegroundColor Red
    Write-Host "   Ejecuta: docker-compose -f docker-compose.sonarqube.yml up -d" -ForegroundColor Yellow
    exit 1
}

# Verificar .NET SDK
Write-Host "🔧 Verificando .NET SDK..." -ForegroundColor Yellow
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Error: .NET SDK no está instalado" -ForegroundColor Red
    exit 1
}
$dotnetVersion = dotnet --version
Write-Host "✅ .NET SDK versión: $dotnetVersion" -ForegroundColor Green

# Verificar SonarScanner
Write-Host "🔧 Verificando SonarScanner..." -ForegroundColor Yellow
if (-not (Get-Command dotnet-sonarscanner -ErrorAction SilentlyContinue)) {
    Write-Host "📦 Instalando SonarScanner..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}
Write-Host "✅ SonarScanner está disponible" -ForegroundColor Green

# Limpiar build anterior
Write-Host "🧹 Limpiando builds anteriores..." -ForegroundColor Yellow
dotnet clean
Remove-Item -Path "TestResults" -Recurse -ErrorAction SilentlyContinue
Remove-Item -Path ".sonarqube" -Recurse -ErrorAction SilentlyContinue

# Configurar token si se proporciona
$tokenParam = ""
if ($Token -ne "") {
    $tokenParam = "/d:sonar.token=$Token"
    Write-Host "🔑 Usando token de autenticación" -ForegroundColor Green
} else {
    Write-Host "⚠️  Sin token - usando autenticación por defecto (admin/admin)" -ForegroundColor Yellow
}

# Iniciar análisis de SonarQube
Write-Host "🚀 Iniciando análisis de SonarQube..." -ForegroundColor Green
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /d:sonar.host.url="$SonarUrl" `
    $tokenParam `
    /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" `
    /d:sonar.cs.vstest.reportsPaths="TestResults/**/*.trx" `
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/TestResults/**"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al iniciar SonarScanner" -ForegroundColor Red
    exit 1
}

# Restaurar dependencias
Write-Host "📦 Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore

# Compilar proyecto
Write-Host "🔨 Compilando proyecto..." -ForegroundColor Yellow
dotnet build --no-restore --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error en la compilación" -ForegroundColor Red
    exit 1
}

# Ejecutar tests con cobertura
Write-Host "🧪 Ejecutando tests con cobertura..." -ForegroundColor Yellow
dotnet test --no-build --configuration Release `
    --collect:"XPlat Code Coverage" `
    --results-directory TestResults `
    --logger trx `
    --settings coverlet.runsettings

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error en la ejecución de tests" -ForegroundColor Red
}

# Finalizar análisis de SonarQube
Write-Host "📊 Finalizando análisis de SonarQube..." -ForegroundColor Green
dotnet sonarscanner end $tokenParam

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al finalizar SonarScanner" -ForegroundColor Red
    exit 1
}

Write-Host "=================================================" -ForegroundColor Green
Write-Host "✅ Análisis completado exitosamente!" -ForegroundColor Green
Write-Host "📊 Revisar resultados en: $SonarUrl" -ForegroundColor Cyan
Write-Host "🔍 Proyecto: $ProjectKey" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Green