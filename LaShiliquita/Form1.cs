using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using LaShiliquita.Datos;

namespace LaShiliquita
{
    // Formulario TEMPORAL solo para probar la conexion. Se reemplaza en la Etapa 2 por el login.
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string resultado = Conexion.ProbarConexion();
            MessageBox.Show(resultado);

            // Prueba de lectura real: cuenta los productos y las tablas nuevas
            try
            {
                using (SqlConnection cn = Conexion.ObtenerConexion())
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM productos", cn);
                    int productos = (int)cmd.ExecuteScalar();

                    SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM movimientos_inventario", cn);
                    int movimientos = (int)cmd2.ExecuteScalar();

                    MessageBox.Show("Productos: " + productos + "\nMovimientos de inventario: " + movimientos);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al leer datos: " + ex.Message);
            }
        }
    }
}