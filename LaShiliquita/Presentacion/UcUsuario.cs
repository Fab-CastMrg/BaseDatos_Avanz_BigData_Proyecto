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
    // Administracion de usuarios: lista a la izquierda y formulario a la derecha.
    // La interfaz se construye por codigo (no usa el diseñador).
    public class UcUsuarios : UserControl
    {
        private UsuarioNegocio negocio = new UsuarioNegocio();
        private List<Usuario> usuarios;
        private Usuario usuarioSeleccionado = null;   // null = se esta creando un usuario nuevo
        private bool cargando = false;

        private DataGridView dgv;
        private TextBox txtNombres;
        private TextBox txtApellidos;
        private TextBox txtDni;
        private TextBox txtTelefono;
        private TextBox txtUsuario;
        private TextBox txtContrasena;
        private Label lblContrasena;
        private ComboBox cboRol;
        private Label lblMensaje;
        private Button btnGuardar;
        private Button btnNuevo;
        private Button btnEstado;

        public UcUsuarios()
        {
            ConstruirInterfaz();
            this.Load += UcUsuarios_Load;
        }

        private void ConstruirInterfaz()
        {
            this.BackColor = Estilo.Fondo;

            // ---------- Tabla de usuarios (Fill) ----------
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.AutoGenerateColumns = false;
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
            dgv.RowTemplate.Height = 32;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Estilo.Cafe;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Estilo.Cafe;
            dgv.DefaultCellStyle.SelectionBackColor = Estilo.Caramelo;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.Columns.Add(CrearColumna("Usuario", "NombreUsuario", 150));
            DataGridViewTextBoxColumn colNombre = CrearColumna("Nombre", "NombreCompleto", 100);
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv.Columns.Add(colNombre);
            dgv.Columns.Add(CrearColumna("Rol", "NombreRol", 180));
            dgv.Columns.Add(CrearColumna("Estado", "EstadoTexto", 110));

            dgv.SelectionChanged += Dgv_SelectionChanged;
            dgv.CellFormatting += Dgv_CellFormatting;

            // ---------- Formulario (Right) ----------
            Panel pnlFormulario = new Panel();
            pnlFormulario.Dock = DockStyle.Right;
            pnlFormulario.Width = 340;
            pnlFormulario.BackColor = Color.White;

            Label lblTitulo = new Label();
            lblTitulo.Text = "Datos del usuario";
            lblTitulo.Font = Estilo.FuenteNegrita(12);
            lblTitulo.ForeColor = Estilo.Texto;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(20, 14);

            Label lblNombres = CrearEtiqueta("Nombres", 20, 48);
            txtNombres = CrearCaja(20, 69, 300, 100);

            Label lblApellidos = CrearEtiqueta("Apellidos", 20, 100);
            txtApellidos = CrearCaja(20, 121, 300, 100);

            Label lblDni = CrearEtiqueta("DNI (opcional)", 20, 152);
            txtDni = CrearCaja(20, 173, 140, 8);

            Label lblTelefono = CrearEtiqueta("Teléfono (opcional)", 180, 152);
            txtTelefono = CrearCaja(180, 173, 140, 15);

            Label lblUsuario = CrearEtiqueta("Usuario", 20, 204);
            txtUsuario = CrearCaja(20, 225, 300, 50);

            lblContrasena = CrearEtiqueta("Contraseña", 20, 256);
            txtContrasena = CrearCaja(20, 277, 300, 100);
            txtContrasena.UseSystemPasswordChar = true;

            Label lblRol = CrearEtiqueta("Rol", 20, 308);
            cboRol = new ComboBox();
            cboRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRol.Font = Estilo.Fuente(10.5f);
            cboRol.Location = new Point(20, 329);
            cboRol.Width = 300;

            lblMensaje = new Label();
            lblMensaje.Location = new Point(20, 366);
            lblMensaje.Size = new Size(300, 40);

            btnGuardar = CrearBoton("Guardar", 20, 410, Estilo.Caramelo, Color.White);
            btnGuardar.Click += BtnGuardar_Click;

            btnNuevo = CrearBoton("Nuevo usuario", 20, 456, Color.White, Estilo.Texto);
            btnNuevo.FlatAppearance.BorderSize = 1;
            btnNuevo.FlatAppearance.BorderColor = Estilo.TextoSuave;
            btnNuevo.Click += BtnNuevo_Click;

            btnEstado = CrearBoton("Desactivar usuario", 20, 502, Color.White, Estilo.Error);
            btnEstado.FlatAppearance.BorderSize = 1;
            btnEstado.FlatAppearance.BorderColor = Estilo.Error;
            btnEstado.Click += BtnEstado_Click;

            pnlFormulario.Controls.Add(lblTitulo);
            pnlFormulario.Controls.Add(lblNombres);
            pnlFormulario.Controls.Add(txtNombres);
            pnlFormulario.Controls.Add(lblApellidos);
            pnlFormulario.Controls.Add(txtApellidos);
            pnlFormulario.Controls.Add(lblDni);
            pnlFormulario.Controls.Add(txtDni);
            pnlFormulario.Controls.Add(lblTelefono);
            pnlFormulario.Controls.Add(txtTelefono);
            pnlFormulario.Controls.Add(lblUsuario);
            pnlFormulario.Controls.Add(txtUsuario);
            pnlFormulario.Controls.Add(lblContrasena);
            pnlFormulario.Controls.Add(txtContrasena);
            pnlFormulario.Controls.Add(lblRol);
            pnlFormulario.Controls.Add(cboRol);
            pnlFormulario.Controls.Add(lblMensaje);
            pnlFormulario.Controls.Add(btnGuardar);
            pnlFormulario.Controls.Add(btnNuevo);
            pnlFormulario.Controls.Add(btnEstado);

            // Espacio entre la tabla y el formulario
            Panel separador = new Panel();
            separador.Dock = DockStyle.Right;
            separador.Width = 16;
            separador.BackColor = Estilo.Fondo;

            // El control con Dock Fill se agrega primero
            this.Controls.Add(dgv);
            this.Controls.Add(separador);
            this.Controls.Add(pnlFormulario);
        }

        private DataGridViewTextBoxColumn CrearColumna(string titulo, string propiedad, int ancho)
        {
            DataGridViewTextBoxColumn columna = new DataGridViewTextBoxColumn();
            columna.HeaderText = titulo;
            columna.DataPropertyName = propiedad;
            columna.Width = ancho;
            return columna;
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

        private TextBox CrearCaja(int x, int y, int ancho, int maxLongitud)
        {
            TextBox caja = new TextBox();
            caja.Font = Estilo.Fuente(10.5f);
            caja.Location = new Point(x, y);
            caja.Width = ancho;
            caja.MaxLength = maxLongitud;
            return caja;
        }

        private Button CrearBoton(string texto, int x, int y, Color fondo, Color letra)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Size = new Size(300, 38);
            boton.BackColor = fondo;
            boton.ForeColor = letra;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = Estilo.FuenteNegrita(10);
            boton.Cursor = Cursors.Hand;
            return boton;
        }

        // ---------- Carga de datos ----------

        private void UcUsuarios_Load(object sender, EventArgs e)
        {
            try
            {
                cboRol.DataSource = negocio.ListarRoles();
                cboRol.DisplayMember = "NombreRol";
                cboRol.ValueMember = "IdRol";
                CargarUsuarios(0);
            }
            catch (SqlException ex)
            {
                MostrarMensaje("Error de base de datos: " + ex.Message, true);
            }
        }

        // Recarga la tabla y deja seleccionado al usuario con ese id (o el primero si no se encuentra)
        private void CargarUsuarios(int idSeleccionar)
        {
            cargando = true;
            usuarios = negocio.ListarUsuarios();
            dgv.DataSource = null;
            dgv.DataSource = usuarios;
            cargando = false;

            if (dgv.Rows.Count == 0)
            {
                return;
            }

            int fila = 0;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                Usuario u = (Usuario)dgv.Rows[i].DataBoundItem;
                if (u.IdUsuario == idSeleccionar)
                {
                    fila = i;
                    break;
                }
            }

            dgv.CurrentCell = dgv.Rows[fila].Cells[0];
            dgv.Rows[fila].Selected = true;
            MostrarUsuario((Usuario)dgv.Rows[fila].DataBoundItem);
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (cargando || dgv.SelectedRows.Count == 0)
            {
                return;
            }

            Usuario u = dgv.SelectedRows[0].DataBoundItem as Usuario;
            if (u != null)
            {
                MostrarUsuario(u);
            }
        }

        // Los usuarios desactivados se muestran en gris
        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            Usuario u = dgv.Rows[e.RowIndex].DataBoundItem as Usuario;
            if (u != null && !u.Activo)
            {
                e.CellStyle.ForeColor = Estilo.TextoSuave;
            }
        }

        private void MostrarUsuario(Usuario u)
        {
            usuarioSeleccionado = u;

            txtNombres.Text = u.Nombres;
            txtApellidos.Text = u.Apellidos;
            txtDni.Text = u.Dni;
            txtTelefono.Text = u.Telefono;
            txtUsuario.Text = u.NombreUsuario;
            txtContrasena.Clear();
            lblContrasena.Text = "Contraseña (vacía = no cambiar)";
            cboRol.SelectedValue = u.IdRol;

            btnEstado.Enabled = true;
            btnEstado.Text = u.Activo ? "Desactivar usuario" : "Activar usuario";
            lblMensaje.Text = "";
        }

        // ---------- Botones ----------

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            usuarioSeleccionado = null;

            txtNombres.Clear();
            txtApellidos.Clear();
            txtDni.Clear();
            txtTelefono.Clear();
            txtUsuario.Clear();
            txtContrasena.Clear();
            lblContrasena.Text = "Contraseña";
            cboRol.SelectedIndex = -1;
            lblMensaje.Text = "";

            btnEstado.Enabled = false;
            btnEstado.Text = "Desactivar usuario";

            dgv.ClearSelection();
            txtNombres.Focus();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            Usuario u = new Usuario();
            if (usuarioSeleccionado != null)
            {
                u.IdUsuario = usuarioSeleccionado.IdUsuario;
            }
            u.Nombres = txtNombres.Text;
            u.Apellidos = txtApellidos.Text;
            u.Dni = txtDni.Text;
            u.Telefono = txtTelefono.Text;
            u.NombreUsuario = txtUsuario.Text;
            if (cboRol.SelectedValue != null)
            {
                u.IdRol = Convert.ToInt32(cboRol.SelectedValue);
            }

            try
            {
                string mensaje = negocio.GuardarUsuario(u, txtContrasena.Text);

                if (mensaje != "")
                {
                    MostrarMensaje(mensaje, true);
                    return;
                }

                CargarUsuarios(u.IdUsuario);
                MostrarMensaje("Usuario guardado correctamente.", false);
            }
            catch (SqlException ex)
            {
                MostrarMensaje("Error de base de datos: " + ex.Message, true);
            }
        }

        private void BtnEstado_Click(object sender, EventArgs e)
        {
            if (usuarioSeleccionado == null)
            {
                return;
            }

            bool activar = !usuarioSeleccionado.Activo;
            string accion = activar ? "activar" : "desactivar";

            DialogResult respuesta = MessageBox.Show(
                "¿Desea " + accion + " al usuario " + usuarioSeleccionado.NombreUsuario + "?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            try
            {
                string mensaje = negocio.CambiarEstado(usuarioSeleccionado, activar);

                if (mensaje != "")
                {
                    MostrarMensaje(mensaje, true);
                    return;
                }

                CargarUsuarios(usuarioSeleccionado.IdUsuario);
                MostrarMensaje(activar ? "Usuario activado." : "Usuario desactivado.", false);
            }
            catch (SqlException ex)
            {
                MostrarMensaje("Error de base de datos: " + ex.Message, true);
            }
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            lblMensaje.ForeColor = esError ? Estilo.Error : Color.FromArgb(22, 128, 60);
            lblMensaje.Text = texto;
        }
    }
}