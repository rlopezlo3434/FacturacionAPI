namespace FacturacionAPI.Models.DTOs
{
    public class EnableProductDto
    {
        public int ProductDefinitionId { get; set; }  // Id del producto genérico
        public int EstablishmentId { get; set; }      // Id de la tienda
    }
}
