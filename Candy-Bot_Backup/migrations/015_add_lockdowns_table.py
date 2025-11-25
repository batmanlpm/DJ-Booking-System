"""migration to add the lockdowns table"""

async def apply(connection):
    """apply the migration"""
    await connection.execute("""
        CREATE TABLE IF NOT EXISTS lockdowns (
            id SERIAL PRIMARY KEY,
            guild_id BIGINT NOT NULL,
            target_id BIGINT NOT NULL,  -- channel_id for channel lockdown, 0 for server-wide
            moderator_id BIGINT NOT NULL,
            reason TEXT,
            expires_at TIMESTAMP WITH TIME ZONE,
            is_active BOOLEAN DEFAULT TRUE,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),
            UNIQUE(guild_id, target_id, is_active)
        )
    """)
    
    await connection.execute("""
        CREATE TABLE IF NOT EXISTS lockdown_permissions (
            id SERIAL PRIMARY KEY,
            lockdown_id INTEGER NOT NULL REFERENCES lockdowns(id) ON DELETE CASCADE,
            target_type TEXT NOT NULL,  -- 'role' or 'member'
            target_id BIGINT NOT NULL,
            channel_id BIGINT NOT NULL,
            permission_type TEXT NOT NULL,  -- 'overwrite' or 'default_role'
            allow_permissions BIGINT NOT NULL,
            deny_permissions BIGINT NOT NULL,
            UNIQUE(lockdown_id, target_type, target_id, channel_id)
        )
    """)
    
    await connection.execute("""
        CREATE INDEX IF NOT EXISTS idx_lockdowns_guild_active 
        ON lockdowns(guild_id, is_active)
    """)
    
    await connection.execute("""
        CREATE INDEX IF NOT EXISTS idx_lockdown_permissions_lockdown_id 
        ON lockdown_permissions(lockdown_id)
    """)

async def rollback(connection):
    """rollback the migration"""
    await connection.execute("DROP TABLE IF EXISTS lockdown_permissions CASCADE")
    await connection.execute("DROP TABLE IF EXISTS lockdowns CASCADE")
