using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using LaShiliquita.Entidades;
using LaShiliquita.Negocio;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Pantalla de inicio de sesion. La interfaz se construye por codigo (no usa el diseñador).
    public class FrmLogin : Form
    {
        private UsuarioNegocio negocio = new UsuarioNegocio();

        private TextBox txtUsuario;
        private TextBox txtContrasena;
        private ComboBox cboSede;
        private Button btnIngresar;
        private Label lblError;

        public FrmLogin()
        {
            ConstruirInterfaz();
            this.Load += FrmLogin_Load;
        }

        private void ConstruirInterfaz()
        {
            this.Text = "La Shiliquita - Iniciar sesión";
            this.ClientSize = new Size(780, 460);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = Estilo.Fuente(10);

            // Panel derecho (formulario)
            Panel pnlDerecha = new Panel();
            pnlDerecha.Dock = DockStyle.Fill;
            pnlDerecha.BackColor = Color.White;

            Label lblTitulo = new Label();
            lblTitulo.Text = "Iniciar sesión";
            lblTitulo.Font = Estilo.FuenteNegrita(20);
            lblTitulo.ForeColor = Estilo.Texto;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(50, 50);

            Label lblUsuario = CrearEtiqueta("Usuario", 50, 115);
            txtUsuario = CrearCaja(50, 140);

            Label lblContrasena = CrearEtiqueta("Contraseña", 50, 185);
            txtContrasena = CrearCaja(50, 210);
            txtContrasena.UseSystemPasswordChar = true;

            Label lblSede = CrearEtiqueta("Sede de trabajo", 50, 255);
            cboSede = new ComboBox();
            cboSede.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSede.Font = Estilo.Fuente(11);
            cboSede.Location = new Point(50, 280);
            cboSede.Width = 380;

            lblError = new Label();
            lblError.ForeColor = Estilo.Error;
            lblError.Location = new Point(50, 322);
            lblError.Size = new Size(380, 22);

            btnIngresar = new Button();
            btnIngresar.Text = "Ingresar";
            btnIngresar.Font = Estilo.FuenteNegrita(11);
            btnIngresar.BackColor = Estilo.Caramelo;
            btnIngresar.ForeColor = Color.White;
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.Cursor = Cursors.Hand;
            btnIngresar.Location = new Point(50, 355);
            btnIngresar.Size = new Size(380, 42);
            btnIngresar.Click += BtnIngresar_Click;

            pnlDerecha.Controls.Add(lblTitulo);
            pnlDerecha.Controls.Add(lblUsuario);
            pnlDerecha.Controls.Add(txtUsuario);
            pnlDerecha.Controls.Add(lblContrasena);
            pnlDerecha.Controls.Add(txtContrasena);
            pnlDerecha.Controls.Add(lblSede);
            pnlDerecha.Controls.Add(cboSede);
            pnlDerecha.Controls.Add(lblError);
            pnlDerecha.Controls.Add(btnIngresar);

            // Panel izquierdo (marca)
            Panel pnlMarca = new Panel();
            pnlMarca.Dock = DockStyle.Left;
            pnlMarca.Width = 300;
            pnlMarca.BackColor = Estilo.Cafe;

            Label lblMarca = new Label();
            lblMarca.Text = "La Shiliquita";
            lblMarca.Font = Estilo.FuenteNegrita(24);
            lblMarca.ForeColor = Color.White;
            lblMarca.AutoSize = true;
            lblMarca.Location = new Point(30, 160);

            Label lblLema = new Label();
            lblLema.Text = "Sistema de gestión\r\nCajamarca, Perú";
            lblLema.Font = Estilo.Fuente(11);
            lblLema.ForeColor = Color.FromArgb(215, 190, 170);
            lblLema.AutoSize = true;
            lblLema.Location = new Point(33, 215);

            pnlMarca.Controls.Add(lblMarca);
            pnlMarca.Controls.Add(lblLema);

            // El control con Dock Fill se agrega primero
            this.Controls.Add(pnlDerecha);
            this.Controls.Add(pnlMarca);

            this.AcceptButton = btnIngresar;   // Enter = Ingresar
        }

        private Label CrearEtiqueta(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.ForeColor = Estilo.TextoSuave;
            etiqueta.AutoSize = true;
            etiqueta.Location = new Point(x, y);
            return etiqueta;
        }

        private TextBox CrearCaja(int x, int y)
        {
            TextBox caja = new TextBox();
            caja.Font = Estilo.Fuente(11);
            caja.Location = new Point(x, y);
            caja.Width = 380;
            return caja;
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            try
            {
                List<Sede> sedes = negocio.ObtenerSedes();
                cboSede.DataSource = sedes;
                cboSede.DisplayMember = "Nombre";
            }
            catch (SqlException ex)
            {
                lblError.Text = "No se pudo conectar con la base de datos.";
                btnIngresar.Enabled = false;
                MessageBox.Show("No se pudo conectar con la base de datos:\r\n" + ex.Message,
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (txtUsuario.Text.Trim() == "")
            {
                lblError.Text = "Ingrese su usuario.";
                txtUsuario.Focus();
                return;
            }

            if (txtContrasena.Text == "")
            {
                lblError.Text = "Ingrese su contraseña.";
                txtContrasena.Focus();
                return;
            }

            if (cboSede.SelectedItem == null)
            {
                lblError.Text = "Seleccione la sede de trabajo.";
                return;
            }

            try
            {
                Usuario usuario = negocio.Autenticar(txtUsuario.Text, txtContrasena.Text);

                if (usuario == null)
                {
                    lblError.Text = "Usuario o contraseña incorrectos.";
                    txtContrasena.Clear();
                    txtContrasena.Focus();
                    return;
                }

                Sesion.UsuarioActual = usuario;
                Sesion.SedeActual = (Sede)cboSede.SelectedItem;
                this.DialogResult = DialogResult.OK;   // cierra el formulario
            }
            catch (SqlException ex)
            {
                lblError.Text = "Error de base de datos: " + ex.Message;
            }
        }
    }
}