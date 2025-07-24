using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Banner;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
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
        public DbSet<HeroiHabilidade> HeroiHabilidades => Set<HeroiHabilidade>();
        public DbSet<TorreAndar> Andares => Set<TorreAndar>();
        public DbSet<Cidade> Cidades => Set<Cidade>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Inimigo> Inimigo => Set<Inimigo>();
        public DbSet<HeroiAfinidadeElemental> HeroisAfinidades => Set<HeroiAfinidadeElemental>();
        public DbSet<HeroiVinculo> HeroisVinculos => Set<HeroiVinculo>();
        public DbSet<HeroiTag> HeroisTags => Set<HeroiTag>();
        public DbSet<BannerHistorico> BannerHistorico => Set<BannerHistorico>();
        public DbSet<BannerProgresso> BannerProgressos { get; set; }



        /// <summary>
        /// Configura o mapeamento das entidades e seus relacionamentos no modelo do EF Core.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura propriedades complexas de Heroi como tipos incorporados (OwnsOne).
            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.AtributosBase);

            modelBuilder.Entity<Heroi>()
                .OwnsOne(h => h.Status);

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
                .HasKey(u => u.Id);

            // HeroiAfinidadeElemental
            modelBuilder.Entity<HeroiAfinidadeElemental>()
                .HasKey(h => new { h.HeroiId, h.Elemento });

            modelBuilder.Entity<HeroiAfinidadeElemental>()
                .HasOne(h => h.Heroi)
                .WithMany(h => h.AfinidadeElemental)
                .HasForeignKey(h => h.HeroiId);

            // HeroiTag
            modelBuilder.Entity<HeroiTag>()
                .HasKey(h => new { h.HeroiId, h.Tag });

            modelBuilder.Entity<HeroiTag>()
                .HasOne(h => h.Heroi)
                .WithMany(h => h.Tags)
                .HasForeignKey(h => h.HeroiId);


            // HeroiVinculo
            modelBuilder.Entity<HeroiVinculo>()
                .HasKey(h => new { h.HeroiId, h.VinculadoId });

            modelBuilder.Entity<HeroiVinculo>()
                .HasOne(h => h.Heroi)
                .WithMany(h => h.VinculosHeroicos)
                .HasForeignKey(h => h.HeroiId);

            modelBuilder.Entity<BannerProgresso>()
                .HasKey(bp => new { bp.UsuarioId, bp.BannerId });

            // HeroiHabilidade (chave composta + relacionamentos)
            modelBuilder.Entity<HeroiHabilidade>()
                .HasKey(hh => new { hh.HeroiId, hh.HabilidadeId });

            modelBuilder.Entity<HeroiHabilidade>()
                .HasOne(hh => hh.Heroi)
                .WithMany(h => h.Habilidades)
                .HasForeignKey(hh => hh.HeroiId);

            modelBuilder.Entity<HeroiHabilidade>()
                .HasOne(hh => hh.Habilidade)
                .WithMany() 
                .HasForeignKey(hh => hh.HabilidadeId);

            // HabilidadeBonusAtributos (relacionamento com Habilidade)
            modelBuilder.Entity<HabilidadeBonusAtributos>()
                .HasKey(hba => new { hba.HabilidadeId, hba.Atributo });

            modelBuilder.Entity<HabilidadeBonusAtributos>()
                .HasOne(hba => hba.Habilidade)
                .WithMany(h => h.HabilidadeBonusAtributos)
                .HasForeignKey(hba => hba.HabilidadeId);

            modelBuilder.Entity<Heroi>(builder =>
            {
                builder.OwnsOne(h => h.AtributosBase);
                builder.OwnsOne(h => h.AtributosDistribuidos);
                builder.OwnsOne(h => h.Status);
                builder.OwnsOne(h => h.Equipamentos);
            });
        }
    }
}
