# Changelog — Legends Awaken

Histórico de mudanças por fase de desenvolvimento. Versões seguem progressão das fases do ROADMAP.

---

## [0.5.0] — 2026-04-18 · Fase 3A.3 — Sistema de Fragmentos

### Removido
- `GachaService` e `BannerService` — sistema de gacha eliminado integralmente
- Entidades `Banner`, `BannerHeroiPool` e DTO `GachaResultadoDTO`

### Adicionado
**Domínio**
- Entidades: `HeroiConfig`, `HeroiUnlockConfig`, `Bioma`, `BiomHeroPool`, `FragmentoProgresso`, `Contrato`, `HeroiDesbloqueado`
- Enums: `TipoFragmento`, `TipoUnlock`, `TipoContrato`, `TipoEventoAlto`, `TipoReward`
- Interfaces de repositório: `IHeroiConfigRepository`, `IHeroiDesbloqueadoRepository`, `IFragmentoRepository`, `IBiomaRepository`, `IContratoRepository`

**Application**
- `BiomeService` — mapeamento andar→bioma; detecção de bioma novo e marco da Torre
- `FragmentService` — drops ponderados por bioma com multiplicador de contrato; upsert TOCTOU-safe
- `RecruitmentService` — 3 caminhos de desbloqueio: fragmentos, marco da Torre, condição única
- `ContractService` — contratos arquétipo (+30%) e nomeado (+50%); expiração automática
- `RewardDistributionService` — factory de payloads `Micro` / `Médio` / `Alto` por tipo de evento
- DTOs: `ContractConfig`, `FragmentDropResult`, `RecruitmentResult`, `RewardPayload`

**Infrastructure**
- Repositórios EF Core para os 5 novos contratos de repositório
- Migration `FragmentoSystem` — 7 novas tabelas
- Migration `FragmentoSystemIndexes` — partial unique indexes em `FragmentosProgresso` e `Contratos`
- Seed: 9 heróis com `HeroiUnlockConfig`; 5 biomas com pools de drop ponderados

**Bot**
- `DiscordIdHelper.ToGuid(ulong)` — conversão determinística Discord ID → Guid (little-endian via `BinaryPrimitives`)
- Painéis: `ColecaoPanel`, `BiomaPanel`, `ContratoPanel`
- Comandos: `/colecao`, `/bioma`, `/contrato`
- `SelectMenuExecuted` wired no `CommandHandler` (antes apenas `ButtonExecuted` era subscrito)

### Alterado
- `TorreService.SubirAndarAsync` — estendido com drop de fragmentos, detecção de bioma novo e desbloqueio de herói por marco
- `SubirAndarResult` — ampliado de 4 para 8 campos: `+ Fragmentos`, `+ NovoBioma`, `+ HeroiDesbloqueado`, `+ RewardPayloads`
- `CommandHandler` — 8 novos campos injetados; 3 novos slash commands; handlers para `colecao_recrutar`, `contrato_arquetipo`, `bioma_ver_colecao`, `bioma_contratos`, `contrato_remover_nomeado`
- `CommandHandler` — logging estruturado (`LogInformation("... {Param}", value)`) nos pontos de entrada de slash commands
- `TipoReward` movido de `Application/DTOs` para `Domain/Enum/Enums.cs`

### Testes
- 39 testes unitários adicionados: `BiomeServiceTests`, `FragmentServiceTests`, `ContractServiceTests`, `RecruitmentServiceTests`, `TorreServiceExtensionTests`

---

## [0.4.0] — 2026-04-16 · Fase 3A.2 — Consolidação do Core

### Adicionado
- **Cidade — Modelo de Slots**: `SlotOcupacao` entity + `ISlotOcupacaoRepository` + migration `CidadeSlotModel3A2`
- Campos `Confianca` (0–100) e `Humor` (0–100) na entidade `Heroi`
- **ResourceNode** (Campo/Floresta/Mina/Prado) — tier 1 de produção sem slot, com bônus de profissão
- **Slots de Responsabilidade e Operação** por prédio — prédio inativo se responsabilidade vazia
- **Humor da Cidade** = média dos heróis alocados × multiplicador (0.9/1.0/1.1/1.2)
- `PredioConfig` e `ResourceNodeConfig` — configs estáticas sem hardcode
- `/cidade construir`, `/cidade alocar_recurso`, `/cidade alocar_predio`
- Campo `Lore` na entidade `Heroi`
- Seed de 9 personagens fixos 5★/4★ via `GeracaoDeDadosService` (idempotente)
- **Check de qualidade no crafting**: `skill_craft + bônus_prédio(Nivel×2) + roll(1..20)` via Responsável da Forja
- `/treinar` via `ArenaService.TreinarAsync` — XP em burst (3×), 4h cooldown, custo Ouro + Comida
- `/arena desafio` — desafio de ondas com cooldown 24h, top-5 heróis automático
- Ouro por andar da Torre: `5 + Numero×3` × boss_mult

### Alterado
- `/cidade ver` reworked — coletores com taxa, prédios com slots + heróis, HumorCidade
- Fórmula de produção Tier 2: `BaseProd × MultResp × SomaOp × HumorMult × horas`

---

## [0.3.0] — 2026-04-15 · Fase 3A.1 — Loop Jogável Mínimo (Vertical Slice)

### Adicionado
- **Fórmula de dano**: `ATK × SkillMult × (1 - DEF/(DEF+1000+Level×50)) × TypeMult`; crit 1.5×; burst cap 65%
- **Ordem de turno ATB**: `InitScore = Agilidade + Random(0, Agilidade×0.1)`
- **Curva de XP (linear)**: `XP_next = B_r × nível` com `B_r` em `RaridadeConfig.BaseXp`
- Stats base por raridade via `ObterAtributosBaseParaRaridade` aplicados na criação do herói
- Bônus racial (+50 no atributo foco) aplicado na criação via `HeroiLevelUpService.BonusRacial`
- XP ganho ao subir andar da Torre (`TorreService.SubirAndarAsync → AplicarXp`)
- Level-up com distribuição de pontos e verificação de cap
- Bloqueio de XP ao atingir o cap (XP zerado, level travado até ascensão)
- 5 receitas de crafting estáticas — espada-ferro, arco-simples, armadura-couro, anel-arcano, amuleto-agilidade
- `/crafting listar` e `/crafting fazer <receitaId>`
- `/heroi_equipar` — equipa item em herói, persiste bônus via `HeroiBonusAtributo`
- `AtributosBase.Get/Set`, `Distribute`, `With`, `ToEnumerable` via `Enum.GetValues`

### Marco
> Um jogador novo entra, invoca, sobe a torre, coleta recursos, crafta e equipa um item. Build: 0 warnings, 0 errors.

---

## [0.2.0] — 2026-04-11 · Fase 2 — Protótipo da Cidade

### Adicionado
- Produção passiva por profissão com cap de 24h
- `/cidade ver`, `/cidade coletar`, `/cidade alocar`, `/cidade desalocar`
- Recurso Erva adicionado
- `CidadeRepository` reescrito em EF Core
- Distribuição de raças por raridade (1★/2★ = humano; 3★ = 10% não-humano; 4★ = 25%)
- Distribuição uniforme entre raças não-humanas
- `HeroiLevelUpService` com `RaridadeConfig` — caps, ganhos e grants calculados sem números mágicos

---

## [0.1.0] — 2026-04 · Fase 1 — Pré-produção + Fundação

### Adicionado
- Clean Architecture em 6 projetos: Domain, Application, Infrastructure, Bot, Data, Tests
- Entidade `Heroi` com atributos base, raça, profissão, habilidades, status de combate
- Sistema de gacha com soft-pity em curva cúbica por banner *(removido em 0.5.0)*
- Geração procedural de heróis (raça, profissão, atributos, habilidades)
- Torre infinita com tipos de andar variados e bosses em andares 5/10/25
- Combate automático por turnos
- Party de até 5 heróis
- Listagem paginada com botões Discord (25 por página)
- Autocomplete para nomes de heróis
- Token do bot via variável de ambiente (`LEGENDSAWAKEN_TOKEN`)
- Seed data de habilidades em JSON
- `RaridadeConfig` — caps, stats base e ganhos por level centralizados (SOLID)
