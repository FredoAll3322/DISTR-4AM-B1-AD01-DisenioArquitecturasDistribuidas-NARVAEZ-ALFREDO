

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'InventarioDB')
BEGIN
    CREATE DATABASE InventarioDB;
END
GO

USE InventarioDB;
GO

-- 1. Tabla Bodegas
IF OBJECT_ID('dbo.Bodegas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bodegas (
        IdBodega INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Ciudad NVARCHAR(100) NOT NULL,
        Direccion NVARCHAR(200) NOT NULL
    );
END
GO

-- 2. Tabla Existencias
-- REGLA ARQUITECTÓNICA DISTRIBUIDA:
-- * IdBodega: Clave Foránea FÍSICA referenciando dbo.Bodegas dentro de InventarioDB.
-- * IdProducto: Clave Foránea LÓGICA referenciando a Catalogo.Api (CatalogoDB). 
--   NO lleva CONSTRAINT FK físico para respetar el patrón "Database per Service".
IF OBJECT_ID('dbo.Existencias', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Existencias (
        IdExistencia INT IDENTITY(1,1) PRIMARY KEY,
        IdProducto INT NOT NULL, -- FK Lógica a CatalogoDB.dbo.Productos
        IdBodega INT NOT NULL,   -- FK Física a InventarioDB.dbo.Bodegas
        Stock INT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Existencias_Bodegas FOREIGN KEY (IdBodega) REFERENCES dbo.Bodegas(IdBodega),
        CONSTRAINT UQ_Existencia_Producto_Bodega UNIQUE (IdProducto, IdBodega)
    );
END
GO

-- 3. Tabla MovimientoInventario
-- * IdBodega: FK Física
-- * IdProducto: FK Lógica
IF OBJECT_ID('dbo.MovimientosInventario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MovimientosInventario (
        IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
        IdProducto INT NOT NULL, -- FK Lógica a CatalogoDB.dbo.Productos
        IdBodega INT NOT NULL,   -- FK Física a InventarioDB.dbo.Bodegas
        Tipo NVARCHAR(50) NOT NULL, -- 'Entrada', 'Salida', 'Ajuste'
        Cantidad INT NOT NULL,
        Fecha DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Movimientos_Bodegas FOREIGN KEY (IdBodega) REFERENCES dbo.Bodegas(IdBodega)
    );
END
GO

-- 4. Tabla ReservaStock
-- * IdBodega: FK Física
-- * IdProducto: FK Lógica
IF OBJECT_ID('dbo.ReservasStock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReservasStock (
        IdReserva INT IDENTITY(1,1) PRIMARY KEY,
        IdProducto INT NOT NULL, -- FK Lógica a CatalogoDB.dbo.Productos
        IdBodega INT NOT NULL,   -- FK Física a InventarioDB.dbo.Bodegas
        Cantidad INT NOT NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- 'Pendiente', 'Confirmada', 'Cancelada'
        CONSTRAINT FK_Reservas_Bodegas FOREIGN KEY (IdBodega) REFERENCES dbo.Bodegas(IdBodega)
    );
END
GO

-- Datos Semilla (Seed Data)
IF NOT EXISTS (SELECT 1 FROM dbo.Bodegas WHERE IdBodega = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Bodegas ON;
    INSERT INTO dbo.Bodegas (IdBodega, Nombre, Ciudad, Direccion) VALUES
    (1, 'Bodega Principal Norte', 'Quito', 'Av. Galo Plaza Lasso N45-12'),
    (2, 'Bodega Sucursal Sur', 'Guayaquil', 'Av. de las Américas 1024'),
    (3, 'Bodega Centro de Distribución', 'Cuenca', 'Parque Industrial Calle C');
    SET IDENTITY_INSERT dbo.Bodegas OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Existencias WHERE IdExistencia = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Existencias ON;
    INSERT INTO dbo.Existencias (IdExistencia, IdProducto, IdBodega, Stock) VALUES
    (1, 1, 1, 25),
    (2, 1, 2, 10),
    (3, 2, 1, 18),
    (4, 3, 1, 40),
    (5, 3, 2, 25),
    (6, 4, 3, 100);
    SET IDENTITY_INSERT dbo.Existencias OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MovimientosInventario WHERE IdMovimiento = 1)
BEGIN
    SET IDENTITY_INSERT dbo.MovimientosInventario ON;
    INSERT INTO dbo.MovimientosInventario (IdMovimiento, IdProducto, IdBodega, Tipo, Cantidad, Fecha) VALUES
    (1, 1, 1, 'Entrada', 25, SYSUTCDATETIME()),
    (2, 3, 1, 'Entrada', 40, SYSUTCDATETIME());
    SET IDENTITY_INSERT dbo.MovimientosInventario OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ReservasStock WHERE IdReserva = 1)
BEGIN
    SET IDENTITY_INSERT dbo.ReservasStock ON;
    INSERT INTO dbo.ReservasStock (IdReserva, IdProducto, IdBodega, Cantidad, Estado) VALUES
    (1, 1, 1, 2, 'Pendiente');
    SET IDENTITY_INSERT dbo.ReservasStock OFF;
END
GO
