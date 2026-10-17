// Patcheur Zeus : met en francais un Zeus.exe anglais (accents, boutons) et adapte l'editeur de mappemonde
// a des mappemondes plus grandes, sur un exe anglais ou francais de n'importe quelle resolution.
// En ligne de commande : Patcheur.exe --fr <Zeus.exe> | --editeur <Zeus.exe> [--taille LxH] [--position gauche|centre|droite]
//                        | --restaurer <Zeus.exe> | --info <Zeus.exe>   (--verifier : sans rien ecrire)
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Patcheur Zeus")]
[assembly: System.Reflection.AssemblyDescription("Patch FR et editeur de mappemonde pour Zeus : Le Maitre de l'Olympe")]
[assembly: System.Reflection.AssemblyCompany("Outils Zeus (projet amateur)")]
[assembly: System.Reflection.AssemblyProduct("Patcheur Zeus")]
[assembly: System.Reflection.AssemblyCopyright("© 2026 Vicinity4813 - MIT License")]
[assembly: System.Resources.NeutralResourcesLanguage("fr-FR")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersion("1.0")]

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

    static int[] Pair(string s) { string[] p = s.ToLowerInvariant().Split('x'); return new[] { int.Parse(p[0]), int.Parse(p[1]) }; }

    static int Cli(string[] a)
    {
        string mode = null, exe = null; int[] size = null; MapLayout layout = MapLayout.Left; bool dry = false;
        for (int i = 0; i < a.Length; i++)
        {
            if ((a[i] == "--fr" || a[i] == "--editeur" || a[i] == "--restaurer" || a[i] == "--info") && i + 1 < a.Length) { mode = a[i]; exe = Path.GetFullPath(a[++i]); }
            else if (a[i] == "--taille" && i + 1 < a.Length) size = Pair(a[++i]);
            else if (a[i] == "--position" && i + 1 < a.Length) { string p = a[++i]; layout = p.StartsWith("c") ? MapLayout.Center : p.StartsWith("d") || p.StartsWith("r") ? MapLayout.Right : MapLayout.Left; }
            else if (a[i] == "--verifier") dry = true;
            else if (a[i] == "--en") L.En = true;
        }
        if (mode == null) { Console.WriteLine("Patcheur.exe --fr <Zeus.exe> | --editeur <Zeus.exe> [--taille LxH] [--position gauche|centre|droite] | --restaurer <Zeus.exe> | --info <Zeus.exe> [--verifier]"); return 2; }
        bool err = false;
        Core.Log = (m, k) => { Console.WriteLine((k == LogKind.Error ? "ERREUR " : k == LogKind.Warn ? "attention " : "") + m); if (k == LogKind.Error) err = true; };
        try
        {
            if (mode == "--info") Console.WriteLine(MainForm.Describe(exe));
            else if (mode == "--fr") Core.PatchFr(exe, dry);
            else if (mode == "--restaurer") Core.Restore(exe);
            else
            {
                if (size == null)
                {
                    int[] r = Core.ResolutionOf(File.ReadAllBytes(exe));
                    if (r == null) { Console.WriteLine("ERREUR résolution introuvable : indiquez --taille"); return 1; }
                    size = Core.ViewOf(r[0], r[1]);
                }
                Core.PatchEditor(exe, size[0], size[1], layout, dry);
            }
        }
        catch (Exception ex) { Console.WriteLine("ERREUR " + ex.Message); return 1; }
        return err ? 1 : 0;
    }
}

class MainForm : Form
{
    ComboBox lang;
    Label title, intro, lExe, lInfo, lFrText, lSize, lX, lPos;
    TextBox tExe;
    Button bExe, bFr, bEditor, bRestore;
    GroupBox gFr, gEd;
    NumericUpDown nW, nH;
    RadioButton rLeft, rCenter, rRight;
    RichTextBox log;
    bool busy;

    public MainForm()
    {
        Font = new Font("Segoe UI", 9.5f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(760, 640);
        MinimumSize = new Size(680, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 238, 226);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        title = new Label { Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(90, 50, 20), AutoSize = true, Location = new Point(16, 12) };
        lang = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lang.Items.AddRange(new object[] { "Français", "English" });
        lang.SelectedIndex = L.En ? 1 : 0;
        lang.Location = new Point(ClientSize.Width - lang.Width - 16, 14);
        lang.SelectedIndexChanged += delegate { L.En = lang.SelectedIndex == 1; L.Save(); ApplyLanguage(); ShowInfo(); };

        intro = new Label { Location = new Point(16, 52), Size = new Size(728, 44), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lExe = new Label { Location = new Point(16, 100), AutoSize = true };
        tExe = new TextBox { Location = new Point(16, 120), Width = 620, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        bExe = new Button { Location = new Point(644, 118), Size = new Size(100, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lInfo = new Label { Location = new Point(16, 150), Size = new Size(728, 22), ForeColor = Color.FromArgb(90, 50, 20), Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

        gFr = new GroupBox { Location = new Point(16, 178), Size = new Size(728, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lFrText = new Label { Location = new Point(12, 22), Size = new Size(500, 64) };
        bFr = new Button { Location = new Point(524, 34), Size = new Size(190, 36), Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        gFr.Controls.AddRange(new Control[] { lFrText, bFr });

        gEd = new GroupBox { Location = new Point(16, 282), Size = new Size(728, 110), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        lSize = new Label { Location = new Point(12, 26), AutoSize = true };
        nW = new NumericUpDown { Location = new Point(12, 48), Width = 80, Minimum = 838, Maximum = 7680, Value = 838 };
        lX = new Label { Location = new Point(96, 51), AutoSize = true, Text = "x" };
        nH = new NumericUpDown { Location = new Point(112, 48), Width = 80, Minimum = 600, Maximum = 4320, Value = 738 };
        lPos = new Label { Location = new Point(214, 26), AutoSize = true };
        rLeft = new RadioButton { Location = new Point(214, 48), AutoSize = true, Checked = true };
        rCenter = new RadioButton { Location = new Point(214, 72), AutoSize = true };
        rRight = new RadioButton { Location = new Point(360, 72), AutoSize = true };
        bEditor = new Button { Location = new Point(524, 40), Size = new Size(190, 36), Font = new Font(Font, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        gEd.Controls.AddRange(new Control[] { lSize, nW, lX, nH, lPos, rLeft, rCenter, rRight, bEditor });

        bRestore = new Button { Location = new Point(16, 402), Size = new Size(260, 32) };
        log = new RichTextBox { Location = new Point(16, 444), Size = new Size(728, 180), ReadOnly = true, BackColor = Color.White, Font = new Font("Consolas", 9f),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };

        bExe.Click += delegate
        {
            using (OpenFileDialog d = new OpenFileDialog { Filter = "Zeus.exe|*.exe" })
                if (d.ShowDialog(this) == DialogResult.OK) { tExe.Text = d.FileName; ShowInfo(); }
        };
        tExe.Leave += delegate { ShowInfo(); };
        bFr.Click += delegate { Run(p => Core.PatchFr(p, false)); };
        bEditor.Click += delegate
        {
            int w = (int)nW.Value, h = (int)nH.Value;
            MapLayout lay = rCenter.Checked ? MapLayout.Center : rRight.Checked ? MapLayout.Right : MapLayout.Left;
            Run(p => Core.PatchEditor(p, w, h, lay, false));
        };
        bRestore.Click += delegate
        {
            if (MessageBox.Show(this, L.T("Remettre l'exe d'origine (copie .bak) ?", "Put the original exe back (.bak copy)?"), Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Run(p => Core.Restore(p));
        };

        Controls.AddRange(new Control[] { title, lang, intro, lExe, tExe, bExe, lInfo, gFr, gEd, bRestore, log });
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
        if (!File.Exists(p)) return;
        byte[] b = File.ReadAllBytes(p);
        string build = Core.BuildOf(b);
        bFr.Enabled = build != null && build != "français";
        bEditor.Enabled = build != null;
        int[] r = Core.ResolutionOf(b);
        if (r != null)
        {
            int[] v = Core.ViewOf(r[0], r[1]);
            nW.Value = Math.Max(nW.Minimum, Math.Min(nW.Maximum, v[0]));
            nH.Value = Math.Max(nH.Minimum, Math.Min(nH.Maximum, v[1]));
        }
    }

    void ApplyLanguage()
    {
        Text = L.T("Patcheur Zeus", "Zeus Patcher");
        title.Text = L.T("Patcheur Zeus : français et éditeur", "Zeus Patcher: French and editor");
        intro.Text = L.T("Modifie seulement le Zeus.exe choisi (une copie « .bak » de l'original est faite à côté). Exe anglais 2.1.4.0 (GOG, Steam, versions widescreen) ou exe français, quelle que soit la résolution.",
                         "Only changes the chosen Zeus.exe (a \".bak\" copy of the original is made next to it). English 2.1.4.0 exe (GOG, Steam, widescreen versions) or French exe, at any resolution.");
        lExe.Text = L.T("Zeus.exe à patcher :", "Zeus.exe to patch:");
        bExe.Text = L.T("Parcourir…", "Browse…");
        gFr.Text = L.T("1. Patch FR (exe anglais)", "1. French patch (English exe)");
        lFrText.Text = L.T("Accents (table des caractères), hauteur des lignes, boutons élargis et textes du menu, comme l'édition française. Le jeu doit avoir les textes et polices françaises (Zeus_Text.eng, Zeus_MM.eng, Zeus eventmsg.txt, Zeus_Fonts).",
                           "Accents (character table), line heights, widened buttons and menu texts, as in the French edition. The game needs the French texts and fonts (Zeus_Text.eng, Zeus_MM.eng, Zeus eventmsg.txt, Zeus_Fonts).");
        bFr.Text = L.T("Patcher en français", "Patch to French");
        gEd.Text = L.T("2. Éditeur de mappemonde (exe anglais ou français)", "2. World map editor (English or French exe)");
        lSize.Text = L.T("Taille de la mappemonde :", "World map size:");
        lPos.Text = L.T("Carte dans la mappemonde :", "Map in the world map:");
        rLeft.Text = L.T("À gauche (comme l'originale)", "Left (like the original)");
        rCenter.Text = L.T("Au centre", "Centred");
        rRight.Text = L.T("À droite", "Right");
        bEditor.Text = L.T("Patcher l'éditeur", "Patch the editor");
        bRestore.Text = L.T("Remettre l'exe d'origine (.bak)", "Put the original exe back (.bak)");
    }

    void Run(Func<string, bool> work)
    {
        if (busy) return;
        string p = tExe.Text.Trim();
        if (p.Length == 0) return;
        busy = true;
        log.Clear();
        foreach (Control c in new Control[] { bFr, bEditor, bRestore, bExe }) c.Enabled = false;
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
