using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace LegendsAwaken.Infrastructure
{
    /// <summary>
    /// Representa o contexto do banco de dados para o projeto Legends Awaken.
    /// Respons�vel por mapear as entidades do dom�nio para tabelas no SQLite.
    /// </summary>
    public class LegendsAwakenDbContext : DbContext
    {
        /// <summary>
        /// Construtor que recebe as op��es de configura��o do contexto.
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
        public DbSet<Party> Parties => Set<Party>();
        public DbSet<PartyHero> PartyHeroes => Set<PartyHero>();
        public DbSet<Item> Itens => Set<Item>();
        public DbSet<ItemBonus> ItemBonus => Set<ItemBonus>();
        public DbSet<SlotOcupacao> SlotOcupacoes => Set<SlotOcupacao>();




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

            // Configura construções e trabalhadores como entidades relacionadas à cidade.
            modelBuilder.Entity<Cidade>()
                .HasMany(c => c.Construcoes)
                .WithOne()
                .HasForeignKey("CidadeId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cidade>()
                .HasMany(c => c.Trabalhadores)
                .WithOne()
                .HasForeignKey("CidadeId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Define a chave prim�ria da entidade Usuario como o ID do Discord.
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

            // Party e PartyHero
            modelBuilder.Entity<Party>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Party>()
                .HasMany(p => p.Membros)
                .WithOne(ph => ph.Party)
                .HasForeignKey(ph => ph.PartyId);

            modelBuilder.Entity<PartyHero>()
                .HasKey(ph => new { ph.PartyId, ph.HeroiId });

            modelBuilder.Entity<PartyHero>()
                .HasOne(ph => ph.Heroi)
                .WithMany()
                .HasForeignKey(ph => ph.HeroiId);

            // Item e ItemBonus
            modelBuilder.Entity<Item>()
                .HasMany(i => i.Bonus)
                .WithOne()
                .HasForeignKey(b => b.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemBonus>()
                .HasKey(b => b.Id);

            // HeroiBonusAtributo
            modelBuilder.Entity<HeroiBonusAtributo>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<HeroiBonusAtributo>()
                .HasOne(b => b.Heroi)
                .WithMany(h => h.BonusAtributos)
                .HasForeignKey(b => b.HeroiId)
                .OnDelete(DeleteBehavior.Cascade);

            // SlotOcupacao
            modelBuilder.Entity<SlotOcupacao>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<SlotOcupacao>()
                .HasOne<Construcao>()
                .WithMany()
                .HasForeignKey(s => s.ConstrucaoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
