# Legends Awaken — Análise do Projeto

**Repositório:** https://github.com/Gabrieljsa21/LegendsAwaken  
**Data da análise:** 2026-04-10 *(atualizada em 2026-04-18 — Fase 3A.3 concluída)* *(atualizada em 2026-04-25 — Sessão Bioma Panel + Torre Op v2)*

---

## Visão Geral

Legends Awaken é um **bot RPG para Discord** escrito em C#. Os jogadores assumem o papel de "Mestre" e interagem com o bot via slash commands para coletar heróis de forma determinística (por fragmentos, marcos e contratos), gerenciar parties, escalar uma torre infinita e administrar uma cidade que cresce com a coleção. Todo o estado do jogo é persistido por usuário em um banco SQLite local.

O sistema de gacha original foi eliminado na Fase 3A.3 e substituído por aquisição 100% determinística — o jogador sempre sabe exatamente o que precisa para obter cada herói.

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

## Arquitetura e Estrutura de Pastas

O projeto segue **Clean Architecture com influência de DDD**, organizado em 6 projetos .NET dentro de uma única solution (`LegendsAwaken.sln`). Veja `Estrutura.md` para o mapa de arquivos atual.

A arquitetura está corretamente aplicada: Domain não tem dependências externas, Application depende apenas das interfaces do Domain, Infrastructure depende de ambos, e Bot depende do Application. O uso de interfaces de repositório no Domain e implementações na Infrastructure respeita corretamente o Princípio da Inversão de Dependência.

---

## Pontos Fortes

- Clean Architecture genuinamente aplicada com separação real de camadas em 6 projetos
- Repository Pattern implementado com interfaces no Domain e concretos EF Core na Infrastructure
- `RaridadeConfig` (record imutável) centraliza cap, stats base e ganhos por level em um único lugar — zero números mágicos no sistema de progressão (SOLID)
- Sistema de fragmentos determinístico: nenhum RNG de raridade; cada herói tem um caminho explícito de desbloqueio
- Drop de fragmentos ponderado por bioma com multiplicador de contrato — `FragmentService.SelecionarPorPeso` com guard `totalPeso <= 0`
- 5 serviços novos bem delimitados com responsabilidade única (BiomeService, FragmentService, RecruitmentService, ContractService, RewardDistributionService)
- Partial unique indexes no banco: uma entrada de progresso por `(UsuarioId, HeroiId)` e um contrato ativo por tipo por usuário — invariantes de negócio garantidos no nível do DB
- `UpsertAsync` de fragmentos com retry em `DbUpdateException` — concorrência Discord corretamente tratada
- Distribuição de raças por raridade uniforme dentro do pool não-humano — geração procedural continua funcionando
- Grant de ascensão catch-up: ao ascender, o herói recebe exatamente os pontos que faltam para equiparar a um nativo da nova raridade no mesmo nível
- `TorreService` usa padrão de tipo de andar limpo (Subjugacao/Fuga/Escolta/Defesa/Armadilha/Evento em múltiplos de 5/10/25) — claro e escalável
- Token do bot armazenado como variável de ambiente (`LEGENDSAWAKEN_TOKEN`) — boa prática de segurança
- `DiscordIdHelper.ToGuid` usa `BinaryPrimitives.WriteUInt64LittleEndian` — determinístico e platform-safe
- `Random.Shared` usado consistentemente em toda a codebase — thread-safe em chamadas Discord concorrentes
- 39 testes unitários cobrindo os 5 novos serviços do sistema de fragmentos e a extensão do TorreService
- Primary constructor injection em todos os novos serviços e comandos bot — C# idiomático
- `ResourceNodeConfig.Icone(string)` — método único de ícone de recurso elimina switches duplicados; fácil de evoluir
- `TorreOperacaoConfig` — config estática imutável centraliza duração, produção por tier e capacidade de slots; nenhum número mágico no sistema de operação
- Sistema de descoberta de heróis no Bioma: heróis secundários ficam ocultos até o primeiro fragmento coletado — revelação progressiva coerente com a progressão do jogador
- `BiomaPanel` com seletor de biomas descobertos e barra de progresso por andares — UX de progressão clara sem consulta manual

---

## Problemas Encontrados

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

---

## Features Notáveis

- **Sistema de fragmentos** — aquisição determinística de heróis: drops ponderados por bioma, 3 caminhos de unlock (fragmentos/marco/condição), contratos de multiplicador (+30%/+50%)
- **Painéis Discord interativos** — `/colecao`, `/bioma`, `/contrato` com select menus e botões; `SelectMenuExecuted` e `ButtonExecuted` corretamente separados
- **Torre infinita** com bosses escalonados (andares 5/10/25), tipos de andar variados, progressão de estado por usuário e agora integração completa com o sistema de fragmentos
- **Sistema de atributos** com base por raridade (derivado de `RaridadeConfig`) + bônus racial (+50 no atributo foco) + progressão por level-up com fase de superação
- **Sistema de progressão** com caps 20/40/60/80/100 por raridade e grant de catch-up na ascensão
- **Sistema de party** (até 5 heróis) com criação, visualização e gerenciamento via slash commands
- **Sistema de cidade** com dois tiers de produção — ResourceNodes (sem slot) e Prédios (Responsabilidade + Operação), Humor da Cidade como multiplicador
- **Crafting com check de qualidade** via Responsável da Forja (skill + prédio + dice roll)
- **Listagem paginada** de heróis com navegação por botões Discord (25 por página)
- **Autocomplete** para nomes de heróis nos slash commands
- **Seed data em JSON** mantendo conteúdo estático fora do banco de dados

---

## Avaliação Geral

Legends Awaken é um projeto pessoal **bem arquitetado e em desenvolvimento ativo**. O autor demonstra sólido entendimento de Clean Architecture, DDD, EF Core e Discord.Net, com entregas consistentes e incrementais em múltiplas fases.

A decisão de substituir gacha por fragmentos na Fase 3A.3 foi tecnicamente bem executada: Big Bang controlado (serviços antigos removidos, novos criados do zero), invariantes de negócio garantidos no banco, concorrência Discord tratada com retry pattern, e 39 testes cobrindo os novos caminhos críticos.

Os gaps principais são os esperados de um projeto solo em alpha: features em stub, cobertura parcial de testes, alguns valores hardcoded que criam problemas de portabilidade. Nenhum é um problema arquitetural — são itens de polish rastreados no `TODO.md`.
