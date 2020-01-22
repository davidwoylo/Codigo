namespace CFC_Negocio.DTO.Ext
{
    public class PaginacaoConfigDTO
    {
        public int? page { get; set; }
        public int? size { get; set; }
        public string sort { get; set; }
        public string order { get; set; }
    }
}
