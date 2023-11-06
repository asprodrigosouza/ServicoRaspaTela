using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace RaspaTela.Dados.Models
{
    public class DBContext : DbContext
    {
        public DbSet<MoedaEstrangeira> TB_Moeda_Estrangeira { get; set; }
        public DbSet<LogServico> TB_Log_Servico { get; set; }
        public DbSet<LogErro> TB_Log_Erro { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-VNNKMF1\\SQLEXPRESS;Database=BDRT002;User Id=Rodrigo;Password=Gabriela@001;Integrated Security=SSPI;TrustServerCertificate=True");
            }
        }
    }
}
