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

CREATE TABLE IF NOT EXISTS categories (
    id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    workspace_id bigint NOT NULL,
    flow int NOT NULL,
    kind int NOT NULL,
    parent_id int,
    name varchar(40) NOT NULL,
    icon varchar(40) NOT NULL,
    active boolean NOT NULL,
    creation_date date NOT NULL,
    updated_date timestamptz NOT NULL,
    inactive_date date,
    deleted_date date,
    status int NOT NULL,
    FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE,
    FOREIGN KEY (parent_id) REFERENCES categories (id)
);

CREATE TABLE IF NOT EXISTS tags (
    id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    workspace_id bigint NOT NULL,
    name varchar(24) NOT NULL,
    color char(7) NOT NULL,
    FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE
);
