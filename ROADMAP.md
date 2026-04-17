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

## Fase 3A.2 — Consolidação do Core
**Objetivo:** aprofundar os sistemas core. Cidade com modelo de slots, gacha completo, arena.

### Gacha
- [ ] Campo `ImageUrl` e `Lore` nos heróis fixos
- [ ] Pool de personagens fixos 5★/4★ cadastrada no seed
- [ ] Arte exibida no embed do pull

### Cidade — Modelo de Slots
- [ ] Campo `Confianca` (0–100) na entidade `Heroi` (valor inicial fixo/simples na 3A.2)
- [ ] Campo `Humor` na entidade `Heroi`
- [ ] **Dois tipos de slot por prédio:** Responsabilidade (gate por Confiança + atributo) e Operação
- [ ] Prédio inativo se slots de Responsabilidade não preenchidos
- [ ] Humor da Cidade: média ponderada dos heróis alocados
- [ ] Fórmula de produção: `Base × Nível × Multi(responsáveis) × Soma(operadores) × HumorCidade`

### Crafting Completo
- [ ] Check de qualidade: `skill_craft + bônus_prédio + roll(1..20)`
- [ ] Laboratório produzindo poções usadas automaticamente na Torre

### Arena
- [ ] `/treinar` — sessão intensiva com XP em burst
- [ ] `/arena desafio` — desafio de ondas com cooldown diário
- [ ] Sistema de Prestígio e títulos

### Testes (contínuos — adicionar junto com cada sistema)
- [ ] Testes unitários: GachaService (pity, raridade, raças)
- [ ] Testes unitários: HeroiLevelUpService (grants, totais, caps)
- [ ] Testes unitários: CombatService (turnos, dano, crit)
- [ ] Testes unitários: Produção passiva da cidade
- [ ] Testes de integração: loop gacha → alocar → produzir → evoluir (SQLite in-memory)

### Qualidade
- [ ] `Random.Shared` no `GachaService`
- [ ] `ILogger<T>` substituindo `Console.WriteLine`
- [ ] Guild ID e caminho do banco em variável de ambiente

**Sinal de saída:** todos os sistemas core profundos e testados; cidade com slots funcionando.

---

## Fase 3B — Expansão de Sistemas (P1)
**Objetivo:** adicionar profundidade econômica e loops complementares. Implementar após 3A.2 estável.

### Sistemas Econômicos
- [ ] **Sistema de Conversão de Heróis** — Venda (fórmula por raridade) e Absorção (50% XP transferido)
- [ ] **Sistema de Sustento** — Comida e Moradia; estados Ativo/Instável/Degradado; grace period de dívida antes de punição
- [ ] Sinks de Ouro explícitos: manutenção leve de prédios, reroll de missões, upgrades progressivamente caros

### Conteúdo Paralelo
- [ ] **Missões (Guilda)** — geração automática a cada 6h; herói vai e retorna; 15 ranks
- [ ] **Relíquias** — drop em boss floors, inventário, equipar/remover, efeito passivo aplicado

### Torre e Cidade
- [ ] **Torre — Modo Operação** — andar concluído → farm automático com eventos de interrupção
- [ ] Recursos exclusivos por andar (biomas produtivos)
- [ ] Novos prédios: Armazém (limite de estoque), Mercado (conversão), Prefeitura (limites globais)
- [ ] Upgrades de prédio nível 2→3
- [ ] Confiança desbloqueia funções avançadas do prédio (ex: Forja nível 71+ → Mestre da Forja)
- [ ] Fragmentos de arquétipo + ascensão de raridade

### Meta Progressão
- [ ] **Nível do Mestre** — progride com atividade global; desbloqueia bônus passivos e identidade de conta
- [ ] **Bônus de Composição de Party** — 3 heróis da mesma raça → bônus XP; full arqueiros → +crítico; party balanceada → bônus misto
- [ ] Personagens fixos via fragmentos específicos (ex: `Fragmento de Nyra`)

**Sinal de saída:** economia com pressão real; heróis excedentes têm destino; Torre tem replay value; missões e relíquias funcionam.

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
