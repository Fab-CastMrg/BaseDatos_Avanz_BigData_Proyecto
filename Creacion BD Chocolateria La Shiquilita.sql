CREATE DATABASE ChocolateriaLaShiliquita;
GO

USE ChocolateriaLaShiliquita;
GO

USE ChocolateriaLaShiliquita;
GO

CREATE TABLE roles(
    id_rol INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_roles PRIMARY KEY,
    nombre_rol VARCHAR(50) NULL,
    area_funcional VARCHAR(100) NULL,
    descripcion VARCHAR(200) NULL
);
GO

CREATE TABLE puntos_abastecimiento(
    id_punto INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_puntos_abastecimiento PRIMARY KEY,
    nombre VARCHAR(100) NULL,
    tipo_punto VARCHAR(50) NULL,
    direccion VARCHAR(200) NULL
);
GO

CREATE TABLE categorias_producto(
    id_categoria INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_categorias_producto PRIMARY KEY,
    nombre VARCHAR(100) NULL,
    tipo_dimension VARCHAR(100) NULL,
    id_categoria_padre INT NULL,
    CONSTRAINT FK_categorias_padre FOREIGN KEY(id_categoria_padre) REFERENCES categorias_producto(id_categoria)
);
GO

CREATE TABLE proveedores(
    id_proveedor INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_proveedores PRIMARY KEY,
    razon_social VARCHAR(150) NULL,
    ruc VARCHAR(20) NULL,
    contacto VARCHAR(100) NULL,
    telefono VARCHAR(20) NULL,
    correo VARCHAR(100) NULL,
    direccion VARCHAR(200) NULL
);
GO

CREATE TABLE materias_primas(
    id_materia_prima INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_materias_primas PRIMARY KEY,
    nombre VARCHAR(100) NULL,
    unidad_medida VARCHAR(50) NULL,
    stock_minimo DECIMAL(10,2) NULL,
    descripcion VARCHAR(200) NULL,
    CONSTRAINT CK_materias_stock_minimo CHECK (stock_minimo IS NULL OR stock_minimo >= 0)
);
GO

CREATE TABLE estados_pedido(
    id_estado INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_estados_pedido PRIMARY KEY,
    nombre_estado VARCHAR(50) NULL,
    orden_flujo INT NULL
);
GO

CREATE TABLE tipos_movimiento(
    id_tipo_movimiento INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tipos_movimiento PRIMARY KEY,
    nombre VARCHAR(100) NULL,
    descripcion VARCHAR(200) NULL
);
GO

CREATE TABLE clientes(
    id_cliente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_clientes PRIMARY KEY,
    tipo_documento VARCHAR(50) NULL,
    numero_documento VARCHAR(20) NULL,
    razon_social VARCHAR(150) NULL,
    nombres VARCHAR(100) NULL,
    apellidos VARCHAR(100) NULL,
    correo VARCHAR(100) NULL,
    telefono VARCHAR(20) NULL
);
GO

CREATE TABLE usuarios(
    id_usuario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_usuarios PRIMARY KEY,
    id_rol INT NULL,
    nombres VARCHAR(100) NULL,
    apellidos VARCHAR(100) NULL,
    dni VARCHAR(20) NULL,
    telefono VARCHAR(20) NULL,
    nombre_usuario VARCHAR(50) NULL,
    contrasena VARCHAR(100) NULL,
    CONSTRAINT FK_usuarios_roles FOREIGN KEY(id_rol) REFERENCES roles(id_rol),
    CONSTRAINT UQ_usuarios_nombre_usuario UNIQUE(nombre_usuario)
);
GO

CREATE TABLE cajas(
    id_caja INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_cajas PRIMARY KEY,
    nombre VARCHAR(100) NULL,
    id_punto INT NULL,
    saldo_inicial DECIMAL(10,2) NULL,
    fecha_apertura DATE NULL,
    CONSTRAINT FK_cajas_puntos FOREIGN KEY(id_punto) REFERENCES puntos_abastecimiento(id_punto),
    CONSTRAINT CK_cajas_saldo CHECK (saldo_inicial IS NULL OR saldo_inicial >= 0)
);
GO

CREATE TABLE productos(
    id_producto INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_productos PRIMARY KEY,
    codigo_barras VARCHAR(50) NULL,
    nombre VARCHAR(100) NULL,
    id_categoria INT NULL,
    presentacion VARCHAR(100) NULL,
    porcentaje_cacao DECIMAL(5,2) NULL,
    precio_venta DECIMAL(10,2) NULL,
    precio_mayorista DECIMAL(10,2) NULL,
    CONSTRAINT FK_productos_categorias FOREIGN KEY(id_categoria) REFERENCES categorias_producto(id_categoria),
    CONSTRAINT CK_productos_precios CHECK ((precio_venta IS NULL OR precio_venta >= 0) AND (precio_mayorista IS NULL OR precio_mayorista >= 0))
);
GO

CREATE TABLE compras(
    id_compra INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_compras PRIMARY KEY,
    id_proveedor INT NULL,
    id_usuario INT NULL,
    fecha_compra DATETIME NULL,
    tipo_comprobante VARCHAR(50) NULL,
    numero_comprobante VARCHAR(50) NULL,
    subtotal DECIMAL(10,2) NULL,
    total DECIMAL(10,2) NULL,
    observaciones VARCHAR(200) NULL,
    CONSTRAINT FK_compras_proveedores FOREIGN KEY(id_proveedor) REFERENCES proveedores(id_proveedor),
    CONSTRAINT FK_compras_usuarios FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT CK_compras_montos CHECK ((subtotal IS NULL OR subtotal >= 0) AND (total IS NULL OR total >= 0))
);
GO

CREATE TABLE detalle_compra(
    id_detalle_compra INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_detalle_compra PRIMARY KEY,
    id_compra INT NULL,
    id_materia_prima INT NULL,
    cantidad DECIMAL(10,2) NULL,
    precio_unitario DECIMAL(10,2) NULL,
    subtotal DECIMAL(10,2) NULL,
    CONSTRAINT FK_detalle_compra_compras FOREIGN KEY(id_compra) REFERENCES compras(id_compra),
    CONSTRAINT FK_detalle_compra_materias FOREIGN KEY(id_materia_prima) REFERENCES materias_primas(id_materia_prima),
    CONSTRAINT CK_detalle_compra_valores CHECK ((cantidad IS NULL OR cantidad > 0) AND (precio_unitario IS NULL OR precio_unitario >= 0) AND (subtotal IS NULL OR subtotal >= 0))
);
GO

CREATE TABLE lotes(
    id_lote INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_lotes PRIMARY KEY,
    id_producto INT NULL,
    codigo_lote VARCHAR(50) NULL,
    fecha_produccion DATE NULL,
    fecha_vencimiento DATE NULL,
    cantidad_producida DECIMAL(10,2) NULL,
    observaciones VARCHAR(200) NULL,
    CONSTRAINT FK_lotes_productos FOREIGN KEY(id_producto) REFERENCES productos(id_producto),
    CONSTRAINT CK_lotes_cantidad CHECK (cantidad_producida IS NULL OR cantidad_producida > 0),
    CONSTRAINT CK_lotes_fechas CHECK (fecha_produccion IS NULL OR fecha_vencimiento IS NULL OR fecha_vencimiento > fecha_produccion)
);
GO

CREATE TABLE inventario(
    id_inventario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_inventario PRIMARY KEY,
    id_producto INT NULL,
    id_punto INT NULL,
    cantidad DECIMAL(10,2) NULL,
    fecha_actualizacion DATETIME NULL,
    id_lote INT NULL,
    CONSTRAINT FK_inventario_productos FOREIGN KEY(id_producto) REFERENCES productos(id_producto),
    CONSTRAINT FK_inventario_puntos FOREIGN KEY(id_punto) REFERENCES puntos_abastecimiento(id_punto),
    CONSTRAINT FK_inventario_lotes FOREIGN KEY(id_lote) REFERENCES lotes(id_lote),
    CONSTRAINT CK_inventario_cantidad CHECK (cantidad IS NULL OR cantidad >= 0)
);
GO

CREATE TABLE ventas(
    id_venta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ventas PRIMARY KEY,
    id_cliente INT NULL,
    id_usuario INT NULL,
    id_punto INT NULL,
    fecha_venta DATETIME NULL,
    subtotal DECIMAL(10,2) NULL,
    descuento DECIMAL(10,2) NULL,
    total DECIMAL(10,2) NULL,
    observacion VARCHAR(200) NULL,
    CONSTRAINT FK_ventas_clientes FOREIGN KEY(id_cliente) REFERENCES clientes(id_cliente),
    CONSTRAINT FK_ventas_usuarios FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT FK_ventas_puntos FOREIGN KEY(id_punto) REFERENCES puntos_abastecimiento(id_punto),
    CONSTRAINT CK_ventas_descuento CHECK (descuento IS NULL OR (descuento >= 0 AND descuento <= 10)),
    CONSTRAINT CK_ventas_descuento_umbral CHECK (subtotal IS NULL OR descuento IS NULL OR subtotal > 200 OR descuento = 0),
    CONSTRAINT CK_ventas_total CHECK (subtotal IS NULL OR descuento IS NULL OR total IS NULL OR total = subtotal - descuento)
);
GO

CREATE TABLE detalle_venta(
    id_detalle_venta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_detalle_venta PRIMARY KEY,
    id_venta INT NULL,
    id_producto INT NULL,
    cantidad DECIMAL(10,2) NULL,
    precio_unitario DECIMAL(10,2) NULL,
    subtotal DECIMAL(10,2) NULL,
    CONSTRAINT FK_detalle_venta_ventas FOREIGN KEY(id_venta) REFERENCES ventas(id_venta),
    CONSTRAINT FK_detalle_venta_productos FOREIGN KEY(id_producto) REFERENCES productos(id_producto),
    CONSTRAINT CK_detalle_venta_valores CHECK ((cantidad IS NULL OR cantidad > 0) AND (precio_unitario IS NULL OR precio_unitario >= 0) AND (subtotal IS NULL OR subtotal >= 0))
);
GO

CREATE TABLE pedidos(
    id_pedido INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_pedidos PRIMARY KEY,
    id_cliente INT NULL,
    id_punto INT NULL,
    id_usuario INT NULL,
    id_estado INT NULL,
    fecha_pedido DATETIME NULL,
    numero_cotizacion VARCHAR(50) NULL,
    subtotal DECIMAL(10,2) NULL,
    descuento_monto DECIMAL(10,2) NULL,
    total DECIMAL(10,2) NULL,
    observaciones VARCHAR(250) NULL,
    CONSTRAINT FK_pedidos_clientes FOREIGN KEY(id_cliente) REFERENCES clientes(id_cliente),
    CONSTRAINT FK_pedidos_puntos FOREIGN KEY(id_punto) REFERENCES puntos_abastecimiento(id_punto),
    CONSTRAINT FK_pedidos_usuarios FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT FK_pedidos_estados FOREIGN KEY(id_estado) REFERENCES estados_pedido(id_estado),
    CONSTRAINT CK_pedidos_descuento CHECK (descuento_monto IS NULL OR (descuento_monto >= 0 AND descuento_monto <= 10)),
    CONSTRAINT CK_pedidos_descuento_umbral CHECK (subtotal IS NULL OR descuento_monto IS NULL OR subtotal > 200 OR descuento_monto = 0),
    CONSTRAINT CK_pedidos_total CHECK (subtotal IS NULL OR descuento_monto IS NULL OR total IS NULL OR total = subtotal - descuento_monto)
);
GO

CREATE TABLE detalle_pedido(
    id_detalle_pedido INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_detalle_pedido PRIMARY KEY,
    id_pedido INT NULL,
    id_producto INT NULL,
    cantidad DECIMAL(10,2) NULL,
    precio_unitario DECIMAL(10,2) NULL,
    subtotal DECIMAL(10,2) NULL,
    CONSTRAINT FK_detalle_pedido_pedidos FOREIGN KEY(id_pedido) REFERENCES pedidos(id_pedido),
    CONSTRAINT FK_detalle_pedido_productos FOREIGN KEY(id_producto) REFERENCES productos(id_producto),
    CONSTRAINT CK_detalle_pedido_valores CHECK ((cantidad IS NULL OR cantidad > 0) AND (precio_unitario IS NULL OR precio_unitario >= 0) AND (subtotal IS NULL OR subtotal >= 0))
);
GO

CREATE TABLE pagos(
    id_pago INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_pagos PRIMARY KEY,
    id_pedido INT NULL,
    id_venta INT NULL,
    id_usuario INT NULL,
    metodo_pago VARCHAR(50) NULL,
    monto DECIMAL(10,2) NULL,
    fecha_pago DATETIME NULL,
    numero_operacion VARCHAR(100) NULL,
    observacion VARCHAR(200) NULL,
    CONSTRAINT FK_pagos_pedidos FOREIGN KEY(id_pedido) REFERENCES pedidos(id_pedido),
    CONSTRAINT FK_pagos_ventas FOREIGN KEY(id_venta) REFERENCES ventas(id_venta),
    CONSTRAINT FK_pagos_usuarios FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT CK_pagos_monto CHECK (monto IS NULL OR monto >= 0),
    CONSTRAINT CK_pagos_origen CHECK ((id_pedido IS NOT NULL AND id_venta IS NULL) OR (id_pedido IS NULL AND id_venta IS NOT NULL))
);
GO

CREATE TABLE ingresos(
    id_ingreso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ingresos PRIMARY KEY,
    id_caja INT NULL,
    id_tipo_movimiento INT NULL,
    id_pedido INT NULL,
    id_usuario INT NULL,
    fecha_ingreso DATETIME NULL,
    monto DECIMAL(10,2) NULL,
    concepto VARCHAR(150) NULL,
    observacion VARCHAR(200) NULL,
    id_pago INT NULL,
    CONSTRAINT FK_ingresos_cajas FOREIGN KEY(id_caja) REFERENCES cajas(id_caja),
    CONSTRAINT FK_ingresos_tipos FOREIGN KEY(id_tipo_movimiento) REFERENCES tipos_movimiento(id_tipo_movimiento),
    CONSTRAINT FK_ingresos_pedidos FOREIGN KEY(id_pedido) REFERENCES pedidos(id_pedido),
    CONSTRAINT FK_ingresos_usuarios FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT FK_ingresos_pagos FOREIGN KEY(id_pago) REFERENCES pagos(id_pago),
    CONSTRAINT CK_ingresos_monto CHECK (monto IS NULL OR monto >= 0)
);
GO

CREATE TABLE egresos(
    id_egreso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_egresos PRIMARY KEY,
    id_caja INT NULL,
    id_tipo_movimiento INT NULL,
    id_compra INT NULL,
    id_usuario INT NULL,
    fecha_egreso DATETIME NULL,
    monto DECIMAL(10,2) NULL,
    concepto VARCHAR(150) NULL,
    observacion VARCHAR(200) NULL,
    CONSTRAINT FK_egresos_cajas FOREIGN KEY(id_caja) REFERENCES cajas(id_caja),
    CONSTRAINT FK_egresos_tipos FOREIGN KEY(id_tipo_movimiento) REFERENCES tipos_movimiento(id_tipo_movimiento),
    CONSTRAINT FK_egresos_compras FOREIGN KEY(id_compra) REFERENCES compras(id_compra),
    CONSTRAINT FK_egresos_usuarios FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT CK_egresos_monto CHECK (monto IS NULL OR monto >= 0)
);
GO
