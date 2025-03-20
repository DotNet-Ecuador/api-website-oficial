# API - Comunidad DotNet Ecuador

Este proyecto proporciona una API para gestionar la información de la comunidad, y aplicaciones de voluntariado. La API está diseñada para ser utilizada por la página oficial de la comunidad.

## Levantamiento del contenedor

En la ruta donde se encuentra el archivo Dockerfile ejecutar:

docker build -t dotnetecuador-api -f api/Dockerfile .


## Endpoints

### 1. **Área de Interés**

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

### 2. **Comunidad**

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

### 3. **Aplicación de Voluntariado**

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
    {
      "name": "string",
      "description": "string"
    }
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
- **areasOfInterest** (array de objetos): Áreas de interés del solicitante (ver esquema `AreaOfInterest`).
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
    {
      "name": "string",
      "description": "string"
    }
  ],
  "otherAreas": "string",
  "availableTime": "string",
  "skillsOrKnowledge": "string",
  "whyVolunteer": "string",
  "additionalComments": "string"
}
```

## Instalación

1. Clona este repositorio en tu máquina local:

   ```bash
   git clone <URL del repositorio>
   ```

2. Navega al directorio del proyecto y restaura los paquetes NuGet:

   ```bash
   cd <directorio del proyecto>
   dotnet restore
   ```

3. Ejecuta la aplicación:

   ```bash
   dotnet run
   ```

4. Accede a la documentación de la API en `https://localhost:7209/swagger`.


## Formato de cadena de conexión MONGODB para la variable: MONGO_CONNECTION_STRING
   mongodb://<usuario>:<contraseña>@<host>:<puerto>/<nombre-base-de-datos>