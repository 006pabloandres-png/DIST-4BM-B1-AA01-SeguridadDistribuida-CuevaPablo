USE UsuariosDB;
GO

INSERT INTO Perfiles (UsuarioId, NombreCompleto, Correo, Telefono, Ciudad, Direccion, FechaCreacion)
VALUES
    ('usr-demo-001', 'Carlos Andrés Mendoza', 'carlos.mendoza@ejemplo.ec', '0991234567', 'Quito', 'Av. Amazonas N34-120', GETUTCDATE()),
    ('usr-demo-002', 'María Fernanda Paredes', 'maria.paredes@ejemplo.ec', '0987654321', 'Guayaquil', 'Av. Francisco de Orellana Mz. 12', GETUTCDATE()),
    ('usr-demo-003', 'Diego Sebastián Salazar', 'diego.salazar@ejemplo.ec', '0971122334', 'Cuenca', 'Calle Larga 8-45', GETUTCDATE());
GO

USE NotificacionesDB;
GO

INSERT INTO EnviosCorreo (UsuarioId, Destinatario, Asunto, TipoNotificacion, EstadoEnvio, DetalleError, FechaEnvio)
VALUES
    ('usr-demo-001', 'carlos.mendoza@ejemplo.ec', 'Bienvenido al Sistema Comercial Distribuido', 'Bienvenida', 'Enviado', NULL, GETUTCDATE()),
    ('usr-demo-002', 'maria.paredes@ejemplo.ec', 'Restablecimiento de contraseña', 'Recuperacion', 'Enviado', NULL, GETUTCDATE()),
    ('usr-demo-003', 'diego.salazar@ejemplo.ec', 'Bienvenido al Sistema Comercial Distribuido', 'Bienvenida', 'Fallido', 'Error SMTP (SmtpException)', GETUTCDATE());
GO