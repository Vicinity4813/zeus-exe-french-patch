// Patcheur Zeus : patch FR de l'exe anglais GOG, avec les textes et polices de l'installation francaise de l'utilisateur.
// Fenetre par defaut ; en ligne de commande :
//   Patcheur.exe --verifier|--patcher|--annuler --exe <Zeus.exe> [--fr <dossier du jeu francais>] [--sans-exe] [--sans-textes] [--menus] [--voix]
//                [--editeur gauche|centre|droite]
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Patcheur Zeus")]
[assembly: System.Reflection.AssemblyDescription("Patch FR de Zeus : Le Maitre de l'Olympe (exe anglais GOG)")]
[assembly: System.Reflection.AssemblyCompany("Outils Zeus (projet amateur)")]
[assembly: System.Reflection.AssemblyProduct("Patcheur Zeus")]
[assembly: System.Reflection.AssemblyCopyright("© 2026 Vicinity4813 - MIT License")]
[assembly: System.Resources.NeutralResourcesLanguage("fr-FR")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Reflection.AssemblyVersion("0.1.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("0.1.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersion("0.1")]

static class Program
{
    [DllImport("kernel32.dll")] static extern bool AttachConsole(int pid);

    [STAThread]
    static int Main(string[] args)
    {
        L.Load();
        if (args.Length > 0) { AttachConsole(-1); return Cli(args); }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
        return 0;
    }

    static int Cli(string[] args)
    {
        string mode = null; Engine e = new Engine(); Options o = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            if (a == "--verifier" || a == "--patcher" || a == "--annuler") mode = a;
            else if (a == "--exe" && i + 1 < args.Length) e.ExePath = Path.GetFullPath(args[++i]);
            else if (a == "--fr" && i + 1 < args.Length) e.FrDir = Path.GetFullPath(args[++i]);
            else if (a == "--sans-exe") o.Exe = false;
            else if (a == "--sans-textes") o.Texts = false;
            else if (a == "--menus") o.Menus = true;
            else if (a == "--voix") o.Voices = true;
            else if (a == "--editeur") { o.Editor = true; if (i + 1 < args.Length && !args[i + 1].StartsWith("--")) { string p = args[++i]; o.Layout = p.StartsWith("c") ? MapLayout.Center : p.StartsWith("d") || p.StartsWith("r") ? MapLayout.Right : MapLayout.Left; } }
            else if (a == "--en") L.En = true;
        }
        if (mode == null || e.ExePath == null) { Console.WriteLine("Patcheur.exe --verifier|--patcher|--annuler --exe <Zeus.exe> [--fr <dossier>] [--sans-exe] [--sans-textes] [--menus] [--voix] [--editeur gauche|centre|droite]"); return 2; }
        bool ok = true;
        e.Log = (m, k) => { Console.WriteLine((k == LogKind.Error ? "ERREUR " : k == LogKind.Warn ? "attention " : "") + m); if (k == LogKind.Error) ok = false; };
        try
        {
            if (mode == "--annuler") e.Restore();
            else e.Patch(o, mode == "--verifier");
        }
        catch (Exception ex) { Console.WriteLine("ERREUR " + ex.Message); return 1; }
        return ok ? 0 : 1;
    }
}

class MainForm : Form
{
    ComboBox lang;
    Label title, intro, lExe, lFr, lOpt;
    TextBox tExe, tFr;
    Button bExe, bFr, bCheck, bPatch, bUndo;
    CheckBox cExe, cTexts, cMenus, cVoices, cEditor;
    RadioButton rLeft, rCenter, rRight;
    RichTextBox log;
    bool busy;

    public MainForm()
    {
        Font = new Font("Segoe UI", 9.5f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(760, 680);
        MinimumSize = new Size(640, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 238, 226);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        title = new Label { Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(90, 50, 20), AutoSize = true, Location = new Point(16, 12) };
        lang = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lang.Items.AddRange(new object[] { "Français", "English" });
        lang.SelectedIndex = L.En ? 1 : 0;
        lang.Location = new Point(ClientSize.Width - lang.Width - 16, 14);
        lang.SelectedIndexChanged += delegate { L.En = lang.SelectedIndex == 1; L.Save(); ApplyLanguage(); };

        intro = new Label { Location = new Point(16, 52), Size = new Size(728, 64), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lExe = new Label { Location = new Point(16, 122), AutoSize = true };
        tExe = new TextBox { Location = new Point(16, 142), Width = 620, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        bExe = new Button { Location = new Point(644, 140), Size = new Size(100, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lFr = new Label { Location = new Point(16, 176), AutoSize = true };
        tFr = new TextBox { Location = new Point(16, 196), Width = 620, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        bFr = new Button { Location = new Point(644, 194), Size = new Size(100, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lOpt = new Label { Location = new Point(16, 232), AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        cExe = new CheckBox { Location = new Point(24, 254), AutoSize = true, Checked = true };
        cTexts = new CheckBox { Location = new Point(24, 278), AutoSize = true, Checked = true };
        cMenus = new CheckBox { Location = new Point(24, 302), AutoSize = true };
        cVoices = new CheckBox { Location = new Point(24, 326), AutoSize = true };
        cEditor = new CheckBox { Location = new Point(24, 350), AutoSize = true };
        rLeft = new RadioButton { Location = new Point(48, 376), AutoSize = true, Checked = true };
        rCenter = new RadioButton { Location = new Point(290, 376), AutoSize = true };
        rRight = new RadioButton { Location = new Point(450, 376), AutoSize = true };
        cEditor.CheckedChanged += delegate { rLeft.Enabled = rCenter.Enabled = rRight.Enabled = cEditor.Checked; };
        rLeft.Enabled = rCenter.Enabled = rRight.Enabled = false;
        bCheck = new Button { Location = new Point(16, 412), Size = new Size(150, 34) };
        bPatch = new Button { Location = new Point(176, 412), Size = new Size(200, 34), Font = new Font(Font, FontStyle.Bold) };
        bUndo = new Button { Location = new Point(386, 412), Size = new Size(200, 34) };
        log = new RichTextBox { Location = new Point(16, 458), Size = new Size(728, 206), ReadOnly = true, BackColor = Color.White, Font = new Font("Consolas", 9f),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };

        bExe.Click += delegate
        {
            using (OpenFileDialog d = new OpenFileDialog { Filter = "Zeus.exe|*.exe", FileName = "Zeus.exe" })
                if (d.ShowDialog(this) == DialogResult.OK) { tExe.Text = d.FileName; Check(); }
        };
        bFr.Click += delegate
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog())
            {
                d.Description = L.T("Dossier du jeu en français (celui qui contient son Zeus.exe et Zeus_Text.eng)", "Folder of the French game (the one with its Zeus.exe and Zeus_Text.eng)");
                if (d.ShowDialog(this) == DialogResult.OK) { tFr.Text = d.SelectedPath; Check(); }
            }
        };
        bCheck.Click += delegate { Check(); };
        bPatch.Click += delegate
        {
            if (MessageBox.Show(this, L.T("Patcher maintenant ? Les fichiers remplacés sont sauvegardés et le patch peut être annulé.",
                                          "Patch now? Replaced files are backed up and the patch can be undone."), Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Run(e => e.Patch(CurrentOptions(), false));
        };
        bUndo.Click += delegate
        {
            if (MessageBox.Show(this, L.T("Annuler le patch FR et remettre les fichiers d'origine ?", "Undo the French patch and put the original files back?"), Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Run(e => e.Restore());
        };

        Controls.AddRange(new Control[] { title, lang, intro, lExe, tExe, bExe, lFr, tFr, bFr, lOpt, cExe, cTexts, cMenus, cVoices, cEditor, rLeft, rCenter, rRight, bCheck, bPatch, bUndo, log });
        ApplyLanguage();
    }

    void ApplyLanguage()
    {
        Text = L.T("Patcheur Zeus - patch FR", "Zeus Patcher - French patch");
        title.Text = L.T("Patch FR de Zeus (version anglaise GOG)", "French patch for Zeus (English GOG version)");
        intro.Text = L.T("Met en français la version anglaise GOG : accents, boutons et textes du jeu. Les textes, polices et images viennent de VOTRE jeu en français (CD ou installation) : rien n'est fourni par ce programme. Fonctionne aussi sur un exe déjà passé au Resolution Customiser. Tout est sauvegardé et peut être annulé.",
                         "Turns the English GOG version into French: accents, buttons and game texts. Texts, fonts and pictures come from YOUR French game (CD or install): this program provides none of them. Also works on an exe already patched by the Resolution Customiser. Everything is backed up and can be undone.");
        lExe.Text = L.T("Zeus.exe à patcher (version anglaise GOG) :", "Zeus.exe to patch (English GOG version):");
        lFr.Text = L.T("Dossier de votre jeu en français :", "Folder of your French game:");
        bExe.Text = bFr.Text = L.T("Parcourir…", "Browse…");
        lOpt.Text = L.T("À faire :", "To do:");
        cExe.Text = L.T("Exe : affichage du français (accents, hauteur des lignes, boutons)", "Exe: French display (accents, line heights, buttons)");
        cTexts.Text = L.T("Textes et polices françaises (indispensable pour les accents)", "French texts and fonts (needed for the accents)");
        cMenus.Text = L.T("Images des menus en français", "French menu pictures");
        cVoices.Text = L.T("Voix, sons et vidéos en français (environ 340 Mo)", "French voices, sounds and videos (about 340 MB)");
        cEditor.Text = L.T(@"Éditeur : placer les cités sur toute la mappemonde installée (DATA\Zeus_MapOfGreece01.jpg)", @"Editor: place cities anywhere on the installed world map (DATA\Zeus_MapOfGreece01.jpg)");
        rLeft.Text = L.T("Carte calée à gauche (origine)", "Map on the left (original)");
        rCenter.Text = L.T("Carte au centre", "Map centred");
        rRight.Text = L.T("Carte calée à droite", "Map on the right");
        bCheck.Text = L.T("Vérifier", "Check");
        bPatch.Text = L.T("Patcher", "Patch");
        bUndo.Text = L.T("Annuler le patch", "Undo the patch");
    }

    Options CurrentOptions()
    {
        return new Options { Exe = cExe.Checked, Texts = cTexts.Checked, Menus = cMenus.Checked, Voices = cVoices.Checked, Editor = cEditor.Checked,
            Layout = rCenter.Checked ? MapLayout.Center : rRight.Checked ? MapLayout.Right : MapLayout.Left };
    }

    void Check() { if (tExe.Text.Trim().Length > 0) Run(e => e.Patch(CurrentOptions(), true)); }

    void Run(Action<Engine> work)
    {
        if (busy) return;
        busy = true;
        log.Clear();
        foreach (Control c in new Control[] { bCheck, bPatch, bUndo, bExe, bFr }) c.Enabled = false;
        Engine e = new Engine { ExePath = tExe.Text.Trim(), FrDir = tFr.Text.Trim() };
        e.Log = (m, k) => BeginInvoke((Action)(() => Append(m, k)));
        Thread t = new Thread(() =>
        {
            try { work(e); }
            catch (Exception ex) { BeginInvoke((Action)(() => Append(L.T("Erreur : ", "Error: ") + ex.Message, LogKind.Error))); }
            BeginInvoke((Action)(() => { busy = false; foreach (Control c in new Control[] { bCheck, bPatch, bUndo, bExe, bFr }) c.Enabled = true; }));
        });
        t.IsBackground = true;
        t.Start();
    }

    void Append(string m, LogKind k)
    {
        log.SelectionStart = log.TextLength;
        log.SelectionColor = k == LogKind.Error ? Color.FromArgb(180, 20, 20) : k == LogKind.Warn ? Color.FromArgb(170, 100, 0) : k == LogKind.Ok ? Color.FromArgb(20, 120, 40) : Color.Black;
        log.AppendText(m + Environment.NewLine);
        log.ScrollToCaret();
    }
}
