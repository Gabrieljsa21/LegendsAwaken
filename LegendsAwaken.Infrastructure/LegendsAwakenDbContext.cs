using LegendsAwaken.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LegendsAwaken.Infrastructure
{
    /// <summary>
    /// Representa o contexto do banco de dados para o projeto Legends Awaken.
    /// Responsável por mapear as entidades do domínio para tabelas no SQLite.
    /// </summary>
    public class LegendsAwakenDbContext : DbContext
    {
        /// <summary>
        /// Construtor que recebe as opções de configuração do contexto.
        /// </summary>
        public LegendsAwakenDbContext(DbContextOptions<LegendsAwakenDbContext> options)
            : base(options)
        {
        }

        // DbSets representam as tabelas no banco de dados.
        public DbSet<Heroi> Herois => Set<Heroi>();
        public DbSet<Habilidade> Habilidades => Set<Habilidade>();
        public DbSet<TorreAndar> Andares => Set<TorreAndar>();
        public DbSet<Cidade> Cidades => Set<Cidade>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();

        /// <summary>
        /// Configura o mapeamento das entidades e seus relacionamentos no modelo do EF Core.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura propriedades complexas de Heroi como tipos incorporados (OwnsOne).
            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.Atributos);

            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.Status);

            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.Habilidades, habilidades =>
                {
                    // Define listas internas de habilidades ativas e passivas como coleções embutidas.
                    habilidades.OwnsMany(h => h.Ativas);
                    habilidades.OwnsMany(h => h.Passivas);
                });

            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.Equipamentos);

            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.Treinamento);

            // Configura atributos do inimigo como tipo incorporado.
            modelBuilder.Entity<Inimigo>()
                .OwnsOne(i => i.Atributos);

            // Configura os recursos da cidade como objeto complexo embutido.
            modelBuilder.Entity<Cidade>()
                .OwnsOne(c => c.Recursos);

            // Define a chave primária da entidade Usuario como o ID do Discord.
            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.DiscordId);
        }
    }
}
