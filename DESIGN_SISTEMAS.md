# Legends Awaken — Design de Sistemas (Fórmulas e Frameworks Matemáticos)

> Este documento captura os frameworks matemáticos que governam os sistemas do LA.  
> O GDD descreve **o quê** os sistemas fazem; este documento descreve **como** funcionam matematicamente.  
> Fases de implementação seguem o ROADMAP.md.

---

## 1. Curva de XP e Progressão de Nível

### 1.1 Fórmula Definitiva

```
XP_next(l, r) = B_r × l^1.25
```

- `l` = nível atual  
- `B_r` = base por raridade (calibrar no beta)  
- Expoente `1.25` gera curva suave: early acelera, endgame estabiliza

**Valores iniciais de B_r (ajustar no beta):**

| Raridade | B_r | Cap | XP total estimado até o cap |
|---|---|---|---|
| 1★ | 80 | 20 | ~34.000 |
| 2★ | 100 | 40 | ~97.000 |
| 3★ | 120 | 60 | ~210.000 |
| 4★ | 150 | 80 | ~400.000 |
| 5★ | 200 | 100 | ~810.000 |

> **Implementação inicial (Fase 3A):** usar `XP_next = B_r × nível` (linear, sem expoente) para desbloqueio rápido; migrar para `l^1.25` quando houver dados de jogo real.

### 1.2 XP Budget por Tempo (modelo de pacing)

```
T_level(l) = 8 × l^0.45   (minutos)
XP_budget(l) = XP_rate × T_level(l)
```

- `XP_rate` = XP por minuto desejado (parâmetro global de pacing)  
- Ajustar `XP_rate` muda a velocidade do jogo inteiro sem mexer em sistemas individuais

### 1.3 Cap de Eficiência por Fonte (anti-farm único)

```
Efficiency_cap = XP_ganho(fonte) / max(XP_todas_as_fontes)
XP_efetivo = XP × (1 - Efficiency_cap^1.5)
```

- Impede que Torre (ou qualquer fonte) domine 100% do ganho de XP  
- Outras fontes ficam viáveis automaticamente

---

## 2. Power Score Unificado (GS — Game Score)

Score escalar que representa o poder real de um herói.

### 2.1 Fórmula Principal

```
HeroPowerScore =
  [(BaseStats × LevelFactor × GrowthFactor) × RaceModifier]
  + GearPower
  + SkillPower
  + RelicPower
```

### 2.2 Componentes

**BaseStats (atributos puros):**
```
BaseStats = (FOR × 1.2) + (AGI × 1.0) + (INT × 1.1) + (VIT × 0.9) + (PER × 1.0)
```

**LevelFactor (escala controlada):**
```
LevelFactor = 1 + (Level^1.25) / K
K = 100  (constante de normalização global)
```

**GrowthFactor (raridade × classe):**
```
GrowthFactor = RarityMultiplier × ClassScaling

RarityMultiplier:  1★=0.8  |  2★=1.0  |  3★=1.15  |  4★=1.35  |  5★=1.6
```

**RaceModifier:**
```
Humano = 1.00  |  Bestial = 1.07  |  Anão = 1.05
Elfo = 1.03    |  Draconato = 1.06  |  Fada = 1.04
```

**GearPower (diminishing returns):**
```
GearPower = Σ(ItemScore^0.85 × SlotWeight)

SlotWeight:  Arma=1.0  |  Armadura=0.8  |  Acessório=0.6  |  Utilidade=0.4
```

**SkillPower:**
```
SkillPower = Σ(DPSContribution × SkillLevel / 10)
```

**RelicPower:**
```
RelicPower = Σ(RelicEffectScore × SynergyMultiplier)
```

### 2.3 Separação Sheet vs Combat Score

| Score | Uso | Inclui buffs? |
|---|---|---|
| `SheetPowerScore` | UI, ranking, missões | Não |
| `CombatPowerScore` | Simulação real de batalha | Sim |

> Missões usam SheetPowerScore. Combate interno usa CombatPowerScore.

### 2.4 Soft Cap Global

```
FinalPower = HeroPowerScore / (1 + log(HeroPowerScore / 5000))
```

Evita inflação infinita de números no late game.

---

## 3. Sistema de Combate — Core Matemático

### 3.1 Fórmula de Dano

```
RawDamage = ATK × SkillMultiplier
DamageReduction = DEF / (DEF + K)    onde K = 1000 + (Level × 50)
FinalDamage = RawDamage × (1 - DamageReduction) × TypeMultiplier
```

**Tabela de redução defensiva (K=1000, lv1):**

| DEF | Redução | DEF remanescente |
|---|---|---|
| 100 | 9% | 91% |
| 500 | 33% | 67% |
| 1.000 | 50% | 50% |
| 2.000 | 67% | 33% |

> DEF nunca zera dano. K escala com Level, impedindo tanks onipotentes no late game.

**SkillMultiplier padrão por tipo:**

| Tier da skill | Multiplicador |
|---|---|
| Ataque básico | 1.0× |
| Habilidade leve | 1.4× |
| Habilidade pesada | 2.0× |
| Ultimate | 3.0–4.0× |

### 3.2 Crítico

```
CritChance = BaseCrit + CritStat / 500
CritDamage = 1.5×  (default; upgrades chegam até 2.0×)

if Crit: FinalDamage *= CritMultiplier
```

### 3.3 TypeMultiplier (vantagem de elemento)

```
Vantagem:   1.25×
Neutro:     1.00×
Desvantagem: 0.75×
```

### 3.4 Ordem de Turno (ATB Simplificado)

```
InitScore = Speed + Random(0, Speed × 0.1)
Delay = 100 / Speed
```

Maior `InitScore` age primeiro. Após agir, o herói espera `Delay` ticks antes do próximo turno. Mais Speed = mais ações por ciclo, não só prioridade.

### 3.5 Burst Cap (anti one-shot)

```
MaxDamagePerHit ≤ 0.65 × HP_máximo_alvo
```

Nenhum hit único pode remover mais de 65% da vida base do alvo. Críticos podem atingir o cap mas não ultrapassá-lo.

### 3.6 Pipeline Completo de Combate

```
1. Calcular InitScore → ordenar turno
2. Calcular RawDamage (ATK × SkillMultiplier)
3. Aplicar DamageReduction (DEF / DEF+K)
4. Aplicar TypeMultiplier
5. Aplicar Crit (se ocorreu)
6. Aplicar BurstCap
7. Aplicar buffs/debuffs ativos (aditivos dentro da categoria)
8. Registrar resultado no CombatLog
```

---

## 4. P0 Engine — Sistema de Balanceamento de Poder

Define o "poder médio esperado" do jogador por fase, servindo de régua universal de balanceamento.

### 4.1 Fórmula Base

```
P0(f) = A × f^k

A = base de poder inicial (ex: 10)
f = índice de fase (1 a 5)
k = 1.6 a 2.2
```

### 4.2 Fases e Ranges

| Fase | Level range | P0 esperado (A=10, k=1.8) |
|---|---|---|
| F0 | 1–20 | ~10 – 120 |
| F1 | 21–40 | ~120 – 380 |
| F2 | 41–60 | ~380 – 950 |
| F3 | 61–80 | ~950 – 1.800 |
| F4 | 81–100 | ~1.800 – 3.200 |

### 4.3 Decomposição do P0 (Power Budget)

P0 = 100% — distribuído entre sistemas:

| Fonte | % do poder total | Cap máximo de bônus |
|---|---|---|
| Level base | 25% | — |
| Skills | 30% | +120% |
| Equipamentos | 20% | +100% |
| Relíquias | 10% | +60% |
| Raça / Profissão | 10% | — |
| Buffs de Cidade | 3% | +40% |
| Confiança / sistema social | 2% | — |

> **Regra de ouro:** nenhuma fonte pode ultrapassar 50% isolada. Se isso ocorrer, builds ficam obrigatórias e o balanceamento quebra.

### 4.4 Fórmula de Poder Total

```
TotalPower = Base × (1 + ΣSkills) × (1 + ΣEquip) × (1 + ΣRelics) × (1 + ΣBuffs)
```

- Aditivo dentro da categoria  
- Multiplicativo apenas entre categorias  
- Nunca multiplicação livre entre todas as fontes

### 4.5 Crescimento por Nível dentro da Fase

```
P(level) = P0(fase) × (1 + r)^n

Taxas r por fase:
  F0 → 0.07   F1 → 0.05   F2 → 0.035   F3 → 0.02   F4 → 0.01–0.015
```

### 4.6 Desvio de Balanceamento

| Desvio do P0 | Estado |
|---|---|
| 0–10% | Normal |
| 10–20% | Herói forte |
| 20–30% | Build otimizada |
| 30–40% | Build extrema |
| >40% | Quebra de balanceamento |

> O jogo deve **permitir intencionalmente** desvios até ~30%. Acima disso é sempre exploit ou tuning errado.

### 4.7 Regra de Expansão (Anti-Power Creep)

Todo sistema novo **não pode aumentar P0 global**. Deve ser:
1. Redistributivo (move poder entre sistemas)
2. Substitutivo (troca por outro sistema)
3. Condicional (trade-off real)

---

## 5. CDI — Content Difficulty Index

Índice unificado de dificuldade para todos os conteúdos. Permite comparar Torre, Missões, Invasões e Expedições na mesma escala.

### 5.1 Fórmula Base

```
CDI = P0_reference × (1 + GrowthRate)^StageIndex × DifficultyClass
```

**DifficultyClass padrão:**

| Classe | Multiplicador |
|---|---|
| Fácil | 0.85 |
| Normal | 1.00 |
| Difícil | 1.25 |
| Elite | 1.60 |
| Nightmare | 2.10 |

### 5.2 CDI por Sistema

**Torre:**
```
TowerCDI(floor) = P0_ref × (1 + 0.10)^floor × FloorModifier

FloorModifier:
  1–20:   0.80   (tutorial)
  21–80:  1.00   (core)
  81–150: 1.25   (endgame)
  150+:   1.50+  (infinito)
```

**Missões:**
```
MissionCDI = P0_ref × MissionTier × NarrativeWeight

MissionTier:  T1=0.8  |  T2=1.0  |  T3=1.3  |  T4=1.6  |  T5=2.0
NarrativeWeight: normal=1.0 | elite=1.3 | boss=1.8 | clímax=2.2
```

**Invasões:**
```
InvasionCDI = P0_ref × (1.06)^wave × ThreatLevel

ThreatLevel: Local=1.0 | Regional=1.4 | Global=1.8 | Cataclismo=2.3
```

**Expedições:**
```
ExpeditionCDI = P0_ref × (1.07)^depth × RiskModifier × UnknownFactor

RiskModifier: Seguro=0.9 | Standard=1.0 | Perigoso=1.3 | Letal=1.7
UnknownFactor: 0.9–1.5 (variância controlada)
```

### 5.3 Regra Fundamental

```
PlayerPowerScore_avg ≈ CDI_target
```

Sempre calibrado para o P0 médio esperado. Nunca baseado em feeling manual.

### 5.4 Clamp Obrigatório (anti-bug)

```
CDI_min = PlayerPowerScore × 0.6
CDI_max = PlayerPowerScore × 1.8
```

Evita conteúdo impossível ou trivial demais.

---

## 6. Economia Global — Modelo Matemático

### 6.1 Condição de Equilíbrio

```
Gold_net(t) = Gold_sources(t) - Gold_sinks(t)

Early game: Gold_net > 0   (crescimento)
Mid game:   Gold_net ≈ 0   (estabilidade)
Late game:  Gold_net ≤ 0   (pressão leve)
```

### 6.2 Fontes (Sources)

**Torre (fonte principal):**
```
Gold_tower = B × (1 + 0.15 × floor) × (1 + team_bonus)

FatigueFactor = e^(-runs_per_day / 5)   (anti-farm infinito)
Gold_tower_efetivo = Gold_tower × FatigueFactor
```

**Missões:** 25–35% da economia diária  
**Arena/PvP:** 10–20% da economia diária  
**Produção da Cidade:** passivo controlado, cresce com infraestrutura

### 6.3 Sumidouros (Sinks)

**Manutenção da Cidade:**
```
Upkeep_city = BaseCityCost × SizeMultiplier^1.15 × (1 + 0.12 × BuildingLevelSum)

SizeMultiplier: Pequena=1.0 | Média=2.5 | Grande=6.0 | Metrópole=12.0
```

**Sustento de Heróis:**
```
Hero_upkeep = TierBaseCost × (1 + Level / 100)
```

### 6.4 Pressão Econômica Dinâmica

```
EconomyPressure = 1 + log(1 + AvgGold / K)

Upkeep_city *= EconomyPressure
Hero_upkeep *= EconomyPressure
```

Garante que a economia "respira" com o progresso dos jogadores.

### 6.5 Anti-Snowball

```
CatchUpFactor = 1 / (1 + WealthGap)
Reward = BaseReward × CatchUpFactor
```

Aplicar em: missões, Torre, drops, recompensas.

### 6.6 Ouro Travado (Delayed Economy)

```
Gold_total = Gold_liquid + Gold_locked
Gold_locked = Construção + Treino + Investimentos

Liberação: d(Gold_locked)/dt = -k × Gold_locked
```

Evita explosão de poder instantânea e cria sensação real de planejamento.

### 6.7 Dívida / Déficit

```
Debt = max(0, Upkeep - Gold_liquid)
EfficiencyPenalty = 1 / (1 + Debt / K)
```

Aplicado em: produção da cidade, velocidade de construção.

---

## 7. Torre — Sistemas Avançados

### 7.1 Estado Pós-Conclusão (Andar Dominado)

Ao atingir 100% em um andar:

| % Overclear | Estado | Risco de eventos para workers |
|---|---|---|
| 100% | Dominado (instável) | Base |
| 120% | Estável | -30% |
| 150% | Controlado | -60% + bônus de produção |

```
chanceEventoPorHora =
  base
  × fatorAndar
  × (1 - bonusOverclear)
  × (1 - bonusParty)
  × (1 - bonusRelíquias)

Tipos: leve (perda eficiência) | médio (combate) | grave (risco de perda)
```

### 7.2 Sistema de Energia do Andar

Cada andar tem uma "energia dominante" que muda dinamicamente:

| Energia | Efeito em inimigos | Sinergia |
|---|---|---|
| Corrupção | Regeneram HP por turno | Relíquias de necrótico |
| Ordem | Buffs previsíveis e empilháveis | Composições consistentes |
| Caos | Efeitos aleatórios por turno | Builds de burst |
| Vida | Cura passiva (aliados e inimigos) | Builds de atrito |
| Morte | DoT em todos os combatentes | Builds de sustain |

> Evita combate repetitivo sem criar novos mobs — variação via modificador de energia.

### 7.3 Sistema de Fadiga da Torre

```
Fadiga acumula por hora de operação no mesmo andar.

Efeito:
  Baixa fadiga  → produção normal
  Média fadiga  → -20% produção
  Alta fadiga   → risco de falha em eventos

Resolve: obriga rotação de heróis; conecta com Sistema de Sustento.
```

### 7.4 Elite Persistente (Mini-Nemesis)

Certos inimigos não morrem definitivamente:
- Foge ao ser derrotado pela primeira vez
- Retorna em run futura mais forte (+1 habilidade nova)
- Aprende contra a composição que o derrotou

> Cria narrativa emergente e rivalidade. Implementar como flag na entidade do andar.

### 7.5 Run Entropy (Degradação de Run)

```
tempo no andar ↑  →  risco ↑  →  eventos ↑  →  eficiência ↓
```

Evita exploração infinita e pressiona decisão. Combina com o Modo Operação.

### 7.6 Economia Própria da Torre

Recursos exclusivos da Torre com uso próprio (não substituem ouro global):

| Recurso | Obtido via | Uso |
|---|---|---|
| Essência do Andar | Completar andares | Ativa mutadores de run |
| Núcleo Instável | Boss floors | Componente de relíquias avançadas |
| Fragmento de Memória | Eventos secretos | Altera comportamento de andares |

### 7.7 Checkpoints de Progressão

Checkpoints a cada X andares, mas com custo:

| Opção | Efeito |
|---|---|
| Salvar progresso | Custa recurso da Torre |
| Continuar sem salvar | Bônus de recompensa final |

### 7.8 Sinergia Torre ↔ Cidade

| Prédio | Impacto na Torre |
|---|---|
| Torre de Vigilância | Previsão de eventos; reduz UnknownFactor |
| Quartel | +10% atributos de combate da party |
| Templo | Resistência a condições de status |
| Laboratório | Consumíveis automáticos em eventos críticos |

---

## 8. Modelos de Dados Canônicos

Referência para refatoração futura da camada de domínio. Cada sistema opera sobre estados serializáveis.

### 8.1 HeroState (runtime)

```csharp
// Campos relevantes para implementação atual
{
  Id, Level, Exp,
  Stats: { HP, ATK, DEF, SPD, CRIT, CRITDMG },
  SkillLevels: Dictionary<SkillId, int>,
  Gear: { Weapon?, Armor?, Accessories[] },
  StatusEffects: List<StatusEffect>,
  Fatigue: float,           // Torre Operação
  Ascension: int, Rarity: int,
  Modifiers: List<Modifier>
}
```

### 8.2 CombatRunState

```csharp
{
  RunId, Floor, Turn,
  Heroes: HeroState[],
  Enemies: EnemyState[],
  Phase: Preparation|InCombat|Reward|Fail,
  RngSeed: long,           // determinismo obrigatório
  Rewards: Reward[],
  Modifiers: Modifier[],
  CombatLog: CombatEvent[] // para replay e debug
}
```

### 8.3 Modifier (sistema de stacking)

```csharp
{
  Id, SourceId,
  Stat: StatType,
  Operation: ADD|MULT|OVERRIDE,
  Value: float,
  Duration?: int,   // null = permanente
  Stackable: bool,
  Priority: int     // ordem de aplicação
}
```

### 8.4 Regras Críticas

1. **Single Source of Truth** — cada dado existe em um único lugar; nada é calculado "solto"  
2. **Simulação pura** — `nextState = simulate(prevState, input)` sem efeitos colaterais  
3. **Serialização obrigatória** — todo state é saveável e replayável  
4. **Separação Definition vs State** — `HeroDefinition` (dados de design fixos) ≠ `HeroState` (runtime do save)

---

## 9. Sistema de Tempo e Simulação (Referência de Arquitetura)

Modelo de referência para quando o sistema crescer. Não implementar agora.

### 9.1 Três Resoluções de Tempo

| Camada | Uso | Exemplos |
|---|---|---|
| TG_fast | Alta frequência | Combate, AI, input |
| TG_mid | Sistêmica | Missões, crafting, expedições |
| TG_slow | Estrutural | Cidade, economia, construção |

### 9.2 Regras Críticas

- `OfflineProgress = Σ(DeferredSystems × elapsed_time × efficiency_factor)`
- `MaxOfflineTime = f(player_progress)` — cap anti-exploit offline
- `OfflineEfficiency = clamp(0.2 → 1.0)` — garante que offline nunca supere online
- Nenhum recurso pode ser gerado por 2 sistemas no mesmo tick sem ownership definido (evita double counting)

---

## 10. AI de Heróis — Fases Futuras (Fase 4+)

Sistema de NPC Comandante, Personalidade Evolutiva e Linhagem Comportamental.  
**Não implementar antes de Fase 3B validada.**

### 10.1 NPC Comandante (Fase 4)

```
Score = (RulePriority × Strictness × Loyalty)
      + PersonalityBias
      - RiskPenalty
      + RandomNoise

PersonalityVector = [Aggression, Caution, Discipline, Loyalty, Greed, RiskTolerance]
  cada eixo varia de 0.0 a 1.0
```

### 10.2 Evolução de Personalidade

```
ΔTrait = (OutcomeImpact × Weight) × ExperienceIntensity × PersonalityPlasticity
Plasticity = 1 / (1 + HeroLevel × 0.05)  ← heróis fortes mudam menos

MaxΔTraitPerBattle = 0.05  ← evita extremos abruptos
Trait pull-to-mean: Trait += (0.5 - Trait) × StabilityFactor  ← evita deriva permanente
```

### 10.3 Tipos Emergentes de Herói

| Tipo | Perfil | Gameplay |
|---|---|---|
| Executor Perfeito | Alta disciplina + lealdade | Segue regras quase 100% |
| Berserker Evolutivo | Alta agressão + risco | Ignora parcialmente o jogador |
| Guardião Estável | Alta cautela | Consistente, previsível |
| Instável (Wild Card) | Alta aleatoriedade | Comportamento imprevisível |

### 10.4 Linhagem Comportamental (Fase 4+)

```
RecrutaPersonality =
  BaseArchetype
  + Σ(MentorTrait × ExposureTime × Affinity)
  + DriftRandomness

InheritanceRate = 0.10 → 0.40  (nunca 100% — evita clones)
```

> Resultado: jogadores não treinam só stats — cultivam uma **cultura de combate** própria.

---

## 11. Sistema de Fragmentos — Matemática

> Implementado na Fase 3A.3. Substitui o gacha por aquisição determinística.

### 11.1 Drop de Fragmentos

```
DropOcorre = Random(0, 1) < ChanceDropBase

ChanceDropBase = 0.30   (constante em ContractConfig)
```

Ao limpar um andar da Torre, há 30% de chance de drop. Não há pity acumulativo — a chance é estática por design para evitar garantias implícitas.

### 11.2 Seleção de Herói por Peso

```
HeroiSelecionado = SelecionarPorPeso(pool_do_bioma)

Algoritmo:
  totalPeso = Σ(BiomHeroPool.Peso)
  if totalPeso <= 0: retorna pool[0]   ← guard obrigatório

  rolagem = Random(0.0, totalPeso)
  acumulado = 0
  para cada entry em pool:
    acumulado += entry.Peso
    if rolagem <= acumulado: retorna entry
  retorna pool[^1]   ← fallback de floating-point drift
```

Pesos são absolutos (ex: 10, 5, 2) — a chance de um herói é `Peso / totalPeso`. Heróis mais raros recebem peso menor.

### 11.3 Multiplicador de Contrato

```
Multiplicador = 1.0
             + (ArquetipoBonus se contrato ativo de arquétipo E profissão combina)
             + (NomeadoBonus  se contrato ativo nomeado E heroiId combina)

ArquetipoBonus = 0.30
NomeadoBonus   = 0.50

Multiplicador máximo teórico = 1.80 (ambos ativos e compatíveis)
```

Os bônus são **aditivos** entre si (não multiplicativos), evitando explosão de fragmentos com dois contratos simultâneos.

### 11.4 Quantidade de Fragmentos por Drop

```
QuantidadeFinal = Ceiling(QuantidadeBase × Multiplicador)

QuantidadeBase = 1   (padrão; configurável por pool futuramente)
```

`Math.Ceiling` garante que qualquer bônus de contrato (mesmo 0.01) resulte em pelo menos 1 fragmento extra — o jogador sempre percebe o benefício do contrato.

### 11.5 Desbloqueio por Fragmentos

```
Desbloqueio ocorre quando:
  FragmentoProgresso.Quantidade >= HeroiUnlockConfig.QuantidadeFragmentos
```

Threshold definido por herói na seed. Exemplos de calibração:

| Herói | Raridade | Threshold | Drops estimados |
|---|---|---|---|
| Hana (Agricultor) | 4★ | 20 | ~67 andares no bioma |
| Grom (Mineiro) | 4★ | 30 | ~100 andares no bioma |
| Aldric (Guerreiro) | 5★ | 50 | ~167 andares no bioma |

*Estimativa assume drop 30%, sem contrato, herói único no pool.*

### 11.6 Biomas e Mapeamento de Andares

```
BiomaPara(andar) = biomas.FirstOrDefault(b => b.AndarInicio <= andar && b.AndarFim >= andar)
EBiomaNovo(andar) = andar == bioma.AndarInicio && andar > 1
EAndarDeMarco(andar) = existe HeroiUnlockConfig com TipoUnlock=MarcoTorre e AndarMarco == andar
```

Biomas são faixas fixas de andares definidas no seed. A detecção de bioma novo ocorre no andar de entrada (`AndarInicio`), disparando o payload de reward `TipoEventoAlto.DescobertaBioma`.

### 11.7 Contratos — Invariantes

```
Por usuário:
  MAX 1 contrato ativo de tipo Arquetipo
  MAX 1 contrato ativo de tipo Nomeado

Pré-condição para Nomeado:
  FragmentoProgresso.Quantidade >= 1   ← exige ter visto o herói pelo menos uma vez

Ativação de Arquétipo:
  Desativa o anterior (sem confirmação) → salva novo
  As duas operações devem ocorrer na mesma transação DB   ← ponto de atenção de qualidade
```

---

## 12. Sistema de Vitalidade — Ferimento Persistente (design candidato, Fase 3B-TorreExp)

> Inspiração: D&D UA Regras Variantes — sistema de Vitalidade. Endereça o pedido de "heróis feridos" após falha na Torre de forma mais rica do que simples cooldown.

### 12.1 Conceito

Heróis que falham em missões sofrem **ferimento persistente**: o HP máximo efetivo é reduzido até a recuperação completa. Cria necessidade real de downtime/recuperação e torna falhas mais consequentes sem ser punitivo demais.

### 12.2 Fórmula

```
HP_máximo_efetivo = HP_base × (1 - FerimentoFator)

FerimentoFator  = danoRecebidoNaRun / HP_base   (clamp 0.0–0.8)
RecuperacaoBase = 0.10 por hora de descanso      (herói Inativo ou em cidade)
RecuperacaoMult = 1.0 | 1.5 (Banhos/Santuário)  (futuro building)
```

**Estado derivado do FerimentoFator:**

| FerimentoFator | Estado | Símbolo | Efeito visível |
|---|---|---|---|
| 0.00 | Íntegro | ✅ | Normal |
| 0.01–0.24 | Levemente ferido | 🩹 | HP máx -5% a -24% |
| 0.25–0.49 | Ferido | 🩸 | HP máx -25% a -49%; bloqueado na Arena |
| 0.50–0.80 | Gravemente ferido | ☠️ | HP máx -50%+; bloqueado em Torre e Arena |

### 12.3 Integração com Sistemas Existentes

- `Heroi.FerimentoFator` (float, 0.0–0.8) — campo a adicionar na migração 3B-TorreExp
- `SustentoService.ProcessarAsync` já roda em todo comando — tick de cura natural integrado aqui
- `HeroPowerScore` usa `HP_máximo_efetivo` no lugar de `HP_base` (reflete poder real do herói)
- Heróis Inativos (`EstadoSustento.Inativo`) recuperam mais rápido (não consomem Comida = mais repouso)

### 12.4 Por que não usar cooldown simples?

Cooldown simples (N horas bloqueado) não afeta o poder do herói — um guerreiro com 4h de cooldown ainda entra com HP cheio. Vitalidade persistente:
- Torna cada falha mensurável no próximo combate
- Cria decisão tática: "entro ferido ou espero recuperação?"
- Liga naturalmente aos buildings de cidade (Banhos, Santuário) como utilidade real

---

## 13. Referência Rápida de Fórmulas

| Sistema | Fórmula |
|---|---|
| XP por nível | `B_r × l^1.25` |
| Redução de dano | `DEF / (DEF + 1000 + Level×50)` |
| Dano final | `ATK × SkillMult × (1 - DR) × TypeMult` |
| Power Score | `(BaseStats × LevelFactor × Growth × Race) + Gear + Skills + Relics` |
| LevelFactor | `1 + Level^1.25 / 100` |
| Dificuldade de conteúdo | `P0_ref × (1 + g)^stage × DiffClass` |
| Upkeep de cidade | `Base × SizeMult^1.15 × (1 + 0.12 × LevelSum)` |
| Chance de evento na Torre | `base × fatorAndar × (1 - overclearBonus)` |
| Burst cap | `MaxHit ≤ 0.65 × HP_max_alvo` |
| Anti-snowball | `CatchUp = 1 / (1 + WealthGap)` |
| Drop de fragmento | `Random(0,1) < 0.30` por andar limpo |
| Quantidade de fragmentos | `Ceiling(QuantidadeBase × (1.0 + ΣContratoBonus))` |
| Multiplicador de contrato | `1.0 + 0.30 (arquétipo) + 0.50 (nomeado)` — aditivo |
| HP efetivo (ferimento) | `HP_base × (1 - FerimentoFator)` — clamp 0.0–0.8 |
| Recuperação de ferimento | `FerimentoFator -= 0.10/hora` quando herói Inativo ou em cidade |
| Juros do Tesouro | `OuroArmazenado × 0.05` a cada 60 dias (requer 10+ guardas qualificados) |
| Produção do Moinho | `Random(1,10) × 100 Ouro/30 dias` (requer recurso bruto + 10 NQ + 3 Q) |
