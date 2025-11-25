-- Add actions_security_enabled column with default value 1 (enabled)
ALTER TABLE admin_security_settings 
ADD COLUMN actions_security_enabled BOOLEAN DEFAULT 1;
