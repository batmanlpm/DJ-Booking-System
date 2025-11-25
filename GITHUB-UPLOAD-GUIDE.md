# ?? GitHub Upload Instructions

This guide will help you upload the DJ Booking System to GitHub.

---

## ?? Prerequisites

- GitHub account
- Git installed on your computer
- GitHub CLI (optional, for easier setup)

---

## ?? Quick Upload (Recommended)

### Step 1: Create GitHub Repository

**Option A: Via GitHub Website**

1. Go to https://github.com/new
2. Repository name: `DJ-Booking-System`
3. Description: `AI-powered DJ booking and management system with Discord integration`
4. **Make it Private** (recommended) or Public
5. **DO NOT** initialize with README (we have one)
6. Click "Create repository"

**Option B: Via GitHub CLI** (if installed)

```bash
gh repo create DJ-Booking-System --private --description "AI-powered DJ booking and management system"
```

---

### Step 2: Link Local Repository to GitHub

Copy the commands from GitHub's "…or push an existing repository from the command line" section, or use these:

```powershell
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

# Add GitHub as remote
git remote add origin https://github.com/YOUR_USERNAME/DJ-Booking-System.git

# Verify remote was added
git remote -v
```

Replace `YOUR_USERNAME` with your actual GitHub username.

---

### Step 3: Stage All Files

```powershell
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

# Check what will be committed
git status

# Add all files (respecting .gitignore)
git add .

# Verify what's staged
git status
```

**?? VERIFY:** Make sure sensitive files are NOT staged:
- DiscordBotToken.txt
- UPLOAD-*.ps1 files
- Files with hardcoded credentials

---

### Step 4: Commit Changes

```powershell
git commit -m "Initial commit: DJ Booking System v1.3.0

Features:
- Discord-style friends list with DM
- Three-strike ban system
- Integrated chat system
- RadioBOSS integration
- Role-based permissions
- Real-time online status

Complete WPF application built with .NET 8 and Azure Cosmos DB"
```

---

### Step 5: Push to GitHub

```powershell
# Set main branch
git branch -M main

# Push to GitHub
git push -u origin main
```

**This may take several minutes** due to large project size (~100+ MB).

---

## ? Verification

### Check Upload Success

1. Go to your GitHub repository: `https://github.com/YOUR_USERNAME/DJ-Booking-System`
2. Verify files are present
3. Check that `.gitignore` is working (sensitive files should be absent)

### Files That Should Be Uploaded

? Source code (.cs, .xaml files)
? Project files (.csproj, .sln)
? Documentation (.md files)
? Resources (images, icons)
? .gitignore
? README.md
? CONFIGURATION.md

### Files That Should NOT Be Uploaded

? `bin/` and `obj/` folders
? `DiscordBotToken.txt`
? `UPLOAD-*.ps1` scripts
? `.vs/` folder
? `*.user` files
? Large installers (.exe files)
? Hardcoded credentials

---

## ?? Security Check

Before making repository public, verify:

```powershell
# Search for potential credentials in committed files
git grep -i "password"
git grep -i "accountkey"
git grep -i "token"
git grep -i "fraser1960"
```

If any sensitive data is found, **DO NOT PUSH** and fix immediately:

```powershell
# If you need to remove a file from Git
git rm --cached FILENAME
git commit -m "Remove sensitive file"
```

---

## ?? Repository Settings (Recommended)

After upload, configure these in GitHub repository settings:

### General Settings
- [ ] Add repository description
- [ ] Add topics/tags: `wpf`, `dotnet`, `csharp`, `dj-booking`, `azure`, `cosmos-db`
- [ ] Set repository to Private (if contains any sensitive info)

### Branches
- [ ] Set `main` as default branch
- [ ] Enable branch protection rules (for collaboration)

### Security
- [ ] Enable Dependabot alerts
- [ ] Enable security advisories
- [ ] Review .gitignore is working

---

## ?? Collaborating

If you want others to contribute:

### Add Collaborators

1. Go to: Settings ? Collaborators
2. Add GitHub usernames
3. Set appropriate permissions

### Create Issues Template

```bash
# Create .github/ISSUE_TEMPLATE/bug_report.md
# Create .github/ISSUE_TEMPLATE/feature_request.md
```

### Add Contributing Guidelines

Create `CONTRIBUTING.md` with:
- Code style guidelines
- Pull request process
- Testing requirements

---

## ?? Future Updates

When you make changes:

```powershell
# Stage changes
git add .

# Commit with descriptive message
git commit -m "Add new feature: XYZ"

# Push to GitHub
git push origin main
```

### Semantic Versioning

Use tags for releases:

```powershell
# Create a release tag
git tag -a v1.3.0 -m "Release v1.3.0: Friends List & DM"
git push origin v1.3.0
```

---

## ?? Common Issues

### Issue: "Permission denied (publickey)"

**Solution:** Set up SSH keys or use HTTPS with Personal Access Token

```powershell
# Use HTTPS instead
git remote set-url origin https://github.com/YOUR_USERNAME/DJ-Booking-System.git
```

### Issue: "Repository not found"

**Solution:** Verify repository name and username

```powershell
# Check remote URL
git remote -v

# Fix if needed
git remote set-url origin https://github.com/CORRECT_USERNAME/DJ-Booking-System.git
```

### Issue: "File too large"

**Solution:** GitHub has 100MB file limit. Large installers should not be committed.

```powershell
# Remove large file
git rm --cached "Installer/Output/DJBookingSystem-Setup-v1.3.0.exe"
git commit -m "Remove large installer file"
```

Use GitHub Releases to host installer files instead.

---

## ?? GitHub Releases

To publish installer:

1. Go to repository ? Releases ? Create new release
2. Tag: `v1.3.0`
3. Title: `DJ Booking System v1.3.0 - Friends List & DM`
4. Upload `DJBookingSystem-Setup-v1.3.0.exe` as binary
5. Add release notes from `CHANGELOG-v1.3.0.md`
6. Publish release

**Download URL will be:**
```
https://github.com/YOUR_USERNAME/DJ-Booking-System/releases/download/v1.3.0/DJBookingSystem-Setup-v1.3.0.exe
```

---

## ? Post-Upload Checklist

- [ ] Repository created on GitHub
- [ ] Local repo linked to remote
- [ ] All files committed (check with `git status`)
- [ ] Pushed to GitHub successfully
- [ ] Verified upload on GitHub website
- [ ] Checked no sensitive data uploaded
- [ ] README.md displays correctly
- [ ] .gitignore is working
- [ ] Repository visibility set correctly (Public/Private)
- [ ] Repository topics/tags added
- [ ] Release created with installer (if public)

---

## ?? Support

If you encounter issues:

- **Git Documentation:** https://git-scm.com/doc
- **GitHub Docs:** https://docs.github.com
- **GitHub Support:** https://support.github.com
- **Project Issues:** https://github.com/YOUR_USERNAME/DJ-Booking-System/issues

---

**Ready to upload!** Follow the steps above in order.

**The Fallen Collective**  
Last Updated: January 25, 2025
