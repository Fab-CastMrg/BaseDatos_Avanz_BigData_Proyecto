namespace LaShiliquita.Entidades
{
    // Producto del catalogo de ventas, con su stock en la sede de la sesion
    public class ProductoVenta
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Presentacion { get; set; }
        public int IdCategoria { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Stock { get; set; }   // suma de inventario.cantidad de todos los lotes en la sede
    }
}