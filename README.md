# INSTITUTO SUPERIOR TECNOLÓGICO CORDILLERA
## CARRERA DE DESARROLLO DE SOFTWARE
### Aplicaciones Distribuidas - Unidad 1 - Práctica 1 (AD01)

* **Tema:** Diseño de Arquitecturas Distribuidas - Microservicios independientes para comercio y distribución multibodega
* **Estudiante:** Alfredo Narváez
* **Nivel / Paralelo:** 4to Nivel - 4AM
* **Docente:** Ing. Diego Maiquez
* **Bimestre:** Primer Bimestre
* **Repositorio:** [https://github.com/FredoAll3322/DISTR-4AM-B1-AD01-DisenioArquitecturasDistribuidas-NARVAEZ-ALFREDO](https://github.com/FredoAll3322/DISTR-4AM-B1-AD01-DisenioArquitecturasDistribuidas-NARVAEZ-ALFREDO)

---

## 1. Descripción de la Actividad
Se diseñaron e implementaron 3 microservicios REST independientes en ASP.NET Core Web API utilizando SQL Server como motor de persistencia, aplicando el patrón arquitectónico **Database-per-Service** (cada microservicio cuenta con su propia base de datos física desacoplada).

El caso de estudio modela una empresa comercializadora de tecnología que requiere separar la información comercial del catálogo, el control físico del stock en múltiples bodegas y la emisión de órdenes de compra.

---

## 2. Microservicios Implementados

### 2.1. Catalogo.Api
* **Responsabilidad:** Administrar la información comercial de productos, categorías y marcas.
* **Base de Datos:** `CatalogoDB`
* **Puerto:** `http://localhost:5001`
* **Tablas:**
  * `Categorias`: IdCategoria, Nombre, Descripcion
  * `Marcas`: IdMarca, Nombre, PaisOrigen
  * `Productos`: IdProducto, IdCategoria, IdMarca, Nombre, SKU, Precio, Estado

### 2.2. Inventario.Api
* **Responsabilidad:** Gestionar las bodegas físicas, el control de stock, movimientos de entrada/salida y reservas de mercancía.
* **Base de Datos:** `InventarioDB`
* **Puerto:** `http://localhost:5002`
* **Tablas:**
  * `Bodegas`: IdBodega, Nombre, Ciudad, Direccion
  * `Existencias`: IdExistencia, IdProducto, IdBodega, Stock
  * `MovimientosInventario`: IdMovimiento, IdProducto, IdBodega, Tipo, Cantidad, Fecha
  * `ReservasStock`: IdReserva, IdProducto, IdBodega, Cantidad, Estado

### 2.3. Pedidos.Api
* **Responsabilidad:** Registro de clientes y procesamiento de órdenes de compra con sus líneas de detalle.
* **Base de Datos:** `PedidosDB`
* **Puerto:** `http://localhost:5003`
* **Tablas:**
  * `Clientes`: IdCliente, Nombres, Cedula, Correo
  * `Pedidos`: IdPedido, IdCliente, Fecha, Total, Estado
  * `DetallesPedido`: IdDetalle, IdPedido, IdProducto, Cantidad, PrecioUnitario

---

## 3. Reglas de Llaves Foráneas (Físicas vs. Lógicas)

En una arquitectura distribuida, no es técnicamente viable ni conceptualmente correcto definir llaves foráneas físicas entre bases de datos diferentes, ya que violaría la autonomía y el desacoplamiento de cada microservicio.

* **Llaves Foráneas Físicas (Intra-servicio):**
  Son aquellas que conectan tablas dentro de la misma base de datos mediante una restricción `FOREIGN KEY` de SQL Server.
  * En `CatalogoDB`: `Productos.IdCategoria` -> `Categorias` y `Productos.IdMarca` -> `Marcas`.
  * En `InventarioDB`: `Existencias.IdBodega` -> `Bodegas`.
  * En `PedidosDB`: `Pedidos.IdCliente` -> `Clientes` y `DetallesPedido.IdPedido` -> `Pedidos`.

* **Llaves Foráneas Lógicas (Inter-servicios):**
  Corresponden a campos de relación entre microservicios que no tienen un constraint físico en base de datos.
  * El campo `IdProducto` en las tablas `Existencias`, `MovimientosInventario` y `ReservasStock` de `InventarioDB` hace referencia lógica a los productos de `Catalogo.Api`.
  * El campo `IdProducto` en la tabla `DetallesPedido` de `PedidosDB` hace referencia lógica al producto vendido del catálogo.
  * La integridad se controla mediante lógica de negocio en los controladores de las APIs.

---

## 4. Códigos de Estado HTTP y Validaciones

Cada controlador implementa validaciones de entrada (`Data Annotations`) y retorna códigos de estado acordes al estándar REST:

* **200 OK:** Respuestas a consultas GET y actualizaciones PUT/PATCH exitosas.
* **201 Created:** Creación de registros por POST, incluyendo la cabecera `Location` con la URL del recurso generado.
* **400 Bad Request:** Datos incompletos, cédula duplicada, SKU repetido o stock insuficiente al solicitar una salida de inventario.
* **404 Not Found:** Cuando se consulta un ID de entidad inexistente.

---

## 5. Pruebas con Postman
Se adjunta la colección de pruebas en formato JSON en la ruta:
`postman/DisenioArquitecturasDistribuidas.postman_collection.json`

Contiene pruebas automatizadas para:
* Listado y consulta por ID de cada entidad.
* Respuestas de error 404 para identificadores no existentes.
* Respuestas de error 400 para reglas de negocio (ej. salida mayor al stock disponible).
* Creación de pedidos con cálculo automático de totales.

---

## 6. Instrucciones de Ejecución

### Requisitos:
* .NET SDK 8 / 9 / 10
* SQL Server (instancia local `localhost` o `.` con autenticación de Windows)

### Ejecución simultánea:
En una consola de PowerShell ubicada en la raíz del proyecto, ejecutar:
```powershell
.\start-all.ps1
```

O abrir la solución `DisenioArquitecturasDistribuidas.slnx` en Visual Studio y configurar en Propiedades de la solución: **Varios proyectos de inicio** -> Poner en **Iniciar** a los 3 proyectos y pulsar **F5**.
