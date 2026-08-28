CREATE TABLE oauth_refresh_tokens (
    token_hash TEXT NOT NULL PRIMARY KEY,
    family_id TEXT NOT NULL,
    client_id TEXT NOT NULL,
    protected_ticket TEXT NOT NULL,
    created_utc TEXT NOT NULL,
    expires_utc TEXT NOT NULL,
    consumed_utc TEXT NULL,
    revoked_utc TEXT NULL
);

CREATE INDEX ix_oauth_refresh_tokens_family_id
    ON oauth_refresh_tokens (family_id);
