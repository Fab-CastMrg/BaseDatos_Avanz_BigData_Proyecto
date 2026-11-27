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
    // Modulo de materia prima, con dos pestañas:
    // Stock (materias primas, stock actual y operaciones) y Movimientos (historial).
    // La interfaz se construye por codigo (no usa el diseñador).
    public class UcMateriaPrima : UserControl
    {
        private MateriaPrimaNegocio negocio = new MateriaPrimaNegocio();

        private List<MateriaPrima> materias = new List<MateriaPrima>();
        private List<MovimientoMateriaPrima> movimientos = new List<MovimientoMateriaPrima>();

        // Rango del historial que se esta mostrando (se fija al pulsar Buscar)
        private DateTime desdeConsultado;
        private DateTime hastaConsultado;

        private bool cargando = false;   // evita reacciones en cadena al llenar las listas por codigo

        private Label lblMensaje;

        private DataGridView dgvStock;
        private Label lblAlerta;

        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private ComboBox cboMateriaMov;
        private ComboBox cboTipoMov;
        private DataGridView dgvMovimientos;
        private Label lblNotaMov;

        public UcMateriaPrima()
        {
            ConstruirInterfaz();
            this.Load += UcMateriaPrima_Load;
        }

        // ---------- Construccion de la interfaz ----------

        private void ConstruirInterfaz()
        {
            this.BackColor = Estilo.Fondo;

            // Mensajes (Bottom)
            lblMensaje = new Label();
            lblMensaje.Dock = DockStyle.Bottom;
            lblMensaje.Height = 28;
            lblMensaje.Font = Estilo.Fuente(9.5f);
            lblMensaje.ForeColor = Estilo.TextoSuave;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;

            // Pestañas (Fill)
            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Font = Estilo.Fuente(10);
            tabs.TabPages.Add(ConstruirPestanaStock());
            tabs.TabPages.Add(ConstruirPestanaMovimientos());

            // Fill primero y Bottom despues
            this.Controls.Add(tabs);
            this.Controls.Add(lblMensaje);
        }

        private TabPage ConstruirPestanaStock()
        {
            TabPage pagina = new TabPage("Stock");
            pagina.UseVisualStyleBackColor = false;
            pagina.BackColor = Estilo.Fondo;
            pagina.Padding = new Padding(8);

            // Tabla (Fill)
            dgvStock = CrearGrid();
            DataGridViewTextBoxColumn colNombre = CrearColumna("Materia prima", 200, false);
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNombre.MinimumWidth = 200;
            dgvStock.Columns.Add(colNombre);
            dgvStock.Columns.Add(CrearColumna("Unidad", 80, false));
            dgvStock.Columns.Add(CrearColumna("Stock actual", 110, true));
            dgvStock.Columns.Add(CrearColumna("Mínimo", 90, true));
            dgvStock.Columns.Add(CrearColumna("Estado", 100, false));

            // Acciones (Top)
            Panel pnlAcciones = new Panel();
            pnlAcciones.Dock = DockStyle.Top;
            pnlAcciones.Height = 48;

            Button btnNueva = CrearBoton("Nueva materia", 0, 8, 130, 32, Estilo.Caramelo, Color.White);
            btnNueva.Click += BtnNueva_Click;

            Button btnEditar = CrearBoton("Editar", 136, 8, 80, 32, Color.White, Estilo.Texto);
            ConContorno(btnEditar, Estilo.TextoSuave);
            btnEditar.Click += BtnEditar_Click;

            Button btnIngreso = CrearBoton("Ingreso", 222, 8, 80, 32, Color.White, Estilo.Texto);
            ConContorno(btnIngreso, Estilo.TextoSuave);
            btnIngreso.Click += BtnIngreso_Click;

            Button btnSalida = CrearBoton("Salida", 308, 8, 80, 32, Color.White, Estilo.Texto);
            ConContorno(btnSalida, Estilo.TextoSuave);
            btnSalida.Click += BtnSalida_Click;

            Button btnAjuste = CrearBoton("Ajuste", 394, 8, 80, 32, Color.White, Estilo.Texto);
            ConContorno(btnAjuste, Estilo.TextoSuave);
            btnAjuste.Click += BtnAjuste_Click;

            lblAlerta = new Label();
            lblAlerta.Dock = DockStyle.Right;
            lblAlerta.Width = 220;
            lblAlerta.Font = Estilo.FuenteNegrita(9.5f);
            lblAlerta.TextAlign = ContentAlignment.MiddleRight;

            pnlAcciones.Controls.Add(btnNueva);
            pnlAcciones.Controls.Add(btnEditar);
            pnlAcciones.Controls.Add(btnIngreso);
            pnlAcciones.Controls.Add(btnSalida);
            pnlAcciones.Controls.Add(btnAjuste);
            pnlAcciones.Controls.Add(lblAlerta);

            pagina.Controls.Add(dgvStock);
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
            DataGridViewTextBoxColumn colMateria = CrearColumna("Materia prima", 160, false);
            colMateria.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colMateria.MinimumWidth = 160;
            dgvMovimientos.Columns.Add(colMateria);
            dgvMovimientos.Columns.Add(CrearColumna("Tipo", 140, false));
            dgvMovimientos.Columns.Add(CrearColumna("Cantidad", 110, true));
            dgvMovimientos.Columns.Add(CrearColumna("Lote", 110, false));
            dgvMovimientos.Columns.Add(CrearColumna("Compra N°", 80, true));
            dgvMovimientos.Columns.Add(CrearColumna("Motivo", 200, false));
            dgvMovimientos.Columns.Add(CrearColumna("Usuario", 130, false));

            // Nota (Bottom)
            lblNotaMov = new Label();
            lblNotaMov.Dock = DockStyle.Bottom;
            lblNotaMov.Height = 26;
            lblNotaMov.Font = Estilo.Fuente(9);
            lblNotaMov.ForeColor = Estilo.TextoSuave;
            lblNotaMov.TextAlign = ContentAlignment.MiddleLeft;

            // Filtros (Top): fila 1 con las fechas y fila 2 con los filtros de lista
            Panel pnlFiltros = new Panel();
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Height = 84;

            Label lblDesde = CrearEtiqueta("Desde:", 0, 13);

            dtpDesde = new DateTimePicker();
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Font = Estilo.Fuente(10);
            dtpDesde.Location = new Point(48, 8);
            dtpDesde.Width = 110;
            dtpDesde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            Label lblHasta = CrearEtiqueta("Hasta:", 172, 13);

            dtpHasta = new DateTimePicker();
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Font = Estilo.Fuente(10);
            dtpHasta.Location = new Point(218, 8);
            dtpHasta.Width = 110;
            dtpHasta.Value = DateTime.Today;

            Button btnBuscar = CrearBoton("Buscar", 342, 6, 90, 32, Estilo.Caramelo, Color.White);
            btnBuscar.Click += BtnBuscar_Click;

            cboMateriaMov = new ComboBox();
            cboMateriaMov.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMateriaMov.DisplayMember = "Nombre";
            cboMateriaMov.Font = Estilo.Fuente(10);
            cboMateriaMov.Location = new Point(0, 48);
            cboMateriaMov.Width = 260;

            cboTipoMov = new ComboBox();
            cboTipoMov.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoMov.Font = Estilo.Fuente(10);
            cboTipoMov.Location = new Point(272, 48);
            cboTipoMov.Width = 190;
            cboTipoMov.Items.Add("Todos los tipos");
            foreach (string tipo in MateriaPrimaNegocio.TiposMovimiento)
            {
                cboTipoMov.Items.Add(tipo);
            }
            cboTipoMov.SelectedIndex = 0;

            // Los eventos se conectan despues de dejar los valores iniciales
            cboMateriaMov.SelectedIndexChanged += FiltroMovimientos_Changed;
            cboTipoMov.SelectedIndexChanged += FiltroMovimientos_Changed;

            pnlFiltros.Controls.Add(lblDesde);
            pnlFiltros.Controls.Add(dtpDesde);
            pnlFiltros.Controls.Add(lblHasta);
            pnlFiltros.Controls.Add(dtpHasta);
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(cboMateriaMov);
            pnlFiltros.Controls.Add(cboTipoMov);

            pagina.Controls.Add(dgvMovimientos);
            pagina.Controls.Add(lblNotaMov);
            pagina.Controls.Add(pnlFiltros);

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

        private int IndiceSeleccionado(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                return -1;
            }
            return dgv.SelectedRows[0].Index;
        }

        // ---------- Carga ----------

        private void UcMateriaPrima_Load(object sender, EventArgs e)
        {
            desdeConsultado = dtpDesde.Value.Date;
            hastaConsultado = dtpHasta.Value.Date;

            CargarMaterias(0);
            CargarMovimientos();
        }

        // ---------- Stock ----------

        // Lee las materias primas con su stock y las muestra; deja seleccionada la de idSeleccionado (si hay)
        private void CargarMaterias(int idSeleccionado)
        {
            try
            {
                materias = negocio.ListarConStock();
            }
            catch (SqlException ex)
            {
                Mensaje("No se pudo cargar el stock: " + ex.Message, true);
                return;
            }

            MostrarStock(idSeleccionado);
            LlenarListaMaterias();
        }

        private int MateriaSeleccionadaId()
        {
            int indice = IndiceSeleccionado(dgvStock);

            if (indice >= 0 && indice < materias.Count)
            {
                return materias[indice].IdMateriaPrima;
            }
            return 0;
        }

        private void MostrarStock(int idSeleccionado)
        {
            dgvStock.Rows.Clear();
            int bajos = 0;

            foreach (MateriaPrima m in materias)
            {
                string minimo = m.TieneMinimo ? Num(m.StockMinimo) : "-";
                string estado = m.EsStockBajo ? "Stock bajo" : "";

                int fila = dgvStock.Rows.Add(m.Nombre, m.UnidadMedida, Num(m.StockActual), minimo, estado);

                if (m.EsStockBajo)
                {
                    dgvStock.Rows[fila].DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    bajos++;
                }
            }

            if (bajos > 0)
            {
                lblAlerta.ForeColor = Estilo.Error;
                lblAlerta.Text = "Stock bajo: " + bajos + " de " + materias.Count;
            }
            else
            {
                lblAlerta.ForeColor = Estilo.TextoSuave;
                lblAlerta.Text = "Sin materias con stock bajo";
            }

            if (idSeleccionado > 0)
            {
                for (int i = 0; i < materias.Count; i++)
                {
                    if (materias[i].IdMateriaPrima == idSeleccionado)
                    {
                        dgvStock.ClearSelection();
                        dgvStock.Rows[i].Selected = true;
                        dgvStock.CurrentCell = dgvStock.Rows[i].Cells[0];
                        break;
                    }
                }
            }
        }

        private void BtnNueva_Click(object sender, EventArgs e)
        {
            FrmMateriaPrima ventana = new FrmMateriaPrima(null);

            if (ventana.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                CargarMaterias(ventana.IdGuardado);
                Mensaje("Materia prima creada.", false);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            MateriaPrima materia = MateriaSeleccionada();
            if (materia == null)
            {
                return;
            }

            FrmMateriaPrima ventana = new FrmMateriaPrima(materia);

            if (ventana.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                CargarMaterias(ventana.IdGuardado);
                CargarMovimientos();
                Mensaje("Materia prima actualizada.", false);
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

        // Devuelve la materia prima de la fila seleccionada; si no hay, avisa y devuelve null
        private MateriaPrima MateriaSeleccionada()
        {
            int indice = IndiceSeleccionado(dgvStock);

            if (indice < 0 || indice >= materias.Count)
            {
                MessageBox.Show("Selecciona una materia prima de la tabla.", "Materia prima", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return materias[indice];
        }

        private void AbrirOperacion(string tipo)
        {
            MateriaPrima materia = MateriaSeleccionada();
            if (materia == null)
            {
                return;
            }

            FrmMovimientoMateriaPrima ventana = new FrmMovimientoMateriaPrima(tipo, materia);

            if (ventana.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                CargarMaterias(materia.IdMateriaPrima);
                CargarMovimientos();
                Mensaje("Operación registrada.", false);
            }
        }

        // ---------- Movimientos ----------

        // Llena la lista de materias del filtro del historial, conservando la elegida
        private void LlenarListaMaterias()
        {
            int idAnterior = 0;
            MateriaPrima anterior = cboMateriaMov.SelectedItem as MateriaPrima;
            if (anterior != null)
            {
                idAnterior = anterior.IdMateriaPrima;
            }

            cargando = true;
            cboMateriaMov.Items.Clear();

            MateriaPrima todas = new MateriaPrima();
            todas.IdMateriaPrima = 0;
            todas.Nombre = "Todas las materias primas";
            cboMateriaMov.Items.Add(todas);

            int indiceElegido = 0;
            foreach (MateriaPrima m in materias)
            {
                int indice = cboMateriaMov.Items.Add(m);
                if (m.IdMateriaPrima == idAnterior)
                {
                    indiceElegido = indice;
                }
            }

            cboMateriaMov.SelectedIndex = indiceElegido;
            cargando = false;
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string error = negocio.ValidarRango(dtpDesde.Value, dtpHasta.Value);
            if (error != "")
            {
                Mensaje(error, true);
                return;
            }

            desdeConsultado = dtpDesde.Value.Date;
            hastaConsultado = dtpHasta.Value.Date;
            Mensaje("", false);
            CargarMovimientos();
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
            int idMateria = 0;
            MateriaPrima materia = cboMateriaMov.SelectedItem as MateriaPrima;
            if (materia != null)
            {
                idMateria = materia.IdMateriaPrima;
            }

            string tipo = "";
            if (cboTipoMov.SelectedIndex > 0)
            {
                tipo = cboTipoMov.SelectedItem.ToString();
            }

            try
            {
                movimientos = negocio.ListarMovimientos(desdeConsultado, hastaConsultado, idMateria, tipo);
            }
            catch (SqlException ex)
            {
                Mensaje("No se pudo cargar el historial: " + ex.Message, true);
                return;
            }

            dgvMovimientos.Rows.Clear();

            foreach (MovimientoMateriaPrima m in movimientos)
            {
                string cantidad = (m.Cantidad > 0 ? "+" : "") + Num(m.Cantidad) + " " + m.UnidadMedida;
                string compra = m.IdCompra > 0 ? m.IdCompra.ToString() : "";

                int fila = dgvMovimientos.Rows.Add(m.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                                                   m.MateriaPrima, m.Tipo, cantidad, m.CodigoLote, compra, m.Motivo, m.Usuario);

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