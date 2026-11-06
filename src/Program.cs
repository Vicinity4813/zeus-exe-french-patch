// Zeus - EXE French Patch : met en francais un Zeus.exe anglais (accents, polices, boutons, fenetres), quelle que soit sa
// resolution, et copie les fichiers d'un jeu francais installe. (Le patch de l'editeur de scenarios est dans Zeus World Map Converter.)
// En ligne de commande : "Zeus EXE French Patch.exe" --fr <Zeus.exe> | --restaurer <Zeus.exe> | --info <Zeus.exe>
//   | --importer <Zeus.exe> <dossier du jeu francais> | --restaurer-fichiers <Zeus.exe>   (--verifier : sans rien ecrire, pour --fr et --importer)
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Zeus - EXE French Patch")]
[assembly: System.Reflection.AssemblyDescription("Patch FR d'un Zeus.exe anglais (Zeus : Le Maitre de l'Olympe)")]
[assembly: System.Reflection.AssemblyCompany("Outils Zeus (projet amateur)")]
[assembly: System.Reflection.AssemblyProduct("Zeus - EXE French Patch")]
[assembly: System.Reflection.AssemblyCopyright("© 2026 Vicinity4813 - MIT License")]
[assembly: System.Resources.NeutralResourcesLanguage("fr-FR")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Reflection.AssemblyVersion("1.1.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.1.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersion("1.1")]

static class Program
{
    [DllImport("kernel32.dll")] static extern bool AttachConsole(int pid);

    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length > 0) { AttachConsole(-1); return Cli(args); }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
        return 0;
    }

    static int[] Pair(string s) { string[] p = s.ToLowerInvariant().Split('x'); return new[] { int.Parse(p[0]), int.Parse(p[1]) }; }

    static int Cli(string[] a)
    {
        string mode = null, exe = null, source = null; bool dry = false;
        for (int i = 0; i < a.Length; i++)
        {
            if ((a[i] == "--fr" || a[i] == "--restaurer" || a[i] == "--info" || a[i] == "--restaurer-fichiers") && i + 1 < a.Length) { mode = a[i]; exe = Path.GetFullPath(a[++i]); }
            else if (a[i] == "--importer" && i + 2 < a.Length) { mode = a[i]; exe = Path.GetFullPath(a[++i]); source = a[++i]; }
            else if (a[i] == "--verifier") dry = true;
        }
        if (mode == null) { Console.WriteLine("\"Zeus EXE French Patch.exe\" --fr <Zeus.exe> | --restaurer <Zeus.exe> | --info <Zeus.exe> | --importer <Zeus.exe> <dossier du jeu français> | --restaurer-fichiers <Zeus.exe> [--verifier]"); return 2; }
        bool err = false;
        Core.Log = (m, k) => { Console.WriteLine((k == LogKind.Error ? "ERREUR " : k == LogKind.Warn ? "attention " : "") + m); if (k == LogKind.Error) err = true; };
        try
        {
            if (mode == "--info") Console.WriteLine(MainForm.Describe(exe));
            else if (mode == "--fr") Core.PatchFr(exe, dry);
            else if (mode == "--restaurer") Core.Restore(exe);
            else if (mode == "--importer") FrenchFiles.Import(exe, source, dry);
            else if (mode == "--restaurer-fichiers") FrenchFiles.RestoreFiles(exe);
        }
        catch (Exception ex) { Console.WriteLine("ERREUR " + ex.Message); return 1; }
        return err ? 1 : 0;
    }
}

class MainForm : Form
{
    Label title, intro, lExe, lInfo, lFrText, lFilesText;
    TextBox tExe;
    Button bExe, bFr, bRestore, bImport, bRestoreFiles;
    GroupBox gFr, gFiles;
    RichTextBox log;
    bool busy;

    public MainForm()
    {
        Font = new Font("Segoe UI", 9.5f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(760, 632);
        MinimumSize = new Size(680, 562);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 238, 226);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        title = new Label { Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(90, 50, 20), AutoSize = true, Location = new Point(16, 12) };

        intro = new Label { Location = new Point(16, 52), Size = new Size(728, 44), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lExe = new Label { Location = new Point(16, 100), AutoSize = true };
        tExe = new TextBox { Location = new Point(16, 120), Width = 620, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        bExe = new Button { Location = new Point(644, 118), Size = new Size(100, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lInfo = new Label { Location = new Point(16, 150), Size = new Size(728, 22), ForeColor = Color.FromArgb(90, 50, 20), Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

        gFr = new GroupBox { Location = new Point(16, 178), Size = new Size(728, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lFrText = new Label { Location = new Point(12, 22), Size = new Size(500, 64) };
        bFr = new Button { Location = new Point(524, 34), Size = new Size(190, 36), Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        gFr.Controls.AddRange(new Control[] { lFrText, bFr });

        gFiles = new GroupBox { Location = new Point(16, 282), Size = new Size(728, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lFilesText = new Label { Location = new Point(12, 22), Size = new Size(500, 64) };
        bImport = new Button { Location = new Point(524, 20), Size = new Size(190, 32), Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        bRestoreFiles = new Button { Location = new Point(524, 56), Size = new Size(190, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        gFiles.Controls.AddRange(new Control[] { lFilesText, bImport, bRestoreFiles });

        bRestore = new Button { Location = new Point(16, 392), Size = new Size(260, 32) };
        log = new RichTextBox { Location = new Point(16, 434), Size = new Size(728, 182), ReadOnly = true, BackColor = Color.White, Font = new Font("Consolas", 9f),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };

        bExe.Click += delegate
        {
            using (OpenFileDialog d = new OpenFileDialog { Filter = "Zeus.exe|*.exe" })
                if (d.ShowDialog(this) == DialogResult.OK) { tExe.Text = d.FileName; ShowInfo(); }
        };
        tExe.Leave += delegate { ShowInfo(); };
        bFr.Click += delegate { Run(p => Core.PatchFr(p, false)); };
        bRestore.Click += delegate
        {
            if (MessageBox.Show(this, L.T("Remettre l'exe d'origine (copie .bak) ?", "Put the original exe back (.bak copy)?"), Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Run(p => Core.Restore(p));
        };
        bImport.Click += delegate
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog { Description = L.T("Dossier de votre jeu français installé (celui qui contient Zeus_Text.eng)", "Your installed French game folder (the one with Zeus_Text.eng)"), ShowNewFolderButton = false })
                if (d.ShowDialog(this) == DialogResult.OK) { string src = d.SelectedPath; Run(p => FrenchFiles.Import(p, src, false)); }
        };
        bRestoreFiles.Click += delegate
        {
            if (MessageBox.Show(this, L.T("Retirer les fichiers français et remettre les fichiers anglais ?", "Remove the French files and put the English files back?"), Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Run(p => FrenchFiles.RestoreFiles(p));
        };

        Controls.AddRange(new Control[] { title, intro, lExe, tExe, bExe, lInfo, gFr, gFiles, bRestore, log });
        ApplyLanguage();
        ShowInfo();
    }

    // Description courte d'un exe : compilation et resolution
    public static string Describe(string path)
    {
        if (!File.Exists(path)) return "";
        byte[] b = File.ReadAllBytes(path);
        string build = Core.BuildOf(b);
        if (build == null) return L.T("Exe non reconnu (autre compilation : russe, CD d'origine protégé…)", "Exe not recognised (other build: Russian, protected original CD…)");
        int[] r = Core.ResolutionOf(b);
        string res = r == null ? "?" : r[0] + " x " + r[1];
        bool en = build != "français";
        return (en ? L.T("Exe anglais 2.1.4.0", "English exe 2.1.4.0") : L.T("Exe français", "French exe")) + " — " + res
            + (File.Exists(path + ".bak") ? L.T("  (copie .bak présente)", "  (.bak copy present)") : "");
    }

    void ShowInfo()
    {
        string p = tExe.Text.Trim();
        lInfo.Text = Describe(p);
        bFr.Enabled = bImport.Enabled = bRestoreFiles.Enabled = false;
        if (!File.Exists(p)) return;
        byte[] b = File.ReadAllBytes(p);
        string build = Core.BuildOf(b);
        bFr.Enabled = bImport.Enabled = build != null && build != "français";
        bRestoreFiles.Enabled = FrenchFiles.HasBackup(p);
    }

    void ApplyLanguage()
    {
        Text = "Zeus - EXE French Patch";
        title.Text = "Zeus - EXE French Patch";
        intro.Text = L.T("Met en français un jeu anglais à partir d'un Zeus.exe anglais 2.1.4.0 (GOG, Steam, versions widescreen), quelle que soit la résolution. Chaque étape s'annule : l'original de l'exe et les fichiers anglais sont gardés à côté.",
                         "Turns an English game into French from an English 2.1.4.0 Zeus.exe (GOG, Steam, widescreen versions), at any resolution. Each step can be undone: the original exe and the English files are kept next to the game.");
        lExe.Text = L.T("Zeus.exe à patcher :", "Zeus.exe to patch:");
        bExe.Text = L.T("Parcourir…", "Browse…");
        gFr.Text = L.T("Patch FR (exe anglais)", "French patch (English exe) - French interface only");
        lFrText.Text = L.T("Accents, hauteur des lignes, polices, boutons, textes du menu et fenêtres élargies (Finances, Culture, messages, didacticiels), comme l'édition française. Seul Zeus.exe est modifié, une copie « .bak » est faite à côté.",
                           "Accents, line heights, fonts, buttons, menu texts and widened windows (Finances, Culture, messages, tutorials), as in the French edition. Only Zeus.exe is changed, a \".bak\" copy is made next to it.");
        bFr.Text = L.T("Patcher en français", "Patch to French");
        gFiles.Text = L.T("Fichiers français (depuis votre jeu français installé)", "French files (from your installed French game)");
        lFilesText.Text = L.T("Copie les textes, polices, voix, campagnes et tutoriels de votre jeu français dans le dossier de ce Zeus.exe (500 Mo environ). Votre jeu français n'est que lu ; les fichiers anglais vont dans « " + FrenchFiles.BackupFolder + " ».",
                              "Copies the texts, fonts, voices, campaigns and tutorials of your French game into this Zeus.exe's folder (about 500 MB). Your French game is only read; the English files go to \"" + FrenchFiles.BackupFolder + "\".");
        bImport.Text = L.T("Importer le jeu français…", "Import the French game…");
        bRestoreFiles.Text = L.T("Remettre les fichiers anglais", "Put the English files back");
        bRestore.Text = L.T("Remettre l'exe d'origine (.bak)", "Put the original exe back (.bak)");
    }

    void Run(Func<string, bool> work)
    {
        if (busy) return;
        string p = tExe.Text.Trim();
        if (p.Length == 0) return;
        busy = true;
        log.Clear();
        foreach (Control c in new Control[] { bFr, bRestore, bExe, bImport, bRestoreFiles }) c.Enabled = false;
        Core.Log = (m, k) => BeginInvoke((Action)(() =>
        {
            log.SelectionStart = log.TextLength;
            log.SelectionColor = k == LogKind.Error ? Color.FromArgb(180, 20, 20) : k == LogKind.Warn ? Color.FromArgb(170, 100, 0) : k == LogKind.Ok ? Color.FromArgb(20, 120, 40) : Color.Black;
            log.AppendText(m + Environment.NewLine);
            log.ScrollToCaret();
        }));
        Thread t = new Thread(() =>
        {
            try { work(p); }
            catch (Exception ex) { Core.Log(L.T("Erreur : ", "Error: ") + ex.Message, LogKind.Error); }
            BeginInvoke((Action)(() => { busy = false; bRestore.Enabled = bExe.Enabled = true; ShowInfo(); }));
        });
        t.IsBackground = true;
        t.Start();
    }
}
