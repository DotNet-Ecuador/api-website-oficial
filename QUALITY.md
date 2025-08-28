# Guía de Calidad de Código - DotNet Ecuador API

## 📋 Resumen

Este proyecto implementa un sistema completo de control de calidad de código con **pre-commit hooks** que garantizan estándares profesionales de nivel empresarial.

## 🛠️ Herramientas de Calidad Implementadas

### Análisis Estático
- **StyleCop.Analyzers**: Análisis de estilo y convenciones de C#
- **Microsoft.CodeAnalysis.NetAnalyzers**: Análisis de código oficial de Microsoft
- **SonarAnalyzer.CSharp**: Detección de code smells y vulnerabilidades de seguridad

### Formateo y Estándares
- **.editorconfig**: Configuración consistente de estilo de código
- **Directory.Build.props**: Configuración global de análisis
- **quality-rules.ruleset**: Reglas personalizadas para el proyecto

### Cobertura de Código
- **coverlet.collector**: Recolección de métricas de cobertura
- **ReportGenerator**: Generación de reportes HTML de cobertura

## 🚀 Comandos Disponibles

### Ejecutar Checks de Calidad Manualmente

```bash
# Versión completa (Windows PowerShell)
pwsh ./scripts/pre-commit-checks.ps1

# Versión completa (Linux/macOS/WSL)
./scripts/pre-commit-checks.sh

# Versión simplificada (recomendada para desarrollo diario)
./scripts/pre-commit-simple.sh

# Con opciones (solo versión completa)
./scripts/pre-commit-checks.sh --skip-tests --verbose
pwsh ./scripts/pre-commit-checks.ps1 -SkipTests -Verbose
```

### Comandos de Desarrollo

```bash
# Formatear código automáticamente
dotnet format

# Construir con análisis completo
dotnet build --configuration Release

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Generar reporte de cobertura
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

# Verificar paquetes vulnerables
dotnet list package --vulnerable --include-transitive
```

## 📊 Métricas de Calidad Exigidas

| Métrica | Objetivo | Nivel |
|---------|----------|-------|
| **Cobertura de Código** | ≥ 80% | Líneas de código |
| **Complejidad Ciclomática** | ≤ 10 | Por método |
| **Duplicación de Código** | ≤ 3% | Del total |
| **Violaciones de Estilo** | 0 | Warnings como errores |
| **Tests Unitarios** | 100% | Deben pasar todos |

## 🔍 Pre-commit Hook

El hook se ejecuta automáticamente en cada commit y verifica:

### Versión Actual (Simplificada)
1. **📦 Restauración**: Restaura paquetes NuGet
2. **🏗️ Compilación**: Construye la solución sin errores
3. **🧪 Tests Unitarios**: Ejecuta todos los tests (43 tests)

### Versión Completa (Disponible)
1. **✅ Formateo de Código**: Verifica que el código sigue las convenciones
2. **🏗️ Compilación**: Construye la solución sin errores
3. **📊 Análisis Estático**: Ejecuta todos los analizadores
4. **🧪 Tests Unitarios**: Ejecuta todos los tests con cobertura
5. **🔒 Seguridad**: Verifica paquetes vulnerables y archivos sensibles
6. **📁 Validación de Archivos**: Detecta archivos que no deberían commitarse

> **Nota**: Por defecto se usa la versión simplificada para facilitar el desarrollo. Para habilitar la versión completa, modifica el archivo `.git/hooks/pre-commit`.

### Bypass del Pre-commit (No Recomendado)

```bash
git commit --no-verify -m "mensaje del commit"
```

## 📁 Estructura de Archivos de Calidad

```
├── .editorconfig                 # Configuración de estilo de código
├── Directory.Build.props         # Configuración global de análisis
├── quality-rules.ruleset         # Reglas personalizadas
├── stylecop.json                # Configuración de StyleCop
├── .gitignore                   # Archivos ignorados (incluyendo outputs de calidad)
├── scripts/
│   ├── pre-commit-checks.ps1    # Script de PowerShell
│   └── pre-commit-checks.sh     # Script de Bash
└── .git/hooks/
    └── pre-commit               # Hook ejecutable
```

## 🔧 Configuración del Entorno de Desarrollo

### Herramientas Globales Necesarias

```bash
# Instalar herramientas globales
dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool install -g dotnet-format
```

### Variables de Entorno

```bash
# Configurar cobertura mínima
export COVERAGE_THRESHOLD=80

# Configurar MongoDB para tests
export MONGO_CONNECTION_STRING="mongodb://localhost:27017/dotnet_ecuador_test"
```

## 📈 Reportes de Calidad

### Cobertura de Código
Los reportes se generan en: `TestResults/CoverageReport/index.html`

### Análisis Estático
Los warnings y errores aparecen durante la compilación y en la salida del IDE.

## 🚨 Resolución de Problemas Comunes

### Error: "Code formatting issues detected"
```bash
# Solución: Formatear el código
dotnet format
```

### Error: "Build failed"
```bash
# Revisar errores de compilación
dotnet build --verbosity normal
```

### Error: "Tests failed"
```bash
# Ejecutar tests para ver detalles
dotnet test --verbosity normal
```

### Error: "Static analysis found issues"
```bash
# Revisar warnings en la salida del build
dotnet build --verbosity detailed
```

## 🎯 Mejores Prácticas

1. **Ejecuta `dotnet format` antes de cada commit**
2. **Mantén la cobertura de tests por encima del 80%**
3. **Resuelve todos los warnings de análisis estático**
4. **No hagas bypass del pre-commit hook sin justificación**
5. **Revisa regularmente los reportes de cobertura**
6. **Mantén la complejidad ciclomática baja**

## 📚 Referencias

- [.NET Code Analysis Rules](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- [StyleCop Documentation](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [EditorConfig Properties](https://editorconfig.org/)
- [SonarAnalyzer Rules](https://rules.sonarsource.com/csharp)