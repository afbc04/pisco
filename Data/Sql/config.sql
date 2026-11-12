CREATE SCHEMA IF NOT EXISTS pisco_config;

CREATE TABLE IF NOT EXISTS pisco_config.workspaces (
    id        bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name      varchar(30) NOT NULL,
    is_public boolean     NOT NULL
);

CREATE TABLE IF NOT EXISTS pisco_config.config (
    id                   smallint    PRIMARY KEY,
    username             varchar(40) NOT NULL,
    password_hash        bytea       NOT NULL,
    password_salt        bytea       NOT NULL,
    landing_workspace_id bigint      REFERENCES pisco_config.workspaces (id),
    last_workspace_id    bigint      REFERENCES pisco_config.workspaces (id)
);
