using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LaShiliquita.Entidades;
using LaShiliquita.Negocio;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Ventana para registrar un ingreso, una salida o un ajuste de un producto en una sede.
    // tipo: "INGRESO", "SALIDA" o "AJUSTE". Devuelve DialogResult.OK si la operacion se guardo.
    public class FrmMovimientoInventario : Form
    {
        private InventarioNegocio negocio = new InventarioNegocio();

        private string tipo;
        private Sede sede;
        private int idProducto;

        private NumericUpDown nudCantidad;
        private ComboBox cboLote;
        private TextBox txtMotivo;
        private Label lblMensaje;
        private Button btnGuardar;

        public FrmMovimientoInventario(string tipo, Sede sede, int idProducto, string nombreProducto, decimal stockActual)
        {
            this.tipo = tipo;
            this.sede = sede;
            this.idProducto = idProducto;

            ConstruirInterfaz(nombreProducto, stockActual);
            CargarLotes();
        }

        private void ConstruirInterfaz(string nombreProducto, decimal stockActual)
        {
            string titulo = "Ingreso de inventario";
            string textoBoton = "Guardar ingreso";

            if (tipo == "SALIDA")
            {
                titulo = "Salida de inventario";
                textoBoton = "Guardar salida";
            }
            else if (tipo == "AJUSTE")
            {
                titulo = "Ajuste de inventario";
                textoBoton = "Guardar ajuste";
            }

            this.Text = titulo;
            this.ClientSize = new Size(440, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.Font = Estilo.Fuente(10);

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Font = Estilo.FuenteNegrita(13);
            lblTitulo.ForeColor = Estilo.Texto;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(20, 16);

            Label lblInfo = new Label();
            lblInfo.Text = "Sede: " + sede.Nombre + "\r\nProducto: " + nombreProducto +
                           "\r\nStock actual: " + stockActual.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            lblInfo.Font = Estilo.Fuente(10);
            lblInfo.ForeColor = Estilo.Texto;
            lblInfo.Location = new Point(20, 52);
            lblInfo.Size = new Size(400, 60);

            // Cantidad
            Label lblCantidad = CrearEtiqueta("Cantidad", 20, 120);
            nudCantidad = new NumericUpDown();
            nudCantidad.DecimalPlaces = 0;
            nudCantidad.ThousandsSeparator = false;
            nudCantidad.Font = Estilo.Fuente(10.5f);
            nudCantidad.Location = new Point(20, 142);
            nudCantidad.Width = 140;
            nudCantidad.TextAlign = HorizontalAlignment.Right;

            if (tipo == "AJUSTE")
            {
                nudCantidad.Minimum = -InventarioNegocio.CantidadMaxima;
                nudCantidad.Maximum = InventarioNegocio.CantidadMaxima;
                nudCantidad.Value = 0;
                nudCantidad.ValueChanged += NudCantidad_ValueChanged;
            }
            else
            {
                nudCantidad.Minimum = 1;
                nudCantidad.Maximum = InventarioNegocio.CantidadMaxima;
                nudCantidad.Value = 1;
            }

            Label lblNotaCantidad = new Label();
            lblNotaCantidad.Font = Estilo.Fuente(9);
            lblNotaCantidad.ForeColor = Estilo.TextoSuave;
            lblNotaCantidad.Location = new Point(172, 136);
            lblNotaCantidad.Size = new Size(248, 40);
            lblNotaCantidad.TextAlign = ContentAlignment.MiddleLeft;
            if (tipo == "AJUSTE")
            {
                lblNotaCantidad.Text = "Positivo suma al stock; negativo lo resta.";
            }

            // Lote
            Label lblLote = CrearEtiqueta("Lote", 20, 184);
            cboLote = new ComboBox();
            cboLote.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLote.DisplayMember = "Texto";
            cboLote.Font = Estilo.Fuente(10);
            cboLote.Location = new Point(20, 206);
            cboLote.Width = 400;

            Label lblNotaLote = new Label();
            lblNotaLote.Font = Estilo.Fuente(9);
            lblNotaLote.ForeColor = Estilo.TextoSuave;
            lblNotaLote.Location = new Point(20, 184);
            lblNotaLote.Size = new Size(400, 40);

            if (tipo == "SALIDA")
            {
                // La salida descuenta sola: primero el lote que vence antes
                lblLote.Visible = false;
                cboLote.Visible = false;
                lblNotaLote.Text = "Se descuenta primero el lote que vence antes.";
            }
            else if (tipo == "AJUSTE")
            {
                cboLote.Enabled = false;   // solo se usa si la cantidad es positiva
                lblNotaLote.Visible = false;
            }
            else
            {
                lblNotaLote.Visible = false;
            }

            // Motivo
            string textoMotivo = (tipo == "INGRESO") ? "Motivo (opcional)" : "Motivo (obligatorio)";
            Label lblMotivo = CrearEtiqueta(textoMotivo, 20, 244);
            txtMotivo = new TextBox();
            txtMotivo.Font = Estilo.Fuente(10);
            txtMotivo.Location = new Point(20, 266);
            txtMotivo.Width = 400;
            txtMotivo.MaxLength = 200;

            lblMensaje = new Label();
            lblMensaje.Font = Estilo.Fuente(9.5f);
            lblMensaje.ForeColor = Estilo.Error;
            lblMensaje.Location = new Point(20, 298);
            lblMensaje.Size = new Size(400, 40);

            btnGuardar = new Button();
            btnGuardar.Text = textoBoton;
            btnGuardar.Location = new Point(20, 346);
            btnGuardar.Size = new Size(200, 38);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.BackColor = Estilo.Caramelo;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Font = Estilo.FuenteNegrita(10);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(232, 346);
            btnCancelar.Size = new Size(188, 38);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 1;
            btnCancelar.FlatAppearance.BorderColor = Estilo.TextoSuave;
            btnCancelar.BackColor = Color.White;
            btnCancelar.ForeColor = Estilo.Texto;
            btnCancelar.Font = Estilo.FuenteNegrita(10);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.Click += BtnCancelar_Click;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblCantidad);
            this.Controls.Add(nudCantidad);
            this.Controls.Add(lblNotaCantidad);
            this.Controls.Add(lblLote);
            this.Controls.Add(cboLote);
            this.Controls.Add(lblNotaLote);
            this.Controls.Add(lblMotivo);
            this.Controls.Add(txtMotivo);
            this.Controls.Add(lblMensaje);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
        }

        private Label CrearEtiqueta(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.AutoSize = true;
            etiqueta.Location = new Point(x, y);
            etiqueta.Font = Estilo.Fuente(9.5f);
            etiqueta.ForeColor = Estilo.TextoSuave;
            return etiqueta;
        }

        // Llena la lista de lotes (solo en ingreso y ajuste): "Sin lote" y los lotes del producto
        private void CargarLotes()
        {
            if (tipo == "SALIDA")
            {
                return;
            }

            LoteStock sinLote = new LoteStock();
            sinLote.IdLote = 0;
            sinLote.CodigoLote = "Sin lote";
            cboLote.Items.Add(sinLote);

            try
            {
                foreach (LoteStock lote in negocio.ListarLotes(idProducto, sede.IdPunto))
                {
                    cboLote.Items.Add(lote);
                }
            }
            catch (SqlException ex)
            {
                lblMensaje.Text = "No se pudieron cargar los lotes: " + ex.Message;
            }

            cboLote.SelectedIndex = 0;
        }

        // En un ajuste el lote solo importa cuando se suma stock (cantidad positiva)
        private void NudCantidad_ValueChanged(object sender, EventArgs e)
        {
            cboLote.Enabled = nudCantidad.Value > 0;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Se deshabilita mientras procesa para evitar registros duplicados por doble clic
            btnGuardar.Enabled = false;

            try
            {
                lblMensaje.Text = "";

                decimal cantidad = nudCantidad.Value;
                int idLote = 0;
                LoteStock lote = cboLote.SelectedItem as LoteStock;
                if (lote != null)
                {
                    idLote = lote.IdLote;
                }

                string error;

                try
                {
                    if (tipo == "INGRESO")
                    {
                        error = negocio.RegistrarIngreso(sede.IdPunto, idProducto, idLote, cantidad, txtMotivo.Text);
                    }
                    else if (tipo == "SALIDA")
                    {
                        error = negocio.RegistrarSalida(sede.IdPunto, idProducto, cantidad, txtMotivo.Text);
                    }
                    else
                    {
                        error = negocio.RegistrarAjuste(sede.IdPunto, idProducto, idLote, cantidad, txtMotivo.Text);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("No se pudo registrar la operación. No se guardó nada.\n\n" + ex.Message,
                                    "Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (error != "")
                {
                    lblMensaje.Text = error;
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}