// Fichiers francais : copie dans le dossier du jeu anglais les textes, polices, voix, video, campagnes et tutoriels d'un
// jeu francais installe. Le jeu francais n'est que lu. Tout est annulable : chaque fichier anglais remplace et les campagnes
// anglaises vont dans « Sauvegarde anglais » (dossier du jeu), les fichiers ajoutes sont notes au fur et a mesure.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class FrenchFiles
{
    public const string BackupFolder = "Sauvegarde anglais";
    const string AddedList = "fichiers-ajoutes.txt";

    static readonly string[] Required = {
        "Zeus_Text.eng", "Zeus_MM.eng", @"Model\Zeus eventmsg.txt", @"DATA\Zeus_Fonts.sg3", @"DATA\zeus_Fonts.555" };
    // Pas d'images de DATA : celles de l'edition francaise sont les memes que les anglaises, sans texte.
    static readonly string[] Optional = { "Zeus_Editor_Text.eng", @"Binks\Zeus_Intro.bik", @"Binks\Zeus_Intro_HiQ.bik" };
    static readonly string[] Tutorials = {
        "agora", "bigcity", "colony", "culture", "firstcity", "food", "goods", "housing", "hygiene", "interface",
        "oliveoil", "palace", "peacewar", "sanctuary", "trade", "warrior" };
    static readonly string[] FrenchAdventures = {
        "L'odyssée", "La naissance de Troie", "La révolte maya", "Les plus jeunes jumeaux", "Atlantide, la cité engloutie",
        "Jeu ouvert - Armée 2", "Jeu ouvert - Bac à sable 2", "Jeu ouvert - Economie 2" };
    // Le jeu classe ses campagnes par le premier caractere du nom : seul le jeu d'origine les nomme ainsi, a la difference
    // des scenarios ajoutes par le joueur, qui ne sont pas copies.
    const string CampaignPrefixes = "#&@[]^{}";
    static readonly string[] EnglishCampaigns = {
        "#The Birth of Atlantis.pak", "#Zeus and Europa.pak", "&Enlightenment in the West.pak", "&Perseus and Medusa.pak",
        "@Athens through the Ages.pak", "@Life in the Mediterranean.pak", "[Atlantis Reborn.pak", "[The Voyages of Jason.pak",
        "]Hercules Labors.pak", "]Proetus and Bellerophon.pak", "^The Peloponnesian War.pak", "^Two Worlds Collide.pak",
        "{The Trojan War.pak", "}Open Play Economic.pak", "}Open Play Military.pak", "}Open Play Sandbox.pak" };
    static readonly string[] EnglishAdventures = {
        "Open Play Economic 2", "Open Play Military 2", "Open Play Sandbox 2", "The Founding of Troy",
        "The Mayan Revolution", "The Odyssey", "The Sinking of Atlantis", "The Youngest Twins" };

    public static bool HasBackup(string exe) { return Directory.Exists(Path.Combine(Path.GetDirectoryName(exe), BackupFolder)); }

    // ---- import

    public static bool Import(string exe, string source, bool dryRun)
    {
        if (!File.Exists(exe)) { Core.Log(L.T("Fichier introuvable : ", "File not found: ") + exe, LogKind.Error); return false; }
        byte[] exeBytes = File.ReadAllBytes(exe);
        string build = Core.BuildOf(exeBytes);
        if (build == null || build == "français") { Core.Log(L.T("Import réservé à un Zeus.exe anglais 2.1.4.0 reconnu.", "Import needs a recognised English 2.1.4.0 Zeus.exe."), LogKind.Error); return false; }
        string game = Path.GetDirectoryName(exe);
        source = Path.GetFullPath(source).TrimEnd('\\');
        string why = SourceProblem(game, source);
        if (why != null) { Core.Log(why, LogKind.Error); return false; }

        List<string> files = FilesToCopy(source);
        long size = 0;
        foreach (string rel in files) size += new FileInfo(Path.Combine(source, rel)).Length;
        foreach (string rel in Optional)
            if (!File.Exists(Path.Combine(source, rel))) Core.Log(L.T("  absent, ignoré : ", "  missing, skipped: ") + rel, LogKind.Warn);
        bool campaigns = files.Exists(f => IsCampaign(f)), adventures = files.Exists(f => IsFrenchAdventure(f));
        Core.Log(string.Format(L.T("Import : {0} fichiers, {1:N0} Mo.", "Import: {0} files, {1:N0} MB."), files.Count, size / 1048576), LogKind.Info);

        string root = Path.GetPathRoot(game);
        if (root.Length == 3 && new DriveInfo(root).AvailableFreeSpace < size) { Core.Log(L.T("Pas assez de place libre sur le disque du jeu : rien n'a été écrit.", "Not enough free space on the game's disk: nothing was written."), LogKind.Error); return false; }
        if (dryRun) return true;

        string backup = Path.Combine(game, BackupFolder), listPath = Path.Combine(backup, AddedList);
        Directory.CreateDirectory(backup);
        HashSet<string> added = ReadAdded(listPath);
        try
        {
            if (campaigns) foreach (string n in EnglishCampaigns) SetAsideFile(game, backup, Path.Combine("Adventures", n));
            if (adventures) foreach (string n in EnglishAdventures) SetAsideFolder(game, backup, Path.Combine("Adventures", n));
            long done = 0; int step = 0;
            foreach (string rel in files)
            {
                string src = Path.Combine(source, rel), dst = Path.Combine(game, rel), saved = Path.Combine(backup, rel);
                if (File.Exists(dst) && !added.Contains(rel) && !File.Exists(saved)) MoveFile(dst, saved);
                else if (!File.Exists(dst) && !File.Exists(saved) && added.Add(rel)) File.AppendAllText(listPath, rel + "\r\n", Encoding.UTF8);
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                File.Copy(src, dst, true);
                File.SetAttributes(dst, File.GetAttributes(dst) & ~FileAttributes.ReadOnly);   // copie depuis un CD
                done += new FileInfo(dst).Length;
                if (size > 0 && done * 10 / size > step) { step = (int)(done * 10 / size); Core.Log("  " + step * 10 + " %", LogKind.Info); }
            }
        }
        catch (Exception e)
        {
            Core.Log(L.T("Import interrompu (jeu ouvert ? dossier protégé ?) : ", "Import stopped (game running? protected folder?): ") + e.Message, LogKind.Error);
            Core.Log(L.T("« Remettre les fichiers anglais » annule ce qui a déjà été fait.", "\"Put the English files back\" undoes what was already done."), LogKind.Warn);
            return false;
        }
        Core.Log(L.T("Fichiers français copiés. Les fichiers anglais remplacés et les campagnes anglaises sont dans « ", "French files copied. The replaced English files and the English campaigns are in \"") + BackupFolder + L.T(" ».", "\"."), LogKind.Ok);
        if (!Core.FrPatchApplied(exeBytes)) Core.Log(L.T("L'exe n'est pas encore patché : cliquez aussi sur « Patcher en français ».", "The exe is not patched yet: also click \"Patch to French\"."), LogKind.Warn);
        return true;
    }

    // Probleme de la source (message), ou null. N'ecrit rien.
    static string SourceProblem(string game, string source)
    {
        if (!Directory.Exists(source)) return L.T("Dossier introuvable : ", "Folder not found: ") + source;
        string g = Path.GetFullPath(game).TrimEnd('\\') + "\\", s = source + "\\";
        if (s.StartsWith(g, StringComparison.OrdinalIgnoreCase) || g.StartsWith(s, StringComparison.OrdinalIgnoreCase))
            return L.T("Le dossier du jeu français doit être séparé de celui du jeu à traduire.", "The French game folder must be separate from the game to translate.");
        foreach (string r in Required)
            if (!File.Exists(Path.Combine(source, r))) return L.T("Ce dossier n'est pas un jeu français installé (fichier manquant : ", "This folder is not an installed French game (missing file: ") + r + ").";
        string text = Encoding.GetEncoding(1252).GetString(File.ReadAllBytes(Path.Combine(source, "Zeus_Text.eng")));
        if (text.IndexOf("Quitter", StringComparison.Ordinal) < 0 || text.IndexOf("Charger", StringComparison.Ordinal) < 0)
            return L.T("Zeus_Text.eng n'est pas en français dans ce dossier (jeu anglais ?).", "Zeus_Text.eng is not French in this folder (English game?).");
        return null;
    }

    static List<string> FilesToCopy(string source)
    {
        List<string> r = new List<string>(Required);
        foreach (string f in Optional) if (File.Exists(Path.Combine(source, f))) r.Add(f);
        AddTree(r, source, "Audio");
        string adv = Path.Combine(source, "Adventures");
        if (!Directory.Exists(adv)) return r;
        foreach (string f in Directory.GetFiles(adv))
        {
            string n = Path.GetFileName(f), ext = Path.GetExtension(n).ToLowerInvariant();
            bool campaign = ext == ".pak" && CampaignPrefixes.IndexOf(n[0]) >= 0;
            bool tutorial = ext == ".pak" && Array.IndexOf(Tutorials, Path.GetFileNameWithoutExtension(n).ToLowerInvariant()) >= 0;
            bool tutorialSave = ext == ".sav" && n.StartsWith("tutorial", StringComparison.OrdinalIgnoreCase);
            if (campaign || tutorial || tutorialSave) r.Add(Path.Combine("Adventures", n));
        }
        foreach (string d in FrenchAdventures) AddTree(r, source, Path.Combine("Adventures", d));
        return r;
    }

    static void AddTree(List<string> list, string root, string rel)
    {
        string d = Path.Combine(root, rel);
        if (!Directory.Exists(d)) return;
        foreach (string f in Directory.GetFiles(d, "*", SearchOption.AllDirectories)) list.Add(f.Substring(root.Length + 1));
    }

    static bool IsCampaign(string rel) { return Path.GetDirectoryName(rel) == "Adventures" && CampaignPrefixes.IndexOf(Path.GetFileName(rel)[0]) >= 0; }

    static bool IsFrenchAdventure(string rel)
    {
        foreach (string d in FrenchAdventures) if (rel.StartsWith(Path.Combine("Adventures", d) + "\\", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    static void SetAsideFile(string game, string backup, string rel)
    {
        string src = Path.Combine(game, rel), dst = Path.Combine(backup, rel);
        if (File.Exists(src) && !File.Exists(dst)) MoveFile(src, dst);
    }

    static void SetAsideFolder(string game, string backup, string rel)
    {
        string src = Path.Combine(game, rel), dst = Path.Combine(backup, rel);
        if (!Directory.Exists(src) || Directory.Exists(dst)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        Directory.Move(src, dst);
    }

    static void MoveFile(string src, string dst)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        File.Move(src, dst);
    }

    static HashSet<string> ReadAdded(string listPath)
    {
        HashSet<string> s = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(listPath)) foreach (string l in File.ReadAllLines(listPath, Encoding.UTF8)) if (l.Trim().Length > 0) s.Add(l.Trim());
        return s;
    }

    // ---- retour a l'anglais

    public static bool RestoreFiles(string exe)
    {
        string game = Path.GetDirectoryName(exe), backup = Path.Combine(game, BackupFolder), listPath = Path.Combine(backup, AddedList);
        if (!Directory.Exists(backup)) { Core.Log(L.T("Pas de dossier « ", "No \"") + BackupFolder + L.T(" » à côté de cet exe.", "\" folder next to this exe."), LogKind.Error); return false; }
        try
        {
            foreach (string rel in ReadAdded(listPath))
            {
                string f = Path.Combine(game, rel);
                if (File.Exists(f)) { File.SetAttributes(f, FileAttributes.Normal); File.Delete(f); }
                RemoveEmptyParents(game, Path.GetDirectoryName(f));
            }
            if (File.Exists(listPath)) File.Delete(listPath);
            string prefix = backup.TrimEnd('\\') + "\\";
            foreach (string d in Directory.GetDirectories(backup, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(game, d.Substring(prefix.Length)));   // dossiers vides compris
            foreach (string f in Directory.GetFiles(backup, "*", SearchOption.AllDirectories))
            {
                string dst = Path.Combine(game, f.Substring(prefix.Length));
                if (File.Exists(dst)) { File.SetAttributes(dst, FileAttributes.Normal); File.Delete(dst); }
                File.Move(f, dst);
            }
            Directory.Delete(backup, true);   // il ne reste que des dossiers vides
        }
        catch (Exception e)
        {
            Core.Log(L.T("Retour interrompu (jeu ouvert ? dossier protégé ?), à relancer : ", "Restore stopped (game running? protected folder?), run it again: ") + e.Message, LogKind.Error);
            return false;
        }
        Core.Log(L.T("Fichiers anglais remis en place.", "English files put back."), LogKind.Ok);
        return true;
    }

    static void RemoveEmptyParents(string game, string dir)
    {
        string top = Path.GetFullPath(game).TrimEnd('\\');
        while (dir != null && dir.Length > top.Length && Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length == 0)
        {
            Directory.Delete(dir);
            dir = Path.GetDirectoryName(dir);
        }
    }
}
