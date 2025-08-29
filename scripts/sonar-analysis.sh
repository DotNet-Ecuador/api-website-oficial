#!/bin/bash
# Script para ejecutar análisis de SonarQube en Linux/macOS
# Autor: Claude Code Assistant
# Descripción: Ejecuta análisis completo de código con SonarQube

set -e

# Configuración por defecto
PROJECT_KEY="dotnet-ecuador-api"
SONAR_URL="http://localhost:9000"
TOKEN=""

# Función para mostrar ayuda
show_help() {
    echo "Uso: $0 [opciones]"
    echo "Opciones:"
    echo "  -k, --project-key  Clave del proyecto SonarQube (default: $PROJECT_KEY)"
    echo "  -u, --url          URL de SonarQube (default: $SONAR_URL)"  
    echo "  -t, --token        Token de autenticación de SonarQube"
    echo "  -h, --help         Mostrar esta ayuda"
    exit 0
}

# Procesar argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        -k|--project-key)
            PROJECT_KEY="$2"
            shift 2
            ;;
        -u|--url)
            SONAR_URL="$2"
            shift 2
            ;;
        -t|--token)
            TOKEN="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            ;;
        *)
            echo "Opción desconocida: $1"
            show_help
            ;;
    esac
done

echo "🔍 Iniciando análisis de SonarQube para DotNet Ecuador API"
echo "================================================="

# Verificar que SonarQube esté ejecutándose
echo "📡 Verificando conexión a SonarQube..."
if curl -s --connect-timeout 10 "$SONAR_URL/api/system/status" > /dev/null; then
    echo "✅ SonarQube está ejecutándose correctamente"
else
    echo "❌ Error: SonarQube no está disponible en $SONAR_URL"
    echo "   Ejecuta: docker-compose -f docker-compose.sonarqube.yml up -d"
    exit 1
fi

# Verificar .NET SDK
echo "🔧 Verificando .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK no está instalado"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK versión: $DOTNET_VERSION"

# Verificar SonarScanner
echo "🔧 Verificando SonarScanner..."
if ! command -v dotnet-sonarscanner &> /dev/null; then
    echo "📦 Instalando SonarScanner..."
    dotnet tool install --global dotnet-sonarscanner
fi
echo "✅ SonarScanner está disponible"

# Limpiar build anterior
echo "🧹 Limpiando builds anteriores..."
dotnet clean
rm -rf TestResults
rm -rf .sonarqube

# Configurar token si se proporciona
TOKEN_PARAM=""
if [[ -n "$TOKEN" ]]; then
    TOKEN_PARAM="/d:sonar.token=$TOKEN"
    echo "🔑 Usando token de autenticación"
else
    echo "⚠️  Sin token - usando autenticación por defecto (admin/admin)"
fi

# Iniciar análisis de SonarQube
echo "🚀 Iniciando análisis de SonarQube..."
dotnet sonarscanner begin \
    /k:"$PROJECT_KEY" \
    /d:sonar.host.url="$SONAR_URL" \
    $TOKEN_PARAM \
    /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" \
    /d:sonar.cs.vstest.reportsPaths="TestResults/**/*.trx" \
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/TestResults/**"

# Restaurar dependencias
echo "📦 Restaurando dependencias..."
dotnet restore

# Compilar proyecto
echo "🔨 Compilando proyecto..."
dotnet build --no-restore --configuration Release

# Ejecutar tests con cobertura
echo "🧪 Ejecutando tests con cobertura..."
dotnet test --no-build --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger trx || echo "⚠️ Algunos tests fallaron, continuando con el análisis..."

# Finalizar análisis de SonarQube
echo "📊 Finalizando análisis de SonarQube..."
dotnet sonarscanner end $TOKEN_PARAM

echo "================================================="
echo "✅ Análisis completado exitosamente!"
echo "📊 Revisar resultados en: $SONAR_URL"
echo "🔍 Proyecto: $PROJECT_KEY"
echo "================================================="