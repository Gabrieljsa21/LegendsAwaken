# Design — Torre: Arcos Narrativos

Framework de design e catálogo de arcos da Torre Infinita.  
Cada arco = um grupo de andares com narrativa, flags e mecânicas próprias.

---

## Framework

### Estrutura de Objetivos (3 Tiers)

| Tier | Tipo | Consequência | Obrigatoriedade |
|------|------|-------------|-----------------|
| A | Colecionável | Recompensa imediata (item, recurso) | Opcional |
| B | Objetivo Principal | Consequência narrativa no arco | Obrigatório para avançar |
| C | Objetivo Secundário | Desbloqueio de conteúdo (flag, NPC, rota) | Opcional, mas expira |

**Regra 70/30:** ~70% dos andares têm objetivo secundário. Andares de transição e pré-boss (30%) não têm — servem como pausa de tensão.

**Expirado:** se o grupo avança sem completar um secundário, o flag não é gerado e o benefício associado é perdido permanentemente neste ciclo.

### Sistema de Flags

- **Flags simples:** `[snake_case]` — geradas por ação do grupo em um andar específico
- **Flags compostas (flagsCompostas):** criadas quando dois ou mais flags de andares diferentes coexistem; desbloqueiam conteúdo que nenhum flag individual ativaria
- **Flags de boss:** `bossDerrotado` | `bossFugiu` — únicas; "ignorado" não existe (boss é portão obrigatório)

### Categorias de Colecionáveis

| Categoria | Descrição |
|-----------|-----------|
| Lore | Texto, fragmento de lore, história do mundo |
| Economia | Valor em ouro ou recurso vendável |
| Build | Efeito de stats (geralmente com tradeoff) |
| Chave | Usável em contexto específico (boss, andar seguinte) |
| Arquivo | Combina com outro colecionável para gerar flag composta |

### Bônus por Flag — Calibração

| Tier do arco | Bônus máximo por flag | Acumulado máximo |
|-------------|----------------------|-----------------|
| Andares 1–5 | 5–10% | 15% |
| Andares 6–15 | 10–15% | 20% |
| Andares 16–25 | 15–20% | 25% |

Rendimentos decrescentes: cada flag além do cap contribui menos que a anterior.

### Estados do Boss

- `bossDerrotado` — boss eliminado em combate; recompensas completas
- `bossFugiu` — boss escapa ou é contido; aparece em arco futuro com +15–20% stats e nova mecânica adquirida pela experiência

### Camadas de Documentação

- **Camada de Design (interna):** lógica de flags, condições, modificadores, thresholds
- **Camada de Display (player-facing):** texto evocativo, sem termos técnicos, sem spoilers de mecânica

---

## Arco 1 — Torre em Ruínas

**Andares:** 1–4  
**Tom:** Decadência arcana, mortos-vivos, mistério crescente  
**Tema mecânico:** Informação prévia simplifica o boss; flags de andar se combinam em revelação de identidade

### Andar 1 — O Térreo

**Inimigos:** Esqueletos básicos  
**Objetivo Principal:** Avançar pelo andar.  
**Objetivo Secundário (SIM):** Encontrar e examinar grimório antes de destruí-lo → `[grimorio_encontrado]`  
- Benefício em Andar 4: boss com **-10% HP**  
- Expirado se destruído antes de ser examinado  

**Colecionável:** `moeda_arcana` (Economia) — moeda de facção antiga; valor ao vender  
**Flags geradas:** `[grimorio_encontrado]`

---

### Andar 2 — Os Esqueletos

**Inimigos:** Esqueletos armados  
**Objetivo Principal:** Derrotar os esqueletos.  
**Objetivo Secundário (SIM):** Destruir o altar antes de avançar → `[altar_destruido]`  
- Efeito no Andar 3: impede ressurgimento de mortos-vivos durante o combate  

**Colecionável:** `amuleto_de_osso` (Build) — +5% resistência física, sem tradeoff  
**Flags geradas:** `[altar_destruido]`

---

### Andar 3 — Os Zumbis

**Inimigos:** Zumbis (sem ressurgimento se `[altar_destruido]`)  
**Objetivo Principal:** Atravessar o andar.  
**Objetivo Secundário (NÃO — 30%):** Andar de tensão pré-boss.  

**Colecionável:** `diario_rasgado` (Arquivo) — páginas parciais; sozinho não revela nada  
**Flag composta (com Andar 1):** `[diario_rasgado]` + `[grimorio_encontrado]` → `[identidade_revelada]`  
- Efeito em Andar 4: bônus adicional **-5% HP** no boss  

**Flags geradas:** `[diario_rasgado]`  
**Flags compostas:** `[identidade_revelada]`

---

### Andar 4 — Boss: O Carniçal

**Boss:** Carniçal (mago corrompido)  
**Estados:** `bossDerrotado` / `bossFugiu`

**Modificadores por flags:**

| Flag | Efeito |
|------|--------|
| `[grimorio_encontrado]` | -10% HP |
| `[identidade_revelada]` | -5% HP adicional |

**Bônus máximo acumulado:** -15% HP (calibrado para andar 1–5)

**Estado `bossFugiu`:** Carniçal retorna em **arco futuro (Andar 20+)** como "Carniçal Rancoroso" com +15% stats base.

**Colecionável:** `anel_do_mago` (Build) — gerado apenas com `bossDerrotado`; +8% dano mágico, sem tradeoff  
**Objetivo Secundário:** Nenhum (boss floor)

### Tracking JSON — Arco 1

```json
{
  "arco": 1,
  "andares": {
    "1": { "flags": ["grimorio_encontrado"], "colecionaveis": ["moeda_arcana"] },
    "2": { "flags": ["altar_destruido"], "colecionaveis": ["amuleto_de_osso"] },
    "3": { "flags": ["diario_rasgado"], "colecionaveis": ["diario_rasgado"] },
    "4": { "estadoBoss": "bossDerrotado", "colecionaveis": ["anel_do_mago"] }
  },
  "flagsCompostas": ["identidade_revelada"]
}
```

---

## Arco 2 — A Praga Ardente

**Andares:** 5–10  
**Tom:** Contaminação, caos subterrâneo, criaturas racionais como aliados inesperados  
**Tema mecânico:** Flags de investigação abrem rotas alternativas; objetivo principal do Andar 7 afeta o boss diretamente

### Andar 5 — Entrada das Minas

**Inimigos:** Kobolds contaminados  
**Objetivo Principal:** Derrotar os inimigos.  
**Objetivo Secundário (SIM):** Investigar os corpos antes de avançar → `[causa_investigada]`  
- Efeito no Andar 8: desbloqueia diálogo com kobold sobrevivente  

**Colecionável:** Nenhum  
**Flags geradas:** `[causa_investigada]`

---

### Andar 6 — O Refeitório

**Inimigos:** Kobolds e trabalhadores infectados  
**Objetivo Principal:** Limpar o andar.  
**Objetivo Secundário (SIM):** Resgatar sobrevivente durante o combate → `[sobrevivente_resgatado]`  
- Efeito pós-arco: NPC permanente no hub (informação, serviços menores)  

**Colecionável:** Nenhum  
**Flags geradas:** `[sobrevivente_resgatado]`

---

### Andar 7 — A Despensa

**Inimigos:** Guardiões da fonte  
**Objetivo Principal (crítico):** Destruir a fonte de contaminação → `[fonte_destruida]`  
- Efeito no Andar 8: reduz Exaustão do grupo durante o combate  
- Efeito no Andar 10 (boss): boss com **-15% HP**  

**Objetivo Secundário (NÃO — 30%):** Andar de objetivo principal crítico; sem secundário para manter foco.  

**Colecionável:** `frasco_agua_pura` (Chave) — usável no boss fight do Andar 10 para cancelar uma mecânica de veneno  
**Flags geradas:** `[fonte_destruida]`

---

### Andar 8 — A Câmara Central

**Inimigos:** Kobolds líderes  
**Objetivo Principal:** Avançar pela câmara.  
**Objetivo Secundário (SIM, condicional):** Disponível apenas se `[causa_investigada]` → diálogo com kobold → `[contexto_obtido]` + `[mapa_rabiscado]`  
- Sem `[causa_investigada]`: kobold é hostil, sem diálogo possível  

**Flag composta:** `[contexto_obtido]` + `[mapa_rabiscado]` → `[rota_alternativa]`  
- Efeito no Andar 9: acesso a rota secundária que evita armadilhas principais  

**Colecionável:** Nenhum  
**Flags geradas:** `[contexto_obtido]`, `[mapa_rabiscado]` (condicionais)  
**Flags compostas:** `[rota_alternativa]`

---

### Andar 9 — O Poço

**Inimigos:** Criaturas aquáticas contaminadas  
**Objetivo Principal:** Atravessar.  
**Objetivo Secundário (NÃO — 30%):** Andar de tensão pré-boss.  

**Colecionável:** `pedra_mana_contaminada` (Build) — tradeoff: **+12% dano mágico**, mas causa **+1 Fadiga** após uso; sempre disponível  
**Flags geradas:** Nenhuma

---

### Andar 10 — Boss: Jakk

**Boss:** Jakk (líder kobold corrompido)  
**Estados:** `bossDerrotado` / `bossFugiu`

**Mecânica de boss:** Invoca zumbis por turno até o altar do andar ser destruído.

**Modificadores por flags:**

| Flag | Efeito |
|------|--------|
| `[fonte_destruida]` | -15% HP |
| `frasco_agua_pura` (usado) | Cancela mecânica de veneno em área uma vez |

**Estado `bossFugiu`:** Jakk retorna no **Andar 17** como "Jakk, o Persistente" com +15% stats + habilidade de veneno em área (nova).

**Colecionável:** `selo_de_jakk` (Arquivo) — combina com item futuro para revelar afiliação de Jakk  
**Objetivo Secundário:** Nenhum (boss floor)

### Tracking JSON — Arco 2

```json
{
  "arco": 2,
  "andares": {
    "5": { "flags": ["causa_investigada"], "colecionaveis": [] },
    "6": { "flags": ["sobrevivente_resgatado"], "colecionaveis": [] },
    "7": { "flags": ["fonte_destruida"], "colecionaveis": ["frasco_agua_pura"] },
    "8": { "flags": ["contexto_obtido", "mapa_rabiscado"], "colecionaveis": [] },
    "9": { "flags": [], "colecionaveis": ["pedra_mana_contaminada"] },
    "10": { "estadoBoss": "bossDerrotado", "colecionaveis": ["selo_de_jakk"] }
  },
  "flagsCompostas": ["rota_alternativa"]
}
```

---

## Arco 3 — A Cabana dos Experimentos

**Andares:** 11–15  
**Tom:** Estranhamento cômico-sombrio — magia desgovernada, objetos com vontade própria, calor que não deveria existir  
**Tema mecânico:** Adaptação forçada. O boss absorve fogo — grupo não informado terá desvantagem severa. Flags de andares anteriores determinam se o grupo chega preparado.

### Andar 11 — A Sala dos Objetos

**Inimigos:** Livro Animado + Atiçador Animado + 2 Cordões de Cortina Animados (constructs)  
**Objetivo Principal:** Neutralizar os objetos animados.  
**Objetivo Secundário (SIM):** Examinar o livro aberto *antes* de destruí-lo → `[grimorio_golem_lido]`  
- Exige ação deliberada durante ou antes do combate  
- Expirado se o livro for destruído sem inspeção  
- Benefício no Andar 15: boss com **-15% HP**  

**Colecionável:** `fragmento_livro_arcano` (Lore) — notas sobre imunidades de constructs; gerado apenas com `[grimorio_golem_lido]`  
**Flags geradas:** `[grimorio_golem_lido]`, `[objetos_destruidos]`

---

### Andar 12 — A Sala de Jantar

**Inimigos:** Nenhum (exploration floor)  
**Objetivo Principal:** Atravessar e rastrear sinais do incidente.  
**Objetivo Secundário (SIM):** Preservar os objetos de prata sem quebrá-los → `[prata_preservada]`  
- Falha se o grupo usar AoE descuidado ou vasculhar brutalmente  
- Necessário para flag composta `[andolyn_aliada]`  

**Colecionável:** `talheres_de_prata` (Economia) — 12 peças; valor em ouro ao vender  
**Flags geradas:** `[prata_preservada]` *ou* `[prata_destruida]` (mutuamente exclusivas)

---

### Andar 13 — O Quarto

**Inimigos:** 3 Diabretes (servos de Woganpuck)  
**Objetivo Principal:** Neutralizar os diabretes.  
**Objetivo Secundário (SIM):** Estabilizar o prisioneiro durante/após o combate → `[gendrew_resgatado]`  
- Prisioneiro está inconsciente e intoxicado  
- Com `[gendrew_resgatado]`: informa que golem não deve ser atacado com fogo + localização da Caixa de Poções  
- Sem resgate: Andolyn chega hostil → sem recompensa de poções  

**Colecionáveis:**
- `caixa_de_pocoes` (Chave) — 6 poções; desbloqueado **somente** com `[gendrew_resgatado]`
- `nota_do_diabrete` (Arquivo) — referência a Woganpuck; gerada apenas se diabretes são derrotados sem fugir

**Flags geradas:** `[gendrew_resgatado]`, `[diabretes_derrotados]` ou `[diabretes_fugiram]`, `[woganpuck_revelado]` (condicional)

---

### Andar 14 — A Cozinha

**Inimigos:** 2 Mephits de Lava (ocultos no fogão)  
**Objetivo Principal:** Investigar a cozinha e preparar descida.  
**Objetivo Secundário (NÃO — 30%):** Andar de tensão pré-boss.  

**Mecânica especial:**
- Sem `[gendrew_resgatado]`: mephits atacam ao se aproximar do fogão  
- Com `[gendrew_resgatado]`: mencionar o nome do prisioneiro acalma os mephits → eles revelam que o boss **absorve fogo** → `[fraqueza_confirmada]`
  - Se grupo já tem `[grimorio_golem_lido]`: confirmação duplicada, sem bônus adicional
  - Se grupo não tem `[grimorio_golem_lido]`: `[fraqueza_confirmada]` concede **-5% HP** (chegaram à informação tarde)

**Colecionável:** Nenhum  
**Flags geradas:** `[mephits_pacificados]` ou `[mephits_destruidos]`, `[fraqueza_confirmada]` (condicional)

---

### Andar 15 — Boss: Golem de Calzone

**Boss:** Golem de Calzone (construct de massa e crosta)  
**Estados:** `bossDerrotado` / `bossFugiu`

**Mecânica de boss — 2 fases:**
- Fase 1 (HP > 30%): pancada dupla + dano de calor por toque + jato fervente (AoE ao receber slashing/piercing)
- Fase 2 (HP ≤ 30%): gatilho berserk — 1d6/turno; no 6, entra em fúria total até ser destruído ou curado

**Mecânica central — Imunidade a Fogo:**  
Ataques de fogo *curam* o golem proporcionalmente e aumentam sua CA. Grupo sem informação descobre isso ao primeiro acerto de fogo.

**Estado `bossFugiu`:** Andolyn se teletransporta (grupo em desvantagem severa) e usa Persuasão para conter o golem. Golem não é destruído → aparece no **Andar 22** como "Golem de Calzone: Restaurado" com +20% HP + habilidade Jato Concentrado (AoE expandido).

**Modificadores por flags:**

| Flag | Efeito |
|------|--------|
| `[grimorio_golem_lido]` | -15% HP |
| `[fraqueza_confirmada]` | -5% HP adicional (apenas se sem grimório) |
| `[gendrew_resgatado]` | Grupo não é surpreendido pelo gatilho berserk — aviso prévio |
| `[mephits_pacificados]` | Mephits atacam o golem com fogo (cauam 0 dano por absorção) — revela a mecânica ao vivo antes que o grupo cometa o erro |

**Bônus máximo acumulado:** -15% HP (calibrado para andares 11–15)

**Colecionáveis:**
- `receita_do_golem` (Arquivo) — notas do experimento; gerada automaticamente após resolução
- `frasco_molho_fervente` (Build) — apenas com `bossDerrotado`; **+8% dano físico em um ataque**, mas **+1 Fadiga** ao usuário (tradeoff item)

**Objetivo Secundário:** Nenhum (boss floor)

### Flags Compostas — Arco 3

| Composta | Componentes | Efeito |
|----------|-------------|--------|
| `[andolyn_aliada]` | `[gendrew_resgatado]` + `[prata_preservada]` | Andolyn vira NPC permanente no hub: identificação de itens + 1 magia gratuita/semana |
| `[woganpuck_rastreado]` | `[woganpuck_revelado]` + flag de arco futuro onde Woganpuck reaparecer | Ativa rota alternativa de confronto em arco posterior |

### Tracking JSON — Arco 3

```json
{
  "arco": 3,
  "andares": {
    "11": { "flags": ["grimorio_golem_lido", "objetos_destruidos"], "colecionaveis": ["fragmento_livro_arcano"] },
    "12": { "flags": ["prata_preservada"], "colecionaveis": ["talheres_de_prata"] },
    "13": { "flags": ["gendrew_resgatado", "diabretes_derrotados", "woganpuck_revelado"], "colecionaveis": ["caixa_de_pocoes", "nota_do_diabrete"] },
    "14": { "flags": ["mephits_pacificados", "fraqueza_confirmada"], "colecionaveis": [] },
    "15": { "estadoBoss": "bossDerrotado", "colecionaveis": ["receita_do_golem", "frasco_molho_fervente"] }
  },
  "flagsCompostas": ["andolyn_aliada", "woganpuck_rastreado"]
}
```

### Camada de Display — Arco 3

> **Andar 11** — *Uma sala que cheira a madeira velha e pólvora. Livros que voam. Cordas que apertam. Algo aqui não quer que você passe.*

> **Andar 12** — *Silêncio. Mesa posta para dois. Velas frias. Prata reluzindo como se esperasse alguém que não veio.*

> **Andar 13** — *Asas de morcego. Risos baixos. Um homem amarrado na cama, quase morto, quase respirando. Ainda há tempo.*

> **Andar 14** — *O fogão está quente. Algo se move lá dentro. O ar fede a enxofre e a farinha queimada.*

> **Andar 15** — *No porão: uma criatura de massa e crosta. Seus punhos fumegam. Você sente o calor antes de vê-la.*

---

## Notas de Balanceamento

### Tradeoff Items (Build)
Itens com custo embutido são consistentes entre arcos:
- `pedra_mana_contaminada` (Arco 2, Andar 9): +12% dano mágico / +1 Fadiga por uso
- `frasco_molho_fervente` (Arco 3, Andar 15): +8% dano físico / +1 Fadiga por uso

### Woganpuck como Villain Seed
`[woganpuck_revelado]` (Arco 3, Andar 13) é a primeira menção a um antagonista externo recorrente. Não tem efeito imediato — o flag composto `[woganpuck_rastreado]` só ativa quando ele reaparecer em arco futuro. Mantém o princípio de que flags inter-arco não dão bônus antes do tempo.

### Bosses que Fogem
Bosses que fogem (`bossFugiu`) retornam com stats aumentados e nova mecânica:
- Carniçal (Arco 1) → Carniçal Rancoroso: Andar 20+ / +15% stats
- Jakk (Arco 2) → Jakk, o Persistente: Andar 17 / +15% stats + veneno em área
- Golem de Calzone (Arco 3) → Golem: Restaurado: Andar 22 / +20% stats + Jato Concentrado

---

*Próximo arco a adaptar: Arco 4 (Andares 16+) — baseado nas aventuras disponíveis.*
