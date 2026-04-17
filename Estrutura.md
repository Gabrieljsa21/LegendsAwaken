# Estrutura de Pastas — Legends Awaken

```
LegendsAwaken/
├── LegendsAwaken.sln
├── README.md
├── TODO.md
├── ROADMAP.md
├── GDD.md
├── Estrutura.md
├── ANALISE.md
├── .gitignore
│
├── LegendsAwaken.Domain/                  # Regras de negócio — sem dependências externas
│   ├── Entities/
│   │   ├── AtributosBase.cs               # Value object com os 5 atributos base
│   │   ├── Cidade.cs
│   │   ├── Equipamentos.cs
│   │   ├── Heroi.cs
│   │   ├── HeroiHabilidade.cs
│   │   ├── Party.cs
│   │   ├── StatusCombate.cs
│   │   ├── TorreAndar.cs
│   │   ├── Treinamento.cs
│   │   ├── Usuario.cs
│   │   ├── Auxiliares/
│   │   │   └── HeroiAuxiliares.cs         # HeroiBonusAtributo, HeroiTag, HeroiVinculo, HeroiAfinidadeElemental
│   │   ├── Banner/
│   │   │   ├── BannerConfiguracao.cs
│   │   │   ├── BannerHistorico.cs
│   │   │   ├── BannerProgresso.cs
│   │   │   └── RacaChance.cs
│   │   └── Combate/
│   │       ├── CombatEncounter.cs
│   │       └── Combatente.cs
│   ├── Enum/
│   │   └── Enums.cs                       # Raridade, Raca, Profissao, Atributo, TipoAndar,
│   │                                      # TipoHabilidade, OrigemBonusAtributo, Elemento, etc.
│   ├── Extensions/
│   │   ├── RacaExtensions.cs
│   │   ├── RaridadeExtensions.cs
│   │   └── StatusCombateExtensions.cs
│   ├── Factories/
│   │   └── HeroiFactory.cs
│   └── Interfaces/
│       ├── IBannerHistoricoRepository.cs
│       ├── ICidadeRepository.cs
│       ├── IHabilidadeRepository.cs
│       ├── IHeroiRepository.cs
│       ├── IPartyRepository.cs
│       ├── ITorreRepository.cs
│       └── IUsuarioRepository.cs
│
├── LegendsAwaken.Application/             # Orquestração de casos de uso
│   ├── DTOs/
│   │   └── GachaResultadoDTO.cs
│   ├── Helpers/
│   │   ├── BannerDinamico.cs
│   │   └── NomeGenerator.cs
│   ├── Interfaces/
│   │   ├── IAtributoBonusService.cs
│   │   └── IHabilidadeService.cs
│   └── Services/
│       ├── AtributoBonusService.cs
│       ├── BannerHistoricoService.cs
│       ├── BannerService.cs
│       ├── CidadeService.cs
│       ├── CombatService.cs
│       ├── GachaService.cs                # Gacha com soft-pity cúbico; RaridadeConfig (SOLID)
│       ├── GeracaoDeDadosService.cs
│       ├── HabilidadeService.cs
│       ├── HeroiLevelUpService.cs         # RaridadeConfig, grants de ascensão, bônus raciais
│       ├── HeroiService.cs
│       ├── PartyService.cs
│       ├── RacaService.cs
│       ├── TorreService.cs
│       ├── TreinamentoService.cs
│       └── UsuarioService.cs
│
├── LegendsAwaken.Infrastructure/          # Persistência de dados (EF Core + SQLite)
│   ├── LegendsAwakenDbContext.cs
│   ├── LegendsAwakenDbContextFactory.cs
│   ├── Migrations/
│   │   ├── 20250724221226_InitialCreate.cs
│   │   ├── 20260411035328_CidadeRefactor.cs
│   │   └── LegendsAwakenDbContextModelSnapshot.cs
│   ├── Providers/
│   │   └── BannerConfiguracoesProvider.cs
│   ├── Repositories/
│   │   ├── BannerHistoricoRepository.cs
│   │   ├── CidadeRepository.cs
│   │   ├── HabilidadeRepository.cs
│   │   ├── HeroiRepository.cs
│   │   ├── PartyRepository.cs
│   │   ├── TorreRepository.cs
│   │   └── UsuarioRepository.cs
│   └── SeedData/
│       └── HabilidadesSeed.cs
│
├── LegendsAwaken.Bot/                     # Interface Discord — entry point
│   ├── Program.cs                         # Startup, wiring de DI
│   ├── CommandHandler.cs                  # Roteamento de interações Discord
│   ├── BotConfig.cs
│   ├── appsettings.json
│   ├── Commands/
│   │   ├── BannerCommand.cs
│   │   ├── CidadeCommand.cs
│   │   ├── CombatCommand.cs
│   │   ├── InvocarCommand.cs
│   │   ├── ListarHeroisCommand.cs
│   │   ├── SubirAndarCommand.cs
│   │   ├── TreinarCommand.cs
│   │   └── VerHeroiCommand.cs
│   └── Helpers/
│       └── EmbedHelper.cs
│
├── LegendsAwaken.Data/                    # Dados estáticos em JSON
│   ├── habilidades.json
│   ├── herois_base.json
│   └── classes.json
│
└── LegendsAwaken.Tests/                   # Testes automatizados (xUnit)
    └── UnitTest1.cs                       # Placeholder — suite a implementar
```
