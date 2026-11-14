CREATE TABLE IF NOT EXISTS workspaces (
    id        bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name      varchar(30) NOT NULL,
    is_public boolean     NOT NULL
);

CREATE TABLE IF NOT EXISTS config (
    id                   smallint    PRIMARY KEY,
    username             varchar(40) NOT NULL,
    password_hash        bytea       NOT NULL,
    password_salt        bytea       NOT NULL,
    landing_workspace_id bigint      REFERENCES workspaces (id),
    last_workspace_id    bigint      REFERENCES workspaces (id)
);
