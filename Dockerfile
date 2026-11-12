# syntax=docker/dockerfile:1

# ---------- build: compila e publica a app ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release
WORKDIR /src

# Só o .csproj primeiro: o restore fica em cache enquanto as dependências não mudarem
COPY Pisco.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c $CONFIGURATION -o /app --no-restore /p:UseAppHost=false

# ---------- base: runtime comum às imagens final e debug ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Pasta das chaves de Data Protection (cookies de login, antiforgery), montada num volume
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R app:app /home/app/.aspnet

# ---------- debug: build Debug + vsdbg, para o VS Code se ligar ao contentor ----------
FROM base AS debug
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl unzip procps \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["dotnet", "Pisco.dll"]

# ---------- final: imagem de produção (por omissão) ----------
FROM base AS final
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["dotnet", "Pisco.dll"]
