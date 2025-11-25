"""
admin security settings management.
"""
from typing import Optional
from datetime import datetime

class AdminSecuritySettings:
    """manages admin security settings for guilds."""
    
    @staticmethod
    def is_security_enabled(guild_id: int) -> bool:
        """check if admin security is enabled for a guild."""
        from modules.database.database import db
        
        try:
            # first ensure the guild exists in the guilds table
            db.execute_query(
                """
                INSERT OR IGNORE INTO guilds (guild_id, owner_id)
                VALUES (?, 0)
                """,
                (guild_id,),
                commit=True
            )
            
            # check if there's an existing setting
            result = db.execute_query(
                """
                SELECT security_enabled 
                FROM admin_security_settings 
                WHERE guild_id = ?
                """,
                (guild_id,),
                fetch=True
            )
            
            if result and len(result) > 0 and 'security_enabled' in result[0]:
                return result[0]['security_enabled'] == 1
                
            # if no setting exists, create one with default value (enabled)
            # use INSERT OR IGNORE to handle race conditions
            db.execute_query(
                """
                INSERT OR IGNORE INTO admin_security_settings 
                (guild_id, security_enabled, actions_security_enabled, updated_at)
                VALUES (?, 1, 1, ?)
                """,
                (guild_id, datetime.utcnow()),
                commit=True
            )
            
            # get the setting we just created
            result = db.execute_query(
                """
                SELECT security_enabled 
                FROM admin_security_settings 
                WHERE guild_id = ?
                """,
                (guild_id,),
                fetch=True
            )
            
            # return the setting or default to True if something went wrong
            return result[0]['security_enabled'] == 1 if result and len(result) > 0 else True
            
        except Exception as e:
            print(f"Error checking security setting: {e}")
            return True  # default to enabled on error
    
    @staticmethod
    def set_security_enabled(guild_id: int, enabled: bool) -> bool:
        """set the admin security setting for a guild."""
        from modules.database.database import db
        
        try:
            # first ensure the guild exists in the guilds table
            db.execute_query(
                """
                INSERT OR IGNORE INTO guilds (guild_id, owner_id)
                VALUES (?, 0)
                """,
                (guild_id,),
                commit=True
            )
            
            # first check if a record exists
            result = db.execute_query(
                """
                SELECT 1 FROM admin_security_settings 
                WHERE guild_id = ?
                """,
                (guild_id,),
                fetch=True
            )
            
            if result and len(result) > 0:
                # update existing record
                db.execute_query(
                    """
                    UPDATE admin_security_settings 
                    SET security_enabled = ?, updated_at = ?
                    WHERE guild_id = ?
                    """,
                    (int(enabled), datetime.utcnow(), guild_id),
                    commit=True
                )
            else:
                # insert new record
                db.execute_query(
                    """
                    INSERT INTO admin_security_settings 
                    (guild_id, security_enabled, actions_security_enabled, updated_at)
                    VALUES (?, ?, 1, ?)
                    """,
                    (guild_id, int(enabled), datetime.utcnow()),
                    commit=True
                )
                
            return True
            
        except Exception as e:
            print(f"Error updating admin security settings: {e}")
            return False
            
    @staticmethod
    def is_actions_security_enabled(guild_id: int) -> bool:
        """check if actions security is enabled for a guild."""
        from modules.database.database import db
        
        try:
            # first ensure the guild exists in the guilds table
            db.execute_query(
                """
                INSERT OR IGNORE INTO guilds (guild_id, owner_id)
                VALUES (?, 0)
                """,
                (guild_id,),
                commit=True
            )
            
            # check if there's an existing setting
            result = db.execute_query(
                """
                SELECT actions_security_enabled 
                FROM admin_security_settings 
                WHERE guild_id = ?
                """,
                (guild_id,),
                fetch=True
            )
            
            if result and len(result) > 0 and 'actions_security_enabled' in result[0]:
                return result[0]['actions_security_enabled'] == 1
                
            # if no setting exists, create one with default value (enabled)
            # use INSERT OR IGNORE to handle race conditions
            db.execute_query(
                """
                INSERT OR IGNORE INTO admin_security_settings 
                (guild_id, security_enabled, actions_security_enabled, updated_at)
                VALUES (?, 1, 1, ?)
                """,
                (guild_id, datetime.utcnow()),
                commit=True
            )
            
            # get the setting we just created
            result = db.execute_query(
                """
                SELECT actions_security_enabled 
                FROM admin_security_settings 
                WHERE guild_id = ?
                """,
                (guild_id,),
                fetch=True
            )
            
            # return the setting or default to True if something went wrong
            return result[0]['actions_security_enabled'] == 1 if result and len(result) > 0 else True
            
        except Exception as e:
            print(f"Error checking actions security setting: {e}")
            return True  # default to enabled on error
    
    @staticmethod
    def set_actions_security_enabled(guild_id: int, enabled: bool) -> bool:
        """set the actions security setting for a guild."""
        from modules.database.database import db
        
        try:
            # first ensure the guild exists in the guilds table
            db.execute_query(
                """
                INSERT OR IGNORE INTO guilds (guild_id, owner_id)
                VALUES (?, 0)
                """,
                (guild_id,),
                commit=True
            )
            
            # first check if a record exists
            result = db.execute_query(
                """
                SELECT 1 FROM admin_security_settings 
                WHERE guild_id = ?
                """,
                (guild_id,),
                fetch=True
            )
            
            if result and len(result) > 0:
                # update existing record
                db.execute_query(
                    """
                    UPDATE admin_security_settings 
                    SET actions_security_enabled = ?, updated_at = ?
                    WHERE guild_id = ?
                    """,
                    (int(enabled), datetime.utcnow(), guild_id),
                    commit=True
                )
            else:
                # insert new record
                db.execute_query(
                    """
                    INSERT INTO admin_security_settings 
                    (guild_id, security_enabled, actions_security_enabled, updated_at)
                    VALUES (?, 1, ?, ?)
                    """,
                    (guild_id, int(enabled), datetime.utcnow()),
                    commit=True
                )
                
            return True
            
        except Exception as e:
            print(f"Error updating actions security settings: {e}")
            return False
