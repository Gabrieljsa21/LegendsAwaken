# Legends Awaken — Roadmap

---

## Fase 1 — Pré-produção ⬅ atual
**Objetivo:** fechar o design antes de codar.

- [x] Conceito e visão geral definidos
- [x] Stack técnica escolhida (C#, Discord.Net, EF Core, SQLite)
- [x] Arquitetura Clean Architecture + DDD aplicada
- [x] GDD criado com sistemas de gacha, cidade, confiança, prioridade e cadeia de dependência
- [ ] Escopo do v1.0 fechado (o que entra, o que fica pra depois)
- [ ] Pool inicial de personagens fixos 5★ e 4★ definida (nomes, lore, profissão, arte)
- [ ] Receitas e cadeia de dependência dos itens básicos documentadas no GDD

**Sinal de saída:** GDD estável, sem features indefinidas no escopo do v1.0.

---

## Fase 2 — Protótipo da Cidade
**Objetivo:** validar se o loop de gestão é divertido antes de polir.

- [ ] Herói alocado em prédio produz recurso simples (sem cadeia, sem humor)
- [ ] `/cidade ver` mostrando recursos e heróis alocados
- [ ] `/cidade coletar` calculando produção por tempo decorrido
- [ ] Testar internamente se a mecânica de base é satisfatória

**Sinal de saída:** loop invocar → alocar → coletar jogável de ponta a ponta.

---

## Fase 3 — Produção (Alpha)
**Objetivo:** construir todos os sistemas do escopo v1.0.

### Gacha
- [ ] Campo `ImageUrl` e `Lore` nos heróis fixos
- [ ] Pool de personagens fixos 5★/4★ cadastrada
- [ ] Arte exibida no embed do pull

### Cidade — Sistemas de Gestão
- [ ] Sistema de Confiança (0–100) por herói
- [ ] Sistema de Humor com modificador de eficiência
- [ ] Política da cidade (`/cidade politica`)
- [ ] Prioridade por construção (`/cidade prioridade`)
- [ ] Auto-alocação por confiança seguindo a política
- [ ] `/cidade otimizar`
- [ ] Cadeia de dependência inteligente
- [ ] `/cidade cadeia` com display do raciocínio

### Cidade — Produção e Crafting
- [ ] Produção passiva com modificador de humor
- [ ] Forja funcional (crafting de equipamentos simples)
- [ ] Equipar heróis com itens craftados
- [ ] Upgrades de prédios (nível 1→2)

### Missões (Guilda)
- [ ] Geração automática de missões
- [ ] Herói parte → retorna com recompensas
- [ ] `/cidade missoes`

### Torre
- [ ] `/treinar` funcional via Arena
- [ ] Drops de materiais em andares de boss

### Qualidade
- [ ] Substituir `Console.WriteLine` por `ILogger` nos serviços
- [ ] Testes unitários para gacha, combate e produção
- [ ] Guild ID movido para `appsettings.json`
- [ ] `Random.Shared` no `GachaService`

**Sinal de saída:** todos os sistemas do escopo existem e funcionam.

---

## Fase 4 — Beta Fechado
**Objetivo:** jogadores reais encontram o que você não viu.

- [ ] Convidar 5–15 pessoas de confiança para testar
- [ ] Coletar feedback de UX e compreensão dos comandos
- [ ] Balancear economia (produção, XP, custo de upgrades)
- [ ] Corrigir bugs reportados
- [ ] Bot rodando em servidor externo (não na máquina local)

**Sinal de saída:** loop principal divertido e compreensível para quem não é o desenvolvedor.

---

## Fase 5 — Beta Aberto
**Objetivo:** estressar o sistema com volume real.

- [ ] Abrir servidor Discord para mais jogadores
- [ ] Monitorar performance e banco de dados
- [ ] Ajuste fino de balanceamento com dados reais
- [ ] Polish de embeds, mensagens e UX geral

---

## Fase 6 — v1.0 (Lançamento)
- [ ] Changelog público
- [ ] README atualizado
- [ ] Bot estável em produção

---

## Pós-lançamento (sem data)
- Novos banners e personagens fixos
- Eventos sazonais
- Novos prédios e receitas
- Mercado entre jogadores
- Multiplayer na Torre (raids)
