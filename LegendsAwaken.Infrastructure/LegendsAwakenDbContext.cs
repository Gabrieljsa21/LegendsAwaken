using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Infrastructure.SeedData;
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
        public DbSet<HeroiPericia> HeroisPericias => Set<HeroiPericia>();

        // Fragmento system
        public DbSet<HeroiConfig> HeroiConfigs => Set<HeroiConfig>();
        public DbSet<Bioma> Biomas => Set<Bioma>();
        public DbSet<BiomHeroPool> BiomHeroPools => Set<BiomHeroPool>();
        public DbSet<HeroiUnlockConfig> HeroiUnlockConfigs => Set<HeroiUnlockConfig>();
        public DbSet<FragmentoProgresso> FragmentosProgresso => Set<FragmentoProgresso>();
        public DbSet<Contrato> Contratos => Set<Contrato>();
        public DbSet<HeroiDesbloqueado> HeroisDesbloqueados => Set<HeroiDesbloqueado>();

        // Torre exploration + checkpoint event system
        public DbSet<TorreExploracao> TorreExploracoes => Set<TorreExploracao>();
        public DbSet<TorreEvento> TorreEventos => Set<TorreEvento>();
        public DbSet<TorreEventoLog> TorreEventoLogs => Set<TorreEventoLog>();
        public DbSet<UsuarioNotificacao> UsuariosNotificacao => Set<UsuarioNotificacao>();




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

            // Inimigo → Bioma FK
            modelBuilder.Entity<Inimigo>()
                .HasOne(i => i.Bioma)
                .WithMany()
                .HasForeignKey(i => i.BiomaId);

            // TorreAndar.Inimigos is managed via raw SQL JSON — exclude from EF
            modelBuilder.Entity<TorreAndar>()
                .Ignore(t => t.Inimigos);

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

            // BiomHeroPool — explicit FK mappings to avoid shadow property conflicts
            modelBuilder.Entity<BiomHeroPool>()
                .HasOne(p => p.Bioma)
                .WithMany(b => b.Pool)
                .HasForeignKey(p => p.BiomeId);

            modelBuilder.Entity<BiomHeroPool>()
                .HasOne(p => p.Heroi)
                .WithMany()
                .HasForeignKey(p => p.HeroiId);

            // HeroiUnlockConfig — PK is HeroiId (1:1 with HeroiConfig); HeroiId is both PK and FK
            modelBuilder.Entity<HeroiUnlockConfig>()
                .HasKey(h => h.HeroiId);

            modelBuilder.Entity<HeroiUnlockConfig>()
                .HasOne(h => h.Heroi)
                .WithMany()
                .HasForeignKey(h => h.HeroiId);

            // HeroiDesbloqueado — composite PK
            modelBuilder.Entity<HeroiDesbloqueado>()
                .HasKey(h => new { h.UsuarioId, h.HeroiId });

            // FragmentoProgresso — unique indexes to enforce logical key per user
            modelBuilder.Entity<FragmentoProgresso>()
                .HasIndex(f => new { f.UsuarioId, f.HeroiId })
                .IsUnique()
                .HasFilter("\"HeroiId\" IS NOT NULL");
            modelBuilder.Entity<FragmentoProgresso>()
                .HasIndex(f => new { f.UsuarioId, f.Arquetipo })
                .IsUnique()
                .HasFilter("\"Arquetipo\" IS NOT NULL");

            // Contrato — unique index: 1 active per type per user
            modelBuilder.Entity<Contrato>()
                .HasIndex(c => new { c.UsuarioId, c.Tipo, c.Ativo })
                .IsUnique()
                .HasFilter("\"Ativo\" = 1");

            // HeroiPericia
            modelBuilder.Entity<HeroiPericia>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<HeroiPericia>()
                .HasOne(p => p.Heroi)
                .WithMany()
                .HasForeignKey(p => p.HeroiId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HeroiPericia>()
                .HasIndex(p => new { p.HeroiId, p.Pericia })
                .IsUnique();

            // TorreEvento FK → TorreExploracao (cascade delete)
            modelBuilder.Entity<TorreEvento>()
                .HasOne(e => e.Exploracao)
                .WithMany()
                .HasForeignKey(e => e.ExploracaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // UsuarioNotificacao — PK is ulong UsuarioId
            modelBuilder.Entity<UsuarioNotificacao>()
                .HasKey(u => u.UsuarioId);

            // TorreExploracao — Version as explicit concurrency token (supplements [ConcurrencyCheck])
            modelBuilder.Entity<TorreExploracao>()
                .Property(e => e.Version)
                .IsConcurrencyToken();

            // Seed data
            modelBuilder.Entity<HeroiConfig>().HasData(FragmentoSeed.HeroiConfigs());
            modelBuilder.Entity<HeroiUnlockConfig>().HasData(FragmentoSeed.UnlockConfigs());
            modelBuilder.Entity<Bioma>().HasData(FragmentoSeed.Biomas());
            modelBuilder.Entity<BiomHeroPool>().HasData(FragmentoSeed.BiomHeroPools());
            modelBuilder.Entity<Bioma>().HasData(InimigoCatalogoSeed.NovoBiomas());
            modelBuilder.Entity<Inimigo>().HasData(InimigoCatalogoSeed.Inimigos());
        }
    }
}
