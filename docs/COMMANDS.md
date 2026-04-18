# Legends Awaken — Referência de Comandos

Todos os slash commands disponíveis no bot, organizados por sistema. Parâmetros entre `<>` são obrigatórios; entre `[]` são opcionais.

---

## Heróis

### `/ver_heroi <nome>`
Exibe o embed completo de um herói: atributos, raça, profissão, nível, XP, habilidades e equipamentos.

| Parâmetro | Tipo | Obrigatório | Autocomplete |
|---|---|---|---|
| `nome` | string | ✅ | ✅ |

---

### `/listar_herois [raridade]`
Lista todos os seus heróis com paginação de 25 por página. Botões ⏮️ Anterior / ⏭️ Próximo na resposta.

| Parâmetro | Tipo | Obrigatório | Valores |
|---|---|---|---|
| `raridade` | integer | ❌ | 1, 2, 3, 4, 5 |

---

### `/heroi_equipar <heroi> <item_id>`
Equipa um item craftado em um herói. O `item_id` é exibido na resposta do `/crafting fazer`.

| Parâmetro | Tipo | Obrigatório | Autocomplete |
|---|---|---|---|
| `heroi` | string | ✅ | ✅ |
| `item_id` | string | ✅ | ❌ |

---

## Coleção e Fragmentos

### `/colecao`
Abre o painel de coleção. Exibe todos os heróis com estado de desbloqueio e progresso.

**Painel resultante:**
- ✅ Herói desbloqueado
- 🔒 `X/Y fragmentos` — em progresso (TipoUnlock: Fragmentos)
- 🔒 `Andar X` — aguardando marco da Torre (TipoUnlock: MarcoTorre)
- ❓ — condição única (TipoUnlock: CondicaoUnica)
- Barra de progresso `█░░░░░░░░░` quando aplicável

**Interações no painel:**
| ID | Tipo | Ação |
|---|---|---|
| `colecao_recrutar` | Select Menu | Recruta herói quando fragmentos suficientes; gera feedback efêmero |

---

### `/bioma`
Exibe o bioma atual da Torre: nome, faixa de andares, heróis disponíveis como drop e pesos relativos.

**Interações no painel:**
| ID | Tipo | Ação |
|---|---|---|
| `bioma_ver_colecao` | Botão | Abre o painel `/colecao` |
| `bioma_contratos` | Botão | Abre o painel `/contrato` |

---

### `/contrato`
Exibe os contratos de drop ativos e permite gerenciá-los.

**Painel resultante:**
- **Contrato Arquétipo** — +30% fragmentos de heróis da profissão escolhida (sem prazo)
- **Contrato Nomeado** — +50% fragmentos de herói específico (requer ≥ 1 fragmento já coletado)

**Interações no painel:**
| ID | Tipo | Ação |
|---|---|---|
| `contrato_arquetipo` | Select Menu | Ativa contrato de arquétipo (Guerreiro / Agricultor / Ferreiro) |
| `contrato_remover_nomeado` | Botão (danger) | Desativa contrato nomeado ativo |

---

## Torre

### `/subir_andar`
Inicia combate automático contra o próximo andar da Torre usando a party ativa.

- XP concedido ao limpar: `10 + Numero×5` × multiplicador de boss
- Ouro concedido: `5 + Numero×3` × multiplicador de boss
- Multiplicadores de boss: andar 5 = 1.5×, andar 10 = 2.0×, andar 25 = 3.0×
- 30% de chance de drop de fragmento ao limpar
- Bioma novo e heróis de marco são detectados automaticamente

---

## Arena

### `/treinar <heroi>`
Treina um herói com XP acelerado (3× `XpParaProximoNivel`).

| Parâmetro | Tipo | Obrigatório | Autocomplete |
|---|---|---|---|
| `heroi` | string | ✅ | ✅ |

- **Custo:** 100 Ouro + 10 Comida
- **Cooldown:** 4 horas por herói

---

### `/arena <acao>`
Comandos da Arena.

| Parâmetro | Tipo | Obrigatório | Valores |
|---|---|---|---|
| `acao` | string | ✅ | `desafio` |

**`desafio`** — Envia os 5 heróis mais fortes para um desafio de ondas.
- Cooldown: 24 horas
- Resultado: embed com ondas completadas, XP e Ouro ganhos

---

## Cidade

### `/cidade <acao> [heroi] [predio] [node] [slot_tipo]`
Gerencia todos os aspectos da cidade. Nem todos os parâmetros são usados em cada ação.

| Parâmetro | Tipo | Obrigatório | Valores / Notas |
|---|---|---|---|
| `acao` | string | ✅ | ver, coletar, construir, alocar_recurso, alocar_predio, desalocar |
| `heroi` | string | ❌ (depende da ação) | Autocomplete |
| `predio` | string | ❌ | Fazenda, Serraria, Mina, Forja, Arena, Guilda |
| `node` | string | ❌ | Campo, Floresta, Mina, Prado |
| `slot_tipo` | string | ❌ | Responsabilidade, Operacao |

**Ações disponíveis:**

| Ação | Parâmetros usados | O que faz |
|---|---|---|
| `ver` | — | Exibe recursos, prédios com slots, coletores com taxa/h e Humor da Cidade |
| `coletar` | — | Coleta produção acumulada (cap: 24h) |
| `construir` | `predio` | Constrói prédio se recursos suficientes |
| `alocar_recurso` | `heroi`, `node` | Aloca herói em ResourceNode (Campo/Floresta/Mina/Prado) |
| `alocar_predio` | `heroi`, `predio`, `slot_tipo` | Aloca herói em slot de Responsabilidade ou Operação de um prédio |
| `desalocar` | `heroi` | Remove herói de qualquer alocação atual |

**Prédios disponíveis:**

| Prédio | Recurso produzido | Gate de Responsabilidade |
|---|---|---|
| Fazenda | Comida | Confiança ≥ 20 + atributo relevante |
| Serraria | Madeira | Confiança ≥ 20 + atributo relevante |
| Mina | Pedra | Confiança ≥ 20 + atributo relevante |
| Forja | (crafting) | Confiança ≥ 40 + atributo relevante |
| Arena | (treino) | Confiança ≥ 30 + atributo relevante |
| Guilda | (missões) | Confiança ≥ 50 + atributo relevante |

---

## Grupos

### `/grupo <acao> [nome_party] [heroi]`
Gerencia parties de heróis usadas na Torre e no combate.

| Parâmetro | Tipo | Obrigatório | Valores / Notas |
|---|---|---|---|
| `acao` | string | ✅ | criar, ver, adicionar, remover |
| `nome_party` | string | ❌ (obrigatório para criar/adicionar/remover) | Autocomplete |
| `heroi` | string | ❌ (obrigatório para adicionar/remover) | Autocomplete |

| Ação | Parâmetros necessários | O que faz |
|---|---|---|
| `criar` | `nome_party` | Cria nova party (máx 5 heróis) |
| `ver` | `nome_party` | Exibe heróis da party e atributos totais |
| `adicionar` | `nome_party`, `heroi` | Adiciona herói à party |
| `remover` | `nome_party`, `heroi` | Remove herói da party |

---

## Crafting

### `/crafting <acao> [receita]`
Sistema de crafting de itens equipáveis.

| Parâmetro | Tipo | Obrigatório | Valores |
|---|---|---|---|
| `acao` | string | ✅ | listar, fazer |
| `receita` | string | ❌ (obrigatório para `fazer`) | ID da receita (obtido em `listar`) |

**Receitas disponíveis:**

| ID | Item | Slot | Recursos necessários |
|---|---|---|---|
| `espada-ferro` | Espada de Ferro | Arma | Madeira + Pedra |
| `arco-simples` | Arco Simples | Arma | Madeira |
| `armadura-couro` | Armadura de Couro | Armadura | Madeira |
| `anel-arcano` | Anel Arcano | Acessório | Pedra |
| `amuleto-agilidade` | Amuleto de Agilidade | Acessório | Erva |

O resultado da qualidade depende do Responsável da Forja: `skill_craft + bônus_prédio(Nível×2) + roll(1..20)`.

---

## Resumo Rápido

| Comando | Sistema | Nota |
|---|---|---|
| `/ver_heroi` | Heróis | Autocomplete no nome |
| `/listar_herois` | Heróis | Paginação com botões |
| `/heroi_equipar` | Heróis | Requer ID do item |
| `/colecao` | Fragmentos | Painel com select menu de recrutar |
| `/bioma` | Fragmentos | Painel com botões de navegação |
| `/contrato` | Fragmentos | Painel com select menu de arquétipo |
| `/subir_andar` | Torre | Usa party ativa |
| `/treinar` | Arena | 4h cooldown, custo Ouro+Comida |
| `/arena` | Arena | `desafio`: 24h cooldown |
| `/cidade` | Cidade | 6 sub-ações com parâmetros combinados |
| `/grupo` | Grupos | 4 sub-ações; máx 5 heróis por party |
| `/crafting` | Crafting | 5 receitas; requer Forja construída |
