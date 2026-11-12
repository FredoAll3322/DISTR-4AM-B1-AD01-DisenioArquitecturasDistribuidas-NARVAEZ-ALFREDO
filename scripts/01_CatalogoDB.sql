

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CatalogoDB')
BEGIN
    CREATE DATABASE CatalogoDB;
END
GO

USE CatalogoDB;
GO

-- 1. Tabla Categorias
IF OBJECT_ID('dbo.Categorias', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias (
        IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(250) NULL
    );
END
GO

-- 2. Tabla Marcas
IF OBJECT_ID('dbo.Marcas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Marcas (
        IdMarca INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        PaisOrigen NVARCHAR(100) NOT NULL
    );
END
GO

-- 3. Tabla Productos (Con FK físicas internas a Categoria y Marca)
IF OBJECT_ID('dbo.Productos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Productos (
        IdProducto INT IDENTITY(1,1) PRIMARY KEY,
        IdCategoria INT NOT NULL,
        IdMarca INT NOT NULL,
        Nombre NVARCHAR(150) NOT NULL,
        SKU NVARCHAR(50) NOT NULL UNIQUE,
        Precio DECIMAL(18,2) NOT NULL,
        Estado NVARCHAR(20) NOT NULL DEFAULT 'Activo',
        CONSTRAINT FK_Productos_Categorias FOREIGN KEY (IdCategoria) REFERENCES dbo.Categorias(IdCategoria),
        CONSTRAINT FK_Productos_Marcas FOREIGN KEY (IdMarca) REFERENCES dbo.Marcas(IdMarca)
    );
END
GO

-- Datos Semilla (Seed Data)
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE IdCategoria = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Categorias ON;
    INSERT INTO dbo.Categorias (IdCategoria, Nombre, Descripcion) VALUES
    (1, 'Laptops & Portátiles', 'Equipos de cómputo portátiles para trabajo y gaming'),
    (2, 'Smartphones & Tablets', 'Dispositivos móviles inteligentes'),
    (3, 'Periféricos & Accesorios', 'Teclados, mouse, monitores y cables');
    SET IDENTITY_INSERT dbo.Categorias OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Marcas WHERE IdMarca = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Marcas ON;
    INSERT INTO dbo.Marcas (IdMarca, Nombre, PaisOrigen) VALUES
    (1, 'Dell', 'Estados Unidos'),
    (2, 'Lenovo', 'China'),
    (3, 'Samsung', 'Corea del Sur'),
    (4, 'Logitech', 'Suiza');
    SET IDENTITY_INSERT dbo.Marcas OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE IdProducto = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Productos ON;
    INSERT INTO dbo.Productos (IdProducto, IdCategoria, IdMarca, Nombre, SKU, Precio, Estado) VALUES
    (1, 1, 1, 'Dell XPS 15', 'DELL-XPS15-001', 1850.00, 'Activo'),
    (2, 1, 2, 'Lenovo ThinkPad X1', 'LEN-TPX1-002', 1600.00, 'Activo'),
    (3, 2, 3, 'Samsung Galaxy S24 Ultra', 'SAM-S24U-003', 1299.99, 'Activo'),
    (4, 3, 4, 'Mouse Inalámbrico MX Master 3S', 'LOG-MX3S-004', 99.50, 'Activo');
    SET IDENTITY_INSERT dbo.Productos OFF;
END
GO
