-- ==========================================================
-- SCRIPT DE BASE DE DATOS: PedidosDB
-- MICROSERVICIO: Pedidos.Api
-- RESPONSABILIDAD: Registrar y administrar las órdenes de compra de los clientes
-- ==========================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PedidosDB')
BEGIN
    CREATE DATABASE PedidosDB;
END
GO

USE PedidosDB;
GO

-- 1. Tabla Clientes
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes (
        IdCliente INT IDENTITY(1,1) PRIMARY KEY,
        Nombres NVARCHAR(150) NOT NULL,
        Cedula NVARCHAR(20) NOT NULL UNIQUE,
        Correo NVARCHAR(100) NOT NULL
    );
END
GO

-- 2. Tabla Pedidos (Con FK física interna a Clientes)
IF OBJECT_ID('dbo.Pedidos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pedidos (
        IdPedido INT IDENTITY(1,1) PRIMARY KEY,
        IdCliente INT NOT NULL,
        Fecha DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Total DECIMAL(18,2) NOT NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- 'Pendiente', 'Procesado', 'Completado', 'Cancelado'
        CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (IdCliente) REFERENCES dbo.Clientes(IdCliente)
    );
END
GO

-- 3. Tabla DetallePedido
-- REGLAS DE INTEGRIDAD:
-- * IdPedido: Clave Foránea FÍSICA referenciando dbo.Pedidos dentro de PedidosDB.
-- * IdProducto: Clave Foránea LÓGICA referenciando a Catalogo.Api (CatalogoDB).
IF OBJECT_ID('dbo.DetallesPedido', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallesPedido (
        IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
        IdPedido INT NOT NULL,
        IdProducto INT NOT NULL, -- FK Lógica a CatalogoDB.dbo.Productos
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_DetallesPedido_Pedidos FOREIGN KEY (IdPedido) REFERENCES dbo.Pedidos(IdPedido) ON DELETE CASCADE
    );
END
GO

-- Datos Semilla (Seed Data)
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE IdCliente = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Clientes ON;
    INSERT INTO dbo.Clientes (IdCliente, Nombres, Cedula, Correo) VALUES
    (1, 'Juan Carlos Pérez', '1712345678', 'juan.perez@cordillera.edu.ec'),
    (2, 'María Elena Gómez', '0923456781', 'maria.gomez@empresa.com');
    SET IDENTITY_INSERT dbo.Clientes OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE IdPedido = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Pedidos ON;
    INSERT INTO dbo.Pedidos (IdPedido, IdCliente, Fecha, Total, Estado) VALUES
    (1, 1, SYSUTCDATETIME(), 1949.50, 'Completado');
    SET IDENTITY_INSERT dbo.Pedidos OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DetallesPedido WHERE IdDetalle = 1)
BEGIN
    SET IDENTITY_INSERT dbo.DetallesPedido ON;
    INSERT INTO dbo.DetallesPedido (IdDetalle, IdPedido, IdProducto, Cantidad, PrecioUnitario) VALUES
    (1, 1, 1, 1, 1850.00),
    (2, 1, 4, 1, 99.50);
    SET IDENTITY_INSERT dbo.DetallesPedido OFF;
END
GO
