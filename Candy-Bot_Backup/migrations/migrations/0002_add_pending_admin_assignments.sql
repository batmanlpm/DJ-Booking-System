-- Add table to track pending admin role assignments
CREATE TABLE IF NOT EXISTS pending_admin_assignments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    guild_id INTEGER NOT NULL,
    assigner_id INTEGER NOT NULL,
    assignee_id INTEGER NOT NULL,
    role_id INTEGER NOT NULL,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    status TEXT DEFAULT 'pending',  -- 'pending', 'approved', 'denied'
    UNIQUE(guild_id, assigner_id, assignee_id, role_id) ON CONFLICT REPLACE
);

-- Add index for faster lookups
CREATE INDEX IF NOT EXISTS idx_pending_assignments_guild ON pending_admin_assignments(guild_id);
CREATE INDEX IF NOT EXISTS idx_pending_assignments_assigner ON pending_admin_assignments(assigner_id);
CREATE INDEX IF NOT EXISTS idx_pending_assignments_assignee ON pending_admin_assignments(assignee_id);
CREATE INDEX IF NOT EXISTS idx_pending_assignments_status ON pending_admin_assignments(status);
