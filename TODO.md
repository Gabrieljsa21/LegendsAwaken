# Legends Awaken — TODO

Tarefas granulares organizadas por área. Acompanhe o progresso macro no `ROADMAP.md`.

---

## Pré-produção (Fase 1) ✅ concluída

- [x] Fechar escopo do v1.0
- [x] Definir pool inicial de personagens fixos (3x 5★ + 6x 4★) com nome, raça, profissão e lore
- [x] Documentar receitas básicas de crafting no GDD
- [x] Documentar cadeia de dependência dos itens básicos no GDD
- [ ] Gerar arte IA para cada personagem fixo e registrar as URLs *(pode ser feito em paralelo com a produção)*

---

## ~~Gacha~~ → substituído pelo Sistema de Fragmentos (Fase 3A.3)

> `GachaService` e `BannerService` foram removidos. Os itens abaixo são históricos.

- [x] Sistema de invocação (x1 e x11) — *legado*
- [x] Soft-pity com curva cúbica por banner — *legado*
- [x] Banners configuráveis — *substituídos por Contratos*
- [x] Geração procedural de heróis
- [x] Dropdown de seleção de banner — *substituído por `/bioma` e `/contrato`*
- [x] Distribuição de raças por raridade — *lógica preservada na geração procedural*
- [x] Distribuição uniforme entre raças não-humanas
- [x] Campo `ImageUrl` na entidade `Heroi`
- [x] Campo `Lore` na entidade `Heroi` (para personagens fixos)
- [x] Cadastrar pool de personagens fixos 5★/4★ no seed (9 personagens, idempotente)
- ~~Exibir arte no embed do pull~~ *(descartado — exibição via `/colecao`)*
- ~~Banner de Profissão~~ *(substituído por Contrato de Arquétipo)*

## Sistema de Fragmentos ✅ concluído (Fase 3A.3)

- [x] Entidades: `HeroiConfig`, `Bioma`, `BiomHeroPool`, `HeroiUnlockConfig`, `FragmentoProgresso`, `Contrato`, `HeroiDesbloqueado`
- [x] Repositórios: `IHeroiConfigRepository`, `IHeroiDesbloqueadoRepository`, `IFragmentoRepository`, `IBiomaRepository`, `IContratoRepository`
- [x] Migration + seed: 9 heróis com unlock config, 5 biomas com pools de drop
- [x] `BiomeService` — mapeamento andar→bioma, detecção de bioma novo e marco da Torre
- [x] `FragmentService` — drops pesados por bioma com multiplicador de contrato
- [x] `RecruitmentService` — 3 caminhos: fragmentos, marco da Torre, condição única
- [x] `ContractService` — arquétipo (+30%) e nomeado (+50%); expiração automática
- [x] `RewardDistributionService` — payloads Micro/Médio/Alto por tipo de evento
- [x] `TorreService` estendido com drops e desbloqueio de herói por marco
- [x] `/colecao` — painel de coleção com progresso, barra, recrutar
- [x] `/bioma` — bioma atual com heróis e pesos de drop
- [x] `/contrato` — contratos ativos com select menu de arquétipo
- [x] `DiscordIdHelper.ToGuid(ulong)` — conversão determinística Discord ID → Guid
- [x] 39 testes unitários passando

---

## Heróis

- [x] Atributos base (Força, Agilidade, Vitalidade, Inteligência, Percepção)
- [x] Raças com bônus passivos
- [x] Profissões
- [x] Sistema de habilidades com XP e níveis
- [x] `/ver_heroi` com autocomplete
- [x] `/listar_herois` com paginação e filtro
- [x] `RaridadeConfig` centralizado (cap, base stats, ganhos) — SOLID
- [x] Aplicar bônus racial (+50 no atributo foco) na criação do herói
- [x] Aplicar `ObterAtributosBaseParaRaridade()` na criação do herói
- [x] Equipar herói com item craftado (`/heroi_equipar`) — `HeroiService.EquiparItemAsync`, persiste via `HeroiBonusAtributo`
- [ ] Exibir equipamentos no `/ver_heroi`
- [ ] Apelido e arte customizada (`/heroi apelido`, `/heroi arte`)

---

## Sistema de Níveis e Ascensão

- [x] `HeroiLevelUpService` com `RaridadeConfig` (caps 20/40/60/80/100)
- [x] Ganhos por level-up por raridade (+2/+3/+4/+6/+8+12)
- [x] `CalcularGrantAscensao` — catch-up automático ao nativo da nova raridade
- [x] `CalcularTotalPontosNativo` — total derivado, sem valores fixos
- [x] **Curva de XP (Fase 3A):** `XP_next = B_r × nível` em `HeroiLevelUpService.XpParaProximoNivel`  
  `B_r`: 1★=80 | 2★=100 | 3★=120 | 4★=150 | 5★=200 (campo `BaseXp` em `RaridadeConfig`)
- [ ] **(Beta)** Migrar curva de XP para `B_r × nível^1.25` após coleta de dados reais (ver `DESIGN_SISTEMAS.md §1`)
- [x] Ganho de XP ao subir andares da Torre (`TorreService.SubirAndarAsync` → `AplicarXp`)
- [ ] Ganho de XP passivo na Cidade (menor que Torre)
- [ ] Ganho de XP em missões da Guilda
- [x] Level-up: distribuir pontos e checar cap da raridade (`HeroiLevelUpService.AplicarXp`)
- [x] Bloquear XP ao atingir o cap (XP zerado, level travado até ascensão)
- [ ] Sistema de fragmentos de arquétipo (entidade, acúmulo por atividade)
- [ ] Ascensão: consumir fragmentos + materiais, aplicar grant, subir raridade
- [ ] `/heroi ascender` com verificação de cap e custo

---

## Camada de Interação — UX-0 (fazer antes de expandir para novos sistemas)

Padrão de UX híbrido aprovado. Validar com `/cidade` antes de aplicar aos demais sistemas.

**Decisões:** painéis principais são públicos (canal); feedback de ação é efêmero; navegação atualiza in-place; expiração de 15 min aceita; dados sempre lidos do banco.

- [ ] Convenção de `customId`: `sistema:acao[:param1:param2]`
- [ ] `InteractionRouter` — parseia `customId`, despacha ao handler correto
- [ ] `PanelBuilder` base — ViewModel → `(Embed, ComponentBuilder)`; Services sem tipos Discord
- [ ] Padrão `DeferAsync` + `UpdateAsync` para handlers com acesso ao banco
- [ ] Padrão de confirmação efêmera: `[Confirmar] [Cancelar]` com timeout para ações destrutivas
- [ ] `/cidade` convertido para painel público com botões e Select Menu (validação do padrão)

---

## Inventário Unificado (Fase 3B-1 — pré-requisito para Relíquias)

Evita fragmentação de lógica entre itens, relíquias e recursos.

- [ ] `Inventario` — entidade unificada por jogador; tipos: `Recurso | Item | Reliquia | Consumivel`
- [ ] Operações atômicas: `Add(tipo, id, qtd)` / `Remove(tipo, id, qtd)` com validação prévia
- [ ] Stack limit por tipo (ex: relíquias não stackam; recursos sim)
- [ ] `/inventario` — painel com abas por categoria (`[Equipamentos] [Relíquias] [Recursos] [Consumíveis]`) e paginação

## Relíquias (Fase 3B-6 — requer Inventário Unificado)

- [ ] Entidade `Reliquia` (Id, Nome, Descrição, Efeito, AndarMinimo)
- [ ] 3 slots de relíquia por herói (`HeroiReliquia`)
- [ ] Drop de relíquias em boss floors da Torre (requer Inventário Unificado)
- [ ] `/heroi reliquia equipar`, `remover`, `ver`
- [ ] `/inventario reliquias`
- [ ] Aplicar efeito passivo da relíquia no combate
- [ ] Extensibilidade: efeitos de relíquia via interface `IEfeitoReliquia` — evita switch gigante

---

## Combate — Especificação Core (Fase 3A.1 — fazer antes da Torre)

Fórmulas definidas em `GDD.md §5.0` e `DESIGN_SISTEMAS.md §3`.

- [x] **Fórmula de dano**: `ATK × SkillMult × (1 - DEF/(DEF+1000+Level×50)) × TypeMult`; crit 1.5× em `CombatService.CalcularDano`
- [x] **Burst cap**: hit único ≤ 65% HP máximo do alvo (`BurstCapFactor = 0.65`)
- [x] **Ordem de turno ATB**: `InitScore = Agilidade + Random(0, Agilidade×0.1)` em `ExecutarRound`
- [ ] **Sistema de ameaça (aggro)**: tanques/paladinos atraem mais ataques; afeta alvo selecionado por inimigos
- [ ] **Escalas de inimigos por andar**: `stats = base × (1 + 0.08)^floor`; calibrar com tabela CDI (`DESIGN_SISTEMAS.md §5`)
- [ ] **Power Score**: implementar `HeroPowerScore` para usar em sucesso/falha de missões (`DESIGN_SISTEMAS.md §2`)
- [ ] **Posicionamento leve (front/back)**: heróis melee → front; ranged/mago → back; inimigos atacam front por padrão
- [ ] **Sinergia de habilidades**: pelo menos 1 combo básico (ex: fogo + vento = área)
- [ ] **IA tática**: atacar menor HP (DPS focus) ou maior ameaça (proteção de suporte)
- [ ] `IRandomProvider` interface — encapsula RNG com seed controlado (base para replay e debug determinístico)

## Torre (Fase 3A — MVP)

- [x] Torre infinita com andares por usuário
- [x] Tipos de andar (Subjugação, Fuga, Escolta, Defesa, Armadilha, Evento)
- [x] Bosses em andares 5 / 10 / 25
- [x] Combate automático por turnos
- [x] Party de até 5 heróis (`/grupo`)
- [ ] Drops de materiais de crafting em andares de boss
- [ ] Fragmentos de personagens fixos como drop raro
- [x] `/treinar` funcional via Arena (XP acelerado)

## Torre — Modo Operação ✅ MVP concluído (Fase 3B-3 — v3.1.0)

Entidade `TorreOperacao`, `TorreOperacaoService`, painel `TorreModoOperacaoPanel` (4 etapas), botão `🏭 Modo Operação` no `TorrePanel`.

- [ ] Estado `AndarConcluido` por usuário + andar *(não exigido no MVP)*
- [x] `TorreModoOperacaoPanel` — flow: Select Menu de andar → objetivo (FarmRecurso / ExploracaoLeve) → perfil de risco (Seguro / Balanceado / Agressivo) → confirmação
- [x] `TorreOperacaoService.IniciarAsync`, `VerificarPendenteAsync` (auto-conclui expirado), `ColetarAsync` (credita ouro), `CancelarAsync`
- [x] Fórmula de ouro: `andar × 3 × horas × mult` (Seguro=0.8, Balanceado=1.0, Agressivo=1.5)
- [x] Recursos exclusivos por andar: Fragmento Rústico (≥5), Essência Corrompida (≥12), Cristal Arcano (≥18), Núcleo Sombrio (≥25)
- [x] Poll de operação pendente em `TorreCommand.ExecutarAsync`
- [ ] Eventos de interrupção com decisão do jogador *(deferido)*
- [ ] Líder decide automaticamente se jogador não responder *(deferido)*
- [ ] Sistema de notificação inteligente *(deferido)*

## Torre — Design Avançado (Fase 3C)

- [ ] Progresso % por andar (múltiplas ações contam: inimigos derrotados, áreas exploradas, eventos resolvidos, boss)
- [ ] Requisito secreto por andar (ex: "termine sem mortes", "não ataque o NPC", "ache a sala oculta") — progresso fica em 92% até ser cumprido
- [ ] Zonas por andar (Entrada, Bioma, Núcleo do Boss)
- [ ] Identidade mecânica por andar (ex: andar 10 → anti-cura; andar 18 → tempo limitado; andar 22 → anti-tank)
- [ ] Memória do andar (NPCs/eventos lembram decisões anteriores do jogador)
- [ ] Overclear: 100% concluído → 120% desbloqueio de evento raro → 150% domínio total
- [ ] Estado do andar (Normal / Corrompido / Instável / Rico) que muda ao longo do tempo
- [ ] Seed de run (identificador único por run para reprodutibilidade e debug)
- [ ] Perfil de run ao final: estilo, eventos resolvidos, segredos, risco assumido
- [ ] Anti-meta rígida: andares que penalizam builds específicas (anti-tank, anti-magia, anti-heal, anti-cura)

---

## Cidade — Base

- [x] Entidade `Cidade` (Nome, Nível, Recursos, Construções, Trabalhadores)
- [x] Recursos: Comida, Madeira, Pedra, Ouro, Erva
- [x] Enum `Profissao` com combate, coleta e produção
- [x] `/cidade ver` — painel com recursos, prédios e heróis alocados
- [x] `/cidade coletar` — coleta produção acumulada por tempo
- [x] Produção passiva com teto de 24h
- [x] Alocação manual (`/cidade alocar`, `/cidade desalocar`)
- [x] `CidadeRepository` em EF Core
- [ ] Upgrades de prédio nível 1→2

---

## Cidade — Modelo de Slots (Fase 3A — ✅ concluída)

Rework determinístico da cidade: sem auto-alocação, sem IA. Cada prédio tem dois tipos de slot.

- [x] **Slots de Responsabilidade**: requerem `Confianca` mínima + atributo mínimo; prédio inativo se não preenchidos
- [x] **Slots de Operação**: heróis que executam trabalho; afetam volume via `Soma(operadores)`
- [x] Campo `Confianca` (0–100) na entidade `Heroi` (default 0)
- [x] Campo `Humor` na entidade `Heroi` (default 50)
- [x] **ResourceNode** (Campo/Floresta/Mina/Prado): tier 1 de produção, sem slot, com bônus de profissão
- [x] **Humor da Cidade** = média simples dos heróis alocados nos trabalhadores; mult 0.9/1.0/1.1/1.2
- [x] Fórmula Tier 2: `BaseProdPorHora × MultResp × SomaOp × HumorMult × horas`
- [x] Modificador de eficiência individual: `1 + (AtributoRelevante / 100)`
- [x] `PredioConfig` — slots, custos e recurso produzido por (TipoPredio, Nivel) — sem hardcode
- [x] `SlotOcupacao` entity + `ISlotOcupacaoRepository` + migration `CidadeSlotModel3A2`
- [ ] Confiança desbloqueia funções avançadas (ex: Forja nível 71+ → Mestre da Forja) — Fase 3B
- [ ] Slots de Liderança: herói com Confiança ≥ 61 → +10% produção global do prédio — Fase 3B

---

## Cidade — Novos Prédios (Fase 3B-7)

- [ ] **Armazém** — limite de estoque de recursos; overflow converte automaticamente (ex: madeira excedente → ouro a 80% eficiência)
- [ ] **Mercado** — conversão de recursos em ouro; pode operar automático; melhores taxas com upgrade
- [ ] **Prefeitura** — define limites globais (nº de heróis alocados, nº de construções); necessária para desbloquear novos prédios avançados
- [ ] **Quartel** — profissões: Guerreiro, Paladino; produção: buffs para Torre; upgrade: +eficiência de combate
- [ ] **Academia** — profissões: Pesquisador; produção: XP global / passivas; upgrade: desbloqueia perks permanentes
- [ ] **Tesouro** — armazena ouro; protege contra eventos negativos; pode gerar juros leve
- [ ] **Torre de Vigilância** — profissões: Arqueiro, Caçador; melhora previsibilidade da Torre; aumenta chance de encontrar segredos
- [ ] **Pedreira** — profissões: Mineiro; produção: pedra em volume; separa pedra de minério para mais controle
- [ ] **Santuário** — profissões: Mago, Clérigo; ativa buffs temporários (+drop, +XP, +evento raro); bom sink de recursos
- [ ] **Oficina de Caça** — profissões: Caçador; produção: carne, couro; expande materiais de crafting

---

## Cidade — Gestão Autônoma (Fase 3C — não implementar antes de 3B estável)

⚠️ Sistemas de IA dependem de economia e combate equilibrados. Implementar só após Fase 3B validada.

- [ ] Auto-alocação por confiança seguindo a política ativa
- [ ] `/cidade politica <foco>` (recursos / producao / combate / equilibrio)
- [ ] `/cidade otimizar`
- [ ] Prioridade por construção (`/cidade prioridade`)
- [ ] Cadeia de dependência inteligente
- [ ] `/cidade cadeia <prédio>`
- [ ] `/cidade orçamento` — define % global de produção/pesquisa/manutenção/reserva; sistema usa isso para decidir prioridades

---

## Sistema de Conversão de Heróis (Fase 3B-2)

- [ ] Entidade/serviço `ConversaoHeroiService`
- [ ] **Venda**: `Valor = BaseRaridade × EscalaDeNivel × FatorDeEscassezGlobal` — raridade pesa mais que nível; evita "farm de XP → venda → ouro infinito"
- [ ] Bloqueios anti-exploit na Venda: herói equipado / em missão / na Torre / alocado em prédio = bloqueado
- [ ] **Absorção**: consome herói, transfere 50% do XP acumulado para herói-alvo
- [ ] Bloqueios anti-exploit na Absorção: mesmos critérios de Venda
- [ ] `/heroi vender <herói>` com confirmação explícita (ação irreversível)
- [ ] `/heroi absorver <alvo> <consumido>` com confirmação

---

## Sistema de Sustento ✅ MVP concluído (Fase 3B-4 — v3.2.0)

`EstadoSustento` enum, campos em `Heroi` e `Cidade`, migration `SustentoSystem`, `SustentoService`, ícones no `HeroisPanel`, linha no `CidadePanel`, toggle via botão.

- [x] Campo `EstadoSustento` na entidade `Heroi` (enum: Ativo / Instavel / Degradado / Inativo)
- [x] Campo `UltimoSustentoEm` em `Cidade`; migration `20260423200000_SustentoSystem`
- [x] `SustentoService.ProcessarAsync(ulong)` — deduz Comida acumulada (flat 1/h por herói ativo, cap 24h), recalcula estado
- [x] `SustentoService.ToggleInativoAsync(Guid)` — alterna Inativo ↔ Ativo; herói Inativo não consome Comida
- [x] `SustentoService.ObterResumo(static)` — consumo/hora e horas restantes para exibição
- [x] `HeroisPanel`: ícone de estado (`✅⚠️🔴💤`) em cada herói; campo "Sustento" no detalhe; botão "Pausar / Ativar Sustento"
- [x] `CidadePanel`: linha de sustento abaixo do Humor — `✅ X 🌾/h | Estoque: Y | ~Z.Zh restantes`
- [x] `CommandHandler`: `ProcessarAsync` chamado em todo slash command; routing `herois_toggle_inativo`
- [ ] Consumo variável: `Base × Raridade × (1 + Nivel/100)` *(flat no MVP)*
- [ ] Consumo maior por classe (Guerreiro/Tanque > Mago/Suporte) *(deferido)*
- [ ] Limite de Moradia via prédio Alojamento *(deferido)*
- [ ] Estado **Instável**: penalidades de -% atributos e -% XP *(informativo apenas no MVP)*
- [ ] Estado **Degradado**: habilidades desativadas; risco de deserção *(informativo apenas no MVP)*

---

## Crafting

- [x] 5 receitas estáticas (Fase 3A): espada-ferro, arco-simples, armadura-couro, anel-arcano, amuleto-agilidade
- [x] `/crafting listar` e `/crafting fazer <receitaId>` (bot commands)
- [x] Check de qualidade: `skill_craft + bônus_prédio(Nivel×2) + roll(1..20)` via Responsável da Forja
- [ ] Forja produzindo equipamentos passivamente (Fase 3B)
- [ ] Laboratório produzindo poções usadas automaticamente na Torre
- [ ] Laboratório produzindo poções
- [ ] Poções usadas automaticamente na Torre
- [ ] Blueprints desbloqueáveis via missões ou drops
- [ ] Confiança do responsável desbloqueia blueprints raros (Confiança ≥ 71 → Mestre da Forja)

---

## Missões (Guilda) — Fase 3B-5

- [ ] Entidade de rank da Guilda (Ferro → Oricalco, 15 tiers)
- [ ] Geração automática de missões a cada 6h (até 8 simultâneas)
- [ ] Herói parte → state machine: `Aguardando → EmMissao → Retornando → Concluida`
- [ ] Tipos: Coleta, Subjugação, Escolta, Transporte, Investigação, Recuperação
- [ ] Cálculo de sucesso/parcial/falha por poder vs dificuldade
- [ ] **Fail interesting**: missão falhou → pode gerar evento secundário (herói capturado → nova dungeon)
- [ ] `/cidade missoes` e `/cidade missoes enviar`

---

## Sistema de Mercado P2P (Fase 3B-Mercado — após 3B-1)

**Decisões:** `/mercado` efêmero; canal `#mercado` público; Phase 1 = equipamentos + consumíveis; taxa de listagem 5 Ouro flat; taxa de venda 10%; limite 3 listagens/player; 24h expiração; sem cap global no canal; bot edita mensagem antes de deletar.

### Step 1 — MVP
- [ ] Entidade `MarketListing` (Id, SellerId, InventoryEntryId, ItemType, ItemName, Quantity, PricePerUnit, TotalPrice, TaxRate, Status, CreatedAt, ExpiresAt, ResolvedAt, BuyerId, DiscordMessageId, RowVersion)
- [ ] Entidade `MarketSaleHistory` (Id, ItemTemplateId, SalePrice, Quantity, SoldAt) — append-only
- [ ] `IsLocked` + `LockReason` no `InventarioEntry` (adicionado ao Inventário Unificado)
- [ ] `MarketConfig` data-driven: `BaseTaxRate=0.10`, `ListingFeeOuro=5`, `MaxActiveListings=3`, `ListingDurationHours=24`, `MinPriceFactor=0.5`, `MaxPriceFactor=10.0`, `PublicSaleThreshold=500`
- [ ] `MarketService.ListItemAsync`: valida elegibilidade + lock + desconta 5 Ouro + cria listing
- [ ] `MarketService.BuyItemAsync`: pipeline de validação + `BEGIN IMMEDIATE TRANSACTION` + transferência atômica
- [ ] `MarketService.CancelListingAsync`: desbloqueia item, atualiza status, agenda delete da mensagem
- [ ] `MarketExpiryWorker`: roda a cada 10 min; resolve Active listings com `ExpiresAt <= now`
- [ ] `MarketBoardService`: `PostListingAsync`, `EditSoldAsync` (badge + desabilita botão), `EditExpiredAsync`, `DeleteAfterDelayAsync`
- [ ] `MarketInteractionHandler`: rota `mercado:comprar:{id}`, `mercado:comprar_confirmar:{id}`, `mercado:cancelar:{id}`, `mercado:vender`, `mercado:minhas_listagens`
- [ ] `/mercado` painel efêmero: `[Vender Item]` + `[Minhas Listagens]`
- [ ] Flow de venda: Select Menu (itens não-locked, elegíveis) → Modal de preço → confirmação com cálculo de taxa → post no canal
- [ ] Flow de compra: `[Comprar]` no canal → check validação → confirmação efêmera → `[Confirmar Compra]` → transaction

### Step 2 — UX Polish
- [ ] `MarketSaleHistory` populado em cada venda
- [ ] Média recente exibida na mensagem (últimas 5 vendas do mesmo `ItemTemplateId`)
- [ ] Post público no canal para vendas acima de `PublicSaleThreshold`
- [ ] Notificação "Evento ao Logar" ao vendedor quando item for vendido
- [ ] Board Summary pinado no `#mercado` (editado a cada transação)
- [ ] `[Ver Detalhes]` na listagem: ephemeral com stats completos do item

### Step 3 — Integração e Extensão
- [ ] `MarketService.GetTaxRateForPlayer()` consulta nível do prédio Mercado (nível 1→8%, 2→6%, 3→3%)
- [ ] `ItemConfig.IsTradeable` (bool) — controle por tipo sem switch/if
- [ ] Suporte a Consumíveis com quantidade (lote único por listagem)
- [ ] Suporte a recursos brutos com stacking (madeira, pedra, minério)
- [ ] Relíquias tradeable (quando 3B-6 estável)

---

## Sistema de Mercenários + Treinamento (Fase 3B-Merc / 3B-Treino — após Market MVP)

Infraestrutura compartilhada entre os dois sistemas. `HeroSnapshot` e `HeroLockStatus` são o núcleo. Implementar Mercenários primeiro; Treinamento reutiliza o mesmo snapshot model.

### Step 1 — Domain + Entities (compartilhado)
- [ ] Estender `HeroLockStatus` enum: + `AsMercenary`, `InTraining`, `AsTrainer`
- [ ] Adicionar `LockStatus` + `LockReason` em `Heroi`
- [ ] Criar `IHeroCombatant` interface no Domain (Id, Nome, Nivel, stats, PowerScore)
- [ ] Implementar `IHeroCombatant` em `Heroi`
- [ ] Criar `HeroSnapshot` entity + `SnapshotTipo` enum (Mercenario | Treinamento)
- [ ] Implementar `IHeroCombatant` em `HeroSnapshot`
- [ ] Criar `EmprestimoHeroi` entity + `EmprestimoStatus` + `DuracaoOpcao` enums
- [ ] Criar `TreinamentoHeroi` entity + `TreinamentoStatus` enum
- [ ] Criar `MercenarioConfig` data-driven (keyed DuracaoOpcao): 6h/12h/24h; CustoBase, CustoPorNivel
- [ ] Criar `TreinamentoConfig` data-driven (keyed ArenaRank 0–5): XpBonus%, MaxSlots, DuracaoHoras, NpcEficiência%
- [ ] EF migrations para todas as novas entidades

### Step 2 — Application Services — Mercenários
- [ ] `SnapshotService.CaptureAsync(heroiId, tipo, expiresAt)` — write-once snapshot
- [ ] `SnapshotService.GetActiveForContratante(playerId)` → `IHeroCombatant?`
- [ ] `MercenarioService.DisponibilizarAsync(playerId, heroiId, duracao)` — cria snapshot + lock herói + cria listing
- [ ] `MercenarioService.ContratarAsync(playerId, emprestimoId)` — validações anti-exploit + `BEGIN IMMEDIATE TRANSACTION`
  - Validações: não próprio herói, limite 1 ativo, anti-chain bilateral, same-pair 24h, ouro suficiente
- [ ] `MercenarioService.CancelarAsync(donoId, emprestimoId)` — unlock herói, atualiza status
- [ ] `MercenarioService.GetDisponiveisAsync()` → lista para board

### Step 3 — Application Services — Treinamento
- [ ] `TreinamentoService.OferecerrTreinoAsync(treinadorId, heroiId)` — valida ArenaRank ≥ 1, cria snapshot, lock herói treinador
- [ ] `TreinamentoService.EnviarHeroiAsync(alunoId, studentHeroId, snapshotId)` — `BEGIN IMMEDIATE TRANSACTION`
  - Validações: aluno ≠ treinador, anti-chain bilateral, same-pair 24h, cap semanal, slots disponíveis
  - XpCap frozen na criação: `CalcWeeklyCap(heroi) - XpSemanaAcumulado(heroi)`
- [ ] `TreinamentoService.ConcluirTreinamentoAsync(treinamentoId)` — calcula XP, aplica via HeroiLevelUpService, unlock aluno, conditional unlock treinador
  - Fórmula: `min(TrainerPS × XpFatorConfig × Horas × (1 + ArenaBonus%), XpCap)`
- [ ] `TreinamentoService.GetAtivosAsync()` → lista para board

### Step 4 — Background Workers
- [ ] `MercenarioExpiryWorker` (IHostedService, poll 5 min):
  - `EmprestimoHeroi.ExpiresAt < Now` → Status = Expirado → heroi.LockStatus = None
  - Edit `#mercenarios` message: "[EXPIRADO]" + disable buttons → delete after 10 min
  - Queue notification para Dono + Contratante (exibe no próximo comando)
- [ ] `TreinamentoExpiryWorker` (IHostedService, poll 5 min):
  - `TreinamentoHeroi.ExpiresAt < Now` → calcular XP → HeroiLevelUpService.AddXpAsync → unlock aluno
  - Se treinador sem sessões ativas → unlock herói treinador
  - Status = Concluido, XpGanho preenchido → queue notification ao aluno

### Step 5 — Adaptar combat services
- [ ] `CombatService`: aceitar `IHeroCombatant` em vez de `Heroi` direto nos parâmetros
- [ ] `TorreService`: buscar snapshot ativo (`SnapshotService.GetActiveForContratante`) antes de montar combate
- [ ] `MissaoService`: idem — verificar mercenário ativo
- [ ] `ArenaService`: **não alterar** — permanece usando `Heroi` diretamente (mercenários proibidos na Arena)

### Step 6 — Bot layer + Slash commands
- [ ] `MercenarioPanelBuilder`: painel efêmero + card de board (`#mercenarios`)
- [ ] `MercenarioInteractionHandler`: rotas `mercenario:disponibilizar`, `mercenario:contratar:{id}`, `mercenario:cancelar:{id}`
- [ ] `/mercenarios` → painel efêmero: `[Disponibilizar Herói]` `[Buscar Mercenário]` `[Meu Empréstimo]`
- [ ] `TreinamentoPanelBuilder`: painel efêmero + card de board (`#treinamento`)
- [ ] `TreinamentoInteractionHandler`: rotas `treinamento:oferecer`, `treinamento:enviar:{snapshotId}`, `treinamento:cancelar:{id}`
- [ ] `/treinamento` → painel efêmero: `[Oferecer Treino]` `[Enviar Herói]` `[Meus Treinos]`

### Step 7 — Guards e integrações
- [ ] Guard `LockStatus == None` em absorb/sell/equip: estender padrão existente do market para novos valores
- [ ] Weekly XP cap via aggregate query em `TreinamentoHeroi` (sem campo extra em `Heroi`)
- [ ] Seed NPC mercenaries: 3 `EmprestimoHeroi` com `IsNpc=true` por tier em `DbSeeder`
- [ ] Seed NPC trainers: 1 entry por ArenaRank com flag NPC em config
- [ ] Recovery job no startup: scan heróis com `LockStatus != None` e `ExpiresAt < Now` → force unlock + warn log

---

## Arena

- [x] `/treinar <herói>` — XP em burst (3× XpParaProximoNivel), 4h cooldown, 100 Ouro + 10 Comida (`ArenaService.TreinarAsync`)
- [x] `/arena desafio` — desafio de ondas com cooldown 24h, top-5 heróis automático (`ArenaService.DesafioOndasAsync`)
- [ ] Sistema de Prestígio e títulos honoríficos

---

## Meta Progressão (Fase 3B)

- [ ] **Nível do Mestre** — progride com atividade global (pulls, andares, produções); desbloqueia bônus passivos (ex: +1% XP global por nível); exibido no perfil
- [ ] **Bônus de Composição de Party** — 3 da mesma raça → +10% XP; full arqueiros → +crit chance; party balanceada (1 de cada arquétipo) → bônus misto
- [ ] **Decisões irreversíveis leves**: ao ascender para 4★, herói ganha 1 traço fixo escolhido pelo jogador (ex: "Incansável" → +5% XP sempre; "Pragmático" → +10% gold sempre) — cria identidade de herói

---

## Qualidade de Código — Fase Q (prioridade antes da 3B)

**Fase Q (fazer primeiro):**
- ~~`Random.Shared` no `GachaService`~~ *(GachaService removido)*
- [ ] `ILogger<T>` substituindo `Console.WriteLine` nos serviços (parcial — CommandHandler já usa logging estruturado)
- [ ] Guild ID movido para `appsettings.json`
- [ ] Caminho do banco via variável de ambiente ou relativo
- [ ] **Guard clauses centralizadas** — padrão único para herói em missão / alocado / inativo / equipado
- ~~Testes unitários: GachaService~~ *(removido)*
- [ ] Testes unitários: HeroiLevelUpService (grants, totais, caps)
- [ ] Testes unitários: CombatService (turnos, dano, crit)
- [ ] Testes unitários: Produção passiva da cidade
- [ ] **Testes de integração**: fragmentos → recrutar → alocar → evoluir (SQLite in-memory)

**Carry-over (Fase 3.5):**
- [x] Token via variável de ambiente
- [x] Clean Architecture com separação real de camadas
- [x] Nullable warnings corrigidos (0 warnings)
- [x] Migração para .NET 10
- [x] `RaridadeConfig` — sem números mágicos no sistema de progressão
- [ ] **`IRandomProvider` interface** — encapsula todo RNG; permite seed controlado para testes e debug; evitar `Random` solto em múltiplos serviços
- [ ] **`TimeProvider` centralizado** — injetar `ITimeProvider` nos serviços que usam tempo (produção, missões, torre); permite testar sem `DateTime.Now` direto
- [ ] **Transações EF Core** nas operações críticas (craft, ascensão, venda, absorção): validar → executar → commit; rollback em falha
- [ ] **State machines para fluxos longos** — missões e Torre Operação modelados como `estado + transição` persistido; garante retomada após restart do bot
- [ ] **Separação explícita Domain/Application/Discord** — Commands apenas orquestram; Services não conhecem Discord; verificar e corrigir onde acoplado
- [ ] Idempotência nos comandos de coleta (evita duplicar recursos se spamado)
- [ ] **Configuração central de balanceamento** — taxas de drop, custo de ascensão, produção base em JSON/tabela; evitar rebuild para ajuste de números (além do `RaridadeConfig` existente)
- [ ] **Validação e sanitização de input** — limites em apelidos, URLs de arte, strings maliciosas; guard em todos os slash commands

---

## Comandos Admin / Debug (Fase 3.5)

Essenciais para desenvolvimento e beta — aceleram debug enormemente.

- [ ] `/admin resetar_cidade <userId>` — reseta cidade para estado inicial
- [ ] `/admin dar_recursos <userId> <tipo> <qtd>` — injeta recursos para teste
- [ ] `/admin spawn_heroi <userId> <raridade>` — spawna herói com raridade definida
- [ ] `/admin ver_estado <userId>` — dump completo do estado do jogador
- [ ] `/admin forcar_nivel <heroiId> <nivel>` — avança nível para testar caps
- [ ] Sistema de permissão admin (IDs autorizados em `appsettings.json`)
- [ ] **Evento ao logar** — ao executar qualquer comando, bot verifica e exibe 1 item pendente relevante: missão concluída, alerta de cidade, herói subiu nível, decisão pendente na Torre

---

## Infraestrutura

- [x] Repositório no GitHub
- [x] `.gitignore` cobrindo `.claude/`, `.idea/`, binários, `*.db-shm`, `*.db-wal`, arquivos pessoais Claude
- [ ] Bot rodando em servidor externo (VPS ou similar)
- [ ] Variável de ambiente configurada no servidor
- [ ] Script de deploy automatizado
- [ ] **Backup automático do SQLite** (diário, arquivo rotacionado)
- [ ] **Definir estratégia de reset para beta** — wipe total / reset parcial (recursos, não heróis) / compensação com bônus
