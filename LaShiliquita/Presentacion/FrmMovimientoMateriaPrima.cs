using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LaShiliquita.Entidades;
using LaShiliquita.Negocio;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Ventana para registrar un ingreso, una salida o un ajuste de una materia prima.
    // tipo: "INGRESO", "SALIDA" o "AJUSTE". Devuelve DialogResult.OK si la operacion se guardo.
    public class FrmMovimientoMateriaPrima : Form
    {
        private MateriaPrimaNegocio negocio = new MateriaPrimaNegocio();

        private string tipo;
        private MateriaPrima materia;

        private NumericUpDown nudCantidad;
        private TextBox txtMotivo;
        private Label lblMensaje;
        private Button btnGuardar;

        public FrmMovimientoMateriaPrima(string tipo, MateriaPrima materia)
        {
            this.tipo = tipo;
            this.materia = materia;
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            string titulo = "Ingreso de materia prima";
            string textoBoton = "Guardar ingreso";

            if (tipo == "SALIDA")
            {
                titulo = "Salida de materia prima";
                textoBoton = "Guardar salida";
            }
            else if (tipo == "AJUSTE")
            {
                titulo = "Ajuste de materia prima";
                textoBoton = "Guardar ajuste";
            }

            this.Text = titulo;
            this.ClientSize = new Size(440, 350);
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
            lblInfo.Text = "Materia prima: " + materia.Nombre + "\r\nStock actual: " +
                           materia.StockActual.ToString("0.##", CultureInfo.InvariantCulture) + " " + materia.UnidadMedida;
            lblInfo.Font = Estilo.Fuente(10);
            lblInfo.ForeColor = Estilo.Texto;
            lblInfo.Location = new Point(20, 52);
            lblInfo.Size = new Size(400, 44);

            Label lblCantidad = CrearEtiqueta("Cantidad", 20, 108);

            nudCantidad = new NumericUpDown();
            nudCantidad.DecimalPlaces = 2;
            nudCantidad.ThousandsSeparator = false;
            nudCantidad.Increment = 1;
            nudCantidad.Font = Estilo.Fuente(10.5f);
            nudCantidad.Location = new Point(20, 130);
            nudCantidad.Width = 140;
            nudCantidad.TextAlign = HorizontalAlignment.Right;

            if (tipo == "AJUSTE")
            {
                nudCantidad.Minimum = -MateriaPrimaNegocio.CantidadMaxima;
                nudCantidad.Maximum = MateriaPrimaNegocio.CantidadMaxima;
                nudCantidad.Value = 0;
            }
            else
            {
                nudCantidad.Minimum = 0.01m;
                nudCantidad.Maximum = MateriaPrimaNegocio.CantidadMaxima;
                nudCantidad.Value = 1;
            }

            Label lblUnidad = new Label();
            lblUnidad.Text = materia.UnidadMedida;
            lblUnidad.Font = Estilo.FuenteNegrita(10);
            lblUnidad.ForeColor = Estilo.Texto;
            lblUnidad.Location = new Point(168, 133);
            lblUnidad.AutoSize = true;

            Label lblNotaCantidad = new Label();
            lblNotaCantidad.Font = Estilo.Fuente(9);
            lblNotaCantidad.ForeColor = Estilo.TextoSuave;
            lblNotaCantidad.Location = new Point(232, 124);
            lblNotaCantidad.Size = new Size(188, 40);
            lblNotaCantidad.TextAlign = ContentAlignment.MiddleLeft;
            if (tipo == "AJUSTE")
            {
                lblNotaCantidad.Text = "Positivo suma al stock; negativo lo resta.";
            }

            string textoMotivo = (tipo == "INGRESO") ? "Motivo (opcional, ej.: Stock inicial)" : "Motivo (obligatorio)";
            Label lblMotivo = CrearEtiqueta(textoMotivo, 20, 176);
            txtMotivo = new TextBox();
            txtMotivo.Font = Estilo.Fuente(10);
            txtMotivo.Location = new Point(20, 198);
            txtMotivo.Width = 400;
            txtMotivo.MaxLength = 200;

            lblMensaje = new Label();
            lblMensaje.Font = Estilo.Fuente(9.5f);
            lblMensaje.ForeColor = Estilo.Error;
            lblMensaje.Location = new Point(20, 232);
            lblMensaje.Size = new Size(400, 50);

            btnGuardar = new Button();
            btnGuardar.Text = textoBoton;
            btnGuardar.Location = new Point(20, 296);
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
            btnCancelar.Location = new Point(232, 296);
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
            this.Controls.Add(lblUnidad);
            this.Controls.Add(lblNotaCantidad);
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

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Se deshabilita mientras procesa para evitar registros duplicados por doble clic
            btnGuardar.Enabled = false;

            try
            {
                lblMensaje.Text = "";

                decimal cantidad = nudCantidad.Value;
                string error;

                try
                {
                    if (tipo == "INGRESO")
                    {
                        error = negocio.RegistrarIngreso(materia.IdMateriaPrima, cantidad, txtMotivo.Text);
                    }
                    else if (tipo == "SALIDA")
                    {
                        error = negocio.RegistrarSalida(materia.IdMateriaPrima, cantidad, txtMotivo.Text);
                    }
                    else
                    {
                        error = negocio.RegistrarAjuste(materia.IdMateriaPrima, cantidad, txtMotivo.Text);
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