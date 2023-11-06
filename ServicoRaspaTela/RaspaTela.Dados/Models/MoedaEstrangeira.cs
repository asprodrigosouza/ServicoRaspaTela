using System;


namespace RaspaTela.Dados.Models
{
    public class MoedaEstrangeira
    {
        public int ID  { get; set; }
        public string Descricao { get; set; }
        public string Valor { get; set; }
        public DateTime Data_Inclusao { get; set; }
    }
}
