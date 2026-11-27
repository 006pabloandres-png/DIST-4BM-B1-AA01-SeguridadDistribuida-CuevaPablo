CREATE DATABASE NotificacionesDB;
GO

USE NotificacionesDB;
GO

CREATE TABLE EnviosCorreo (
    EnvioId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UsuarioId NVARCHAR(450) NOT NULL,
    Destinatario NVARCHAR(150) NOT NULL,
    Asunto NVARCHAR(200) NOT NULL,
    TipoNotificacion NVARCHAR(50) NOT NULL, -- Bienvenida | Recuperacion | CambioPerfil
    EstadoEnvio NVARCHAR(30) NOT NULL,      -- Enviado | Fallido
    DetalleError NVARCHAR(500) NULL,
    FechaEnvio DATETIME2 NOT NULL CONSTRAINT DF_EnviosCorreo_FechaEnvio DEFAULT GETUTCDATE()
);
GO