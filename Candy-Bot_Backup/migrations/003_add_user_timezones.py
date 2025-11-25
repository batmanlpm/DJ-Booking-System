"""
migration to add user_timezones table for storing user timezone preferences.
"""

def up():
    """run the migration to create the user_timezones table."""
    query = """
    CREATE TABLE IF NOT EXISTS user_timezones (
        user_id INTEGER PRIMARY KEY,
        timezone TEXT NOT NULL,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    )
    """
    from modules.database import db
    db.execute_query(query, commit=True)
    print("✅ Created user_timezones table")

def down():
    """rollback the migration by dropping the user_timezones table."""
    query = "DROP TABLE IF EXISTS user_timezones"
    from modules.database import db
    db.execute_query(query, commit=True)
    print("❌ Dropped user_timezones table")
