# Changelog — Legends Awaken

Todas as alterações significativas neste projeto serão documentadas neste arquivo.

O formato segue o padrão [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/), e o versionamento segue [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [3.8.0] - 2026-05-07 · Fase 3B-TorreArcos — Torre: Arcos Narrativos

### Adicionado

- `TorreArcoConfig` — config estático com 3 arcos (Andares 1–15): records `ArcoDefinicao`, `AndarArcoDefinicao`, `ObjetivoDefinicao`, `ColecionavelDefinicao`, `BossModificador`, `FlagCompostaDefinicao`; 4 flags compostas; helpers `ObterArcoPorAndar`, `ObterAndar`, `EBossFloor`
- `AndarFlagProgresso` entity + `AndarFlagProgressoRepository` — raw SQLite com PK `(UsuarioId, Andar, FlagNome)`; UPSERT idempotente via `ON CONFLICT DO UPDATE SET Gerada=1 WHERE Gerada=0`; tabela criada no startup por `GeracaoDeDadosService`
- `TorreFlagService` — geração de flags por andar (primárias sempre; secundária com 65% de probabilidade), avaliação e persistência de compostas, `ObterModificadoresBossAsync` com cap de 50% HP reduction
- `TorrePanel` — bloco narrativo de arco (nome + texto) e flags ativas do arco como parâmetros opcionais em `CriarEmbed`
- `TorreExploracaoPanel.CriarConfirmacao` — objetivo secundário (`AndarArcoDefinicao?`) exibido na confirmação de exploração com nota "será tentado automaticamente"
- Boss modifiers — `TorreExploracaoService.IniciarAsync` aplica HP reduction percentual com base em flags acumuladas no arco antes de iniciar combate (cap 50%); idempotência garantida pelo guard de duplo início existente
- Testes: `TorreArcoConfigTests` (4 testes — cobertura andares 1–15, boss floors, arco lookup, `EBossFloor`) + `TorreFlagServiceTests` (6 testes — geração de flags, compostas, expiração de secundária, modificadores de boss); **76 testes passando, 0 falhas**

### Alterado

- `TorreExploracaoService.ColetarAsync` — tipo de retorno alterado para `(TorreExploracao?, FlagsColetaResult)` com `FlagsGeradas`, `FlagsExpiradas`, `FlagsCompostas` do andar concluído
- `TorreCommand` — exibe seções de flags no embed de resultado de coleta; passa `AndarArcoDefinicao` e flags ativas para `TorrePanel`/`TorreExploracaoPanel`
- `CommandHandler` + `Program.cs` — `TorreFlagService` e `IAndarFlagProgressoRepository` registrados como Scoped; `CommandHandler` recebe `_torreFlagService`

---

## [3.7.0] - 2026-05-06 · Fase Q — Fundações de Qualidade

### Adicionado

- `HeroiGuard` — classe estática em `Application/Services/` com `ValidarDisponivel(Heroi)→string?` e `ValidarTodosDisponiveis(IEnumerable<Heroi>)→string?`; centraliza checagem de `Degradado`/`Inativo` antes de qualquer ação de combate
- **Testes unitários:** `HeroiLevelUpServiceTests` (8 testes — XP fórmula, caps por raridade, GanhoSuperação 5★, multiplicador racial Humano, grant de ascensão), `CombatServiceTests` (6 testes — burst cap, fórmula de mitigação, skillMult, mínimo 1, ExecutarRound), `CidadeServiceTests` (4 testes — produção após 2h, guard <1min, cap 24h, sem trabalhadores)
- **Integration test** `FragmentosRecrutarIntegrationTests` — fluxo completo fragmentos → recrutar com SQLite in-memory real (SQLite `Cache=Shared`, `Guid` por instância, `EnsureDeleted` no teardown)

### Alterado

- `GeracaoDeDadosService` — 3 `Console.WriteLine` substituídos por `ILogger<GeracaoDeDadosService>` (LogDebug/LogInformation com structured logging)
- `HeroiRepository` — 4 `Console.WriteLine` em blocos catch substituídos por `ILogger<HeroiRepository>` (`LogError(ex, ...)`)
- `Program.cs` — `GUILD_ID` hardcoded removido; lido de `configuration["Discord:GuildId"]` com throw contextualizado
- `appsettings.json` — DB path mudado de absoluto para relativo (`legendsawaken.db`); seção `Discord.GuildId` adicionada
- `CombatService.CalcularDano` — visibilidade `internal → public` para testabilidade direta
- `CombatService` — `private static readonly Random _random` substituído por `private static Random _random => Random.Shared` (thread-safe, net10.0+)
- `ArenaCommand.DesafioAsync` — checagens inline de `Degradado`/`Inativo` substituídas por `HeroiGuard.ValidarTodosDisponiveis`

### Corrigido

- `RecruitmentServiceTests` / `TorreServiceExtensionTests` — `Mock<HeroiService>` inválido (HeroiService não tem construtor vazio) substituído por instância real com mocks das sub-dependências; 66 testes passando, 0 falhas

---

## [3.6.0] - 2026-05-06 · UX-0 — Infraestrutura de Interação do Bot + Correções de Torre

### Adicionado

**Bot — Infraestrutura de Interação (UX-0) — `LegendsAwaken.Bot/Interactions/`**
- `PanelResult` record — ViewModel `(Embed, MessageComponent)` para separar construção de painel de exibição
- `IInteractionHandler` — interface com `CustomIdPrefix` + `HandleAsync(component, parts[])`; convenção de customId `sistema:acao[:p1:p2]` com `:` como separador
- `InteractionRouter` — roteador thread-safe (`ConcurrentDictionary`) que parseia `customId`, localiza handler pelo prefixo e despacha; 5 testes unitários (`InteractionRouterTests.cs`)
- `ConfirmationPanel.Criar(mensagem, confirmId, cancelId)` — fábrica estática de painel efêmero `[Confirmar] [Cancelar]` com embed laranja; `global:cancelar` como escape hatch universal
- `CidadeCommand : IInteractionHandler` — migrado para o router com 16 handlers internos (`coletar`, `alocar_node`, `alocar_predio`, `desalocar`, `desalocar_confirmar`, `construir`, `construir_predio`, `construir_confirmar`, `booster`, `atualizar`, `node_para_heroi`, `predio_para_heroi`)
- Confirmação de desalocação: `HandleDesalocarAsync` exibe `ConfirmationPanel`; `HandleDesalocarConfirmarAsync` executa e recarrega painel só no sucesso
- Confirmação de construção: `HandleConstruirPredioAsync` exibe custo + `ConfirmationPanel`; `HandleConstruirConfirmarAsync` executa e recarrega painel só no sucesso

### Alterado

- `CommandHandler` — `InteractionRouter` integrado antes do bloco legacy; bloco legacy `cidade_*` removido; `global:cancelar` tratado diretamente como escape hatch global
- `Program.cs` — `InteractionRouter` registrado como Singleton; `CidadeCommand` instanciado e registrado no router após `handler.Initialize()`
- `CidadePanel` — 7 botões migrados para convenção `:` (ex: `cidade_coletar` → `cidade:coletar`)
- Todos os sub-handlers de `CidadeCommand` tornados `private`

### Corrigido

- `ConfirmationPanel.Criar` — guard `ArgumentException.ThrowIfNullOrWhiteSpace(confirmId)` contra confirmId nulo
- `CommandHandler.HandleButtonExecutedAsync` — `TryRouteAsync` envolto em try/catch com `LogError` e resposta efêmera de fallback
- Handlers de confirmação — painel recarregado **apenas** no caminho de sucesso (path de erro retorna mensagem simples)
- `Andares` (SQLite) — migration `TorreNivelDificuldade` não tinha `.Designer.cs`; EF Core não descobria a migration e a coluna `NivelDificuldade` nunca era aplicada → `.Designer.cs` criado com `[Migration]` + `[DbContext]`
- `GeracaoDeDadosService.CriarTabelasAsync` — coluna `Inimigos` em `Andares` é excluída do EF (`.Ignore`) e não tem migration; adicionado `EnsureAndaresColunaInimigosAsync` com `PRAGMA table_info` + `ALTER TABLE` idempotente no startup

---

## [Sessão Design — 2026-05-06] · Torre: Framework de Arcos Narrativos + Skill analyze-folder-for-la

### Adicionado

**Skill**
- `/analyze-folder-for-la` — skill de análise de design: recebe um caminho de pasta, lê todos os `.txt`, produz análise por arquivo (6 seções: resumo, conceitos, elementos aproveitáveis, sugestões LA, nível de relevância) + sumário final (padrões recorrentes, melhores ideias, recomendações estratégicas); utiliza análise em paralelo via sub-agentes
- Executado em `C:\Workspace\D&D` (57 arquivos) e `C:\Workspace\Ideias` (2 arquivos); resultados compilados em `ANALISE_DND_PARA_LA.md`

**Design — Torre Arcos Narrativos**
- `DESIGN_TORRE_ARCOS.md` — documento de design completo: framework de objetivos 3-tier (A/B/C), sistema de flags (simples + compostas), 5 categorias de colecionável (Lore/Economia/Build/Chave/Arquivo), regra 70/30 para objetivos secundários, calibração de bônus por tier de andar, camada de Design vs camada de Display
- **Arco 1 — Torre em Ruínas** (Andares 1–4): boss Carniçal, flags `[grimorio_encontrado]`/`[altar_destruido]`/`[identidade_revelada]` (composta), 3 colecionáveis
- **Arco 2 — A Praga Ardente** (Andares 5–10): boss Jakk, mecânica de altar (invoca zumbis/turno), flags de investigação desbloqueiam rota alternativa via `[rota_alternativa]` (composta), item tradeoff `pedra_mana_contaminada`
- **Arco 3 — A Cabana dos Experimentos** (Andares 11–15): boss Golem de Calzone (absorve fogo), seed de villain recorrente Woganpuck via `[woganpuck_revelado]`, NPC permanente `[andolyn_aliada]` (composta), item tradeoff `frasco_molho_fervente`

### Decisões de Design

- Boss states: apenas `bossDerrotado` / `bossFugiu`; "ignorado" descartado (boss é portão obrigatório)
- Bosses que fogem retornam em arco futuro com +15–20% stats + nova mecânica adquirida
- Objetivos secundários têm estado `expirado` se o grupo avança sem completar
- Flags inter-arco (ex.: `[woganpuck_rastreado]`) só ativam efeito quando o trigger futuro ocorre — sem bônus antecipado

---

## [3.5.0] - 2026-04-27 · Sessão — Inventário (RecursoEstoque + JogadorItem), Enforcement de Sustento e Correção de Migration

### Adicionado

**Domínio**
- `IRecursoEstoqueRepository` — interface com `EnsureTableAsync`, `UpsertAsync`, `ObterAsync`, `ListarAsync`
- `IJogadorItemRepository` — interface com `EnsureTableAsync`, `UpsertAsync`, `ListarAsync`, `ObterPorConfigAsync`

**Infraestrutura**
- `RecursoEstoqueRepository` — repositório SQLite puro; tabela `RecursoEstoque(Id, UsuarioId, Recurso, Quantidade, UNIQUE(UsuarioId, Recurso))`; upsert via `ON CONFLICT DO UPDATE SET Quantidade += excluded.Quantidade`
- `JogadorItemRepository` — repositório SQLite puro; tabela `JogadorItens(Id, UsuarioId, ItemConfigId, Nome, Tipo, Icone, Efeito, Quantidade, ObtidoEm, ExtraData)`; upsert via `ON CONFLICT(UsuarioId, ItemConfigId)` acumulando quantidade

**Aplicação**
- `RecursoService` — fachada de estoque: `AdicionarAsync`, `ObterAsync`, `ListarEstoqueAsync`
- `AndarItemConfig` — config estática de 25 itens (andares 1–25, Bioma A); distribuição: 6 ComponenteCrafting, 6 Consumivel, 7 Equipamento, 6 ItemProgressao; item de destaque: Pedra-Chave do Bioma (andar 25, desbloqueia Bioma B) e Cristal de Sombra (andar 20, crafting de relíquias)
- `JogadorItemService` — fachada de itens únicos: `AdicionarAsync(def, quantidade)`, `ListarAsync`, `ObterPorConfigAsync`

**Bot**
- `TorreModoOperacaoPanel.CriarBoard` — novos parâmetros opcionais `estoque`, `itens`, `estadoSustento`, `horasComidaRestantes`; banner de aviso de sustento (🔴 Degradado / ⚠️ Instável com horas restantes); campos "📦 Estoque de Recursos" e "🎒 Itens" no embed; dropdown de andar exibe `{icone} {recurso} ×qtd | 🎁 {item}` por andar

### Alterado

**Aplicação**
- `TorreOperacaoService.ColetarTodasAsync` — crédita recursos não-Ouro em `RecursoEstoque` via `RecursoService` e concede item do andar via `AndarItemConfig` + `JogadorItemService`
- `GeracaoDeDadosService` — `CriarTabelasAsync` chama `EnsureTableAsync` dos dois novos repositórios; recebe `IRecursoEstoqueRepository` e `IJogadorItemRepository` no construtor
- `TorreExploracaoService` — bloco de início de exploração: `EstadoSustento.Degradado` agora lança `InvalidOperationException` (equiparado ao `Inativo`)

**Bot**
- `TorreCommand.HandleModoOperacaoAsync` — busca estoque + itens + estado de sustento e os repassa ao painel
- `TorreCommand.HandleOpAndarSelAsync` — verifica `EstadoSustento.Degradado` em todos os heróis; bloqueia início de operação se qualquer um estiver degradado
- `ArenaCommand` — verifica Degradado antes de `DesafioOndasAsync`; filtra heróis `Inativo` da party; guard de party vazia após filtro
- `Program.cs` — registra `IRecursoEstoqueRepository`, `IJogadorItemRepository`, `RecursoService`, `JogadorItemService`
- `CommandHandler` — campos e injeção de `_recursoService` e `_jogadorItemService`; todas as instanciações de `TorreCommand` atualizadas

### Corrigido
- **Migration `AddInimigoCatalogo`** — reordenação de `Up()`: `CreateIndex` e `AddForeignKey` movidos para ANTES dos blocos `InsertData`; corrige `SQLite Error 19: NOT NULL constraint failed: Inimigo.Atributos_Forca` causado por EF Core executar INSERTs enquanto a reconstrução da tabela (causada por `DropColumn`) ainda estava pendente

---

## [3.4.0] - 2026-04-27 · Sessão — Imagens de Heróis via R2, Limpeza de Personagens Legados e Desbloqueios Iniciais

### Adicionado

**Infraestrutura / Cloudflare R2**
- `R2ImageService` — serviço que acessa imagens privadas no Cloudflare R2 via API S3 (`AWSSDK.S3`); credenciais lidas de variáveis de ambiente `R2_ACCESS_KEY_ID` / `R2_SECRET_KEY`; endpoint configurado em `appsettings.json` (`R2:Endpoint`, `R2:Bucket`)
- `appsettings.json` — seção `R2` substituída (`BaseUrl` → `Endpoint` + `Bucket`); endpoint aponta para bucket `game-assets`
- Variáveis de ambiente de sistema: `R2_ACCESS_KEY_ID`, `R2_SECRET_KEY`

**Conversor de Imagens (`Converter.js`)**
- Upload automático para R2 ao final da conversão via `@aws-sdk/client-s3`
- Credenciais lidas de `.env` local (via `dotenv`); `.env.example` adicionado
- Nomes de saída migrados para 3 dígitos zero-padded: `001.webp` … `030.webp`

**Domínio / Aplicação**
- `HeroiConfig.ImageUrl` e `HeroiConfig.ImageUrlThumb` — comentários atualizados: campos agora armazenam chaves R2 (`heroes/display/001.webp`) em vez de URLs públicas
- `HeroiDataLoader` — derivação de chave R2 com zero-padding 3 dígitos (`"D3"`); removida dependência de `IConfiguration` para `R2:BaseUrl`

**Bot — exibição de imagem no detalhe de herói**
- `HeroisPanel.CriarEmbedDetalhe` — parâmetro opcional `bool comImagem`; adiciona `.WithImageUrl("attachment://hero.webp")` quando `true`
- `HeroisCommand.HandleVerDetalhesAsync` — busca `HeroiConfig` por nome, faz download da imagem via `R2ImageService` e envia como `FileAttachment`; degrada graciosamente (sem imagem) se o arquivo não existir no R2
- `HeroisCommand` — recebe `IHeroiConfigRepository` e `R2ImageService` como dependências opcionais
- `CommandHandler` — campo e injeção de `R2ImageService`; `HeroisCommand` instanciado com os novos parâmetros
- `Program.cs` — `R2ImageService` registrado como `Singleton`; passado ao `CommandHandler`

**Desbloqueios iniciais**
- `UsuarioService` — injeta `IHeroiDesbloqueadoRepository` e `IHeroiConfigRepository`; ao criar novo usuário, desbloqueia automaticamente Kaeryn (#16), Elize (#29) e Aegis (#9)
- `DiscordToGuid` — conversão `ulong → Guid` internalizada em `UsuarioService` (sem dependência do `DiscordIdHelper` do Bot)

### Alterado
- `HeroisCommand` — construtor migrado de primary-constructor simples para primary-constructor com parâmetros opcionais para `R2ImageService` e `IHeroiConfigRepository`

### Removido
- `PersonagensFixos` (Aldric, Yuzara, Thorvald, Kaen, Nyra, Seraph, Mira, Grom, Hana) — array e método `PopularPersonagensFixosAsync` removidos de `GeracaoDeDadosService`
- Startup limpa automaticamente heróis legados (`UsuarioId == 0`) do banco na primeira execução após atualização

---

## [3.3.0] - 2026-04-25 · Sessão — Bioma Panel, Cidade UX e Torre Modo Operação v2

### Corrigido
- `CidadeCommand` — ícones dos nodes todos exibindo 📦 por mismatch de case ("Comida" vs "comida" no switch inline); substituído por `ResourceNodeConfig.Icone()` centralizado
- `TorreCommand.HandleExplorarAsync` / `HandleExpAtualizarAsync` — teamPS calculado sobre todos os heróis do jogador em vez dos heróis da exploração; corrigido com filtro por `HeroisIds`
- `ColecaoCommand.MostrarAsync` — erro 40060 causado por `DeferAsync` + `UpdateAsync`; trocado para `ModifyOriginalResponseAsync`

### Adicionado

**Domínio / Aplicação**
- `BiomeService.ListarDescobertosAsync(andarAtual)` e `ObterPorIdAsync(Guid)` — métodos adicionados ao serviço de biomas
- `ResourceNodeConfig.Icone(string recurso)` — método centralizado de ícone por recurso (switch lowercase: comida→🌾, madeira→🪵, pedra→⛏️, erva→🌿, ouro→💰)
- `TorreOperacaoConfig` (arquivo novo) — config estática: duração fixa 8h, produção por tier de andar (1-5→Ouro×100, 6-10→Gema Rústica×5, 11-25→Essência Corrompida×8, 26-50→Fragmento Arcano×10, 51-75→Cristal Dimensional×15, 76+→Núcleo Primordial×20), afinidade racial leve, cálculo de slots (2 + GuildaNivel×2)
- `ITorreOperacaoRepository` — novos métodos `ListarAtivasAsync`, `ListarConcluidasAsync`, `ObterPorAndarAsync`
- `TorreOperacaoRepository` — implementação dos novos métodos do repositório

**Bot**
- `BiomaPanel.CriarLista` — Select Menu com lista de biomas descobertos, % de andares conquistados, indicador de bioma atual
- `BiomaPanel.CriarDetalhe` — painel de detalhe: descrição, barra de progresso por andares, pool de heróis com sistema de descoberta (herói principal sempre visível; heróis secundários aparecem como "?" até o jogador coletar o primeiro fragmento; contador "? N heróis por descobrir")
- `CidadePanel.CriarEmbed` — display de coletores agrupado por node ("• **Campo** — 12.0 🌾/h" com heróis indentados abaixo); contador de heróis disponíveis ("👥 **Heróis:** X disponíveis / Y total")
- `TorreModoOperacaoPanel` reescrito — `CriarBoard(ativas, concluidas, andarAtual, maxSlots)`, `CriarSemAndares`, `CriarSeletorAndar`, `CriarSeletorRemover`, `CriarNotificacaoTexto`

### Alterado
- `TorreOperacaoService` reescrito — `IniciarAsync(userId, andar, construcoes)`, `ProcessarTodasAsync`, `ColetarTodasAsync`, `CancelarPorAndarAsync`, `ConcluirOperacao`; sistema anterior de 1 operação por vez substituído por board de andares com múltiplas operações simultâneas
- `BiomaCommand` refatorado com 4 handlers: `ExecutarAsync`, `MostrarListaAsync` (torre_bioma / bioma_atualizar), `VoltarListaAsync` (bioma_lista), `MostrarDetalheAsync` (bioma_sel SelectMenu)
- `TorreCommand` — handlers substituídos: `HandleModoOperacaoAsync`, `HandleOpAlocarAsync`, `HandleOpAndarSelAsync`, `HandleOpColetarTodasAsync`, `HandleOpRemoverSelAsync`, `HandleOpRemoverAndarSelAsync`, `HandleOpFecharAsync`; construtor passa a receber `CidadeService`
- `CommandHandler` — IDs novos: `torre_op_alocar`, `torre_op_coletar_todas`, `torre_op_remover_sel`, `torre_op_fechar`, `torre_op_andar_sel`, `torre_op_remover_andar_sel`

---

## [3.2.0] - 2026-04-24 · Fase 3B.4 — Sistema de Sustento (MVP)

### Adicionado
**Domínio**
- Enum `EstadoSustento` (Ativo, Instavel, Degradado, Inativo) em `Enums.cs`
- Campo `EstadoSustento EstadoSustento` (default `Ativo`) em `Heroi`
- Campo `DateTime UltimoSustentoEm` (default `UtcNow`) em `Cidade`

**Infrastructure**
- Migration `20260423200000_SustentoSystem` — `ADD COLUMN EstadoSustento INTEGER DEFAULT 0` em `Herois`; `ADD COLUMN UltimoSustentoEm TEXT` em `Cidades`
- Designer file `20260423200000_SustentoSystem.Designer.cs` com snapshot do modelo

**Application**
- `SustentoService.ProcessarAsync(ulong)` — deduz Comida acumulada desde último tick (cap 24 h), recalcula e persiste `EstadoSustento` de todos os heróis ativos
- `SustentoService.ToggleInativoAsync(Guid)` — alterna `Inativo ↔ Ativo` (herói pausado não consome Comida)
- `SustentoService.ObterResumo(static)` — calcula consumo/hora e horas restantes para exibição no painel

**Bot**
- `HeroisPanel` — ícone de estado (`✅⚠️🔴💤`) em cada herói na lista e campo "Sustento" no embed de detalhe; `CriarComponentesDetalhe` com botão "Pausar / Ativar Sustento"
- `HeroisCommand.HandleToggleInativoAsync` — toggle + `UpdateAsync` no embed de detalhe (ephemeral)
- `CidadePanel` — linha de sustento abaixo do Humor: `✅ 3 🌾/h | Estoque: 120 | ~40.0h restantes`
- `CommandHandler` — `ProcessarAsync` chamado em todo slash command; routing `herois_toggle_inativo|{id}`
- `Program.cs` — registra e injeta `SustentoService`

### Decisões de MVP (vs. spec completa)
- Consumo flat: 1 Comida/hora por herói ativo (sem escala por raridade ou classe — reservado para 3B.4 completo)
- Estados são informativos: sem penalidades de atributo/XP ainda
- Moradia (Alojamento) não implementada neste MVP

---

## [3.1.0] - 2026-04-23 · Fase 3B.3 — Torre — Modo Operação

### Adicionado
**Domínio**
- Entidade `TorreOperacao` — `Id`, `UsuarioId`, `AndarNumero`, `ObjetivoOperacao`, `PerfilRisco`, `StatusOperacao`, `IniciadoEm`, `DuracaoHoras`, `ResultadoOuro?`, `ResultadoRecursoNome?`, `ResultadoRecursoQtd?`, `ConcluidoEm?`
- Enums: `ObjetivoOperacao` (FarmRecurso, ExploracaoLeve), `PerfilRisco` (Seguro, Balanceado, Agressivo), `StatusOperacao` (Ativa, Concluida, Expirada)
- Interface `ITorreOperacaoRepository`

**Infrastructure**
- `TorreOperacaoRepository` — raw SQLite (`EnsureTableAsync`, CREATE TABLE IF NOT EXISTS `TorreOperacoes`)

**Application**
- `TorreOperacaoService` — `IniciarAsync`, `VerificarPendenteAsync` (auto-conclui se expirado), `ColetarAsync` (credita ouro), `CancelarAsync`
- `GeracaoDeDadosService.CriarTabelasAsync` — chama `_torreOpRepo.EnsureTableAsync()`
- Fórmula de ouro: `andar × 3 × horas × mult` (Seguro=0.8, Balanceado=1.0, Agressivo=1.5)
- Recursos exclusivos por andar: Fragmento Rústico (≥5), Essência Corrompida (≥12), Cristal Arcano (≥18), Núcleo Sombrio (≥25)

**Bot**
- `TorreModoOperacaoPanel` — flow em 4 etapas: Select Menu de andar → objetivo → perfil de risco → confirmação; painéis de status ativo, coleta e notificação
- `TorrePanel` — botão `🏭 Modo Operação` adicionado
- `TorreCommand` — `HandleModoOperacaoAsync`, `HandleOpAndarAsync`, `HandleOpObjetivoAsync`, `HandleOpRiscoAsync`, `HandleOpColetarAsync`, `HandleOpCancelarAtivoAsync`, `HandleOpCancelarAsync`; poll de operação pendente em `ExecutarAsync`
- `CommandHandler` — routing do bloco `torre_modo_operacao` e `torre_op_*`; injeção de `TorreOperacaoService`
- `Program.cs` — registra `ITorreOperacaoRepository` e `TorreOperacaoService`

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
