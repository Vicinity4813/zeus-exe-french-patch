// Coeur du patcheur : reconnaissance de l'exe, patch FR (exe anglais), patch de l'editeur (exe anglais ou francais),
// restauration. Seul le Zeus.exe choisi est lu et modifie ; une copie « Zeus.exe.bak » est faite avant la 1re modification.
using System;
using System.Collections.Generic;
using System.IO;

public enum LogKind { Info, Ok, Warn, Error }

public static class Core
{
    public static Action<string, LogKind> Log = delegate { };

    // Compilation reconnue (« anglais 2.1.4.0 », « français ») ou null. Anglais : fonction de l'editeur ET resolution a sa
    // place (les exe russes ont la meme fonction mais un autre code ailleurs : non reconnus).
    public static string BuildOf(byte[] exe)
    {
        string b = EditorPatch.Recognised(exe);
        if (b == "anglais 2.1.4.0") { int o = 0x10BA28; if (o + 10 > exe.Length || exe[o] != 0xBE || exe[o + 5] != 0xB9) return null; }
        return b;
    }

    // Resolution ecrite dans l'exe : anglais, mov esi,largeur / mov ecx,hauteur (offset 0x10BA28) ;
    // francais, mov [FC691C],largeur / mov [FC6918],hauteur (0x57C8CF / 0x57C8D9). null si introuvable.
    public static int[] ResolutionOf(byte[] b)
    {
        string build = EditorPatch.Recognised(b);
        if (build == null) return null;
        if (build == "français")
        {
            Pe pe = new Pe(b);
            int w = pe.Off(0x0057C8CF), h = pe.Off(0x0057C8D9);
            if (w > 0 && h > 0 && b[w] == 0xC7 && b[w + 1] == 0x05 && b[h] == 0xC7 && b[h + 1] == 0x05)
                return new[] { BitConverter.ToInt32(b, w + 6), BitConverter.ToInt32(b, h + 6) };
            return null;
        }
        int o = 0x10BA28;
        if (o + 10 <= b.Length && b[o] == 0xBE && b[o + 5] == 0xB9) return new[] { BitConverter.ToInt32(b, o + 1), BitConverter.ToInt32(b, o + 6) };
        return null;
    }

    // Taille de la vue (et de la mappemonde) pour une resolution : cases de 60 px moins 2 en largeur, rangees de 15 px
    // sous la barre du haut (30 px).
    public static int[] ViewOf(int w, int h) { return new[] { 60 * ((w - 180) / 60) - 2, ((h - 30) / 15) * 15 }; }

    static byte[] Load(string path)
    {
        if (!File.Exists(path)) { Log(L.T("Fichier introuvable : ", "File not found: ") + path, LogKind.Error); return null; }
        return File.ReadAllBytes(path);
    }

    // ---- patch FR

    public static bool PatchFr(string path, bool dryRun)
    {
        byte[] exe = Load(path);
        if (exe == null) return false;
        string build = BuildOf(exe);
        if (build == "français") { Log(L.T("Cet exe est déjà en français : rien à faire.", "This exe is already French: nothing to do."), LogKind.Ok); return true; }
        if (build == null) { Log(L.T("Exe non reconnu : seule la compilation anglaise 2.1.4.0 (GOG, Steam et ses versions widescreen) peut être patchée.", "Exe not recognised: only the English 2.1.4.0 build (GOG, Steam and its widescreen versions) can be patched."), LogKind.Error); return false; }

        List<Site> sites = FrenchPatch.Build(exe, FrTable.Bytes, null, null);
        Survey sv = Survey.Of(sites, exe);
        if (sv.Patched == 0 && !FrenchPatch.LooksEnglishTable(exe, FrTable.Bytes)) { Log(L.T("Table des caractères inattendue dans cet exe.", "Unexpected character table in this exe."), LogKind.Error); return false; }
        foreach (Site x in sv.Unexpected)
            Log(string.Format(L.T("  {0} : déjà modifié par un autre patch ({1})", "  {0}: already changed by another patch ({1})"), x.Name, x.Required ? L.T("bloquant", "blocking") : L.T("sauté", "skipped")), x.Required ? LogKind.Error : LogKind.Warn);
        if (!sv.Patchable) { Log(L.T("Patch FR impossible : rien n'a été écrit.", "French patch not possible: nothing was written."), LogKind.Error); return false; }
        if (sv.Done) { Log(L.T("Patch FR déjà appliqué.", "French patch already applied."), LogKind.Ok); return true; }
        Log(string.Format(L.T("Patch FR : {0} endroits à modifier.", "French patch: {0} places to change."), sv.Original), LogKind.Info);
        if (dryRun) return true;

        byte[] outb = (byte[])exe.Clone();
        foreach (Site x in sites) if (x.StateIn(exe) == SiteState.Original) Array.Copy(x.Patched, 0, outb, x.Offset, x.Patched.Length);
        if (!Survey.Of(sites, outb).Done) throw new Exception(L.T("vérification après patch", "check after patching"));
        if (!Write(path, outb)) return false;
        Log(L.T("Exe patché en français : accents, hauteur des lignes, boutons et textes du menu. Il faut aussi les textes et polices françaises du jeu (Zeus_Text.eng, Zeus_MM.eng, Model\\Zeus eventmsg.txt, DATA\\Zeus_Fonts.*).",
                "Exe patched to French: accents, line heights, buttons and menu texts. The game's French texts and fonts are also needed (Zeus_Text.eng, Zeus_MM.eng, Model\\Zeus eventmsg.txt, DATA\\Zeus_Fonts.*)."), LogKind.Ok);
        return true;
    }

    // ---- editeur

    public static bool PatchEditor(string path, int w, int h, MapLayout layout, bool dryRun)
    {
        byte[] exe = Load(path);
        if (exe == null) return false;
        if (BuildOf(exe) == null) { Log(L.T("Exe non reconnu : fonction de placement des cités introuvable.", "Exe not recognised: city placement function not found."), LogKind.Error); return false; }
        int[] lim = EditorPatch.Limits(w, h, layout);
        Log(string.Format(L.T("Éditeur : mappemonde {0} x {1}, limites autour du centre : gauche {2}, droite {3}, haut {4}, bas {5}.",
            "Editor: {0} x {1} world map, limits around the centre: left {2}, right {3}, top {4}, bottom {5}."), w, h, lim[0], lim[1], lim[2], lim[3]), LogKind.Info);
        List<Site> sites = EditorPatch.Build(exe, lim);
        Survey sv = Survey.Of(sites, exe);
        foreach (Site x in sv.Unexpected) Log(string.Format(L.T("  {0} : déjà modifié par un autre patch", "  {0}: already changed by another patch"), x.Name), LogKind.Error);
        if (!sv.Patchable) { Log(L.T("Patch de l'éditeur impossible : rien n'a été écrit.", "Editor patch not possible: nothing was written."), LogKind.Error); return false; }
        if (sv.Original == 0) { Log(L.T("Éditeur déjà patché avec ces limites.", "Editor already patched with these limits."), LogKind.Ok); return true; }
        if (dryRun) return true;
        byte[] outb = (byte[])exe.Clone();
        foreach (Site x in sites) Array.Copy(x.Patched, 0, outb, x.Offset, x.Patched.Length);
        if (!Write(path, outb)) return false;
        Log(L.T("Éditeur patché : les cités se placent sur toute la carte peinte de la mappemonde.", "Editor patched: cities can be placed anywhere on the painted map."), LogKind.Ok);
        return true;
    }

    // ---- restauration

    public static bool Restore(string path)
    {
        string bak = path + ".bak";
        if (!File.Exists(bak)) { Log(L.T("Pas de copie « .bak » à côté de cet exe.", "No \".bak\" copy next to this exe."), LogKind.Error); return false; }
        Writable(path);
        File.Copy(bak, path, true);
        Log(L.T("Exe d'origine remis en place (depuis la copie .bak).", "Original exe put back (from the .bak copy)."), LogKind.Ok);
        return true;
    }

    static void Writable(string path)
    {
        if (File.Exists(path)) { FileAttributes a = File.GetAttributes(path); if ((a & FileAttributes.ReadOnly) != 0) File.SetAttributes(path, a & ~FileAttributes.ReadOnly); }
    }

    static bool Write(string path, byte[] b)
    {
        string tmp = path + ".tmp";
        try
        {
            if (!File.Exists(path + ".bak")) { File.Copy(path, path + ".bak"); Log(L.T("Copie de l'original : ", "Copy of the original: ") + Path.GetFileName(path) + ".bak", LogKind.Info); }
            Writable(path);
            File.WriteAllBytes(tmp, b);
            File.Copy(tmp, path, true);
            File.Delete(tmp);
            return true;
        }
        catch (Exception e)
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
            Log(L.T("Impossible d'écrire l'exe (jeu ouvert ? dossier protégé ?) : ", "Could not write the exe (game running? protected folder?): ") + e.Message, LogKind.Error);
            return false;
        }
    }
}
