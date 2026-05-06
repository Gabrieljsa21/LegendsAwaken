# Legends Awaken — Backlog

Tarefas pendentes por área. Para progresso macro de fases, veja `ROADMAP.md`.

---

## Alta Prioridade — Fase Q (fazer antes de 3B)

- [ ] `ILogger<T>` substituindo `Console.WriteLine` nos serviços (parcial — CommandHandler já usa logging estruturado)
- [ ] Guild ID movido para `appsettings.json`; caminho do banco via variável de ambiente ou relativo
- [ ] Guard clauses centralizadas — padrão único para herói em missão / alocado / inativo / equipado
- [ ] Testes unitários: `HeroiLevelUpService` (grants, totais, caps)
- [ ] Testes unitários: `CombatService` (turnos, dano, crit)
- [ ] Testes unitários: produção passiva da cidade
- [ ] Teste de integração: fragmentos → recrutar → alocar → evoluir (SQLite in-memory)

---

---

## 3B — Torre

### 3B-ItemUnico — Item Único por Andar (ponto crítico T3)
- [ ] `AndarItem` entity — `AndarNumero`, `ItemConfigId`, `Nome`, `Tipo`, `Efeito`
- [ ] `AndarItemConfig` seed — 1 item por andar documentado (seed inicial: andares 1–25)
  - [ ] Equipamento com efeito específico (crit, sustain, speed — não apenas "mais forte")
  - [ ] Item de nicho (forte para certos heróis/builds, irrelevante para outros)
  - [ ] Consumível estratégico (buff temporário poderoso)
  - [ ] Componente de crafting raro (ingrediente de recipe avançada — cria demanda de andar)
  - [ ] Item de progressão (usado em ascensão futura — cria demanda de longo prazo)
- [ ] `TorreOperacaoConfig.ObterProducao` — retornar item do andar além do recurso
- [ ] `TorreModoOperacaoPanel.CriarBoard` — exibir nome/tipo do item de cada andar no board
- [ ] `CriarSeletorAndar` — mostrar item ao lado do andar no Select Menu de alocação
- [ ] `TorreOperacaoService.ConcluirOperacao` — adicionar item ao inventário do jogador

### 3B-TorreExp — Torre Exploração Avançada (T0)

#### Investigação do Andar
- [ ] Botão `[Investigar]` no `TorrePanel` ou no seletor de andar da Exploração
- [ ] Nível Básico — chance de vitória: `teamPS / andarDificuldade` (clamp 5–95%), exibida antes de confirmar
- [ ] Nível Intermediário — fraquezas elementais do andar
- [ ] Nível Avançado — distribuição de checkpoints + modificadores ativos do andar
- [ ] `AndarConfig` ou campo `Dificuldade` em `TorreAndar` para alimentar o cálculo

#### Checkpoints de Loot
- [ ] Campo `ProgressoAtual` (0–100) em `TorreExploracao`
- [ ] Config de checkpoints por andar (ex: `{25, 50, 75, 100}`)
- [ ] Ao atingir checkpoint: gerar e garantir loot daquele intervalo
- [ ] Ao falhar: preservar loot até último checkpoint; perder intervalo atual
- [ ] `TorreCheckpointLoot` entity + repositório + migration
- [ ] Integração com `TorreExploracaoService.ProcessarAsync`

#### Heróis Feridos ao Falhar
- [ ] Ao falhar: heróis do time marcados como feridos (`FerimentoAt`, `HorasRecuperacao`)
- [ ] Bloqueio em Torre, Arena e Missões enquanto ferido
- [ ] Tick de cura baseado em tempo (4–12h por raridade) em `SustentoService` ou `CuraService`
- [ ] Ícone `🩹` em `HeroisPanel` + ETA de cura

### 3B-RotacaoInteresse — Rotação de Interesse (T3)
- [ ] Crafting avançado exige componentes de biomas diferentes (recipe Tier II requer drop Bioma A + Bioma B)
- [ ] Eventos semanais pedem drop específico de andar rotativo
- [ ] Items de progressão (ascensão, relíquias) consomem drop de andar específico

### Torre — Pendências do MVP
- [ ] Estado `AndarConcluido` por usuário + andar (não exigido no MVP — qualquer andar operável atualmente)
- [ ] Escala de inimigos por andar: `stats = base × (1 + 0.08)^floor`; calibrar com tabela CDI
- [ ] Drops de materiais de crafting em andares de boss
- [ ] Drops de fragmentos de personagens fixos como drop raro

---

## 3B — Cidade

### 3B-BoostCidade — Boosters da Cidade (T1)
- [ ] Enum `TipoBoosterCidade`: Producao, Rendimento, Eficiencia, Qualidade, Especializacao, Conversao
- [ ] Entidade `BoosterCidadeAtivo` — `UsuarioId`, `Tipo`, `IniciadoEm`, `DuracaoMinutos`, `Valor`
- [ ] `BoosterCidadeService.AtivarAsync` — valida 1 ativo; desativa anterior se troca
- [ ] `BoosterCidadeService.ObterAtivoAsync` + `AplicarEfeitoAsync(multiplier)`
- [ ] Hook em `CidadeService.ColetarProducaoAsync` — aplica booster ativo
- [ ] Crafting recipes de boosters: materiais da Torre como ingredientes
- [ ] `BoosterCidadeConfig` data-driven (efeito, duração, custo por tipo)
- [ ] Painel `/cidade` — booster ativo com duração restante; botão `[Ativar Booster]`

### 3B-7 — Novos Prédios
- [ ] **Armazém** — limite de estoque; overflow converte automaticamente (ex: madeira excedente → ouro a 80%)
- [ ] **Mercado** — conversão de recursos em ouro; taxas melhoram com upgrade
- [ ] **Prefeitura** — limites globais; pré-requisito para prédios avançados
- [ ] **Quartel** — profissões: Guerreiro, Paladino; produção: buffs para Torre
- [ ] **Academia** — profissões: Pesquisador; produção: XP global / passivas; desbloqueia perks permanentes
- [ ] **Tesouro** — armazena ouro; protege contra eventos negativos; juros de 5% a cada 60 dias com 10+ guardas
- [ ] **Torre de Vigilância** — melhora previsibilidade da Torre; aumenta chance de encontrar segredos
- [ ] **Pedreira** — produção: pedra em volume; separa pedra de minério
- [ ] **Santuário** — ativa buffs temporários (+drop, +XP, +evento raro)
- [ ] **Oficina de Caça** — produção: carne, couro; expande materiais de crafting
- [ ] **Moinho** — processa recursos brutos dos biomas em materiais refinados; liga Exploração↔Cidade
- [ ] **Jardim** — ciclo de 30 dias; produz ingredientes para poções (check de craft CD 10/15/20; falha destrói lote)
- [ ] Upgrades de prédio nível 2→3
- [ ] Slots de Liderança: herói com Confiança ≥ 61 → +10% produção global do prédio
- [ ] Confiança desbloqueia funções avançadas (ex: Forja nível 71+ → Mestre da Forja)

### Cidade — Pendências de MVP
- [ ] Upgrades de prédio nível 1→2

### Cidade — Sustento (deferidos do MVP)
- [ ] Consumo variável: `Base × Raridade × (1 + Nivel/100)` (flat no MVP atual)
- [ ] Consumo maior por classe (Guerreiro/Tanque > Mago/Suporte)
- [ ] Limite de Moradia via prédio Alojamento
- [ ] Estado Instável: penalidades de -% atributos e -% XP (informativo apenas no MVP)
- [ ] Estado Degradado: habilidades desativadas; risco de deserção (informativo apenas no MVP)

---

## 3B — Inventário e Relíquias

### 3B-1 — Inventário Unificado (pré-requisito para Relíquias e Mercado)
- [ ] `Inventario` — entidade unificada por jogador; tipos: `Recurso | Item | Reliquia | Consumivel`
- [ ] Operações atômicas: `Add(tipo, id, qtd)` / `Remove(tipo, id, qtd)` com validação prévia
- [ ] Stack limit por tipo (relíquias não stackam; recursos sim)
- [ ] `/inventario` — painel com abas por categoria (`[Equipamentos] [Relíquias] [Recursos] [Consumíveis]`) e paginação

### 3B-6 — Relíquias (requer Inventário Unificado)
- [ ] Entidade `Reliquia` (Id, Nome, Descrição, Efeito, AndarMinimo)
- [ ] 3 slots de relíquia por herói (`HeroiReliquia`)
- [ ] Drop de relíquias em boss floors da Torre
- [ ] Equipar/remover via painel `/inventario`
- [ ] Aplicar efeito passivo da relíquia no combate via interface `IEfeitoReliquia`

---

## 3B — Heróis e Progressão

### 3B-2 — Conversão de Heróis
- [ ] `ConversaoHeroiService`
- [ ] **Venda**: `Valor = BaseRaridade × EscalaDeNivel × FatorDeEscassezGlobal`
- [ ] Bloqueios anti-exploit: herói equipado / em missão / na Torre / alocado = bloqueado
- [ ] **Absorção**: consome herói, transfere 50% do XP acumulado para herói-alvo
- [ ] `/heroi vender <herói>` com confirmação explícita (ação irreversível)
- [ ] `/heroi absorver <alvo> <consumido>` com confirmação

### Heróis — Pendências Gerais
- [ ] Exibir equipamentos no `/ver_heroi`
- [ ] Apelido e arte customizada (`/heroi apelido`, `/heroi arte`)
- [ ] Gerar arte IA para cada personagem fixo e registrar URLs

### Ascensão de Heróis
- [ ] Sistema de fragmentos de arquétipo (entidade, acúmulo por atividade)
- [ ] Ascensão: consumir fragmentos + materiais, aplicar grant, subir raridade
- [ ] `/heroi ascender` com verificação de cap e custo

### Combate — Pendências Core
- [ ] Sistema de ameaça (aggro): tanques/paladinos atraem mais ataques
- [ ] Power Score: implementar `HeroPowerScore` para sucesso/falha de missões (`DESIGN_SISTEMAS.md §2`)
- [ ] Posicionamento leve (front/back): melee → front; ranged/mago → back
- [ ] IA tática: atacar menor HP (DPS focus) ou maior ameaça (proteção de suporte)
- [ ] Sinergia de habilidades: pelo menos 1 combo básico (ex: fogo + vento = área)
- [ ] `IRandomProvider` interface — encapsula RNG com seed controlado (replay + debug determinístico)

---

## 3B — Missões (Guilda)

### 3B-5 — Guilda / Missões (requer HeroPowerScore)
- [ ] Entidade de rank da Guilda (Ferro → Oricalco, 15 tiers)
- [ ] Geração automática de missões a cada 6h (até 8 simultâneas)
- [ ] State machine: `Aguardando → EmMissao → Retornando → Concluida`
- [ ] Tipos: Coleta, Subjugação, Escolta, Transporte, Investigação, Recuperação
- [ ] Cálculo de sucesso/parcial/falha por poder vs dificuldade
- [ ] Fail interesting: missão falhou → evento secundário (ex: herói capturado → nova dungeon)
- [ ] Painel `/guilda` com botões `[Missões Ativas]` `[Missões Disponíveis]` `[Enviar Heróis]`
- [ ] Ganho de XP em missões da Guilda
- [ ] Ganho de XP passivo na Cidade (menor que Torre)

---

## 3B — Mercado P2P (após Inventário Unificado)

### Step 1 — MVP
- [ ] Entidade `MarketListing` (Id, SellerId, InventoryEntryId, ItemType, ItemName, Quantity, PricePerUnit, TotalPrice, TaxRate, Status, ExpiresAt, BuyerId, DiscordMessageId, RowVersion)
- [ ] Entidade `MarketSaleHistory` (Id, ItemTemplateId, SalePrice, Quantity, SoldAt) — append-only
- [ ] `IsLocked` + `LockReason` no `InventarioEntry`
- [ ] `MarketConfig` data-driven: `BaseTaxRate=0.10`, `ListingFeeOuro=5`, `MaxActiveListings=3`, `ListingDurationHours=24`, `MinPriceFactor=0.5`, `MaxPriceFactor=10.0`
- [ ] `MarketService.ListItemAsync` — valida elegibilidade + lock + desconta 5 Ouro + cria listing
- [ ] `MarketService.BuyItemAsync` — validação + `BEGIN IMMEDIATE TRANSACTION` + transferência atômica
- [ ] `MarketService.CancelListingAsync` — desbloqueia item, atualiza status
- [ ] `MarketExpiryWorker` — roda a cada 10 min; resolve listagens expiradas
- [ ] `MarketBoardService` — `PostListingAsync`, `EditSoldAsync`, `EditExpiredAsync`, `DeleteAfterDelayAsync`
- [ ] `/mercado` painel efêmero: `[Vender Item]` + `[Minhas Listagens]`
- [ ] Flow de venda: Select Menu → Modal de preço → confirmação com cálculo de taxa → post no canal
- [ ] Flow de compra: `[Comprar]` → confirmação efêmera → `[Confirmar Compra]` → transaction

### Step 2 — UX Polish
- [ ] Média recente exibida na mensagem (últimas 5 vendas do mesmo `ItemTemplateId`)
- [ ] Post público no canal para vendas acima de threshold (500 Ouro)
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

## 3B — Mercenários + Treinamento (após Market MVP)

### Step 1 — Domain + Entities (compartilhado)
- [ ] Estender `HeroLockStatus` enum: + `AsMercenary`, `InTraining`, `AsTrainer`
- [ ] Adicionar `LockStatus` + `LockReason` em `Heroi`
- [ ] Criar `IHeroCombatant` interface no Domain (Id, Nome, Nivel, stats, PowerScore)
- [ ] Implementar `IHeroCombatant` em `Heroi` e em `HeroSnapshot`
- [ ] Entidade `HeroSnapshot` + `SnapshotTipo` enum (Mercenario | Treinamento)
- [ ] Entidade `EmprestimoHeroi` + `EmprestimoStatus` + `DuracaoOpcao` enums
- [ ] Entidade `TreinamentoHeroi` + `TreinamentoStatus` enum
- [ ] `MercenarioConfig` data-driven: 6h/12h/24h; CustoBase, CustoPorNivel
- [ ] `TreinamentoConfig` data-driven (keyed ArenaRank 0–5): XpBonus%, MaxSlots, DuracaoHoras, NpcEficiência%
- [ ] EF migrations para todas as novas entidades

### Step 2 — Application Services — Mercenários
- [ ] `SnapshotService.CaptureAsync(heroiId, tipo, expiresAt)` — write-once snapshot
- [ ] `MercenarioService.DisponibilizarAsync` — cria snapshot + lock herói + cria listing
- [ ] `MercenarioService.ContratarAsync` — validações anti-exploit + `BEGIN IMMEDIATE TRANSACTION`
- [ ] `MercenarioService.CancelarAsync` — unlock herói, atualiza status
- [ ] `MercenarioService.GetDisponiveisAsync()` → lista para board

### Step 3 — Application Services — Treinamento
- [ ] `TreinamentoService.OferecerrTreinoAsync` — valida ArenaRank ≥ 1, lock herói treinador
- [ ] `TreinamentoService.EnviarHeroiAsync` — validações: anti-chain bilateral, same-pair 24h, cap semanal
- [ ] `TreinamentoService.ConcluirTreinamentoAsync` — calcula XP, aplica, unlock; fórmula: `min(TrainerPS × XpFatorConfig × Horas × (1 + ArenaBonus%), XpCap)`

### Step 4 — Background Workers
- [ ] `MercenarioExpiryWorker` (IHostedService, poll 5 min): expire → unlock herói → edit+delete mensagem
- [ ] `TreinamentoExpiryWorker` (IHostedService, poll 5 min): calcular XP → AddXpAsync → unlock aluno

### Step 5 — Adaptar combat services
- [ ] `CombatService` e `TorreService`: aceitar `IHeroCombatant` em vez de `Heroi` direto
- [ ] `MissaoService`: idem — verificar mercenário ativo
- [ ] `ArenaService`: manter usando `Heroi` (mercenários proibidos na Arena)

### Step 6 — Bot layer
- [ ] `MercenarioPanelBuilder` + `MercenarioInteractionHandler`
- [ ] `/mercenarios` painel efêmero: `[Disponibilizar Herói]` `[Buscar Mercenário]` `[Meu Empréstimo]`
- [ ] `TreinamentoPanelBuilder` + `TreinamentoInteractionHandler`
- [ ] `/treinamento` painel efêmero: `[Oferecer Treino]` `[Enviar Herói]` `[Meus Treinos]`

### Step 7 — Guards e integrações
- [ ] Guard `LockStatus == None` em absorb/sell/equip
- [ ] Weekly XP cap via aggregate query em `TreinamentoHeroi`
- [ ] Seed NPC mercenaries: 3 `EmprestimoHeroi` com `IsNpc=true` por tier
- [ ] Recovery job no startup: force unlock heróis com `LockStatus != None` e `ExpiresAt < Now`

---

## 3B — Meta Progressão

- [ ] **Nível do Mestre** — progride com atividade global; desbloqueia bônus passivos (ex: +1% XP global por nível)
- [ ] **Bônus de Composição de Party** — 3 da mesma raça → +10% XP; full arqueiros → +crit; party balanceada → bônus misto
- [ ] **XP por 3 Pilares** — exploração de biomas novos e negociação com NPCs de alto rank também dão XP escalado
- [ ] **Traço fixo na ascensão 4★** — jogador escolhe 1 traço permanente (ex: "Incansável" → +5% XP; "Pragmático" → +10% gold)

---

## Arena

- [ ] Sistema de Prestígio e títulos honoríficos

---

## Crafting

- [ ] Forja produzindo equipamentos passivamente (Fase 3B)
- [ ] Laboratório produzindo poções usadas automaticamente na Torre
- [ ] Blueprints desbloqueáveis via missões ou drops
- [ ] Confiança do responsável desbloqueia blueprints raros (Confiança ≥ 71 → Mestre da Forja)

---

## Qualidade de Código — Fase 3.5

- [ ] `IRandomProvider` interface — encapsula todo RNG; permite seed controlado para testes e debug
- [ ] `TimeProvider` centralizado — injetar `ITimeProvider` nos serviços que usam tempo (produção, missões, torre)
- [ ] Transações EF Core nas operações críticas (craft, ascensão, venda, absorção): validar → executar → commit; rollback em falha
- [ ] State machines para fluxos longos — missões e Torre Operação modelados como `estado + transição` persistido
- [ ] Separação explícita Domain/Application/Discord — Commands apenas orquestram; Services sem tipos Discord
- [ ] Idempotência nos comandos de coleta (evita duplicar recursos se spamado)
- [ ] Configuração central de balanceamento — taxas de drop, custo de ascensão, produção base em JSON/tabela
- [ ] Validação e sanitização de input — limites em apelidos, URLs de arte; guard em todos os slash commands
- [ ] (Beta) Migrar curva de XP para `B_r × nível^1.25` após coleta de dados reais

---

## Comandos Admin / Debug — Fase 3.5

- [ ] `/admin resetar_cidade <userId>` — reseta cidade para estado inicial
- [ ] `/admin dar_recursos <userId> <tipo> <qtd>` — injeta recursos para teste
- [ ] `/admin spawn_heroi <userId> <raridade>` — spawna herói com raridade definida
- [ ] `/admin ver_estado <userId>` — dump completo do estado do jogador
- [ ] `/admin forcar_nivel <heroiId> <nivel>` — avança nível para testar caps
- [ ] Sistema de permissão admin (IDs autorizados em `appsettings.json`)
- [ ] **Evento ao logar** — bot verifica e exibe 1 item pendente relevante em qualquer comando

---

## Fase 3C — IA e Automação (não implementar antes de 3B estável)

- [ ] `/cidade politica <foco>` (recursos / producao / combate / equilibrio)
- [ ] Auto-alocação de heróis por confiança seguindo a política ativa
- [ ] `/cidade otimizar`
- [ ] Cadeia de dependência inteligente + `/cidade cadeia <prédio>`
- [ ] `/cidade orçamento` — define % global de produção/pesquisa/manutenção/reserva
- [ ] Slots de Liderança: herói Parceiro+ assume liderança, buffa operadores
- [ ] Torre — Progresso % por andar com requisito secreto opcional
- [ ] Torre — Zonas e identidade mecânica por andar (anti-cura, buffs aleatórios, etc.)
- [ ] Torre — Decisões automáticas do líder quando jogador não responde
- [ ] Torre — Memória do andar (NPCs/eventos lembram decisões anteriores)
- [ ] Torre — Overclear: 100% → 120% evento raro → 150% domínio total
- [ ] Torre — Estado do andar (Normal / Corrompido / Instável / Rico) muda ao longo do tempo
- [ ] Torre — Anti-meta rígida: andares que penalizam builds específicas

---

## Fase 3.5 — Infra & Observabilidade

- [ ] Telemetria mínima — eventos estruturados: `PlayerLoggedIn`, `HeroLeveledUp`, `ResourceCollected`, `MissionCompleted`; métricas de ouro/dia, XP médio, taxa de uso de heróis
- [ ] Feature flags — ligar/desligar sistemas sem deploy
- [ ] Configuração dinâmica — taxas de drop, curva de XP, custos configuráveis via JSON sem rebuild
- [ ] Anti-exploit e concorrência — lock por jogador (semáforo/mutex por UserId); idempotência em operações críticas
- [ ] Rate limiting — cooldown por comando crítico; fila de execução
- [ ] Fail-safe patterns — rollback em falha (EF Core Transaction); retry automático
- [ ] Versionamento de dados — versão do schema no banco; migrations controladas
- [ ] Sistema de log de auditoria — histórico de ações críticas por jogador (venda, ascensão, drop raro)
- [ ] Backup automático do SQLite (diário, arquivo rotacionado)

---

## Beta e Lançamento

- [ ] Onboarding — tutorial inicial guiado; primeira invocação garantida; primeira missão dirigida
- [ ] Bot rodando em servidor externo (VPS ou similar)
- [ ] Variável de ambiente configurada no servidor
- [ ] Script de deploy automatizado
- [ ] Definir estratégia de reset para beta — wipe total / reset parcial / compensação com bônus
- [ ] Teste de carga — simular múltiplos usuários, spam de comando, produção simultânea
- [ ] Considerar migração SQLite → PostgreSQL se volume justificar
- [ ] Changelog público
- [ ] README atualizado
