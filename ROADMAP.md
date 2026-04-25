# Legends Awaken — Roadmap

---

## Fase 1 — Pré-produção ✅ concluída
**Objetivo:** fechar o design antes de codar.

- [x] Conceito e visão geral definidos
- [x] Stack técnica escolhida (C#, Discord.Net, EF Core, SQLite)
- [x] Arquitetura Clean Architecture + DDD aplicada
- [x] GDD criado com sistemas de gacha, cidade, confiança, prioridade e cadeia de dependência
- [x] Escopo do v1.0 fechado
- [x] Pool inicial de personagens fixos 5★ e 4★ definida (3 lendários + 6 épicos)
- [x] Receitas e cadeia de dependência dos itens básicos documentadas no GDD
- [x] Sistema de progressão de nível (caps, ganhos, grants de ascensão) desenhado
- [x] Bônus raciais (+50 por raça não-humana, foco em atributo único) desenhados
- [x] Sistema de Relíquias desenhado (3 slots, removíveis/transferíveis)
- [x] Sistema de Missões com Guilda de 15 ranks desenhado
- [x] Arena e crafting com check de qualidade desenhados

**Sinal de saída:** GDD estável, sem features indefinidas no escopo do v1.0.

---

## Fase 2 — Protótipo da Cidade ✅ concluída
**Objetivo:** validar se o loop de gestão é divertido antes de polir.

- [x] Herói alocado produz recurso conforme profissão
- [x] `/cidade ver` mostrando recursos, prédios e heróis alocados
- [x] `/cidade coletar` calculando produção por tempo decorrido (cap 24h)
- [x] `/cidade alocar` e `/cidade desalocar` com autocomplete
- [x] Recurso Erva adicionado; `CidadeRepository` reescrito em EF Core
- [x] Distribuição de raças por raridade implementada no gacha
- [x] `HeroiLevelUpService` com `RaridadeConfig` — caps, ganhos e grants calculados
- [ ] Testar internamente se a mecânica de base é satisfatória

**Sinal de saída:** loop invocar → alocar → coletar jogável de ponta a ponta.

---

## Fase 3A.1 — Loop Jogável Mínimo ✅ concluída
**Objetivo:** um jogador consegue invocar, subir a torre, ganhar XP, fazer craft de 1 item e equipá-lo. Nada mais. Validar se o loop é divertido antes de complexificar.

> **Testes são contínuos a partir daqui** — cada sistema criado nasce com teste unitário.

### XP e Progressão
- [x] **Curva de XP**: `XP_next = B_r × nível` com B_r por raridade (80/100/120/150/200) em `RaridadeConfig.BaseXp`
- [x] Aplicar stats base por raridade na criação do herói (`ObterAtributosBaseParaRaridade`)
- [x] Aplicar bônus racial (+50 no atributo foco) na criação do herói (`HeroiLevelUpService.BonusRacial`)
- [x] XP ganho ao subir andar da Torre (`TorreService.SubirAndarAsync` → `AplicarXp`)
- [x] Level-up com distribuição de pontos e verificação de cap (`HeroiLevelUpService.AplicarXp`)
- [x] Bloqueio de XP ao atingir o cap (XP zerado, level travado)

### Torre
- [x] Fórmula de dano: `ATK × SkillMult × (1 - DEF/(DEF+1000+Level×50)) × TypeMult`; crit 1.5×; burst cap 65% HP
- [x] Ordem de turno ATB: `Agilidade + Random(0, Agilidade×0.1)`
- [ ] IA tática no combate (atacar menor HP / maior ameaça)
- [ ] Escala de inimigos por andar (stats derivados, não hardcoded)
- [ ] Drop de materiais em andares de boss

### Cidade — Produção Simples
- [x] Produção passiva com teto de 24h (`CidadeService` — pré-existente)
- [ ] Upgrades de prédio nível 1→2

### Crafting Básico
- [x] 5 receitas estáticas (`CraftingService`) — espada, arco, armadura, anel, amuleto
- [x] `/crafting listar` e `/crafting fazer <receitaId>`
- [x] `/heroi_equipar` — equipa item em herói, persiste bônus via `HeroiBonusAtributo`
- [x] Sistema de atributos extensível: `AtributosBase.Get/Set`, `Distribute`, `With`, `ToEnumerable` via `Enum.GetValues`
- [x] Sistema de raças extensível: tabelas de peso calculadas via `Enum.GetValues<Raca>()`

### Marco: Vertical Slice ✦ ATINGIDO
> Um jogador novo entra, invoca, sobe a torre, coleta recursos, crafta e equipa um item. Build: 0 warnings, 0 errors.

**Sinal de saída:** loop básico divertido e compreensível de ponta a ponta.

---

## Fase 3A.2 — Consolidação do Core ✅ concluída
**Objetivo:** aprofundar os sistemas core. Cidade com modelo de slots, gacha completo, arena.

### Gacha (substituído na 3A.3)
- [x] Campo `Lore` na entidade `Heroi`
- [x] Pool de personagens fixos 5★/4★ cadastrada no seed (9 personagens, idempotente)
- ~~Arte exibida no embed do pull~~ *(descartado — exibição migrou para `/colecao`)*

### Cidade — Modelo de Slots
- [x] Campo `Confianca` (0–100) na entidade `Heroi` (default 0)
- [x] Campo `Humor` na entidade `Heroi` (default 50)
- [x] **ResourceNode architecture**: Campo, Floresta, Mina, Prado — sempre disponíveis, sem slot
- [x] **Dois tipos de slot por prédio:** Responsabilidade (gate por Confiança + atributo) e Operação
- [x] Prédio inativo se slots de Responsabilidade não preenchidos
- [x] Humor da Cidade: média dos heróis alocados (mult 0.9–1.2×)
- [x] Fórmula de produção dois tiers: ResourceNode (`BaseRate × (1+ProfBonus) × h`) + Building (`Base × MultResp × SomaOp × HumorMult`)
- [x] `/cidade construir <prédio>` com validação de recursos
- [x] `/cidade alocar_recurso <herói> <node>` com taxa/h calculada no feedback
- [x] `/cidade alocar_predio <herói> <prédio> <slot_tipo>` com gates de Confiança e atributo
- [x] `/cidade ver` reworked: coletores com taxa, prédios com slots + heróis, HumorCidade
- [x] `PredioConfig` + `ResourceNodeConfig` — configurações estáticas imutáveis
- [x] `SlotOcupacao` entity + repository + migration

### Crafting Completo
- [x] Check de qualidade: `skill_craft + bônus_prédio(Nivel×2) + roll(1..20)` via Responsável da Forja
- [ ] Laboratório produzindo poções usadas automaticamente na Torre

### Torre
- [x] Ouro por andar: `5 + Numero×3` × boss_mult (mesmo multiplicador do XP)

### Arena
- [x] `/treinar <herói>` — XP acelerado (3× XpParaProximoNivel), 4h cooldown, custo 100 Ouro + 10 Comida
- [x] `/arena desafio` — desafio de ondas com cooldown 24h, top-5 heróis automático
- [ ] Sistema de Prestígio e títulos

### Testes (pendentes — carry-over para 3B)
- [ ] Testes unitários: GachaService (pity, raridade, raças)
- [ ] Testes unitários: HeroiLevelUpService (grants, totais, caps)
- [ ] Testes unitários: CombatService (turnos, dano, crit)
- [ ] Testes unitários: Produção passiva da cidade
- [ ] Testes de integração: loop gacha → alocar → produzir → evoluir (SQLite in-memory)

### Qualidade (pendente — carry-over para 3.5)
- [ ] `Random.Shared` no `GachaService`
- [ ] `ILogger<T>` substituindo `Console.WriteLine`
- [ ] Guild ID e caminho do banco em variável de ambiente / appsettings

**Sinal de saída:** ✅ todos os sistemas core da 3A implementados; cidade com dois tiers de produção funcionando; arena jogável.

---

## Fase 3A.3 — Sistema de Fragmentos ✅ concluída
**Objetivo:** substituir gacha por aquisição determinística de heróis via fragmentos, biomas, marcos e contratos.

> Big Bang: `GachaService` e `BannerService` removidos. Sistema de fragmentos implementado do zero.

### Domínio e Repositórios
- [x] Entidades: `HeroiConfig`, `Bioma`, `BiomHeroPool`, `HeroiUnlockConfig`, `FragmentoProgresso`, `Contrato`, `HeroiDesbloqueado`
- [x] Interfaces de repositório: `IHeroiConfigRepository`, `IHeroiDesbloqueadoRepository`, `IFragmentoRepository`, `IBiomaRepository`, `IContratoRepository`
- [x] Implementações EF Core + migration + seed (9 heróis com unlock config; 5 biomas com pools)

### Serviços (Application)
- [x] `BiomeService` — mapeamento andar→bioma, detecção de bioma novo e marco
- [x] `FragmentService` — drops pesados por bioma, multiplicador de contrato, upsert de progresso
- [x] `RecruitmentService` — 3 caminhos: fragmentos, marco da Torre, condição única
- [x] `ContractService` — contratos arquétipo (+30%) e nomeado (+50%); expiração automática
- [x] `RewardDistributionService` — factory de payloads Micro/Médio/Alto por tipo de evento

### Extensão da Torre
- [x] `TorreService.SubirAndarAsync` estendido: drop de fragmentos, detecção de bioma novo, desbloqueio de herói por marco
- [x] `SubirAndarResult` ampliado com `Fragmentos`, `NovoBioma`, `HeroiDesbloqueado`, `RewardPayloads`

### Bot — Painéis Discord
- [x] `/colecao` — coleção completa com estado por herói, barra de progresso, botão de recrutar
- [x] `/bioma` — bioma atual com heróis disponíveis e pesos
- [x] `/contrato` — contratos ativos com select menu de arquétipo e botão de remover nomeado
- [x] `DiscordIdHelper.ToGuid(ulong)` — conversão determinística Discord ID → Guid

### Testes
- [x] 39 testes passando (BiomeService, FragmentService, ContractService, RecruitmentService, TorreService extensão)

**Sinal de saída:** ✅ gacha eliminado; 3 caminhos de recrutamento funcionais; painéis `/colecao` `/bioma` `/contrato` operacionais; build 0w/0e.

---

## Fase Q — Fundações de Qualidade
**Objetivo:** fechar a dívida técnica da 3A antes de construir a 3B sobre ela.

- ~~`Random.Shared` no `GachaService`~~ *(GachaService removido na 3A.3)*
- [ ] `ILogger<T>` substituindo `Console.WriteLine` nos serviços (parcial — CommandHandler já usa logging estruturado)
- [ ] Guild ID e caminho do banco em `appsettings.json` / variável de ambiente
- [ ] Guard clauses centralizadas: padrão único para herói em missão / alocado / inativo / equipado
- [ ] Testes unitários: HeroiLevelUpService (grants, totais, caps), CombatService (turnos, dano, crit), CidadeService
- ~~Testes unitários: GachaService~~ *(removido)*
- [ ] Teste de integração: fragmentos → recrutar → alocar → evoluir (SQLite in-memory)

**Sinal de saída:** build verde com cobertura nos serviços core; sem valores hardcoded de ambiente; sem `Random` solto.

---

## Fase UX-0 — Camada de Interação (Mínima)
**Objetivo:** estabelecer o padrão de UX híbrido usando `/cidade` como validação. Nenhuma feature de jogo nova — apenas a infraestrutura de UI. Expandir para outros sistemas só após validação.

### Decisões de UX aprovadas
- Painéis principais (`/cidade`, `/torre`, `/heroi`) → **públicos** — persistem no canal
- Feedback de ação e sub-fluxos → **efêmero**
- Confirmações destrutivas → efêmero com `[Confirmar] [Cancelar]` e timeout de 30s
- Navegação entre sub-painéis → atualiza mensagem in-place (`UpdateAsync`)
- Expiração de interação → aceitar limite de 15 min; player reabre com `/sistema`
- Seleção de listas → Select Menu; ações primárias → botões (máx 4–5 por painel)
- Dados → sempre lidos do banco a cada interação (sem cache de painel)
- Eventos pendentes da Torre → exibidos quando player executar qualquer próximo comando

### Implementação
- [ ] Convenção de `customId`: `sistema:acao[:param1:param2]`
- [ ] `InteractionRouter` — parseia `customId` e despacha ao handler correto
- [ ] `PanelBuilder` — padrão base: ViewModel → `(Embed, ComponentBuilder)`; nenhum tipo Discord nos Services
- [ ] Padrão `DeferAsync` + `UpdateAsync` para operações com acesso ao banco
- [ ] `/cidade` convertido para painel público com botões (primeiro sistema validado)

**Sinal de saída:** `/cidade` funciona como painel público com botões e Select Menu; padrão documentado e reutilizável pelos demais sistemas.

---

## Sessão 2026-04-25 — Bioma Panel, Cidade UX e Torre Modo Operação v2 ✅ concluída

### Correções de bug
- [x] `CidadeCommand` — ícones dos nodes corrigidos; switch de case substituído por `ResourceNodeConfig.Icone()` centralizado
- [x] `TorreCommand.HandleExplorarAsync` / `HandleExpAtualizarAsync` — teamPS filtrado por `HeroisIds` da exploração
- [x] `ColecaoCommand.MostrarAsync` — erro 40060 resolvido trocando `DeferAsync` + `UpdateAsync` por `ModifyOriginalResponseAsync`

### Bioma
- [x] `BiomeService.ListarDescobertosAsync(andarAtual)` e `ObterPorIdAsync(Guid)` adicionados
- [x] `BiomaPanel.CriarLista` — Select Menu com biomas descobertos, % andares conquistados e indicador de bioma atual
- [x] `BiomaPanel.CriarDetalhe` — barra de progresso por andares, pool de heróis com sistema de descoberta ("?" para heróis não descobertos, contador de pendentes)
- [x] `BiomaCommand` refatorado com 4 handlers: `ExecutarAsync`, `MostrarListaAsync`, `VoltarListaAsync`, `MostrarDetalheAsync`

### Cidade UX
- [x] `CidadePanel.CriarEmbed` — coletores agrupados por node com heróis indentados e taxa por hora
- [x] Contador de heróis disponíveis exibido no painel (`👥 X disponíveis / Y total`)
- [x] `ResourceNodeConfig.Icone(string recurso)` — método centralizado (lowercase switch)

### Torre Modo Operação v2
- [x] `TorreOperacaoConfig` (arquivo novo) — duração fixa 8h, produção por tier de andar em 6 faixas, afinidade racial leve, cálculo de slots por nível de guilda
- [x] `TorreOperacaoService` reescrito — suporte a múltiplas operações simultâneas (`IniciarAsync`, `ProcessarTodasAsync`, `ColetarTodasAsync`, `CancelarPorAndarAsync`)
- [x] `ITorreOperacaoRepository` / `TorreOperacaoRepository` — `ListarAtivasAsync`, `ListarConcluidasAsync`, `ObterPorAndarAsync`
- [x] `TorreModoOperacaoPanel` reescrito — board de andares (`CriarBoard`, `CriarSemAndares`, `CriarSeletorAndar`, `CriarSeletorRemover`, `CriarNotificacaoTexto`)
- [x] `TorreCommand` — wizard de 4 etapas substituído por handlers de board: `HandleModoOperacaoAsync`, `HandleOpAlocarAsync`, `HandleOpAndarSelAsync`, `HandleOpColetarTodasAsync`, `HandleOpRemoverSelAsync`, `HandleOpRemoverAndarSelAsync`, `HandleOpFecharAsync`
- [x] `CommandHandler` — novos IDs de interação roteados; `TorreCommand` injetado com `CidadeService`

---

## Fase 3B — Expansão de Sistemas (P1)
**Objetivo:** adicionar profundidade econômica e loops complementares. Implementar após UX-0 validado.

### Próximos Passos Recomendados (2026-04-25)

> Baseado na análise dos documentos de design T0 (Torre) e T1 (Boosters Cidade) cruzados com o que está implementado.

| Prioridade | Sistema | Motivo |
|---|---|---|
| 🔴 Alta | **3B-ItemUnico** — Item único por andar (Torre Operação) | T3 identifica como ponto mais crítico: sem diversidade de itens o sistema colapsa em "farm 2 andares pra sempre" |
| 🔴 Alta | **3B-TorreExp** — Investigação + Checkpoints + Heróis Feridos | Loop central da Torre; T0 define estes como mecanismos core |
| 🟠 Média | **3B-BoostCidade** — Boosters consumíveis da cidade | T1 completo; conecta crafting, economia e Torre |
| 🟠 Média | **3B-RotacaoInteresse** — Crafting + eventos que demandam itens de andares variados | T3: sem rotação de interesse o jogador escolhe 2 andares e nunca muda |
| 🟡 Média | **3B-1** — Inventário Unificado | Pré-requisito bloqueante para Relíquias e Mercado |
| 🟡 Média | **3B-2** — Conversão de Heróis | Sink de heróis excedentes; requer Inventário |
| 🔵 Baixa | **3B-5** — Guilda / Missões | Requer HeroPowerScore estável |
| 🔵 Baixa | **3B-6** — Relíquias | Requer Inventário (3B-1) |

### Ordem de build completa
Cada etapa é pré-requisito ou dependência lógica da seguinte.

| # | Sistema | Dependência |
|---|---|---|
| 3B-ItemUnico | Item Único por Andar (Torre Operação) | `TorreOperacaoConfig`, sistema de Inventário básico |
| 3B-TorreExp | Torre Exploração Avançada — Investigação, Checkpoints, Heróis Feridos | `TorreExploracaoService` |
| 3B-BoostCidade | Boosters da Cidade — consumíveis craftáveis de produção | Crafting expandido |
| 3B-RotacaoInteresse | Rotação de Interesse — demandas de crafting e eventos semanais | 3B-ItemUnico + Crafting |
| 3B-1 | Inventário Unificado | Pré-requisito para Relíquias e Mercado |
| 3B-2 | Conversão de Heróis (Venda + Absorção) | — |
| 3B-3 | Torre — Modo Operação ✅ | — |
| 3B-4 | Sustento (Comida / Moradia / Estados) ✅ MVP | — |
| 3B-5 | Guilda / Missões | HeroPowerScore (requer combate estável) |
| 3B-6 | Relíquias | Inventário Unificado (3B-1) |
| 3B-7 | Novos Prédios | Sustento (3B-4) + Guilda (3B-5) |
| 3B-Mercado | Sistema de Mercado P2P | Inventário (3B-1); pode rodar em paralelo com 3B-7 |
| 3B-Merc | Mercenários (empréstimo de heróis) | Snapshot model; pode rodar em paralelo com 3B-Mercado |
| 3B-Treino | Treinamento como Serviço | Requer Arena rank ≥ 1; complementar à Torre/Arena |
| 3B-Meta | Nível do Mestre + Traços 4★ | Requer atividade de todos os sistemas 3B |

Cada sistema recebe UX de painel desde o primeiro dia (usando o padrão da UX-0).

### 3B-ItemUnico — Item Único por Andar (T3 — ponto crítico)
Design: cada andar da Torre tem 1 item exclusivo associado. O jogador avança a Torre, vê o item do andar, decide se vale alocar um slot de operação para farmá-lo. Sem diversidade de itens o sistema colapsa em "farm 2 andares pra sempre".

**Status dos tipos de item (T3):**
- [ ] **Equipamento com efeito específico** — crit, sustain, speed (não apenas "mais forte")
- [ ] **Item de nicho** — forte para certos heróis/builds, irrelevante para outros
- [ ] **Consumível estratégico** — buff temporário poderoso, crafteável ou drop direto
- [ ] **Componente de crafting raro** — ingrediente de recipe avançada (cria demanda de andar)
- [ ] **Item de progressão** — usado em ascensão futura (cria demanda de longo prazo)

**Implementação:**
- [ ] `AndarItem` entity — `AndarNumero`, `ItemConfig` (Id, Nome, Tipo, Efeito, Slot/Raridade)
- [ ] `AndarItemConfig` static seed ou tabela — 1 entrada por andar (ex: andares 1–100 documentados)
- [ ] `TorreOperacaoConfig.ObterProducao` estendido para incluir item do andar (além do recurso)
- [ ] `TorreModoOperacaoPanel.CriarBoard` — exibir item do andar em cada linha do board
- [ ] `CriarSeletorAndar` — mostrar item ao lado do andar no Select Menu
- [ ] `TorreOperacaoService.ConcluirOperacao` — adicionar item ao inventário do jogador

### 3B-TorreExp — Torre Exploração Avançada (T0)
Sistemas pendentes do design T0 para completar o loop de exploração com risco controlado e checkpoints.

- [ ] **Sistema de Investigação do Andar** — 3 níveis: Básico (% vitória com time atual), Intermediário (fraquezas elementais), Avançado (distribuição de checkpoints + modificadores)
- [ ] **Probabilidade de Vitória** — cálculo: `teamPS / andarDificuldade` (clamp 5–95%); exibição ao jogador antes de confirmar
- [ ] **Checkpoints de Loot** — loot gerado ao atingir marcos de progresso (ex: 25%, 50%, 75%, 100%); falha preserva loot até último checkpoint atingido
- [ ] **Heróis feridos ao falhar** — heróis da run ficam indisponíveis por tempo proporcional à raridade; ícone 🩹 no painel
- [ ] **Checkpoints dinâmicos** — andares mais difíceis têm checkpoints mais espaçados (configurado por `AndarConfig`)
- [ ] **Modificadores de andar** — ex: "sem checkpoint até 50%", "+50% loot / risco dobrado"

### 3B-BoostCidade — Boosters da Cidade (T1)
Itens consumíveis craftáveis que melhoram eficiência de atividades fora da Torre. 1 ativo globalmente, duração em tempo real.

**6 tipos (T1):**
- [ ] **Produção** — +X% velocidade de produção (aumenta consumo por tempo)
- [ ] **Rendimento** — +X% quantidade produzida (não aumenta consumo)
- [ ] **Eficiência** — -X% custo por ciclo (foco em economia pura)
- [ ] **Qualidade** — chance de produzir item/versão melhor
- [ ] **Especialização** — +X% eficiência para tipo específico (mineração / agricultura / crafting)
- [ ] **Conversão** — chance de gerar recurso secundário

**Implementação:**
- [ ] Enum `TipoBoosterCidade` + entidade `BoosterCidadeAtivo` (`UsuarioId`, `Tipo`, `IniciadoEm`, `DuracaoMinutos`)
- [ ] `BoosterCidadeService.AtivarAsync`, `ObterAtivoAsync`, `AplicarEfeitoAsync`
- [ ] Hook em `CidadeService.ColetarProducaoAsync` — aplica multiplicador do booster ativo
- [ ] Crafting recipes de boosters — materiais da Torre como ingredientes
- [ ] `BoosterCidadeConfig` data-driven (efeito, duração, custo por tipo)
- [ ] Painel `/cidade` — linha de booster ativo com duração restante; botão `[Ativar Booster]`

### 3B-RotacaoInteresse — Rotação de Interesse (T3)
Sem rotação, jogador escolhe 2 andares e nunca muda. T3 identifica três soluções:

- [ ] **Crafting demanda itens de andares variados** — recipes avançadas exigem componentes de biomas diferentes
- [ ] **Eventos semanais** — evento pede drop específico de andar rotativo (incentivo temporário a mudar alocação)
- [ ] **Ascensão / progressão** consome item de andar específico — cria demanda permanente

### 3B-1 — Inventário Unificado
- [ ] `Inventario` — entidade unificada por jogador; tipos: `Recurso | Item | Reliquia | Consumivel`
- [ ] Operações atômicas: `Add(tipo, id, qtd)` / `Remove(tipo, id, qtd)` com validação prévia
- [ ] Stack limit por tipo (relíquias não stackam; recursos sim)
- [ ] `/inventario` — painel com abas por categoria e paginação

### 3B-2 — Conversão de Heróis
- [ ] **Venda**: `Valor = BaseRaridade × EscalaDeNivel × FatorDeEscassezGlobal`
- [ ] **Absorção**: consome herói, transfere 50% do XP acumulado para herói-alvo
- [ ] Bloqueios anti-exploit: herói equipado / em missão / na Torre / alocado em prédio
- [ ] Botões `[Vender]` e `[Absorver]` no painel de herói com confirmação efêmera

### 3B-3 — Torre — Modo Operação ✅ concluído (MVP — v3.1.0)

> Entidade `TorreOperacao` (raw SQLite), serviço `TorreOperacaoService`, painel `TorreModoOperacaoPanel` (4 etapas), botão no `TorrePanel`, roteamento no `CommandHandler`.

- [ ] Estado `AndarConcluido` por usuário + andar *(não exigido no MVP — qualquer andar operável)*
- [x] Sub-painel: escolha de andar (Select Menu), objetivo (FarmRecurso / ExploracaoLeve), perfil de risco (Seguro / Balanceado / Agressivo)
- [x] Resultado automático com ouro creditado (`andar × 3 × horas × mult`); recursos exclusivos por andar
- [ ] Eventos de interrupção: exibidos no próximo comando do jogador *(deferido)*
- [x] `[Cancelar]` aborta operação ativa (sem coleta parcial no MVP)
- [x] Recursos exclusivos por andar: Fragmento Rústico (≥5), Essência Corrompida (≥12), Cristal Arcano (≥18), Núcleo Sombrio (≥25)

### 3B-4 — Sustento ✅ concluído (MVP — v3.2.0)

> `EstadoSustento` enum + campo em `Heroi`, `UltimoSustentoEm` em `Cidade`, migration, `SustentoService` (poll, toggle, resumo), ícones no painel `/herois`, linha de sustento no `/cidade`, toggle Inativo via botão.

- [x] Campo `EstadoSustento` na entidade `Heroi` (enum: Ativo / Instavel / Degradado / Inativo)
- [x] Consumo flat: 1 Comida/hora por herói ativo (escala por raridade/classe deferida)
- [x] `SustentoService.ToggleInativoAsync` — herói Inativo não consome Comida
- [x] Transições de estado calculadas por horas restantes (≥8h Ativo, 2–8h Instavel, <2h Degradado)
- [x] Linha de sustento no painel `/cidade`: `✅/⚠️/🔴 X 🌾/h | Estoque: Y | ~Z.Zh restantes`
- [ ] Penalidades de atributo e XP por estado *(informativo apenas no MVP)*
- [ ] Limite de Moradia via prédio Alojamento *(deferido)*
- [ ] Consumo variável por raridade/classe *(deferido)*
- [ ] Sinks de Ouro adicionais *(deferido)*

### 3B-5 — Guilda / Missões
- [ ] `HeroPowerScore` implementado (`DESIGN_SISTEMAS.md §2`) — pré-requisito de cálculo de sucesso
- [ ] Entidade de rank da Guilda (Ferro → Oricalco, 15 tiers)
- [ ] Geração automática de missões a cada 6h (até 8 simultâneas)
- [ ] State machine: `Aguardando → EmMissao → Retornando → Concluida`
- [ ] Cálculo de sucesso/parcial/falha por poder vs dificuldade
- [ ] Painel `/guilda` com botões `[Missões Ativas]` `[Missões Disponíveis]` `[Enviar Heróis]`

### 3B-6 — Relíquias
- [ ] Entidade `Reliquia` (Id, Nome, Descrição, Efeito, AndarMinimo)
- [ ] 3 slots de relíquia por herói (`HeroiReliquia`)
- [ ] Drop em boss floors → armazenado no Inventário (3B-1)
- [ ] Equipar/remover via painel `/inventario`
- [ ] Efeito passivo aplicado no combate via interface `IEfeitoReliquia`

### 3B-7 — Novos Prédios
- [ ] **Armazém** — limite de estoque; overflow converte automaticamente
- [ ] **Mercado** — conversão de recursos em ouro; taxas melhoram com upgrade
- [ ] **Prefeitura** — limites globais; pré-requisito para prédios avançados
- [ ] Upgrades de prédio nível 2→3
- [ ] Confiança desbloqueia funções avançadas (ex: Forja nível 71+ → Mestre da Forja)

### 3B-Mercado — Sistema de Mercado P2P
Pode ser implementado em paralelo com 3B-7 ou logo após. Depende apenas de 3B-1 (Inventário Unificado).

**Decisões aprovadas:**
- `/mercado` → efêmero (gestão privada); canal `#mercado` → público (vitrine)
- Phase 1: Equipamentos + Consumíveis apenas (recursos brutos adiados para Step 3)
- Taxa de listagem: 5 Ouro flat por listagem (não reembolsável, anti-spam)
- Taxa de venda: 10% base (reduzida pelo prédio Mercado: nível 1→8%, 2→6%, 3→3%)
- Limite: 3 listagens ativas por jogador; sem cap global no canal
- Expiração: 24h; bot edita mensagem no canal (badge status + botão desabilitado) antes de deletar
- Preço: livre com floor (50% BasePrice) e ceiling (1000% BasePrice); mostra BasePrice + média recente

**Step 1 — MVP:**
- [ ] Entidades `MarketListing` + `MarketSaleHistory` + migration
- [ ] `IsLocked` + `LockReason` no `InventarioEntry`
- [ ] `MarketConfig` data-driven (taxa, limite, expiração, floors)
- [ ] `MarketService`: listar, comprar, cancelar, expirar
- [ ] `MarketBoardService`: post/edit/delete mensagens no `#mercado`
- [ ] `MarketExpiryWorker`: detecta e resolve expirados (roda a cada 10 min)
- [ ] `/mercado` painel efêmero: `[Vender Item]` + `[Minhas Listagens]`
- [ ] Flow de venda: Select Menu → Modal de preço → confirmação → post no canal
- [ ] Flow de compra: `[Comprar]` → confirmação efêmera → transaction com `BEGIN IMMEDIATE`

**Step 2 — UX Polish:**
- [ ] Média recente de preço exibida nas listagens (`MarketSaleHistory` — últimas 5 vendas)
- [ ] Post público breve no canal em vendas acima de threshold (ex: 500 Ouro)
- [ ] Notificação "Evento ao Logar" para o vendedor quando item for vendido
- [ ] Board Summary pinado no `#mercado` (editado a cada transação)
- [ ] `[Ver Detalhes]` na listagem: ephemeral com stats completos do item

**Step 3 — Integração e Extensão:**
- [ ] `MarketService.GetTaxRateForPlayer()` consulta nível do prédio Mercado na cidade
- [ ] `ItemConfig.IsTradeable` (bool) — controle por tipo de item sem lógica de switch
- [ ] Suporte a Consumíveis com quantidade (comprador recebe lote inteiro)
- [ ] Suporte a recursos brutos (madeira, pedra, minério) com stacking
- [ ] Relíquias como item tradeable (quando 3B-6 estiver estável)

### 3B-Mercenários — Mercenários (Empréstimo de Heróis)
Pode rodar em paralelo com 3B-Mercado. Compartilha a infraestrutura de `HeroSnapshot` com 3B-Treino.

**Decisões aprovadas:**
- Herói nunca transferido — snapshot capturado no momento do empréstimo, write-once, frozen
- Arena proibida para mercenários (integridade competitiva + anti-exploit circular com ArenaRank)
- Torre e Missões: liberados. Cidade: proibida
- Custo: `CustoBase + (CustoPorNivel × heroi.Nivel)` via `MercenarioConfig` (data-driven)
- Duração: 6h / 12h / 24h
- Limites: 1 herói emprestado + 1 contratado por jogador simultâneo
- NPC fallback sempre disponível (sem snapshot real, stats virtuais por tier no config)
- Anti-exploit: anti-chain bilateral, same-pair cooldown 24h, `BEGIN IMMEDIATE TRANSACTION`

**Step 1 — MVP:**
- [ ] `IHeroCombatant` interface no Domain; `Heroi` e `HeroSnapshot` implementam
- [ ] Estender `HeroLockStatus`: + `AsMercenary`, `InTraining`, `AsTrainer`
- [ ] Adicionar `LockStatus` + `LockReason` em `Heroi`
- [ ] Entidade `HeroSnapshot` + `SnapshotTipo` enum + migration
- [ ] Entidade `EmprestimoHeroi` + `EmprestimoStatus` + `DuracaoOpcao` enums + migration
- [ ] `MercenarioConfig` data-driven: seed 3 durações (6h/12h/24h)
- [ ] `SnapshotService.CaptureAsync` + `GetActiveForContratante`
- [ ] `MercenarioService`: disponibilizar, contratar, cancelar (com `BEGIN IMMEDIATE TRANSACTION`)
- [ ] `MercenarioExpiryWorker` (IHostedService, poll 5 min): expire → unlock herói → edit+delete channel message
- [ ] `MercenarioPanelBuilder` + `MercenarioInteractionHandler`
- [ ] `/mercenarios` painel efêmero: `[Disponibilizar Herói]` `[Buscar Mercenário]` `[Meu Empréstimo]`
- [ ] Canal `#mercenarios`: post automático com `[Contratar]` button

**Step 2 — Integração combat:**
- [ ] `CombatService`, `TorreService`, `MissaoService`: aceitar `IHeroCombatant` em vez de `Heroi` direto
- [ ] `ArenaService`: permanece usando `Heroi` — não recebe `IHeroCombatant`
- [ ] Guard `LockStatus == None` em absorb/sell/equip (reaproveita padrão do market)
- [ ] Recovery job no startup: unlock forçado de heróis com `LockStatus != None` e `ExpiresAt < Now`

### 3B-Treinamento — Treinamento como Serviço
Complementar à Torre e Arena — nunca substituto. Implementar após 3B-Merc (compartilha snapshot model).

**Decisões aprovadas:**
- XP fórmula: `base_xp = TrainerPowerScore × XpFatorConfig × DuracaoHoras × (1 + ArenaBonus%)`; frozen no cap: `min(base_xp, MaxXpSemana - acumulado)`
- Cap semanal por herói: `500 + (heroi.Nivel × 50)` — treino é suplementar
- `TreinamentoConfig` (data-driven, keyed por ArenaRank 0–5): XpBonus, MaxSlots, DuracaoHoras, NpcEficiência
- ArenaRank ≥ 1 obrigatório para oferecer treino
- NPC trainers sempre disponíveis a 40% eficiência
- Custo split: 70% para treinador, 30% sink de ouro
- Técnicas Especiais: NOT Phase 1 — deferido para pós-lançamento

**Step 1 — MVP:**
- [ ] Entidade `TreinamentoHeroi` + `TreinamentoStatus` enum + migration (reusa `HeroSnapshot` de 3B-Merc)
- [ ] `TreinamentoConfig` data-driven: seed 6 ranks (0–5)
- [ ] `TreinamentoService`: oferecer treino, enroll aluno, concluir (calcula XP, aplica, desbloqueia)
- [ ] `TreinamentoExpiryWorker` (IHostedService, poll 5 min): conclude → apply XP → unlock aluno → conditional unlock treinador
- [ ] Anti-exploit: same-pair 24h, anti-chain bilateral, cap semanal via aggregate query, ArenaRank ≥ 1
- [ ] `TreinamentoPanelBuilder` + `TreinamentoInteractionHandler`
- [ ] `/treinamento` painel efêmero: `[Oferecer Treino]` `[Enviar Herói]` `[Meus Treinos]`
- [ ] Canal `#treinamento`: post com rank badge + XpBonus% + `[Enviar Herói]` button
- [ ] Weekly XP tracking via aggregate query em `TreinamentoHeroi` (sem campo extra em `Heroi`)

### Meta Progressão (após 3B-7)
- [ ] **Nível do Mestre** — progride com atividade global; desbloqueia bônus passivos e identidade de conta
- [ ] **Bônus de Composição de Party** — 3 da mesma raça → +10% XP; full arqueiros → +crit; party balanceada → bônus misto
- [ ] **Traço fixo na ascensão 4★** — jogador escolhe 1 traço permanente (ex: Incansável, Pragmático)
- [x] Personagens fixos via fragmentos específicos — ✅ implementado na Fase 3A.3

**Sinal de saída:** economia com pressão real; heróis excedentes têm destino; Torre tem replay value; missões e relíquias funcionam; mercado P2P ativo; empréstimo de heróis e treino entre jogadores operacionais.

---

## Fase 3C — IA e Automação (P2)
**Objetivo:** sistemas de gestão autônoma. **Não implementar antes de 3B estável** — IA sobre economia instável gera bugs emergentes difíceis de debugar.

- [ ] Política da cidade: `/cidade politica <foco>`
- [ ] Auto-alocação de heróis por confiança e política ativa
- [ ] `/cidade otimizar`
- [ ] Cadeia de dependência inteligente + `/cidade cadeia <prédio>`
- [ ] Slots de Liderança: herói Parceiro+ assume liderança, buffa operadores
- [ ] **Torre — Progresso % por andar** com requisito secreto opcional
- [ ] **Torre — Zonas e identidade mecânica** por andar (anti-cura, buffs aleatórios, etc.)
- [ ] Decisões automáticas do líder na Torre quando jogador não responde

**Sinal de saída:** jogador pode delegar a direção da cidade; torre opera sem supervisão constante.

---

## Fase 3.5 — Infra & Observabilidade
**Objetivo:** garantir que o sistema aguenta o beta sem exploits ou quebras silenciosas. Inserir antes do Beta Fechado.

- [ ] **Telemetria mínima** — eventos estruturados: `PlayerLoggedIn`, `HeroLeveledUp`, `ResourceCollected`, `MissionCompleted`, `GachaPulled`; métricas: ouro gerado/dia, XP médio por jogador, taxa de uso de heróis, tempo entre sessões
- [ ] **Feature flags** — ligar/desligar sistemas sem deploy; liberar para jogadores específicos
- [ ] **Configuração dinâmica** — taxas de drop, curva de XP, custos de ascensão configuráveis via JSON/banco sem rebuild
- [ ] **Anti-exploit e concorrência** — lock por jogador (semáforo/mutex por UserId); validação de timestamp no servidor; idempotência em operações críticas
- [ ] **Rate limiting** — cooldown por comando crítico; fila de execução; feedback "aguarde X segundos"
- [ ] **Fail-safe patterns** — rollback em falha (EF Core Transaction); retry automático; fallback de estado
- [ ] **Versionamento de dados** — versão do schema salva no banco; migrations controladas; script de correção de dados
- [ ] **Sistema de log de auditoria** — histórico de ações críticas por jogador (venda, ascensão, drop raro)
- [ ] **Backup automático** do SQLite

**Sinal de saída:** nenhum exploit testável; economia monitorável; rollback de dados possível.

---

## Fase 4 — Beta Fechado
**Objetivo:** jogadores reais encontram o que você não viu.

- [ ] **Onboarding** — tutorial inicial guiado; primeira invocação garantida; primeira missão dirigida; UI de ajuda básica
- [ ] Convidar 5–15 pessoas de confiança para testar
- [ ] Coletar feedback de UX e compreensão dos comandos
- [ ] **Balancear com dados reais** — usar telemetria da 3.5 para decisões; não balancear "no feeling"
- [ ] Corrigir bugs reportados
- [ ] Bot rodando em servidor externo
- [ ] Definir estratégia de reset: wipe total / reset parcial (recursos, não heróis) / compensação com bônus — decidir antes do beta aberto

---

## Fase 5 — Beta Aberto
**Objetivo:** estressar o sistema com volume real.

- [ ] Abrir servidor Discord para mais jogadores
- [ ] **Teste de carga** — simular múltiplos usuários, spam de comando, produção simultânea antes da abertura
- [ ] Considerar gate de migração SQLite → PostgreSQL se volume justificar
- [ ] Monitorar performance e banco de dados
- [ ] Ajuste fino de balanceamento com dados reais
- [ ] Polish de embeds, mensagens e UX geral

---

## Fase 6 — v1.0 (Lançamento)
- [ ] Changelog público
- [ ] README atualizado
- [ ] Bot estável em produção

---

## Pós-lançamento (sem data)

### Expansão de conteúdo
- Novos banners e personagens fixos
- Eventos sazonais
- Novos prédios e receitas

### Sistemas de alto risco / alto impacto
- **Sistema de Invasão NPC** — cidades sofrem ataques periódicos de facções; tipos: Saque, Sabotagem, Sequestro, Cerco
- **Sistema de Traição** — heróis com Confiança ≤ 0 viram agentes hostis internos
- **Sistema de Expedições** — jogador invade o mundo; retaliação gera invasão na cidade
- **Torre — Anti-Meta Rígida** — andares com counters de build (anti-tank, anti-magia, anti-cura)
- **Torre — Identidade de Run** — seed único por run, perfil de estilo, histórico de decisões

### Econômico / Social
- **Arena — Torneios** com apostas em Ouro (conecta com Mercado)
- Mercado entre jogadores
- Multiplayer na Torre (raids)

### Design avançado (decisões a tomar antes de implementar)
- **Heróis Únicos** — nunca existe o mesmo herói duas vezes no gacha
- **Permadeath** — modo hardcore
- **Sacrifício/Síntese** — consumir herói cria relíquia especial ou transfere traço
- **PvP invasão assíncrona**
