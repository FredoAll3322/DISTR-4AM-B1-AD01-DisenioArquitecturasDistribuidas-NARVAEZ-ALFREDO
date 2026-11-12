# INSTITUTO SUPERIOR TECNOLÓGICO CORDILLERA
## Carrera de Desarrollo de Software
### Aplicaciones Distribuidas – Unidad 1 – Práctica 1 (AD01)
**TEMA:** Diseño de Arquitecturas Distribuidas – Microservicios Independientes para Comercio y Distribución Multibodega

---

## 1. Contexto y Arquitectura General
El sistema implementa una arquitectura basada en **3 Microservicios REST independientes** construidos con **ASP.NET Core Web API (.NET 10)** y **SQL Server**, aplicando el patrón arquitectónico **Database-per-Service** (una base de datos independiente por cada microservicio):

```
                        +--------------------+
                        |  Cliente / Postman |
                        +---------+----------+
                                  |
                                  v
                      +------------------------+
                      |      Pedidos.Api       |
                      | (http://localhost:5003)|
                      |------------------------|
                      | Pedido. Detalle. Cliente|
                      +-----------+------------+
                                  | (Ref. Lógica IdProducto)
          +-----------------------+-----------------------+
          |                                               |
          v                                               v
+------------------------+                     +------------------------+
|      Catalogo.Api      |                     |     Inventario.Api     |
| (http://localhost:5001)|                     | (http://localhost:5002)|
|------------------------|                     |------------------------|
| Producto. Categoria.   |<---(Ref. Lógica)----+ Bodega. Existencia.    |
| Marca                  |     IdProducto      | Movimiento. Reserva    |
+-----------+------------+                     +-----------+------------+
            |                                              |
            v                                              v
     [( CatalogoDB )]                               [( InventarioDB )]
            ^
            |
     [( PedidosDB )] (Asociada a Pedidos.Api)
```

---

## 2. Definición de Reglas de Llaves Foráneas (Físicas vs. Lógicas)

En una arquitectura monolítica tradicional, todas las tablas comparten la misma base de datos y se utiliza integridad referencial estricta mediante `FOREIGN KEY` (FK física). 

En una **Arquitectura Distribuida de Microservicios**, cada servicio posee su propio almacenamiento (*Database per Service*) para garantizar:
1. **Autonomía y Desacoplamiento:** Los microservicios pueden desplegarse, escalarse y evolucionar independientemente.
2. **Aislamiento de Fallos:** Si la base de datos de pedidos falla, el catálogo sigue disponible para consultas.
3. **Escalabilidad y Seguridad:** Ningún microservicio tiene acceso directo ni dependencias de esquema con la base de datos de otro servicio.

### Reglas aplicadas en el sistema:

| Tipo de Llave | Definición y Comportamiento | Ejemplos en el Sistema |
|---|---|---|
| **FK Física (Intra-servicio)** | Restricción `FOREIGN KEY` estándar de SQL Server dentro de la **misma base de datos**. El motor de base de datos valida la existencia de la tupla padre y previene inconsistencias locales. | • `CatalogoDB`: `Productos.IdCategoria` -> `Categorias.IdCategoria`<br>• `CatalogoDB`: `Productos.IdMarca` -> `Marcas.IdMarca`<br>• `InventarioDB`: `Existencias.IdBodega` -> `Bodegas.IdBodega`<br>• `PedidosDB`: `Pedidos.IdCliente` -> `Clientes.IdCliente`<br>• `PedidosDB`: `DetallesPedido.IdPedido` -> `Pedidos.IdPedido` |
| **FK Lógica (Inter-servicio)** | Campo numérico/identificador que referencia una entidad que reside en **otra base de datos o servicio externo**. **NO existe CONSTRAINT físico en SQL Server**, pues las bases de datos son completamente independientes. La validación se gestiona a nivel de lógica de aplicación / APIs. | • `InventarioDB.Existencias.IdProducto` -> Referencia lógica al producto de `Catalogo.Api`<br>• `InventarioDB.MovimientosInventario.IdProducto` -> Referencia lógica al producto de `Catalogo.Api`<br>• `InventarioDB.ReservasStock.IdProducto` -> Referencia lógica al producto de `Catalogo.Api`<br>• `PedidosDB.DetallesPedido.IdProducto` -> Referencia lógica al producto de `Catalogo.Api` |

---

## 3. Detalle de los Microservicios

### 3.1. Catalogo.Api
* **Responsabilidad:** Administrar la información comercial de los productos.
* **Base de datos:** `CatalogoDB`
* **Puerto:** `http://localhost:5001`
* **Swagger UI:** `http://localhost:5001`
* **Entidades y Tablas:**
  * `Categorias`: `IdCategoria`, `Nombre`, `Descripcion`
  * `Marcas`: `IdMarca`, `Nombre`, `PaisOrigen`
  * `Productos`: `IdProducto`, `IdCategoria` (FK física), `IdMarca` (FK física), `Nombre`, `SKU`, `Precio`, `Estado`

### 3.2. Inventario.Api
* **Responsabilidad:** Gestionar el almacenamiento y los movimientos físicos de mercancías en múltiples bodegas.
* **Base de datos:** `InventarioDB`
* **Puerto:** `http://localhost:5002`
* **Swagger UI:** `http://localhost:5002`
* **Entidades y Tablas:**
  * `Bodegas`: `IdBodega`, `Nombre`, `Ciudad`, `Direccion`
  * `Existencias`: `IdExistencia`, `IdProducto` (FK lógica), `IdBodega` (FK física), `Stock`
  * `MovimientosInventario`: `IdMovimiento`, `IdProducto` (FK lógica), `IdBodega` (FK física), `Tipo` (Entrada/Salida/Ajuste), `Cantidad`, `Fecha`
  * `ReservasStock`: `IdReserva`, `IdProducto` (FK lógica), `IdBodega` (FK física), `Cantidad`, `Estado` (Pendiente/Confirmada/Cancelada)

### 3.3. Pedidos.Api
* **Responsabilidad:** Registrar y administrar las órdenes de compra de los clientes.
* **Base de datos:** `PedidosDB`
* **Puerto:** `http://localhost:5003`
* **Swagger UI:** `http://localhost:5003`
* **Entidades y Tablas:**
  * `Clientes`: `IdCliente`, `Nombres`, `Cedula`, `Correo`
  * `Pedidos`: `IdPedido`, `IdCliente` (FK física), `Fecha`, `Total`, `Estado`
  * `DetallesPedido`: `IdDetalle`, `IdPedido` (FK física), `IdProducto` (FK lógica), `Cantidad`, `PrecioUnitario`

---

## 4. Respuestas HTTP Implementadas y Validaciones
Cada endpoint implementa validaciones estrictas (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`) y devuelve códigos de estado HTTP semánticos:
* **`200 OK`**: Solicitud procesada correctamente (GET, PUT, DELETE, PATCH).
* **`201 Created`**: Recurso creado exitosamente (POST), con cabecera `Location` hacia el recurso creado.
* **`400 Bad Request`**: Datos inválidos, SKU repetido, cédula duplicada, stock insuficiente o violación de reglas de negocio.
* **`404 Not Found`**: El recurso solicitado por ID no existe en la base de datos correspondiente.

---

## 5. Pruebas con Postman
Se incluye una colección completa en formato JSON lista para importar en Postman:
* **Ubicación:** `postman/DisenioArquitecturasDistribuidas.postman_collection.json`
* **Variables incluidas:**
  * `catalogo_url` = `http://localhost:5001`
  * `inventario_url` = `http://localhost:5002`
  * `pedidos_url` = `http://localhost:5003`
* **Casos de prueba automatizados en Postman Tests:**
  * Verificación de Códigos de Estado (200, 201, 400, 404).
  * Validación del cálculo del Total en creación de pedidos multilínea.
  * Validación de respuesta ante stock insuficiente.
  * Validación de duplicidad en Cédulas y SKUs.

---

## 6. Instrucciones de Ejecución

### Opción A: Script Automatizado (PowerShell)
En la raíz del proyecto, ejecute:
```powershell
.\start-all.ps1
```
Se abrirán 3 terminales ejecutando los 3 servicios en los puertos 5001, 5002 y 5003.

### Opción B: Ejecución Manual con `dotnet CLI`
Abra tres terminales independientes y ejecute:
```powershell
# Terminal 1 - Catálogo
cd src/Catalogo.Api
dotnet run

# Terminal 2 - Inventario
cd src/Inventario.Api
dotnet run

# Terminal 3 - Pedidos
cd src/Pedidos.Api
dotnet run
```

### Opción C: Visual Studio
1. Abrir `DisenioArquitecturasDistribuidas.slnx`.
2. En el Explorador de soluciones, hacer clic derecho en la Solución -> **Configurar proyectos de inicio**.
3. Seleccionar **Proyectos de inicio múltiples** y poner en `Iniciar` los 3 proyectos (`Catalogo.Api`, `Inventario.Api`, `Pedidos.Api`).
4. Presionar `F5`.

---

## 7. Instrucciones para la Entrega en GitHub

De acuerdo a la rúbrica del docente:
* **Formato del Repositorio:**
  `DISTR-[NIVEL/PARALELO]-[BIMESTRE]-[ACTIVIDAD]-[TEMA]-[APELLIDO/NOMBRE]`
  *Ejemplo:* `DISTR-4AM-B1-AD01-DisenioArquitecturasDistribuidas-PerezJuan`

* **Comandos Git para subir el proyecto:**
```bash
git init
git add .
git commit -m "feat: Implementacion de 3 microservicios independientes con SQL Server y Postman"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/DISTR-4AM-B1-AD01-DisenioArquitecturasDistribuidas-TU_APELLIDO_NOMBRE.git
git push -u origin main
```
*(El archivo `.gitignore` ya está configurado para omitir automáticamente las carpetas `bin/`, `obj/` y archivos temporales).*
