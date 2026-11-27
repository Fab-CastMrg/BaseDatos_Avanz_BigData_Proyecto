using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using LaShiliquita.Entidades;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Presentacion
{
    // Ticket (comprobante interno) de una venta ya guardada. Se puede imprimir.
    public class FrmTicket : Form
    {
        private const int Ancho = 38;   // caracteres por linea

        private Venta venta;
        private decimal vuelto;
        private TextBox txtTicket;

        public FrmTicket(Venta venta, decimal vuelto)
        {
            this.venta = venta;
            this.vuelto = vuelto;
            ConstruirInterfaz();
            txtTicket.Text = ArmarTexto();
            txtTicket.SelectionStart = 0;
            txtTicket.SelectionLength = 0;
        }

        private void ConstruirInterfaz()
        {
            this.Text = "Ticket N° " + venta.IdVenta.ToString("000000");
            this.ClientSize = new Size(390, 640);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Estilo.Fondo;
            this.Padding = new Padding(12);

            // El ticket usa letra de ancho fijo (Consolas) para que las columnas queden alineadas
            txtTicket = new TextBox();
            txtTicket.Multiline = true;
            txtTicket.ReadOnly = true;
            txtTicket.ScrollBars = ScrollBars.Vertical;
            txtTicket.Font = new Font("Consolas", 9.5f);
            txtTicket.BackColor = Color.White;
            txtTicket.ForeColor = Estilo.Texto;
            txtTicket.BorderStyle = BorderStyle.None;
            txtTicket.Dock = DockStyle.Fill;

            Panel pnlBotones = new Panel();
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Height = 52;

            Button btnImprimir = new Button();
            btnImprimir.Text = "Imprimir";
            btnImprimir.Location = new Point(0, 10);
            btnImprimir.Size = new Size(170, 36);
            btnImprimir.FlatStyle = FlatStyle.Flat;
            btnImprimir.FlatAppearance.BorderSize = 0;
            btnImprimir.BackColor = Estilo.Caramelo;
            btnImprimir.ForeColor = Color.White;
            btnImprimir.Font = Estilo.FuenteNegrita(10.5f);
            btnImprimir.Cursor = Cursors.Hand;
            btnImprimir.Click += BtnImprimir_Click;

            Button btnCerrar = new Button();
            btnCerrar.Text = "Cerrar";
            btnCerrar.Location = new Point(182, 10);
            btnCerrar.Size = new Size(184, 36);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 1;
            btnCerrar.FlatAppearance.BorderColor = Estilo.TextoSuave;
            btnCerrar.BackColor = Color.White;
            btnCerrar.ForeColor = Estilo.Texto;
            btnCerrar.Font = Estilo.FuenteNegrita(10.5f);
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Click += BtnCerrar_Click;

            pnlBotones.Controls.Add(btnImprimir);
            pnlBotones.Controls.Add(btnCerrar);

            this.Controls.Add(txtTicket);
            this.Controls.Add(pnlBotones);
            this.CancelButton = btnCerrar;
        }

        // ---------- Texto del ticket ----------

        private string Num(decimal valor)
        {
            return valor.ToString("0.00", CultureInfo.InvariantCulture);
        }

        // Texto a la izquierda y valor a la derecha, en una linea de ancho fijo
        private string Linea(string izquierda, string derecha)
        {
            int espacios = Ancho - izquierda.Length - derecha.Length;
            if (espacios < 1)
            {
                espacios = 1;
            }
            return izquierda + new string(' ', espacios) + derecha;
        }

        private string Centrar(string texto)
        {
            if (texto.Length >= Ancho)
            {
                return texto;
            }
            return new string(' ', (Ancho - texto.Length) / 2) + texto;
        }

        private string ArmarTexto()
        {
            StringBuilder sb = new StringBuilder();
            string separador = new string('-', Ancho);

            sb.AppendLine(Centrar("LA SHILIQUITA"));
            sb.AppendLine(Centrar("Chocolatería artesanal"));
            sb.AppendLine(Centrar(venta.NombreSede));
            sb.AppendLine(separador);
            sb.AppendLine("Ticket N° " + venta.IdVenta.ToString("000000"));
            sb.AppendLine("Fecha:    " + venta.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
            sb.AppendLine("Vendedor: " + venta.NombreVendedor);
            sb.AppendLine(separador);

            foreach (ItemCarrito item in venta.Items)
            {
                string nombre = item.Nombre;
                if (nombre.Length > Ancho)
                {
                    nombre = nombre.Substring(0, Ancho);
                }
                sb.AppendLine(nombre);
                sb.AppendLine(Linea("   " + item.Cantidad + " x " + Num(item.PrecioUnitario), Num(item.Subtotal)));
            }

            sb.AppendLine(separador);
            sb.AppendLine(Linea("Subtotal", Num(venta.Subtotal)));

            if (venta.Descuento > 0)
            {
                sb.AppendLine(Linea("Descuento", "-" + Num(venta.Descuento)));
            }

            sb.AppendLine(Linea("Base imponible", Num(venta.BaseImponible)));
            sb.AppendLine(Linea("IGV incluido", Num(venta.Igv)));
            sb.AppendLine(Linea("TOTAL S/", Num(venta.Total)));
            sb.AppendLine(separador);

            foreach (PagoVenta pago in venta.Pagos)
            {
                sb.AppendLine(Linea(pago.MetodoPago, Num(pago.Monto)));
                if (!string.IsNullOrEmpty(pago.NumeroOperacion))
                {
                    sb.AppendLine("   Operación: " + pago.NumeroOperacion);
                }
            }

            if (vuelto > 0)
            {
                sb.AppendLine(Linea("Vuelto", Num(vuelto)));
            }

            sb.AppendLine(separador);
            sb.AppendLine(Centrar("Comprobante interno"));
            sb.AppendLine(Centrar("Gracias por su compra"));

            return sb.ToString();
        }

        // ---------- Botones e impresion ----------

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            PrintDocument documento = new PrintDocument();
            documento.DocumentName = "Ticket " + venta.IdVenta;
            documento.PrintPage += Documento_PrintPage;

            PrintDialog dialogo = new PrintDialog();
            dialogo.Document = documento;

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    documento.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo imprimir: " + ex.Message, "Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Documento_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Font fuente = new Font("Consolas", 9))
            {
                e.Graphics.DrawString(txtTicket.Text, fuente, Brushes.Black, e.MarginBounds.Left, e.MarginBounds.Top);
            }
            e.HasMorePages = false;
        }
    }
}