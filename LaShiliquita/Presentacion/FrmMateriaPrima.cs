using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LaShiliquita.Entidades;
using LaShiliquita.Negocio;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Ventana para crear o editar una materia prima.
    // Se le pasa la materia prima a editar, o null para crear una nueva.
    // Devuelve DialogResult.OK si se guardo; el id guardado queda en IdGuardado.
    public class FrmMateriaPrima : Form
    {
        private MateriaPrimaNegocio negocio = new MateriaPrimaNegocio();
        private MateriaPrima materia;

        public int IdGuardado = 0;

        private TextBox txtNombre;
        private ComboBox cboUnidad;
        private CheckBox chkMinimo;
        private NumericUpDown nudMinimo;
        private TextBox txtDescripcion;
        private Label lblMensaje;
        private Button btnGuardar;

        public FrmMateriaPrima(MateriaPrima materia)
        {
            this.materia = materia;
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            bool esNueva = (materia == null);

            this.Text = esNueva ? "Nueva materia prima" : "Editar materia prima";
            this.ClientSize = new Size(440, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.Font = Estilo.Fuente(10);

            Label lblTitulo = new Label();
            lblTitulo.Text = esNueva ? "Nueva materia prima" : "Editar materia prima";
            lblTitulo.Font = Estilo.FuenteNegrita(13);
            lblTitulo.ForeColor = Estilo.Texto;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(20, 16);

            Label lblNombre = CrearEtiqueta("Nombre", 20, 56);
            txtNombre = new TextBox();
            txtNombre.Font = Estilo.Fuente(10.5f);
            txtNombre.Location = new Point(20, 78);
            txtNombre.Width = 400;
            txtNombre.MaxLength = 100;

            Label lblUnidad = CrearEtiqueta("Unidad de medida", 20, 116);
            cboUnidad = new ComboBox();
            cboUnidad.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnidad.Font = Estilo.Fuente(10.5f);
            cboUnidad.Location = new Point(20, 138);
            cboUnidad.Width = 170;
            foreach (string unidad in MateriaPrimaNegocio.Unidades)
            {
                cboUnidad.Items.Add(unidad);
            }

            chkMinimo = new CheckBox();
            chkMinimo.Text = "Con stock mínimo";
            chkMinimo.Font = Estilo.Fuente(10);
            chkMinimo.ForeColor = Estilo.Texto;
            chkMinimo.Location = new Point(206, 140);
            chkMinimo.Size = new Size(126, 24);
            chkMinimo.CheckedChanged += ChkMinimo_CheckedChanged;

            nudMinimo = new NumericUpDown();
            nudMinimo.DecimalPlaces = 2;
            nudMinimo.Minimum = 0;
            nudMinimo.Maximum = MateriaPrimaNegocio.CantidadMaxima;
            nudMinimo.Increment = 1;
            nudMinimo.Font = Estilo.Fuente(10.5f);
            nudMinimo.TextAlign = HorizontalAlignment.Right;
            nudMinimo.Location = new Point(334, 138);
            nudMinimo.Width = 86;
            nudMinimo.Enabled = false;

            Label lblNotaUnidad = new Label();
            lblNotaUnidad.Font = Estilo.Fuente(9);
            lblNotaUnidad.ForeColor = Estilo.TextoSuave;
            lblNotaUnidad.Location = new Point(20, 168);
            lblNotaUnidad.Size = new Size(400, 20);
            if (!esNueva)
            {
                lblNotaUnidad.Text = "La unidad no se puede cambiar después de crear la materia prima.";
            }

            Label lblDescripcion = CrearEtiqueta("Descripción (opcional)", 20, 196);
            txtDescripcion = new TextBox();
            txtDescripcion.Font = Estilo.Fuente(10);
            txtDescripcion.Location = new Point(20, 218);
            txtDescripcion.Size = new Size(400, 60);
            txtDescripcion.Multiline = true;
            txtDescripcion.MaxLength = 200;

            lblMensaje = new Label();
            lblMensaje.Font = Estilo.Fuente(9.5f);
            lblMensaje.ForeColor = Estilo.Error;
            lblMensaje.Location = new Point(20, 288);
            lblMensaje.Size = new Size(400, 40);

            btnGuardar = new Button();
            btnGuardar.Text = "Guardar";
            btnGuardar.Location = new Point(20, 344);
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
            btnCancelar.Location = new Point(232, 344);
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
            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            this.Controls.Add(lblUnidad);
            this.Controls.Add(cboUnidad);
            this.Controls.Add(chkMinimo);
            this.Controls.Add(nudMinimo);
            this.Controls.Add(lblNotaUnidad);
            this.Controls.Add(lblDescripcion);
            this.Controls.Add(txtDescripcion);
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

        // Llena los campos: los de la materia prima al editar, o valores iniciales al crear
        private void CargarDatos()
        {
            if (materia == null)
            {
                cboUnidad.SelectedIndex = 0;
                return;
            }

            txtNombre.Text = materia.Nombre;
            txtDescripcion.Text = materia.Descripcion;

            // Si la unidad guardada no esta en la lista, se agrega para no perderla
            int indice = cboUnidad.Items.IndexOf(materia.UnidadMedida);
            if (indice < 0)
            {
                indice = cboUnidad.Items.Add(materia.UnidadMedida);
            }
            cboUnidad.SelectedIndex = indice;
            cboUnidad.Enabled = false;   // cambiar la unidad desordenaria los movimientos ya registrados

            chkMinimo.Checked = materia.TieneMinimo;
            if (materia.TieneMinimo)
            {
                nudMinimo.Value = Math.Min(materia.StockMinimo, nudMinimo.Maximum);
            }
        }

        private void ChkMinimo_CheckedChanged(object sender, EventArgs e)
        {
            nudMinimo.Enabled = chkMinimo.Checked;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Se deshabilita mientras procesa para evitar registros duplicados por doble clic
            btnGuardar.Enabled = false;

            try
            {
                lblMensaje.Text = "";

                MateriaPrima nueva = new MateriaPrima();
                nueva.IdMateriaPrima = (materia == null) ? 0 : materia.IdMateriaPrima;
                nueva.Nombre = txtNombre.Text;
                nueva.UnidadMedida = (cboUnidad.SelectedItem == null) ? "" : cboUnidad.SelectedItem.ToString();
                nueva.Descripcion = txtDescripcion.Text;
                nueva.TieneMinimo = chkMinimo.Checked;
                nueva.StockMinimo = nudMinimo.Value;

                string error;

                try
                {
                    error = negocio.GuardarMateriaPrima(nueva);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("No se pudo guardar la materia prima.\n\n" + ex.Message,
                                    "Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (error != "")
                {
                    lblMensaje.Text = error;
                    return;
                }

                IdGuardado = nueva.IdMateriaPrima;
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