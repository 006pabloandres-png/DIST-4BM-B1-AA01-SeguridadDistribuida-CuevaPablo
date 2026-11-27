CREATE DATABASE UsuariosDB;
GO

USE UsuariosDB;
GO

CREATE TABLE Perfiles (
    PerfilId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UsuarioId NVARCHAR(450) NOT NULL,
    NombreCompleto NVARCHAR(150) NOT NULL,
    Correo NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(20) NULL,
    Ciudad NVARCHAR(100) NULL,
    Direccion NVARCHAR(250) NULL,
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Perfiles_FechaCreacion DEFAULT GETUTCDATE(),
    FechaActualizacion DATETIME2 NULL
);
GO

CREATE UNIQUE INDEX IX_Perfiles_UsuarioId 
ON Perfiles (UsuarioId);
GO