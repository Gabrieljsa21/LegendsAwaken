# AI_INDEX.md — LegendsAwaken Navigation Reference
<!-- Lookup-only. Read this before opening any file. Update whenever the codebase changes. -->
<!-- Last updated: 2026-04-18 · Phase 3A.3 complete; Sistema de Fragmentos implementado; GachaService/BannerService removidos -->

---

## Projects

| Project | Purpose |
|---|---|
| `LegendsAwaken.Domain` | Entities, enums, interfaces, extensions, factories |
| `LegendsAwaken.Application` | Services, DTOs, helpers |
| `LegendsAwaken.Infrastructure` | EF Core context, repositories, migrations, providers |
| `LegendsAwaken.Bot` | Discord entry point, CommandHandler, slash command classes |
| `LegendsAwaken.Data` | Static JSON seed data (classes, habilidades, herois_base) |
| `LegendsAwaken.Tests` | Unit tests — 39 testes passando (BiomeService, FragmentService, ContractService, RecruitmentService, TorreService) |

---

## Enums — `Domain/Enum/Enums.cs`

| Enum | Values |
|---|---|
| `Atributo` | Forca, Agilidade, Vitalidade, Inteligencia, Percepcao |
| `Raca` | Humano, Bestial, Anao, Elfo, Draconato, Fada |
| `Raridade` | Estrela1=1 … Estrela5=5 |
| `Profissao` | Guerreiro, Arqueiro, Mago, Ladino, Paladino, Clerigo, Agricultor, Pescador, Caçador, Lenhador, Mineiro, Cozinheiro, Ferreiro, Alfaiate, Joalheiro, Alquimista, Construtor, Pesquisador |
| `FuncaoTatica` | Frente, Suporte, Controle, LongoAlcance, Curandeiro |
| `TipoAndar` | Subjugacao, Fuga, Escolta, Defesa, Armadilha, EventoEspecial |
| `TipoResourceNode` | Campo, Floresta, Mina, Prado |
| `TipoPredio` | Fazenda, Serraria, Mina, Forja, Arena, Guilda |
| `SlotTipo` | Responsabilidade, Operacao |
| `Elemento` | Fogo, Água, Terra, Ar, Luz, Trevas, Gelo, Raio, Natureza, Metal |
| `TipoHabilidade` | Combate, Craft, Coleta |
| `OrigemBonusAtributo` | Racial, Profissao, Antecedente, Equipamento, Talento, LevelUp, Outro |
| `SlotEquipamento` | Arma, Armadura, Acessorio |
| `Qualidade` | Comum=1, Bom=2, Raro=3, Excepcional=4, Mestre=5 |
| `TipoFragmento` | Normal (extensível) |
| `TipoUnlock` | Fragmentos, MarcoTorre, CondicaoUnica |
| `TipoContrato` | Arquetipo, Nomeado |
| `TipoEventoAlto` | DescobertaBioma, HeroiIconicoDesbloqueado |
| `TipoReward` | Micro, Medio, Alto |

---

## Entities

### `Heroi` — `Domain/Entities/Heroi.cs`
Fields: `Id`, `UsuarioId`, `Nome`, `Raridade`, `Raca`, `Profissao?`, `Antecedente?`, `Nivel`, `XP`, `AtributosBase`, `AtributosDistribuidos`, `PontosAtributosDisponiveis`, `BonusAtributos` (List<HeroiBonusAtributo>), `Status` (StatusCombate), `Habilidades` (List<HeroiHabilidade>), `Equipamentos`, `Treinamento?`, `Funcao?`, `EstaAtivo`, `ImagemUrl?`, `Confianca` (int, default 0), `Humor` (int, default 50), `Lore?`, `Vitorias`, `Derrotas`, `AndaresConquistados`, `Lealdade`, `Historia?`, `Personalidade?`, `AfinidadeElemental`, `VinculosHeroicos`, `Tags`  
Key method: `ObterAtributosTotais(AtributosBase bonusExterno)` — sums Base + Distribuidos + BonusAtributos + habilidade bonuses + bonusExterno

### `AtributosBase` — `Domain/Entities/AtributosBase.cs`
EF columns: `Forca`, `Agilidade`, `Vitalidade`, `Inteligencia`, `Percepcao`  
API: `Get(Atributo)`, `Set(Atributo, int)`, `operator +`, `Distribute(int total)`, `With(Atributo, int)`, `ToEnumerable()`, `AdicionarPorTipo(Atributo, int)`  
**To add new attribute:** enum value → property → 2 lines in Get/Set. Everything else auto-scales via `Enum.GetValues<Atributo>()`.

### `HeroiBonusAtributo` — `Domain/Entities/Auxiliares/HeroiAuxiliares.cs`
Fields: `Id`, `HeroiId`, `Atributo`, `Valor`, `Origem` (OrigemBonusAtributo), `ItemId?` (Guid — set when Origem=Equipamento)

### `Item` / `ItemBonus` — `Domain/Entities/Item.cs`
`Item`: `Id`, `Nome`, `Slot` (SlotEquipamento), `Qualidade`, `ProprietarioId` (ulong), `EstaEquipado`, `HeroiEquipadoId?`, `Bonus` (List<ItemBonus>)  
`ItemBonus`: `Id`, `ItemId`, `Atributo`, `Valor`

### `Equipamentos` — `Domain/Entities/Equipamentos.cs`
Fields: `Id`, `ArmaId?` (Guid), `ArmaduraId?` (Guid), `AcessorioId?` (Guid)  
Owned by `Heroi` (EF OwnsOne)

### `Cidade` / `Recursos` / `Construcao` / `PersonagemTrabalhador` — `Domain/Entities/Cidade.cs`
`Cidade`: `Id`, `UsuarioId`, `Nome`, `Nivel`, `Populacao`, `CapacidadeMaxima`, `Recursos`, `Construcoes`, `Trabalhadores`, `UltimaColeta`  
`Recursos`: `Comida`, `Madeira`, `Pedra`, `Ouro`, `Erva` + `Adicionar(int qtd, string tipo)`  
`Construcao` gained: `TipoPredio TipoPredio`  
`PersonagemTrabalhador` gained: `TipoResourceNode? ResourceNode` (null = legacy)

### `SlotOcupacao` — `Domain/Entities/SlotOcupacao.cs`
Fields: `Id`, `ConstrucaoId`, `HeroiId`, `SlotTipo`, `PosicaoSlot`  
Tracks which hero occupies which slot in a building.

### `TorreAndar` — `Domain/Entities/TorreAndar.cs`
Fields include: `UsuarioId`, `Numero`, `TipoAndar`, `Concluido`

### `StatusCombate` — `Domain/Entities/StatusCombate.cs`
Fields: `VidaAtual`, `VidaMaxima`, `ManaAtual`, `ManaMaxima`

### `HeroiHabilidade` — `Domain/Entities/HeroiHabilidade.cs`
Fields: `HabilidadeId`, `Habilidade`, `Nivel`, `XPAtual`, `XPMaximo`

### `Usuario` — `Domain/Entities/Usuario.cs`
### `Party` / `PartyHero` — `Domain/Entities/Party.cs`
### `Inimigo` — (in DbContext as `Set<Inimigo>()`, defined in Domain)

### Entidades do Sistema de Fragmentos — `Domain/Entities/Fragmento/`

| Entity | Fields chave |
|---|---|
| `HeroiConfig` | `Id`, `Nome`, `Profissao`, `Raridade`, `Descricao?`, `UnlockConfig` (nav) |
| `HeroiUnlockConfig` | `HeroiId`, `TipoUnlock`, `QuantidadeFragmentos?`, `AndarMarco?` |
| `Bioma` | `Id`, `Nome`, `AndarInicio`, `AndarFim`, `HeroPool` (nav list) |
| `BiomHeroPool` | `BiomId`, `HeroiId`, `Peso`, `Heroi` (nav) |
| `FragmentoProgresso` | `UsuarioId` (Guid), `HeroiId`, `Quantidade`, `UltimaAtualizacao` |
| `Contrato` | `Id`, `UsuarioId`, `TipoContrato`, `Ativo`, `ArquetipoAlvo?` (Profissao?), `HeroiAlvoId?`, `CriadoEm`, `ExpiraEm?` |
| `HeroiDesbloqueado` | `UsuarioId`, `HeroiId`, `DesbloqueadoEm`, `Heroi` (nav) |

### Auxiliares — `Domain/Entities/Auxiliares/HeroiAuxiliares.cs`
`HeroiAfinidadeElemental`, `HeroiVinculo`, `HeroiTag`, `HeroiBonusAtributo`

---

## Services — `Application/Services/`

| Service | Responsibility | Key methods |
|---|---|---|
| `HeroiLevelUpService` | Progression math — XP, caps, grants, racial multipliers | `XpParaProximoNivel(nivel, raridade)`, `AplicarXp(heroi, xp)→int`, `CapParaRaridade(r)`, `CalcularGrantAscensao(nivel, r)`, `ObterAtributosBaseParaRaridade(r)` |
| `HeroiService` | Hero CRUD, skill training, equip items | `CriarHeroiAsync(...)`, `EquiparItemAsync(heroiId, itemId, userId)→string?`, `ObterHeroisPorUsuarioAsync(userId)`, `TreinarHabilidadeAsync(...)` |
| `CraftingService` | Static recipes, resource validation, item creation | `ListarReceitas()`, `CraftarAsync(userId, receitaId)→(Item?, string?)` |
| `CombatService` | Turn-based combat, ATB ordering, damage formula | `IniciarCombate(herois, inimigos)`, `ExecutarRound(enc)`, `CalcularDano(atk, def, skillMult, typeMult)` |
| `TorreService` | Floor progression, XP grant, fragment drops, biome/marco detection | `SubirAndarAsync(userId, heroisParticipantes)→SubirAndarResult` — result inclui `Fragmentos`, `NovoBioma`, `HeroiDesbloqueado`, `RewardPayloads` |
| `CidadeService` | City production, building construction, slot allocation | `CriarCidadeAsync`, `ColetarProducaoAsync`, `AlocarRecursoAsync(userId, heroiId, TipoResourceNode)`, `AlocarSlotPredioAsync(userId, heroiId, TipoPredio, SlotTipo)→string?`, `DesalocarHeroiAsync(userId, heroiId)→string?`, `ConstruirPredioAsync(userId, TipoPredio)→string?`, `ObterSlotsPorPredioAsync(construcaoId)` |
| `ArenaService` | Training sessions and wave challenges | `TreinarAsync(userId, heroiId)→TreinoResult`, `DesafioOndasAsync(userId, List<Heroi>)→(DesafioResult?, string?)` |
| `PredioConfig` | Static config for all buildings (slots, costs, production) | `Slots[(TipoPredio,Nivel)]→SlotDefinicao`, `CustosConstrucao[TipoPredio]→ConstrucaoCusto`, `RecursoProducao[TipoPredio]→string?` |
| `ResourceNodeConfig` | Static config for ResourceNode base rates and profession bonuses | `BaseRates[TipoResourceNode]→(recurso,basePorHora)`, `ProfissaoBonus[(node,profissao)]→double` |
| `BiomeService` | Biome mapping, new biome detection, marco detection | `ObterBiomaParaAndarAsync(andar)`, `EBiomaNovoAsync(andar)`, `EAndarDeMarco(andar)` |
| `FragmentService` | Fragment drops, contract multiplier, progress upsert | `ProcessarDropAsync(userId, andar)→List<FragmentDropResult>`, `AdicionarFragmentosAsync(...)`, `ObterMultiplicadorAsync(userId, heroiId)` |
| `RecruitmentService` | 3 unlock paths | `TentarRecrutarPorFragmentosAsync(userId, heroiId)→RecruitmentResult`, `ProcessarMarcoTorreAsync(userId, andar)`, `DesbloquearPorCondicaoAsync(userId, heroiId)` |
| `ContractService` | Archetype + named contracts | `AtivarContratoArquetipoAsync(userId, profissao)`, `AtivarContratoNomeadoAsync(userId, heroiId, duracao?)`, `ExpirarContratosVencidosAsync()` |
| `RewardDistributionService` | 3-tier reward payload factory | `GerarMicroPico(drop)`, `GerarPicoMedio(heroi)`, `GerarPicoAlto(tipo, bioma?, heroi?)` |
| `GeracaoDeDadosService` | DB seed — tables + base data | `CriarTabelasAsync()`, `PopularDadosBaseAsync()` |
| `HabilidadeService` | Skill data access | `ObterTodasAsync()` |
| `UsuarioService` | User creation/lookup | |
| `RacaService` | Race metadata | |
| `AtributoBonusService` | Compute external bonus from skills (implements `IAtributoBonusService`) | `ObterBonus(habilidades)` |
| `PartyService` | Party CRUD | |
| `TreinamentoService` | Training sessions | |

### Static data on services
- `HeroiLevelUpService.Configs` — `IReadOnlyDictionary<int, RaridadeConfig>` (cap/BaseStatsTotal/GanhoPorNivel/GanhoSuperacao/BaseXp per rarity)
- `HeroiLevelUpService.BonusRacial` — `IReadOnlyDictionary<Raca, AtributosBase>` (+50 to focus attribute per race)
- `HeroiLevelUpService.MultiplicadorXpRacial` — `IReadOnlyDictionary<Raca, double>` (Humano=1.10, others=1.0)
- `CraftingService.Receitas` — 5 static recipes (espada-ferro, arco-simples, armadura-couro, anel-arcano, amuleto-agilidade)
- `ContractConfig` — `Application/DTOs/ContractConfig.cs`: `ChanceDropBase=0.30`, `BonusArquetipo=0.30`, `BonusNomeado=0.50`

---

## Formulas (canonical)

| Formula | Location |
|---|---|
| XP to next level | `B_r × nivel` — `HeroiLevelUpService.XpParaProximoNivel` |
| Damage | `ATK × SkillMult × (1 − DEF/(DEF + K)) × TypeMult`; K = 1000 + def.Nivel×50 — `CombatService.CalcularDano` |
| Crit | `5% + Percepcao×0.1%` → ×1.5 |
| Burst cap | hit ≤ 65% target.VidaMaxima |
| ATB initiative | `Agilidade + Random(0, Agilidade×0.1)` — `CombatService.ExecutarRound` |
| Tower XP | `10 + Numero×5` × boss_mult (×1.5/2.0/3.0) — `TorreService.SubirAndarAsync` |
| Racial mult | Humano +10% XP; others ×1.0 — `HeroiLevelUpService.AplicarXp` |

---

## Repositories

| Interface | Implementation | File |
|---|---|---|
| `ISlotOcupacaoRepository` | `SlotOcupacaoRepository` | `Infrastructure/Repositories/SlotOcupacaoRepository.cs` |
| `IHeroiRepository` | `HeroiRepository` | `Infrastructure/Repositories/HeroiRepository.cs` |
| `ICidadeRepository` | `CidadeRepository` | `Infrastructure/Repositories/CidadeRepository.cs` |
| `ITorreRepository` | `TorreRepository` | `Infrastructure/Repositories/TorreRepository.cs` |
| `IItemRepository` | `ItemRepository` | `Infrastructure/Repositories/ItemRepository.cs` |
| `IUsuarioRepository` | `UsuarioRepository` | `Infrastructure/Repositories/UsuarioRepository.cs` |
| `IHabilidadeRepository` | `HabilidadeRepository` | `Infrastructure/Repositories/HabilidadeRepository.cs` |
| `IPartyRepository` | `PartyRepository` | `Infrastructure/Repositories/PartyRepository.cs` |
| `IHeroiConfigRepository` | `HeroiConfigRepository` | `Infrastructure/Repositories/HeroiConfigRepository.cs` — `ObterPorIdAsync`, `ListarTodosAsync`, `ObterUnlockConfigAsync` |
| `IHeroiDesbloqueadoRepository` | `HeroiDesbloqueadoRepository` | `Infrastructure/Repositories/HeroiDesbloqueadoRepository.cs` — `JaDesbloqueadoAsync`, `SalvarAsync`, `ListarPorUsuarioAsync` |
| `IFragmentoRepository` | `FragmentoRepository` | `Infrastructure/Repositories/FragmentoRepository.cs` — `ObterPorHeroiAsync`, `UpsertAsync` (TOCTOU-safe) |
| `IBiomaRepository` | `BiomaRepository` | `Infrastructure/Repositories/BiomaRepository.cs` — `ObterParaAndarAsync`, `ListarTodosAsync` |
| `IContratoRepository` | `ContratoRepository` | `Infrastructure/Repositories/ContratoRepository.cs` — `ObterAtivoAsync(userId, tipo)`, `SalvarAsync`, `DesativarAsync`, `ListarAtivosVencidosAsync` |

`HeroiRepository.ObterPorIdAsync` and `ObterPorUsuarioIdAsync` include `BonusAtributos`.  
`ItemRepository` includes `Bonus` on all queries.

---

## DbContext — `Infrastructure/LegendsAwakenDbContext.cs`

| DbSet | Entity |
|---|---|
| `Herois` | `Heroi` |
| `Habilidades` | `Habilidade` |
| `HeroiHabilidades` | `HeroiHabilidade` |
| `Andares` | `TorreAndar` |
| `Cidades` | `Cidade` |
| `Usuarios` | `Usuario` |
| `Inimigo` | `Inimigo` |
| `HeroisAfinidades` | `HeroiAfinidadeElemental` |
| `HeroisVinculos` | `HeroiVinculo` |
| `HeroisTags` | `HeroiTag` |
| `Parties` | `Party` |
| `PartyHeroes` | `PartyHero` |
| `Itens` | `Item` |
| `ItemBonus` | `ItemBonus` |
| `HeroisConfig` | `HeroiConfig` |
| `HeroisUnlockConfig` | `HeroiUnlockConfig` |
| `Biomas` | `Bioma` |
| `BiomHeroPools` | `BiomHeroPool` |
| `FragmentosProgresso` | `FragmentoProgresso` |
| `Contratos` | `Contrato` |
| `HeroisDesbloqueados` | `HeroiDesbloqueado` |

EF OwnsOne: `Heroi → AtributosBase`, `Heroi → Status`, `Heroi → Equipamentos`, `Heroi → Treinamento`, `Inimigo → Atributos`, `Cidade → Recursos`

---

## Migrations — `Infrastructure/Migrations/`

| Migration | Date | What changed |
|---|---|---|
| `20250724221226_InitialCreate` | 2025-07-24 | Initial schema |
| `20260411035328_CidadeRefactor` | 2026-04-11 | City/worker model refactor |
| `20260416195336_CraftingV1` | 2026-04-16 | Equipamentos string→Guid FKs; ItemId on HeroiBonusAtributo; Itens + ItemBonus tables |
| `CidadeSlotModel3A2` | 2026-04-16 | Confianca/Humor/Lore on Heroi; ResourceNode on PersonagemTrabalhador; TipoPredio on Construcao; SlotOcupacoes table |
| `FragmentoSystem` (timestamp ~20260417) | 2026-04-17 | HeroisConfig, HeroisUnlockConfig, Biomas, BiomHeroPools, FragmentosProgresso, Contratos, HeroisDesbloqueados |
| `FragmentoSystemIndexes` (timestamp ~20260418) | 2026-04-18 | Partial unique indexes: FragmentosProgresso(UsuarioId+HeroiId), Contratos(UsuarioId+TipoContrato WHERE Ativo=1) |

---

## Bot — `LegendsAwaken.Bot/`

### Entry point — `Program.cs`
DI registration for all repositories and services. Builds `CommandHandler` with all services. Calls `CriarBancoEDadosBaseAsync()` on startup.  
Guild ID: hardcoded `1388541192806989834` (TODO: move to appsettings).

### `CommandHandler.cs`
Handles `SlashCommandExecuted`, `ButtonExecuted`, **`SelectMenuExecuted`**, `AutocompleteExecuted`.  
Constructor injects: `HeroiService`, `GeracaoDeDadosService`, `UsuarioService`, `RacaService`, `AtributoBonusService`, `CombatService`, `PartyService`, `CidadeService`, `CraftingService`, `ArenaService`, + fragment system: `IHeroiConfigRepository`, `IHeroiDesbloqueadoRepository`, `IFragmentoRepository`, `RecruitmentService`, `BiomeService`, `ContractService`, `IContratoRepository`, `ITorreRepository`.

### `Bot/Helpers/DiscordIdHelper.cs`
`ToGuid(ulong discordId) → Guid` — deterministic, little-endian via `BinaryPrimitives.WriteUInt64LittleEndian`. Used everywhere Discord `ulong` must map to internal `Guid UsuarioId`.

### Slash commands → Command classes

| Command | Class | File |
|---|---|---|
| `/ver_heroi` | `VerHeroiCommand` | `Commands/VerHeroiCommand.cs` |
| `/listar_herois` | `ListarHeroisCommand` | `Commands/ListarHeroisCommand.cs` |
| `/subir_andar` | `SubirAndarCommand` | `Commands/SubirAndarCommand.cs` |
| `/treinar` | `TreinarCommand` | `Commands/TreinarCommand.cs` |
| `/cidade` | `CidadeCommand` | `Commands/CidadeCommand.cs` |
| `/combate` | `CombatCommand` | `Commands/CombatCommand.cs` |
| `/crafting` | `CraftingCommand` | `Commands/CraftingCommand.cs` |
| `/heroi_equipar` | (inline in CommandHandler) | `CommandHandler.cs` |
| `/arena` | `ArenaCommand` | `Commands/ArenaCommand.cs` |
| `/colecao` | `ColecaoCommand` | `Commands/ColecaoCommand.cs` — coleção de heróis; button `colecao_recrutar` → recrutar por fragmentos; `MostrarAsync` para delegação |
| `/bioma` | `BiomaCommand` | `Commands/BiomaCommand.cs` — bioma atual com pools de drop |
| `/contrato` | `ContratoCommand` | `Commands/ContratoCommand.cs` — contratos ativos; select menu `contrato_arquetipo`; button `contrato_remover_nomeado`; `MostrarAsync` para delegação |

---

## Infrastructure Providers / Helpers

| File | Purpose |
|---|---|
| `Application/Helpers/NomeGenerator.cs` | Procedural hero name generation |
| `Bot/Helpers/EmbedHelper.cs` | Discord embed utilities |
| `Bot/Helpers/DiscordIdHelper.cs` | `ToGuid(ulong)` — Discord ID → Guid (little-endian, deterministic) |
| `Infrastructure/SeedData/HabilidadesSeed.cs` | Seed data for habilidades |
| `Infrastructure/SeedData/FragmentoSeed.cs` | Seed data for HeroisConfig, Biomas, BiomHeroPools, HeroisUnlockConfig |
| `Bot/Panels/ColecaoPanel.cs` | Static panel builder for `/colecao` |
| `Bot/Panels/BiomaPanel.cs` | Static panel builder for `/bioma` |
| `Bot/Panels/ContratoPanel.cs` | Static panel builder for `/contrato` |

---

## Extension Points

### Add a new `Atributo`
1. `Domain/Enum/Enums.cs` — add enum value
2. `Domain/Entities/AtributosBase.cs` — add property + 2 lines in `Get()`/`Set()`
3. EF migration needed (new column on OwnsOne tables)
4. Nothing else — `operator +`, `Distribute`, `With`, `ToEnumerable`, `BonusRacial`, `ObterAtributosBaseParaRaridade` auto-adapt

### Add a new `Raca`
1. `Domain/Enum/Enums.cs` — add enum value
2. `Application/Services/HeroiLevelUpService.cs` — add to `BonusRacial` and `MultiplicadorXpRacial`

### Add a crafting recipe (Phase 3A)
- `Application/Services/CraftingService.cs` — add entry to `Receitas` list

---

## Documentation files

| File | Contents |
|---|---|
| `docs/COMMANDS.md` | Referência completa de todos os 12 slash commands — parâmetros, valores, interações de painel |
| `GDD.md` | Full game design document (19 sections) |
| `DESIGN_SISTEMAS.md` | Math frameworks — XP curve, Power Score, combat, CDI, economy (§1–§11) |
| `ROADMAP.md` | Phase-by-phase plan; Fase 3A.1 ✅ complete |
| `TODO.md` | Granular task list by area |
| `ANALISE.md` | Architecture analysis |
| `Estrutura.md` | Project structure reference |
| `SESSAO_2026-04-16.md` | Session summary for git commit reference |
| `CLAUDE.md` | Project instructions for Claude Code (overrides) |

---

## Phase status

| Phase | Status |
|---|---|
| Fase 1 — Pré-produção | ✅ Complete |
| Fase 2 — Protótipo da Cidade | ✅ Complete |
| Fase 3A.1 — Vertical Slice | ✅ Complete (build 0w/0e) |
| Fase 3A.2 — Consolidação do Core | ✅ Complete (build 0w/0e) — backend + bot layer |
| Fase 3A.3 — Sistema de Fragmentos | ✅ Complete (build 0w/0e, 39 tests) — gacha substituído por fragmentos/biomas/contratos |
| Fase 3B+ | Not started |
