USE ChocolateriaLaShiliquita;
GO
SET NOCOUNT ON;
GO

-- roles
INSERT INTO roles (nombre_rol, area_funcional, descripcion)
VALUES
    ('Gerente', 'Gerencia', 'Dirige el negocio y supervisa todas las areas'),
    ('Administrador', 'Administracion', 'Gestiona compras, proveedores, inventario y finanzas'),
    ('Maestro Chocolatero', 'Produccion', 'Elabora los chocolates y controla los lotes'),
    ('Vendedor', 'Ventas', 'Atiende a los clientes y registra ventas y pedidos');
GO

-- puntos_abastecimiento
INSERT INTO puntos_abastecimiento (nombre, tipo_punto, direccion)
VALUES
    ('Tienda Principal', 'Tienda', 'Av. Miguel Grau 148, Cajamarca'),
    ('Hacienda La Colpa', 'Puesto de venta', 'Hacienda La Colpa, Cajamarca'),
    ('Panaderia Campos', 'Panaderia', 'Av. De los Héroes 780, Cajamarca'),
    ('Restaurante "Castope"', 'Restaurante', 'Jr. Del Comercio, Cajamarca'),
    ('Aeropuerto Internacional Armando Revoredo Iglesias', 'Aeropuerto', 'Aeropuerto Internacional Armando Revoredo Iglesias, Cajamarca');
GO

-- categorias_producto
INSERT INTO categorias_producto (nombre, tipo_dimension, id_categoria_padre)
VALUES
    ('Chocolates', 'Peso (g)', NULL),
    ('Snacks con Chocolate', 'Peso (g)', NULL),
    ('Derivados del Cacao', 'Peso (g)', NULL),
    ('Chocolate para Comer', 'Peso (g)', 1),
    ('Chocolate sin Azucar', 'Peso (g)', 1),
    ('Chocolate con Panela', 'Peso (g)', 1),
    ('Chocolate de Taza y Pasta', 'Peso (g)', 1),
    ('Cacao en Polvo', 'Peso (g)', 3),
    ('Infusiones de Cacao', 'Peso (g)', 3);
GO

-- proveedores
INSERT INTO proveedores (razon_social, ruc, contacto, telefono, correo, direccion)
VALUES
    ('Cacao Norte Peru S.A.C.', '20604578121', 'Luis Mendoza', '987654321', 'ventas@cacaonorte.pe', 'Jaen, Cajamarca'),
    ('Insumos Andinos del Peru E.I.R.L.', '20607845213', 'Rosa Huaman', '986214753', 'contacto@insumosandinos.pe', 'Cercado de Lima, Lima'),
    ('Panela Organica Cajamarca S.A.C.', '20609123456', 'Jose Salazar', '984563210', 'ventas@panelacajamarca.pe', 'San Marcos, Cajamarca'),
    ('Frutos Secos del Peru S.A.C.', '20587456321', 'Andrea Torres', '981452367', 'pedidos@frutossecosperu.pe', 'La Victoria, Lima'),
    ('Empaques y Envases Peru S.R.L.', '20603214587', 'Carlos Rojas', '989321654', 'ventas@empaquesperu.pe', 'Ate, Lima');
GO

-- materias_primas
INSERT INTO materias_primas (nombre, unidad_medida, stock_minimo, descripcion)
VALUES
    ('Grano de cacao', 'kg', CAST(50.00 AS Decimal(10, 2)), 'Cacao utilizado para la elaboracion de pasta y chocolate'),
    ('Manteca de cacao', 'kg', CAST(20.00 AS Decimal(10, 2)), 'Grasa natural utilizada en la produccion de chocolate'),
    ('Azucar rubia', 'kg', CAST(30.00 AS Decimal(10, 2)), 'Azucar utilizada en chocolates tradicionales'),
    ('Panela granulada', 'kg', CAST(20.00 AS Decimal(10, 2)), 'Endulzante utilizado en chocolates con panela'),
    ('Leche en polvo', 'kg', CAST(15.00 AS Decimal(10, 2)), 'Ingrediente para chocolates con leche y chocolate blanco'),
    ('Almendras', 'kg', CAST(10.00 AS Decimal(10, 2)), 'Fruto seco utilizado en snacks con chocolate'),
    ('Aguaymanto deshidratado', 'kg', CAST(10.00 AS Decimal(10, 2)), 'Fruta deshidratada utilizada en snacks con chocolate'),
    ('Cafe molido', 'kg', CAST(5.00 AS Decimal(10, 2)), 'Cafe utilizado en chocolate sabor cafe'),
    ('Stevia', 'kg', CAST(3.00 AS Decimal(10, 2)), 'Endulzante utilizado en chocolates sin azucar'),
    ('Bolsas para producto', 'unidad', CAST(200.00 AS Decimal(10, 2)), 'Empaque para cacao, te y snacks'),
    ('Envolturas para barras', 'unidad', CAST(500.00 AS Decimal(10, 2)), 'Empaque individual para barras de chocolate'),
    ('Cajas de carton', 'unidad', CAST(100.00 AS Decimal(10, 2)), 'Empaque externo para productos');
GO

-- estados_pedido
INSERT INTO estados_pedido (nombre_estado, orden_flujo)
VALUES
    ('Cotizado', 1),
    ('Confirmado', 2),
    ('En preparacion', 3),
    ('Listo para entrega', 4),
    ('Entregado', 5),
    ('Cancelado', 6);
GO

-- tipos_movimiento
INSERT INTO tipos_movimiento (nombre, descripcion)
VALUES
    ('Venta', 'Ingreso generado por venta directa'),
    ('Pedido', 'Ingreso generado por pedido de cliente'),
    ('Compra de materia prima', 'Egreso por adquisicion de insumos'),
    ('Gasto operativo', 'Pago relacionado con operacion del negocio'),
    ('Transporte', 'Pago por transporte o distribucion'),
    ('Servicios', 'Pago de servicios utilizados por el negocio');
GO

-- usuarios
INSERT INTO usuarios (id_rol, nombres, apellidos, dni, telefono, nombre_usuario, contrasena)
VALUES
    (1, 'Paul', 'Escalante Pereira', '71230451', '944123456', 'pescalante', 'paul2026'),
    (2, 'Deidemia', 'Pisco Portal', '42817365', '955234567', 'dpisco_admin', 'deidemia2026'),
    (3, 'Deidemia', 'Pisco Portal', '42817365', '955234567', 'dpisco_choco', 'chocolatera2026'),
    (4, 'Fabian', 'Castañeda Murga', '60965471', '966345678', 'fcastaneda', 'fabian2026'),
    (4, 'Jhudit', 'Llaxa Lozano', '60503650', '977456789', 'jllaxa', 'jhudit2026');
GO

-- cajas
INSERT INTO cajas (nombre, id_punto, saldo_inicial, fecha_apertura)
VALUES
    ('Caja Principal Tienda', 1, CAST(800.00 AS Decimal(10, 2)), CAST('2026-04-01' AS Date)),
    ('Caja Hacienda La Colpa', 2, CAST(400.00 AS Decimal(10, 2)), CAST('2026-04-01' AS Date));
GO

-- productos
INSERT INTO productos (codigo_barras, nombre, id_categoria, presentacion, porcentaje_cacao, precio_venta, precio_mayorista)
VALUES
    ('7750001000011', 'Chocolate Pasta 100% Cacao 180 g', 7, 'Pasta 180 g', CAST(100.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000028', 'Chocolate Pasta 100% Cacao 80 g', 7, 'Pasta 80 g', CAST(100.00 AS Decimal(5, 2)), CAST(9.00 AS Decimal(10, 2)), CAST(7.50 AS Decimal(10, 2))),
    ('7750001000035', 'Chocolate 90% Cacao 60 g', 4, 'Barra 60 g', CAST(90.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000042', 'Chocolate 70% Cacao 60 g', 4, 'Barra 60 g', CAST(70.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000059', 'Chocolate 50% Cacao 60 g', 4, 'Barra 60 g', CAST(50.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000066', 'Chocolate con Cafe 45% Cacao 60 g', 4, 'Barra 60 g', CAST(45.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000073', 'Chocolate Blanco 60 g', 4, 'Barra 60 g', NULL, CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000080', 'Chocolate 70% sin Azucar 60 g', 5, 'Barra 60 g', CAST(70.00 AS Decimal(5, 2)), CAST(18.00 AS Decimal(10, 2)), CAST(15.50 AS Decimal(10, 2))),
    ('7750001000097', 'Chocolate 50% sin Azucar 60 g', 5, 'Barra 60 g', CAST(50.00 AS Decimal(5, 2)), CAST(18.00 AS Decimal(10, 2)), CAST(15.50 AS Decimal(10, 2))),
    ('7750001000103', 'Chocolate 70% con Panela 60 g', 6, 'Barra 60 g', CAST(70.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000110', 'Chocolate 50% con Panela 60 g', 6, 'Barra 60 g', CAST(50.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000127', 'Almendras con Chocolate 50% Cacao 90 g', 2, 'Bolsa 90 g', CAST(50.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000134', 'Aguaymantos con Chocolate 50% Cacao 90 g', 2, 'Bolsa 90 g', CAST(50.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000141', 'Cacao en Polvo 100% Cacao 200 g', 8, 'Bolsa 200 g', CAST(100.00 AS Decimal(5, 2)), CAST(17.00 AS Decimal(10, 2)), CAST(14.50 AS Decimal(10, 2))),
    ('7750001000158', 'Te de Cacao 80 g', 9, 'Bolsa 80 g', NULL, CAST(9.00 AS Decimal(10, 2)), CAST(7.50 AS Decimal(10, 2)));
GO