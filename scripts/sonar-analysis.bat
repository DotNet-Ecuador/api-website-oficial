@echo off
REM Script para ejecutar análisis de SonarQube en Windows (CMD)
REM Autor: Claude Code Assistant
REM Descripción: Ejecuta análisis completo de código con SonarQube

setlocal EnableDelayedExpansion

set PROJECT_KEY=dotnet-ecuador-api
set SONAR_URL=http://localhost:9000
set TOKEN=

echo 🔍 Iniciando análisis de SonarQube para DotNet Ecuador API
echo =================================================

REM Verificar que SonarQube esté ejecutándose
echo 📡 Verificando conexión a SonarQube...
curl -s --connect-timeout 10 "%SONAR_URL%/api/system/status" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ SonarQube está ejecutándose correctamente
) else (
    echo ❌ Error: SonarQube no está disponible en %SONAR_URL%
    echo    Ejecuta: docker-compose -f docker-compose.sonarqube.yml up -d
    exit /b 1
)

REM Verificar .NET SDK
echo 🔧 Verificando .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Error: .NET SDK no está instalado
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET SDK versión: !DOTNET_VERSION!

REM Verificar SonarScanner
echo 🔧 Verificando SonarScanner...
dotnet-sonarscanner --version >nul 2>&1
if %errorlevel% neq 0 (
    echo 📦 Instalando SonarScanner...
    dotnet tool install --global dotnet-sonarscanner
)
echo ✅ SonarScanner está disponible

REM Limpiar build anterior
echo 🧹 Limpiando builds anteriores...
dotnet clean >nul
if exist TestResults rmdir /s /q TestResults
if exist .sonarqube rmdir /s /q .sonarqube

REM Configurar token si se proporciona
set TOKEN_PARAM=
if not "%TOKEN%"=="" (
    set TOKEN_PARAM=/d:sonar.token=%TOKEN%
    echo 🔑 Usando token de autenticación
) else (
    echo ⚠️  Sin token - usando autenticación por defecto (admin/admin)
)

REM Iniciar análisis de SonarQube
echo 🚀 Iniciando análisis de SonarQube...
dotnet sonarscanner begin ^
    /k:"%PROJECT_KEY%" ^
    /d:sonar.host.url="%SONAR_URL%" ^
    %TOKEN_PARAM% ^
    /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" ^
    /d:sonar.cs.vstest.reportsPaths="TestResults/**/*.trx" ^
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/TestResults/**"

if %errorlevel% neq 0 (
    echo ❌ Error al iniciar SonarScanner
    exit /b 1
)

REM Restaurar dependencias
echo 📦 Restaurando dependencias...
dotnet restore

REM Compilar proyecto
echo 🔨 Compilando proyecto...
dotnet build --no-restore --configuration Release

if %errorlevel% neq 0 (
    echo ❌ Error en la compilación
    exit /b 1
)

REM Ejecutar tests con cobertura
echo 🧪 Ejecutando tests con cobertura...
dotnet test --no-build --configuration Release ^
    --collect:"XPlat Code Coverage" ^
    --results-directory TestResults ^
    --logger trx

if %errorlevel% neq 0 (
    echo ⚠️ Algunos tests fallaron, continuando con el análisis...
)

REM Finalizar análisis de SonarQube
echo 📊 Finalizando análisis de SonarQube...
dotnet sonarscanner end %TOKEN_PARAM%

if %errorlevel% neq 0 (
    echo ❌ Error al finalizar SonarScanner
    exit /b 1
)

echo =================================================
echo ✅ Análisis completado exitosamente!
echo 📊 Revisar resultados en: %SONAR_URL%
echo 🔍 Proyecto: %PROJECT_KEY%
echo =================================================

endlocal