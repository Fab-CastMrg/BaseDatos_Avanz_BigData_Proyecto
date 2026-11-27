using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LaShiliquita.Entidades;

namespace LaShiliquita.Datos
{
    public class InventarioDatos
    {
        // ---------- Consultas ----------

        // Resumen por producto de una sede entre dos fechas (ambas incluidas).
        // - El SALDO_INICIAL del sistema siempre cuenta como stock inicial, sea cual sea el rango.
        // - Stock final = stock inicial + ingresadas - vendidas - salidas + ajustes.
        public List<ResumenInventario> ObtenerResumen(int idPunto, DateTime desde, DateTime hasta)
        {
            List<ResumenInventario> lista = new List<ResumenInventario>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql =
                    "SELECT p.id_producto, p.nombre, p.presentacion, p.stock_minimo, " +
                    "(SELECT ISNULL(SUM(i.cantidad), 0) FROM inventario i " +
                    " WHERE i.id_producto = p.id_producto AND i.id_punto = @punto) AS stock_actual, " +
                    "ISNULL(SUM(CASE WHEN m.tipo = 'SALDO_INICIAL' OR m.fecha < @desde THEN m.cantidad ELSE 0 END), 0) AS stock_inicial, " +
                    "ISNULL(SUM(CASE WHEN m.tipo IN ('INGRESO', 'PRODUCCION') AND m.fecha >= @desde AND m.fecha < @hasta THEN m.cantidad ELSE 0 END), 0) AS ingresadas, " +
                    "ISNULL(SUM(CASE WHEN m.tipo = 'VENTA' AND m.fecha >= @desde AND m.fecha < @hasta THEN -m.cantidad ELSE 0 END), 0) AS vendidas, " +
                    "ISNULL(SUM(CASE WHEN m.tipo = 'SALIDA' AND m.fecha >= @desde AND m.fecha < @hasta THEN -m.cantidad ELSE 0 END), 0) AS salidas, " +
                    "ISNULL(SUM(CASE WHEN m.tipo = 'AJUSTE' AND m.fecha >= @desde AND m.fecha < @hasta THEN m.cantidad ELSE 0 END), 0) AS ajustes, " +
                    "ISNULL(SUM(CASE WHEN m.tipo = 'SALDO_INICIAL' OR m.fecha < @hasta THEN m.cantidad ELSE 0 END), 0) AS stock_final " +
                    "FROM productos p " +
                    "LEFT JOIN movimientos_inventario m ON m.id_producto = p.id_producto AND m.id_punto = @punto " +
                    "GROUP BY p.id_producto, p.nombre, p.presentacion, p.stock_minimo " +
                    "ORDER BY p.nombre";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@punto", idPunto);
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1));   // el dia final se incluye completo
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        ResumenInventario fila = new ResumenInventario();
                        fila.IdProducto = Convert.ToInt32(dr["id_producto"]);
                        fila.Nombre = dr["nombre"].ToString();
                        fila.Presentacion = dr["presentacion"].ToString();
                        fila.StockActual = Convert.ToDecimal(dr["stock_actual"]);
                        fila.StockInicial = Convert.ToDecimal(dr["stock_inicial"]);
                        fila.Ingresadas = Convert.ToDecimal(dr["ingresadas"]);
                        fila.Vendidas = Convert.ToDecimal(dr["vendidas"]);
                        fila.Salidas = Convert.ToDecimal(dr["salidas"]);
                        fila.Ajustes = Convert.ToDecimal(dr["ajustes"]);
                        fila.StockFinal = Convert.ToDecimal(dr["stock_final"]);

                        if (dr["stock_minimo"] == DBNull.Value)
                        {
                            fila.TieneMinimo = false;
                            fila.StockMinimo = 0;
                        }
                        else
                        {
                            fila.TieneMinimo = true;
                            fila.StockMinimo = Convert.ToDecimal(dr["stock_minimo"]);
                        }

                        lista.Add(fila);
                    }
                }
            }

            return lista;
        }

        // Historial de movimientos de una sede entre dos fechas (ambas incluidas), del mas reciente al mas antiguo.
        // idProducto = 0 trae todos los productos; tipo = "" trae todos los tipos. Maximo 1000 filas.
        public List<MovimientoInventario> ListarMovimientos(int idPunto, DateTime desde, DateTime hasta, int idProducto, string tipo)
        {
            List<MovimientoInventario> lista = new List<MovimientoInventario>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql =
                    "SELECT TOP 1000 m.id_movimiento, m.fecha, m.id_producto, p.nombre AS producto, m.tipo, m.cantidad, " +
                    "l.codigo_lote, m.motivo, u.nombres, u.apellidos, m.id_venta " +
                    "FROM movimientos_inventario m " +
                    "INNER JOIN productos p ON p.id_producto = m.id_producto " +
                    "LEFT JOIN lotes l ON l.id_lote = m.id_lote " +
                    "LEFT JOIN usuarios u ON u.id_usuario = m.id_usuario " +
                    "WHERE m.id_punto = @punto AND m.fecha >= @desde AND m.fecha < @hasta " +
                    "AND (@producto = 0 OR m.id_producto = @producto) " +
                    "AND (@tipo = '' OR m.tipo = @tipo) " +
                    "ORDER BY m.fecha DESC, m.id_movimiento DESC";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@punto", idPunto);
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1));
                cmd.Parameters.AddWithValue("@producto", idProducto);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        MovimientoInventario mov = new MovimientoInventario();
                        mov.IdMovimiento = Convert.ToInt32(dr["id_movimiento"]);
                        mov.Fecha = Convert.ToDateTime(dr["fecha"]);
                        mov.IdProducto = Convert.ToInt32(dr["id_producto"]);
                        mov.Producto = dr["producto"].ToString();
                        mov.Tipo = dr["tipo"].ToString();
                        mov.Cantidad = Convert.ToDecimal(dr["cantidad"]);
                        mov.CodigoLote = dr["codigo_lote"].ToString();
                        mov.Motivo = dr["motivo"].ToString();
                        mov.Usuario = (dr["nombres"].ToString() + " " + dr["apellidos"].ToString()).Trim();
                        mov.IdVenta = dr["id_venta"] == DBNull.Value ? 0 : Convert.ToInt32(dr["id_venta"]);
                        lista.Add(mov);
                    }
                }
            }

            return lista;
        }

        // Lotes de un producto con el stock que tienen en la sede (0 si aun no tienen filas de inventario ahi)
        public List<LoteStock> ListarLotes(int idProducto, int idPunto)
        {
            List<LoteStock> lista = new List<LoteStock>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql =
                    "SELECT l.id_lote, l.codigo_lote, l.fecha_vencimiento, " +
                    "(SELECT ISNULL(SUM(i.cantidad), 0) FROM inventario i " +
                    " WHERE i.id_lote = l.id_lote AND i.id_punto = @punto) AS stock " +
                    "FROM lotes l WHERE l.id_producto = @producto " +
                    "ORDER BY CASE WHEN l.fecha_vencimiento IS NULL THEN 1 ELSE 0 END, l.fecha_vencimiento, l.id_lote";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@producto", idProducto);
                cmd.Parameters.AddWithValue("@punto", idPunto);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        LoteStock lote = new LoteStock();
                        lote.IdLote = Convert.ToInt32(dr["id_lote"]);
                        lote.CodigoLote = dr["codigo_lote"].ToString();
                        if (dr["fecha_vencimiento"] != DBNull.Value)
                        {
                            lote.FechaVencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]);
                        }
                        lote.StockEnSede = Convert.ToDecimal(dr["stock"]);
                        lista.Add(lote);
                    }
                }
            }

            return lista;
        }

        // ---------- Operaciones (cada una en UNA transaccion: inventario + movimientos_inventario) ----------
        // Si algo falla se hace Rollback. InvalidOperationException = problema para mostrar al usuario
        // (stock insuficiente, lote que no corresponde); SqlException = fallo de la base de datos.

        // Suma unidades al stock. idLote <= 0 significa "sin lote".
        public void RegistrarIngreso(int idPunto, int idProducto, int idLote, decimal cantidad, string motivo, int idUsuario)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    AgregarStock(cn, tran, idPunto, idProducto, idLote, cantidad, "INGRESO", motivo, idUsuario);
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        // Resta unidades del stock, primero del lote que vence antes (FEFO)
        public void RegistrarSalida(int idPunto, int idProducto, decimal cantidad, string motivo, int idUsuario)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    DescontarFefo(cn, tran, idPunto, idProducto, cantidad, "SALIDA", motivo, idUsuario);
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        // Ajuste con signo: positivo suma (en el lote indicado o sin lote); negativo resta por FEFO
        public void RegistrarAjuste(int idPunto, int idProducto, int idLote, decimal cantidad, string motivo, int idUsuario)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    if (cantidad > 0)
                    {
                        AgregarStock(cn, tran, idPunto, idProducto, idLote, cantidad, "AJUSTE", motivo, idUsuario);
                    }
                    else
                    {
                        DescontarFefo(cn, tran, idPunto, idProducto, -cantidad, "AJUSTE", motivo, idUsuario);
                    }
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        // ---------- Ayudas privadas ----------

        // Fila de inventario (un lote de un producto en la sede). Solo la usa DescontarFefo.
        private class FilaStock
        {
            public int IdInventario;
            public object IdLote;       // DBNull.Value si la fila no tiene lote
            public decimal Cantidad;
        }

        // Agrega un parametro entero que puede ser NULL (valor es un int o DBNull.Value)
        private void AgregarParametroLote(SqlCommand cmd, object valor)
        {
            SqlParameter p = cmd.Parameters.Add("@lote", SqlDbType.Int);
            p.Value = valor;
        }

        // Suma 'cantidad' (positiva) a la fila de inventario del producto/sede/lote (la crea si no existe)
        // y registra el movimiento con ese tipo.
        private void AgregarStock(SqlConnection cn, SqlTransaction tran, int idPunto, int idProducto, int idLote,
                                  decimal cantidad, string tipo, string motivo, int idUsuario)
        {
            object lote = idLote > 0 ? (object)idLote : DBNull.Value;

            // El lote elegido debe ser de este producto
            if (idLote > 0)
            {
                SqlCommand cmdLote = new SqlCommand("SELECT COUNT(*) FROM lotes WHERE id_lote = @lote AND id_producto = @producto", cn, tran);
                cmdLote.Parameters.AddWithValue("@lote", idLote);
                cmdLote.Parameters.AddWithValue("@producto", idProducto);

                if (Convert.ToInt32(cmdLote.ExecuteScalar()) == 0)
                {
                    throw new InvalidOperationException("El lote elegido no corresponde a este producto.");
                }
            }

            // Fila de inventario existente para ese producto, sede y lote (o sin lote)
            string sqlBuscar = "SELECT TOP 1 id_inventario FROM inventario " +
                               "WHERE id_producto = @producto AND id_punto = @punto " +
                               "AND ((@lote IS NULL AND id_lote IS NULL) OR id_lote = @lote) " +
                               "ORDER BY id_inventario";
            SqlCommand cmdBuscar = new SqlCommand(sqlBuscar, cn, tran);
            cmdBuscar.Parameters.AddWithValue("@producto", idProducto);
            cmdBuscar.Parameters.AddWithValue("@punto", idPunto);
            AgregarParametroLote(cmdBuscar, lote);
            object encontrada = cmdBuscar.ExecuteScalar();

            if (encontrada != null)
            {
                SqlCommand cmdUpdate = new SqlCommand("UPDATE inventario SET cantidad = cantidad + @cantidad, fecha_actualizacion = GETDATE() WHERE id_inventario = @id", cn, tran);
                cmdUpdate.Parameters.AddWithValue("@cantidad", cantidad);
                cmdUpdate.Parameters.AddWithValue("@id", Convert.ToInt32(encontrada));
                cmdUpdate.ExecuteNonQuery();
            }
            else
            {
                string sqlInsert = "INSERT INTO inventario (id_producto, id_punto, cantidad, fecha_actualizacion, id_lote) " +
                                   "VALUES (@producto, @punto, @cantidad, GETDATE(), @lote)";
                SqlCommand cmdInsert = new SqlCommand(sqlInsert, cn, tran);
                cmdInsert.Parameters.AddWithValue("@producto", idProducto);
                cmdInsert.Parameters.AddWithValue("@punto", idPunto);
                cmdInsert.Parameters.AddWithValue("@cantidad", cantidad);
                AgregarParametroLote(cmdInsert, lote);
                cmdInsert.ExecuteNonQuery();
            }

            RegistrarMovimiento(cn, tran, idProducto, idPunto, lote, tipo, cantidad, motivo, idUsuario);
        }

        // Resta 'cantidad' (positiva) del stock de la sede, lote por lote (el que vence antes primero),
        // y registra un movimiento con cantidad negativa por cada descuento.
        private void DescontarFefo(SqlConnection cn, SqlTransaction tran, int idPunto, int idProducto, decimal cantidad,
                                   string tipo, string motivo, int idUsuario)
        {
            // Primero se leen todas las filas (el lector se cierra) y despues se actualiza
            List<FilaStock> filas = new List<FilaStock>();

            string sqlFilas = "SELECT i.id_inventario, i.id_lote, i.cantidad " +
                              "FROM inventario i LEFT JOIN lotes l ON l.id_lote = i.id_lote " +
                              "WHERE i.id_producto = @producto AND i.id_punto = @punto AND i.cantidad > 0 " +
                              "ORDER BY CASE WHEN l.fecha_vencimiento IS NULL THEN 1 ELSE 0 END, l.fecha_vencimiento, i.id_inventario";
            SqlCommand cmdFilas = new SqlCommand(sqlFilas, cn, tran);
            cmdFilas.Parameters.AddWithValue("@producto", idProducto);
            cmdFilas.Parameters.AddWithValue("@punto", idPunto);

            using (SqlDataReader dr = cmdFilas.ExecuteReader())
            {
                while (dr.Read())
                {
                    FilaStock fila = new FilaStock();
                    fila.IdInventario = Convert.ToInt32(dr["id_inventario"]);
                    fila.IdLote = dr["id_lote"];
                    fila.Cantidad = Convert.ToDecimal(dr["cantidad"]);
                    filas.Add(fila);
                }
            }

            decimal pendiente = cantidad;

            foreach (FilaStock fila in filas)
            {
                if (pendiente <= 0)
                {
                    break;
                }

                decimal tomar = Math.Min(pendiente, fila.Cantidad);

                // La condicion cantidad >= @cantidad evita dejar el stock negativo si otra operacion se adelanto
                SqlCommand cmdUpdate = new SqlCommand("UPDATE inventario SET cantidad = cantidad - @cantidad, fecha_actualizacion = GETDATE() " +
                                                      "WHERE id_inventario = @id AND cantidad >= @cantidad", cn, tran);
                cmdUpdate.Parameters.AddWithValue("@cantidad", tomar);
                cmdUpdate.Parameters.AddWithValue("@id", fila.IdInventario);

                if (cmdUpdate.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("El stock cambió mientras se procesaba la operación. Inténtalo de nuevo.");
                }

                RegistrarMovimiento(cn, tran, idProducto, idPunto, fila.IdLote, tipo, -tomar, motivo, idUsuario);

                pendiente = pendiente - tomar;
            }

            if (pendiente > 0)
            {
                throw new InvalidOperationException("No hay stock suficiente en esta sede (disponible: " +
                                                    (cantidad - pendiente).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ").");
            }
        }

        // Escribe una fila en movimientos_inventario (cantidad con signo). idLote es un int o DBNull.Value.
        private void RegistrarMovimiento(SqlConnection cn, SqlTransaction tran, int idProducto, int idPunto, object idLote,
                                         string tipo, decimal cantidad, string motivo, int idUsuario)
        {
            string sql = "INSERT INTO movimientos_inventario (id_producto, id_punto, id_lote, tipo, cantidad, motivo, id_usuario) " +
                         "VALUES (@producto, @punto, @lote, @tipo, @cantidad, @motivo, @usuario)";
            SqlCommand cmd = new SqlCommand(sql, cn, tran);
            cmd.Parameters.AddWithValue("@producto", idProducto);
            cmd.Parameters.AddWithValue("@punto", idPunto);
            AgregarParametroLote(cmd, idLote);
            cmd.Parameters.AddWithValue("@tipo", tipo);
            cmd.Parameters.AddWithValue("@cantidad", cantidad);
            cmd.Parameters.AddWithValue("@motivo", motivo);
            cmd.Parameters.AddWithValue("@usuario", idUsuario);
            cmd.ExecuteNonQuery();
        }
    }
}