# Spec: Atributos D&D com Perícias — LegendsAwaken

**Data:** 2026-05-08  
**Status:** Draft v3 (decisões arquiteturais incorporadas)  
**Filosofia:** Inspirado em D&D, não simulação de D&D. Sistema autobattle/party-manager que usa modificadores, d20 suavizado e perícias, mas não replica combat turn-by-turn D&D.

---

## 1. Visão Geral

Substituição do sistema de 5 atributos LA-próprios pelos 6 atributos de D&D 5e, adoção da escala 1–20 com modificadores, distribuição inicial por Profissão, sistema de Perícias (18 skills) com estrutura extensível para rank futuro, e testes de perícia na Torre com modelo agregado de grupo. Fórmulas de dano completas (dice system) e traits raciais são fora de escopo desta spec.

---

## 2. Hierarquia de Cálculo de Atributos

### 2.1 Camadas (ordem canônica)

```
AtributoEfetivo =
    AtributosBase          (determinado por Profissão — vide §3.2)
  + AtributosDistribuidos  (pontos ASI alocados pelo jogador)
  + BonusEquipamento       (somado de HeroiBonusAtributo onde Origem=Equipamento)
  + BonusTraits            (de HeroiBonusAtributo onde Origem=Racial/Talento/Outro)
  + BonusBuffs             (temporários — não persistidos no BD)
  ─────────────────────────
  = Total

Modificador = (Total − 10) / 2  (floor)
```

**Regra: nunca persistir modificadores.** Modificador é sempre calculado do Total no momento do uso. Salvar ambos cria inconsistência. Cache de performance pode ser adicionado em memória no futuro sem mudança de contrato.

### 2.2 Implicações para debuff/buff

A ordem acima garante que:
- Equipamentos e traits nunca interagem incorretamente (stacking);
- Debuffs (ex: penalidade Degradado −25%) são aplicados no final, sobre o Total, antes do modificador;
- Snapshots para combate capturam o Total — nunca um atributo intermediário.

### 2.3 Implementação em `Heroi.ObterAtributosTotais`

A assinatura atual já recebe `bonusExterno` para buffs temporários. A ordem de soma é mantida; a penalidade Degradado (já implementada) é o último passo antes de retornar.

---

## 3. Os 6 Atributos

| Enum           | Nome PT      | Abrev | Substitui    | Função sistêmica principal |
|---|---|---|---|---|
| `Forca`        | Força        | STR   | Forca        | Dano corpo-a-corpo, Atletismo |
| `Destreza`     | Destreza     | DEX   | Agilidade    | Iniciativa ATB, ataques à distância, perícias DEX |
| `Constituicao` | Constituição | CON   | Vitalidade   | HP máxima, resistência a status |
| `Inteligencia` | Inteligência | INT   | Inteligencia | Dano mágico, perícias arcanas/investigação |
| `Sabedoria`    | Sabedoria    | WIS   | Percepcao    | Percepção, cura, resistência mental, chance crítico |
| `Carisma`      | Carisma      | CHA   | *(novo)*     | Moral da party, eventos sociais, liderança |

### 3.1 Carisma — loop sistêmico
CHA não pode ser dump stat:
- **Bônus de liderança:** `MOD_CHA` do herói líder (maior CHA) aplica `+MOD_CHA × 1%` a todos os atributos efetivos da party.
- **Eventos sociais na Torre:** testes de Persuasao/Intimidacao/Enganacao usam CHA para evitar combate, rotas secretas, reduzir dificuldade.
- **Futuro:** custo reduzido em NPCs, summons escalando com CHA, buffs de grupo.

### 3.2 Destreza — controle de dominância
DEX não afeta CA. CA = `10 + bônus_armadura` apenas.  
DEX afeta: iniciativa ATB, ataques à distância, perícias Acrobacia/Furtividade/Prestidigitacao.

### 3.3 Bônus racial
+50 substituído por +2 no atributo-foco. Ponte para sistema futuro de traits/passivas.

| Raça       | Bônus          |
|---|---|
| Humano     | +1 em todos os 6 atributos |
| Bestial    | +2 Forca |
| Anão       | +2 Constituicao |
| Elfo       | +2 Sabedoria |
| Draconato  | +2 Inteligencia |
| Fada       | +2 Destreza |

### 3.4 Nota sobre Profissão vs Classe (decisão futura)
O enum `Profissao` hoje mistura classes de combate (Guerreiro, Mago) e profissões civis (Cozinheiro, Ferreiro). Isso gera tensão: "um Guerreiro não pode ser Ferreiro?". Separar `Classe` (combate) de `Profissao` (economia/crafting) evita conflito estrutural futuro. **Fora de escopo desta spec**, mas deve ser decidido antes de adicionar mais profissões civis ou classes de combate.

---

## 4. Escala, Modificadores e Progressão

### 4.1 Escala e modificadores
- **Escala:** 1–20 (cap soft para mortais).
- **Modificador:** `(atributo − 10) / 2` arredondado para baixo.

| Valor | Mod | Valor | Mod |
|---|---|---|---|
| 8  | −1 | 14 | +2 |
| 10 | +0 | 16 | +3 |
| 12 | +1 | 18 | +4 |
|    |    | 20 | +5 |

### 4.2 Distribuição inicial por Profissão
Identidade imediata no early game. Total = 60 pontos por template.

| Profissão     | STR | DEX | CON | INT | WIS | CHA | Total |
|---|---|---|---|---|---|---|---|
| Guerreiro     | 14  | 10  | 12  |  8  |  9  |  7  | 60 |
| Arqueiro      |  9  | 14  | 10  |  8  | 12  |  7  | 60 |
| Mago          |  7  |  9  | 10  | 14  | 12  |  8  | 60 |
| Ladino        |  8  | 14  |  7  | 12  |  9  | 10  | 60 |
| Paladino      | 14  |  8  | 10  |  7  |  9  | 12  | 60 |
| Clerigo       |  7  |  8  | 10  |  9  | 14  | 12  | 60 |
| Agricultor    | 10  |  9  | 12  |  8  | 13  |  8  | 60 |
| Pescador      |  9  | 12  | 10  |  8  | 12  |  9  | 60 |
| Caçador       |  9  | 14  | 10  |  8  | 12  |  7  | 60 |
| Lenhador      | 14  |  9  | 12  |  7  | 10  |  8  | 60 |
| Mineiro       | 13  |  8  | 14  |  8  |  9  |  8  | 60 |
| Cozinheiro    |  8  |  9  | 10  | 11  | 13  |  9  | 60 |
| Ferreiro      | 14  |  8  | 12  | 10  |  9  |  7  | 60 |
| Alfaiate      |  7  | 13  |  8  | 11  |  9  | 12  | 60 |
| Joalheiro     |  7  | 11  |  8  | 13  |  9  | 12  | 60 |
| Alquimista    |  7  |  9  | 10  | 14  | 12  |  8  | 60 |
| Construtor    | 13  |  8  | 12  | 11  | 10  |  6  | 60 |
| Pesquisador   |  6  |  9  |  8  | 14  | 12  |  9  | 60 |

### 4.3 Progressão — custo crescente

**Cadência:** 1 ponto livre a cada 4 níveis (ASI D&D).  
- 5★ nível 100 → 25 pontos totais.

**Custo por incremento:**

| Valor atual do atributo | Custo para subir +1 |
|---|---|
| 8–14 | 1 ponto |
| 15–16 | 2 pontos |
| 17–18 | 3 pontos |
| 19–20 | 4 pontos |

Com 25 pontos: máximo realista é primário ≈ 18 (14 pts) + secundário ≈ 14 (4 pts) + sobra 7 pts. Endgame viável sem colapso de escala.

**Implementação:** `PontosAtributosDisponiveis` acumula pontos livres. `GanhoPorNivel` em `RaridadeConfig` usa `nivel % 4 == 0 ? 1 : 0` — ou valor fracionário com acumulação.

### 4.4 RaridadeConfig

| Raridade | Cap nível | BaseStatsTotal | GanhoPorNivel | GanhoSuperacao | BaseXp |
|---|---|---|---|---|---|
| 1★ | 20  | 60 (por profissão) | 1 a cada 4 níveis | 0   | 80  |
| 2★ | 40  | 60 (por profissão) | 1 a cada 4 níveis | 0   | 100 |
| 3★ | 60  | 60 (por profissão) | 1 a cada 4 níveis | 0   | 120 |
| 4★ | 80  | 60 (por profissão) | 1 a cada 4 níveis | 0   | 150 |
| 5★ | 100 | 60 (por profissão) | 1 a cada 4 níveis | 1/nível acima lv80 | 200 |

### 4.5 HP — fórmula sem multiplicação acumulada

Para evitar HP inflation (CON cresce + nível cresce = multiplicação acumula):

```
HP_maxima = base_HP_classe + (nivel × ganho_nivel_fixo) + MOD_CON
```

Onde:
- `base_HP_classe`: valor fixo por profissão (combate: 10–12, suporte: 7–9)
- `ganho_nivel_fixo`: ~1–2 HP por nível (constante, não multiplicado)
- `MOD_CON`: modificador estático, adicionado uma vez — **não** multiplicado por nível

Isso mantém HP escalando linearmente, não exponencialmente.

---

## 5. Fórmulas de Combate Remapeadas

| Fórmula         | Atual                              | Transição inicial                            |
|---|---|---|
| Iniciativa ATB  | `Agilidade + rand(0, Ag×0.1)`     | `Destreza + rand(0, max(MOD_DEX,1)×2)`       |
| Dano físico     | `Forca × SkillMult × DEF formula` | `(MOD_STR + SkillMult_base) × DEF formula`   |
| Dano mágico     | `Inteligencia × SkillMult × ...`  | `(MOD_INT + SkillMult_base) × DEF formula`   |
| HP máxima       | `Vitalidade × fator`              | `base_HP + nivel×ganho + MOD_CON` (§4.5)     |
| Crítico chance  | `5% + Percepcao×0.1%`             | `5% + MOD_WIS × 1%`                          |
| CA (defesa)     | `DEF/(DEF + K)`                   | `10 + bônus_armadura` (DEX não afeta CA)     |
| Liderança (novo)| *n/a*                             | `×(1 + MOD_CHA_lider × 0.01)` sobre todos   |

Redesign profundo de dano (dice system) fica para spec de combate D&D.

---

## 6. Sistema de Perícias

### 6.1 Enum `Pericia`

| Enum                | PT                  | Atributo       |
|---|---|---|
| `Atletismo`         | Atletismo           | Forca          |
| `Acrobacia`         | Acrobacia           | Destreza       |
| `Prestidigitacao`   | Prestidigitação     | Destreza       |
| `Furtividade`       | Furtividade         | Destreza       |
| `Arcanismo`         | Arcanismo           | Inteligencia   |
| `Historia`          | História            | Inteligencia   |
| `Investigacao`      | Investigação        | Inteligencia   |
| `Natureza`          | Natureza            | Inteligencia   |
| `Religiao`          | Religião            | Inteligencia   |
| `AdestrarAnimais`   | Adestrar Animais    | Sabedoria      |
| `Intuicao`          | Intuição            | Sabedoria      |
| `Medicina`          | Medicina            | Sabedoria      |
| `Percepcao`         | Percepção           | Sabedoria      |
| `Sobrevivencia`     | Sobrevivência       | Sabedoria      |
| `Enganacao`         | Enganação           | Carisma        |
| `Intimidacao`       | Intimidação         | Carisma        |
| `Atuacao`           | Atuação             | Carisma        |
| `Persuasao`         | Persuasão           | Carisma        |

### 6.2 Bônus de Proficiência

| Nível do herói | Bônus |
|---|---|
| 1–4            | +2    |
| 5–8            | +3    |
| 9–12           | +4    |
| 13–16          | +5    |
| 17+            | +6    |

### 6.3 Proficiências iniciais por Profissão

| Profissão    | Perícias iniciais |
|---|---|
| Guerreiro    | Atletismo, Intimidacao |
| Arqueiro     | Furtividade, Percepcao |
| Mago         | Arcanismo, Historia |
| Ladino       | Prestidigitacao, Furtividade, Enganacao |
| Paladino     | Atletismo, Religiao, Persuasao |
| Clerigo      | Medicina, Religiao, Intuicao |
| Agricultor   | Natureza, Sobrevivencia |
| Pescador     | Natureza, Atletismo |
| Caçador      | Sobrevivencia, Furtividade, Percepcao |
| Lenhador     | Natureza, Atletismo |
| Mineiro      | Atletismo, Historia |
| Cozinheiro   | Medicina, Natureza |
| Ferreiro     | Atletismo, Historia |
| Alfaiate     | Prestidigitacao |
| Joalheiro    | Historia, Investigacao |
| Alquimista   | Arcanismo, Natureza, Medicina |
| Construtor   | Atletismo, Historia |
| Pesquisador  | Arcanismo, Historia, Investigacao, Religiao |

### 6.4 Entidade `HeroiPericia` — extensível para rank futuro

```csharp
public class HeroiPericia
{
    public Guid Id { get; set; }
    public Guid HeroiId { get; set; }
    public Pericia Pericia { get; set; }
    public bool TemProficiencia { get; set; }
    // Reservado para progressão horizontal futura (XP de perícia, rank, mastery)
    public int Rank { get; set; } = 0;
    public Heroi Heroi { get; set; } = null!;
}
```

`Rank` começa em 0, não usado agora, mas evita migration futura. Quando ativado: Rank 1–5, cada rank dá +1 ao roll e pode desbloquear eventos especiais na Torre.

EF: tabela `HeroisPericias`, FK `HeroiId → Herois(Id)`, índice único em `(HeroiId, Pericia)`.

### 6.5 `SkillRollContext` — extensível para vantagem/desvantagem

```csharp
public record SkillRollContext(
    AdvantageType Advantage = AdvantageType.Normal,
    int FlatBonus = 0,
    int? AutoSuccessThreshold = null,  // roll acima disso é sucesso automático
    bool CritEnabled = false           // crítico em 20 natural
);

public enum AdvantageType { Disadvantage = -1, Normal = 0, Advantage = 1 }
```

Por ora, todos os testes usam `Normal` + `CritEnabled = false`. Estrutura preparada para expertise, itens, condições e eventos especiais.

---

## 7. Testes de Perícia na Torre

### 7.1 DC Tiers globais

| Dificuldade  | DC |
|---|---|
| Muito Fácil  |  5 |
| Fácil        | 10 |
| Média        | 15 |
| Difícil      | 20 |
| Extrema      | 25 |
| Lendária     | 30 |

Usar esses tiers como referência para eventos procedurais da Torre. DC andar N ≈ `10 + floor(N/5)` como baseline.

### 7.2 Mecânica de roll — 2d10 em vez de d20

d20 puro em autobattle cria UX ruim: especialista falha série de checks fáceis, parece injusto. Alternativa com distribuição mais normal:

```
Roll = 2d10 + MOD + BonusProf
```

Média de 2d10 = 11 (vs. 10 do d20), desvio padrão = 4.08 (vs. 5.77 do d20). Resultado: especialistas raramente falham checks fáceis; checks difíceis ainda são desafiadores.

**Impacto:** `SkillRollContext` encapsula o método de roll. Troca d20 → 2d10 é uma linha no `TorreExploracaoService` sem alterar contratos.

### 7.3 Modelo agregado de grupo

"Todos os heróis rolam" gera logs enormes e ruído em autobattle. Modelo mais previsível:

```
ScoreGrupo = BonusMelhorHeroi + (SomaBonusRestantes / (N-1)) × 0.5
Sucesso se ScoreGrupo + Roll >= DC × MinimoSucessos_normalizado
```

Simplificado: para a implementação inicial, usar a média ponderada `(melhor × 0.6 + segundo_melhor × 0.3 + terceiro × 0.1)` e rolar uma vez com o score agregado.

Isso mantém legibilidade no log e resultado previsível.

### 7.4 Evento individual

Herói com maior `MOD + BonusProf` na perícia exigida rola `2d10 + MOD + BonusProf + SkillRollContext.FlatBonus`.

### 7.5 Estrutura `TestePericiaEvento`

```csharp
public record TestePericiaEvento(
    string Descricao,
    Pericia PericiaExigida,
    int DC,
    bool EhGrupo,
    string RecompensaSucesso,
    string PenalidadeFalha,
    SkillRollContext? RollContext = null  // null = default Normal/sem crítico
);
```

### 7.6 Integração com `TorreExploracaoService`
- Chance de evento por andar: ~20% (constante configurável).
- Evento registrado como sub-estado na exploração ativa.
- Sucesso: bônus de progresso percentual ou recurso extra na coleta.
- Falha: penalidade de progresso ou redução de HP da party.

---

## 8. Migração de Dados

| Entidade / Tabela | Mudança |
|---|---|
| `Herois` — colunas EF | Renomear: `Agilidade→Destreza`, `Vitalidade→Constituicao`, `Percepcao→Sabedoria`; adicionar `Carisma`. |
| `AtributosBase` stats | Reset para distribuição por Profissão da tabela §4.2. |
| `HeroiBonusAtributo` | Enum `Atributo` muda; registros migram para novos nomes. |
| `InimigoAndar.Atributos` | JSON; reconstruído via seed/migration com ~10 por atributo. |
| Nova tabela `HeroisPericias` | EF migration; populada em `CriarHeroiAsync` e para heróis existentes. |

**Heróis existentes:** atributos resetados para template da Profissão. `AtributosDistribuidos` zerado. `PontosAtributosDisponiveis` = `nivel / 4`. Bônus raciais ajustados de +50 para +2.

---

## 9. Arquivos Impactados

| Arquivo | Mudança |
|---|---|
| `Domain/Enum/Enums.cs` | `Atributo`: 5→6 valores, renomear 3; `Pericia`: novo; `AdvantageType`: novo |
| `Domain/Entities/AtributosBase.cs` | Nova property `Carisma`; renomear 3 props; Get/Set |
| `Domain/Entities/Heroi.cs` | `ObterAtributosTotais`: sem mudança estrutural |
| `Application/Services/HeroiLevelUpService.cs` | `Configs` revisado; `BonusRacial`: +2; tabela de distribuição por Profissão |
| `Application/Services/CombatService.cs` | Fórmulas remapeadas + liderança CHA + HP formula §4.5 |
| `Application/Services/HeroiService.cs` | `CriarHeroiAsync`: distribuir stats por Profissão; criar HeroiPericias iniciais |
| `Infrastructure/Migrations/` | Renomear colunas + Carisma + HeroisPericias |
| `Bot/Panels/HeroisPanel.cs` | Exibir 6 atributos com modificadores (`STR +2`, etc.) |

---

## 10. Fora de Escopo nesta Spec

- Redesign de fórmula de dano para dice system D&D (spec separada).
- Traits/passivas raciais (darkvision, resistência, afinidade, mobilidade) — **prioridade alta para fase seguinte**.
- Separação `Classe` vs `Profissao` — **decisão necessária antes de escalar profissões civis**.
- XP/mastery de perícias — estrutura preparada (`Rank` em `HeroiPericia`), ativação futura.
- Expertise (dobro do proficiency bonus).
- Spell slots ou magia D&D completa.
- Feats (talentos) D&D.
- Vantagem/Desvantagem em combate (apenas em skill checks por ora via `SkillRollContext`).
