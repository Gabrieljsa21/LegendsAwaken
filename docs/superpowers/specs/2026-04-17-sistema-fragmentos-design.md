# Design Spec — Sistema de Fragmentos (Substituição do Gacha)

**Data:** 2026-04-17
**Fase:** Pré-Phase Q (decisão de design antes de solidificar código atual)
**Status:** Aprovado — pronto para implementação

---

## 1. Motivação

O sistema de gacha atual gera frustração por dependência de sorte, cria obsolescência por hierarquia linear de raridade (só 5★ importa) e torna personagens de baixa raridade inúteis. A economia depende de RNG, não de progressão.

**Objetivo:** substituir gacha por um modelo determinístico baseado em fragmentos, biomas da Torre, contratos e unlocks por conquista — mantendo progressão, coleção e momentos de recompensa emocional.

---

## 2. Decisões de Design

### 2.1 Timing

Esta mudança ocorre **antes da Fase Q**. O GachaService pode ser removido sem refatoração prévia, limpando dívida técnica antes da passagem de qualidade.

### 2.2 Classificação dos 9 Heróis Fixos

A forma de obtenção comunica o papel do personagem no jogo:

| Tier | Critério | TipoUnlock | Exemplos |
|---|---|---|---|
| **Icônico** | Central no lore, momento de campanha | `MarcoTorre` | Nyra, Kaen |
| **Sistema** | Habilita estratégia, sinergia com sistemas | `Fragmentos` | Heróis de crafting/cidade |
| **Oculto** | Descoberta, recompensa de mastery | `CondicaoUnica` | Herói de alta Confiança, cadeia de missões |

A classificação exata dos 9 heróis é decisão de lore/design do autor — o `HeroiUnlockConfig` no seed reflete essa escolha.

### 2.3 Modelo de Raridade

Raridade é mantida como **atributo nativo** do herói. Define:
- Complexidade do kit
- Especialização
- Impacto dentro de um eixo (combate, cidade, arena)

Raridade **não define** poder absoluto nem hierarquia linear. Um 5★ trabalhador puro é S-tier na cidade e irrelevante na Torre — isso é intencional. Ascensão até 5★ é permitida, mas não apaga a identidade original do herói.

### 2.4 Torre como Sistema Central de Coleção

A Torre é organizada em **biomas por faixa de andares**:

```
Andar 1–10   → Bioma: Floresta
Andar 11–25  → Bioma: Ruínas
Andar 26–40  → Bioma: Vulcânico
(expansível)
```

Dentro de cada bioma:
- **Pool base** (sempre ativo): 2–5 heróis secundários + fragmentos do bioma + materiais de evolução
- **Herói principal**: fragmentos mais raros, dropa ao longo de todo o bioma (baixa taxa), identidade do bioma
- **Andares de marco** (ex: 5, 10, 25): drop garantido ou altamente aumentado do herói principal; possíveis unlocks diretos para heróis icônicos

### 2.5 Sistema de Contratos (substituto dos banners)

Dois níveis de contrato, cumulativos (não multiplicativos entre si):

**Contrato de Arquétipo** (sempre ativo, sem expiração):
- `/contrato arquetipo <Combate|Coleta|Producao>`
- Bônus: +30% sobre base de drop do arquétipo escolhido
- Global (funciona em qualquer bioma)

**Contrato Nomeado** (opcional, custo real, duração limitada):
- `/contrato foco <heroi>`
- Bônus: +50% sobre base para fragmentos daquele herói específico
- Regras de gating: requer ter encontrado ao menos 1 fragmento do herói OU estar no bioma correspondente
- Expiração configurável (ex: 2h, 6h, 24h)
- Custo: ouro ou recurso raro (definido em `ContractConfig`)

**Invariante:** máximo 1 contrato ativo por tipo por usuário.

### 2.6 Momentos de Recompensa (substituto emocional do pull)

Três camadas com pesos diferentes — hierarquia intencional:

| Camada | Trigger | Peso Emocional | Implementação Discord |
|---|---|---|---|
| **Micro** | Fragmento dropado | Leve | Atualização de barra/progresso (`+3 fragmentos de Nyra — 37/50`) |
| **Médio** | Fragmentos completos → unlock | Médio | Embed com arte, nome, lore, animação textual |
| **Alto** | Descoberta de bioma novo | Alto | Embed narrativo apresentando heróis do pool |
| **Alto** | Herói icônico desbloqueado em marco | Máximo | Evento narrativo (herói aparece como boss ou aliado antes do recrutamento) |

---

## 3. O que é Removido

| Removido | Tipo |
|---|---|
| `GachaService` | Serviço |
| `BannerService` | Serviço |
| `BannerHistoricoService` | Serviço |
| `BannerConfiguracao` | Entidade |
| `BannerHistorico` | Entidade |
| `BannerProgresso` | Entidade |
| `RacaChance` | Entidade |
| `/invocar` (pull) | Comando Discord |
| `/banner` / `/pity` | Comandos Discord |

---

## 4. O que é Adicionado

### 4.1 Novos Serviços

| Serviço | Responsabilidade |
|---|---|
| `BiomeService` | Resolve bioma por andar, expõe pool de heróis e marcos |
| `FragmentService` | Processa drops, aplica bônus de contratos, atualiza progresso |
| `RecruitmentService` | Orquestra os 3 caminhos de desbloqueio com idempotência |
| `ContractService` | Gerencia contratos ativos, calcula multiplicador de drop |
| `RewardDistributionService` | Centraliza os 3 picos de recompensa, produz payloads de embed |

### 4.2 Métodos Principais por Serviço

**BiomeService:**
- `ObterBiomaPorAndar(int andar) → Bioma`
- `ObterPoolDoBioma(Guid biomeId) → List<BiomHeroPool>`
- `EAndarDeMarco(int andar) → bool`

**FragmentService:**
- `ProcessarDropAsync(Guid usuarioId, int andar) → FragmentDropResult`
- `ObterProgressoAsync(Guid usuarioId, Guid heroiId) → FragmentoProgresso`
- `AdicionarFragmentosAsync(Guid usuarioId, TipoFragmento tipo, Guid? heroiId, int quantidade)`
- Emite evento `FragmentThresholdReached` ao atingir threshold

**RecruitmentService:**
- `TentarRecrutarPorFragmentosAsync(Guid usuarioId, Guid heroiId) → RecruitmentResult`
- `ProcessarMarcoTorreAsync(Guid usuarioId, int andar) → RecruitmentResult?`
- `DesbloquearPorCondicaoAsync(Guid usuarioId, Guid heroiId) → RecruitmentResult`
- Verifica `HeroiDesbloqueado` antes de qualquer caminho — nunca cria herói duplicado

**ContractService:**
- `AtivarContratoArquetipoAsync(Guid usuarioId, Profissao arquetipo) → Contrato`
- `AtivarContratoNomeadoAsync(Guid usuarioId, Guid heroiId, TimeSpan duracao) → Contrato`
- `ObterMultiplicadorAsync(Guid usuarioId, Guid heroiId) → float`
- `ExpirarContratosVencidosAsync()` — chamado periodicamente

**RewardDistributionService:**
- `GerarMicroPico(FragmentoProgresso progresso) → RewardPayload`
- `GerarPicoMedio(HeroiConfig heroi) → RewardPayload`
- `GerarPicoAlto(TipoEventoAlto tipo, Bioma? bioma, HeroiConfig? heroi) → RewardPayload`

`TipoEventoAlto`: `DescobertaBioma` / `HeroiIconicoDesbloqueado`

### 4.3 Novas Entidades de Domínio

#### Entidades de Configuração (seeded)

**`HeroiConfig`** — base referencial para heróis nomeados
| Campo | Tipo |
|---|---|
| `Id` | Guid |
| `Nome` | string |
| `RaridadeBase` | Raridade |
| `Arquetipo` | Profissao |
| `Tag` | string? |

**`Bioma`**
| Campo | Tipo |
|---|---|
| `Id` | Guid |
| `Nome` | string |
| `AndarInicio` | int |
| `AndarFim` | int |
| `Descricao` | string |
| `Tag` | string? |

**`BiomHeroPool`**
| Campo | Tipo | Nota |
|---|---|---|
| `BiomeId` | Guid | FK → Bioma |
| `HeroiId` | Guid | FK → HeroiConfig |
| `Raridade` | Raridade | |
| `DropWeight` | int | Peso relativo (não probabilidade fixa) |
| `EHeroPrincipal` | bool | |

**`HeroiUnlockConfig`**
| Campo | Tipo | Nota |
|---|---|---|
| `HeroiId` | Guid (PK) | FK → HeroiConfig |
| `TipoUnlock` | enum | `Fragmentos` / `MarcoTorre` / `CondicaoUnica` |
| `QuantidadeFragmentos` | int? | Se `Fragmentos` |
| `AndarMarco` | int? | Se `MarcoTorre` |
| `CondicaoDescricao` | string? | Se `CondicaoUnica` |

> Nota: `HeroiUnlockConfig` usa campos nullable por tipo (polimorfismo via null). Aceitável por agora — separar em `FragmentUnlockConfig` / `MilestoneUnlockConfig` / `ConditionUnlockConfig` se crescer.

#### Entidades de Estado do Jogador (mutáveis)

**`FragmentoProgresso`**
| Campo | Tipo | Nota |
|---|---|---|
| `Id` | Guid | |
| `UsuarioId` | Guid | FK → Usuario |
| `TipoFragmento` | enum | `Heroi` / `Arquetipo` / `Generico` |
| `HeroiId` | Guid? | Preenchido se `Heroi` |
| `Arquetipo` | Profissao? | Preenchido se `Arquetipo` |
| `Quantidade` | int | |
| `AtualizadoEm` | DateTime | |

Para `TipoFragmento.Generico`: ambos `HeroiId` e `Arquetipo` são nulos — representam materiais genéricos do bioma usados em evolução/ascensão.

Índices: `(UsuarioId, HeroiId)` e `(UsuarioId, Arquetipo)`

**`Contrato`**
| Campo | Tipo | Nota |
|---|---|---|
| `Id` | Guid | |
| `UsuarioId` | Guid | FK → Usuario |
| `Tipo` | enum | `Arquetipo` / `Nomeado` |
| `Arquetipo` | Profissao? | Se `Arquetipo` |
| `HeroiId` | Guid? | FK → HeroiConfig, se `Nomeado` |
| `Ativo` | bool | |
| `ExpiraEm` | DateTime? | Nulo para arquétipo |
| `CriadoEm` | DateTime | |

Índice único: `(UsuarioId, Tipo, Ativo = true)` — garante 1 ativo por tipo.
`BonusPercentual` vive em `ContractConfig` no código, não no banco.

**`HeroiDesbloqueado`**
| Campo | Tipo |
|---|---|
| `UsuarioId` | Guid |
| `HeroiId` | Guid |
| `DesbloqueadoEm` | DateTime |

PK composta `(UsuarioId, HeroiId)` — previne re-unlock e duplicação.

---

## 5. Integração com TorreService

`TorreService` não é reescrito — recebe dois pontos de extensão em `SubirAndarAsync`:

### Extensão 1: Drop de fragmentos após combate

```
SubirAndarAsync(usuarioId, herois)
  → [existente] CalcularXp, CalcularOuro, AplicarXp
  → [novo] FragmentService.ProcessarDropAsync(usuarioId, andar)
  → [novo] RewardDistributionService.GerarMicroPico(progresso)     // se houve drop
  → [novo] RecruitmentService.ProcessarMarcoTorreAsync(usuarioId, andar)  // se marco
```

### Extensão 2: Detecção de bioma novo

```
SubirAndarAsync(usuarioId, herois)
  → BiomeService.ObterBiomaPorAndar(andar)
  → Se bioma diferente do andar anterior:
      RewardDistributionService.GerarPicoAlto(DescobertaBioma, bioma)
```

### Campos adicionados a `SubirAndarResult`

| Campo | Tipo | Descrição |
|---|---|---|
| `Fragmentos` | `List<FragmentDropResult>` | Fragmentos dropados neste andar |
| `NovoBioma` | `Bioma?` | Preenchido na primeira entrada em um bioma |
| `HeroiDesbloqueado` | `HeroiConfig?` | Preenchido se marco disparou unlock |
| `RewardPayloads` | `List<RewardPayload>` | Embeds a enviar no Discord |

---

## 6. UX — Comandos como Entry Points

**Princípio:** comandos `/` apenas abrem painéis interativos. Lógica executa via botões/selects dentro dos painéis.

| Comando | Painel Aberto | Ações no Painel |
|---|---|---|
| `/colecao` | Lista de heróis com barra de progresso, estado `locked/unlocked/???` | Botão "Recrutar" (aparece só com fragmentos completos) |
| `/bioma` | Nome, descrição, herói principal em destaque, pool, marcos, progresso | Botão "Ver Pool", botão "Contratos" |
| `/contrato` | Arquétipo ativo + nomeado + tempo restante | Selects para alterar arquétipo / ativar foco |

**`/recrutar` não existe como comando direto** — recrutamento ocorre via botão em `/colecao` ou automaticamente ao completar fragmentos.

**`/bioma` é o equivalente emocional do banner** — apresenta o ecossistema atual sem RNG, com foco no herói principal e progresso do jogador naquele bioma.

### Picos de recompensa no Discord

| Camada | Formato |
|---|---|
| Micro | Barra de progresso atualizada + mensagem leve |
| Médio | Embed com arte + lore + animação textual (recrutamento) |
| Alto (bioma) | Embed narrativo de descoberta |
| Alto (icônico) | Evento narrativo — herói aparece como boss/aliado antes do unlock |

---

## 7. O que Não Muda

- `HeroiLevelUpService` e toda a progressão por raridade
- `TorreService` (exceto os dois pontos de extensão acima)
- Entidade `Heroi` — raridade continua campo nativo
- `CombatService`, fórmulas de dano, ATB
- `CidadeService`, alocação, recursos
- Todos os outros sistemas existentes

---

## 8. Configurações em Código (não no banco)

**`ContractConfig`:**
```csharp
public static class ContractConfig
{
    public const float ArchetypeBonus = 0.30f;  // +30% sobre base
    public const float NamedBonus = 0.50f;      // +50% sobre base (não multiplicativo)
}
```

Bônus são aditivos: `multiplicadorFinal = 1.0f + arquétipoBonus + nomeadoBonus` (quando aplicável).

---

## 9. Abordagem de Implementação

**Big Bang (Abordagem 1):** remoção completa do sistema de gacha + construção do sistema novo em paralelo.

Ordem de implementação sugerida:
1. Remover entidades Banner/* e serviços de gacha
2. Criar migration com novas entidades
3. Implementar `HeroiConfig` seed data (classificar os 9 heróis)
4. Implementar `BiomeService` + seed de biomas
5. Implementar `FragmentService`
6. Implementar `ContractService`
7. Implementar `RecruitmentService`
8. Implementar `RewardDistributionService`
9. Estender `TorreService` (dois pontos de extensão)
10. Implementar painéis Discord (`/colecao`, `/bioma`, `/contrato`)
