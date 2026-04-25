# AI_INDEX.md — LegendsAwaken Navigation Reference
<!-- Lookup-only. Read this before opening any file. Update whenever the codebase changes. -->
<!-- Last updated: 2026-04-25 · Bioma Panel, Cidade UX e Torre Modo Operação v2 -->

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
| `TipoAndar` | Normal, BossFacil, BossMedio, BossDificil |
| `StatusExploracao` | Ativa=0, Concluida=1, Falha=2, Coletada=3 |
| `TipoBooster` | Eficiencia, Ouro, Fragmento, Checkpoint, Progresso |
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
| `ObjetivoOperacao` | FarmRecurso, ExploracaoLeve |
| `PerfilRisco` | Seguro, Balanceado, Agressivo |
| `StatusOperacao` | Ativa, Concluida, Expirada |
| `EstadoSustento` | Ativo, Instavel, Degradado, Inativo |

---

## Entities

### `Heroi` — `Domain/Entities/Heroi.cs`
Fields: `Id`, `UsuarioId`, `Nome`, `Raridade`, `Raca`, `Profissao?`, `Antecedente?`, `Nivel`, `XP`, `AtributosBase`, `AtributosDistribuidos`, `PontosAtributosDisponiveis`, `BonusAtributos` (List<HeroiBonusAtributo>), `Status` (StatusCombate), `Habilidades` (List<HeroiHabilidade>), `Equipamentos`, `Treinamento?`, `Funcao?`, `EstaAtivo`, `EstadoSustento` (EstadoSustento, default Ativo), `ImagemUrl?`, `Confianca` (int, default 0), `Humor` (int, default 50), `Lore?`, `Vitorias`, `Derrotas`, `AndaresConquistados`, `Lealdade`, `Historia?`, `Personalidade?`, `AfinidadeElemental`, `VinculosHeroicos`, `Tags`  
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
`Cidade`: `Id`, `UsuarioId`, `Nome`, `Nivel`, `Populacao`, `CapacidadeMaxima`, `Recursos`, `Construcoes`, `Trabalhadores`, `UltimaColeta`, `UltimoSustentoEm` (DateTime, default UtcNow)  
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
`Party`: `Id`, `UsuarioId` (ulong), `Nome`, `NomeModoManual` (bool — false=auto, true=fixed), `Membros` (List<PartyHero>)  
`PartyHero`: `PartyId`, `HeroiId`, nav `Party`, nav `Heroi`  
**Auto-name**: ≤3 heroes → all names joined by ` / `; >3 → first 2 + `" +N"`. Updated automatically on add/remove unless `NomeModoManual = true`.  
**Migration**: `GruposNomeModoManual` adds `NomeModoManual INTEGER NOT NULL DEFAULT 0`.
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

### `TorreOperacao` — `Domain/Entities/TorreOperacao.cs`
Fields: `Id`, `UsuarioId`, `AndarNumero`, `ObjetivoOperacao`, `PerfilRisco`, `StatusOperacao`, `IniciadoEm`, `DuracaoHoras`, `ResultadoOuro?`, `ResultadoRecursoNome?`, `ResultadoRecursoQtd?`, `ConcluidoEm?`  
Managed by raw SQLite in `TorreOperacaoRepository.EnsureTableAsync()` — not tracked by EF Core.

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
| `BiomeService` | Biome mapping, new biome detection, marco detection | `ObterBiomaParaAndarAsync(andar)`, `EBiomaNovoAsync(andar)`, `EAndarDeMarco(andar)`, `ListarDescobertosAsync(andarAtual)`, `ObterPorIdAsync(id)` |
| `FragmentService` | Fragment drops, contract multiplier, progress upsert | `ProcessarDropAsync(userId, andar)→List<FragmentDropResult>`, `AdicionarFragmentosAsync(...)`, `ObterMultiplicadorAsync(userId, heroiId)` |
| `RecruitmentService` | 3 unlock paths | `TentarRecrutarPorFragmentosAsync(userId, heroiId)→RecruitmentResult`, `ProcessarMarcoTorreAsync(userId, andar)`, `DesbloquearPorCondicaoAsync(userId, heroiId)` |
| `ContractService` | Archetype + named contracts | `AtivarContratoArquetipoAsync(userId, profissao)`, `AtivarContratoNomeadoAsync(userId, heroiId, duracao?)`, `ExpirarContratosVencidosAsync()` |
| `RewardDistributionService` | 3-tier reward payload factory | `GerarMicroPico(drop)`, `GerarPicoMedio(heroi)`, `GerarPicoAlto(tipo, bioma?, heroi?)` |
| `TorreOperacaoService` | Torre Modo Operação — múltiplas operações simultâneas por andar (board) | `IniciarAsync(userId, andar, construcoes)`, `ProcessarTodasAsync(userId)` (auto-conclui expiradas), `ColetarTodasAsync(userId)` (credita recursos de todas concluídas), `CancelarPorAndarAsync(userId, andar)` |
| `TorreOperacaoConfig` | Config estática de Modo Operação (não injetável) | `DuracaoHoras=8`, `ObterProducao(andar)`, `ObterAfinidade(heroi, andar)`, `CalcularMaxSlots(construcoes)` (2 + GuildaNivel×2) |
| `TorreExploracaoService` | Torre Exploração — progresso em tempo real por party | `ProcessarAsync(userId)` (tick, debounce 0.1 min), `IniciarAsync(userId, heroisIds, booster?)`, `ColetarAsync(userId, discordId)`, `CancelarAsync(userId)`, `ObterAtivaAsync`, `ObterPendenteAsync`, `ObterBoostersAsync`, `AplicarBoosterGratuitoAsync`; rate: `Math.Min(1.5 × ratio × boosterMult, 3.0)` %/min (max ~33 min/andar) |
| `SustentoService` | City-level food consumption + hero state | `ProcessarAsync(ulong userId)` (poll on every command), `ToggleInativoAsync(Guid heroiId)`, `ObterResumo(Cidade, herois) → static (consumoPorHora, horasRestantes, estado)` |
| `GeracaoDeDadosService` | DB seed — tables + base data | `CriarTabelasAsync()`, `PopularDadosBaseAsync()` |
| `HabilidadeService` | Skill data access | `ObterTodasAsync()` |
| `UsuarioService` | User creation/lookup | |
| `RacaService` | Race metadata | |
| `AtributoBonusService` | Compute external bonus from skills (implements `IAtributoBonusService`) | `ObterBonus(habilidades)` |
| `PartyService` | Party CRUD + group preset management | `ObterPartiesUsuarioAsync(userId)`, `ObterPorIdAsync(partyId)`, `CriarComHeroisAsync(userId, herois)`, `CriarRecomendadaAsync(userId, todos)` (top-5 by PS, guarantees Tank+Healer), `AdicionarHeroiComNomeAutoAsync`, `RemoverHeroiComNomeAutoAsync`, `ToggleModoNomeAsync`, `AtualizarNomeManualAsync`, `DeletarAsync`. Static: `GerarNomeAuto(herois)` — ≤3 names shown, then `"A / B +N"` |
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
| `IPartyRepository` | `PartyRepository` | `Infrastructure/Repositories/PartyRepository.cs` — `CriarAsync`, `ObterPartiesPorUsuarioAsync`, `ObterPorIdAsync`, `AdicionarHeroiAsync`, `RemoverHeroiAsync`, `AtualizarAsync(party)`, `DeletarAsync(partyId)` |
| `IHeroiConfigRepository` | `HeroiConfigRepository` | `Infrastructure/Repositories/HeroiConfigRepository.cs` — `ObterPorIdAsync`, `ListarTodosAsync`, `ObterUnlockConfigAsync` |
| `IHeroiDesbloqueadoRepository` | `HeroiDesbloqueadoRepository` | `Infrastructure/Repositories/HeroiDesbloqueadoRepository.cs` — `JaDesbloqueadoAsync`, `SalvarAsync`, `ListarPorUsuarioAsync` |
| `IFragmentoRepository` | `FragmentoRepository` | `Infrastructure/Repositories/FragmentoRepository.cs` — `ObterPorHeroiAsync`, `UpsertAsync` (TOCTOU-safe) |
| `IBiomaRepository` | `BiomaRepository` | `Infrastructure/Repositories/BiomaRepository.cs` — `ObterParaAndarAsync`, `ListarTodosAsync` |
| `IContratoRepository` | `ContratoRepository` | `Infrastructure/Repositories/ContratoRepository.cs` — `ObterAtivoAsync(userId, tipo)`, `SalvarAsync`, `DesativarAsync`, `ListarAtivosVencidosAsync` |
| `ITorreOperacaoRepository` | `TorreOperacaoRepository` | `Infrastructure/Repositories/TorreOperacaoRepository.cs` — raw SQLite; `EnsureTableAsync()`, `SalvarAsync`, `ObterAtivaAsync(userId)`, `AtualizarAsync`, `ObterPorIdAsync`, `ListarAtivasAsync(userId)`, `ListarConcluidasAsync(userId)`, `ObterPorAndarAsync(userId, andar)` |
| `ITorreExploracaoRepository` | `TorreExploracaoRepository` | `Infrastructure/Repositories/TorreExploracaoRepository.cs` — raw SQLite; `EnsureTableAsync()`, `SalvarAsync`, `AtualizarAsync`, `ObterAtivaAsync(userId)`, `ObterPendenteAsync(userId)`. **Note:** `Mapear` uses `DateTimeStyles.RoundtripKind` to avoid UTC→local timezone inflation bug |
| `ITorreBoosterRepository` | `TorreBoosterRepository` | `Infrastructure/Repositories/TorreBoosterRepository.cs` — raw SQLite; `AdicionarAsync(userId, tipo, qtd)`, `ConsumirAsync(userId, tipo)→bool`, `ListarAsync(userId)→List<(TipoBooster,int)>` |

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
| `20260423200000_SustentoSystem` | 2026-04-23 | `ADD COLUMN EstadoSustento INTEGER DEFAULT 0` em Herois; `ADD COLUMN UltimoSustentoEm TEXT` em Cidades; inclui `.Designer.cs` |

---

## Bot — `LegendsAwaken.Bot/`

### Entry point — `Program.cs`
DI registration for all repositories and services. Builds `CommandHandler` with all services. Calls `CriarBancoEDadosBaseAsync()` on startup.  
Guild ID: hardcoded `1388541192806989834` (TODO: move to appsettings).

### `CommandHandler.cs`
Handles `SlashCommandExecuted`, `ButtonExecuted`, **`SelectMenuExecuted`**, `AutocompleteExecuted`.  
Constructor injects: `HeroiService`, `GeracaoDeDadosService`, `UsuarioService`, `RacaService`, `AtributoBonusService`, `CombatService`, `PartyService`, `CidadeService`, `CraftingService`, `ArenaService`, fragment system: `IHeroiConfigRepository`, `IHeroiDesbloqueadoRepository`, `IFragmentoRepository`, `RecruitmentService`, `BiomeService`, `ContractService`, `IContratoRepository`, `ITorreRepository`, `TorreOperacaoService`, `SustentoService`, `ITorreExploracaoRepository`, `ITorreBoosterRepository`, `TorreExploracaoService`.  
`SustentoService.ProcessarAsync(userId)` called at the top of every `HandleSlashCommandAsync` after `ObterOuCriarAsync`.  
Command registration uses **delete-before-create** pattern (ensures removed commands disappear from Discord client).  

**Button/Select routing additions (2026-04-24):**
- `torre_exp_grupo_sel` → `TorreCommand.HandleExpGrupoSelAsync`
- `torre_exp_booster_sel|{partyId}` → `TorreCommand.HandleExpBoosterSelAsync` (partyId in custom_id)
- `torre_explorar_confirmar|{booster}|{partyId}` → `TorreCommand.HandleExplorarConfirmarAsync` (3 parts)
- `torre_bioma` → `BiomaCommand.ExecutarAsync` (redireciona para lista)
- `bioma_atualizar` → `BiomaCommand.MostrarListaAsync`
- `bioma_lista` → `BiomaCommand.MostrarListaAsync`
- `herois_colecao` → `ColecaoCommand.MostrarAsync`
- `herois_treinar` / `herois_treinar_heroi` → `ArenaService` training flow

**Button/Select routing additions (2026-04-25 — Torre Modo Operação v2):**
- `torre_op_alocar` (button) → `TorreCommand.HandleOpAlocarAsync`
- `torre_op_coletar_todas` (button) → `TorreCommand.HandleOpColetarTodasAsync`
- `torre_op_remover_sel` (button) → `TorreCommand.HandleOpRemoverSelAsync`
- `torre_op_fechar` (button) → `TorreCommand.HandleOpFecharAsync`
- `torre_op_andar_sel` (SelectMenu) → `TorreCommand.HandleOpAndarSelAsync`
- `torre_op_remover_andar_sel` (SelectMenu) → `TorreCommand.HandleOpRemoverAndarSelAsync`
- `bioma_detalhe_sel` (SelectMenu) → `BiomaCommand.MostrarDetalheAsync`

**Removed handlers:** `contrato_arquetipo`, `contrato_remover_nomeado`, `bioma_contratos`, wizard IDs do Modo Operação antigo (`torre_op_andar`, `torre_op_objetivo`, `torre_op_risco`, `torre_op_confirmar`, `torre_op_coletar`, `torre_op_cancelar`)

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
| `/bioma` | `BiomaCommand` | `Commands/BiomaCommand.cs` — lista de biomas descobertos e detalhe por bioma com sistema de descoberta de heróis; construtor: `BiomaCommand(BiomeService, ITorreRepository, IFragmentoRepository, IHeroiConfigRepository)`; handlers: `ExecutarAsync`, `MostrarListaAsync`, `VoltarListaAsync`, `MostrarDetalheAsync`; button routing `torre_bioma`/`bioma_atualizar`/`bioma_lista`; Select Menu `bioma_detalhe_sel` |
| `/herois` | `HeroisCommand` | `Commands/HeroisCommand.cs` — lista com ícones de sustento (`✅⚠️🔴💤`); detalhe com campo Sustento + toggle; `HandleGruposAsync` → delegates to `GruposCommand.AbrirAsync` |
| (Grupos panel) | `GruposCommand` | `Commands/GruposCommand.cs` — full panel-based group management: `AbrirAsync` (entry from `herois_grupos`), `HandleListaAsync`, `HandleVerSelAsync`, `HandleCriarAsync`, `HandleCriarSelAsync`, `HandleRecomendadoAsync`, `HandleVerAsync`, `HandleAddSelAsync`, `HandleAddAsync`, `HandleRemSelAsync`, `HandleRemAsync`, `HandleNomeToggleAsync`, `HandleNomeEditarAsync` (opens modal `grupos_nome_modal\|{id}`), `HandleNomeModalAsync` (ModalSubmitted), `HandleDeletarAsync` |
| `/torre` | `TorreCommand` | `Commands/TorreCommand.cs` — handles both Exploração (party-based) and Modo Operação. Exploração: `HandleExplorarAsync`→party selector→`HandleExpGrupoSelAsync`→`MostrarConfirmacaoGrupoAsync`→`HandleExpBoosterSelAsync`→`HandleExplorarConfirmarAsync`→`PrepararInicioAsync`. Operação (v2 board): `HandleModoOperacaoAsync`, `HandleOpAlocarAsync`, `HandleOpAndarSelAsync`, `HandleOpColetarTodasAsync`, `HandleOpRemoverSelAsync`, `HandleOpRemoverAndarSelAsync`, `HandleOpFecharAsync`. Construtor agora inclui `CidadeService`. Takes `PartyService` + `TorreExploracaoService` + `CidadeService` + booster/exploration repos. |

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
| `Bot/Panels/ContratoPanel.cs` | Static panel builder for `/contrato` (slash command removed; panel file retained) |
| `Bot/Panels/BiomaPanel.cs` | Lista de biomas descobertos e detalhe por bioma; `CriarLista(biomas, andarAtual)` — Select Menu com % andares conquistados e indicador do bioma atual; `CriarDetalhe(bioma, pool, fragmentos, unlockMap, andarAtual)` — barra de progresso + sistema de descoberta de heróis do pool |
| `Bot/Panels/HeroisPanel.cs` | List with sustento icons; detail with Sustento field; buttons: `👥 Grupos` (`herois_grupos`), `📖 Coleção` (`herois_colecao`), `⚔️ Treinar` (`herois_treinar`), `🔄` (`herois_atualizar`), toggle `herois_toggle_inativo\|{id}` |
| `Bot/Panels/GruposPanel.cs` | Group preset panel — `CriarEmbedLista`, `CriarComponentesLista` (select `grupos_ver_sel`); `CriarEmbedDetalhe`, `CriarComponentesDetalhe`; `CriarSeletorCriacao` (multi-select `grupos_criar_sel`); `CriarSeletorAddHeroi` (select `grupos_add\|{id}`); `CriarSeletorRemHeroi` (select `grupos_rem\|{id}`) |
| `Bot/Panels/TorrePanel.cs` | Torre main view — exploration status block with `⏱️ ETA`; `🗺️ Bioma` button (`torre_bioma`) |
| `Bot/Panels/TorreModoOperacaoPanel.cs` | Torre Modo Operação — board de andares ativos/concluídos com múltiplos slots simultâneos; `CriarBoard(ativas, concluidas, andarAtual, maxSlots)`, `CriarSemAndares()`, `CriarSeletorAndar(andarAtual, bloqueados, maxSlots, emUso)`, `CriarSeletorRemover(ativas)`, `CriarNotificacaoTexto(List<>)` e sobrecarga |
| `Bot/Panels/TorreExploracaoPanel.cs` | Torre Exploração flow — `CriarSeletorGrupo(andar, parties)`: select `torre_exp_grupo_sel`; `CriarSeletorBooster(partyId, partyNome)`: select `torre_exp_booster_sel\|{partyId}`; `CriarConfirmacao(partyNome, heroisNomes, partyId)`: button `torre_explorar_confirmar\|nenhum\|{partyId}`; `CriarAtivo`, `CriarConcluido`, `CriarFalha`, `CriarInvestigacao` |

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
| Fase 3B.3 — Torre Modo Operação | ✅ Complete (v3.1.0) — TorreOperacao entity, TorreOperacaoService, TorreModoOperacaoPanel, roteamento no CommandHandler |
| Fase 3B.4 — Sustento MVP | ✅ Complete (v3.2.0) — EstadoSustento enum+campo, SustentoService (poll+toggle+resumo), ícones HeroisPanel, linha CidadePanel, migration SustentoSystem |
| Sessão 2026-04-24 — UX + Torre Exploração | ✅ Complete — party-based exploration flow, datetime timezone fix (RoundtripKind), rate 20→3%/min, `ObterHeroisAlocadosAsync` helper, node prod display com bônus de profissão, slot filter em prédios, bioma só com Atualizar, herois_colecao/treinar buttons, torre_bioma button, ETA display, delete-before-create command registration, `/contrato` removido |
| Sessão 2026-04-24 — Sistema de Grupos | ✅ Complete — `Party.NomeModoManual`, EF migration, `PartyRepository.AtualizarAsync/DeletarAsync`, `PartyService` full rewrite (auto-name, recomendado, CRUD), `GruposPanel`, `GruposCommand` (15 handlers), `herois_grupos` button, modal integration (`grupos_nome_modal\|{id}`) |
| Sessão 2026-04-25 — Bioma Panel + Cidade UX + Torre Op v2 | ✅ Complete — bug fixes (CidadeCommand ícones, TorreCommand teamPS, ColecaoCommand 40060), BiomaPanel list/detail com sistema de descoberta de heróis, CidadePanel agrupado por node com contador, ResourceNodeConfig.Icone centralizado, TorreOperacaoConfig estática, TorreOperacaoService reescrito (múltiplas ops simultâneas, 8h fixo, slots por GuildaNivel), TorreModoOperacaoPanel board, ITorreOperacaoRepository+Impl 3 novos métodos, TorreCommand handlers v2, CommandHandler routing atualizado |
| Fase 3B (restante) | 3B-1 Inventário, 3B-2 Conversão, 3B-5 Guilda/Missões, 3B-6 Relíquias, 3B-7 Prédios, Market, Mercenários, Treinamento — não iniciados |
