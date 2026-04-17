# Legends Awaken — Análise do Projeto

**Repositório:** https://github.com/Gabrieljsa21/LegendsAwaken  
**Data da análise:** 2026-04-10 *(atualizada em 2026-04-11)*

---

## Visão Geral

Legends Awaken é um **bot RPG para Discord** escrito em C#. Os jogadores assumem o papel de "Mestre" e interagem com o bot via slash commands para invocar heróis por um sistema de gacha, gerenciar parties, treinar heróis e escalar uma torre infinita de combate. Todo o estado do jogo é persistido por usuário em um banco SQLite local.

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
| Testes | xUnit |

---

## Arquitetura e Estrutura de Pastas

O projeto segue **Clean Architecture com influência de DDD**, organizado em 6 projetos .NET dentro de uma única solution (`LegendsAwaken.sln`). Veja `Estrutura.md` para o mapa de arquivos atual.

A arquitetura está corretamente aplicada: Domain não tem dependências externas, Application depende apenas das interfaces do Domain, Infrastructure depende de ambos, e Bot depende do Application. O uso de interfaces de repositório no Domain e implementações na Infrastructure respeita corretamente o Princípio da Inversão de Dependência.

---

## Pontos Fortes

- Clean Architecture genuinamente aplicada com separação real de camadas em 6 projetos
- Repository Pattern implementado com interfaces no Domain e concretos EF Core na Infrastructure
- `GachaService` implementa soft-pity com progressão em curva cúbica — feature não-trivial e tecnicamente precisa
- `RaridadeConfig` (record imutável) centraliza cap, stats base e ganhos por level em um único lugar — zero números mágicos no sistema de progressão (SOLID)
- Distribuição de raças por raridade: 1★/2★ = sempre humano; 3★ = 10% não-humano; 4★ = 25% não-humano; uniforme dentro do pool não-humano
- Grant de ascensão catch-up: ao ascender, o herói recebe exatamente os pontos que faltam para equiparar a um nativo da nova raridade no mesmo nível — garante que um 1★ upado ao 5★ lv100 seja igual a um 5★ nativo
- `TorreService` usa um padrão de tipo de andar limpo (Subjugacao/Fuga/Escolta/Defesa/Armadilha/Evento em múltiplos de 5/10/25) — claro e escalável
- Token do bot armazenado como variável de ambiente (`LEGENDSAWAKEN_TOKEN`) — boa prática de segurança
- `CidadeRepository` reescrito em EF Core; produção passiva com cap de 24h funcional
- 8 slash commands com autocomplete, paginação, dropdowns e botões

---

## Problemas Encontrados

### Alta Prioridade

| Problema | Status | Detalhe | Correção Recomendada |
|---|---|---|---|
| Caminho absoluto no appsettings.json | **Pendente** | `"Data Source=E:\\..."` quebra em qualquer outra máquina | Usar `./legendsawaken.db` ou variável de ambiente |
| `Random` não é thread-safe | **Pendente** | `GachaService` usa `new Random()` — não é thread-safe com interações Discord concorrentes | Substituir por `Random.Shared` (.NET 6+) |
| Testes unitários vazios | **Pendente** | `UnitTest1.cs` contém apenas um `[Fact]` vazio | Adicionar testes para gacha, progressão e cidade |

### Média Prioridade

| Problema | Status | Detalhe | Correção Recomendada |
|---|---|---|---|
| `Console.WriteLine` em serviços de produção | **Pendente** | `GachaService` e outros usam `Console.WriteLine` para debug | Substituir por `ILogger<T>` injetado |
| Guild ID hardcoded | **Pendente** | `Program.cs` registra comandos a um único server ID fixo | Mover para `appsettings.json` / env var |
| Lógica de habilidades incompleta | **Pendente** | Em `CombatService`, ambos os branches chamam o mesmo cálculo de dano físico | Implementar handlers específicos para habilidades |
| Comando `treinar` é stub | **Pendente** | Integração com `TreinamentoService` não implementada | Completar implementação |

### Baixa Prioridade

| Problema | Status | Detalhe | Correção Recomendada |
|---|---|---|---|
| Seleção de alvo no combate | **Pendente** | Sempre ataca o primeiro inimigo | Implementar IA tática (menor HP, maior ameaça, etc.) |

---

## Features Notáveis

- **Sistema de gacha** com banners configuráveis, tiers de raridade e mecanismo de soft-pity com curva cúbica
- **Torre infinita** com bosses escalonados (andares 5/10/25), tipos de andar variados e estado de progressão por usuário
- **Sistema de atributos** com base por raridade (derivado de `RaridadeConfig`) + bônus racial (+50 no atributo foco por raça não-humana) + progressão por level-up com fase de superação
- **Sistema de progressão** com caps 20/40/60/80/100 por raridade e grant de catch-up na ascensão
- **Sistema de party** (até 5 heróis) com criação, visualização e gerenciamento via slash commands
- **Sistema de cidade** com produção passiva por profissão, alocação/desalocação de heróis, coleta com cap de 24h
- **Listagem paginada** de heróis com navegação por botões Discord (25 por página)
- **Autocomplete** para nomes de heróis e banners nos slash commands
- **Seed data em JSON** mantendo conteúdo estático fora do banco de dados

---

## Avaliação Geral

Legends Awaken é um projeto pessoal **bem arquitetado e em desenvolvimento ativo**. O autor demonstra sólido entendimento de Clean Architecture, DDD, EF Core e Discord.Net. As escolhas arquiteturais são genuinamente boas — isolamento correto de camadas, abstração de repositório, injeção de dependência, owned entities, SOLID (RaridadeConfig sem números mágicos) e segredos via variável de ambiente estão todos corretamente implementados.

Os gaps principais são os esperados de um projeto solo em alpha: features em stub, suite de testes vazia, alguns valores hardcoded que criam problemas de portabilidade. Nenhum é um problema arquitetural — são itens de polish diretos de corrigir, todos rastreados no `TODO.md`.
