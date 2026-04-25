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
│       ├── ICidadeRepository.cs
│       ├── IHabilidadeRepository.cs
│       ├── IHeroiRepository.cs
│       ├── IPartyRepository.cs
│       ├── ITorreRepository.cs
│       └── IUsuarioRepository.cs
│
├── LegendsAwaken.Application/             # Orquestração de casos de uso
│   ├── Helpers/
│   │   └── NomeGenerator.cs
│   ├── Interfaces/
│   │   ├── IAtributoBonusService.cs
│   │   └── IHabilidadeService.cs
│   └── Services/
│       ├── AtributoBonusService.cs
│       ├── CidadeService.cs
│       ├── CombatService.cs
│       ├── GeracaoDeDadosService.cs
│       ├── HabilidadeService.cs
│       ├── HeroiLevelUpService.cs         # RaridadeConfig, grants de ascensão, bônus raciais
│       ├── HeroiService.cs
│       ├── PartyService.cs
│       ├── RacaService.cs
│       ├── TorreOperacaoConfig.cs         # Config estática: duração, produção por tier, CalcularMaxSlots
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
│   ├── Repositories/
│   │   ├── BiomaRepository.cs
│   │   ├── CidadeRepository.cs
│   │   ├── ContratoRepository.cs
│   │   ├── FragmentoRepository.cs
│   │   ├── HabilidadeRepository.cs
│   │   ├── HeroiConfigRepository.cs
│   │   ├── HeroiDesbloqueadoRepository.cs
│   │   ├── HeroiRepository.cs
│   │   ├── ItemRepository.cs
│   │   ├── PartyRepository.cs
│   │   ├── SlotOcupacaoRepository.cs
│   │   ├── TorreBoosterRepository.cs
│   │   ├── TorreExploracaoRepository.cs
│   │   ├── TorreOperacaoRepository.cs
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
│   │   ├── ArenaCommand.cs
│   │   ├── BiomaCommand.cs                # 4 handlers: lista + detalhe de bioma
│   │   ├── CidadeCommand.cs
│   │   ├── ColecaoCommand.cs
│   │   ├── CombatCommand.cs
│   │   ├── GruposCommand.cs
│   │   ├── HeroisCommand.cs
│   │   ├── InvocarCommand.cs
│   │   ├── ListarHeroisCommand.cs
│   │   ├── SubirAndarCommand.cs
│   │   ├── TorreCommand.cs               # Handlers de Modo Operação board-based
│   │   ├── TreinarCommand.cs
│   │   └── VerHeroiCommand.cs
│   ├── Panels/
│   │   ├── BiomaPanel.cs                  # Seletor de biomas descobertos + barra de progresso
│   │   ├── CidadePanel.cs                 # Agrupamento por node + contador de heróis disponíveis
│   │   ├── ColecaoPanel.cs
│   │   ├── ContratoPanel.cs
│   │   ├── GruposPanel.cs
│   │   ├── TorreExploracaoPanel.cs
│   │   └── TorreModoOperacaoPanel.cs      # Board de andares ativos/concluídos
│   └── Helpers/
│       ├── DiscordIdHelper.cs             # ToGuid via BinaryPrimitives.WriteUInt64LittleEndian
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
