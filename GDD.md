# Legends Awaken — Game Design Document

> Inspirado em *Pick Me Up Infinite Gacha*.  
> Bot RPG para Discord em C#.

---

## 1. Visão Geral

O jogador é um **Mestre** que invoca heróis via gacha, envia grupos para escalar uma Torre infinita e gerencia uma **Cidade** que cresce conforme sua coleção de heróis evolui. O loop central é: **invocar → alocar → progredir**.

---

## 2. Loop de Jogo

```
[INVOCAR]  → Gacha gera heróis novos
    ↓
[ALOCAR]   → Heróis são designados à Torre ou a um prédio da Cidade
    ↓
[PRODUZIR] → Cidade gera recursos/XP passivamente enquanto o jogador está offline
    ↓
[COMBATER] → Party sobe andares da Torre, ganha recompensas
    ↓
[MELHORAR] → Recursos da cidade financiam upgrades de prédios e equipamentos
    ↓
 (volta ao início)
```

---

## 3. Sistema de Gacha (Reformulado)

### 3.1. Modelo Híbrido

| Raridade | Tipo | Arte | Identidade |
|---|---|---|---|
| 5★ | Personagem fixo nomeado | Arte IA única | Nome, lore, habilidade exclusiva |
| 4★ | Personagem fixo nomeado | Arte IA única | Nome, lore, habilidade exclusiva |
| 3★ | Procedural por arquétipo | Arte por classe/raça (compartilhada) | Nome gerado, stats variados |
| 2★ | Procedural | Ícone por classe | Genérico |
| 1★ | Procedural | Ícone por classe | Genérico |

### 3.2. Soft-pity (já implementado)
- Curva cúbica a partir do pity configurado por banner.
- Reset ao obter 4★ ou superior.

### 3.3. Banners
- **Banner Padrão:** pool fixo de todos os personagens nomeados + procedurais.
- **Banner de Evento:** destaca personagens 5★ específicos com rate-up.
- **Banner de Profissão:** aumenta chance de personagens de uma profissão específica (útil para quem precisa de um Ferreiro, por exemplo).

### 3.4. Arte dos Personagens Fixos
- Gerada por IA (Midjourney / DALL-E) com prompt padronizado por classe e raça.
- URL da imagem salva no banco e exibida no embed do pull.

---

## 4. Sistema de Heróis

### 4.1. Atributos Base (já implementado)
- **Força** — dano físico, carga
- **Agilidade** — velocidade, evasão, crítico
- **Vitalidade** — HP, recuperação
- **Inteligência** — dano mágico, pesquisa
- **Percepção** — detecção, precisão

### 4.2. Raças (já implementado)
`Humano | Bestial | Anão | Elfo | Draconato | Fada`

Cada raça concede bônus passivos em atributos e afinidades com certas profissões.

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

### 4.5. Progressão
- Heróis ganham XP via combate na Torre ou via trabalho na Cidade.
- Level up aumenta atributos base.
- Equipamentos craftados na Forja da cidade podem ser equipados.

---

## 5. Sistema de Torre (já implementado)

- Torre infinita com andares em sequência por usuário.
- Tipos de andar: `Subjugação | Fuga | Escolta | Defesa | Armadilha | EventoEspecial`
- Bosses em andares múltiplos de 5 (fácil), 10 (médio), 25 (difícil).
- Party de até 5 heróis.
- Combate automático por turnos.

**Recompensas por andar:**
- Ouro e XP para os heróis participantes.
- Materiais de crafting em andares de boss.
- Fragmentos para recrutar personagens fixos (futuro).

---

## 6. Sistema de Cidade

O jogador possui uma cidade que cresce com seus heróis. Heróis alocados em prédios produzem recursos passivamente. O jogador coleta periodicamente com `/cidade coletar`.

### 6.1. Estrutura Base (já no código)
```
Cidade
├── Nome / Nível / Populacao / CapacidadeMaxima
├── Recursos: Comida, Madeira, Pedra, Ouro
├── Construcoes[]
└── Trabalhadores[] (heroiId + profissao + início)
```

### 6.2. Prédios e Heróis Ideais

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

### 6.3. Autonomia e Confiança

O sistema de gestão escala com a coleção do jogador. Heróis não são peças passivas — têm dois eixos de estado:

#### Confiança (0–100) — relação permanente com o Mestre

| Faixa | Título | Comportamento |
|---|---|---|
| 0–20 | Recém-invocado | Só trabalha onde foi explicitamente alocado |
| 21–40 | Leal | Se auto-aloca na profissão primária quando há slot livre |
| 41–60 | Dedicado | Toma iniciativa, produz acima do esperado |
| 61–80 | Parceiro | Assume liderança de um prédio, gerencia heróis menores |
| 81–100 | Braço Direito | Executa a política da cidade de forma quase autônoma |

**Sobe via:** tempo de serviço, vitórias na Torre, bons equipamentos, trabalho na profissão correta  
**Cai via:** herói ignorado por muito tempo, derrotas repetidas, trabalho forçado fora da profissão

#### Humor — estado diário

```
Deprimido → Mal-humorado → Neutro → Contente → Animado
   50%          75%          100%      125%       150%  eficiência
```

**Sobe via:** descanso, vitória, prédio de nível alto, líder motivador no mesmo prédio  
**Cai via:** derrota, trabalho fora da profissão, prédio deteriorado, realocação forçada

> Heróis sob liderança de um **Parceiro** têm a queda de humor reduzida quando realocados fora da profissão — o líder absorve o impacto.

### 6.4. Política da Cidade

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
- A produção é calculada com base no tempo decorrido desde a última coleta.
- Fórmula base: `produção = (produção_base_herói + bônus_habilidade) × modificador_humor × horas_decorridas`
- Produção é limitada a um teto de 24h acumuladas para evitar estoques infinitos.

### 6.9. Upgrades de Prédios
- Construções têm nível 1 a 5.
- Cada nível custa recursos e tempo (calculado em horas reais).
- Construtor alocado reduz o tempo de upgrade.
- Upgrades desbloqueiam: mais slots, maior produção, novos itens ou funcionalidades.

### 6.10. Nível da Cidade
- O nível da cidade sobe conforme o total de prédios construídos e seus níveis.
- Nível da cidade aumenta `CapacidadeMaxima` (heróis que podem ser alocados).
- Níveis mais altos desbloqueiam novos prédios.

---

## 7. Sistema de Missões (Guilda)

Heróis alocados na Guilda saem em missões automáticas periódicas.

### 7.1. Tipos de Missão
`Coleta | Subjugação | Escolta | Transporte | Investigação | Recuperação`

### 7.2. Funcionamento
- Missões são geradas automaticamente com base no nível da Guilda.
- Herói parte → volta após duração → traz recompensas.
- Heróis mais fortes completam missões mais difíceis, com melhores recompensas.
- Falha possível se o herói for muito fraco para a missão.

### 7.3. Recompensas de Missão
- Ouro, materiais, XP.
- Missões raras: fragmentos de personagens fixos, blueprints de crafting.

---

## 8. Sistema de Crafting (Forja / Ateliê / Laboratório)

### 8.1. Equipamentos
- Heróis na Forja produzem armas e armaduras ao longo do tempo.
- Qualidade depende do nível de habilidade do herói + nível da Forja.
- Qualidades: `Comum | Bom | Raro | Excepcional | Mestre`
- Equipamentos são atribuídos a heróis via `/heroi equipar`.

### 8.2. Poções e Consumíveis
- Laboratório / Alquimia produz poções de HP, bônus temporários.
- Usadas automaticamente pelo grupo na Torre, ou manualmente.

### 8.3. Blueprints
- Receitas desbloqueadas via missões, drops de boss ou reputação.
- Itens raros exigem materiais de andares avançados da Torre.

---

## 9. Comandos Planejados

| Comando | Descrição |
|---|---|
| `/invocar` | Sistema gacha — já implementado |
| `/ver_heroi` | Detalhes do herói — já implementado |
| `/listar_herois` | Lista paginada — já implementada |
| `/grupo` | Gerenciar party — já implementado |
| `/subir_andar` | Combate na Torre — já implementado |
| `/cidade ver` | Painel geral da cidade (recursos, prédios, heróis alocados, humor geral) |
| `/cidade coletar` | Coleta produção acumulada de todos os prédios |
| `/cidade politica <foco>` | Define política macro: `recursos \| producao \| combate \| equilibrio` |
| `/cidade prioridade <prédio> <nível>` | Define prioridade do prédio: `pausado \| baixa \| normal \| alta \| critica` |
| `/cidade otimizar` | Auto-aloca todos os heróis vagos no melhor slot disponível |
| `/cidade alocar <herói> <prédio>` | Alocação manual de herói em um prédio |
| `/cidade desalocar <herói>` | Remove herói do trabalho |
| `/cidade construir <prédio>` | Inicia construção ou upgrade de prédio |
| `/cidade missoes` | Vê missões ativas na guilda e seus status |
| `/cidade cadeia <prédio>` | Exibe a cadeia de dependência e status atual da produção |
| `/treinar <herói>` | Envia herói para a Arena (treino acelerado) |

---

## 10. Progressão e Endgame

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

## 11. O que Está Implementado Hoje

| Sistema | Status |
|---|---|
| Gacha com soft-pity e banners | Implementado |
| Geração procedural de heróis | Implementado |
| Sistema de habilidades com XP | Implementado |
| Party (até 5 heróis) | Implementado |
| Torre infinita com bosses | Implementado |
| Combate automático por turnos | Implementado |
| Listagem paginada de heróis | Implementado |
| Autocomplete nos comandos | Implementado |
| Estrutura base da Cidade (entidade) | Implementado |
| Profissões no enum | Implementado |
| Recursos (Comida, Madeira, Pedra, Ouro) | Implementado |
| Personagens fixos nomeados | Não iniciado |
| Arte via URL nos heróis | Não iniciado |
| Sistema de Confiança e Humor | Não iniciado |
| Política da cidade (macro gestão) | Não iniciado |
| Prioridade por construção | Não iniciado |
| Cadeia de dependência inteligente | Não iniciado |
| Alocação de heróis na cidade | Não iniciado |
| Produção passiva da cidade | Não iniciado |
| Upgrades de prédios | Não iniciado |
| Sistema de missões (Guilda) | Não iniciado |
| Crafting de equipamentos | Não iniciado |
| `/treinar` (Arena) | Stub — lógica não implementada |

---

## 12. Próximas Prioridades Sugeridas

1. **Personagens fixos** — adicionar campo `ImageUrl` + `Lore` nos heróis, definir pool de 5★/4★
2. **Comando `/cidade ver`** — painel básico mostrando recursos e heróis alocados
3. **Alocação de heróis** — `/cidade alocar` e produção passiva simples
4. **Coleta** — `/cidade coletar` com cálculo por tempo decorrido
5. **Forja** — primeiro prédio funcional completo (crafting de equipamentos simples)
