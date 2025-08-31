# GitHub Actions - Despliegue Automático en VPS Ubuntu

## Descripción

Este proyecto utiliza GitHub Actions para desplegar automáticamente la API DotNet Ecuador en un VPS Ubuntu cuando se hace push a la rama `main`.

## Configuración Previa del Servidor

### 1. Preparación del VPS Ubuntu

```bash
# Actualizar sistema
sudo apt update && sudo apt upgrade -y

# Instalar .NET 8
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0

# Instalar MongoDB
wget -qO - https://www.mongodb.org/static/pgp/server-6.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/6.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-6.0.list
sudo apt update
sudo apt install -y mongodb-org

# Iniciar y habilitar MongoDB
sudo systemctl start mongod
sudo systemctl enable mongod

# Crear usuario de aplicación
sudo useradd -r -s /bin/false www-data || true
```

### 2. Configuración de MongoDB (Seguridad)

```bash
# Conectar a MongoDB
mongo

# Crear usuario administrador
use admin
db.createUser({
  user: "admin",
  pwd: "tu_password_seguro",
  roles: ["root"]
})

# Habilitar autenticación
sudo nano /etc/mongod.conf
# Agregar:
# security:
#   authorization: enabled

sudo systemctl restart mongod
```

### 3. Configurar Usuario SSH para Despliegue

```bash
# Crear usuario específico para despliegue
sudo adduser deployuser
sudo usermod -aG sudo deployuser

# Configurar SSH key
sudo mkdir -p /home/deployuser/.ssh
sudo nano /home/deployuser/.ssh/authorized_keys
# Pegar la clave pública SSH

sudo chown -R deployuser:deployuser /home/deployuser/.ssh
sudo chmod 700 /home/deployuser/.ssh
sudo chmod 600 /home/deployuser/.ssh/authorized_keys
```

## Configuración de GitHub Secrets

En tu repositorio de GitHub, ve a **Settings > Secrets and variables > Actions** y configura los siguientes secrets:

### Conexión SSH
```
VPS_HOST=tu-servidor.com
VPS_USERNAME=deployuser
VPS_SSH_KEY=-----BEGIN OPENSSH PRIVATE KEY-----
[tu clave SSH privada completa]
-----END OPENSSH PRIVATE KEY-----
VPS_PORT=22
```

### Variables de Aplicación
```
APP_PATH=/opt/dotnet-ecuador-api
MONGO_CONNECTION_STRING=mongodb://admin:password@localhost:27017/dotnet_ecuador?authSource=admin
JWT_SECRET_KEY=tu-clave-secreta-de-al-menos-32-caracteres-aqui
JWT_ISSUER=DotNetEcuador.API
JWT_AUDIENCE=DotNetEcuador.API
JWT_ACCESS_TOKEN_EXPIRATION=15
JWT_REFRESH_TOKEN_EXPIRATION=7
```

## Flujo de Despliegue

### Trigger
- Se ejecuta automáticamente en cada `push` a la rama `main`
- No requiere intervención manual

### Pasos del Workflow

1. **Checkout** - Descarga el código fuente
2. **Setup .NET** - Configura .NET 8 SDK
3. **Build** - Compila la aplicación en modo Release
4. **Publish** - Genera los binarios optimizados
5. **Package** - Empaqueta archivos de despliegue
6. **Transfer** - Sube archivos al servidor via SSH/SCP
7. **Deploy** - Ejecuta script de despliegue en el servidor
8. **Health Check** - Verifica que la aplicación esté funcionando
9. **Cleanup** - Limpia archivos temporales

### Scripts de Despliegue

#### `scripts/deploy-server.sh`
- Script principal que se ejecuta en el servidor
- Detiene la aplicación, actualiza archivos, configura variables
- Instala/actualiza el servicio systemd
- Inicia la aplicación

#### `scripts/stop-app.sh`
- Detiene el servicio de forma segura
- Limpia procesos restantes si es necesario

#### `scripts/start-app.sh`
- Inicia el servicio systemd
- Verifica que la aplicación responda correctamente

## Estructura en el Servidor

```
/opt/dotnet-ecuador-api/
├── app/                          # Aplicación actual
│   ├── DotNetEcuador.API.dll    # Binario principal
│   ├── appsettings.Production.json
│   └── [otros archivos de la app]
├── logs/                        # Logs de la aplicación
├── app.backup.YYYYMMDD_HHMMSS/ # Backups automáticos
└── scripts/                    # Scripts de mantenimiento
```

## Monitoreo y Troubleshooting

### Verificar Estado del Servicio
```bash
# Estado del servicio
sudo systemctl status dotnet-ecuador-api

# Logs en tiempo real
sudo journalctl -u dotnet-ecuador-api -f

# Logs de aplicación
tail -f /opt/dotnet-ecuador-api/logs/app-*.log
```

### Comandos Útiles
```bash
# Reiniciar servicio manualmente
sudo systemctl restart dotnet-ecuador-api

# Ver configuración del servicio
sudo systemctl cat dotnet-ecuador-api

# Ver variables de entorno
sudo cat /etc/systemd/system/dotnet-ecuador-api.env

# Probar endpoint de salud
curl http://localhost:5000/health
```

### Solución de Problemas Comunes

#### El servicio no inicia
```bash
# Verificar logs
sudo journalctl -u dotnet-ecuador-api --since "5 minutes ago"

# Verificar permisos
sudo chown -R www-data:www-data /opt/dotnet-ecuador-api/

# Probar ejecución manual
cd /opt/dotnet-ecuador-api/app
sudo -u www-data dotnet DotNetEcuador.API.dll
```

#### Error de conexión a MongoDB
```bash
# Verificar estado de MongoDB
sudo systemctl status mongod

# Probar conexión
mongo "mongodb://admin:password@localhost:27017/dotnet_ecuador?authSource=admin"
```

#### Problemas de permisos
```bash
# Corregir permisos de archivos
sudo chown -R www-data:www-data /opt/dotnet-ecuador-api/
sudo chmod -R 755 /opt/dotnet-ecuador-api/app/
sudo chmod +x /opt/dotnet-ecuador-api/app/DotNetEcuador.API
```

## Seguridad

### Variables de Entorno
- Las variables sensibles se almacenan como GitHub Secrets
- Se inyectan durante el despliegue sin persistir en archivos
- El archivo de environment systemd tiene permisos restrictivos (600)

### Usuario de Aplicación
- La aplicación se ejecuta como usuario `www-data` (sin privilegios)
- Acceso limitado solo a directorios necesarios

### Respaldos Automáticos
- Se mantienen los últimos 3 backups de la aplicación
- Los backups antiguos se eliminan automáticamente

## Rollback Manual

En caso de problemas, puedes hacer rollback a la versión anterior:

```bash
# Detener servicio
sudo systemctl stop dotnet-ecuador-api

# Restaurar backup (sustituir fecha)
cd /opt/dotnet-ecuador-api
sudo rm -rf app
sudo mv app.backup.YYYYMMDD_HHMMSS app

# Reiniciar servicio
sudo systemctl start dotnet-ecuador-api
```