# Legends Awaken — TODO

Tarefas granulares organizadas por área. Acompanhe o progresso macro no `ROADMAP.md`.

---

## Pré-produção (Fase 1) ✅ concluída

- [x] Fechar escopo do v1.0
- [x] Definir pool inicial de personagens fixos (3x 5★ + 6x 4★) com nome, raça, profissão e lore
- [x] Documentar receitas básicas de crafting no GDD
- [x] Documentar cadeia de dependência dos itens básicos no GDD
- [ ] Gerar arte IA para cada personagem fixo e registrar as URLs *(pode ser feito em paralelo com a produção)*

---

## Gacha

- [x] Sistema de invocação (x1 e x11)
- [x] Soft-pity com curva cúbica por banner
- [x] Banners configuráveis
- [x] Geração procedural de heróis
- [x] Dropdown de seleção de banner
- [ ] Campo `ImageUrl` na entidade `Heroi`
- [ ] Campo `Lore` na entidade `Heroi` (para personagens fixos)
- [ ] Cadastrar pool de personagens fixos 5★/4★ no seed
- [ ] Exibir arte no embed do pull quando disponível
- [ ] Banner de Profissão (rate-up por profissão)

---

## Heróis

- [x] Atributos base (Força, Agilidade, Vitalidade, Inteligência, Percepção)
- [x] Raças com bônus passivos
- [x] Profissões
- [x] Sistema de habilidades com XP e níveis
- [x] `/ver_heroi` com autocomplete
- [x] `/listar_herois` com paginação e filtro
- [ ] Equipar herói com item craftado (`/heroi equipar`)
- [ ] Exibir equipamentos no `/ver_heroi`

---

## Torre

- [x] Torre infinita com andares por usuário
- [x] Tipos de andar (Subjugação, Fuga, Escolta, Defesa, Armadilha, Evento)
- [x] Bosses em andares 5 / 10 / 25
- [x] Combate automático por turnos
- [x] Party de até 5 heróis (`/grupo`)
- [ ] Drops de materiais de crafting em andares de boss
- [ ] Fragmentos de personagens fixos como drop raro
- [ ] `/treinar` funcional via Arena (XP acelerado)
- [ ] IA tática no combate (atacar menor HP / maior ameaça)

---

## Cidade — Base

- [x] Entidade `Cidade` (Nome, Nível, Recursos, Construções, Trabalhadores)
- [x] Recursos: Comida, Madeira, Pedra, Ouro
- [x] Enum `Profissao` com combate, coleta e produção
- [ ] `/cidade ver` — painel com recursos, prédios e heróis alocados
- [ ] `/cidade coletar` — coleta produção acumulada por tempo
- [ ] Produção passiva com teto de 24h
- [ ] Alocação manual (`/cidade alocar`, `/cidade desalocar`)
- [ ] Upgrades de prédio nível 1 → 2

---

## Cidade — Gestão Autônoma

- [ ] Campo `Confianca` (0–100) na entidade `Heroi`
- [ ] Campo `Humor` na entidade `Heroi` (enum: Deprimido → Animado)
- [ ] Modificador de eficiência baseado no Humor
- [ ] Auto-alocação por confiança seguindo a política ativa
- [ ] `/cidade politica <foco>` (recursos / producao / combate / equilibrio)
- [ ] `/cidade otimizar` — aloca todos os vagos no melhor slot
- [ ] Lógica de líder de prédio (Parceiro+) reduzindo queda de humor do time
- [ ] Prioridade por construção (`/cidade prioridade`)
- [ ] Cadeia de dependência inteligente
- [ ] `/cidade cadeia <prédio>` — exibe raciocínio e estimativa

---

## Crafting

- [ ] Receitas básicas definidas (armas, armaduras, poções)
- [ ] Forja produzindo equipamentos passivamente
- [ ] Qualidade do item (Comum → Mestre) baseada em habilidade + nível do prédio
- [ ] Laboratório produzindo poções
- [ ] Poções usadas automaticamente na Torre
- [ ] Blueprints desbloqueáveis via missões ou drops

---

## Missões (Guilda)

- [ ] Geração automática de missões por nível da Guilda
- [ ] Herói parte → retorna após duração → traz recompensas
- [ ] Tipos: Coleta, Subjugação, Escolta, Transporte, Investigação, Recuperação
- [ ] Falha possível se herói for fraco demais
- [ ] `/cidade missoes` — lista missões ativas e status
- [ ] Missões raras com fragmentos e blueprints como recompensa

---

## Qualidade de Código

- [x] Token via variável de ambiente
- [x] Clean Architecture com separação real de camadas
- [x] Nullable warnings corrigidos (0 warnings)
- [x] Migração para .NET 10
- [ ] `Random.Shared` no `GachaService` (thread-safety)
- [ ] `ILogger<T>` substituindo `Console.WriteLine` nos serviços
- [ ] Guild ID movido para `appsettings.json`
- [ ] Caminho do banco de dados via variável de ambiente ou relativo
- [ ] Testes unitários: GachaService (pity, raridade)
- [ ] Testes unitários: CombatService (turnos, dano)
- [ ] Testes unitários: produção passiva da cidade

---

## Infraestrutura

- [x] Repositório no GitHub
- [x] `.gitignore` cobrindo `.claude/`, `.idea/`, binários
- [ ] Bot rodando em servidor externo (VPS ou similar)
- [ ] Variável de ambiente configurada no servidor
- [ ] Script de deploy automatizado
