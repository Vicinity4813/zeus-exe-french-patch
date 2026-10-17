// Moteur du patcheur : analyse, patch et restauration. Toutes les modifications sont notees dans
// « <jeu>\Patch FR - sauvegarde » (copie des fichiers remplaces, octets d'origine de l'exe) pour pouvoir les annuler.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public enum LogKind { Info, Ok, Warn, Error }

public class Options
{
    public bool Exe = true, Texts = true, Menus, Voices;
}

public class Engine
{
    public Action<string, LogKind> Log = delegate { };

    public const string BackupDirName = "Patch FR - sauvegarde";
    const string StateFileName = "patch-fr.txt";

    // Fichiers de l'installation francaise. Les textes et les polices (lettres accentuees) sont indispensables.
    public static readonly string[] TextFiles = { "Zeus_Text.eng", "Zeus_MM.eng", @"Model\Zeus eventmsg.txt", @"DATA\Zeus_Fonts.sg3", @"DATA\zeus_Fonts.555" };
    public static readonly string[] MenuFiles = { @"DATA\Poseidon_FE_MainMenu.JPG", @"DATA\Poseidon_FE_Registry.jpg", @"DATA\scoreb.jpg", @"DATA\Zeus_Defeat.jpg",
        @"DATA\Zeus_FE_CampaignSelection.JPG", @"DATA\Zeus_FE_ChooseGame.jpg", @"DATA\Zeus_FE_MissionIntroduction.jpg", @"DATA\Zeus_FE_Registry.jpg",
        @"DATA\Zeus_FE_tutorials.JPG", @"DATA\Zeus_Victory.jpg" };
    // Voix et sons : tout le dossier Audio francais (fichiers differents ou absents de l'installation anglaise) et les videos.
    static readonly string[] VoiceDirs = { "Audio", "Binks" };

    public string ExePath, FrDir;
    public string GameDir { get { return Path.GetDirectoryName(ExePath); } }
    public string BackupDir { get { return Path.Combine(GameDir, BackupDirName); } }

    byte[] exe, frTable, savedTable;
    List<Site> sites;
    public Survey Status;
    public string FrExeUsed;

    // ---- analyse

    // Lit l'exe et l'installation francaise ; renvoie false si le patch de l'exe est impossible (message deja ecrit).
    public bool Analyse() { return Analyse(false); }

    public bool Analyse(bool quiet)
    {
        exe = null; sites = null; Status = null; frTable = null; FrExeUsed = null;
        if (!File.Exists(ExePath)) { Log(L.T("Exe introuvable : ", "Exe not found: ") + ExePath, LogKind.Error); return false; }
        exe = File.ReadAllBytes(ExePath);
        Pe pe;
        try { pe = new Pe(exe); if (pe.Off(FrenchPatch.CharTable) < 0) throw new Exception(); }
        catch { Log(L.T("Ce fichier n'est pas un Zeus.exe reconnu.", "This file is not a recognised Zeus.exe."), LogKind.Error); return false; }

        savedTable = LoadSavedTable();
        frTable = FindFrTable();
        if (frTable == null && savedTable == null)
        {
            if (!quiet) Log(L.T("Table des caractères française introuvable : choisissez le dossier de votre jeu en français (il doit contenir son Zeus.exe).",
                    "French character table not found: choose the folder of your French game (it must contain its Zeus.exe)."), LogKind.Warn);
            return false;
        }
        if (frTable == null) frTable = LoadPatchedTable();

        sites = FrenchPatch.Build(exe, frTable, savedTable);
        Status = Survey.Of(sites, exe);
        if (Status.AllOriginal && !FrenchPatch.LooksEnglishTable(exe, frTable))
        {
            Log(L.T("Table des caractères de l'exe inattendue.", "Unexpected character table in the exe."), LogKind.Error);
            return false;
        }
        if (Status.AllOriginal) Log(L.T("Exe : anglais GOG (v2.1.4.0), pas encore patché. ", "Exe: English GOG (v2.1.4.0), not patched yet. ") + sites.Count + L.T(" endroits à modifier, tous conformes.", " places to change, all as expected."), LogKind.Ok);
        else if (Status.AllPatched) Log(L.T("Exe : patch FR déjà appliqué.", "Exe: French patch already applied."), LogKind.Ok);
        else
        {
            Log(string.Format(L.T("Exe : {0} endroits d'origine, {1} déjà patchés, {2} inattendus.", "Exe: {0} original places, {1} already patched, {2} unexpected."), Status.Original, Status.Patched, Status.Other), Status.Other > 0 ? LogKind.Error : LogKind.Warn);
            foreach (Site x in Status.Unexpected)
                Log(string.Format("  {0} @ {1:X} : {2}", x.Name, x.Offset, Site.Hex(Read(x.Offset, x.Original.Length), 12)), LogKind.Error);
            if (Status.Other > 0)
                Log(L.T("Cet exe n'est pas la version GOG anglaise, ou un autre patch a modifié les mêmes octets : rien ne sera écrit.",
                        "This exe is not the English GOG version, or another patch changed the same bytes: nothing will be written."), LogKind.Error);
        }
        return true;
    }

    byte[] Read(int off, int n) { byte[] b = new byte[n]; if (off >= 0 && off + n <= exe.Length) Array.Copy(exe, off, b, 0, n); return b; }

    byte[] FindFrTable()
    {
        if (string.IsNullOrEmpty(FrDir) || !Directory.Exists(FrDir)) return null;
        List<string> candidates = new List<string>();
        string main = Path.Combine(FrDir, "Zeus.exe");
        if (File.Exists(main)) candidates.Add(main);
        foreach (string f in Directory.GetFiles(FrDir, "*.exe")) if (!candidates.Contains(f)) candidates.Add(f);
        foreach (string f in candidates)
        {
            if (new FileInfo(f).Length < 1000000) continue;
            byte[] t = FrenchPatch.FindFrenchTable(File.ReadAllBytes(f), exe);
            if (t != null) { FrExeUsed = f; return t; }
        }
        return null;
    }

    // ---- etat sauvegarde

    string StatePath { get { return Path.Combine(BackupDir, StateFileName); } }

    Dictionary<string, string> LoadState()
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        if (!File.Exists(StatePath)) return d;
        foreach (string line in File.ReadAllLines(StatePath))
        {
            int sp = line.IndexOf(' ');
            if (sp > 0) d[line.Substring(0, sp)] = line.Substring(sp + 1);
        }
        return d;
    }

    byte[] LoadSavedTable() { string v; return LoadState().TryGetValue("table-origine", out v) ? FromHex(v) : null; }
    byte[] LoadPatchedTable() { string v; return LoadState().TryGetValue("table-fr", out v) ? FromHex(v) : null; }

    List<string> LoadCopied()
    {
        List<string> r = new List<string>();
        if (!File.Exists(StatePath)) return r;
        foreach (string line in File.ReadAllLines(StatePath)) if (line.StartsWith("fichier ")) r.Add(line.Substring(8));
        return r;
    }

    void SaveState(byte[] tableOrig, byte[] tableFr, List<string> copied)
    {
        Directory.CreateDirectory(BackupDir);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Patch FR de Zeus.exe : etat pour l'annulation. Ne pas modifier.");
        if (tableOrig != null) sb.AppendLine("table-origine " + ToHex(tableOrig));
        if (tableFr != null) sb.AppendLine("table-fr " + ToHex(tableFr));
        foreach (string c in copied) sb.AppendLine("fichier " + c);
        File.WriteAllText(StatePath, sb.ToString(), Encoding.UTF8);
    }

    static string ToHex(byte[] b) { StringBuilder sb = new StringBuilder(); foreach (byte x in b) sb.Append(x.ToString("X2")); return sb.ToString(); }
    static byte[] FromHex(string s) { s = s.Trim(); byte[] b = new byte[s.Length / 2]; for (int i = 0; i < b.Length; i++) b[i] = Convert.ToByte(s.Substring(2 * i, 2), 16); return b; }

    // ---- patch

    public bool Patch(Options o, bool dryRun)
    {
        if (!Analyse()) return false;
        if (o.Exe && Status.Other > 0) return false;
        if ((o.Texts || o.Menus || o.Voices) && (string.IsNullOrEmpty(FrDir) || !Directory.Exists(FrDir)))
        {
            Log(L.T("Choisissez le dossier de votre jeu en français pour copier ses fichiers.", "Choose the folder of your French game to copy its files."), LogKind.Error);
            return false;
        }

        // Fichiers a copier
        List<string> files = new List<string>();
        if (o.Texts) files.AddRange(TextFiles);
        if (o.Menus) files.AddRange(MenuFiles);
        if (o.Voices) files.AddRange(VoiceFiles());
        List<string> missing = new List<string>();
        foreach (string f in (o.Texts ? TextFiles : new string[0])) if (!File.Exists(Path.Combine(FrDir, f))) missing.Add(f);
        if (missing.Count > 0)
        {
            Log(L.T("Fichiers absents du dossier français : ", "Files missing from the French folder: ") + string.Join(", ", missing.ToArray()), LogKind.Error);
            return false;
        }

        List<string> copied = LoadCopied();
        int toCopy = 0;
        foreach (string f in files)
        {
            string src = Path.Combine(FrDir, f), dst = Path.Combine(GameDir, f);
            if (!File.Exists(src) || SameFile(src, dst)) continue;
            toCopy++;
        }
        bool exeToDo = o.Exe && !Status.AllPatched;
        Log(string.Format(L.T("À faire : exe {0}, {1} fichier(s) français à copier.", "To do: exe {0}, {1} French file(s) to copy."),
            exeToDo ? L.T("à patcher", "to patch") : L.T("rien", "nothing"), toCopy), LogKind.Info);
        if (dryRun) { Log(L.T("Vérification terminée : rien n'a été écrit.", "Check done: nothing was written."), LogKind.Ok); return true; }
        if (!exeToDo && toCopy == 0) { Log(L.T("Rien à faire.", "Nothing to do."), LogKind.Ok); return true; }

        Directory.CreateDirectory(BackupDir);
        byte[] tableOrig = savedTable;

        // Exe : copie de l'original, ecriture dans un fichier temporaire, verification, remplacement
        if (exeToDo)
        {
            string exeBak = Path.Combine(BackupDir, Path.GetFileName(ExePath));
            // copie de l'exe tel qu'il est avant le patch (rafraichie si l'exe a change, par ex. autre resolution)
            if (Status.Patched == 0 || !File.Exists(exeBak)) { Writable(exeBak); File.Copy(ExePath, exeBak, true); }
            if (tableOrig == null) tableOrig = Read(sites[0].Offset, 224);
            byte[] outb = (byte[])exe.Clone();
            foreach (Site x in sites)
                if (x.StateIn(exe) == SiteState.Original) Array.Copy(x.Patched, 0, outb, x.Offset, x.Patched.Length);
            if (!Survey.Of(sites, outb).AllPatched) throw new Exception("vérification après patch");
            SaveState(tableOrig, frTable, copied);
            if (!WriteExe(outb)) return false;
            Log(string.Format(L.T("Exe patché ({0} endroits). Original sauvegardé dans « {1} ».", "Exe patched ({0} places). Original saved in \"{1}\"."), sites.Count, BackupDirName), LogKind.Ok);
        }

        // Fichiers : l'original anglais est copie une seule fois dans la sauvegarde
        int n = 0;
        foreach (string f in files)
        {
            string src = Path.Combine(FrDir, f), dst = Path.Combine(GameDir, f);
            if (!File.Exists(src) || SameFile(src, dst)) continue;
            string bak = Path.Combine(BackupDir, "fichiers", f);
            if (!copied.Contains(f))
            {
                if (File.Exists(dst)) { Directory.CreateDirectory(Path.GetDirectoryName(bak)); Writable(bak); File.Copy(dst, bak, true); }
                copied.Add(f);
                SaveState(tableOrig, frTable, copied);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            Writable(dst);
            File.Copy(src, dst, true);
            n++;
        }
        if (n > 0) Log(string.Format(L.T("{0} fichier(s) français copié(s) ; les originaux sont dans la sauvegarde.", "{0} French file(s) copied; the originals are in the backup."), n), LogKind.Ok);
        Log(L.T("Terminé. Lancez le jeu pour vérifier.", "Done. Start the game to check."), LogKind.Ok);
        return true;
    }

    IEnumerable<string> VoiceFiles()
    {
        foreach (string d in VoiceDirs)
        {
            string root = Path.Combine(FrDir, d);
            if (!Directory.Exists(root)) continue;
            foreach (string f in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                yield return f.Substring(FrDir.TrimEnd('\\').Length + 1);
        }
    }

    static bool SameFile(string a, string b)
    {
        if (!File.Exists(b)) return false;
        FileInfo fa = new FileInfo(a), fb = new FileInfo(b);
        if (fa.Length != fb.Length) return false;
        using (FileStream x = File.OpenRead(a)) using (FileStream y = File.OpenRead(b))
        {
            byte[] p = new byte[65536], q = new byte[65536];
            while (true)
            {
                int r1 = x.Read(p, 0, p.Length), r2 = y.Read(q, 0, q.Length);
                if (r1 != r2) return false;
                if (r1 == 0) return true;
                for (int i = 0; i < r1; i++) if (p[i] != q[i]) return false;
            }
        }
    }

    // Fichiers installes depuis un CD : souvent en lecture seule, ce qui empeche de les remplacer
    static void Writable(string path)
    {
        if (File.Exists(path)) { FileAttributes a = File.GetAttributes(path); if ((a & FileAttributes.ReadOnly) != 0) File.SetAttributes(path, a & ~FileAttributes.ReadOnly); }
    }

    bool WriteExe(byte[] b)
    {
        Writable(ExePath);
        string tmp = ExePath + ".tmp";
        try
        {
            File.WriteAllBytes(tmp, b);
            File.Copy(tmp, ExePath, true);
            File.Delete(tmp);
            return true;
        }
        catch (IOException e)
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
            Log(L.T("Impossible d'écrire l'exe (le jeu est-il ouvert ?) : ", "Could not write the exe (is the game running?): ") + e.Message, LogKind.Error);
            return false;
        }
        catch (UnauthorizedAccessException e)
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
            Log(L.T("Accès refusé (dossier protégé : copiez le jeu ailleurs ou lancez en administrateur) : ", "Access denied (protected folder: copy the game elsewhere or run as administrator): ") + e.Message, LogKind.Error);
            return false;
        }
    }

    // ---- annulation

    public bool Restore()
    {
        if (!Analyse(true) && exe == null) return false;
        bool any = false;
        if (sites != null && Status.Patched > 0)
        {
            if (savedTable == null) { Log(L.T("Sauvegarde du patch introuvable : impossible d'annuler le patch de l'exe.", "Patch backup not found: cannot undo the exe patch."), LogKind.Error); return false; }
            if (Status.Other > 0) { Log(L.T("Des octets patchés ont été modifiés depuis : annulation refusée.", "Patched bytes were changed since: undo refused."), LogKind.Error); return false; }
            byte[] outb = (byte[])exe.Clone();
            foreach (Site x in sites) if (x.StateIn(exe) == SiteState.Patched) Array.Copy(x.Original, 0, outb, x.Offset, x.Original.Length);
            if (!Survey.Of(sites, outb).AllOriginal) throw new Exception("vérification après annulation");
            if (!WriteExe(outb)) return false;
            Log(L.T("Exe : patch FR annulé (les autres modifications, comme la résolution, sont conservées).", "Exe: French patch undone (other changes, such as the resolution, are kept)."), LogKind.Ok);
            any = true;
        }
        List<string> copied = LoadCopied();
        int n = 0;
        foreach (string f in copied)
        {
            string bak = Path.Combine(BackupDir, "fichiers", f), dst = Path.Combine(GameDir, f);
            Writable(dst);
            if (File.Exists(bak)) File.Copy(bak, dst, true);
            else if (File.Exists(dst)) File.Delete(dst);   // fichier absent de l'installation anglaise : retire
            n++;
        }
        if (n > 0) { Log(string.Format(L.T("{0} fichier(s) d'origine remis en place.", "{0} original file(s) put back."), n), LogKind.Ok); any = true; }
        if (any)
        {
            SaveState(null, null, new List<string>());
            Log(L.T("Le jeu est revenu à son état d'avant le patch FR.", "The game is back to its state before the French patch."), LogKind.Ok);
        }
        else Log(L.T("Rien à annuler.", "Nothing to undo."), LogKind.Info);
        return true;
    }
}
