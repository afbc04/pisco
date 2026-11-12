# Docker no Pisco

Como estão montados as imagens, os contentores e o debug. Para instalar e correr, vê o [README](../README.md).

## Ficheiros

| Ficheiro | Para que serve |
|---|---|
| `Dockerfile` | Receita das imagens da app (produção e debug) |
| `docker-compose.yml` | Define os serviços `db` (PostgreSQL) e `pisco` (a app) |
| `docker-compose.debug.yml` | Acrescenta-se ao anterior para correr a app em modo debug |
| `.dockerignore` | O que não entra na imagem (`bin/`, `obj/`, `.git/`, `.env`, documentação, …) |
| `.env` | Valores usados pelo Compose (base de dados, portas) |

## O Dockerfile

Está dividido em fases (*stages*). Cada imagem final só leva o que precisa.

```
build  (SDK .NET 8)       restaura as dependências e publica a app em /app
  │
base   (runtime ASP.NET)  porta 8080 + pasta das chaves de Data Protection
  ├── debug               + vsdbg (debugger) + build em Debug   → pisco:debug
  └── final               só a app publicada em Release        → pisco:latest
```

- **Cache do restore** — o `Pisco.csproj` é copiado e restaurado antes do resto do código. Enquanto não mudares os pacotes NuGet, esse passo vem da cache e a imagem constrói-se mais depressa.
- **Utilizador sem privilégios** — a app corre como o utilizador `app` (UID 1654) que já vem na imagem da Microsoft, e não como `root`.
- **`CONFIGURATION`** — argumento de build: `Release` por omissão, `Debug` na imagem de debug.
- **vsdbg** — o debugger da Microsoft, instalado em `/vsdbg` só na imagem de debug. É ele que o VS Code usa para se ligar à app.

Para construir só a imagem de produção, à mão:

```bash
docker build -t pisco:latest .
```

## Os serviços (`docker-compose.yml`)

### `db` — PostgreSQL 16

- Contentor `pisco-db`.
- Porta `127.0.0.1:5432`, ou seja, só acessível a partir do teu PC (o DBeaver no Windows também chega lá).
- Os dados ficam no volume `pisco-pgdata` e sobrevivem a `docker compose down`.
- Tem um *healthcheck* (`pg_isready`): a app só arranca quando a base de dados está pronta a aceitar ligações.

### `pisco` — a app

- Contentor `pisco`, em http://localhost:5080 (muda com `PISCO_PORT`).
- Liga-se à base de dados pelo nome `db`, na rede interna do Compose.
- As chaves de Data Protection (que assinam os cookies de login e os tokens dos formulários) ficam no volume `pisco-keys`. Assim, recriar o contentor não faz logout a ninguém.
- `restart: unless-stopped`: volta a arrancar sozinha se o PC ou o Docker reiniciarem.

### Volumes

| Volume | Conteúdo | Apaga-se com |
|---|---|---|
| `pisco-pgdata` | Dados do PostgreSQL | `docker compose down -v` |
| `pisco-keys` | Chaves dos cookies da app | `docker compose down -v` |

`docker compose down` sem `-v` mantém os dois volumes.

## Debug dentro do contentor

O `docker-compose.debug.yml` muda só o serviço `pisco`:

- usa a fase `debug` do Dockerfile, com `CONFIGURATION=Debug` (imagem `pisco:debug`);
- define `ASPNETCORE_ENVIRONMENT=Development` (mensagens de erro detalhadas);
- desliga o `restart` automático.

O que acontece quando carregas em **F5** com **Pisco (Docker)**:

1. A tarefa `docker: debug` corre `docker compose -f docker-compose.yml -f docker-compose.debug.yml up -d --build --wait`.
2. O VS Code executa `docker exec -i pisco /vsdbg/vsdbg` e liga-se ao processo 1 do contentor, que é a app (`dotnet Pisco.dll`).
3. A app foi compilada em `/src` dentro da imagem; o `sourceFileMap` traduz esse caminho para a pasta do projeto, e por isso os breakpoints batem certo com os teus ficheiros.

Quando paras o debug, o contentor continua a correr em modo debug. Para voltar à imagem de produção:

```bash
docker compose up -d --build --wait
```

## Comandos úteis

Ver o estado dos contentores:

```bash
docker compose ps
```

Logs da app em tempo real:

```bash
docker compose logs -f pisco
```

Abrir uma shell dentro do contentor da app:

```bash
docker exec -it pisco bash
```

Consola `psql`:

```bash
docker compose exec db sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
```

Apagar imagens antigas que já não são usadas:

```bash
docker image prune
```
