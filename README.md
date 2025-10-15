# Sistema de Gestión de Citas - Hospital San Vicente

Este proyecto es una aplicación web desarrollada con ASP.NET Core MVC para la gestión de citas médicas en el Hospital San Vicente. Permite administrar pacientes, doctores y agendar citas, aplicando reglas de negocio para evitar conflictos de horario y notificando a los pacientes por correo electrónico.

## Tecnologías Utilizadas

*   **Framework:** .NET 8
*   **Lenguaje:** C#
*   **Arquitectura:** MVC (Model-View-Controller)
*   **Base de Datos:** MySQL
*   **ORM:** Entity Framework Core 8
*   **Envío de Correo:** MailKit

---

## Prerrequisitos

Antes de comenzar, asegúrate de tener instalado el siguiente software en tu sistema:

1.  **SDK de .NET 8:** [Descargar .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
2.  **Git:** [Descargar Git](https://git-scm.com/downloads)
3.  **Un editor de código:** Visual Studio 2022 o Visual Studio Code.
4.  **Servidor de Base de Datos MySQL:** Puedes usar una instalación local, un contenedor de Docker, XAMPP, etc.
5.  **Herramientas de línea de comandos de EF Core:** Si no las tienes instaladas, ejecuta el siguiente comando en tu terminal:
    ```bash
    dotnet tool install --global dotnet-ef
    ```

---

## Guía de Instalación y Configuración

Sigue estos pasos para levantar el proyecto en tu entorno de desarrollo local.

### 1. Clonar el Repositorio

Abre una terminal y clona el proyecto en la carpeta de tu elección.
```bash
git clone <URL_DEL_REPOSITORIO>
cd HospitalSanVicente
```

### 2. Configurar la Conexión a la Base de Datos

Este es el paso más importante. Debes configurar la cadena de conexión para que la aplicación pueda comunicarse con tu base de datos MySQL.

1.  Abre el archivo `appsettings.json` en la raíz del proyecto.
2.  Busca la sección `ConnectionStrings`.
3.  Modifica el valor de `DefaultConnection` con los datos de tu servidor MySQL.

**Ejemplo:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=HospitalDB;user=root;password=tu_contraseña_secreta"
  }
}
```
**Asegúrate de cambiar:**
*   `server`: La dirección de tu servidor de base de datos (usualmente `localhost`).
*   `database`: El nombre que deseas darle a la base de datos (ej. `HospitalDB`). **No es necesario crearla manualmente**, Entity Framework lo hará por ti.
*   `user`: Tu usuario de MySQL.
*   `password`: La contraseña de tu usuario de MySQL.

### 3. Configurar el Servicio de Correo Electrónico

La aplicación utiliza MailKit para enviar correos de confirmación. Para las pruebas de desarrollo, se recomienda usar un servicio como [Mailtrap.io](https://mailtrap.io/), que provee una bandeja de entrada falsa para evitar enviar correos a usuarios reales.

1.  En el mismo archivo `appsettings.json`, busca la sección `EmailSettings`.
2.  Rellena los campos con tus credenciales SMTP.

**Ejemplo (usando Mailtrap):**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailtrap.io",
    "Port": 2525,
    "SenderName": "Hospital San Vicente",
    "SenderEmail": "no-reply@hospitalsanvicente.com",
    "Username": "tu_usuario_de_mailtrap",    // <-- Cambiar por tus datos
    "Password": "tu_contraseña_de_mailtrap"  // <-- Cambiar por tus datos
  }
}
```

### 4. Preparación de la Base de Datos

Una vez configurada la conexión, usaremos las migraciones de Entity Framework Core para crear la base de datos y todas las tablas necesarias.

Abre una terminal en la raíz del proyecto (donde está el archivo `.csproj`) y ejecuta el siguiente comando:```bash
dotnet ef database update
```
Este comando leerá los archivos de migración existentes en el proyecto y construirá el esquema de la base de datos de acuerdo a los modelos definidos.

---

## Ejecutar el Proyecto

Con todo configurado, estás listo para ejecutar la aplicación.

1.  En la misma terminal, en la raíz del proyecto, ejecuta el siguiente comando:
    ```bash
    dotnet watch run
    ```
    *   El comando `watch` es opcional, pero muy recomendado para desarrollo, ya que recompila y recarga la aplicación automáticamente cuando detecta cambios en el código.

2.  La terminal te mostrará un mensaje indicando que la aplicación está escuchando en un puerto local.
    ```
    info: Microsoft.Hosting.Lifetime
          Now listening on: http://localhost:5036
    ```

3.  Abre tu navegador web y navega a la URL indicada (ej. `http://localhost:5036`).

¡Listo! La aplicación del Hospital San Vicente debería estar funcionando en tu máquina.

## Funcionalidades del Proyecto

La solución está organizada en tres módulos principales:

*   **Gestión de Pacientes:**
    *   Crear, editar y listar pacientes.
    *   Activar y desactivar pacientes (borrado lógico).
    *   Validación de número de documento único.

*   **Gestión de Doctores:**
    *   Crear, editar y listar doctores.
    *   Activar y desactivar doctores.
    *   Validación de número de documento único.
    *   Validación de combinación única de Nombre + Apellido + Especialidad.

*   **Gestión de Citas:**
    *   Agendar nuevas citas asociando un paciente y un doctor.
    *   Listar todas las citas con opción de filtrar por paciente o doctor.
    *   Validación para que un doctor o paciente no tenga más de una cita en el mismo horario.
    *   Cambiar el estado de una cita a "Cancelada" o "Atendida".
    *   Envío de correo de confirmación al agendar una cita.
    *   Historial visual del estado de envío de correos ("Enviado" / "No Enviado").
