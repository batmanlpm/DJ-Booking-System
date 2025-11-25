"""add temp_bans table"""
from pathlib import Path
import logging

logger = logging.getLogger(__name__)

async def apply(connection):
    """apply the migration"""
    await connection.execute("""
        CREATE TABLE IF NOT EXISTS temp_bans (
            id SERIAL PRIMARY KEY,
            guild_id BIGINT NOT NULL,
            user_id BIGINT NOT NULL,
            moderator_id BIGINT NOT NULL,
            reason TEXT,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),
            expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
            active BOOLEAN DEFAULT TRUE,
            UNIQUE(guild_id, user_id, active)
        )
    """)
    logger.info("created temp_bans table")

async def rollback(connection):
    """rollback the migration"""
    await connection.execute("DROP TABLE IF EXISTS temp_bans")
    logger.info("dropped temp_bans table")

# This allows the migration to be run directly for testing
if __name__ == "__main__":
    import asyncio
    import asyncpg
    from config import DB_CONFIG
    
    async def test():
        """test the migration"""
        conn = await asyncpg.connect(**DB_CONFIG)
        try:
            await apply(conn)
            print("applied migration")
            # Test rollback
            # await rollback(conn)
            # print("rolled back migration")
        finally:
            await conn.close()
    
    asyncio.get_event_loop().run_until_complete(test())
