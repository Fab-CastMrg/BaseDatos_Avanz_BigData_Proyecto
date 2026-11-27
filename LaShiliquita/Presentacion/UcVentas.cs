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
    // Modulo de ventas: catalogo en tarjetas a la izquierda y la venta actual (carrito, totales y pago) a la derecha.
    // La interfaz se construye por codigo (no usa el diseñador).
    public class UcVentas : UserControl
    {
        private VentaNegocio negocio = new VentaNegocio();

        private List<ProductoVenta> productos = new List<ProductoVenta>();
        private List<ItemCarrito> carrito = new List<ItemCarrito>();
        private List<PagoVenta> pagos = new List<PagoVenta>();
        private decimal vuelto = 0;          // vuelto por pagar de mas en efectivo
        private bool actualizando = false;   // evita reacciones en cadena al cambiar el descuento por codigo

        // Un color por categoria (se elige con el id de la categoria)
        private static readonly Color[] Paleta =
        {
            Color.FromArgb(139, 90, 60),
            Color.FromArgb(200, 135, 58),
            Color.FromArgb(120, 144, 100),
            Color.FromArgb(150, 90, 110),
            Color.FromArgb(90, 120, 150),
            Color.FromArgb(170, 120, 90)
        };

        private TextBox txtBuscar;
        private FlowLayoutPanel flpCatalogo;
        private Label lblMensaje;

        private DataGridView dgvCarrito;
        private Label lblSubtotal;
        private Label lblIgv;
        private Label lblTotal;
        private NumericUpDown nudDescuento;

        private ComboBox cboMetodo;
        private TextBox txtMonto;
        private TextBox txtOperacion;
        private ListBox lstPagos;
        private Label lblResumenPago;
        private Label lblVuelto;
        private Button btnConfirmar;

        public UcVentas()
        {
            ConstruirInterfaz();
            this.Load += UcVentas_Load;
        }

        // ---------- Construccion de la interfaz ----------

        private void ConstruirInterfaz()
        {
            this.BackColor = Estilo.Fondo;

            Panel pnlCatalogo = ConstruirCatalogo();
            Panel pnlVenta = ConstruirVenta();

            // Fill primero y Right despues
            this.Controls.Add(pnlCatalogo);
            this.Controls.Add(pnlVenta);
        }

        private Panel ConstruirCatalogo()
        {
            Panel pnlCatalogo = new Panel();
            pnlCatalogo.Dock = DockStyle.Fill;
            pnlCatalogo.Padding = new Padding(0, 0, 16, 0);

            // Tarjetas de productos (Fill)
            flpCatalogo = new FlowLayoutPanel();
            flpCatalogo.Dock = DockStyle.Fill;
            flpCatalogo.AutoScroll = true;
            flpCatalogo.WrapContents = true;
            flpCatalogo.FlowDirection = FlowDirection.LeftToRight;
            flpCatalogo.BackColor = Estilo.Fondo;

            // Mensajes (Bottom)
            lblMensaje = new Label();
            lblMensaje.Dock = DockStyle.Bottom;
            lblMensaje.Height = 30;
            lblMensaje.Font = Estilo.Fuente(9.5f);
            lblMensaje.ForeColor = Estilo.TextoSuave;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;

            // Busqueda (Top)
            Panel pnlBusqueda = new Panel();
            pnlBusqueda.Dock = DockStyle.Top;
            pnlBusqueda.Height = 44;

            Label lblBuscar = CrearEtiqueta("Buscar:", 0, 11);

            txtBuscar = new TextBox();
            txtBuscar.Font = Estilo.Fuente(10.5f);
            txtBuscar.Location = new Point(62, 7);
            txtBuscar.Width = 260;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;

            Button btnActualizar = CrearBoton("Actualizar", 334, 6, 110, 30, Color.White, Estilo.Texto);
            ConContorno(btnActualizar, Estilo.TextoSuave);
            btnActualizar.Click += BtnActualizar_Click;

            pnlBusqueda.Controls.Add(lblBuscar);
            pnlBusqueda.Controls.Add(txtBuscar);
            pnlBusqueda.Controls.Add(btnActualizar);

            pnlCatalogo.Controls.Add(flpCatalogo);
            pnlCatalogo.Controls.Add(lblMensaje);
            pnlCatalogo.Controls.Add(pnlBusqueda);

            return pnlCatalogo;
        }

        private Panel ConstruirVenta()
        {
            Panel pnlVenta = new Panel();
            pnlVenta.Dock = DockStyle.Right;
            pnlVenta.Width = 430;
            pnlVenta.BackColor = Color.White;

            // ---------- Titulo (Top) ----------
            Label lblTituloVenta = new Label();
            lblTituloVenta.Text = "Venta actual";
            lblTituloVenta.Dock = DockStyle.Top;
            lblTituloVenta.Height = 36;
            lblTituloVenta.Font = Estilo.FuenteNegrita(12);
            lblTituloVenta.ForeColor = Estilo.Texto;
            lblTituloVenta.TextAlign = ContentAlignment.MiddleLeft;
            lblTituloVenta.Padding = new Padding(12, 0, 0, 0);

            // ---------- Carrito (Fill) ----------
            dgvCarrito = new DataGridView();
            dgvCarrito.Dock = DockStyle.Fill;
            dgvCarrito.ReadOnly = true;
            dgvCarrito.AllowUserToAddRows = false;
            dgvCarrito.AllowUserToDeleteRows = false;
            dgvCarrito.AllowUserToResizeRows = false;
            dgvCarrito.MultiSelect = false;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCarrito.RowHeadersVisible = false;
            dgvCarrito.BackgroundColor = Color.White;
            dgvCarrito.BorderStyle = BorderStyle.None;
            dgvCarrito.Font = Estilo.Fuente(10);
            dgvCarrito.RowTemplate.Height = 30;
            dgvCarrito.EnableHeadersVisualStyles = false;
            dgvCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvCarrito.ColumnHeadersHeight = 32;
            dgvCarrito.ColumnHeadersDefaultCellStyle.BackColor = Estilo.Cafe;
            dgvCarrito.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCarrito.ColumnHeadersDefaultCellStyle.SelectionBackColor = Estilo.Cafe;
            dgvCarrito.DefaultCellStyle.SelectionBackColor = Estilo.Caramelo;
            dgvCarrito.DefaultCellStyle.SelectionForeColor = Color.White;

            DataGridViewTextBoxColumn colProducto = CrearColumna("Producto", 100, false);
            colProducto.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCarrito.Columns.Add(colProducto);
            dgvCarrito.Columns.Add(CrearColumna("Cant.", 50, true));
            dgvCarrito.Columns.Add(CrearColumna("P. unit.", 70, true));
            dgvCarrito.Columns.Add(CrearColumna("Subtotal", 80, true));

            // ---------- Botones del carrito (Bottom) ----------
            Panel pnlBotonesCarrito = new Panel();
            pnlBotonesCarrito.Dock = DockStyle.Bottom;
            pnlBotonesCarrito.Height = 40;

            Button btnMas = CrearBoton("+", 12, 5, 56, 30, Estilo.Caramelo, Color.White);
            btnMas.Click += BtnMas_Click;
            Button btnMenos = CrearBoton("-", 74, 5, 56, 30, Color.White, Estilo.Texto);
            ConContorno(btnMenos, Estilo.TextoSuave);
            btnMenos.Click += BtnMenos_Click;
            Button btnQuitarItem = CrearBoton("Quitar", 136, 5, 90, 30, Color.White, Estilo.Texto);
            ConContorno(btnQuitarItem, Estilo.TextoSuave);
            btnQuitarItem.Click += BtnQuitarItem_Click;
            Button btnVaciar = CrearBoton("Vaciar", 232, 5, 90, 30, Color.White, Estilo.Error);
            ConContorno(btnVaciar, Estilo.Error);
            btnVaciar.Click += BtnVaciar_Click;

            pnlBotonesCarrito.Controls.Add(btnMas);
            pnlBotonesCarrito.Controls.Add(btnMenos);
            pnlBotonesCarrito.Controls.Add(btnQuitarItem);
            pnlBotonesCarrito.Controls.Add(btnVaciar);

            // ---------- Totales (Bottom) ----------
            Panel pnlTotales = new Panel();
            pnlTotales.Dock = DockStyle.Bottom;
            pnlTotales.Height = 112;

            Label lblT1 = CrearEtiqueta("Subtotal", 12, 8);
            lblSubtotal = CrearValor(200, 6, 214, 24);

            Label lblT2 = CrearEtiqueta("Descuento (S/)", 12, 36);
            nudDescuento = new NumericUpDown();
            nudDescuento.DecimalPlaces = 2;
            nudDescuento.Increment = 0.5m;
            nudDescuento.Minimum = 0;
            nudDescuento.Maximum = VentaNegocio.DescuentoMaximo;
            nudDescuento.Font = Estilo.Fuente(10.5f);
            nudDescuento.TextAlign = HorizontalAlignment.Right;
            nudDescuento.Location = new Point(314, 33);
            nudDescuento.Width = 100;
            nudDescuento.ValueChanged += NudDescuento_ValueChanged;

            string textoIgv = "IGV incluido (" + (negocio.ObtenerTasaIgv() * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%)";
            Label lblT3 = CrearEtiqueta(textoIgv, 12, 62);
            lblIgv = CrearValor(200, 60, 214, 24);

            Label lblT4 = CrearEtiqueta("TOTAL", 12, 84);
            lblT4.Font = Estilo.FuenteNegrita(13);
            lblTotal = CrearValor(200, 80, 214, 28);
            lblTotal.Font = Estilo.FuenteNegrita(15);
            lblTotal.ForeColor = Estilo.Caramelo;

            pnlTotales.Controls.Add(lblT1);
            pnlTotales.Controls.Add(lblSubtotal);
            pnlTotales.Controls.Add(lblT2);
            pnlTotales.Controls.Add(nudDescuento);
            pnlTotales.Controls.Add(lblT3);
            pnlTotales.Controls.Add(lblIgv);
            pnlTotales.Controls.Add(lblT4);
            pnlTotales.Controls.Add(lblTotal);

            // ---------- Pago (Bottom) ----------
            Panel pnlPago = new Panel();
            pnlPago.Dock = DockStyle.Bottom;
            pnlPago.Height = 200;

            Label lblPago = CrearEtiqueta("Pago", 12, 4);
            lblPago.Font = Estilo.FuenteNegrita(10.5f);

            Label lblMetodo = CrearEtiqueta("Método", 12, 26);
            lblMetodo.Font = Estilo.Fuente(9);
            lblMetodo.ForeColor = Estilo.TextoSuave;
            Label lblMonto = CrearEtiqueta("Monto (S/)", 140, 26);
            lblMonto.Font = Estilo.Fuente(9);
            lblMonto.ForeColor = Estilo.TextoSuave;

            cboMetodo = new ComboBox();
            cboMetodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMetodo.Font = Estilo.Fuente(10.5f);
            cboMetodo.Location = new Point(12, 44);
            cboMetodo.Width = 118;
            foreach (string metodo in VentaNegocio.MetodosPago)
            {
                cboMetodo.Items.Add(metodo);
            }
            cboMetodo.SelectedIndex = 0;

            txtMonto = new TextBox();
            txtMonto.Font = Estilo.Fuente(10.5f);
            txtMonto.Location = new Point(140, 44);
            txtMonto.Width = 90;
            txtMonto.TextAlign = HorizontalAlignment.Right;

            Button btnAgregarPago = CrearBoton("Agregar", 240, 42, 80, 30, Estilo.Caramelo, Color.White);
            btnAgregarPago.Click += BtnAgregarPago_Click;
            Button btnQuitarPago = CrearBoton("Quitar", 326, 42, 80, 30, Color.White, Estilo.Texto);
            ConContorno(btnQuitarPago, Estilo.TextoSuave);
            btnQuitarPago.Click += BtnQuitarPago_Click;

            Label lblOperacion = CrearEtiqueta("N° oper. (opc.)", 12, 84);
            lblOperacion.Font = Estilo.Fuente(9);
            lblOperacion.ForeColor = Estilo.TextoSuave;
            txtOperacion = new TextBox();
            txtOperacion.Font = Estilo.Fuente(10);
            txtOperacion.Location = new Point(110, 80);
            txtOperacion.Width = 296;
            txtOperacion.MaxLength = 100;

            lstPagos = new ListBox();
            lstPagos.Font = Estilo.Fuente(9.5f);
            lstPagos.Location = new Point(12, 110);
            lstPagos.Size = new Size(394, 44);

            lblResumenPago = new Label();
            lblResumenPago.Location = new Point(12, 158);
            lblResumenPago.Size = new Size(394, 20);
            lblResumenPago.Font = Estilo.FuenteNegrita(9.5f);
            lblResumenPago.ForeColor = Estilo.Texto;

            lblVuelto = new Label();
            lblVuelto.Location = new Point(12, 178);
            lblVuelto.Size = new Size(394, 20);
            lblVuelto.Font = Estilo.FuenteNegrita(9.5f);
            lblVuelto.ForeColor = Estilo.Caramelo;

            pnlPago.Controls.Add(lblPago);
            pnlPago.Controls.Add(lblMetodo);
            pnlPago.Controls.Add(lblMonto);
            pnlPago.Controls.Add(cboMetodo);
            pnlPago.Controls.Add(txtMonto);
            pnlPago.Controls.Add(btnAgregarPago);
            pnlPago.Controls.Add(btnQuitarPago);
            pnlPago.Controls.Add(lblOperacion);
            pnlPago.Controls.Add(txtOperacion);
            pnlPago.Controls.Add(lstPagos);
            pnlPago.Controls.Add(lblResumenPago);
            pnlPago.Controls.Add(lblVuelto);

            // ---------- Confirmar (Bottom) ----------
            btnConfirmar = CrearBoton("Confirmar venta", 0, 0, 100, 46, Estilo.Caramelo, Color.White);
            btnConfirmar.Dock = DockStyle.Bottom;
            btnConfirmar.Font = Estilo.FuenteNegrita(11);
            btnConfirmar.Click += BtnConfirmar_Click;

            // Orden: Fill primero; despues los Bottom (el ultimo queda mas abajo) y al final el Top
            pnlVenta.Controls.Add(dgvCarrito);
            pnlVenta.Controls.Add(pnlBotonesCarrito);
            pnlVenta.Controls.Add(pnlTotales);
            pnlVenta.Controls.Add(pnlPago);
            pnlVenta.Controls.Add(btnConfirmar);
            pnlVenta.Controls.Add(lblTituloVenta);

            return pnlVenta;
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

        private Label CrearValor(int x, int y, int ancho, int alto)
        {
            Label valor = new Label();
            valor.Location = new Point(x, y);
            valor.Size = new Size(ancho, alto);
            valor.TextAlign = ContentAlignment.MiddleRight;
            valor.Font = Estilo.Fuente(10.5f);
            valor.ForeColor = Estilo.Texto;
            return valor;
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

        // Numeros siempre con punto decimal y dos decimales (ej. 12.50)
        private string Num(decimal valor)
        {
            return valor.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private string Dinero(decimal valor)
        {
            return "S/ " + Num(valor);
        }

        private void Mensaje(string texto, bool esError)
        {
            lblMensaje.ForeColor = esError ? Estilo.Error : Estilo.TextoSuave;
            lblMensaje.Text = texto;
        }

        // ---------- Catalogo ----------

        private void UcVentas_Load(object sender, EventArgs e)
        {
            ActualizarTotales();
            CargarCatalogo();
        }

        // Lee los productos con el stock de la sede y ajusta el carrito si el stock cambio
        private void CargarCatalogo()
        {
            try
            {
                productos = negocio.ListarCatalogo();
            }
            catch (SqlException ex)
            {
                Mensaje("No se pudo cargar el catálogo: " + ex.Message, true);
                return;
            }

            MostrarCatalogo();

            if (SincronizarCarritoConStock())
            {
                ReiniciarPagosSiHay();
                RefrescarCarrito(IndiceSeleccionado());
                Mensaje("El carrito se ajustó al stock actual.", true);
            }
        }

        private void MostrarCatalogo()
        {
            string filtro = txtBuscar.Text.Trim();

            flpCatalogo.SuspendLayout();

            while (flpCatalogo.Controls.Count > 0)
            {
                Control anterior = flpCatalogo.Controls[0];
                flpCatalogo.Controls.Remove(anterior);
                anterior.Dispose();
            }

            foreach (ProductoVenta p in productos)
            {
                if (filtro != "" && !Coincide(p, filtro))
                {
                    continue;
                }
                flpCatalogo.Controls.Add(CrearTarjeta(p));
            }

            flpCatalogo.ResumeLayout();
        }

        private bool Coincide(ProductoVenta p, string filtro)
        {
            return p.Nombre.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0
                || p.Presentacion.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0
                || p.Categoria.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Tarjeta clicable de un producto: franja de color por categoria, nombre, presentacion, precio y stock
        private Panel CrearTarjeta(ProductoVenta p)
        {
            int stock = (int)Math.Floor(p.Stock);
            bool agotado = stock < 1;
            Color colorCategoria = Paleta[p.IdCategoria % Paleta.Length];

            Panel tarjeta = new Panel();
            tarjeta.Size = new Size(215, 118);
            tarjeta.Margin = new Padding(0, 0, 12, 12);
            tarjeta.BackColor = Color.White;
            tarjeta.Tag = p;

            Panel franja = new Panel();
            franja.Dock = DockStyle.Left;
            franja.Width = 8;
            franja.BackColor = agotado ? Color.Silver : colorCategoria;
            franja.Tag = p;

            Label lblNombre = new Label();
            lblNombre.Text = p.Nombre;
            lblNombre.Font = Estilo.FuenteNegrita(10);
            lblNombre.ForeColor = agotado ? Estilo.TextoSuave : Estilo.Texto;
            lblNombre.Location = new Point(16, 8);
            lblNombre.Size = new Size(192, 38);
            lblNombre.Tag = p;

            Label lblPresentacion = new Label();
            lblPresentacion.Text = p.Presentacion;
            lblPresentacion.Font = Estilo.Fuente(9);
            lblPresentacion.ForeColor = Estilo.TextoSuave;
            lblPresentacion.AutoEllipsis = true;
            lblPresentacion.Location = new Point(16, 48);
            lblPresentacion.Size = new Size(192, 18);
            lblPresentacion.Tag = p;

            Label lblPrecio = new Label();
            lblPrecio.Text = Dinero(p.PrecioVenta);
            lblPrecio.Font = Estilo.FuenteNegrita(12);
            lblPrecio.ForeColor = agotado ? Estilo.TextoSuave : Estilo.Caramelo;
            lblPrecio.Location = new Point(16, 78);
            lblPrecio.Size = new Size(110, 26);
            lblPrecio.Tag = p;

            Label lblStock = new Label();
            lblStock.Text = agotado ? "Agotado" : "Stock: " + stock;
            lblStock.Font = Estilo.Fuente(9);
            lblStock.ForeColor = agotado ? Estilo.Error : Estilo.TextoSuave;
            lblStock.TextAlign = ContentAlignment.MiddleRight;
            lblStock.Location = new Point(120, 80);
            lblStock.Size = new Size(88, 22);
            lblStock.Tag = p;

            tarjeta.Controls.Add(franja);
            tarjeta.Controls.Add(lblNombre);
            tarjeta.Controls.Add(lblPresentacion);
            tarjeta.Controls.Add(lblPrecio);
            tarjeta.Controls.Add(lblStock);

            // Solo los productos con stock responden al clic (en la tarjeta o en cualquiera de sus partes)
            if (!agotado)
            {
                Control[] partes = { tarjeta, franja, lblNombre, lblPresentacion, lblPrecio, lblStock };
                foreach (Control parte in partes)
                {
                    parte.Cursor = Cursors.Hand;
                    parte.Click += Tarjeta_Click;
                }
            }

            return tarjeta;
        }

        private void Tarjeta_Click(object sender, EventArgs e)
        {
            Control origen = (Control)sender;
            ProductoVenta producto = (ProductoVenta)origen.Tag;
            AgregarAlCarrito(producto);
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            MostrarCatalogo();
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            CargarCatalogo();
            Mensaje("Catálogo actualizado.", false);
        }

        // ---------- Carrito ----------

        private void AgregarAlCarrito(ProductoVenta p)
        {
            int stock = (int)Math.Floor(p.Stock);
            ItemCarrito existente = null;

            foreach (ItemCarrito item in carrito)
            {
                if (item.IdProducto == p.IdProducto)
                {
                    existente = item;
                }
            }

            int indice;

            if (existente != null)
            {
                if (existente.Cantidad >= stock)
                {
                    Mensaje("Ya agregaste todo el stock disponible de \"" + p.Nombre + "\".", true);
                    return;
                }
                existente.Cantidad++;
                indice = carrito.IndexOf(existente);
            }
            else
            {
                ItemCarrito nuevo = new ItemCarrito();
                nuevo.IdProducto = p.IdProducto;
                nuevo.Nombre = p.Nombre;
                nuevo.PrecioUnitario = p.PrecioVenta;
                nuevo.Cantidad = 1;
                nuevo.StockDisponible = stock;
                carrito.Add(nuevo);
                indice = carrito.Count - 1;
            }

            Mensaje("", false);
            ReiniciarPagosSiHay();
            RefrescarCarrito(indice);
        }

        private int IndiceSeleccionado()
        {
            if (dgvCarrito.SelectedRows.Count == 0)
            {
                return -1;
            }
            return dgvCarrito.SelectedRows[0].Index;
        }

        // Vuelve a llenar la tabla del carrito y deja seleccionada la fila indicada
        private void RefrescarCarrito(int indice)
        {
            dgvCarrito.Rows.Clear();

            foreach (ItemCarrito item in carrito)
            {
                dgvCarrito.Rows.Add(item.Nombre, item.Cantidad, Num(item.PrecioUnitario), Num(item.Subtotal));
            }

            if (indice >= carrito.Count)
            {
                indice = carrito.Count - 1;
            }

            if (indice >= 0)
            {
                dgvCarrito.ClearSelection();
                dgvCarrito.Rows[indice].Selected = true;
                dgvCarrito.CurrentCell = dgvCarrito.Rows[indice].Cells[0];
            }

            ActualizarTotales();
        }

        private void BtnMas_Click(object sender, EventArgs e)
        {
            int indice = IndiceSeleccionado();
            if (indice < 0)
            {
                return;
            }

            ItemCarrito item = carrito[indice];

            if (item.Cantidad + 1 > item.StockDisponible)
            {
                Mensaje("No hay más stock de \"" + item.Nombre + "\" (disponible: " + item.StockDisponible + ").", true);
                return;
            }

            item.Cantidad++;
            Mensaje("", false);
            ReiniciarPagosSiHay();
            RefrescarCarrito(indice);
        }

        private void BtnMenos_Click(object sender, EventArgs e)
        {
            int indice = IndiceSeleccionado();
            if (indice < 0)
            {
                return;
            }

            ItemCarrito item = carrito[indice];
            item.Cantidad--;

            if (item.Cantidad <= 0)
            {
                carrito.RemoveAt(indice);
            }

            Mensaje("", false);
            ReiniciarPagosSiHay();
            RefrescarCarrito(indice);
        }

        private void BtnQuitarItem_Click(object sender, EventArgs e)
        {
            int indice = IndiceSeleccionado();
            if (indice < 0)
            {
                return;
            }

            carrito.RemoveAt(indice);
            Mensaje("", false);
            ReiniciarPagosSiHay();
            RefrescarCarrito(indice);
        }

        private void BtnVaciar_Click(object sender, EventArgs e)
        {
            if (carrito.Count == 0)
            {
                return;
            }

            NuevaVenta();
            Mensaje("", false);
        }

        // Ajusta el carrito al stock recien leido. Devuelve true si cambio algo.
        private bool SincronizarCarritoConStock()
        {
            bool huboCambios = false;

            for (int i = carrito.Count - 1; i >= 0; i--)
            {
                ItemCarrito item = carrito[i];
                int stock = 0;

                foreach (ProductoVenta p in productos)
                {
                    if (p.IdProducto == item.IdProducto)
                    {
                        stock = (int)Math.Floor(p.Stock);
                    }
                }

                item.StockDisponible = stock;

                if (stock < 1)
                {
                    carrito.RemoveAt(i);
                    huboCambios = true;
                }
                else if (item.Cantidad > stock)
                {
                    item.Cantidad = stock;
                    huboCambios = true;
                }
            }

            return huboCambios;
        }

        // ---------- Totales y descuento ----------

        private decimal TotalActual()
        {
            return negocio.CalcularSubtotal(carrito) - nudDescuento.Value;
        }

        private void ActualizarTotales()
        {
            decimal subtotal = negocio.CalcularSubtotal(carrito);
            bool descuentoPermitido = negocio.DescuentoPermitido(subtotal);

            // El descuento solo se habilita si el subtotal supera 200 soles
            actualizando = true;
            nudDescuento.Enabled = descuentoPermitido;
            if (!descuentoPermitido)
            {
                nudDescuento.Value = 0;
            }
            actualizando = false;

            decimal total = subtotal - nudDescuento.Value;

            lblSubtotal.Text = Dinero(subtotal);
            lblIgv.Text = Dinero(negocio.CalcularIgv(total));
            lblTotal.Text = Dinero(total);

            ActualizarResumenPago();
        }

        private void NudDescuento_ValueChanged(object sender, EventArgs e)
        {
            if (actualizando)
            {
                return;
            }

            ReiniciarPagosSiHay();
            ActualizarTotales();
        }

        // ---------- Pagos ----------

        private decimal SumaPagos()
        {
            decimal suma = 0;

            foreach (PagoVenta pago in pagos)
            {
                suma = suma + pago.Monto;
            }

            return suma;
        }

        // Si cambia el total, los pagos ya registrados dejan de servir
        private void ReiniciarPagosSiHay()
        {
            if (pagos.Count > 0)
            {
                pagos.Clear();
                vuelto = 0;
                RefrescarListaPagos();
                Mensaje("Cambió el total: se reiniciaron los pagos.", false);
            }
        }

        private void RefrescarListaPagos()
        {
            lstPagos.Items.Clear();

            foreach (PagoVenta pago in pagos)
            {
                string texto = pago.MetodoPago + "   " + Dinero(pago.Monto);
                if (!string.IsNullOrEmpty(pago.NumeroOperacion))
                {
                    texto = texto + "   (op. " + pago.NumeroOperacion + ")";
                }
                lstPagos.Items.Add(texto);
            }
        }

        // Muestra lo pagado y lo que falta, y deja en el cuadro de monto lo que falta pagar
        private void ActualizarResumenPago()
        {
            decimal total = TotalActual();
            decimal pagado = SumaPagos();
            decimal falta = total - pagado;

            if (falta < 0)
            {
                falta = 0;
            }

            lblResumenPago.Text = "Pagado " + Dinero(pagado) + "     Falta " + Dinero(falta);
            lblVuelto.Text = vuelto > 0 ? "Vuelto a entregar: " + Dinero(vuelto) : "";
            txtMonto.Text = falta > 0 ? Num(falta) : "";
        }

        private void BtnAgregarPago_Click(object sender, EventArgs e)
        {
            if (carrito.Count == 0)
            {
                Mensaje("Agrega productos al carrito antes de registrar pagos.", true);
                return;
            }

            decimal falta = TotalActual() - SumaPagos();

            if (falta <= 0)
            {
                Mensaje("El total ya está cubierto.", true);
                return;
            }

            // Se acepta punto o coma como separador decimal
            decimal monto;
            string textoMonto = txtMonto.Text.Trim().Replace(',', '.');

            if (!decimal.TryParse(textoMonto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out monto) || monto <= 0)
            {
                Mensaje("Escribe un monto válido mayor que cero.", true);
                return;
            }

            monto = Math.Round(monto, 2);
            string metodo = cboMetodo.SelectedItem.ToString();

            if (monto > falta)
            {
                if (metodo == "Efectivo")
                {
                    // Pago en efectivo de mas: se registra solo lo que falta y la diferencia es el vuelto
                    vuelto = vuelto + (monto - falta);
                    monto = falta;
                }
                else
                {
                    Mensaje("El monto supera lo que falta por pagar (" + Dinero(falta) + ").", true);
                    return;
                }
            }

            PagoVenta pago = new PagoVenta();
            pago.MetodoPago = metodo;
            pago.Monto = monto;
            pago.NumeroOperacion = txtOperacion.Text.Trim();
            pagos.Add(pago);

            txtOperacion.Text = "";
            Mensaje("", false);
            RefrescarListaPagos();
            ActualizarResumenPago();
        }

        private void BtnQuitarPago_Click(object sender, EventArgs e)
        {
            int indice = lstPagos.SelectedIndex;
            if (indice < 0)
            {
                return;
            }

            pagos.RemoveAt(indice);
            vuelto = 0;
            Mensaje("", false);
            RefrescarListaPagos();
            ActualizarResumenPago();
        }

        // ---------- Confirmar la venta ----------

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            // Se deshabilita mientras procesa para evitar ventas duplicadas por doble clic
            btnConfirmar.Enabled = false;

            try
            {
                int idVenta = 0;
                string error;

                try
                {
                    error = negocio.RegistrarVenta(carrito, nudDescuento.Value, pagos, out idVenta);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("No se pudo registrar la venta. No se guardó nada.\n\n" + ex.Message,
                                    "Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (error != "")
                {
                    Mensaje(error, true);
                    MessageBox.Show(error, "No se pudo registrar la venta", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Si el problema fue de stock, se vuelve a leer el catalogo para ver el stock real
                    if (error.IndexOf("stock", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        CargarCatalogo();
                    }
                    return;
                }

                // La venta ya esta guardada: el ticket se arma con lo guardado en la BD
                decimal vueltoVenta = vuelto;
                Venta venta = null;

                try
                {
                    venta = negocio.ObtenerVentaParaTicket(idVenta);
                }
                catch (SqlException)
                {
                    venta = null;
                }

                NuevaVenta();
                CargarCatalogo();

                if (venta != null)
                {
                    Mensaje("Venta N° " + idVenta + " registrada.", false);
                    FrmTicket ticket = new FrmTicket(venta, vueltoVenta);
                    ticket.ShowDialog(this.FindForm());
                }
                else
                {
                    Mensaje("Venta N° " + idVenta + " registrada, pero no se pudo mostrar el ticket.", false);
                }
            }
            finally
            {
                btnConfirmar.Enabled = true;
            }
        }

        // Deja la pantalla lista para una venta nueva
        private void NuevaVenta()
        {
            carrito.Clear();
            pagos.Clear();
            vuelto = 0;

            actualizando = true;
            nudDescuento.Value = 0;
            actualizando = false;

            txtOperacion.Text = "";
            RefrescarListaPagos();
            RefrescarCarrito(-1);
        }
    }
}