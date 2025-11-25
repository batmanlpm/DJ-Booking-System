-- Add assigner_roles column to track roles that need to be restored
ALTER TABLE pending_admin_assignments
ADD COLUMN assigner_roles TEXT;
