# Legends Awaken — Arquitetura Técnica

> Visão técnica da stack, camadas e decisões de design. Para entidades/serviços/repositórios, veja `docs/AI_INDEX.md`.

---

## Stack e Dependências

| Camada | Tecnologia |
|---|---|
| Linguagem | C# (.NET 10) |
| Discord | Discord.Net |
| ORM | Entity Framework Core 10 |
| Banco de dados | SQLite |
| Injeção de Dependência | .NET built-in DI container |
| Configuração | appsettings.json + variáveis de ambiente |
| Testes | xUnit + Moq |

---

## Arquitetura

O projeto segue **Clean Architecture com influência de DDD**, organizado em 6 projetos .NET dentro de uma única solution (`LegendsAwaken.sln`):

| Projeto | Responsabilidade |
|---|---|
| `LegendsAwaken.Domain` | Entities, enums, interfaces, extensions, factories — sem dependências externas |
| `LegendsAwaken.Application` | Services, DTOs, helpers — depende apenas das interfaces do Domain |
| `LegendsAwaken.Infrastructure` | EF Core context, repositories, migrations, providers — depende de Domain e Application |
| `LegendsAwaken.Bot` | Discord entry point, CommandHandler, slash command classes — depende do Application |
| `LegendsAwaken.Data` | Static JSON seed data (classes, habilidades, herois_base) |
| `LegendsAwaken.Tests` | Unit tests — xUnit + Moq |

O fluxo de dependência respeita o Princípio de Inversão de Dependência: interfaces de repositório são definidas no Domain, implementações concretas residem na Infrastructure. Bot nunca acessa Infrastructure diretamente.

---

## Decisões de Design

- **Clean Architecture em 6 projetos distintos:** separação real de camadas, não apenas convencional; Domain não importa nenhum assembly externo.
- **Repository Pattern com interfaces no Domain:** `IHeroiRepository`, `ITorreRepository` etc. são contratos do Domain; EF Core fica isolado na Infrastructure.
- **`RaridadeConfig` como record imutável central:** todos os caps, stats base e ganhos por level derivam deste único ponto — nenhum número mágico no sistema de progressão (SOLID/OCP).
- **Aquisição de heróis 100% determinística:** sistema de fragmentos eliminou gacha na Fase 3A.3; cada herói tem caminho explícito de desbloqueio (fragmentos / marco de torre / condição única).
- **Drop de fragmentos ponderado por bioma com multiplicador de contrato:** `FragmentService.SelecionarPorPeso` com guard `totalPeso <= 0` — sem silêncio em pool vazio.
- **Partial unique indexes no banco:** uma entrada de progresso por `(UsuarioId, HeroiId)` e um contrato ativo por tipo por usuário — invariantes de negócio garantidos no nível do DB, não só na aplicação.
- **`UpsertAsync` com retry em `DbUpdateException`:** concorrência de chamadas Discord tratada explicitamente no repositório de fragmentos.
- **`DiscordIdHelper.ToGuid` via `BinaryPrimitives.WriteUInt64LittleEndian`:** conversão Discord `ulong` → `Guid` determinística e platform-safe.
- **`Random.Shared` em toda a codebase:** thread-safe para chamadas Discord concorrentes sem instâncias por-request.
- **Token do bot via variável de ambiente `LEGENDSAWAKEN_TOKEN`:** segredo nunca em appsettings.
- **`TorreOperacaoConfig` como config estática imutável:** duração, produção por tier e capacidade de slots centralizados; nenhum número mágico no sistema de operação.
- **`TorreArcoConfig` estende o padrão de config estática:** 3 arcos (Andares 1–15) com flags simples/compostas, objetivos e modificadores de boss em records imutáveis; `AndarFlagProgresso` rastreado em raw SQLite com PK composta `(UsuarioId, Andar, FlagNome)` — deliberadamente fora do EF Core DbContext, seguindo o padrão de `TorreExploracaoRepository`.
- **Objetivo secundário da Torre sempre tentado automaticamente (65% base):** elimina botão de escolha no bot Discord; simplifica fluxo async sem perda de agência narrativa.
- **`ResourceNodeConfig.Icone(string)` como método único:** elimina switches duplicados de ícone de recurso em múltiplos painéis.
- **Sistema de descoberta de heróis no Bioma:** heróis secundários ficam ocultos até o primeiro fragmento coletado — progressão por estado, não por query condicional.
- **Grant de ascensão catch-up:** ao ascender, o herói recebe exatamente os pontos que faltam para equiparar a um nativo da nova raridade no mesmo nível — consistência matemática garantida.
- **Distribuição de raças por raridade uniforme dentro do pool não-humano:** geração procedural de heróis mantém balanceamento sem tabelas hardcoded por raça.
- **Primary constructor injection em todos os novos serviços:** idiomático C# 12+, reduz boilerplate.
- **Delete-before-create no registro de slash commands:** garante que comandos removidos desaparecem do client Discord sem lixo acumulado.
- **`DateTimeStyles.RoundtripKind` no mapeamento de repos SQLite raw:** previne inflação de timezone UTC→local em leituras de `DateTime` persistidos como texto.

---

## Issues Técnicas Conhecidas

### Alta Prioridade

| Problema | Status | Detalhe | Correção Recomendada |
|---|---|---|---|
| Caminho absoluto no appsettings.json | **Pendente** | `"Data Source=E:\\..."` quebra em qualquer outra máquina | Usar `./legendsawaken.db` ou variável de ambiente |
| Guild ID hardcoded | **Pendente** | `Program.cs` registra comandos a um único server ID fixo | Mover para `appsettings.json` / env var |

### Média Prioridade

| Problema | Status | Detalhe | Correção Recomendada |
|---|---|---|---|
| `Console.WriteLine` em serviços legados | **Pendente** | Serviços anteriores à 3A.3 ainda usam `Console.WriteLine` | Substituir por `ILogger<T>` injetado (novos serviços já usam logging estruturado) |
| Lógica de habilidades incompleta | **Pendente** | Em `CombatService`, ambos os branches chamam o mesmo cálculo de dano físico | Implementar handlers específicos por tipo de habilidade |
| N+1 em `ProcessarMarcoTorreAsync` | **Pendente** | `ListarTodosAsync()` + N queries `ObterUnlockConfigAsync` por herói a cada claro de andar de marco | Adicionar `ListarHeroisDoMarcoAsync(andar)` com JOIN para eliminar N+1 |

### Baixa Prioridade

| Problema | Status | Detalhe | Correção Recomendada |
|---|---|---|---|
| Seleção de alvo no combate | **Pendente** | Sempre ataca o primeiro inimigo | Implementar IA tática (menor HP, maior ameaça) |
| Testes de `RewardDistributionService` | **Pendente** | Sem cobertura de teste | Adicionar testes unitários; lançar `ArgumentOutOfRangeException` no default arm |
| Teste probabilístico de fragmento | **Pendente** | Loop até 50 tentativas depende de `Random.Shared` | Injetar abstração de RNG para tornar testável de forma determinística |
