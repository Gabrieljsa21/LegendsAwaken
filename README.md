# Legends Awaken

Bot RPG para Discord escrito em C#. O jogador assume o papel de **Mestre** que coleciona heróis de forma determinística — por fragmentos, conquistas e contratos — gerencia uma cidade e escala uma torre infinita.

---

## Funcionalidades

### Implementadas

**Aquisição de Heróis (Sistema de Fragmentos)**
- Drops de fragmentos ao limpar andares da Torre (30% de chance, ponderados por bioma)
- 3 caminhos de desbloqueio: acúmulo de fragmentos, marcos da Torre, condição única
- Contratos de drop: arquétipo (+30% fragmentos da profissão) e nomeado (+50% de herói específico)
- `/colecao` — painel de coleção com progresso por herói e botão de recrutar
- `/bioma` — bioma atual com heróis disponíveis e pesos de drop
- `/contrato` — contratos ativos com select menu de arquétipo

**Heróis e Progressão**
- Geração procedural de heróis (raça, profissão, atributos, habilidades)
- `RaridadeConfig` centralizado — caps (20/40/60/80/100), stats base e ganhos por nível sem números mágicos
- Stats base por raridade e bônus racial (+50 no atributo foco) aplicados na criação
- Curva de XP: `XP_next = B_r × nível` por raridade
- Level-up com distribuição de pontos, verificação de cap e bloqueio ao atingir o teto
- Grant de catch-up na ascensão — herói ascendido fica idêntico a um nativo da nova raridade

**Torre**
- Torre infinita com 6 tipos de andar (Subjugação, Fuga, Escolta, Defesa, Armadilha, Evento)
- Bosses escalonados nos andares 5/10/25 com multiplicadores de XP e Ouro
- Ouro por andar: `5 + Numero×3` × boss_mult
- Drops de fragmentos integrados ao progresso da Torre; detecção automática de bioma novo e heróis de marco

**Combate**
- Combate automático por turnos com ATB (iniciativa por Agilidade)
- Fórmula: `ATK × SkillMult × (1 - DEF/(DEF+1000+Level×50)) × TypeMult`; crit 1.5×; burst cap 65%
- Party de até 5 heróis com gestão via slash commands

**Cidade**
- Produção passiva com cap de 24h
- ResourceNodes (Campo/Floresta/Mina/Prado) — tier 1 de produção por profissão
- Dois tipos de slot por prédio: Responsabilidade (gate por Confiança + atributo) e Operação
- Humor da Cidade = média dos heróis alocados × multiplicador de produção
- `/cidade ver`, `/cidade coletar`, `/cidade alocar_recurso`, `/cidade alocar_predio`, `/cidade construir`

**Crafting e Equipamentos**
- 5 receitas estáticas (espada, arco, armadura, anel, amuleto)
- Check de qualidade: `skill_craft + bônus_prédio + roll(1..20)` via Responsável da Forja
- `/crafting listar`, `/crafting fazer`, `/heroi_equipar`

**Arena**
- `/treinar` — XP em burst (3× XpParaProximoNivel), 4h cooldown, custo Ouro + Comida
- `/arena desafio` — desafio de ondas com cooldown 24h, top-5 heróis automático

**Testes**
- 39 testes unitários: BiomeService, FragmentService, ContractService, RecruitmentService, TorreService

---

## Stack

| Camada | Tecnologia |
|---|---|
| Linguagem | C# (.NET 10) |
| Discord | Discord.Net |
| ORM | Entity Framework Core 10 |
| Banco de dados | SQLite |
| DI | .NET built-in DI container |
| Testes | xUnit + Moq |

---

## Arquitetura

Clean Architecture + DDD organizado em 6 projetos:

| Projeto | Responsabilidade |
|---|---|
| `Domain` | Entidades, enums, interfaces de repositório, regras de negócio |
| `Application` | Serviços de aplicação, DTOs, casos de uso |
| `Infrastructure` | EF Core, repositórios, migrations, seed data |
| `Bot` | Slash commands, handlers Discord, entry point |
| `Data` | JSON estático (habilidades, etc.) |
| `Tests` | Testes automatizados (xUnit + Moq) |

---

## Como rodar

**Pré-requisitos:** .NET 10 SDK, token de bot do Discord.

1. Clone o repositório:
   ```bash
   git clone https://github.com/Gabrieljsa21/LegendsAwaken.git
   cd LegendsAwaken
   ```

2. Defina o token como variável de ambiente:
   ```bash
   # Windows
   set LEGENDSAWAKEN_TOKEN=seu_token_aqui

   # Linux / macOS
   export LEGENDSAWAKEN_TOKEN=seu_token_aqui
   ```

3. Restaure os pacotes:
   ```bash
   dotnet restore
   ```

4. Aplique as migrations:
   ```bash
   dotnet ef database update --project LegendsAwaken.Infrastructure --startup-project LegendsAwaken.Bot
   ```

5. Execute o bot:
   ```bash
   dotnet run --project LegendsAwaken.Bot
   ```

---

## Documentação

| Arquivo | Conteúdo |
|---|---|
| `GDD.md` | Game Design Document — sistemas, mecânicas, balanceamento |
| `DESIGN_SISTEMAS.md` | Frameworks matemáticos de todos os sistemas |
| `ROADMAP.md` | Fases de desenvolvimento macro |
| `TODO.md` | Tarefas granulares por área |
| `CHANGELOG.md` | Histórico de mudanças por versão |
| `docs/COMMANDS.md` | Referência completa dos 12 slash commands — parâmetros, valores, interações |
| `AI_INDEX.md` | Índice de navegação de código para AI assistants |
| `Estrutura.md` | Estrutura de pastas do projeto |
