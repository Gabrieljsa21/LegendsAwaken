# Changelog — Legends Awaken

Todas as alterações significativas neste projeto serão documentadas neste arquivo.

O formato segue o padrão [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/), e o versionamento segue [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [3.0.0] - 2026-04-18 · Fase 3A.3 — Sistema de Fragmentos

> **Breaking change:** sistema de gacha removido integralmente. Aquisição de heróis agora é 100% determinística.

### Removido
- `GachaService` e `BannerService` — sistema de gacha eliminado
- Entidades `Banner`, `BannerHeroiPool` e DTO `GachaResultadoDTO`
- Tabelas `Banners` e `BannerHeroiPools` removidas do banco

### Adicionado
**Domínio**
- Entidades: `HeroiConfig`, `HeroiUnlockConfig`, `Bioma`, `BiomHeroPool`, `FragmentoProgresso`, `Contrato`, `HeroiDesbloqueado`
- Enums: `TipoFragmento`, `TipoUnlock`, `TipoContrato`, `TipoEventoAlto`, `TipoReward`
- Interfaces de repositório: `IHeroiConfigRepository`, `IHeroiDesbloqueadoRepository`, `IFragmentoRepository`, `IBiomaRepository`, `IContratoRepository`

**Application**
- `BiomeService` — mapeamento andar→bioma; detecção de bioma novo e marco da Torre
- `FragmentService` — drops ponderados por bioma com multiplicador de contrato; upsert TOCTOU-safe
- `RecruitmentService` — 3 caminhos de desbloqueio: fragmentos, marco da Torre, condição única
- `ContractService` — contratos arquétipo (+30%) e nomeado (+50%); expiração automática
- `RewardDistributionService` — factory de payloads Micro / Médio / Alto por tipo de evento
- DTOs: `ContractConfig`, `FragmentDropResult`, `RecruitmentResult`, `RewardPayload`

**Infrastructure**
- Repositórios EF Core para os 5 novos contratos de repositório
- Migration `FragmentoSystem` — 7 novas tabelas
- Migration `FragmentoSystemIndexes` — partial unique indexes em `FragmentosProgresso` e `Contratos`
- Seed: 9 heróis com `HeroiUnlockConfig`; 5 biomas com pools de drop ponderados

**Bot**
- `DiscordIdHelper.ToGuid(ulong)` — conversão determinística Discord ID → Guid (little-endian via `BinaryPrimitives`)
- Painéis: `ColecaoPanel`, `BiomaPanel`, `ContratoPanel`
- Comandos: `/colecao`, `/bioma`, `/contrato`
- `SelectMenuExecuted` wired no `CommandHandler` (antes apenas `ButtonExecuted` era subscrito)

### Alterado
- `TorreService.SubirAndarAsync` — estendido com drop de fragmentos, detecção de bioma novo e desbloqueio de herói por marco
- `SubirAndarResult` — ampliado de 4 para 8 campos: `Fragmentos`, `NovoBioma`, `HeroiDesbloqueado`, `RewardPayloads`
- `CommandHandler` — 8 novos campos injetados; 3 novos slash commands; handlers para `colecao_recrutar`, `contrato_arquetipo`, `bioma_ver_colecao`, `bioma_contratos`, `contrato_remover_nomeado`; logging estruturado
- `TipoReward` movido de `Application/DTOs` para `Domain/Enum/Enums.cs`

### Testes
- 39 testes unitários: `BiomeServiceTests`, `FragmentServiceTests`, `ContractServiceTests`, `RecruitmentServiceTests`, `TorreServiceExtensionTests`

---

## [2.0.0] - 2026-04-16 · Fase 3A — Loop Jogável + Consolidação do Core

> Implementação em um único commit de grande porte cobrindo Fase 3A.1 (vertical slice) e Fase 3A.2 (consolidação). Build: 0 warnings, 0 errors.

### Adicionado

**XP e Progressão**
- `RaridadeConfig.BaseXp` — curva de XP linear `XP_next = B_r × nível` por raridade (80/100/120/150/200)
- Stats base por raridade via `ObterAtributosBaseParaRaridade` aplicados na criação do herói
- Bônus racial +50 no atributo foco aplicado via `HeroiLevelUpService.BonusRacial`
- Level-up com distribuição de pontos e verificação de cap por raridade
- Bloqueio de XP ao atingir o cap (XP zerado, level travado até ascensão)
- XP e Ouro concedidos ao limpar andar da Torre

**Combate**
- Fórmula de dano: `ATK × SkillMult × (1 - DEF/(DEF+1000+Level×50)) × TypeMult`
- Crítico: 1.5× de dano
- Burst cap: hit único ≤ 65% HP máximo do alvo
- Ordem de turno ATB: `InitScore = Agilidade + Random(0, Agilidade×0.1)`

**Crafting e Equipamentos**
- 5 receitas estáticas: espada-ferro, arco-simples, armadura-couro, anel-arcano, amuleto-agilidade
- Check de qualidade: `skill_craft + bônus_prédio(Nível×2) + roll(1..20)` via Responsável da Forja
- `/crafting listar`, `/crafting fazer <receitaId>`, `/heroi_equipar`
- `HeroiBonusAtributo` — bônus de equipamento persistido separadamente dos atributos base

**Cidade — Modelo de Slots**
- `SlotOcupacao` entity + `ISlotOcupacaoRepository` + migration `CidadeSlotModel3A2`
- Campos `Confianca` (0–100) e `Humor` (0–100) na entidade `Heroi`
- ResourceNode (Campo/Floresta/Mina/Prado) — tier 1 de produção por profissão, sem slot
- Slots de Responsabilidade (gate por Confiança + atributo) e Operação por prédio
- Humor da Cidade = média dos heróis alocados × multiplicador 0.9/1.0/1.1/1.2
- Fórmula Tier 2: `BaseProd × MultResp × SomaOp × HumorMult × horas`
- `PredioConfig` e `ResourceNodeConfig` — configs estáticas sem hardcode
- `/cidade construir`, `/cidade alocar_recurso`, `/cidade alocar_predio`, `/cidade desalocar`

**Arena**
- `/treinar <heroi>` — XP em burst (3× XpParaProximoNivel), 4h cooldown, custo 100 Ouro + 10 Comida
- `/arena desafio` — desafio de ondas, cooldown 24h, top-5 heróis automático

**Personagens fixos**
- Campo `Lore` na entidade `Heroi`
- Seed de 9 personagens fixos 5★/4★ via `GeracaoDeDadosService` (idempotente)
- Ouro por andar da Torre: `5 + Numero×3` × boss_mult

### Alterado
- `/cidade ver` reworked — coletores com taxa/h, prédios com slots e heróis alocados, HumorCidade exibido

---

## [1.1.0] - 2026-04-10 · Migração para .NET 10

### Alterado
- Target framework atualizado de .NET 8 para .NET 10
- Todos os nullable warnings corrigidos (0 warnings, 0 errors no build)
- Dependências atualizadas para versões compatíveis com .NET 10

---

## [1.0.3] - 2025-07-24

### Adicionado
- Sistema de grupos implementado: agora é possível montar um grupo com até 5 heróis utilizando o comando `/grupo`.
- Comando `/listar_herois` aprimorado com sistema de paginação: exibe até 25 heróis por página com botões ⏮️ Anterior e ⏭️ Próximo para facilitar a navegação.

---

## [1.0.2] - 2025-07-24

### Adicionado
- Comando `/ver_heroi` aprimorado para aceitar nome do herói como parâmetro com autocomplete.
- Exibição detalhada das habilidades do herói, incluindo nomes e níveis, no embed do comando `/ver_heroi`.
- Cálculo dos bônus de atributos agora inclui corretamente os bônus das habilidades multiplicados pelo nível de cada habilidade.
- Criação e cadastro das habilidades iniciais do jogo para uso pelos heróis.

---

## [1.0.1] - 2025-07-20

### Adicionado
- Sistema de invocação com rolagens simples e múltiplas (x1, x11).
- Implementação da lógica de pity por banner com reinício imediato após obter herói 4★.
- Comando `/roll` atualizado para aceitar seleção via dropdown de banners disponíveis.
- Exibição de progresso do pity no embed do resultado de invocação.
- Ícone ✨ adicionado ao lado do nome de heróis 4★ no resultado das rolagens.

### Corrigido
- Reset do contador de pity agora ocorre corretamente após obtenção de herói 4★ (não ao fim da rolagem múltipla).
- Corrigido erro de interação falha ao selecionar banner no menu suspenso.

---

## [1.0.0] - 2025-07-16

### Adicionado
- Definição de escopo e objetivos do projeto.
- Estrutura modular baseada em Clean Architecture com influência de DDD.
- Camadas organizadas: `Domain`, `Application`, `Infrastructure`, `Bot`, `Data`, `Tests`.
- Configuração inicial de bot Discord com `Discord.Net`.
- Projeto `LegendsAwaken.sln` criado no Visual Studio 2022.
- Integração com SQLite via `Microsoft.EntityFrameworkCore.Sqlite`.
- Repositório público criado no GitHub com README detalhado.
- Estrutura de comandos básicos iniciada.
- Suporte a variáveis de ambiente para configuração de token do Discord.
- Base de dados com tabelas iniciais (`Heroi`, `Usuario`, etc.).
- Estrutura de arquivos `.json` no projeto `LegendsAwaken.Data`.

### Corrigido
- Ajustes em conflitos de DLL durante build (lock de arquivo).
- Correções na configuração do bot no Discord Developer Portal (comandos slash visíveis).
