CREATE TABLE IF NOT EXISTS wallets (
    id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    workspace_id bigint NOT NULL,
    name varchar(40) NOT NULL,
    color char(6) NOT NULL,
    icon varchar(40) NOT NULL,
    initial_balance bigint NOT NULL,
    active boolean NOT NULL,
    creation_date date NOT NULL,
    updated_date timestamptz NOT NULL,
    inactive_date date,
    deleted_date date,
    status int NOT NULL,
    FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
);
