using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Drawing;
using System.Windows.Forms;
using LaShiliquita.Utilidades;

using System;
using System.Drawing;
using System.Windows.Forms;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Ventana principal: menu lateral segun el rol, encabezado con el usuario y zona de contenido.
    public class FrmPrincipal : Form
    {
        // Lo lee Program.cs: true si el usuario eligio "Cerrar sesión" (vuelve al login)
        public bool CerroSesion = false;

        private Panel pnlMenu;
        private FlowLayoutPanel flpMenu;
        private Panel pnlEncabezado;
        private Panel pnlContenido;
        private Label lblTitulo;
        private Label lblUsuario;
        private Button btnCerrarSesion;
        private Button botonActivo;

        public FrmPrincipal()
        {
            ConstruirInterfaz();
            this.Load += FrmPrincipal_Load;
        }

        private void ConstruirInterfaz()
        {
            this.Text = "La Shiliquita - Sistema de gestión";
            this.ClientSize = new Size(1200, 720);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Estilo.Fondo;
            this.Font = Estilo.Fuente(10);

            // Zona de contenido (Fill)
            pnlContenido = new Panel();
            pnlContenido.Dock = DockStyle.Fill;
            pnlContenido.Padding = new Padding(24);

            // Encabezado (Top)
            pnlEncabezado = new Panel();
            pnlEncabezado.Dock = DockStyle.Top;
            pnlEncabezado.Height = 64;
            pnlEncabezado.BackColor = Color.White;

            lblTitulo = new Label();
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = Estilo.FuenteNegrita(15);
            lblTitulo.ForeColor = Estilo.Texto;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblTitulo.Padding = new Padding(24, 0, 0, 0);

            Panel pnlUsuario = new Panel();
            pnlUsuario.Dock = DockStyle.Right;
            pnlUsuario.Width = 480;

            lblUsuario = new Label();
            lblUsuario.Dock = DockStyle.Fill;
            lblUsuario.Font = Estilo.Fuente(9.5f);
            lblUsuario.ForeColor = Estilo.TextoSuave;
            lblUsuario.TextAlign = ContentAlignment.MiddleRight;

            btnCerrarSesion = new Button();
            btnCerrarSesion.Text = "Cerrar sesión";
            btnCerrarSesion.Dock = DockStyle.Right;
            btnCerrarSesion.Width = 130;
            btnCerrarSesion.FlatStyle = FlatStyle.Flat;
            btnCerrarSesion.FlatAppearance.BorderSize = 0;
            btnCerrarSesion.ForeColor = Estilo.Caramelo;
            btnCerrarSesion.Font = Estilo.FuenteNegrita(10);
            btnCerrarSesion.Cursor = Cursors.Hand;
            btnCerrarSesion.Click += BtnCerrarSesion_Click;

            pnlUsuario.Controls.Add(lblUsuario);
            pnlUsuario.Controls.Add(btnCerrarSesion);

            pnlEncabezado.Controls.Add(lblTitulo);
            pnlEncabezado.Controls.Add(pnlUsuario);

            // Menu lateral (Left)
            pnlMenu = new Panel();
            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Width = 240;
            pnlMenu.BackColor = Estilo.Cafe;

            flpMenu = new FlowLayoutPanel();
            flpMenu.Dock = DockStyle.Fill;
            flpMenu.FlowDirection = FlowDirection.TopDown;
            flpMenu.WrapContents = false;
            flpMenu.BackColor = Estilo.Cafe;

            Label lblMarca = new Label();
            lblMarca.Text = "La Shiliquita";
            lblMarca.Dock = DockStyle.Top;
            lblMarca.Height = 80;
            lblMarca.Font = Estilo.FuenteNegrita(16);
            lblMarca.ForeColor = Color.White;
            lblMarca.TextAlign = ContentAlignment.MiddleCenter;

            pnlMenu.Controls.Add(flpMenu);
            pnlMenu.Controls.Add(lblMarca);

            // Los controles con Dock se agregan en este orden: Fill, Top y Left
            this.Controls.Add(pnlContenido);
            this.Controls.Add(pnlEncabezado);
            this.Controls.Add(pnlMenu);
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            // Datos de la sesion en el encabezado
            lblUsuario.Text = Sesion.UsuarioActual.NombreCompleto + "\r\n" +
                              Sesion.UsuarioActual.NombreRol + "  |  " + Sesion.SedeActual.Nombre;

            // Solo se muestran los modulos a los que el rol tiene acceso
            AgregarBotonMenu("Inicio", "Dashboard");
            AgregarBotonMenu("Ventas", "Ventas");
            AgregarBotonMenu("Inventario", "Inventario");
            AgregarBotonMenu("Materia prima y producción", "MateriaPrima");
            AgregarBotonMenu("Proveedores", "Proveedores");
            AgregarBotonMenu("Caja", "Caja");
            AgregarBotonMenu("Reportes", "Reportes");
            AgregarBotonMenu("Usuarios", "Usuarios");

            if (flpMenu.Controls.Count > 0)
            {
                MostrarModulo((Button)flpMenu.Controls[0]);
            }
        }

        private void AgregarBotonMenu(string texto, string modulo)
        {
            if (!Sesion.TieneAcceso(modulo))
            {
                return;
            }

            Button boton = new Button();
            boton.Text = texto;
            boton.Tag = modulo;
            boton.Size = new Size(240, 48);
            boton.Margin = new Padding(0);
            boton.Padding = new Padding(24, 0, 0, 0);
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Estilo.CafeClaro;
            boton.BackColor = Estilo.Cafe;
            boton.ForeColor = Color.White;
            boton.Font = Estilo.Fuente(10.5f);
            boton.Cursor = Cursors.Hand;
            boton.Click += BotonMenu_Click;

            flpMenu.Controls.Add(boton);
        }

        private void BotonMenu_Click(object sender, EventArgs e)
        {
            MostrarModulo((Button)sender);
        }

        private void MostrarModulo(Button boton)
        {
            if (botonActivo != null)
            {
                botonActivo.BackColor = Estilo.Cafe;
            }
            botonActivo = boton;
            botonActivo.BackColor = Estilo.CafeClaro;

            string modulo = boton.Tag.ToString();
            lblTitulo.Text = boton.Text;

            pnlContenido.Controls.Clear();

            // Modulos ya desarrollados
            if (modulo == "Usuarios")
            {
                UcUsuarios usuarios = new UcUsuarios();
                usuarios.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(usuarios);
                return;
            }

            // El resto muestra un aviso hasta que se desarrolle su etapa
            Label aviso = new Label();
            aviso.Dock = DockStyle.Top;
            aviso.Height = 60;
            aviso.Font = Estilo.Fuente(11);
            aviso.ForeColor = Estilo.TextoSuave;

            switch (modulo)
            {
                case "Dashboard":
                    aviso.Text = "Bienvenido, " + Sesion.UsuarioActual.Nombres + ". El resumen del día se agrega en la Etapa 7.";
                    break;
                case "Ventas":
                    aviso.Text = "Este módulo se desarrolla en la Etapa 3.";
                    break;
                case "Inventario":
                    aviso.Text = "Este módulo se desarrolla en la Etapa 4.";
                    break;
                case "MateriaPrima":
                case "Proveedores":
                    aviso.Text = "Este módulo se desarrolla en la Etapa 5.";
                    break;
                case "Caja":
                    aviso.Text = "Este módulo se desarrolla en la Etapa 6.";
                    break;
                case "Reportes":
                    aviso.Text = "Este módulo se desarrolla en la Etapa 7.";
                    break;
            }

            pnlContenido.Controls.Add(aviso);
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            Sesion.Cerrar();
            CerroSesion = true;
            this.Close();
        }
    }
}