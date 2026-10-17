-- Defines main database | It registers available workspaces & users
CREATE SCHEMA IF NOT EXISTS core;

-- Defines workspace
CREATE TABLE IF NOT EXISTS core.workspaces (
    id            bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name          varchar(30) NOT NULL,
    schema_name   varchar(63) NOT NULL UNIQUE,
    is_demo       boolean     NOT NULL DEFAULT false,
    is_landing    boolean     NOT NULL DEFAULT false
);

-- Defines users
CREATE TABLE IF NOT EXISTS core.users (
    id                bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username          varchar(40) NOT NULL UNIQUE,
    password_hash     bytea       NOT NULL,
    password_salt     bytea       NOT NULL,
    is_admin          boolean     NOT NULL DEFAULT false,
    created_at        timestamptz NOT NULL DEFAULT now(),
    last_workspace_id bigint      REFERENCES core.workspaces (id) ON DELETE SET NULL
);

-- Permissions of users
CREATE TABLE IF NOT EXISTS core.workspace_members (
    workspace_id bigint      NOT NULL REFERENCES core.workspaces (id) ON DELETE CASCADE,
    user_id      bigint      NOT NULL REFERENCES core.users (id) ON DELETE CASCADE,
    added_at     timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (workspace_id, user_id)
);
