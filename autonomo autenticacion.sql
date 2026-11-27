CREATE DATABASE AuthDB;
GO

USE AuthDB;
GO

-- 1. Tabla de Roles
CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
GO

CREATE UNIQUE INDEX RoleNameIndex 
ON AspNetRoles (NormalizedName) 
WHERE NormalizedName IS NOT NULL;
GO

-- 2. Tabla de Usuarios (IdentityUser + propiedades personalizadas)
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    NombreCompleto NVARCHAR(150) NOT NULL,
    CuentaHabilitada BIT NOT NULL CONSTRAINT DF_AspNetUsers_CuentaHabilitada DEFAULT 1,
    FechaRegistro DATETIME2 NOT NULL CONSTRAINT DF_AspNetUsers_FechaRegistro DEFAULT GETUTCDATE(),
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET(7) NULL,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0
);
GO

CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
GO

CREATE UNIQUE INDEX UserNameIndex 
ON AspNetUsers (NormalizedUserName) 
WHERE NormalizedUserName IS NOT NULL;
GO

-- 3. Tabla de Claims por Rol
CREATE TABLE AspNetRoleClaims (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId 
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);
GO

-- 4. Tabla de Claims por Usuario
CREATE TABLE AspNetUserClaims (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);
GO

-- 5. Tabla de Proveedores Externos (Vinculación Google OpenID Connect por 'sub')
CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);
GO

-- 6. Tabla Intermedia Usuario - Rol
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId 
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);
GO

-- 7. Tabla de Tokens de Usuario
CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(450) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, [Name]),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

-- Seeding inicial de Roles obligatorios
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'Cliente', 'CLIENTE', NEWID()),
    ('22222222-2222-2222-2222-222222222222', 'Administrador', 'ADMINISTRADOR', NEWID());
GO