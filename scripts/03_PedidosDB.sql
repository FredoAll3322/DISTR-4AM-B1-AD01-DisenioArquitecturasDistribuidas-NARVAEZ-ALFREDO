-- ==========================================================
-- SCRIPT DE BASE DE DATOS: PedidosDB
-- Estudiante: Alfredo Narváez
-- Asignatura: Aplicaciones Distribuidas - AD01
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

-- 2. Tabla Pedidos (FK física interna a Clientes)
IF OBJECT_ID('dbo.Pedidos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pedidos (
        IdPedido INT IDENTITY(1,1) PRIMARY KEY,
        IdCliente INT NOT NULL,
        Fecha DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Total DECIMAL(18,2) NOT NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
        CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (IdCliente) REFERENCES dbo.Clientes(IdCliente)
    );
END
GO

-- 3. Tabla DetallePedido
-- IdPedido: FK física hacia Pedidos
-- IdProducto: FK lógica hacia el microservicio Catalogo
IF OBJECT_ID('dbo.DetallesPedido', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallesPedido (
        IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
        IdPedido INT NOT NULL,
        IdProducto INT NOT NULL, -- Referencia logica a CatalogoDB.dbo.Productos
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_DetallesPedido_Pedidos FOREIGN KEY (IdPedido) REFERENCES dbo.Pedidos(IdPedido) ON DELETE CASCADE
    );
END
GO

-- Datos semilla
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE IdCliente = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Clientes ON;
    INSERT INTO dbo.Clientes (IdCliente, Nombres, Cedula, Correo) VALUES
    (1, 'Alfredo Narváez', '1723456789', 'alfrednrvz9@gmail.com'),
    (2, 'María Elena Gómez', '0923456781', 'maria.gomez@gmail.com');
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
