using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using LaShiliquita.Entidades;

namespace LaShiliquita.Datos
{
    // Acceso a materias_primas y movimientos_materia_prima.
    // El stock de una materia prima es la SUMA de sus movimientos (no hay tabla de existencias).
    public class MateriaPrimaDatos
    {
        // ---------- Consultas ----------

        // Todas las materias primas con su stock actual
        public List<MateriaPrima> ListarConStock()
        {
            List<MateriaPrima> lista = new List<MateriaPrima>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql =
                    "SELECT m.id_materia_prima, m.nombre, m.unidad_medida, m.stock_minimo, m.descripcion, " +
                    "ISNULL((SELECT SUM(mm.cantidad) FROM movimientos_materia_prima mm " +
                    " WHERE mm.id_materia_prima = m.id_materia_prima), 0) AS stock_actual " +
                    "FROM materias_primas m ORDER BY m.nombre";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        MateriaPrima m = new MateriaPrima();
                        m.IdMateriaPrima = Convert.ToInt32(dr["id_materia_prima"]);
                        m.Nombre = dr["nombre"].ToString();
                        m.UnidadMedida = dr["unidad_medida"].ToString();
                        m.Descripcion = dr["descripcion"].ToString();
                        m.StockActual = Convert.ToDecimal(dr["stock_actual"]);

                        if (dr["stock_minimo"] == DBNull.Value)
                        {
                            m.TieneMinimo = false;
                            m.StockMinimo = 0;
                        }
                        else
                        {
                            m.TieneMinimo = true;
                            m.StockMinimo = Convert.ToDecimal(dr["stock_minimo"]);
                        }

                        lista.Add(m);
                    }
                }
            }

            return lista;
        }

        // true si ya existe otra materia prima con ese nombre (idExcluir = la que se esta editando, 0 si es nueva)
        public bool ExisteNombre(string nombre, int idExcluir)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT COUNT(*) FROM materias_primas WHERE nombre = @nombre AND id_materia_prima <> @id";
                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@id", idExcluir);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Historial de movimientos entre dos fechas (ambas incluidas), del mas reciente al mas antiguo.
        // idMateria = 0 trae todas; tipo = "" trae todos los tipos. Maximo 1000 filas.
        public List<MovimientoMateriaPrima> ListarMovimientos(DateTime desde, DateTime hasta, int idMateria, string tipo)
        {
            List<MovimientoMateriaPrima> lista = new List<MovimientoMateriaPrima>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql =
                    "SELECT TOP 1000 mm.id_movimiento_mp, mm.fecha, mm.id_materia_prima, m.nombre AS materia, m.unidad_medida, " +
                    "mm.tipo, mm.cantidad, l.codigo_lote, mm.id_compra, mm.motivo, u.nombres, u.apellidos " +
                    "FROM movimientos_materia_prima mm " +
                    "INNER JOIN materias_primas m ON m.id_materia_prima = mm.id_materia_prima " +
                    "LEFT JOIN lotes l ON l.id_lote = mm.id_lote " +
                    "LEFT JOIN usuarios u ON u.id_usuario = mm.id_usuario " +
                    "WHERE mm.fecha >= @desde AND mm.fecha < @hasta " +
                    "AND (@materia = 0 OR mm.id_materia_prima = @materia) " +
                    "AND (@tipo = '' OR mm.tipo = @tipo) " +
                    "ORDER BY mm.fecha DESC, mm.id_movimiento_mp DESC";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1));   // el dia final se incluye completo
                cmd.Parameters.AddWithValue("@materia", idMateria);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        MovimientoMateriaPrima mov = new MovimientoMateriaPrima();
                        mov.IdMovimiento = Convert.ToInt32(dr["id_movimiento_mp"]);
                        mov.Fecha = Convert.ToDateTime(dr["fecha"]);
                        mov.IdMateriaPrima = Convert.ToInt32(dr["id_materia_prima"]);
                        mov.MateriaPrima = dr["materia"].ToString();
                        mov.UnidadMedida = dr["unidad_medida"].ToString();
                        mov.Tipo = dr["tipo"].ToString();
                        mov.Cantidad = Convert.ToDecimal(dr["cantidad"]);
                        mov.CodigoLote = dr["codigo_lote"].ToString();
                        mov.IdCompra = dr["id_compra"] == DBNull.Value ? 0 : Convert.ToInt32(dr["id_compra"]);
                        mov.Motivo = dr["motivo"].ToString();
                        mov.Usuario = (dr["nombres"].ToString() + " " + dr["apellidos"].ToString()).Trim();
                        lista.Add(mov);
                    }
                }
            }

            return lista;
        }

        // ---------- Alta y edicion de materias primas ----------

        // Inserta una materia prima y devuelve su id
        public int Insertar(MateriaPrima materia)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "INSERT INTO materias_primas (nombre, unidad_medida, stock_minimo, descripcion) " +
                             "VALUES (@nombre, @unidad, @minimo, @descripcion); " +
                             "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                SqlCommand cmd = new SqlCommand(sql, cn);
                AgregarParametros(cmd, materia);
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Actualizar(MateriaPrima materia)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "UPDATE materias_primas SET nombre = @nombre, unidad_medida = @unidad, " +
                             "stock_minimo = @minimo, descripcion = @descripcion " +
                             "WHERE id_materia_prima = @id";
                SqlCommand cmd = new SqlCommand(sql, cn);
                AgregarParametros(cmd, materia);
                cmd.Parameters.AddWithValue("@id", materia.IdMateriaPrima);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Parametros comunes de insertar y actualizar (el minimo y la descripcion vacios se guardan como NULL)
        private void AgregarParametros(SqlCommand cmd, MateriaPrima materia)
        {
            cmd.Parameters.AddWithValue("@nombre", materia.Nombre);
            cmd.Parameters.AddWithValue("@unidad", materia.UnidadMedida);

            SqlParameter pMinimo = cmd.Parameters.Add("@minimo", SqlDbType.Decimal);
            pMinimo.Precision = 10;
            pMinimo.Scale = 2;
            if (materia.TieneMinimo)
            {
                pMinimo.Value = materia.StockMinimo;
            }
            else
            {
                pMinimo.Value = DBNull.Value;
            }

            if (string.IsNullOrWhiteSpace(materia.Descripcion))
            {
                cmd.Parameters.AddWithValue("@descripcion", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@descripcion", materia.Descripcion);
            }
        }

        // ---------- Operaciones de stock (cada una en UNA transaccion) ----------
        // InvalidOperationException = problema para mostrar al usuario (stock insuficiente);
        // SqlException = fallo de la base de datos. En ambos casos se hace Rollback.

        public void RegistrarIngreso(int idMateria, decimal cantidad, string motivo, int idUsuario)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    RegistrarMovimiento(cn, tran, idMateria, 0, 0, "INGRESO", cantidad, motivo, idUsuario);
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        // cantidad es positiva: se registra como movimiento negativo
        public void RegistrarSalida(int idMateria, decimal cantidad, string motivo, int idUsuario)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    ValidarStockDisponible(cn, tran, idMateria, cantidad);
                    RegistrarMovimiento(cn, tran, idMateria, 0, 0, "SALIDA", -cantidad, motivo, idUsuario);
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        // cantidad con signo: positivo suma, negativo resta (y exige stock suficiente)
        public void RegistrarAjuste(int idMateria, decimal cantidad, string motivo, int idUsuario)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();

                try
                {
                    if (cantidad < 0)
                    {
                        ValidarStockDisponible(cn, tran, idMateria, -cantidad);
                    }
                    RegistrarMovimiento(cn, tran, idMateria, 0, 0, "AJUSTE", cantidad, motivo, idUsuario);
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        // ---------- Piezas reutilizables dentro de otras transacciones (compras y produccion) ----------

        // Stock actual de una materia prima. Bloquea sus movimientos hasta que termine la transaccion,
        // para que dos operaciones al mismo tiempo no gasten el mismo stock.
        public decimal ObtenerStockBloqueado(SqlConnection cn, SqlTransaction tran, int idMateria)
        {
            string sql = "SELECT ISNULL(SUM(cantidad), 0) FROM movimientos_materia_prima WITH (UPDLOCK, HOLDLOCK) " +
                         "WHERE id_materia_prima = @materia";
            SqlCommand cmd = new SqlCommand(sql, cn, tran);
            cmd.Parameters.AddWithValue("@materia", idMateria);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        // Lanza InvalidOperationException si no hay 'cantidad' disponible de la materia prima
        public void ValidarStockDisponible(SqlConnection cn, SqlTransaction tran, int idMateria, decimal cantidad)
        {
            SqlCommand cmdNombre = new SqlCommand("SELECT nombre, unidad_medida FROM materias_primas WHERE id_materia_prima = @id", cn, tran);
            cmdNombre.Parameters.AddWithValue("@id", idMateria);

            string nombre = "";
            string unidad = "";

            using (SqlDataReader dr = cmdNombre.ExecuteReader())
            {
                if (!dr.Read())
                {
                    throw new InvalidOperationException("La materia prima no existe.");
                }
                nombre = dr["nombre"].ToString();
                unidad = dr["unidad_medida"].ToString();
            }

            decimal stock = ObtenerStockBloqueado(cn, tran, idMateria);

            if (stock < cantidad)
            {
                throw new InvalidOperationException("No hay stock suficiente de \"" + nombre + "\" (disponible: " +
                                                    stock.ToString("0.##", CultureInfo.InvariantCulture) + " " + unidad + ").");
            }
        }

        // Escribe una fila en movimientos_materia_prima (cantidad con signo).
        // idCompra e idLote en 0 se guardan como NULL.
        public void RegistrarMovimiento(SqlConnection cn, SqlTransaction tran, int idMateria, int idCompra, int idLote,
                                        string tipo, decimal cantidad, string motivo, int idUsuario)
        {
            string sql = "INSERT INTO movimientos_materia_prima (id_materia_prima, id_compra, id_lote, tipo, cantidad, motivo, id_usuario) " +
                         "VALUES (@materia, @compra, @lote, @tipo, @cantidad, @motivo, @usuario)";
            SqlCommand cmd = new SqlCommand(sql, cn, tran);
            cmd.Parameters.AddWithValue("@materia", idMateria);

            SqlParameter pCompra = cmd.Parameters.Add("@compra", SqlDbType.Int);
            pCompra.Value = idCompra > 0 ? (object)idCompra : DBNull.Value;

            SqlParameter pLote = cmd.Parameters.Add("@lote", SqlDbType.Int);
            pLote.Value = idLote > 0 ? (object)idLote : DBNull.Value;

            cmd.Parameters.AddWithValue("@tipo", tipo);
            cmd.Parameters.AddWithValue("@cantidad", cantidad);
            cmd.Parameters.AddWithValue("@motivo", motivo);
            cmd.Parameters.AddWithValue("@usuario", idUsuario);
            cmd.ExecuteNonQuery();
        }
    }
}