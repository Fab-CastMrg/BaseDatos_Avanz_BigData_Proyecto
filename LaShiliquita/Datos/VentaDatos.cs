using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LaShiliquita.Entidades;

namespace LaShiliquita.Datos
{
    public class VentaDatos
    {
        // Productos con su stock en una sede (suma de todos los lotes de esa sede)
        public List<ProductoVenta> ListarCatalogo(int idPunto)
        {
            List<ProductoVenta> lista = new List<ProductoVenta>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT p.id_producto, p.nombre, p.presentacion, p.id_categoria, " +
                             "c.nombre AS categoria, ISNULL(p.precio_venta, 0) AS precio_venta, " +
                             "ISNULL(SUM(i.cantidad), 0) AS stock " +
                             "FROM productos p " +
                             "LEFT JOIN categorias_producto c ON c.id_categoria = p.id_categoria " +
                             "LEFT JOIN inventario i ON i.id_producto = p.id_producto AND i.id_punto = @punto " +
                             "GROUP BY p.id_producto, p.nombre, p.presentacion, p.id_categoria, c.nombre, p.precio_venta " +
                             "ORDER BY p.nombre";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@punto", idPunto);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        ProductoVenta p = new ProductoVenta();
                        p.IdProducto = Convert.ToInt32(dr["id_producto"]);
                        p.Nombre = dr["nombre"].ToString();
                        p.Presentacion = dr["presentacion"].ToString();
                        p.IdCategoria = dr["id_categoria"] == DBNull.Value ? 0 : Convert.ToInt32(dr["id_categoria"]);
                        p.Categoria = dr["categoria"].ToString();
                        p.PrecioVenta = Convert.ToDecimal(dr["precio_venta"]);
                        p.Stock = Convert.ToDecimal(dr["stock"]);
                        lista.Add(p);
                    }
                }
            }

            return lista;
        }

        // Guarda la venta completa en UNA transaccion: ventas, descuento de stock por lote (primero el que
        // vence antes) con su movimiento, detalle_venta, pagos e ingresos de caja.
        // Devuelve el id_venta. Si el stock no alcanza lanza InvalidOperationException (con el mensaje para el usuario);
        // cualquier otro fallo de la BD lanza SqlException. En ambos casos se deshace todo (Rollback).
        public int RegistrarVenta(Venta venta, int idTipoMovimiento)
        {
            int idVenta = 0;

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    // 1. Cabecera de la venta (id_cliente queda NULL: publico general)
                    string sqlVenta = "INSERT INTO ventas (id_usuario, id_punto, fecha_venta, subtotal, descuento, total) " +
                                      "VALUES (@usuario, @punto, GETDATE(), @subtotal, @descuento, @total); " +
                                      "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    SqlCommand cmdVenta = new SqlCommand(sqlVenta, cn, tran);
                    cmdVenta.Parameters.AddWithValue("@usuario", venta.IdUsuario);
                    cmdVenta.Parameters.AddWithValue("@punto", venta.IdPunto);
                    cmdVenta.Parameters.AddWithValue("@subtotal", venta.Subtotal);
                    cmdVenta.Parameters.AddWithValue("@descuento", venta.Descuento);
                    cmdVenta.Parameters.AddWithValue("@total", venta.Total);
                    idVenta = Convert.ToInt32(cmdVenta.ExecuteScalar());

                    // 2 y 3. Por cada producto: descontar stock (con movimiento) y guardar el detalle
                    foreach (ItemCarrito item in venta.Items)
                    {
                        DescontarStock(cn, tran, venta, idVenta, item);

                        string sqlDetalle = "INSERT INTO detalle_venta (id_venta, id_producto, cantidad, precio_unitario, subtotal) " +
                                            "VALUES (@venta, @producto, @cantidad, @precio, @subtotal)";
                        SqlCommand cmdDetalle = new SqlCommand(sqlDetalle, cn, tran);
                        cmdDetalle.Parameters.AddWithValue("@venta", idVenta);
                        cmdDetalle.Parameters.AddWithValue("@producto", item.IdProducto);
                        cmdDetalle.Parameters.AddWithValue("@cantidad", (decimal)item.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@precio", item.PrecioUnitario);
                        cmdDetalle.Parameters.AddWithValue("@subtotal", item.Subtotal);
                        cmdDetalle.ExecuteNonQuery();
                    }

                    // 4 y 5. Cada pago y su ingreso en la caja de la sede (enlazado por id_pago)
                    foreach (PagoVenta pago in venta.Pagos)
                    {
                        string sqlPago = "INSERT INTO pagos (id_venta, id_usuario, metodo_pago, monto, fecha_pago, numero_operacion) " +
                                         "VALUES (@venta, @usuario, @metodo, @monto, GETDATE(), @operacion); " +
                                         "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        SqlCommand cmdPago = new SqlCommand(sqlPago, cn, tran);
                        cmdPago.Parameters.AddWithValue("@venta", idVenta);
                        cmdPago.Parameters.AddWithValue("@usuario", venta.IdUsuario);
                        cmdPago.Parameters.AddWithValue("@metodo", pago.MetodoPago);
                        cmdPago.Parameters.AddWithValue("@monto", pago.Monto);
                        if (string.IsNullOrWhiteSpace(pago.NumeroOperacion))
                        {
                            cmdPago.Parameters.AddWithValue("@operacion", DBNull.Value);
                        }
                        else
                        {
                            cmdPago.Parameters.AddWithValue("@operacion", pago.NumeroOperacion);
                        }
                        int idPago = Convert.ToInt32(cmdPago.ExecuteScalar());

                        string sqlIngreso = "INSERT INTO ingresos (id_caja, id_tipo_movimiento, id_usuario, fecha_ingreso, monto, concepto, id_pago) " +
                                            "VALUES (@caja, @tipo, @usuario, GETDATE(), @monto, @concepto, @pago)";
                        SqlCommand cmdIngreso = new SqlCommand(sqlIngreso, cn, tran);
                        cmdIngreso.Parameters.AddWithValue("@caja", venta.IdCaja);
                        cmdIngreso.Parameters.AddWithValue("@tipo", idTipoMovimiento);
                        cmdIngreso.Parameters.AddWithValue("@usuario", venta.IdUsuario);
                        cmdIngreso.Parameters.AddWithValue("@monto", pago.Monto);
                        cmdIngreso.Parameters.AddWithValue("@concepto", "Venta " + idVenta + " - " + pago.MetodoPago);
                        cmdIngreso.Parameters.AddWithValue("@pago", idPago);
                        cmdIngreso.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            return idVenta;
        }

        // Fila de inventario (un lote de un producto en la sede). Solo la usa DescontarStock.
        private class FilaStock
        {
            public int IdInventario;
            public object IdLote;       // DBNull.Value si la fila no tiene lote
            public decimal Cantidad;
        }

        // Descuenta del inventario las unidades de un producto, lote por lote (el que vence antes primero),
        // y registra un movimiento VENTA (cantidad negativa) por cada descuento. Usa la transaccion recibida.
        private void DescontarStock(SqlConnection cn, SqlTransaction tran, Venta venta, int idVenta, ItemCarrito item)
        {
            // Primero se leen todas las filas (el lector se cierra) y despues se actualiza
            List<FilaStock> filas = new List<FilaStock>();

            string sqlFilas = "SELECT i.id_inventario, i.id_lote, i.cantidad " +
                              "FROM inventario i LEFT JOIN lotes l ON l.id_lote = i.id_lote " +
                              "WHERE i.id_producto = @producto AND i.id_punto = @punto AND i.cantidad > 0 " +
                              "ORDER BY CASE WHEN l.fecha_vencimiento IS NULL THEN 1 ELSE 0 END, l.fecha_vencimiento, i.id_inventario";
            SqlCommand cmdFilas = new SqlCommand(sqlFilas, cn, tran);
            cmdFilas.Parameters.AddWithValue("@producto", item.IdProducto);
            cmdFilas.Parameters.AddWithValue("@punto", venta.IdPunto);

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

            decimal pendiente = item.Cantidad;

            foreach (FilaStock fila in filas)
            {
                if (pendiente <= 0)
                {
                    break;
                }

                decimal tomar = Math.Min(pendiente, fila.Cantidad);

                // La condicion cantidad >= @cantidad evita dejar el stock negativo si otra venta se adelanto
                string sqlUpdate = "UPDATE inventario SET cantidad = cantidad - @cantidad, fecha_actualizacion = GETDATE() " +
                                   "WHERE id_inventario = @id AND cantidad >= @cantidad";
                SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, cn, tran);
                cmdUpdate.Parameters.AddWithValue("@cantidad", tomar);
                cmdUpdate.Parameters.AddWithValue("@id", fila.IdInventario);

                if (cmdUpdate.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("El stock de \"" + item.Nombre + "\" cambió mientras se procesaba la venta. " +
                                                        "Actualiza el catálogo e inténtalo de nuevo.");
                }

                string sqlMov = "INSERT INTO movimientos_inventario (id_producto, id_punto, id_lote, id_venta, tipo, cantidad, motivo, id_usuario) " +
                                "VALUES (@producto, @punto, @lote, @venta, 'VENTA', @cantidad, @motivo, @usuario)";
                SqlCommand cmdMov = new SqlCommand(sqlMov, cn, tran);
                cmdMov.Parameters.AddWithValue("@producto", item.IdProducto);
                cmdMov.Parameters.AddWithValue("@punto", venta.IdPunto);
                cmdMov.Parameters.AddWithValue("@lote", fila.IdLote);
                cmdMov.Parameters.AddWithValue("@venta", idVenta);
                cmdMov.Parameters.AddWithValue("@cantidad", -tomar);
                cmdMov.Parameters.AddWithValue("@motivo", "Venta " + idVenta);
                cmdMov.Parameters.AddWithValue("@usuario", venta.IdUsuario);
                cmdMov.ExecuteNonQuery();

                pendiente = pendiente - tomar;
            }

            if (pendiente > 0)
            {
                throw new InvalidOperationException("No hay stock suficiente de \"" + item.Nombre + "\" en esta sede.");
            }
        }

        // Lee una venta ya guardada (cabecera, productos y pagos) para armar el ticket. Devuelve null si no existe.
        public Venta ObtenerVenta(int idVenta)
        {
            Venta venta = null;

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();

                string sqlVenta = "SELECT v.id_venta, ISNULL(v.id_usuario, 0) AS id_usuario, ISNULL(v.id_punto, 0) AS id_punto, " +
                                  "v.fecha_venta, ISNULL(v.subtotal, 0) AS subtotal, ISNULL(v.descuento, 0) AS descuento, " +
                                  "ISNULL(v.total, 0) AS total, u.nombres, u.apellidos, p.nombre AS sede " +
                                  "FROM ventas v " +
                                  "LEFT JOIN usuarios u ON u.id_usuario = v.id_usuario " +
                                  "LEFT JOIN puntos_abastecimiento p ON p.id_punto = v.id_punto " +
                                  "WHERE v.id_venta = @id";
                SqlCommand cmdVenta = new SqlCommand(sqlVenta, cn);
                cmdVenta.Parameters.AddWithValue("@id", idVenta);

                using (SqlDataReader dr = cmdVenta.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        venta = new Venta();
                        venta.IdVenta = Convert.ToInt32(dr["id_venta"]);
                        venta.IdUsuario = Convert.ToInt32(dr["id_usuario"]);
                        venta.IdPunto = Convert.ToInt32(dr["id_punto"]);
                        venta.Fecha = dr["fecha_venta"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["fecha_venta"]);
                        venta.Subtotal = Convert.ToDecimal(dr["subtotal"]);
                        venta.Descuento = Convert.ToDecimal(dr["descuento"]);
                        venta.Total = Convert.ToDecimal(dr["total"]);
                        venta.NombreVendedor = (dr["nombres"].ToString() + " " + dr["apellidos"].ToString()).Trim();
                        venta.NombreSede = dr["sede"].ToString();
                    }
                }

                if (venta == null)
                {
                    return null;
                }

                string sqlDetalle = "SELECT d.id_producto, p.nombre, ISNULL(d.cantidad, 0) AS cantidad, ISNULL(d.precio_unitario, 0) AS precio_unitario " +
                                    "FROM detalle_venta d LEFT JOIN productos p ON p.id_producto = d.id_producto " +
                                    "WHERE d.id_venta = @id ORDER BY d.id_detalle_venta";
                SqlCommand cmdDetalle = new SqlCommand(sqlDetalle, cn);
                cmdDetalle.Parameters.AddWithValue("@id", idVenta);

                using (SqlDataReader dr = cmdDetalle.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        ItemCarrito item = new ItemCarrito();
                        item.IdProducto = Convert.ToInt32(dr["id_producto"]);
                        item.Nombre = dr["nombre"].ToString();
                        item.Cantidad = Convert.ToInt32(dr["cantidad"]);
                        item.PrecioUnitario = Convert.ToDecimal(dr["precio_unitario"]);
                        venta.Items.Add(item);
                    }
                }

                string sqlPagos = "SELECT metodo_pago, ISNULL(monto, 0) AS monto, numero_operacion " +
                                  "FROM pagos WHERE id_venta = @id ORDER BY id_pago";
                SqlCommand cmdPagos = new SqlCommand(sqlPagos, cn);
                cmdPagos.Parameters.AddWithValue("@id", idVenta);

                using (SqlDataReader dr = cmdPagos.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        PagoVenta pago = new PagoVenta();
                        pago.MetodoPago = dr["metodo_pago"].ToString();
                        pago.Monto = Convert.ToDecimal(dr["monto"]);
                        pago.NumeroOperacion = dr["numero_operacion"].ToString();
                        venta.Pagos.Add(pago);
                    }
                }
            }

            return venta;
        }
    }
}