# Legends Awaken — Game Design Document

> Inspirado em *Pick Me Up Infinite Gacha*.  
> Bot RPG para Discord em C#.

---

## Sumário

- [1. Visão Geral](#1-visão-geral)
- [2. Loop de Jogo](#2-loop-de-jogo)
- [3. Sistema de Fragmentos (Aquisição de Heróis)](#3-sistema-de-fragmentos-aquisição-de-heróis)
- [4. Sistema de Heróis](#4-sistema-de-heróis)
- [5. Sistema de Torre (já implementado)](#5-sistema-de-torre-já-implementado)
- [6. Sistema de Cidade](#6-sistema-de-cidade)
- [7. Sistema de Missões (Guilda)](#7-sistema-de-missões-guilda)
- [8. Sistema de Crafting (Forja / Ateliê / Laboratório)](#8-sistema-de-crafting-forja--ateliê--laboratório)
- [9. Arena de Combate](#9-arena-de-combate)
- [10. Sistema de Relíquias](#10-sistema-de-relíquias)
- [11. Sistema de Conversão de Heróis (Fase 3B)](#11-sistema-de-conversão-de-heróis--fase-3b)
- [12. Sistema de Sustento (Fase 3B)](#12-sistema-de-sustento--fase-3b)
- [13. Sistema de Invasão, Traição e Expedições (Pós-lançamento)](#13-sistema-de-invasão-traição-e-expedições-pós-lançamento)
- [14. Nível do Mestre e Meta Progressão (Fase 3B)](#14-nível-do-mestre-e-meta-progressão--fase-3b)
- [15. Princípios de Design e UX](#15-princípios-de-design-e-ux)
- [16. Comandos e Interações](#16-comandos-e-interações)
- [17. Progressão e Endgame](#17-progressão-e-endgame)
- [18. Status de Implementação](#18-status-de-implementação)
- [19. Sistema de Mercado P2P (Fase 3B-Mercado)](#19-sistema-de-mercado-p2p--fase-3b-mercado)
- [20. Sistema de Mercenários + Treinamento como Serviço](#20-sistema-de-mercenários--treinamento-como-serviço)
- [21. Prioridades e Escopo por Fase](#21-prioridades-e-escopo-por-fase)

---

## 1. Visão Geral

O jogador é um **Mestre** que coleciona heróis de forma determinística — por fragmentos, conquistas e contratos — enquanto escala uma Torre infinita e gerencia uma **Cidade** que cresce conforme sua coleção evolui. O loop central é: **progredir → coletar fragmentos → recrutar → alocar**.

---

## 2. Loop de Jogo

```
[PROGREDIR] → Sobe andares da Torre, enfrenta biomas, conquista marcos
    ↓
[FRAGMENTOS] → Drops de fragmentos de heróis ao limpar andares (30% chance)
    ↓
[ALOCAR]     → Heróis são designados à Torre ou a um prédio da Cidade
    ↓
[PRODUZIR]   → Cidade gera recursos/XP passivamente enquanto o jogador está offline
    ↓
[RECRUTAR]   → Acumula fragmentos, bate marcos ou cumpre condições para desbloquear heróis
    ↓
[MELHORAR]   → Recursos da cidade financiam upgrades de prédios e equipamentos
    ↓
 (volta ao início)
```

---

## 3. Sistema de Fragmentos (Aquisição de Heróis)

> **Status:** ✅ Implementado

> O sistema de gacha foi substituído por aquisição determinística. Nenhum RNG de raridade — o jogador sabe exatamente o que precisa para obter cada herói.

### 3.1. Modelo de Raridade

| Raridade | Tipo | Arte | Identidade |
|---|---|---|---|
| 5★ | Personagem fixo nomeado ou herói ascendido | Arte IA única / customizada pelo jogador | Nome, lore, habilidade exclusiva |
| 4★ | Personagem fixo nomeado ou herói ascendido | Arte IA única / customizada pelo jogador | Nome, lore, habilidade exclusiva |
| 3★ | Procedural por arquétipo | Arte por classe/raça (compartilhada) | Nome gerado, stats variados |
| 2★ | Procedural | Ícone por classe | Genérico |
| 1★ | Procedural | Ícone por classe | Genérico |

### 3.2. Três Caminhos de Desbloqueio

Cada herói tem exatamente **um** caminho de desbloqueio, definido em `HeroiUnlockConfig`:

| Tipo | Como funciona | Exemplo |
|---|---|---|
| **Fragmentos** | Acumular N fragmentos do herói via drops na Torre (bioma específico) | 30 fragmentos de Grom → Grom desbloqueado |
| **Marco da Torre** | Atingir um andar específico desbloqueado automaticamente | Andar 10 → Seraph desbloqueado |
| **Condição Única** | Ação especial fora da Torre (missão, conquista, etc.) | A definir por herói |

### 3.3. Fragmentos e Biomas

A Torre é dividida em **biomas** (faixas de andares). Cada bioma tem um pool de heróis associado. Ao limpar um andar (30% de chance), o jogador recebe fragmento(s) de um herói do bioma atual.

- Peso de drop por herói configurável por bioma (`BiomHeroPool.Peso`)
- Quantidade de fragmentos: `base × multiplicador de contrato` (arredondado para cima)
- `/bioma` — exibe o bioma atual, quais heróis dropam e suas chances relativas

### 3.4. Contratos (substituem banners)

Contratos são bônus de drop ativados pelo jogador, sem prazo de validade.

| Tipo | Bônus | Notas |
|---|---|---|
| **Arquétipo** | +30% fragmentos de heróis da profissão escolhida | 1 ativo por vez; substituível |
| **Nomeado** | +50% fragmentos de herói específico | Requer ≥1 fragmento já coletado; 1 ativo por vez |

- `/contrato` — exibe contrato ativo e permite trocar ou remover

### 3.5. Arte dos Personagens Fixos
- Gerada por IA (Midjourney / DALL-E) com prompt padronizado por classe e raça.
- URL da imagem salva no banco e exibida nos painéis de coleção.

### 3.6. Pool Inicial de Personagens Fixos

#### 5★ — Lendários

| Nome | Profissão | Raça | Elemento | Referência | Lore |
|---|---|---|---|---|---|
| **Aldric, o Sem-Corrente** | Guerreiro | Humano | Metal | Guts (Berserk) | Mercenário solitário com espada enorme que nunca serviu a nenhum mestre por vontade própria... até encontrar o Mestre. |
| **Yuzara, a Tecelã do Destino** | Mago | Elfo | Luz | Mavis Vermillion (Fairy Tail) | Capaz de antever o futuro, raramente escolhe interferir. Sempre sorri como se soubesse o que está por vir. |
| **Thorvald, o Arquiteto das Eras** | Ferreiro / Construtor | Anão | Terra | Artesãos lendários do fantasy | Ergueu três cidades antes de ser invocado. Diz que a quarta será a mais grandiosa de todas. |

#### 4★ — Épicos

| Nome | Profissão | Raça | Elemento | Referência | Lore |
|---|---|---|---|---|---|
| **Kaen** | Arqueiro | Humano | Fogo | Sinbad (Magi) | Aventureiro carismático que entrou em cada batalha sorrindo. Nunca perdeu — ainda. |
| **Nyra** | Ladino | Bestial (felina) | Ar | Yoruichi (Bleach) | Aparece quando quer, desaparece quando bem entende. Diz que trabalha melhor sozinha, mas raramente está sozinha de verdade. |
| **Seraph** | Paladino | Humano | Luz | Izuku / Simon (MHA / Gurren Lagann) | Jovem idealista convicto de que proteger todos é possível. Ainda não foi provado errado. |
| **Mira** | Alquimista | Humano | Fogo | Edward Elric (FMA) | Prodígio da alquimia que transformou o laboratório da cidade em algo que nenhum mestre esperava. Teimosa. Brilhante. |
| **Grom** | Mineiro | Anão | Terra | Anões clássicos do fantasy | Nunca abandona uma veia de minério. Nunca. Dizem que ele encontra metal onde outros só veem pedra comum. |
| **Hana** | Cozinheiro / Agricultor | Humano | Natureza | Soma (Shokugeki no Soma) | A culinária dela tem efeitos que nenhuma poção replica. O time rende 20% a mais depois do almoço dela. |

### 3.7. Painéis Discord Implementados

| Comando | Painel | O que mostra |
|---|---|---|
| `/colecao` | ColecaoPanel | Lista de todos os heróis com estado (✅ desbloqueado / 🔒 fragmentos / 🔒 marco / ❓ condição); botão de recrutar quando pronto |
| `/bioma` | BiomaPanel | Bioma atual, heróis que dropam nele, pesos; atalhos para coleção e contratos |
| `/contrato` | ContratoPanel | Contratos ativos (arquétipo + nomeado); select menu para trocar arquétipo |

---

## 4. Sistema de Heróis

> **Status:** ✅ Implementado (parcial)

### 4.1. Atributos Base (já implementado)
- **Força** — dano físico, carga
- **Agilidade** — velocidade, evasão, crítico
- **Vitalidade** — HP, recuperação
- **Inteligência** — dano mágico, pesquisa
- **Percepção** — detecção, precisão

### 4.2. Raças (já implementado)
`Humano | Bestial | Anão | Elfo | Draconato | Fada`

Cada raça concede bônus passivos em atributos e afinidades com certas profissões.

#### Bônus Raciais

Cada raça não-humana é especializada em **um único atributo** — seu ponto forte natural. Quanto mais rara a raça no gacha, maior o bônus. Os bônus são permanentes, aplicados na criação como `OrigemBonusAtributo.Racial`.

| Raça | Atributo foco | Bônus | Passiva Única |
|---|---|---|---|
| **Humano** | — | +3 a um atributo escolhido na criação | Versátil: +10% XP de todas as fontes |
| **Bestial** | Força | +50 | Instinto Predatório: +10% dano em ataques físicos |
| **Anão** | Vitalidade | +50 | Pele de Pedra: -10% dano recebido (sempre ativo) |
| **Elfo** | Percepção | +50 | Sentidos Élficos: imune a Cego; nunca erra ataques físicos |
| **Draconato** | Inteligência | +50 | Sopro Dracônico: habilidade de área baseada no elemento do herói |
| **Fada** | Agilidade | +50 | Leveza: imune a Preso; sempre age primeiro no turno |

Todas as raças não-humanas têm o mesmo bônus (+50 no atributo foco) e a mesma chance dentro do pool não-humano. Chance de sair uma raça não-humana: 1★/2★ = sempre humano; 3★ = 10%; 4★ = 25%; 5★ = raça fixa do personagem nomeado.

### 4.3. Profissões
Cada herói nasce com uma profissão primária que determina onde ele trabalha melhor na cidade e que tipo de habilidades pode masterizar.

**Combate:**
`Guerreiro | Arqueiro | Mago | Ladino | Paladino | Clérigo`

**Coleta:**
`Agricultor | Pescador | Caçador | Lenhador | Mineiro`

**Produção:**
`Ferreiro | Alfaiate | Joalheiro | Alquimista | Construtor | Cozinheiro`

**Suporte:**
`Pesquisador`

### 4.4. Habilidades (já implementado)
- Cada herói tem habilidades vinculadas à sua profissão.
- Habilidades evoluem de nível 1 a 10 via XP.
- Concedem bônus de atributos por tipo: `Combate | Craft | Coleta`.
- Habilidades de Craft/Coleta aumentam a produção da cidade quando o herói está alocado.

### 4.5. Progressão de Nível

#### Cap de nível por raridade
| Raridade | Cap | Notas |
|---|---|---|
| 1★ | 20 | Ponto de partida — ascensão é o caminho |
| 2★ | 40 | — |
| 3★ | 60 | — |
| 4★ | 80 | — |
| 5★ | 100 | Teto absoluto de progressão |

Ao atingir o cap, o herói **para de ganhar XP** e só pode ascender quando estiver exatamente no cap — a ascensão não é disponível antes disso.

#### Stats base por raridade (nível 1)

Distribuídos igualmente entre os 5 atributos:

| Raridade | Total | Por atributo |
|---|---|---|
| 1★ | 50 | 10 |
| 2★ | 70 | 14 |
| 3★ | 95 | 19 |
| 4★ | 130 | 26 |
| 5★ | 175 | 35 |

A diferença de base representa o talento natural. Raça e profissão constroem a especialização em cima disso.

#### Ganho de pontos por level-up

A cada level-up, o herói recebe pontos para distribuir livremente entre os 5 atributos:

| Raridade | Níveis 1–80 | Níveis 81–100 |
|---|---|---|
| 1★ | +2 | — (cap 20) |
| 2★ | +3 | — (cap 40) |
| 3★ | +4 | — (cap 60) |
| 4★ | +6 | — (cap 80) |
| 5★ | +8 | +12 |

O salto para +12 nos níveis 81–100 é exclusivo do 5★ — heróis que chegam lá estão quebrando seus próprios limites.

#### Totais acumulados no cap

| Raridade | Nível máximo | Total de pontos |
|---|---|---|
| 1★ | 20 | 88 |
| 2★ | 40 | 187 |
| 3★ | 60 | 331 |
| 4★ | 80 | 604 |
| 5★ | 100 | 1043 |

#### Grant de Ascensão (catch-up automático)

Ao ascender, o herói recebe um grant de pontos que o iguala a um nativo da nova raridade no mesmo nível. A partir daí, segue o fluxo normal da nova raridade.

```
grant = total_pontos(nova_raridade, nível_atual)
      − total_pontos(raridade_atual, nível_atual)
```

Valores nas transições de cap (herói que ascende exatamente no cap):

| Transição | Grant concedido |
|---|---|
| 1★ → 2★ no lv20 | +39 |
| 2★ → 3★ no lv40 | +64 |
| 3★ → 4★ no lv60 | +153 |
| 4★ → 5★ no lv80 | +203 |

> Ascender antes do cap gera um grant menor (o herói tem menos acumulado). Ascender no cap maximiza o grant. Qualquer que seja o nível, ao chegar no lv100 como 5★, todos os heróis têm exatamente 1043 pontos — independente de qual raridade nasceram.

#### Fontes de XP (modelo balanceado)
| Fonte | Peso | Condição |
|---|---|---|
| Subir andar da Torre | Principal | Todos os heróis da party ganham XP |
| Kill de boss (andares 5 / 10 / 25) | Bônus grande | Apenas heróis que participaram |
| Missões da Guilda | Secundário | Herói que foi em missão |
| Arena (treino) | Acelerado | Consome recursos; qualquer skill beneficiada |
| Trabalho na Cidade | Pequeno passivo | Proporcional ao tempo alocado |

> Escolha estratégica real: herói na Torre ganha mais XP, herói na Cidade produz recursos. Não dá para fazer os dois ao mesmo tempo.

#### Curva de XP

**Fórmula definitiva:** `XP_next(l, r) = B_r × l^1.25`

| Raridade | B_r | Cap | XP total estimado |
|---|---|---|---|
| 1★ | 80 | 20 | ~34.000 |
| 2★ | 100 | 40 | ~97.000 |
| 3★ | 120 | 60 | ~210.000 |
| 4★ | 150 | 80 | ~400.000 |
| 5★ | 200 | 100 | ~810.000 |

> **Implementação Fase 3A:** usar `XP_next = B_r × nível` (linear) para desbloqueio rápido; migrar para `l^1.25` com dados de beta. Fórmulas completas e modelo de pacing em `DESIGN_SISTEMAS.md §1`.

---

### 4.6. Ascensão por Fragmentos

Heróis podem ascender de raridade consumindo **Fragmentos de Arquétipo**. Qualquer herói — incluindo genéricos — pode chegar até 5★ com dedicação suficiente.

#### Arquétipos e fragmentos
Fragmentos são por profissão, não por herói individual:

| Arquétipo | Profissões | Como obter fragmentos |
|---|---|---|
| Combate | Guerreiro, Arqueiro, Mago, Ladino, Paladino, Clérigo | Subir andares da Torre usando heróis desse arquétipo |
| Coleta | Agricultor, Pescador, Caçador, Lenhador, Mineiro, Cozinheiro | Heróis do arquétipo coletando recursos na Cidade |
| Produção | Ferreiro, Alfaiate, Joalheiro, Alquimista, Construtor, Pesquisador | Heróis do arquétipo produzindo itens na Cidade |

> A lógica é: você fortalece quem usa. Um Lenhador que passa semanas derrubando árvores acumula a experiência necessária para transcender.

#### Custo de ascensão
| Transição | Fragmentos do arquétipo | Materiais adicionais |
|---|---|---|
| 1★ → 2★ | 15 | — |
| 2★ → 3★ | 30 | Ouro x500 |
| 3★ → 4★ | 60 | Ouro x2000 + material raro¹ |
| 4★ → 5★ | 120 | Ouro x10000 + material lendário² |

¹ Material raro: provavelmente drop de boss avançado (andar 50+).  
² Material lendário: a definir — drop exclusivo de boss de andar 75+ ou missão Oricalco.

#### Identidade ao ascender para 4★
Na ascensão 3★ → 4★ e em todas as ascensões subsequentes, o herói "emerge" — o jogador pode:
- **Definir um apelido** para o herói (opcional — veja seção 4.8 Apelidos)
- **Enviar uma arte customizada** via URL no comando de ascensão

Se nenhuma arte for enviada, o herói usa arte por arquétipo (compartilhada com outros do mesmo tipo). Heróis ascendidos não entram no pool de personagens fixos — são únicos *para aquele jogador*, não parte do lore global.

#### Ascensão 4★ → 5★
O custo é mais alto e exige material lendário (a definir). Chegar a 5★ via ascensão representa a jornada completa de um herói — narrativamente equivalente aos personagens fixos do gacha, mas forjado pelo próprio jogador.

#### Fragmentos de Personagem Fixo (separado)
Personagens nomeados do pool (4★/5★) têm seus próprios fragmentos específicos (`Fragmento de Nyra`, `Fragmento de Kaen`, etc.), obtidos em boss fights avançados e missões raras. Juntar o número necessário recruta o personagem fixo diretamente, sem gacha.

---

### 4.7. Progressão de Habilidades

Skills evoluem de nível 1 a 10 conforme o herói **usa** aquele tipo de habilidade. A ação atual do herói determina qual categoria de skill recebe XP:

| Onde está o herói | Skills que evoluem |
|---|---|
| Torre (combate) | Skills de Combate |
| Forja / Ateliê / Laboratório | Skills de Craft |
| Mina / Serraria / Fazenda / Herborário | Skills de Coleta |
| Arena | Qualquer skill (XP acelerado) |
| Missão da Guilda | Skills conforme tipo da missão |

> Um Ferreiro que nunca lutou terá skills de Craft altas e skills de Combate zeradas. Um Guerreiro alocado na Forja por semanas pode surpreender.

Bônus de habilidade:
- Skills de Combate → atributos de batalha (Força, Agilidade, etc.)
- Skills de Craft → qualidade e velocidade de produção na Cidade
- Skills de Coleta → quantidade e variedade de recursos coletados

---

### 4.8. Sistema de Apelidos e Arte Customizada

Qualquer herói pode receber um **apelido** e uma **arte customizada** a qualquer momento.

**Apelido:**
- Exibido no lugar do nome gerado em todos os comandos
- `/ver_heroi` mostra: `**Apelido** *(Thorin Stoneback)*`
- Renomear em massa disponível com filtros (por raridade, raça, profissão)
- Se o apelido for removido, o nome procedural original é restaurado

**Arte customizada:**
- Definida via URL em `/heroi arte <nome> <url>`
- Obrigatória na ascensão 3★→4★ se quiser arte única (caso contrário usa arte de arquétipo)

> O jogador é o artista da própria coleção. Se não quiser personalizar, o sistema funciona normalmente com arte procedural.

---

## 5. Sistema de Torre (já implementado)

> **Status:** 🚧 MVP implementado — IA tática e posicionamento pendentes  
> **Design de Arcos Narrativos:** ver [`DESIGN_TORRE_ARCOS.md`](DESIGN_TORRE_ARCOS.md) — framework de flags, colecionáveis e catálogo de arcos (Andares 1–15)

- Torre infinita com andares em sequência por usuário.
- Tipos de andar: `Subjugação | Fuga | Escolta | Defesa | Armadilha | EventoEspecial`
- Bosses em andares múltiplos de 5 (fácil), 10 (médio), 25 (difícil).
- Party de até 5 heróis.
- Combate automático por turnos.

### 5.0. Fórmulas de Combate Core

```
RawDamage   = ATK × SkillMultiplier
Reduction   = DEF / (DEF + 1000 + Level × 50)
FinalDamage = RawDamage × (1 - Reduction) × TypeMultiplier
              [× 1.5 se Crit, máx 2.0×]

Burst cap: nenhum hit remove mais de 65% do HP máximo do alvo

Iniciativa: InitScore = Speed + Random(0, Speed × 0.1)
Delay pós-ação: 100 / Speed
```

**TypeMultiplier:** vantagem=1.25 | neutro=1.0 | desvantagem=0.75

> Fórmulas completas, tabelas de redução e Power Score em `DESIGN_SISTEMAS.md §2 e §3`.

**Recompensas por andar:**
- Ouro e XP para os heróis participantes.
- Materiais de crafting em andares de boss.
- Fragmentos para recrutar personagens fixos (futuro).

### 5.1. Roster de Inimigos por Tier

| Faixa de Andares | Inimigos comuns | Boss |
|---|---|---|
| 1–10 | Bandidos, Veteranos, Goblins, Esqueletos | Boss 5: Capitão Bandido / Boss 10: Golem de Argila |
| 11–25 | Espectros, Minotauro Esqueleto, Hellhound, Lobo do Inverno | Boss 25: Vampiro ou Gladiador Lendário |
| 26–50 | Shadow Demon, Dretch, Barlgura, Aberrações menores | Boss 50: Mummy Lord ou Golem de Ferro |
| 51–75 | Glabrezu, Mind Flayer, Dragon-kin | Boss 75: Dragão Jovem |
| 76–100 | Balor, Aberrações maiores, Elite de Dragon-kin | Boss 100: Dragão Adulto (boss lendário) |
| 100+ | Inimigos gerados proceduralmente com stats escalados | Bosses únicos a cada 25 andares |

### 5.2. Tipos de Dano

O combate usa tipos de dano para interações com resistências e fraquezas dos inimigos e habilidades dos heróis:

`Concussão | Cortante | Perfurante | Fogo | Elétrico | Frio | Ácido | Necrótico | Radiante | Trovejante | Psíquico | Energia`

- Cada herói tem um tipo de dano primário baseado na profissão + elemento.
- Inimigos podem ter **resistência** (dano pela metade) ou **imunidade** a certos tipos.
- Acertar uma **fraqueza** aplica bônus de dano.

### 5.3. Condições de Status

Habilidades e bosses podem aplicar condições temporárias:

| Condição | Efeito |
|---|---|
| **Envenenado** | Perde HP no início de cada turno |
| **Amedrontado** | -ATK enquanto o causador estiver vivo |
| **Enfeitiçado** | Não ataca o causador pelo turno |
| **Paralisado** | Perde o turno completamente |
| **Cego** | Ataques têm chance de errar |
| **Preso** | Não pode mudar de alvo; -Agilidade |
| **Exausto** | Penalidade crescente em ATK e DEF (até 6 stacks) |
| **Petrificado** | Incapacitado; resistência física; dura X turnos |

Condições têm duração em turnos. Heróis com skills ou equipamentos específicos podem ser imunes ou ter resistência a certas condições.

### 5.4. Bônus de Composição de Party *(Fase 3B)*

A party não é apenas soma de atributos — a composição gera bônus próprios:

| Composição | Bônus |
|---|---|
| 3+ heróis da mesma raça | +10% XP para toda a party |
| Full arqueiros (5/5) | +15% chance de crítico |
| Party balanceada (1 de cada arquétipo: Combate, Coleta, Produção, Suporte + 1 livre) | +10% em todos os recursos obtidos |
| 2+ heróis com mesmo elemento | bônus específico do elemento (ex: 2 Fogo → +dano de área) |

> Isso cria razão para diversificar o gacha e incentiva builds temáticas sem tornar uma composição dominante obrigatória.

### 5.5. Torre — Modo Operação *(Fase 3B)*

Cada andar tem dois estados possíveis após a primeira conclusão:

- **Modo Exploração** (padrão, primeira vez): semi-automático; jogador recebe eventos e toma decisões pontuais; experiência narrativa.
- **Modo Operação** (andar concluído): totalmente automático com eventos de interrupção ocasionais; andar vira fonte de recursos específicos.

```
Explorar Andar → Descobrir → Completar → Desbloquear Operação → Automatizar → Otimizar
```

Durante a Operação:
- Grupo é enviado com objetivo (farm recurso / exploração leve) e perfil de risco (seguro / balanceado / agressivo).
- Combate é automático; eventos simples são resolvidos pelo líder.
- **Eventos de interrupção**: o sistema pausa e notifica o jogador com escolha de decisão (2 min para responder; líder decide baseado no perfil se não houver resposta).
- **Resumo consolidado** ao fim da operação: recursos obtidos, eventos resolvidos, riscos evitados.

**Recursos exclusivos por andar** (cada andar vira um "bioma produtivo"):

| Andar | Recurso exclusivo |
|---|---|
| 5 | Fragmento Rústico |
| 12 | Essência Corrompida |
| 18 | Cristal Arcano |
| 25 | Núcleo Sombrio |

**UX — 3 tipos de mensagem:**
1. **Evento** (interação): curto, com escolhas em botão
2. **Narrativa** (imersão): log flavor, opcional
3. **Resumo** (eficiência): consolidado, entendível em 5 segundos

### 5.6. Torre — Design Avançado *(Fase 3C)*

**Progresso % por andar:** cada andar não é uma luta — é um mini-mundo. Ações que contam para o %: inimigos chave derrotados (peso alto), áreas exploradas (médio), eventos resolvidos (alto), boss (grande), segredo descoberto (variável).

**Requisito secreto:** cada andar tem uma condição oculta adicional. Sem cumpri-la, o progresso trava em ~92%. Tipos: lógicos (matar boss específico primeiro), comportamentais (terminar sem mortes), exploratórios (achar sala secreta), hardcore (vencer boss com condição especial).

> Dicas são sempre indiretas — via log de líder, fala de NPC, descrição do ambiente. Nunca entregues diretamente.

**Zonas por andar:** cada andar dividido em regiões (Entrada, Bioma, Ruínas, Núcleo do Boss) com eventos locais, progressão não-linear e decisões de rota.

**Identidade mecânica por andar** (trait único):

| Andar | Trait |
|---|---|
| 10 | Anti-cura |
| 12 | Buffs aleatórios |
| 18 | Tempo limitado |
| 22 | Visibilidade reduzida |

**Anti-meta rígida** *(pós-lançamento)*: andares que penalizam builds específicas (anti-tank, anti-magia, anti-heal) — força adaptação de party; evita estratégia fixa dominante.

**Overclear:** 100% = concluído; 120% = segredo; 150% = domínio total → desbloqueia bônus de produção e eventos raros.

**Estado do andar** (muda ao longo do tempo):

| Estado | Efeito |
|---|---|
| Normal | Padrão |
| Corrompido | Inimigos mais agressivos |
| Instável | Eventos caóticos |
| Rico | Mais recursos |

**Seed de run:** identificador único por run para reprodutibilidade, debug e possível compartilhamento futuro.

---

## 6. Sistema de Cidade

> **Status:** ✅ Implementado

O jogador possui uma cidade que cresce com seus heróis. Heróis alocados em prédios produzem recursos passivamente. O jogador coleta periodicamente com `/cidade coletar`.

### 6.1. Estrutura Base (já no código)
```
Cidade
├── Nome / Nível / Populacao / CapacidadeMaxima
├── Recursos: Comida, Madeira, Pedra, Ouro, Erva
├── Construcoes[]
└── Trabalhadores[] (heroiId + início)
```
> A profissão do trabalhador é resolvida a partir da entidade `Heroi` no momento da coleta — não está armazenada em `PersonagemTrabalhador`.

### 6.2. Prédios e Heróis Ideais

**Prédios de v1.0 (Fase 3A):**

| Prédio | Profissões ideais | Produção | Upgrade |
|---|---|---|---|
| **Fazenda** | Agricultor, Cozinheiro | Comida | +capacidade de heróis na cidade |
| **Floresta / Serraria** | Lenhador, Caçador | Madeira | +produção |
| **Mina** | Mineiro | Pedra, minérios | +andares de boss desbloqueados |
| **Forja** | Ferreiro | Equipamentos para heróis | +qualidade dos itens |
| **Ateliê** | Alfaiate, Joalheiro | Armaduras, acessórios | +slots de equipamento |
| **Laboratório / Alquimia** | Alquimista | Poções, consumíveis | +efeito das poções |
| **Torre de Pesquisa** | Pesquisador, Mago | XP passivo, upgrades globais | +velocidade de pesquisa |
| **Templo** | Clérigo, Paladino | Recuperação de HP entre andares | +cura, bênçãos temporárias |
| **Guilda** | Qualquer combatente | Missões automáticas → Ouro + XP | +qualidade das missões |
| **Taverna** | Ladino, qualquer | Ouro passivo, rumores de missão | +ganho de ouro |
| **Arena** | Guerreiro, Arqueiro | XP de treino acelerado | +XP por sessão |

**Novos prédios (Fase 3B):**

| Prédio | Profissões | Função | Mecânica |
|---|---|---|---|
| **Prefeitura** | Líderes / qualquer | Centro de controle da cidade | Define limites globais (nº heróis alocados, nº construções); desbloqueia prédios avançados |
| **Armazém** | — | Limite de estoque | Overflow converte automaticamente (ex: madeira excedente → ouro a 80% eficiência); sem armazém, excesso é perdido |
| **Mercado** | Ladino, Comerciante | Conversão de recursos | Converte madeira → ouro, comida → buffs; pode operar automático; melhores taxas com upgrade |
| **Quartel** | Guerreiro, Paladino | Buffs para Torre | Melhora performance de grupos na Torre; pode definir formações automáticas |
| **Academia** | Pesquisador | XP global / passivas | Upgrades permanentes: +produção, +XP, +drop |
| **Tesouro** | — | Armazena ouro com proteção | Protege contra eventos negativos; pode gerar juros leve |
| **Torre de Vigilância** | Arqueiro, Caçador | Informação / previsibilidade | Melhora previsibilidade da Torre; +chance de encontrar segredos |
| **Pedreira** | Mineiro | Pedra em volume | Separa pedra de minério → mais controle de cadeia produtiva |
| **Santuário** | Mago, Clérigo | Buffs temporários globais | Ativa +drop, +XP, +chance de evento raro; bom sink de recursos |
| **Oficina de Caça** | Caçador | Carne, couro | Expande materiais de crafting para novos blueprints |

### 6.3. Autonomia e Confiança

O sistema de gestão escala com a coleção do jogador. Heróis não são peças passivas — têm dois eixos de estado:

> **Nota de implementação:** as faixas 21–100 de Confiança (auto-alocação, liderança, autonomia) são **Fase 3C**. Na Fase 3A, apenas a faixa 0–20 e o gate de Confiança para Slots de Responsabilidade são implementados. A IA de cidade sobre economia instável é difícil de debugar — implementar só após 3A e 3B validadas.

#### Confiança (0–100) — relação permanente com o Mestre

| Faixa | Título | Comportamento | Fase |
|---|---|---|---|
| 0–20 | Recém-invocado | Só trabalha onde foi explicitamente alocado | 3A |
| 21–40 | Leal | Se auto-aloca na profissão primária quando há slot livre | 3C |
| 41–60 | Dedicado | Toma iniciativa, produz acima do esperado | 3C |
| 61–80 | Parceiro | Assume liderança de um prédio (Slot de Liderança), gerencia heróis menores | 3C |
| 81–100 | Braço Direito | Executa a política da cidade de forma quase autônoma | 3C |

**Sobe via:** tempo de serviço, vitórias na Torre, bons equipamentos, trabalho na profissão correta  
**Cai via:** herói ignorado por muito tempo, derrotas repetidas, trabalho forçado fora da profissão

**Confiança como requisito estrutural (Fase 3A):** prédios possuem Slots de Responsabilidade que exigem `Confiança mínima` + atributo mínimo. Sem esses slots preenchidos, o prédio não funciona. Confiança alta também desbloqueia funções avançadas do prédio (ex: Forja com responsável em Confiança ≥ 71 → acesso ao crafting "Mestre da Forja").

> **Design note — risco de Confiança dominante:** Para evitar que Confiança se torne o único atributo que importa (meta dominante), considerar separação futura em dois eixos: **Confiança** (relacional — desbloqueios, liderança, resistência a traição) e **Disciplina** (operacional — eficiência de produção, velocidade de crafting). Disciplina seria mais fácil de aumentar/diminuir por tarefa; Confiança seria mais lenta e permanente. Implementar como campo separado se a Confiança única se provar problemática no beta.

#### Humor — estado diário

```
Deprimido → Mal-humorado → Neutro → Contente → Animado
   50%          75%          100%      125%       150%  eficiência
```

**Sobe via:** descanso, vitória, prédio de nível alto, líder motivador no mesmo prédio  
**Cai via:** derrota, trabalho fora da profissão, prédio deteriorado, realocação forçada

> Heróis sob liderança de um **Parceiro** têm a queda de humor reduzida quando realocados fora da profissão — o líder absorve o impacto.

### 6.4. Política da Cidade *(Fase 3C)*

> ⚠️ Auto-alocação e política são **Fase 3C** — dependem de economia estável e Confiança implementada. Na Fase 3A, toda alocação é manual.

O jogador não aloca herói por herói. Define uma **política macro** e os heróis agem:

| Política | Comportamento |
|---|---|
| `recursos` | Coletores e agricultores têm prioridade nos slots disponíveis |
| `producao` | Ferreiros, alquimistas e alfaiates em frente |
| `combate` | Combatentes treinam na Arena e a Guilda fica ativa |
| `equilibrio` | Distribuição automática balanceada por profissão |

- Heróis com confiança **Leal+** se auto-alocam seguindo a política ativa.
- Heróis **Recém-invocados** ficam em espera até alocação manual ou `/cidade otimizar`.
- `/cidade otimizar` aloca todos os heróis vagos no melhor slot disponível imediatamente.

### 6.5. Prioridade de Construções

Cada prédio tem um nível de prioridade independente da política global:

```
Pausado → Baixa → Normal → Alta → Crítica
```

- **Pausado:** produção para, heróis do prédio são redistribuídos automaticamente.
- **Crítica:** puxa heróis ociosos de prédios de prioridade menor para completar a cadeia de produção.
- Comando: `/cidade prioridade <prédio> <nível>`

### 6.6. Cadeia de Dependência Inteligente

Quando um prédio de produção está em **Alta** ou **Crítica**, o sistema rastreia automaticamente os insumos necessários e redireciona heróis ociosos para alimentar toda a cadeia upstream.

**Exemplo — Forja em Alta, produzindo Picareta de Ferro:**
```
Picareta de Ferro
 ├── Barra de Ferro (x2)
 │    └── Minério de Ferro (x4)  ← Mina  (redireciona Mineiros ociosos)
 └── Cabo de Madeira (x1)
      └── Madeira (x2)           ← Serraria (redireciona Lenhadores ociosos)
```

O sistema exibe o raciocínio no Discord:
```
Forja [Alta] → Picareta de Ferro
  ✓ Cabo de Madeira: estoque suficiente
  ✗ Barra de Ferro: faltam 6 unidades
    → 2 heróis ociosos redirecionados para a Mina
    → Estimativa: pronto em ~3h
```

**Regras da cadeia:**
- Heróis com profissão compatível têm prioridade no redirecionamento.
- Se não há heróis ociosos, heróis de prédios **Baixa** são temporariamente realocados.
- Heróis **Recém-invocados** não participam da cadeia automática — precisam de alocação manual.
- Heróis **Dedicados+** aceitam redirecionamento sem penalidade de humor.
- Heróis **Leais** e abaixo sofrem penalidade de humor leve ao serem redirecionados fora da profissão.

### 6.7. Alocação Manual
- Heróis alocados em um prédio **não podem participar de expedições à Torre** enquanto estiverem trabalhando.
- Um herói pode ser realocado a qualquer momento, mas perde o progresso de produção em andamento.
- Prédios têm capacidade máxima de slots por nível.

### 6.8. Produção Passiva

**Fórmula atual (Fase 2 — implementada):**
```
produção = taxa_profissao × horas_decorridas
```
`taxa_profissao` é um valor fixo por profissão definido no `CidadeService` (ex: Agricultor = 12/h, Lenhador = 10/h, Mineiro = 8/h). Sem modificador de humor ou bônus de habilidade ainda — a fórmula completa da Fase 3A substitui isso.

**Fórmula rework (Fase 3A — modelo de slots):**
```
ProduçãoFinal = Base(Prédio) × Nível × Multiplicador(Responsáveis) × Soma(Operadores) × HumorCidade
```
- `Multiplicador(Responsáveis)` = eficiência dos heróis no Slot de Responsabilidade; prédio inativo = 0
- `Soma(Operadores)` = soma da eficiência individual de todos os heróis no Slot de Operação
- `Eficiência individual` = `1 + (AtributoRelevante / 100)` — penalidade abaixo do requisito; bônus leve acima; cap anti-exploit
- Produção limitada a teto de 24h acumuladas para evitar estoques infinitos
- **Simulação offline**: `produção = taxaPorHora × horasDecorridas` (delta de timestamp, nunca tick-a-tick)

### 6.9. Upgrades de Prédios
- Construções têm nível 1 a 5.
- Cada nível custa recursos e tempo (calculado em horas reais).
- Construtor alocado reduz o tempo de upgrade.
- Upgrades desbloqueiam: mais slots, maior produção, novos itens ou funcionalidades.

### 6.10. Nível da Cidade
- O nível da cidade sobe conforme o total de prédios construídos e seus níveis.
- Nível da cidade aumenta `CapacidadeMaxima` (heróis que podem ser alocados).
- Níveis mais altos desbloqueiam novos prédios.

### 6.11. Rework de Slots — Modelo Determinístico *(Fase 3A)*

A cidade abandona o modelo de slot único genérico e passa a ter dois tipos de slot por prédio. Esse modelo é altamente controlável e facilita balanceamento.

#### Dois tipos de slot

**Slot de Responsabilidade** — heróis que "fazem o prédio funcionar":
- Requerem: Confiança mínima + atributo mínimo (depende do prédio e nível)
- Função: ativam o prédio; definem eficiência base; podem desbloquear funções avançadas
- Sem este slot preenchido: prédio NÃO funciona

**Slot de Operação** — heróis que executam o trabalho:
- Não precisam de Confiança alta
- Dependem dos responsáveis para existir
- Afetam volume e qualidade da produção

Exemplo — Forja Nível 3:
```
Forja Nv3
 ├── Slot de Responsabilidade (x2): Confiança ≥ 60, Força ou Int ≥ 40
 │     → prédio não funciona sem esses slots preenchidos
 └── Slots de Operação (x3): qualquer ferreiro
       → aumentam velocidade e volume de produção
```

#### Humor da Cidade (estado operacional global)
```
HumorCidade = média ponderada do Humor dos heróis alocados
```
- Heróis em prédios críticos → peso maior no cálculo
- Heróis em produção secundária → peso menor

| Estado | Range | Efeito |
|---|---|---|
| Ruim | 0–25 | -10% produção global |
| Neutro | 26–60 | base |
| Bom | 61–85 | +10% produção global |
| Excelente | 86–100 | +20% produção global |

> Sempre visível ao jogador; muda lentamente para evitar oscilação abrupta.

#### Slot de Liderança *(Fase 3C)*
Um slot de cada prédio pode ser designado como "Líder". Heróis com Confiança ≥ 61 (Parceiro) podem ocupar esse slot:
- +10% produção global do prédio
- Sinergia com profissão (ex: ferreiro buffa outros ferreiros)
- Reduz penalidade de humor em realocações dentro do prédio

---

## 7. Sistema de Missões (Guilda)

> **Status:** 📋 Planejado

Heróis alocados na Guilda saem em missões automáticas periódicas. A Guilda tem um **Rank próprio** que cresce independente do nível do prédio e determina o tipo e qualidade das missões disponíveis.

### 7.1. Rank da Guilda

O Rank da Guilda da cidade cresce conforme missões são concluídas com sucesso. Quanto mais alto o rank, melhores as missões disponíveis e maiores as recompensas.

| # | Rank | Missões disponíveis | Recompensas adicionais |
|---|---|---|---|
| 1 | **Ferro** | Coleta e Subjugação simples | Ouro e materiais básicos |
| 2 | **Bronze** | + variedade de Coleta e Subjugação | + XP bônus leve |
| 3 | **Aço** | + Escolta simples | Acesso a receitas comuns |
| 4 | **Prata** | + Investigação e Escolta média | + Blueprints básicos |
| 5 | **Ouro** | Missões de elite começam a aparecer | + Materiais intermediários |
| 6 | **Platina** | + Transporte e Recuperação | + Fragmentos de Arquétipo (raro) |
| 7 | **Ametista** | Alto prestígio regional; missões longas | + Fragmentos de Arquétipo (regular) |
| 8 | **Jade** | Missões com influência política | + Blueprints raros |
| 9 | **Esmeralda** | Acesso a contratos secretos | + Materiais raros garantidos |
| 10 | **Safira** | Agentes de elite; missões interregionais | + Fragmentos de Personagens Fixos (raro) |
| 11 | **Rubi** | Operações altamente sigilosas | + Fragmentos de Personagens Fixos (regular) |
| 12 | **Diamante** | Missões de escala mundial | + Blueprints lendários |
| 13 | **Mithral** | Lendas vivas; missões únicas | + Itens exclusivos de Mithral |
| 14 | **Adamantina** | Campeões consagrados | Recompensas máximas do jogo |
| 15 | **Oricalco** | Status mítico; missões impossíveis | Tier de endgame absoluto |

**Progressão de rank:** cada missão concluída com sucesso total soma pontos. Falhas subtraem. O rank nunca cai abaixo do tier anterior (piso fixo por rank conquistado).

### 7.2. Tipos de Missão
`Coleta | Subjugação | Escolta | Transporte | Investigação | Recuperação`

### 7.3. Funcionamento

- O mural de missões é **gerado automaticamente a cada 6h** para cada cidade (até 8 missões disponíveis simultaneamente).
- O jogador envia heróis via `/cidade missoes enviar` — o herói fica indisponível até retornar.
- Cada missão tem **duração real** (ex: 1h, 4h, 12h) e **dificuldade** comparada ao poder do herói enviado.
- `/cidade missoes` lista as missões ativas, tempo restante e heróis designados.

### 7.4. Sucesso e Falha

O resultado é calculado comparando o poder do herói com a dificuldade da missão:

| Resultado | Condição | Consequência |
|---|---|---|
| **Sucesso total** | Herói bem acima da dificuldade | Recompensa completa + bônus de rank |
| **Sucesso parcial** | Herói levemente abaixo | Metade das recompensas |
| **Falha** | Herói muito fraco | Sem recompensa, herói retorna exausto (penalidade de Humor) |
| **Expirada** | Missão não aceita antes do próximo ciclo | Missão removida, sem penalidade |

Missões têm **prazo de aceitação**: se não forem aceitas até o próximo ciclo de geração, expiram silenciosamente.

### 7.5. Recompensas de Missão

- Ouro e XP proporcionais à dificuldade
- Materiais de crafting (tipo varia conforme missão: Coleta → matérias-primas; Subjugação → materiais de boss)
- Missões Platina/Mithral: Fragmentos de Arquétipo ou de Personagens Fixos
- Blueprints raros como recompensa de Investigação e Recuperação de alto rank

---

## 8. Sistema de Crafting (Forja / Ateliê / Laboratório)

> **Status:** ✅ MVP implementado

### 8.1. Equipamentos
- Heróis alocados na Forja produzem itens ao longo do tempo (produção passiva, como outros prédios).
- Ao concluir um item, é realizado um **check de qualidade** que determina o tier final.
- Qualidades: `Comum | Bom | Raro | Excepcional | Mestre`
- Equipamentos são atribuídos a heróis via `/heroi equipar`.

### 8.2. Check de Qualidade

No final de cada produção, o sistema faz um check automático:

```
resultado = skill_craft_do_heroi + bônus_nível_prédio + roll(1..20)
```

| Resultado | Qualidade |
|---|---|
| < 10 | Comum |
| 10–14 | Bom |
| 15–19 | Raro |
| 20–24 | Excepcional |
| 25+ | Mestre |

- Heróis com skill de Craft alta têm média de resultados muito melhor.
- Prédio de nível alto contribui com bônus fixo ao check.
- **Falha crítica** (roll 1 + skill baixa): item saiu defeituoso — consome metade dos materiais, sem produto.

### 8.3. Heróis Auxiliares

Mais de um herói pode trabalhar no mesmo prédio de produção:

- O herói principal determina o check de qualidade (sua skill é usada).
- Cada auxiliar com profissão compatível **reduz o tempo de produção** em uma fração.
- Ex: Forja com 1 Ferreiro (principal) + 1 auxiliar → tempo reduzido em 25%.
- Heróis com profissão incompatível no mesmo prédio não contribuem (não prejudicam).

### 8.4. Poções e Consumíveis
- Laboratório / Alquimia produz poções de HP e bônus temporários.
- Usadas automaticamente pelo grupo na Torre, ou manualmente.
- Check de qualidade também se aplica: poção Mestre tem efeito maior.

### 8.5. Blueprints e Receitas
- Receitas básicas disponíveis desde o início (ver 8.7).
- **Blueprints** desbloqueiam receitas avançadas: obtidos via missões da Guilda, drops de boss, ou rank alto da Guilda.
- Itens de qualidade Excepcional/Mestre podem exigir blueprints específicos.
- Blueprints são permanentes por cidade — uma vez desbloqueado, fica disponível para sempre.

### 8.6. Upgrades de Oficina

Cada prédio de produção pode ser "especializado" com upgrades internos que afetam o crafting além do nível do prédio:

| Upgrade | Efeito |
|---|---|
| Bancada Reforçada (Forja) | +3 ao check de qualidade |
| Fole Aprimorado (Forja) | -20% tempo de produção |
| Banco de Ervas (Lab) | +2 check + acesso a poções raras |
| Tear Veloz (Ateliê) | -15% tempo de produção |

Upgrades custam recursos e são desbloqueados conforme o nível do prédio.

### 8.7. Cadeia de Receitas — v1.0

#### Recursos brutos (coletados por heróis)

| Recurso | Coletado em | Profissões ideais |
|---|---|---|
| Minério de Ferro | Mina | Mineiro |
| Madeira | Serraria / Floresta | Lenhador |
| Pedra | Mina | Mineiro |
| Comida | Fazenda | Agricultor, Cozinheiro |
| Erva | Fazenda / Floresta | Agricultor, Caçador |

#### Recursos processados (Forja)

| Item | Ingredientes |
|---|---|
| Barra de Ferro | Minério de Ferro x3 |
| Tábua de Madeira | Madeira x2 |

#### Equipamentos (Forja / Ateliê)

| Item | Ingredientes | Efeito |
|---|---|---|
| Espada de Ferro | Barra de Ferro x2 | +Força (Guerreiro, Paladino) |
| Arco Simples | Tábua de Madeira x2 | +Percepção (Arqueiro) |
| Armadura de Couro | Madeira x1 + Comida x2 | +Vitalidade (qualquer) |
| Picareta de Ferro | Barra de Ferro x2 + Tábua de Madeira x1 | +produção da Mina quando equipada por Mineiro |

#### Consumíveis (Laboratório)

| Item | Ingredientes | Efeito |
|---|---|---|
| Poção Simples | Comida x3 | Restaura HP na Torre |
| Poção de Força | Comida x2 + Erva x1 | +Força temporário na Torre |

---

## 9. Arena de Combate

> **Status:** ✅ Implementado

A Arena é um prédio especial da cidade — não produz recursos, mas é a principal fonte de **XP acelerado** e **prestígio** para heróis de combate.

### 9.1. Treino
- Heróis alocados na Arena treinam continuamente.
- Ganham XP de habilidades de Combate passivamente (mais rápido que na Torre).
- Custo: a Arena consome Comida passivamente para manter os treinos ativos.
- `/treinar <herói>` envia um herói para sessão intensiva: XP em burst, custo de Ouro + Comida.

### 9.2. Desafios de Ondas
- Eventos ativados manualmente pelo jogador (`/arena desafio`).
- A party enfrenta ondas crescentes de inimigos (diferente da Torre, que é andar por andar).
- Recompensas escalam com a onda sobrevivida: Ouro, XP bônus, materiais raros.
- Cada desafio tem cooldown (ex: 1 por dia).

### 9.3. Ranking da Arena
Heróis que participam de desafios acumulam **Prestígio**, formando um ranking pessoal. Prestígio alto desbloqueia títulos honoríficos exibidos no `/ver_heroi`:

| Prestígio | Título |
|---|---|
| 0–99 | — |
| 100–499 | Iniciante |
| 500–1499 | Combatente |
| 1500–3999 | Veterano |
| 4000–9999 | Campeão |
| 10000+ | Lenda da Arena |

### 9.4. Torneios *(futuro — pós v1.0)*
- Eventos periódicos com chaveamento automático.
- Jogadores inscrevem heróis; confrontos são resolvidos automaticamente.
- Vencedor recebe recompensas únicas e Prestígio em massa.
- Sistema de apostas: jogadores apostam Ouro no resultado.

---

## 10. Sistema de Relíquias

Relíquias são itens passivos especiais, diferentes de equipamentos. Cada herói tem **3 slots de relíquia** independentes dos slots de equipamento. São obtidas exclusivamente em **boss fights** da Torre — não são craftáveis nem compráveis.

### 10.1. Mecânica Central

- **3 slots** por herói (relíquia 1, relíquia 2, relíquia 3).
- Cada slot aceita qualquer relíquia (sem restrição de slot).
- **Removíveis**: o jogador pode desencaixar uma relíquia a qualquer momento via `/heroi reliquia remover`.
- **Transferíveis**: uma relíquia removida vai para o inventário do Mestre e pode ser equipada em outro herói com `/heroi reliquia equipar`.
- Cada herói pode ter no máximo **uma cópia da mesma relíquia** nos seus 3 slots.
- Sem custo de equipar/remover — a restrição é só de drop (raras de obter, não de usar).

### 10.2. Catálogo de Relíquias

| Relíquia | Drop a partir do andar | Efeito passivo | Restrição |
|---|---|---|---|
| **Ouvido de Drow** | Andar 10 | Imunidade a Amedrontado e Envenenado; +5 Percepção | — |
| **Perna de Eladrin** | Andar 25 | +8% resistência física; 1x por combate pode trocar de posição com aliado (esquiva forçada) | — |
| **Voz de Sereia** | Andar 25 | Resistência a Frio (dano pela metade); +8 Inteligência; comunicação com criaturas aquáticas (efeito de lore) | — |
| **Braço de Titivilus** | Andar 50 | Imunidade a Fogo e Frio; regenera 3% do HP máximo por turno | Arquétipo Combate |
| **Olho de Githzerai** | Andar 50 | Imunidade a Enfeitiçado e Paralisado; bloqueia 30% do dano Psíquico; +10 Percepção | — |
| **Coração de Múmia** | Andar 75 | Imunidade a Necrótico; +15 Vitalidade; recupera HP igual a 10% do dano causado ao matar um inimigo | — |

> As relíquias são inspiradas em partes de seres poderosos — o herói não se transforma, absorve a essência do fragmento. A imagem é a de uma gema octagonal encaixada no equipamento ou na armadura.

### 10.3. Taxa de Drop

- Boss de andares múltiplos de 5 e 10 têm chance pequena de dropar relíquias do tier correspondente.
- Boss de andar 25 e 50 têm chance maior de dropar relíquias mais raras.
- Drop vai direto para o **inventário do Mestre** (não é equipado automaticamente).
- O jogador escolhe em qual herói equipar.

### 10.4. Comandos de Relíquia

| Comando | Ação |
|---|---|
| `/heroi reliquia ver <herói>` | Exibe os 3 slots e as relíquias equipadas |
| `/heroi reliquia equipar <herói> <relíquia>` | Equipa relíquia do inventário no próximo slot livre |
| `/heroi reliquia remover <herói> <slot>` | Remove relíquia do slot e devolve ao inventário |
| `/inventario reliquias` | Lista todas as relíquias no inventário do Mestre |

### 10.5. Estratégia de Builds

A combinação de 3 slots permite builds focadas:

- **Tank puro:** Perna de Eladrin + Braço de Titivilus + Ouvido de Drow → resistência física, regeneração, imunidade a CC
- **Mago ofensivo:** Voz de Sereia + Olho de Githzerai + Coração de Múmia → bonus de INT, imunidade a controle, sustain por kills
- **Suporte de coleta:** Ouvido de Drow + Perna de Eladrin (heróis de coleta raramente combatem — relíquias dão bônus de atributo passivo mesmo fora da Torre)

> Heróis de Coleta e Produção também se beneficiam dos bônus de atributo das relíquias mesmo quando alocados na Cidade — o efeito é sempre passivo.

> **Design note (futuro):** As relíquias são literalmente fragmentos de seres poderosos. O conceito se estende naturalmente ao sistema de Sacrifício/Síntese (pós-lançamento): heróis sacrificados podem gerar uma "Relíquia de Herói" com traço único. Bosses de andares avançados cujos fragmentos não são coletáveis hoje poderiam gerar relíquias de tier superior.

---

## 11. Sistema de Conversão de Heróis *(Fase 3B)*

> **Status:** 📋 Planejado

Heróis de baixa utilidade podem ser convertidos em valor estratégico. O sistema resolve o acúmulo estrutural de 1★/2★ sem microgerenciamento — são válvulas de economia controlada, não descarte.

### 11.1. Venda (Conversão em Ouro)

- Herói é permanentemente convertido em Ouro.
- Fórmula: `Valor = BaseRaridade × EscalaDeNivel × FatorDeEscassezGlobal`
- **Raridade pesa mais que nível** para evitar o exploit "farm de XP → venda → ouro infinito".
- Referência de papel econômico:

| Raridade | Papel |
|---|---|
| 1★ | Lixo funcional — ouro baixo, alto volume de descarte |
| 2★ | Conversão leve — ouro + utilidade ocasional |
| 3★ | Recurso intermediário de economia |
| 4★ | Valor alto — decisão estratégica |
| 5★ | Conversão quase proibitiva (alto custo emocional) |

- **Bloqueios anti-exploit:** herói equipado / em missão / na Torre ativo / alocado em prédio → bloqueado; precisa desalocar primeiro.

### 11.2. Absorção (Conversão em Progresso)

- Um herói é consumido para transferir 50% do seu XP acumulado para o herói-alvo.
- **Mesmo peso de raridade**: raridade alta do consumido transfere mais valor (não apenas XP bruto).
- Mesmos bloqueios da Venda se aplicam ao herói consumido e ao alvo.

### 11.3. Loop de Decisão

```
Herói excedente
 → Muito caro de sustentar (Sustento)? → candidato a Venda / Absorção
 → Nível alto mas inútil em combate? → Absorção (transfere XP para herói útil)
 → 1★/2★ genérico? → Venda rápida para ouro
```

### 11.4. Comandos

| Comando | Ação |
|---|---|
| `/heroi vender <herói>` | Converte em Ouro com confirmação explícita (irreversível) |
| `/heroi absorver <alvo> <consumido>` | Consome herói e transfere XP com confirmação |

---

## 12. Sistema de Sustento *(Fase 3B)*

> **Status:** 📋 Planejado

Heróis não são ativos estáticos — exigem manutenção contínua. O Sustento cria pressão de gestão sem punir, forçando decisões de priorização e dando valor natural ao sistema de Conversão.

### 12.1. Recursos de Sustento

**Comida** — consumo diário por herói:
```
Consumo = Base × Raridade × (1 + Nivel / 100)
```
Guerreiros e Tanques consomem mais que Magos e Suportes.

**Moradia** — define quantos heróis podem ser mantidos sem penalidade estrutural. Prédio Alojamento define a capacidade. Sem moradia suficiente: redução de moral, aumento de custo de manutenção, chance de eventos negativos.

### 12.2. Estados do Herói

| Estado | Condição | Efeito |
|---|---|---|
| **Ativo** | Totalmente sustentado | 100% eficiência |
| **Instável** | Falta parcial de recursos | -X% atributos, -Y% ganho de XP |
| **Degradado** | Falta crítica | Habilidades desativadas; risco de deserção temporária |
| **Inativo** | Definido manualmente pelo jogador | Não consome (ou consome muito menos); não participa de atividades |

### 12.3. Loop de Decisão

- Manter muitos heróis → custo alto de Comida
- Focar em poucos → eficiência máxima
- Rotacionar heróis (ativo/inativo) → otimização de recursos
- Heróis excedentes caros → candidatos naturais para Conversão (seção 11)

### 12.4. Integração com Outros Sistemas

- **Fazenda**: produção de Comida alimenta o pool de Sustento automaticamente.
- **Alojamentos** (via Prefeitura/Quartel): aumentam capacidade de Moradia.
- **Conversão de Heróis**: Sustento é o gatilho natural para decidir vender ou absorver excedentes.
- **Cidade**: heróis em estado Degradado puxam HumorCidade para baixo indiretamente.

### 12.5. Grace Period — Modelo de Dívida (não punição direta)

Em vez de punir imediatamente quando falta comida, o sistema acumula **dívida alimentar**:

```
Falta comida → gera Dívida Alimentar (cresce por hora)
Herói só entra em estado Instável depois de X horas de dívida
Herói só entra em Degradado com dívida crítica prolongada
```

Isso elimina o risco de "jogador volta e tudo está quebrado" — a punição é gradual e recuperável.

### 12.6. Princípios de Balanceamento

- Early game: custo baixo → incentivo a expandir roster
- Mid game: pressão começa — Fazenda precisa escalar com o roster
- Late game: otimização obrigatória (quem fica ativo, quem fica inativo, quem vira recurso)
- **Nunca travar o jogador**: sempre oferecer alternativa (estado Inativo, Conversão, buffs de Fazenda avançada)
- **Grace period**: dívida antes de punição; jogador que ficou offline não encontra cidade destruída ao voltar

---

## 13. Sistema de Invasão, Traição e Expedições *(Pós-lançamento)*

> **Status:** 📋 Planejado

Este sistema cria três pressões simultâneas sobre o jogador: crescer a cidade aumenta o risco de invasão; a fraqueza interna (Confiança baixa) abre brechas para traição; e as Expedições oferecem uma saída ofensiva de alto risco/recompensa que pode gerar retaliação.

### 13.1. Invasões NPC

O mundo não é estático. Periodicamente, facções NPC atacam a cidade do jogador.

**Gatilhos de invasão** (dirigidos por estado, não aleatórios puros):

| Fator | Impacto |
|---|---|
| Nível da cidade alto | +chance de invasão |
| Estoque de ouro/recursos alto | +chance |
| Rank da Guilda alto | +qualidade dos invasores |
| Falta de heróis de defesa | +chance crítica |
| HumorCidade Ruim | +chance |
| HumorCidade Excelente | -chance |

**Tipos de invasão:**
- **Saque**: rouba % dos recursos; duração curta; resolve rápido
- **Sabotagem**: ataca prédios específicos; reduz nível de construção temporariamente
- **Sequestro**: tenta capturar heróis alocados; herói fica "em cativeiro" temporário
- **Cerco** *(late game)*: evento raro; cidade entra em estado de guerra; múltiplas ondas; impacta todos os sistemas

**Defesa da cidade** = heróis alocados (fator principal) + prédios de defesa (Torre, Guilda, Arena) + Confiança média dos heróis

**Herói capturado:** fica em estado "Em Cativeiro"; fora de todas as atividades. Resgate via:
- Missão de resgate na Torre (andar instanciado) — A
- Pagamento de resgate em Ouro/recursos — B (chance de falha por rank da invasão)

**Facções NPC (futuro):**

| Facção | Estilo |
|---|---|
| Saqueadores da Névoa | Loot pesado |
| Ordem Quebrada | Sequestro de heróis |
| Engenheiros Abissais | Sabotagem de prédios |
| Culto do Vazio | Invasões raras e massivas |

### 13.2. Sistema de Traição (Confiança Negativa)

Quando a Confiança de um herói cai abaixo de um limiar crítico, ele pode deixar de ser ineficiente e se tornar um agente hostil interno.

**Condições de ativação:**

| Confiança | Estado |
|---|---|
| 20–0 | Instável — comportamento negativo leve |
| < 0 | Traidor Ativo — hostil; só heróis ≥ 3★ para evitar spam de 1★ irrelevante |

**Comportamento do Traidor:**
1. **Sabotagem interna** — reduz produção de um prédio que ele conhecia
2. **Fuga com recursos** — rouba parte do estoque (prioriza ouro e materiais raros)
3. **Facilitação de invasões** — aumenta chance de invasão NPC; pode "abrir portões"
4. **Venda de informação** *(late game)* — buffa facções NPC temporariamente

**Interação com Invasão NPC:**

| Situação | Efeito |
|---|---|
| Traidor ativo + invasão NPC | Invasores recebem bônus tático |
| Múltiplos traidores | Chance de "cerco interno" |
| Traidor capturado | Reduz severamente dificuldade de invasões futuras |

**Resolução pelo jogador:**
1. **Execução** (permanente) — remove herói; pode gerar queda de moral global
2. **Prisão** (temporário) — herói inativo; não pode ser usado
3. **Redenção** — missão especial para restaurar Confiança; pode virar evento narrativo
4. **Expulsão** (soft delete) — herói removido do roster; pode retornar via gacha com "marca narrativa"

**Função de design:** Confiança passa a ter risco real nos dois extremos — alta desbloqueia autonomia; baixa gera ameaça sistêmica interna.

### 13.3. Sistema de Expedições (Jogador → Mundo)

Além da Torre, o jogador pode enviar heróis para invadir locais do mundo (vilas, fortalezas, ruínas, caravanas, bases de facções). É o contrapeso ofensivo ao sistema de Invasão.

**Diferença da Torre:**

| Torre | Expedição |
|---|---|
| Progressão linear | Missões pontuais |
| Sem perda estrutural | Pode perder heróis |
| Controle previsível | Alto risco |
| Sempre positivo | Pode dar prejuízo |

**Tipos de Expedição:**
- **Saque**: foco em ouro/recursos; baixa dificuldade; baixo risco
- **Ataque a Fortaleza**: alto combate; drop de materiais raros/relíquias; chance de ferimentos graves
- **Incursão Especial**: objetivo único (roubar blueprint, capturar alvo, destruir algo); alta variância
- **Sequestro**: captura NPCs especiais; pode gerar bônus de produção, troca por recursos, eventos narrativos

**Sistema de Risco:**
```
Chance de Sucesso = Poder da Party / Dificuldade × Modificadores
```

| Resultado | Efeito |
|---|---|
| Sucesso total | Recompensa máxima |
| Sucesso parcial | Recompensa reduzida + dano |
| Falha | Sem recompensa + penalidade |
| Falha crítica | Perda de herói / captura temporária |

**Sinergia com Confiança:** heróis com alta Confiança têm menor chance de falha, resistem melhor a penalidades, menor chance de captura. Heróis com baixa Confiança podem fugir no meio ou causar falha crítica.

**Custo:** Comida + Ouro + Tempo. Heróis em expedição não produzem na cidade e não defendem invasões → decisão real entre produzir OU arriscar.

**Integração com Facções NPC:**

| Ação | Consequência |
|---|---|
| Invadir facção | Aumenta hostilidade |
| Atacar repetidamente | Gera retaliação (invasão na cidade) |
| Alvo neutro | Mantém equilíbrio |

**Loop completo:**
```
INVOCAR → SUBIR TORRE → FORTALECER CIDADE → FAZER EXPEDIÇÕES
 → GERAR RISCO (retaliação / invasão) → DEFENDER / REAGIR → repetir
```

**Comandos:**

| Comando | Função |
|---|---|
| `/expedicoes ver` | Lista invasões disponíveis |
| `/expedicoes enviar` | Envia party |
| `/expedicoes status` | Acompanha progresso |
| `/expedicoes resgatar` | Coleta resultado |
| `/expedicoes cancelar` | Aborta com penalidade |

---

## 14. Nível do Mestre e Meta Progressão *(Fase 3B)*

> **Status:** 📋 Planejado

O jogador (Mestre) tem sua própria progressão independente dos heróis.

### 14.1. Nível do Mestre

- Progride com atividade global: pulls de gacha, andares subidos, produções coletadas, missões concluídas
- Cada nível desbloqueia um bônus passivo global (ex: +1% XP para todos os heróis; +capacidade de heróis na cidade)
- Exibido no perfil do jogador com título baseado no nível
- Funciona como "identidade de conta" — jogadores com níveis altos têm progressão visível mesmo sem heróis 5★

### 14.2. Traços Fixos na Ascensão *(decisão irreversível leve)*

Ao ascender um herói para 4★ (3★→4★), o jogador escolhe 1 traço permanente para o herói:

| Traço | Efeito |
|---|---|
| Incansável | +5% XP em todas as fontes |
| Pragmático | +10% Ouro de todas as fontes |
| Vigilante | +10% chance de crit |
| Dedicado | +10% eficiência na Cidade |
| Resiliente | +5% resistência a todas as condições de status |

- Traço é permanente e não pode ser alterado após escolha
- Cria identidade única por herói e incentiva decisões com peso real
- O mesmo herói invocado duas vezes pode ter traços diferentes (escolha do jogador)

---

## 15. Princípios de Design e UX

### 15.1. Modelo de Tempo Híbrido

O jogo usa diferentes modelos de progressão por sistema:

| Sistema | Modelo | Racional |
|---|---|---|
| Cidade (produção) | Idle — progride offline | Loop passivo, não exige presença |
| Torre | Ativo — progresso com ação | Experiência narrativa, decisões |
| Torre Operação | Semi-idle — farm com eventos | Compromisso intermediário |
| Missões | Semi-idle — envia e espera | Loop paralelo sem atenção constante |
| Arena | Ativo — comandos manuais | Conteúdo de burst e eventos |

> Regra técnica: toda produção usa `timestamp + delta`. Nunca tick-a-tick. Nunca loop contínuo.

### 15.2. Princípio de UX — 3 Camadas

Toda resposta do bot deve ter 3 níveis de detalhe:

```
Nível 1 — Resumo (resposta padrão, entendível em 5 segundos):
  Cidade: Comida 1.200 (+80/h) | Madeira 540 (+30/h)
  Humor: Bom (+10%) | ⚠ Falta Ferro — Forja parada

Nível 2 — Detalhado (botão "Ver mais"):
  breakdown por prédio, heróis alocados, eficiências

Nível 3 — Debug (comando admin):
  todos os valores calculados, fonte de cada número
```

> Se o jogador precisar ler um parágrafo para entender a resposta padrão, o design falhou.

### 15.3. Evento ao Logar (Retenção)

Ao executar qualquer comando, o bot verifica e exibe no máximo 1 item pendente relevante:

- Missão concluída com recompensa pronta
- Alerta de cidade (prédio parado, gargalo crítico)
- Herói que subiu de nível
- Decisão pendente na Torre (evento esperando resposta)

> Isso cria hábito de retorno sem spam.

### 15.4. Fail Interesting

Falha não deve ser apenas punição — deve gerar conteúdo:

- Missão falhou → pode gerar evento narrativo derivado
- Herói capturado em missão → abre instância de resgate na Torre
- Craft defeituoso → item especial "quebrado" com propriedade incomum (ideia futura)
- Run da Torre wipe → próximo attempt tem dica indireta baseada na causa da falha

### 15.5. Prevenção de Power Creep

- Bônus em % ao invés de flat onde possível
- Soft caps em stacks de bônus (ex: bônus de composição de party não acumula infinitamente)
- Diminishing returns na produção em escala (ex: prédio com 10 heróis não produz 10x mais que com 1)
- Fragmentos de arquétipo com cap diário suave (evita AFK farming infinito)

### 15.6. Sinks de Ouro Explícitos

Para evitar inflação, ouro deve ter destinos obrigatórios:

| Sink | Timing |
|---|---|
| Manutenção leve de prédios (custo passivo) | 3A.2 |
| Reroll de missões no mural da Guilda | 3B |
| Upgrades de prédio (progressivamente caros) | 3A.1+ |
| Custo de Expedições | Pós-lançamento |
| Apostas nos Torneios da Arena | Pós-lançamento |

### 15.7. Modelo de Interação Híbrido *(UX-0 em diante)*

O jogo evolui de comandos puros para um modelo híbrido: **comando abre painel; ações acontecem via botões**.

#### Princípio central

```
/sistema → painel público (persiste no canal)
  → botão de ação  → resultado efêmero (só o jogador vê)
  → botão de navegação → sub-painel atualiza in-place (UpdateAsync)
  → expiração após 15 min → /sistema reabre painel fresco
```

#### Tipos de interação

| Tipo | Quando usar | Primitivo Discord |
|---|---|---|
| **Painel** | Visão principal de um sistema | Embed público + botões |
| **Ação** | Operação simples (coletar, treinar, subir andar) | Botão → feedback efêmero |
| **Confirmação** | Ação irreversível (vender herói, absorver) | Efêmero com `[Confirmar] [Cancelar]` + timeout 30s |
| **Sub-painel** | Detalhe navegável (prédio, herói específico) | Atualiza mensagem in-place |
| **Select Menu** | Escolha de lista (qual herói, qual prédio, qual andar) | `SelectMenuComponent` |
| **Modal** | Input de texto livre (apelido, URL de arte, quantidade) | `ModalBuilder` acionado por botão |

#### Regras de visibilidade

- `/cidade`, `/torre`, `/heroi` → **públicos** — persistem no canal como referência social
- Feedback de ação, confirmações, sub-fluxos → **efêmeros** — só o jogador vê
- Eventos de milestone (pull 5★, boss derrubado, nível cap atingido) → **públicos** separados

#### Arquitetura de suporte

```
DiscordCommand / InteractionHandler
    → Service  (sem tipos Discord — retorna ViewModel)
    → PanelBuilder  (ViewModel → Embed + ComponentBuilder)
```

- `customId` dos botões segue convenção: `sistema:acao[:param1:param2]`
- Services não conhecem Discord; PanelBuilders não chamam Services diretamente
- Dados sempre lidos do banco a cada interação — sem cache de estado de painel

---

## 16. Comandos e Interações

O modelo de interação é híbrido: comandos abrem painéis; a maioria das ações acontece via botões. Veja §15.7 para o modelo completo.

### 16.1. Painéis Principais (entrada via comando, ações via botões)

| Comando | Painel abre | Botões / ações disponíveis |
|---|---|---|
| `/cidade` | Painel da cidade (público) | `[Coletar]` `[Construir]` `[Alocar]` `[Ver Prédios]` `[Missões]` |
| `/heroi` | Lista de heróis + detalhe (público) | `[Treinar]` `[Equipar]` `[Alocar]` `[Vender]` `[Absorver]` `[Ascender]` |
| `/torre` | Painel da Torre (público) | `[Subir Andar]` `[Trocar Party]` `[Modo Operação]` |
| `/arena` | Painel da Arena (público) | `[Treinar Herói]` `[Desafio de Ondas]` |
| `/guilda` | Painel da Guilda (público) — *Fase 3B* | `[Missões Ativas]` `[Missões Disponíveis]` `[Enviar Heróis]` |
| `/inventario` | Inventário por categoria — *Fase 3B* | `[Equipamentos]` `[Relíquias]` `[Recursos]` `[Consumíveis]` |

> `/ver_heroi` e `/listar_herois` são substituídos pelo painel `/heroi`. Os comandos antigos podem ser mantidos como alias enquanto a transição ocorre.

### 16.2. Comandos de Entrada Direta (permanecem como comando)

| Comando | Descrição |
|---|---|
| `/invocar` | Sistema gacha — pull x1 ou x11; resultado embed público |
| `/grupo` | Gerenciar party — permanece como comando por enquanto |
| `/cidade construir <prédio>` | Construção com validação de recursos; pode migrar para modal no painel |
| `/heroi arte <herói> <url>` | Define arte customizada via URL |
| `/heroi apelido <herói> <nome>` | Define apelido — modal acionado do painel de herói |
| `/heroi ascender <herói>` | Ascensão de raridade — botão no painel de herói |

### 16.3. Comandos de Automação e Gestão *(Fase 3C)*

| Comando | Descrição |
|---|---|
| `/cidade politica <foco>` | Define política macro: `recursos \| producao \| combate \| equilibrio` |
| `/cidade prioridade <prédio> <nível>` | Define prioridade: `pausado \| baixa \| normal \| alta \| critica` |
| `/cidade otimizar` | Auto-aloca todos os heróis vagos no melhor slot |
| `/cidade cadeia <prédio>` | Exibe cadeia de dependência e gargalos |

### 16.4. Comandos Admin / Debug *(Fase 3.5)*

| Comando | Descrição |
|---|---|
| `/admin resetar_cidade <userId>` | Reseta cidade para estado inicial |
| `/admin dar_recursos <userId> <tipo> <qtd>` | Injeta recursos para teste |
| `/admin spawn_heroi <userId> <raridade>` | Spawna herói para teste |
| `/admin ver_estado <userId>` | Dump completo do estado do jogador |
| `/admin forcar_nivel <heroiId> <nivel>` | Avança nível para testar caps |

---

## 17. Progressão e Endgame

```
Início:
  → Invocar primeiros heróis
  → Montar primeira party
  → Subir andares iniciais da Torre

Médio prazo:
  → Construir prédios básicos (Fazenda, Forja, Guilda)
  → Alocar heróis de coleta na cidade
  → Equipar heróis com itens da Forja
  → Avançar na Torre com party mais forte

Longo prazo:
  → Cidade com todos os prédios em nível alto
  → Heróis 5★ com habilidades masterizadas
  → Desafios de andares de boss difíceis (múltiplos de 25)
  → Banners de evento com personagens exclusivos
```

---

## 18. Status de Implementação *(atualizado em 2026-04-17 — pós Fase 3A.2)*

| Sistema | Status | Fase |
|---|---|---|
| Gacha com soft-pity e banners | ✅ Implementado | 2 |
| Geração procedural de heróis | ✅ Implementado | 2 |
| Sistema de habilidades com XP | ✅ Implementado | 2 |
| Party (até 5 heróis) | ✅ Implementado | 2 |
| Torre infinita com bosses | ✅ Implementado | 2 |
| Combate automático por turnos | ✅ Implementado | 2 |
| Listagem paginada de heróis | ✅ Implementado | 2 |
| Autocomplete nos comandos | ✅ Implementado | 2 |
| Estrutura base da Cidade (entidade) | ✅ Implementado | 2 |
| Profissões no enum | ✅ Implementado | 2 |
| Recursos (Comida, Madeira, Pedra, Ouro, Erva) | ✅ Implementado | 2 |
| Produção passiva com cap de 24h | ✅ Implementado | 2 |
| Curva de XP (`B_r × nível`) + level-up com cap por raridade | ✅ Implementado | 3A.1 |
| Fórmula de combate (dano, crit, burst cap, ATB) | ✅ Implementado | 3A.1 |
| XP e Ouro na Torre por andar + boss mults | ✅ Implementado | 3A.1 |
| Stats base por raridade + bônus racial (+50) | ✅ Implementado | 3A.1 |
| Crafting — 5 receitas estáticas + check de qualidade | ✅ Implementado | 3A.1 |
| `/heroi_equipar` com bônus persistidos | ✅ Implementado | 3A.1 |
| Pool de personagens fixos 5★/4★ (9 personagens, seed) | ✅ Implementado | 3A.2 |
| Campo `Confiança` (0–100) e `Humor` (0–100) no herói | ✅ Implementado | 3A.2 |
| Modelo de Slots da Cidade (Responsabilidade + Operação) | ✅ Implementado | 3A.2 |
| ResourceNode (Campo/Floresta/Mina/Prado) sem slot | ✅ Implementado | 3A.2 |
| `PredioConfig` + `ResourceNodeConfig` data-driven | ✅ Implementado | 3A.2 |
| Humor da Cidade (média dos alocados, mult 0.9–1.2×) | ✅ Implementado | 3A.2 |
| `/cidade construir`, `alocar_recurso`, `alocar_predio`, `desalocar` | ✅ Implementado | 3A.2 |
| Arena — `/treinar` (burst XP, cooldown 4h) | ✅ Implementado | 3A.2 |
| Arena — `/arena desafio` (ondas, cooldown 24h) | ✅ Implementado | 3A.2 |
| Painel de interação com botões (`/cidade`) | ⏳ UX-0 | UX-0 |
| Inventário Unificado | ⏳ Planejado | 3B-1 |
| Conversão de Heróis (Venda + Absorção) | ⏳ Planejado | 3B-2 |
| Torre — Modo Operação | ⏳ Planejado | 3B-3 |
| Sistema de Sustento (Comida/Moradia/Estados) | ⏳ Planejado | 3B-4 |
| Sistema de Missões (Guilda, 15 ranks) | ⏳ Planejado | 3B-5 |
| Sistema de Relíquias (drop, inventário, equip/remover) | ⏳ Planejado | 3B-6 |
| Novos Prédios (Armazém, Mercado, Prefeitura) | ⏳ Planejado | 3B-7 |
| Nível do Mestre + Traços na ascensão 4★ | ⏳ Planejado | 3B-Meta |
| Upgrades de prédio nível 2→3 | ⏳ Planejado | 3B |
| Ascensão por fragmentos | ⏳ Planejado | 3B |
| Arte no embed do pull | ⏳ Planejado | Qualquer |
| Apelidos e arte customizada de herói | ⏳ Planejado | Qualquer |
| Progressão de skills por atividade (onde está → skill evolui) | ⏳ Planejado | 3B |
| Política da cidade (macro gestão) | ⏳ Planejado | 3C |
| Auto-alocação e cadeia de dependência inteligente | ⏳ Planejado | 3C |
| Torre — design avançado (%, secreto, zonas, identidade) | ⏳ Planejado | 3C |
| Invasão NPC, Traição, Expedições | ⏳ Planejado | Pós-lançamento |
| Sistema de Mercado P2P (`#mercado`, listagens, compra/venda) | ⏳ Planejado | 3B-Mercado |

---

## 19. Sistema de Mercado P2P *(Fase 3B-Mercado)*

> **Status:** 📋 Planejado

Mercado de itens entre jogadores via canal Discord dedicado. Heróis **não são negociáveis** — apenas equipamentos e consumíveis craftados ou obtidos na Torre.

### 19.1. Superfícies e Responsabilidades

| Superfície | Papel |
|---|---|
| Canal `#mercado` | Vitrine pública — feed de listagens ativas como mensagens do bot |
| `/mercado` | Painel de gestão **efêmero** — vender, ver e cancelar próprias listagens |
| Flows efêmeros | Seleção de item, definição de preço, confirmação de compra |

### 19.2. Regras Econômicas

| Regra | Valor | Configurável via |
|---|---|---|
| Taxa de listagem | 5 Ouro flat (não reembolsável) | `MarketConfig` |
| Taxa de venda | 10% base | `MarketConfig.BaseTaxRate` |
| Taxa com prédio Mercado nível 1 | 8% | `CidadeService.GetBuildingLevel` |
| Taxa com prédio Mercado nível 2 | 6% | — |
| Taxa com prédio Mercado nível 3 | 3% | — |
| Listagens ativas por jogador | 3 | `MarketConfig.MaxActiveListings` |
| Expiração | 24h | `MarketConfig.ListingDurationHours` |
| Preço mínimo | 50% do `BasePrice` | `MarketConfig.MinPriceFactor` |
| Preço máximo | 1000% do `BasePrice` | `MarketConfig.MaxPriceFactor` |

A taxa de listagem é um **ouro sink ativo** — mesmo que o item não seja vendido, a economia perdeu ouro. Isso desincentiva listagens especulativas e vitrine fake sem punir tentativas honestas.

### 19.3. Itens Negociáveis

| Tipo | Phase 1 | Futuro |
|---|---|---|
| Equipamentos (armas, armaduras, acessórios) | ✅ | — |
| Consumíveis (poções, buffs) | ✅ | — |
| Recursos brutos (madeira, pedra, minério) | ❌ | Step 3 |
| Relíquias | ❌ | Step 3 (após 3B-6) |
| Heróis | ❌ | Nunca (fora do escopo) |

Extensibilidade via `ItemConfig.IsTradeable` (bool) — nenhum switch/if por tipo.

### 19.4. Ciclo de Vida da Listagem

```
[/mercado → Vender Item]
       │  taxa de listagem cobrada (5 Ouro)
       ▼
[Item locked no Inventário]
       │
       ▼
[MarketListing — Status: Active]
[Bot posta mensagem em #mercado]
       │
   ────┴─────────────────────────────────────
   │                                         │
[Comprador clica Comprar]             [24h sem compra]
   │                                         │
[Transaction atômica]                 [ExpiryWorker]
   ├─ Sucesso:                               │
   │   item → inventário comprador     [Mensagem editada: ⏰ EXPIRADA]
   │   ouro transferido com taxa       [Item desbloqueado]
   │   mensagem editada: ✅ VENDIDO    [Mensagem deletada após 10 min]
   │   mensagem deletada após 30s
   │
   └─ Race condition:
       "Item já foi vendido."
```

### 19.5. Concorrência

`BEGIN IMMEDIATE TRANSACTION` no SQLite — idêntico ao sistema de missões. O segundo comprador recebe resposta efêmera imediata sem janela de duplicação. `RowVersion` no `MarketListing` para concorrência otimista via EF Core.

### 19.6. Mensagem de Listagem no Canal

Cada listagem é uma mensagem pública com embed + botão. A mensagem é sempre editada antes de ser deletada (nunca deletada direto sem feedback visual):

```
🗡️  ESPADA DE FERRO — Qualidade: Raro
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Tipo: Equipamento  │  Slot: Arma
Vendedor: @player
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
💰 Preço: 600 Ouro
📊 Referência: 450 Ouro  |  Média recente: 520 Ouro
⏱️ Expira em: 23h 55m
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[Comprar]
```

Quando resolvida, a mensagem é editada para mostrar o estado (`✅ VENDIDO` / `⏰ EXPIRADA` / `❌ CANCELADA`) com o botão desabilitado, antes de ser deletada.

### 19.7. Integração com Sistemas

- **Inventário** (3B-1): campo `IsLocked` no `InventarioEntry`; operações `Lock/Unlock` atômicas
- **Prédio Mercado** (3B-7): nível do prédio reduz taxa de venda via `GetTaxRateForPlayer()`
- **Crafting**: itens craftados entram no inventário e podem ser listados voluntariamente
- **Torre**: drops de boss (equipamentos, consumíveis) alimentam o mercado naturalmente
- **Economia global** (`DESIGN_SISTEMAS.md §6`): taxa de venda é ouro sink real; alinha com modelo de equilíbrio

---

## 20. Sistema de Mercenários + Treinamento como Serviço

> **Status:** 📋 Planejado

Dois sistemas integrados que criam economia de heróis entre jogadores: empréstimo temporário (Mercenários) e XP offline contratado (Treinamento). Compartilham infraestrutura de snapshot e lock.

### 20.1 Princípios de Design

- **Herói nunca transferido** — snapshot capturado no momento do empréstimo; write-once; frozen para sempre
- **Complementar, não substituto** — Treinamento é suplementar à Torre e Arena (cap semanal por herói)
- **Seguro por design** — locks mutex no herói, `BEGIN IMMEDIATE TRANSACTION`, anti-chain bilateral
- **Data-driven** — custos, duração, XP bonus, slots via `MercenarioConfig` e `TreinamentoConfig`
- **Técnicas Especiais** — NOT Phase 1; deferido para pós-lançamento

### 20.2 Superfícies de Interação

| Superfície | Visibilidade | Conteúdo |
|---|---|---|
| `/mercenarios` | Efêmero (privado) | Gerenciar empréstimo: disponibilizar, buscar, cancelar |
| `#mercenarios` | Público | Cards dos heróis disponíveis + `[Contratar]` button |
| `/treinamento` | Efêmero (privado) | Gerenciar treinos: oferecer, enviar herói, ver ativos |
| `#treinamento` | Público | Cards de treinadores com rank badge + `[Enviar Herói]` button |

### 20.3 Modelo de Dados

**HeroSnapshot** (write-once, compartilhado):
- `Id`, `HeroiOrigemId`, `DonoId`, `Tipo` (Mercenario/Treinamento)
- Stats congelados: `Nome`, `Nivel`, `Vida`, `Ataque`, `Defesa`, `Agilidade`, `Raridade`, `Raca`, `HabilidadeEspecial`, `PowerScore`
- `CapturedAt`, `ExpiresAt`

**HeroLockStatus** (enum estendido):
```
None | LockedForSale | AsMercenary | InTraining | AsTrainer
```

**EmprestimoHeroi**: snapshot + dono + contratante + status + duração + custo + ChannelMessageId

**TreinamentoHeroi**: snapshot treinador + herói aluno + XpCap (frozen) + XpGanho (preenchido ao concluir) + status

### 20.4 IHeroCombatant

Interface que abstrai `Heroi` real e `HeroSnapshot` para os serviços de combate:
```
Id | Nome | Nivel | Vida | Ataque | Defesa | Agilidade | Raridade | Raca | HabilidadeEspecial | PowerScore
```
`CombatService`, `TorreService`, `MissaoService` aceitam `IHeroCombatant`.
`ArenaService` permanece usando `Heroi` diretamente — mercenários **proibidos na Arena**.

### 20.5 Regras — Mercenários

| Regra | Implementação |
|---|---|
| Não pode contratar próprio herói | `ContratanteId != DonoId` |
| Max 1 emprestado + 1 contratado simultâneo | Query active loans por jogador |
| Anti-chain A→B + B→A bloqueado | Cross-check active (DonoId, ContratanteId) |
| Same-pair cooldown 24h | Query `TreinamentoHeroi` history |
| Custo: `CustoBase + (CustoPorNivel × heroi.Nivel)` | `MercenarioConfig` por DuracaoOpcao |
| Duração: 6h / 12h / 24h | Enum `DuracaoOpcao` |
| NPC fallback sempre disponível | `IsNpc=true`, stats virtuais do config |

**Uso permitido:** Torre ✅ Missões ✅ Cidade ❌ Arena ❌

**Justificativa Arena proibida:** Arena pareia por PowerScore — mercenário inflacionaria score sem representar progressão real; combinado com ArenaRank → TreinamentoBonus criaria loop circular de escalonamento artificial.

### 20.6 Regras — Treinamento

| Regra | Implementação |
|---|---|
| ArenaRank ≥ 1 obrigatório para treinar | Validação ao oferecer |
| Aluno ≠ Treinador (mesmo jogador) | `AlunoId != TreinadorId` |
| Anti-chain bilateral | Cross-check sessões ativas |
| Same-pair cooldown 24h | Query history por (TreinadorId, AlunoId) |
| Cap semanal por herói | `500 + (heroi.Nivel × 50)` XP/semana |
| XpCap frozen na criação | `CalcWeeklyCap - AcumuladoSemana` |
| NPC trainers: 40% eficiência | `NpcEficiência%` no `TreinamentoConfig` |
| Pagamento split | 70% → treinador, 30% sink de ouro |

**Fórmula XP:**
```
base_xp = TrainerPowerScore × XpFatorConfig × DuracaoHoras × (1 + ArenaBonus%)
xp_final = min(base_xp, XpCap)
```
`XpFatorConfig = 0.02` (calibrável no beta sem rebuild).

**TreinamentoConfig por ArenaRank:**
| Rank | XpBonus | MaxSlots | Duração | NPC Efic. |
|---|---|---|---|---|
| 0 | — | 0 | — | 40% |
| 1 | +10% | 1 | 4h | 40% |
| 2 | +20% | 2 | 4h | 40% |
| 3 | +35% | 2 | 6h | 40% |
| 4 | +50% | 3 | 6h | 40% |
| 5 | +75% | 3 | 8h | 40% |

### 20.7 Ciclo de Vida (ambos)

```
Disponibilizar → Post canal → [Contratar/Enviar Herói]
  → BEGIN IMMEDIATE TRANSACTION (validações anti-exploit)
  → Status = Ativo → herói locked
  → ExpiryWorker (poll 5 min) → conclude/expire
  → unlock herói → edit channel message → delete após delay
  → notification na fila do jogador (exibe no próximo comando)
```

### 20.8 Integração com outros sistemas

- **Arena** (`ArenaService`): usa `Heroi` direto — isola mercenários da competição
- **Torre/Missões**: `SnapshotService.GetActiveForContratante()` → `IHeroCombatant?`
- **Conversão (3B-2)**: guard `LockStatus == None` bloqueia venda/absorção de herói locked
- **HeroiLevelUpService**: `TreinamentoExpiryWorker` chama `AddXpAsync(heroi, xpFinal)` na conclusão
- **Economia** (`DESIGN_SISTEMAS.md §6`): 30% do custo de treino é ouro sink; custo de mercenário é transferência pura entre jogadores

---

## 21. Prioridades e Escopo por Fase  *(antigo §20)*

### P0 — Core Loop (Fase 3A) ✅ concluída

1. ✅ **Curva de XP** — `B_r × nível` (linear), migra para `l^1.25` no beta
2. ✅ **Personagens fixos** — 9 personagens 5★/4★ no seed
3. ✅ **Leveling** — XP por Torre, level-up, caps por raridade
4. ✅ **Modelo de Slots da Cidade** — Responsabilidade + Operação, Confiança como gate, Humor da Cidade
5. ✅ **Crafting** — 5 receitas com check de qualidade
6. ✅ **Arena** — treino (burst XP) e desafio de ondas

### Fase Q — Qualidade (antes da 3B)
> Fechar dívida técnica de 3A.

- `Random.Shared` no GachaService
- `ILogger<T>` substituindo `Console.WriteLine`
- Guild ID + caminho do banco em appsettings/env
- Guard clauses centralizadas (herói em missão / alocado / inativo)
- Testes unitários: GachaService, HeroiLevelUpService, CombatService, CidadeService
- Teste de integração: gacha → alocar → produzir → evoluir

### Fase UX-0 — Camada de Interação (antes de expandir sistemas)
> Padrão de UX híbrido validado com `/cidade` antes de aplicar a outros sistemas.

- `InteractionRouter` + `PanelBuilder` (ViewModel → Embed + ComponentBuilder)
- `/cidade` convertido para painel público com botões e Select Menu
- Padrões: DeferAsync, UpdateAsync, confirmação efêmera (§15.7)

### P1 — Expansão (Fase 3B)
> Implementar após UX-0 validado. Cada sistema usa o padrão de painel desde o primeiro dia.

| # | Sistema | Por que esta ordem |
|---|---|---|
| 3B-1 | **Inventário Unificado** | Pré-requisito para Relíquias |
| 3B-2 | **Conversão de Heróis** (Venda + Absorção) | Sink econômico imediato; sem dependências |
| 3B-3 | **Torre — Modo Operação** | Farm loop; replay value na Torre |
| 3B-4 | **Sustento** (Comida / Moradia / Estados) | Pressão econômica; dá sentido à Conversão |
| 3B-5 | **Guilda / Missões** | Loop paralelo; requer HeroPowerScore estável |
| 3B-6 | **Relíquias** | Requer Inventário (3B-1) |
| 3B-7 | **Novos Prédios** (Armazém, Mercado, Prefeitura) | Requer Sustento + Guilda funcionando |
| 3B-Mercado | **Sistema de Mercado P2P** | Requer Inventário (3B-1); pode rodar em paralelo com 3B-7 |
| 3B-Merc | **Mercenários** (empréstimo de heróis) | Snapshot model; pode rodar em paralelo com 3B-Mercado |
| 3B-Treino | **Treinamento como Serviço** | Requer Arena rank ≥ 1; complementar à Torre/Arena |
| 3B-Meta | **Nível do Mestre + Traços 4★** | Requer atividade de todos os sistemas 3B |

### P2 — IA e Automação (Fase 3C)
> Só após economia e combate equilibrados no beta.

- **Gestão autônoma da cidade** — política, auto-alocação, cadeia inteligente, lideranças
- **Torre avançada** — progresso % por andar, requisito secreto, zonas, identidade mecânica

### Pós-lançamento (sem data)

- **Invasão NPC + Traição** — pressão de crescimento, risco de perda de herói
- **Expedições** — loop ofensivo de alto risco/recompensa com retaliação
- **Anti-meta rígida na Torre** — andares com counters de build
- **Torneios da Arena** com apostas em Ouro
- **Sacrifício/Síntese** — consumir herói gera relíquia especial
- **Heróis Únicos / Permadeath** — decisões de design que mudam o escopo radicalmente
