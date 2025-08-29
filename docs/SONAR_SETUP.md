# 🔍 Configuración Detallada de SonarQube

## Introducción

SonarQube es una plataforma de análisis estático de código que proporciona métricas detalladas sobre la calidad del código, cobertura de tests, vulnerabilidades de seguridad y deuda técnica.

## Arquitectura del Setup

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Docker Host   │    │   SonarQube     │    │   PostgreSQL    │
│                 │    │   Container     │    │   Container     │
│  ┌───────────┐  │    │                 │    │                 │
│  │   .NET    │  │───▶│  Port: 9000     │───▶│  Port: 5432     │
│  │ Project   │  │    │  Analysis       │    │  Database       │
│  └───────────┘  │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Archivos de Configuración

### 1. `docker-compose.sonarqube.yml`

Configura dos servicios:
- **SonarQube Community Edition 10.4**: Servidor principal de análisis
- **PostgreSQL 15**: Base de datos para persistencia

**Características:**
- Volúmenes persistentes para datos, extensiones y logs
- Red privada para comunicación entre servicios
- Configuración de memoria y límites del sistema
- Reinicio automático de contenedores

### 2. `sonar-project.properties`

Configuración específica del proyecto:
- **Identificación**: Clave y nombre del proyecto
- **Rutas de código**: Fuentes y tests
- **Exclusiones**: Archivos generados, bins, objetos
- **Cobertura**: Configuración para reportes de coverlet
- **Encoding**: UTF-8 para caracteres especiales

### 3. Scripts de Análisis

#### `sonar-analysis.ps1` (PowerShell)
- Validación de prerequisitos (Docker, .NET, SonarScanner)
- Instalación automática de herramientas faltantes
- Limpieza de builds anteriores
- Ejecución secuencial: build → test → análisis
- Manejo de errores y logs informativos

#### `sonar-analysis.sh` (Bash)
- Soporte para Linux y macOS
- Argumentos por línea de comandos
- Validaciones robustas de conectividad
- Compatible con CI/CD pipelines

#### `sonar-analysis.bat` (Windows CMD)
- Compatibilidad con Command Prompt
- Manejo de variables de entorno Windows
- Validaciones específicas para Windows

## Proceso de Análisis Paso a Paso

### 1. Preparación
```bash
# Limpiar builds anteriores
dotnet clean
rm -rf TestResults
rm -rf .sonarqube
```

### 2. Inicio de Análisis
```bash
dotnet sonarscanner begin \
    /k:"dotnet-ecuador-api" \
    /d:sonar.host.url="http://localhost:9000" \
    /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml"
```

### 3. Compilación
```bash
dotnet restore
dotnet build --configuration Release
```

### 4. Ejecución de Tests
```bash
dotnet test --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger trx
```

### 5. Finalización
```bash
dotnet sonarscanner end
```

## Métricas Analizadas

### Calidad de Código
- **Bugs**: Errores que pueden causar fallos
- **Vulnerabilities**: Problemas de seguridad
- **Code Smells**: Problemas de mantenibilidad

### Cobertura de Tests
- **Line Coverage**: Líneas ejecutadas por tests
- **Branch Coverage**: Ramas de código cubiertas
- **Condition Coverage**: Condiciones booleanas evaluadas

### Métricas de Complejidad
- **Cyclomatic Complexity**: Complejidad de control de flujo
- **Cognitive Complexity**: Dificultad de entendimiento
- **Technical Debt**: Tiempo estimado para resolver issues

### Duplicación
- **Duplicated Lines**: Líneas de código duplicadas
- **Duplicated Blocks**: Bloques completos duplicados
- **Duplicated Files**: Archivos con duplicación significativa

## Configuración de Quality Gates

### Default Quality Gate
- **Coverage**: > 80%
- **Duplicated Lines**: < 3%
- **Maintainability Rating**: A
- **Reliability Rating**: A
- **Security Rating**: A

### Personalización
```bash
# Acceder a Quality Gates en SonarQube Web UI
http://localhost:9000/quality_gates

# Configurar umbrales específicos para el proyecto
- New Coverage: > 85%
- New Duplicated Lines: < 1%
- New Bugs: 0
- New Vulnerabilities: 0
```

## Integración con IDEs

### Visual Studio Code
```bash
# Instalar extensión SonarLint
code --install-extension SonarSource.sonarlint-vscode

# Configurar conexión a SonarQube local
# Settings > Extensions > SonarLint > Connected Mode
```

### Visual Studio 2022
```bash
# Instalar SonarLint for Visual Studio
# Extensions > Manage Extensions > Search "SonarLint"

# Conectar a SonarQube:
# Team Explorer > Connect to Server > Add SonarQube Server
```

## Automatización y CI/CD

### GitHub Actions (ejemplo)
```yaml
name: SonarQube Analysis

on:
  workflow_dispatch:  # Manual trigger only

jobs:
  sonarqube:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Run SonarQube Analysis
      run: ./scripts/sonar-analysis.sh
      env:
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

## Resolución de Problemas Comunes

### 1. Error de Memoria Insuficiente
```bash
# Docker Desktop: Settings > Resources > Memory > 4GB+

# Linux: Aumentar vm.max_map_count
echo 'vm.max_map_count=524288' | sudo tee -a /etc/sysctl.conf
sudo sysctl -p

# Windows: Asegurar WSL2 con suficiente memoria
```

### 2. SonarQube No Responde
```bash
# Verificar logs
docker-compose -f docker-compose.sonarqube.yml logs sonarqube

# Reiniciar servicios
docker-compose -f docker-compose.sonarqube.yml restart

# Verificar puertos
netstat -tulpn | grep :9000
```

### 3. Error de Autenticación
```bash
# Reset de contraseña admin:
# 1. Acceder a http://localhost:9000
# 2. Login: admin/admin
# 3. Cambiar contraseña obligatoriamente
# 4. Generar nuevo token en My Account > Security
```

### 4. Problemas de Cobertura
```bash
# Verificar que coverlet genere reportes OpenCover
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Verificar rutas en sonar-project.properties
ls -la TestResults/*/coverage.opencover.xml
```

### 5. SonarScanner No Encontrado
```bash
# Desinstalar y reinstalar
dotnet tool uninstall --global dotnet-sonarscanner
dotnet tool install --global dotnet-sonarscanner

# Verificar PATH
dotnet tool list --global
```

## Mejores Prácticas

### 1. Frecuencia de Análisis
- **Desarrollo**: Antes de cada merge a main
- **Releases**: Obligatorio para cada release
- **Features**: Opcional en branches de features

### 2. Gestión de Issues
- **Priorizar**: Bugs > Vulnerabilities > Code Smells
- **Asignar**: Responsables específicos por módulo
- **Documentar**: Razones si se marcan como "Won't Fix"

### 3. Quality Gates
- **Nuevas funciones**: Cobertura > 80%
- **Refactoring**: Reducir deuda técnica existente
- **Hotfixes**: Cero nuevos bugs o vulnerabilities

### 4. Mantenimiento
```bash
# Backup de configuración (mensual)
docker exec sonarqube pg_dump -h sonarqube-db -U sonar sonar > backup.sql

# Limpieza de análisis antiguos (trimestral)
# SonarQube Web UI > Administration > Projects > Housekeeping
```

## Recursos Adicionales

- **Documentación Oficial**: https://docs.sonarqube.org/
- **Reglas C#**: https://rules.sonarsource.com/csharp
- **Community Forum**: https://community.sonarsource.com/
- **Plugins Disponibles**: https://docs.sonarqube.org/latest/instance-administration/marketplace/