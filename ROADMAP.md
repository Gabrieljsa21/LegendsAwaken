# Legends Awaken — Roadmap

Visão macro de fases. Tarefas granulares pendentes estão no `BACKLOG.md`.

---

## Fase 1 — Pré-produção ✅ concluída
**Objetivo:** fechar o design antes de codar.

- [x] Stack técnica escolhida (C#, Discord.Net, EF Core, SQLite) e arquitetura Clean Architecture + DDD aplicada
- [x] GDD criado com sistemas de cidade, confiança, prioridade e cadeia de dependência
- [x] Pool inicial de personagens fixos 5★ e 4★ definida (3 lendários + 6 épicos)
- [x] Sistema de progressão de nível (caps, ganhos, grants de ascensão) desenhado
- [x] Sistemas de Relíquias, Missões com Guilda de 15 ranks, Arena e crafting desenhados

**Sinal de saída:** GDD estável, sem features indefinidas no escopo do v1.0.

---

## Fase 2 — Protótipo da Cidade ✅ concluída
**Objetivo:** validar se o loop de gestão é divertido antes de polir.

- [x] Herói alocado produz recurso conforme profissão; `/cidade ver`, `/cidade coletar`, `/cidade alocar`
- [x] `CidadeRepository` reescrito em EF Core; recurso Erva adicionado
- [x] `HeroiLevelUpService` com `RaridadeConfig` — caps, ganhos e grants calculados sem hardcode

**Sinal de saída:** loop invocar → alocar → coletar jogável de ponta a ponta.

---

## Fase 3A.1 — Loop Jogável Mínimo ✅ concluída
**Objetivo:** um jogador consegue invocar, subir a torre, ganhar XP, fazer craft de 1 item e equipá-lo.

- [x] Curva de XP `B_r × nível` em `RaridadeConfig.BaseXp`; level-up com distribuição de pontos e cap
- [x] Fórmula de dano com crit 1.5× e burst cap 65% HP; ordem de turno ATB
- [x] 5 receitas estáticas de crafting; `/crafting listar`, `/crafting fazer`, `/heroi_equipar`
- [x] Build 0 warnings, 0 errors; marco Vertical Slice atingido

**Sinal de saída:** loop básico divertido e compreensível de ponta a ponta.

---

## Fase 3A.2 — Consolidação do Core ✅ concluída
**Objetivo:** aprofundar os sistemas core. Cidade com modelo de slots, gacha completo, arena.

- [x] Modelo de slots: Responsabilidade (gate por Confiança + atributo) e Operação por prédio
- [x] ResourceNode architecture: Campo, Floresta, Mina, Prado — dois tiers de produção com HumorMult
- [x] Check de qualidade no crafting via Responsável da Forja
- [x] `/treinar` (XP acelerado) e `/arena desafio` (ondas, cooldown 24h) operacionais

**Sinal de saída:** todos os sistemas core da 3A implementados; cidade com dois tiers de produção; arena jogável.

---

## Fase 3A.3 — Sistema de Fragmentos ✅ concluída
**Objetivo:** substituir gacha por aquisição determinística de heróis via fragmentos, biomas, marcos e contratos.

- [x] `GachaService` e `BannerService` removidos; 7 novas entidades de domínio com repositórios EF Core e seed
- [x] 3 caminhos de recrutamento: fragmentos, marco da Torre, condição única
- [x] Contratos de arquétipo (+30%) e nomeado (+50%) com expiração automática
- [x] Painéis `/colecao`, `/bioma`, `/contrato` operacionais
- [x] 39 testes unitários passando

**Sinal de saída:** gacha eliminado; 3 caminhos de recrutamento funcionais; build 0w/0e.

---

## Sessão 2026-04-25 — Bioma Panel, Cidade UX e Torre Modo Operação v2 ✅ concluída

- [x] Correções de bug: ícones de nodes, filtro de teamPS, erro 40060 no `/colecao`
- [x] `BiomaPanel` completo: Select Menu com biomas descobertos, barra de progresso, sistema de descoberta de heróis ("?")
- [x] `CidadePanel`: coletores agrupados por node com taxa/h; contador de heróis disponíveis
- [x] Torre Modo Operação redesenhado: múltiplas operações simultâneas, board de andares, `TorreOperacaoConfig` com 6 faixas de produção

---

## Sessão 2026-05-06 — Torre: Framework de Arcos Narrativos (Design) ✅ concluída

- [x] Skill `/analyze-folder-for-la` criada e executada em 57 arquivos D&D + 2 arquivos de ideias → `ANALISE_DND_PARA_LA.md`
- [x] Framework de arcos definido: objetivos 3-tier (A/B/C), flags simples + compostas, 5 categorias de colecionável, regra 70/30, calibração de bônus por tier, Design Layer vs Display Layer
- [x] Arco 1 — Torre em Ruínas (Andares 1–4) desenhado
- [x] Arco 2 — A Praga Ardente (Andares 5–10) desenhado
- [x] Arco 3 — A Cabana dos Experimentos (Andares 11–15) desenhado
- [x] `DESIGN_TORRE_ARCOS.md` criado com framework completo + 3 arcos + tracking JSON

---

## Fase Q — Fundações de Qualidade
**Objetivo:** fechar a dívida técnica da 3A antes de construir a 3B sobre ela.

**Pendente** — ver tarefas em `BACKLOG.md § Alta Prioridade — Fase Q`.

**Sinal de saída:** build verde com cobertura nos serviços core; sem valores hardcoded de ambiente.

---

## Fase UX-0 — Camada de Interação (Mínima) ✅ concluída
**Objetivo:** estabelecer o padrão de UX híbrido usando `/cidade` como validação. Nenhuma feature de jogo nova.

Painéis principais são públicos; feedback de ação é efêmero; navegação atualiza in-place; dados sempre lidos do banco.

- [x] `PanelResult` + `IInteractionHandler` + `InteractionRouter` (thread-safe) — infraestrutura de roteamento; 5 testes unitários
- [x] `ConfirmationPanel` — painel efêmero de confirmação `[Confirmar] [Cancelar]`; `global:cancelar` como escape hatch universal
- [x] `/cidade` migrado para convenção `sistema:acao[:param]`; `CidadeCommand` implementa `IInteractionHandler` com 16 handlers
- [x] Desalocação e construção protegidas por `ConfirmationPanel`
- [x] `CommandHandler` integrado com router; bloco legacy `cidade_*` removido

---

## Fase 3B — Expansão de Sistemas
**Objetivo:** adicionar profundidade econômica e loops complementares. Implementar após UX-0 validado.

### Ordem de build recomendada

| # | Sistema | Prioridade | Dependência |
|---|---------|-----------|-------------|
| 3B-ItemUnico | Item Único por Andar (Torre Operação) | Alta | `TorreOperacaoConfig`, Inventário básico |
| 3B-TorreExp | Torre Exploração Avançada — Investigação, Checkpoints, Heróis Feridos | Alta | `TorreExploracaoService` |
| 3B-BoostCidade | Boosters da Cidade — consumíveis craftáveis | Média | Crafting expandido |
| 3B-RotacaoInteresse | Rotação de Interesse — demandas de crafting e eventos semanais | Média | 3B-ItemUnico |
| 3B-1 | Inventário Unificado | Média | Pré-requisito para Relíquias e Mercado |
| 3B-2 | Conversão de Heróis (Venda + Absorção) | Média | — |
| 3B-5 | Guilda / Missões | Baixa | HeroPowerScore |
| 3B-6 | Relíquias | Baixa | 3B-1 Inventário |
| 3B-7 | Novos Prédios | Baixa | 3B-4 Sustento + 3B-5 Guilda |
| 3B-Mercado | Mercado P2P | Baixa | 3B-1 Inventário |
| 3B-Merc | Mercenários | Baixa | Snapshot model |
| 3B-Treino | Treinamento como Serviço | Baixa | 3B-Merc |
| 3B-Meta | Nível do Mestre + Traços 4★ | Baixa | Todos os sistemas 3B |

**Sinal de saída:** economia com pressão real; Torre tem replay value; missões e relíquias funcionam; mercado P2P ativo; empréstimo e treino entre jogadores operacionais.

---

## Fase 3C — IA e Automação
**Objetivo:** sistemas de gestão autônoma. Não implementar antes de 3B estável.

**Pendente** — ver tarefas em `BACKLOG.md § 3C — IA e Automação`.

**Sinal de saída:** jogador pode delegar a direção da cidade; torre opera sem supervisão constante.

---

## Fase 3.5 — Infra & Observabilidade
**Objetivo:** garantir que o sistema aguenta o beta sem exploits ou quebras silenciosas. Inserir antes do Beta Fechado.

**Pendente** — ver tarefas em `BACKLOG.md § Fase 3.5 — Infra & Observabilidade`.

**Sinal de saída:** nenhum exploit testável; economia monitorável; rollback de dados possível.

---

## Fase 4 — Beta Fechado
**Objetivo:** jogadores reais encontram o que você não viu.

**Pendente** — convidar 5–15 testadores de confiança; onboarding guiado; balancear com telemetria real; bot em servidor externo; definir estratégia de reset antes do beta aberto.

---

## Fase 5 — Beta Aberto
**Objetivo:** estressar o sistema com volume real.

**Pendente** — abrir servidor Discord; teste de carga; avaliar migração para PostgreSQL; ajuste fino com dados reais.

---

## Fase 6 — v1.0 (Lançamento)

**Pendente** — changelog público; README atualizado; bot estável em produção.

---

## Pós-lançamento (sem data)

### Expansão de conteúdo
- Novos personagens fixos e eventos sazonais
- Novos prédios e receitas

### Sistemas de alto risco / alto impacto (decisões a tomar antes de implementar)
- **Sistema de Invasão NPC** — cidades sofrem ataques periódicos de facções (Saque, Sabotagem, Sequestro, Cerco)
- **Sistema de Traição** — heróis com Confiança ≤ 0 viram agentes hostis internos
- **Sistema de Expedições** — jogador invade o mundo; retaliação gera invasão na cidade
- **Arena — Torneios** com apostas em Ouro
- **Multiplayer na Torre** (raids)
- **Heróis Únicos**, **Permadeath**, **Sacrifício/Síntese**, **PvP invasão assíncrona**
