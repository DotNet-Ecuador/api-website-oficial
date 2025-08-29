# API - Comunidad DotNet Ecuador

Este proyecto proporciona una API para gestionar la información de la comunidad, y aplicaciones de voluntariado. La API está diseñada para ser utilizada por la página oficial de la comunidad.

## Levantamiento del contenedor

En la ruta donde se encuentra el archivo Dockerfile ejecutar:

docker build -t dotnetecuador-api -f DotNetEcuador.API/Dockerfile .


## Endpoints

### 1. **Autenticación**

- **POST** `/api/v1/auth/register`

Registra un nuevo usuario en el sistema.

#### Cuerpo de la solicitud

```json
{
  "email": "string",
  "password": "string",
  "fullName": "string",
  "role": "string"
}
```

- **POST** `/api/v1/auth/login`

Inicia sesión y obtiene tokens JWT.

#### Cuerpo de la solicitud

```json
{
  "email": "string",
  "password": "string"
}
```

#### Respuesta

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "user": {
    "id": "string",
    "email": "string",
    "fullName": "string",
    "role": "string"
  }
}
```

- **POST** `/api/v1/auth/refresh`

Renueva el token de acceso usando el refresh token.

- **POST** `/api/v1/auth/logout`

Cierra sesión y revoca el refresh token.

### 2. **Área de Interés**

- **GET** `/api/v1/AreaOfInterest`

Este endpoint permite obtener la lista de áreas de interés disponibles para la comunidad.

#### Respuesta

```json
{
  "name": "string",
  "description": "string"
}
```

- **name** (string): Nombre del área de interés.
- **description** (string): Descripción del área de interés.

### 3. **Comunidad**

- **POST** `/api/v1/Community`

Este endpoint permite agregar un miembro a la comunidad.

#### Cuerpo de la solicitud

```json
{
  "fullName": "string",
  "email": "string"
}
```

- **fullName** (string): Nombre completo del miembro.
- **email** (string): Correo electrónico del miembro.

### 4. **Aplicación de Voluntariado**

- **POST** `/api/v1/VolunteerApplication/apply`

Este endpoint permite que una persona aplique para ser voluntario en la comunidad.

#### Cuerpo de la solicitud

```json
{
  "fullName": "string",
  "email": "string",
  "phoneNumber": "string",
  "city": "string",
  "hasVolunteeringExperience": "boolean",
  "areasOfInterest": [
    "string"
  ],
  "otherAreas": "string",
  "availableTime": "string",
  "skillsOrKnowledge": "string",
  "whyVolunteer": "string",
  "additionalComments": "string"
}
```

- **fullName** (string): Nombre completo del solicitante.
- **email** (string): Correo electrónico del solicitante.
- **phoneNumber** (string): Número de teléfono del solicitante.
- **city** (string): Ciudad de residencia.
- **hasVolunteeringExperience** (boolean): Indica si el solicitante tiene experiencia previa en voluntariado.
- **areasOfInterest** (array de strings): Áreas de interés del solicitante. Valores válidos: `"EventOrganization"`, `"ContentCreation"`, `"TechnicalSupport"`, `"SocialMediaManagement"`, `"Other"`.
- **otherAreas** (string): Otras áreas de interés que no estén en la lista.
- **availableTime** (string): Tiempo disponible para el voluntariado.
- **skillsOrKnowledge** (string): Habilidades o conocimientos relevantes.
- **whyVolunteer** (string): Razones para ser voluntario.
- **additionalComments** (string): Comentarios adicionales.

## Esquemas

### Área de Interés (`AreaOfInterest`)

```json
{
  "name": "string",
  "description": "string"
}
```

### Miembro de la Comunidad (`CommunityMember`)

```json
{
  "fullName": "string",
  "email": "string"
}
```

### Aplicación de Voluntariado (`VolunteerApplication`)

```json
{
  "fullName": "string",
  "email": "string",
  "phoneNumber": "string",
  "city": "string",
  "hasVolunteeringExperience": "boolean",
  "areasOfInterest": [
    "string"
  ],
  "otherAreas": "string",
  "availableTime": "string",
  "skillsOrKnowledge": "string",
  "whyVolunteer": "string",
  "additionalComments": "string"
}
```

## Instalación

### Prerrequisitos

- **.NET 8 SDK**
- **MongoDB Community Server** (para desarrollo local)

### 1. Configuración de MongoDB Local (Recomendado para Desarrollo)

**Opción A: MongoDB sin Autenticación (Desarrollo Local)**
```bash
# Instalar MongoDB Community Server
# Windows: Descargar desde https://www.mongodb.com/try/download/community
# macOS: brew install mongodb-community
# Ubuntu: sudo apt install mongodb

# Iniciar MongoDB localmente SIN autenticación
mongod --dbpath /path/to/your/db --port 27017

# O si usas el servicio del sistema
# Windows: net start MongoDB
# macOS/Linux: brew services start mongodb-community
```

### 2. Clonar el Repositorio

```bash
git clone <URL del repositorio>
cd <directorio del proyecto>
```

### 3. Configurar Variables de Entorno

```bash
# Linux/macOS
export MONGO_CONNECTION_STRING="mongodb://localhost:27017/dotnet_ecuador"
export JWT_SECRET_KEY="your-super-secret-jwt-key-here-should-be-at-least-256-bits"
export JWT_ISSUER="DotNetEcuador.API"
export JWT_AUDIENCE="DotNetEcuador.API"
export JWT_ACCESS_TOKEN_EXPIRATION="15"
export JWT_REFRESH_TOKEN_EXPIRATION="7"

# Windows (PowerShell)
$env:MONGO_CONNECTION_STRING="mongodb://localhost:27017/dotnet_ecuador"
$env:JWT_SECRET_KEY="your-super-secret-jwt-key-here-should-be-at-least-256-bits"
$env:JWT_ISSUER="DotNetEcuador.API"
$env:JWT_AUDIENCE="DotNetEcuador.API"
$env:JWT_ACCESS_TOKEN_EXPIRATION="15"
$env:JWT_REFRESH_TOKEN_EXPIRATION="7"

# Windows (Command Prompt)
set MONGO_CONNECTION_STRING=mongodb://localhost:27017/dotnet_ecuador
set JWT_SECRET_KEY=your-super-secret-jwt-key-here-should-be-at-least-256-bits
set JWT_ISSUER=DotNetEcuador.API
set JWT_AUDIENCE=DotNetEcuador.API
set JWT_ACCESS_TOKEN_EXPIRATION=15
set JWT_REFRESH_TOKEN_EXPIRATION=7
```

**Importante:** 
- `JWT_SECRET_KEY` debe ser una clave secreta de al menos 256 bits (32 caracteres)
- `JWT_ACCESS_TOKEN_EXPIRATION` está en minutos
- `JWT_REFRESH_TOKEN_EXPIRATION` está en días

### 4. Restaurar Dependencias y Ejecutar

```bash
dotnet restore
dotnet run --project DotNetEcuador.API
```

### 5. Verificar Instalación

- **API Documentation**: https://localhost:7209/swagger
- **Health Check**: https://localhost:7209/health
- **Database Health**: https://localhost:7209/health/ready


## Troubleshooting

### Problemas Comunes de MongoDB

#### Error: "MongoAuthenticationException" o "Authentication failed"

**Causa**: MongoDB está configurado con autenticación pero la cadena de conexión no incluye credenciales.

**Solución**:
```bash
# Opción 1: Reiniciar MongoDB SIN autenticación (Recomendado para desarrollo)
mongod --dbpath /path/to/your/db --port 27017

# Opción 2: Usar autenticación (Para producción)
# Crear usuario administrador en MongoDB:
mongo
use admin
db.createUser({user:"admin", pwd:"password", roles:["root"]})

# Actualizar variable de entorno:
export MONGO_CONNECTION_STRING="mongodb://admin:password@localhost:27017/dotnet_ecuador"
```

#### Error: "MongoConnectionException" o "No server available"

**Causa**: MongoDB no está ejecutándose.

**Solución**:
```bash
# Verificar si MongoDB está ejecutándose
# Windows
net start MongoDB
# O manualmente: mongod --dbpath C:\data\db

# macOS
brew services start mongodb-community
# O manualmente: mongod --config /usr/local/etc/mongod.conf

# Linux
sudo systemctl start mongod
# O manualmente: mongod --dbpath /var/lib/mongodb
```

#### Error: "InvalidOperationException: Could not connect to MongoDB"

**Causa**: Puerto o host incorrectos.

**Solución**:
1. Verificar que MongoDB esté en el puerto correcto: `netstat -an | grep 27017`
2. Verificar la cadena de conexión: `mongodb://localhost:27017/dotnet_ecuador`
3. Probar conexión: `mongo mongodb://localhost:27017/dotnet_ecuador`

### Problemas de Autenticación JWT

#### Error: "JWT_SECRET_KEY environment variable is required"

**Solución**:
```bash
export JWT_SECRET_KEY="tu-clave-secreta-de-al-menos-32-caracteres-aqui"
```

#### Error: "SecurityTokenValidationException"

**Causa**: Clave JWT incorrecta o tokens expirados.

**Solución**:
1. Verificar que la clave sea consistente entre ejecuciones
2. Regenerar token con `/api/v1/auth/login`

### Verificación de Health Checks

```bash
# Verificar estado general de la aplicación
curl https://localhost:7209/health

# Verificar específicamente MongoDB
curl https://localhost:7209/health/ready

# Verificar que la aplicación esté viva
curl https://localhost:7209/health/live
```

### Logs de Desarrollo

Para obtener más información de diagnóstico:

```bash
# Ejecutar con logs detallados
dotnet run --project DotNetEcuador.API --verbosity diagnostic

# O configurar logging en appsettings.Development.json:
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "MongoDB": "Debug"
    }
  }
}
```

## Configuraciones Avanzadas

### MongoDB con Autenticación (Producción)

```bash
# Formato de cadena de conexión completa
mongodb://<usuario>:<contraseña>@<host>:<puerto>/<nombre-base-de-datos>?authSource=admin

# Ejemplo:
export MONGO_CONNECTION_STRING="mongodb://apiuser:secretpassword@localhost:27017/dotnet_ecuador?authSource=admin"
```

### Variables de Entorno para Diferentes Ambientes

**Desarrollo**:
```bash
export ASPNETCORE_ENVIRONMENT="Development"
export MONGO_CONNECTION_STRING="mongodb://localhost:27017/dotnet_ecuador"
```

**Producción**:
```bash
export ASPNETCORE_ENVIRONMENT="Production"
export MONGO_CONNECTION_STRING="mongodb://user:pass@prod-host:27017/dotnet_ecuador"
```