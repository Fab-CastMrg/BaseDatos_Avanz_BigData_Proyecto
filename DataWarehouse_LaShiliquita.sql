CREATE DATABASE Modelo_Fisico_DataWarehouse;
GO
USE Modelo_Fisico_DataWarehouse;
GO
--DIMENSIONES
CREATE TABLE Cliente (
    ID_Cliente INT PRIMARY KEY,
    Id_Cliente_Origen VARCHAR(50),
    Tipo_Documento VARCHAR(50),
    Numero_Documento VARCHAR(50),
    Tipo_Cliente VARCHAR(50),
    Nombre_Cliente VARCHAR(150)
);

CREATE TABLE Producto (
    Id_Producto INT PRIMARY KEY,
    Categoría VARCHAR(100),
    Nombre_Producto VARCHAR(150),
    Precio_Venta DECIMAL(10, 2),
    Precio_Mayorista DECIMAL(10, 2)
);

CREATE TABLE Vendedor (
    Id_Vendedor INT PRIMARY KEY,
    DNI_Vendedor VARCHAR(20),
    Nombre_Vendedor VARCHAR(150)
);

CREATE TABLE Sede (
    Id_Sede INT PRIMARY KEY,
    Nombre_Sede VARCHAR(100),
    Tipo_Lugar VARCHAR(50),
    Ubicacion VARCHAR(150)
);

CREATE TABLE Sede_Almacen (
    Id_Sede_Almacen INT PRIMARY KEY,
    Nombre_Sede_Almacen VARCHAR(100)
);

CREATE TABLE Tiempo (
    Id_Tiempo INT PRIMARY KEY,
    Fecha DATE,
    Fecha_Pedido DATE,
    Fecha_Pago DATE,
    Dia INT,
    Mes INT,
    Anio INT,
    Fecha_Festiva VARCHAR(10),
    Nombre_Fecha_Festiva VARCHAR(100),
    Fecha_Campania VARCHAR(10),
    Nombre_Campania VARCHAR(100)
);

CREATE TABLE Inventario (
    Id_Inventario INT PRIMARY KEY,
    Codigo_Lote VARCHAR(50),
    Fecha_Produccion DATE,
    Fecha_Vencimiento DATE
);

CREATE TABLE Gastos (
    Id_Gastos INT PRIMARY KEY,
    Costo_Total DECIMAL(12, 2)
);

CREATE TABLE Egresos (
    Id_Egresos INT PRIMARY KEY,
    Tipo_Egreso VARCHAR(100)
);

CREATE TABLE Compras_Materia_Prima (
    Id_Compras_Materia_Prima INT PRIMARY KEY,
    Detalle_Compra VARCHAR(255)
);

CREATE TABLE Medio_Pago (
    Id_Medio_Pago INT PRIMARY KEY,
    Metodo_Pago VARCHAR(100)
);

CREATE TABLE Tipo_Venta (
    Id_Tipo_Venta INT PRIMARY KEY,
    Tipo_Venta VARCHAR(100)
);

CREATE TABLE Venta (
    Id_Venta INT PRIMARY KEY,
    Detalle_Venta VARCHAR(255),
    Cantidad_Productos_Vendidos INT,
    Total_Vendido DECIMAL(12, 2)
);

-- 2.5 TABLAS DE HECHOS
-- Esquema 1: Venta y Rentabilidad
CREATE TABLE Venta_Rentabilidad (
    Id_Venta_Rentabilidad INT PRIMARY KEY,
    Id_Tiempo INT,
    Id_Cliente INT,
    Id_Producto INT,
    Id_Vendedor INT,
    Id_Sede INT,
    Numero_Documento_Origen VARCHAR(50),
    Origen VARCHAR(50),
    Cantidad INT,
    Subtotal DECIMAL(10, 2),
    Descuento DECIMAL(10, 2),
    Total_Neto DECIMAL(10, 2),
    Ganancia DECIMAL(10, 2),
    
    CONSTRAINT FK_VentaR_Tiempo FOREIGN KEY (Id_Tiempo) REFERENCES Tiempo(Id_Tiempo),
    CONSTRAINT FK_VentaR_Cliente FOREIGN KEY (Id_Cliente) REFERENCES Cliente(ID_Cliente),
    CONSTRAINT FK_VentaR_Producto FOREIGN KEY (Id_Producto) REFERENCES Producto(Id_Producto),
    CONSTRAINT FK_VentaR_Vendedor FOREIGN KEY (Id_Vendedor) REFERENCES Vendedor(Id_Vendedor),
    CONSTRAINT FK_VentaR_Sede FOREIGN KEY (Id_Sede) REFERENCES Sede(Id_Sede)
);

-- Esquema 2: Ganancias por Sede
CREATE TABLE Ganancias_Sede (
    Id_Ganancias_Sede INT PRIMARY KEY,
    Id_Sede INT,
    Id_Gastos INT,
    Id_Tiempo INT,
    Id_Venta INT,
    Ganancia_Total DECIMAL(12, 2),
    
    CONSTRAINT FK_GananciasS_Sede FOREIGN KEY (Id_Sede) REFERENCES Sede(Id_Sede),
    CONSTRAINT FK_GananciasS_Gastos FOREIGN KEY (Id_Gastos) REFERENCES Gastos(Id_Gastos),
    CONSTRAINT FK_GananciasS_Tiempo FOREIGN KEY (Id_Tiempo) REFERENCES Tiempo(Id_Tiempo),
    CONSTRAINT FK_GananciasS_Venta FOREIGN KEY (Id_Venta) REFERENCES Venta(Id_Venta)
);

-- Esquema 3: Control de Inventario
CREATE TABLE Control_Inventario (
    Id_Control_Inventario INT PRIMARY KEY,
    Id_Producto INT,
    Id_Tiempo INT,
    Id_Venta INT,
    Id_Inventario INT,
    Id_Sede_Almacen INT,
    Cantidad_Inventario INT,
    Valor_Monetario_Inventario DECIMAL(12, 2),
    Dias_Para_Vencer INT,
    Cantidad_Vendida INT,
    
    CONSTRAINT FK_ControlI_Producto FOREIGN KEY (Id_Producto) REFERENCES Producto(Id_Producto),
    CONSTRAINT FK_ControlI_Tiempo FOREIGN KEY (Id_Tiempo) REFERENCES Tiempo(Id_Tiempo),
    CONSTRAINT FK_ControlI_Venta FOREIGN KEY (Id_Venta) REFERENCES Venta(Id_Venta),
    CONSTRAINT FK_ControlI_Inventario FOREIGN KEY (Id_Inventario) REFERENCES Inventario(Id_Inventario),
    CONSTRAINT FK_ControlI_SedeAlmacen FOREIGN KEY (Id_Sede_Almacen) REFERENCES Sede_Almacen(Id_Sede_Almacen)
);

-- Esquema 4: Porcentaje de Gasto
CREATE TABLE Porcentaje_Gasto (
    Id_Porcentaje_Gasto INT PRIMARY KEY,
    Id_Egresos INT,
    Id_Tiempo INT,
    Id_Compras_Materia_Prima INT,
    Total_Compras DECIMAL(12, 2),
    Costo_Total_Insumos DECIMAL(12, 2),
    Egreso_Total DECIMAL(12, 2),
    
    CONSTRAINT FK_PorcentajeG_Egresos FOREIGN KEY (Id_Egresos) REFERENCES Egresos(Id_Egresos),
    CONSTRAINT FK_PorcentajeG_Tiempo FOREIGN KEY (Id_Tiempo) REFERENCES Tiempo(Id_Tiempo),
    CONSTRAINT FK_PorcentajeG_ComprasMP FOREIGN KEY (Id_Compras_Materia_Prima) REFERENCES Compras_Materia_Prima(Id_Compras_Materia_Prima)
);

-- Esquema 5: Pago de Clientes
CREATE TABLE Pago_Clientes (
    Id_Pago_Cliente INT PRIMARY KEY,
    Id_Medio_Pago INT,
    Id_Cliente INT,
    Id_Tiempo INT,
    Id_Tipo_Venta INT,
    Monto DECIMAL(12, 2),
    Dias_Hasta_Pago INT,
    
    CONSTRAINT FK_PagoC_MedioPago FOREIGN KEY (Id_Medio_Pago) REFERENCES Medio_Pago(Id_Medio_Pago),
    CONSTRAINT FK_PagoC_Cliente FOREIGN KEY (Id_Cliente) REFERENCES Cliente(ID_Cliente),
    CONSTRAINT FK_PagoC_Tiempo FOREIGN KEY (Id_Tiempo) REFERENCES Tiempo(Id_Tiempo),
    CONSTRAINT FK_PagoC_TipoVenta FOREIGN KEY (Id_Tipo_Venta) REFERENCES Tipo_Venta(Id_Tipo_Venta)
);