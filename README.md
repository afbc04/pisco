# Pisco

App pessoal em Blazor Server (.NET 8) com PostgreSQL.

## Requisitos

- .NET 8 SDK
- Docker
- VS Code com a extensão **C# Dev Kit** (ou **C#**)

## Configuração

Os dados da base de dados ficam em `.env` (não vai para o git). Para começar, copia o exemplo:

```bash
cp .env.example .env
```

O `docker-compose.yml` e a app leem as mesmas variáveis (`POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`).

## Correr em debug (VS Code)

1. Abre a pasta no VS Code.
2. Carrega em **F5** (configuração **Pisco**).

O VS Code arranca o PostgreSQL (`docker compose up -d db`), compila o projeto e abre o browser em http://localhost:5000.

No primeiro arranque é criado o utilizador `admin` / `admin` e a workspace `demo` (marcada como demo e página inicial).

## Correr sem debugger

```bash
docker compose up -d db
dotnet run
```

## Correr tudo em Docker

```bash
docker compose up -d --build
```

A app fica em http://localhost:5080.

## Estrutura

```
components/   páginas e layout (Blazor)
data/         acesso à BD (Dapper), schema SQL (data/sql) e seed
Domain/       entidades
Services/     autenticação, workspaces
Resources/    traduções (pt / en)
wwwroot/      css, js, fontes, imagens
```

Tudo fica na BD `pisco`: utilizadores e workspaces no schema `core` (definido em `data/sql/core.sql`) e cada workspace no seu próprio schema (`ws_<nome>`).
