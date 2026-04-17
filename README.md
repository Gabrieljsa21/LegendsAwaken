# Legends Awaken

Bot RPG para Discord escrito em C#. O jogador assume o papel de **Mestre** que invoca heróis via gacha, gerencia uma cidade e sobe andares de uma torre infinita.

---

## Funcionalidades

### Implementadas
- Sistema de gacha com soft-pity (curva cúbica) e banners configuráveis
- Geração procedural de heróis (raça, profissão, atributos, habilidades)
- Distribuição de raças por raridade (1★/2★ = humano; 3★ = 10% não-humano; 4★ = 25%)
- `RaridadeConfig` centralizado — caps, stats base e ganhos por nível sem números mágicos
- Torre infinita com tipos de andar variados e bosses escalonados (andares 5/10/25)
- Combate automático por turnos
- Party de até 5 heróis com gestão via slash commands
- Sistema de cidade: produção passiva por profissão, alocação/desalocação de heróis, coleta com cap de 24h
- Listagem e visualização de heróis com paginação e autocomplete

### Em construção (Fase 3A)
- `HeroiLevelUpService`: lógica de caps (20/40/60/80/100), ganhos por level-up e grant de catch-up na ascensão implementados — curva de XP e comandos de level-up pendentes (BLOQUEADOR P0)

### Planejadas (design no GDD)
- Aplicação de stats base por raridade e bônus raciais (+50 no atributo foco) na criação do herói
- Ascensão por fragmentos de arquétipo (1★ → 5★ para qualquer herói)
- Relíquias (drops de boss, 3 slots por herói, removíveis e transferíveis)
- Crafting com check de qualidade (Forja, Ateliê, Laboratório)
- Sistema de missões com Guilda de 15 ranks (Ferro → Oricalco)
- Arena: treino acelerado, desafios de ondas, ranking de prestígio
- Confiança e Humor dos heróis com política autônoma da cidade
- Apelidos e arte customizada por herói

---

## Stack

| Camada | Tecnologia |
|---|---|
| Linguagem | C# (.NET 10) |
| Discord | Discord.Net |
| ORM | Entity Framework Core 10 |
| Banco de dados | SQLite |
| DI | .NET built-in DI container |
| Testes | xUnit |

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
| `Tests` | Testes automatizados |

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

4. Crie o banco de dados:
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
| `GDD.md` | Game Design Document completo — sistemas, mecânicas, balanceamento |
| `ROADMAP.md` | Fases de desenvolvimento macro |
| `TODO.md` | Tarefas granulares por área |
| `Estrutura.md` | Estrutura de pastas do projeto |
