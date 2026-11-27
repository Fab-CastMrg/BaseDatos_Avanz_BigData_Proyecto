using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LaShiliquita.Entidades;
using LaShiliquita.Negocio;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Modulo de inventario: filtros de sede y fechas arriba, y dos pestañas:
    // Existencias (resumen por producto y operaciones) y Movimientos (historial).
    // La interfaz se construye por codigo (no usa el diseñador).
    public class UcInventario : UserControl
    {
        private InventarioNegocio negocio = new InventarioNegocio();

        // Lo que se esta mostrando (se fija al consultar)
        private Sede sedeConsultada = null;
        private DateTime desdeConsultado;
        private DateTime hastaConsultado;
        private List<ProductoVenta> productos = new List<ProductoVenta>();
        private List<ResumenInventario> resumen = new List<ResumenInventario>();
        private List<MovimientoInventario> movimientos = new List<MovimientoInventario>();

        private bool cargando = false;   // evita reacciones en cadena al llenar las listas por codigo

        private ComboBox cboSede;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Label lblMensaje;

        private DataGridView dgvExistencias;
        private Label lblAlerta;

        private ComboBox cboProductoMov;
        private ComboBox cboTipoMov;
        private DataGridView dgvMovimientos;
        private Label lblNotaMov;

        public UcInventario()
        {
            ConstruirInterfaz();
            this.Load += UcInventario_Load;
        }

        // ---------- Construccion de la interfaz ----------

        private void ConstruirInterfaz()
        {
            this.BackColor = Estilo.Fondo;

            // ---------- Filtros comunes (Top) ----------
            Panel pnlFiltros = new Panel();
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Height = 56;

            Label lblSede = CrearEtiqueta("Sede:", 0, 18);

            cboSede = new ComboBox();
            cboSede.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSede.DisplayMember = "Nombre";
            cboSede.Font = Estilo.Fuente(10);
            cboSede.Location = new Point(48, 14);
            cboSede.Width = 190;
            cboSede.SelectedIndexChanged += CboSede_SelectedIndexChanged;

            Label lblDesde = CrearEtiqueta("Desde:", 252, 18);

            dtpDesde = new DateTimePicker();
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Font = Estilo.Fuente(10);
            dtpDesde.Location = new Point(300, 14);
            dtpDesde.Width = 110;
            dtpDesde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            Label lblHasta = CrearEtiqueta("Hasta:", 422, 18);

            dtpHasta = new DateTimePicker();
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Font = Estilo.Fuente(10);
            dtpHasta.Location = new Point(468, 14);
            dtpHasta.Width = 110;
            dtpHasta.Value = DateTime.Today;

            Button btnConsultar = CrearBoton("Consultar", 592, 12, 100, 32, Estilo.Caramelo, Color.White);
            btnConsultar.Click += BtnConsultar_Click;

            pnlFiltros.Controls.Add(lblSede);
            pnlFiltros.Controls.Add(cboSede);
            pnlFiltros.Controls.Add(lblDesde);
            pnlFiltros.Controls.Add(dtpDesde);
            pnlFiltros.Controls.Add(lblHasta);
            pnlFiltros.Controls.Add(dtpHasta);
            pnlFiltros.Controls.Add(btnConsultar);

            // ---------- Mensajes (Bottom) ----------
            lblMensaje = new Label();
            lblMensaje.Dock = DockStyle.Bottom;
            lblMensaje.Height = 28;
            lblMensaje.Font = Estilo.Fuente(9.5f);
            lblMensaje.ForeColor = Estilo.TextoSuave;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;

            // ---------- Pestañas (Fill) ----------
            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Font = Estilo.Fuente(10);
            tabs.TabPages.Add(ConstruirPestanaExistencias());
            tabs.TabPages.Add(ConstruirPestanaMovimientos());

            // Fill primero; despues Bottom y Top
            this.Controls.Add(tabs);
            this.Controls.Add(lblMensaje);
            this.Controls.Add(pnlFiltros);
        }

        private TabPage ConstruirPestanaExistencias()
        {
            TabPage pagina = new TabPage("Existencias");
            pagina.UseVisualStyleBackColor = false;
            pagina.BackColor = Estilo.Fondo;
            pagina.Padding = new Padding(8);

            // Tabla (Fill)
            dgvExistencias = CrearGrid();
            DataGridViewTextBoxColumn colProducto = CrearColumna("Producto", 180, false);
            colProducto.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProducto.MinimumWidth = 180;
            dgvExistencias.Columns.Add(colProducto);
            dgvExistencias.Columns.Add(CrearColumna("Inicial", 70, true));
            dgvExistencias.Columns.Add(CrearColumna("Ingresadas", 85, true));
            dgvExistencias.Columns.Add(CrearColumna("Vendidas", 75, true));
            dgvExistencias.Columns.Add(CrearColumna("Salidas", 65, true));
            dgvExistencias.Columns.Add(CrearColumna("Ajustes", 65, true));
            dgvExistencias.Columns.Add(CrearColumna("Final", 70, true));
            dgvExistencias.Columns.Add(CrearColumna("Actual", 70, true));
            dgvExistencias.Columns.Add(CrearColumna("Mínimo", 65, true));
            dgvExistencias.Columns.Add(CrearColumna("Estado", 90, false));

            // Acciones (Top)
            Panel pnlAcciones = new Panel();
            pnlAcciones.Dock = DockStyle.Top;
            pnlAcciones.Height = 48;

            if (negocio.PuedeOperar())
            {
                Button btnIngreso = CrearBoton("Ingreso", 0, 8, 100, 32, Estilo.Caramelo, Color.White);
                btnIngreso.Click += BtnIngreso_Click;

                Button btnSalida = CrearBoton("Salida", 108, 8, 100, 32, Color.White, Estilo.Texto);
                ConContorno(btnSalida, Estilo.TextoSuave);
                btnSalida.Click += BtnSalida_Click;

                Button btnAjuste = CrearBoton("Ajuste", 216, 8, 100, 32, Color.White, Estilo.Texto);
                ConContorno(btnAjuste, Estilo.TextoSuave);
                btnAjuste.Click += BtnAjuste_Click;

                pnlAcciones.Controls.Add(btnIngreso);
                pnlAcciones.Controls.Add(btnSalida);
                pnlAcciones.Controls.Add(btnAjuste);
            }

            lblAlerta = new Label();
            lblAlerta.Dock = DockStyle.Right;
            lblAlerta.Width = 320;
            lblAlerta.Font = Estilo.FuenteNegrita(9.5f);
            lblAlerta.TextAlign = ContentAlignment.MiddleRight;
            pnlAcciones.Controls.Add(lblAlerta);

            pagina.Controls.Add(dgvExistencias);
            pagina.Controls.Add(pnlAcciones);

            return pagina;
        }

        private TabPage ConstruirPestanaMovimientos()
        {
            TabPage pagina = new TabPage("Movimientos");
            pagina.UseVisualStyleBackColor = false;
            pagina.BackColor = Estilo.Fondo;
            pagina.Padding = new Padding(8);

            // Tabla (Fill)
            dgvMovimientos = CrearGrid();
            dgvMovimientos.Columns.Add(CrearColumna("Fecha", 125, false));
            DataGridViewTextBoxColumn colProducto = CrearColumna("Producto", 160, false);
            colProducto.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProducto.MinimumWidth = 160;
            dgvMovimientos.Columns.Add(colProducto);
            dgvMovimientos.Columns.Add(CrearColumna("Tipo", 110, false));
            dgvMovimientos.Columns.Add(CrearColumna("Cantidad", 75, true));
            dgvMovimientos.Columns.Add(CrearColumna("Lote", 100, false));
            dgvMovimientos.Columns.Add(CrearColumna("Motivo", 200, false));
            dgvMovimientos.Columns.Add(CrearColumna("Usuario", 130, false));
            dgvMovimientos.Columns.Add(CrearColumna("Venta N°", 70, true));

            // Nota (Bottom)
            lblNotaMov = new Label();
            lblNotaMov.Dock = DockStyle.Bottom;
            lblNotaMov.Height = 26;
            lblNotaMov.Font = Estilo.Fuente(9);
            lblNotaMov.ForeColor = Estilo.TextoSuave;
            lblNotaMov.TextAlign = ContentAlignment.MiddleLeft;

            // Filtros del historial (Top)
            Panel pnlFiltrosMov = new Panel();
            pnlFiltrosMov.Dock = DockStyle.Top;
            pnlFiltrosMov.Height = 46;

            Label lblProducto = CrearEtiqueta("Producto:", 0, 13);

            cboProductoMov = new ComboBox();
            cboProductoMov.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProductoMov.DisplayMember = "Nombre";
            cboProductoMov.Font = Estilo.Fuente(10);
            cboProductoMov.Location = new Point(70, 9);
            cboProductoMov.Width = 260;

            Label lblTipo = CrearEtiqueta("Tipo:", 346, 13);

            cboTipoMov = new ComboBox();
            cboTipoMov.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoMov.Font = Estilo.Fuente(10);
            cboTipoMov.Location = new Point(392, 9);
            cboTipoMov.Width = 170;
            cboTipoMov.Items.Add("Todos los tipos");
            foreach (string tipo in InventarioNegocio.TiposMovimiento)
            {
                cboTipoMov.Items.Add(tipo);
            }
            cboTipoMov.SelectedIndex = 0;

            // Los eventos se conectan despues de dejar los valores iniciales
            cboProductoMov.SelectedIndexChanged += FiltroMovimientos_Changed;
            cboTipoMov.SelectedIndexChanged += FiltroMovimientos_Changed;

            pnlFiltrosMov.Controls.Add(lblProducto);
            pnlFiltrosMov.Controls.Add(cboProductoMov);
            pnlFiltrosMov.Controls.Add(lblTipo);
            pnlFiltrosMov.Controls.Add(cboTipoMov);

            pagina.Controls.Add(dgvMovimientos);
            pagina.Controls.Add(lblNotaMov);
            pagina.Controls.Add(pnlFiltrosMov);

            return pagina;
        }

        // ---------- Ayudas para crear controles ----------

        private Label CrearEtiqueta(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.AutoSize = true;
            etiqueta.Location = new Point(x, y);
            etiqueta.Font = Estilo.Fuente(10);
            etiqueta.ForeColor = Estilo.Texto;
            return etiqueta;
        }

        private Button CrearBoton(string texto, int x, int y, int ancho, int alto, Color fondo, Color letra)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Size = new Size(ancho, alto);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = fondo;
            boton.ForeColor = letra;
            boton.Font = Estilo.FuenteNegrita(10);
            boton.Cursor = Cursors.Hand;
            return boton;
        }

        private void ConContorno(Button boton, Color color)
        {
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = color;
        }

        // Tabla de solo lectura con el mismo estilo que el resto del sistema
        private DataGridView CrearGrid()
        {
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.Font = Estilo.Fuente(10);
            dgv.RowTemplate.Height = 30;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 34;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Estilo.Cafe;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Estilo.Cafe;
            dgv.DefaultCellStyle.SelectionBackColor = Estilo.Caramelo;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            return dgv;
        }

        private DataGridViewTextBoxColumn CrearColumna(string titulo, int ancho, bool alineadaDerecha)
        {
            DataGridViewTextBoxColumn columna = new DataGridViewTextBoxColumn();
            columna.HeaderText = titulo;
            columna.Width = ancho;
            columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            if (alineadaDerecha)
            {
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                columna.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            return columna;
        }

        // Numeros con punto decimal y sin ceros sobrantes (12, 12.5)
        private string Num(decimal valor)
        {
            return valor.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void Mensaje(string texto, bool esError)
        {
            lblMensaje.ForeColor = esError ? Estilo.Error : Estilo.TextoSuave;
            lblMensaje.Text = texto;
        }

        // ---------- Carga y consulta ----------

        private void UcInventario_Load(object sender, EventArgs e)
        {
            // Sedes a las que puede acceder el usuario (Gerente y Administrador: ambas; los demas: la suya)
            cargando = true;

            List<Sede> sedes = negocio.ObtenerSedesDisponibles();
            int indice = 0;

            for (int i = 0; i < sedes.Count; i++)
            {
                cboSede.Items.Add(sedes[i]);
                if (sedes[i].IdPunto == Sesion.SedeActual.IdPunto)
                {
                    indice = i;
                }
            }

            if (sedes.Count > 0)
            {
                cboSede.SelectedIndex = indice;
            }
            cboSede.Enabled = sedes.Count > 1;

            cargando = false;

            Consultar();
        }

        private void BtnConsultar_Click(object sender, EventArgs e)
        {
            Consultar();
        }

        private void CboSede_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cargando)
            {
                return;
            }
            Consultar();
        }

        // Lee el resumen y el historial de la sede y el rango elegidos, y los muestra
        private void Consultar()
        {
            Sede sede = cboSede.SelectedItem as Sede;
            if (sede == null)
            {
                return;
            }

            string error = negocio.ValidarRango(dtpDesde.Value, dtpHasta.Value);
            if (error != "")
            {
                Mensaje(error, true);
                return;
            }

            int idSeleccionado = ProductoSeleccionadoId();

            try
            {
                productos = negocio.ListarProductos(sede.IdPunto);
                resumen = negocio.ObtenerResumen(sede.IdPunto, dtpDesde.Value, dtpHasta.Value);
            }
            catch (SqlException ex)
            {
                Mensaje("No se pudo consultar el inventario: " + ex.Message, true);
                return;
            }

            // Desde aqui lo que se ve (y las operaciones) corresponde a esta sede y a este rango
            sedeConsultada = sede;
            desdeConsultado = dtpDesde.Value.Date;
            hastaConsultado = dtpHasta.Value.Date;

            LlenarListaProductos();
            MostrarExistencias(idSeleccionado);
            CargarMovimientos();
            Mensaje("", false);
        }

        // ---------- Existencias ----------

        private int IndiceSeleccionado(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                return -1;
            }
            return dgv.SelectedRows[0].Index;
        }

        // id del producto de la fila seleccionada en Existencias (0 si no hay)
        private int ProductoSeleccionadoId()
        {
            int indice = IndiceSeleccionado(dgvExistencias);

            if (indice >= 0 && indice < resumen.Count)
            {
                return resumen[indice].IdProducto;
            }
            return 0;
        }

        private void MostrarExistencias(int idSeleccionado)
        {
            dgvExistencias.Rows.Clear();
            int bajos = 0;

            foreach (ResumenInventario f in resumen)
            {
                string nombre = f.Presentacion == "" ? f.Nombre : f.Nombre + " (" + f.Presentacion + ")";
                string minimo = f.TieneMinimo ? Num(f.StockMinimo) : "-";
                string estado = f.EsStockBajo ? "Stock bajo" : "";

                int fila = dgvExistencias.Rows.Add(nombre, Num(f.StockInicial), Num(f.Ingresadas), Num(f.Vendidas),
                                                   Num(f.Salidas), Num(f.Ajustes), Num(f.StockFinal), Num(f.StockActual),
                                                   minimo, estado);

                if (f.EsStockBajo)
                {
                    dgvExistencias.Rows[fila].DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    bajos++;
                }
            }

            if (bajos > 0)
            {
                lblAlerta.ForeColor = Estilo.Error;
                lblAlerta.Text = "Stock bajo en " + bajos + " producto(s)";
            }
            else
            {
                lblAlerta.ForeColor = Estilo.TextoSuave;
                lblAlerta.Text = "Ningún producto con stock bajo";
            }

            // Se vuelve a seleccionar el producto que estaba elegido
            if (idSeleccionado > 0)
            {
                for (int i = 0; i < resumen.Count; i++)
                {
                    if (resumen[i].IdProducto == idSeleccionado)
                    {
                        dgvExistencias.ClearSelection();
                        dgvExistencias.Rows[i].Selected = true;
                        dgvExistencias.CurrentCell = dgvExistencias.Rows[i].Cells[0];
                        break;
                    }
                }
            }
        }

        private void BtnIngreso_Click(object sender, EventArgs e)
        {
            AbrirOperacion("INGRESO");
        }

        private void BtnSalida_Click(object sender, EventArgs e)
        {
            AbrirOperacion("SALIDA");
        }

        private void BtnAjuste_Click(object sender, EventArgs e)
        {
            AbrirOperacion("AJUSTE");
        }

        // Abre la ventana de la operacion para el producto seleccionado, en la sede que se esta viendo
        private void AbrirOperacion(string tipo)
        {
            if (sedeConsultada == null)
            {
                return;
            }

            int indice = IndiceSeleccionado(dgvExistencias);

            if (indice < 0 || indice >= resumen.Count)
            {
                MessageBox.Show("Selecciona un producto de la tabla.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ResumenInventario fila = resumen[indice];

            FrmMovimientoInventario ventana = new FrmMovimientoInventario(tipo, sedeConsultada, fila.IdProducto, fila.Nombre, fila.StockActual);

            if (ventana.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                Consultar();
                Mensaje("Operación registrada en " + sedeConsultada.Nombre + ".", false);
            }
        }

        // ---------- Movimientos ----------

        // Llena la lista de productos del filtro del historial, conservando el producto elegido
        private void LlenarListaProductos()
        {
            int idAnterior = 0;
            ProductoVenta anterior = cboProductoMov.SelectedItem as ProductoVenta;
            if (anterior != null)
            {
                idAnterior = anterior.IdProducto;
            }

            cargando = true;
            cboProductoMov.Items.Clear();

            ProductoVenta todos = new ProductoVenta();
            todos.IdProducto = 0;
            todos.Nombre = "Todos los productos";
            cboProductoMov.Items.Add(todos);

            int indiceElegido = 0;
            foreach (ProductoVenta p in productos)
            {
                int indice = cboProductoMov.Items.Add(p);
                if (p.IdProducto == idAnterior)
                {
                    indiceElegido = indice;
                }
            }

            cboProductoMov.SelectedIndex = indiceElegido;
            cargando = false;
        }

        private void FiltroMovimientos_Changed(object sender, EventArgs e)
        {
            if (cargando)
            {
                return;
            }
            CargarMovimientos();
        }

        private void CargarMovimientos()
        {
            if (sedeConsultada == null)
            {
                return;
            }

            int idProducto = 0;
            ProductoVenta producto = cboProductoMov.SelectedItem as ProductoVenta;
            if (producto != null)
            {
                idProducto = producto.IdProducto;
            }

            string tipo = "";
            if (cboTipoMov.SelectedIndex > 0)
            {
                tipo = cboTipoMov.SelectedItem.ToString();
            }

            try
            {
                movimientos = negocio.ListarMovimientos(sedeConsultada.IdPunto, desdeConsultado, hastaConsultado, idProducto, tipo);
            }
            catch (SqlException ex)
            {
                Mensaje("No se pudo cargar el historial: " + ex.Message, true);
                return;
            }

            dgvMovimientos.Rows.Clear();

            foreach (MovimientoInventario m in movimientos)
            {
                string cantidad = (m.Cantidad > 0 ? "+" : "") + Num(m.Cantidad);
                string venta = m.IdVenta > 0 ? m.IdVenta.ToString() : "";

                int fila = dgvMovimientos.Rows.Add(m.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                                                   m.Producto, m.Tipo, cantidad, m.CodigoLote, m.Motivo, m.Usuario, venta);

                if (m.Cantidad < 0)
                {
                    dgvMovimientos.Rows[fila].Cells[3].Style.ForeColor = Estilo.Error;
                }
            }

            if (movimientos.Count >= 1000)
            {
                lblNotaMov.Text = "Se muestran los 1000 movimientos más recientes. Acota las fechas o los filtros para ver otros.";
            }
            else
            {
                lblNotaMov.Text = movimientos.Count + " movimiento(s).";
            }
        }
    }
}