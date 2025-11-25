"""add warnings table"""
from pathlib import Path
import logging

logger = logging.getLogger(__name__)

async def apply(connection):
    """apply the migration"""
    await connection.execute("""
        CREATE TABLE IF NOT EXISTS warnings (
            id SERIAL PRIMARY KEY,
            guild_id BIGINT NOT NULL,
            user_id BIGINT NOT NULL,
            moderator_id BIGINT NOT NULL,
            reason TEXT NOT NULL,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),
            active BOOLEAN DEFAULT TRUE
        )
    """)
    logger.info("created warnings table")

async def rollback(connection):
    """rollback the migration"""
    await connection.execute("DROP TABLE IF EXISTS warnings")
    logger.info("dropped warnings table")

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
