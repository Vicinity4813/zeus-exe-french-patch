// Patch FR : affichage du texte francais avec l'exe anglais GOG (v2.1.4.0).
// D'apres la documentation de zeus-poseidon-arm64 (nic0lasm, GPL-2.0) : docs/reverse-engineering.md et
// src/win32/language.cpp, plus nos relevés dans l'edition francaise. Les corrections sont ecrites dans l'exe :
//   1. table caractere -> glyphe (224 octets) : reprise de l'exe francais de l'utilisateur, jamais distribuee ;
//   2. hauteur de ligne de chaque police (textes d'une ligne, et textes des panneaux) ;
//   3. suppression des exceptions polonaises ;
//   4. boutons elargis ou deplaces comme dans l'edition francaise ;
//   5. positions de quelques textes du menu ecrites dans le code.
using System;
using System.Collections.Generic;
using System.IO;

public static class FrenchPatch
{
    // Hauteurs de ligne de l'edition francaise par rang de police dans Zeus_Fonts.sg3 (zeus-poseidon-arm64).
    static readonly byte[] LineHeights = { 10, 16, 16, 16, 16, 16, 29, 29, 12, 16, 12, 11, 11, 11, 11, 16, 10 };

    // Boutons (tables de .data, enregistrements de 28 octets) : adresse du rectangle, anglais x1 y1 x2 y2, francais x1 y1 x2 y2.
    static readonly uint[] RectVa = { 0x005E86E8, 0x005E8704, 0x005E8720, 0x005E873C, 0x005E8758, 0x005E8774, 0x005EE740, 0x005EE75C, 0x005EE778, 0x005EE794, 0x005EE7B0, 0x005EE7CC, 0x005EE7E8, 0x005EE804, 0x005EE820, 0x005EE83C, 0x005FB900, 0x005FB91C, 0x005FB938, 0x00632BEC, 0x00632C08, 0x00632C78, 0x00632D74, 0x00632D90, 0x00632E38, 0x00632E54, 0x00632E70, 0x006431C8, 0x006431E4, 0x00643200, 0x0065BE90, 0x0065BEAC, 0x0065BEC8, 0x0065BEE4 };
    static readonly ushort[][] Rects = {
        new ushort[] { 20, 40, 140, 120, 18, 40, 172, 120 },
        new ushort[] { 170, 40, 290, 120, 184, 40, 340, 120 },
        new ushort[] { 320, 40, 440, 120, 352, 40, 510, 120 },
        new ushort[] { 20, 150, 140, 230, 18, 150, 172, 230 },
        new ushort[] { 170, 150, 290, 230, 184, 150, 340, 230 },
        new ushort[] { 320, 150, 440, 230, 352, 150, 510, 230 },
        new ushort[] { 437, 30, 529, 55, 502, 30, 594, 55 },
        new ushort[] { 437, 60, 529, 85, 502, 60, 594, 85 },
        new ushort[] { 437, 90, 529, 115, 502, 90, 594, 115 },
        new ushort[] { 437, 120, 529, 145, 502, 120, 594, 145 },
        new ushort[] { 437, 150, 529, 175, 502, 150, 594, 175 },
        new ushort[] { 437, 180, 529, 205, 502, 180, 594, 205 },
        new ushort[] { 437, 210, 529, 235, 502, 210, 594, 235 },
        new ushort[] { 437, 240, 529, 265, 502, 240, 594, 265 },
        new ushort[] { 437, 270, 529, 295, 502, 270, 594, 295 },
        new ushort[] { 437, 300, 529, 325, 502, 300, 594, 325 },
        new ushort[] { 20, 40, 330, 64, 20, 40, 360, 64 },
        new ushort[] { 20, 68, 330, 92, 20, 68, 360, 92 },
        new ushort[] { 20, 96, 330, 120, 20, 96, 360, 120 },
        new ushort[] { 280, 358, 450, 378, 260, 358, 410, 378 },
        new ushort[] { 113, 55, 311, 80, 133, 55, 311, 80 },
        new ushort[] { 113, 151, 343, 176, 113, 151, 243, 176 },
        new ushort[] { 140, 440, 200, 465, 202, 440, 242, 465 },
        new ushort[] { 401, 57, 588, 82, 401, 57, 561, 82 },
        new ushort[] { 113, 88, 311, 113, 133, 88, 311, 113 },
        new ushort[] { 201, 343, 351, 368, 201, 343, 321, 368 },
        new ushort[] { 220, 440, 380, 465, 252, 440, 312, 465 },
        new ushort[] { 142, 332, 267, 357, 142, 332, 262, 357 },
        new ushort[] { 276, 332, 406, 357, 268, 332, 396, 357 },
        new ushort[] { 418, 332, 498, 357, 403, 332, 498, 357 },
        new ushort[] { 5, 480, 125, 500, 35, 480, 145, 500 },
        new ushort[] { 153, 480, 273, 500, 158, 480, 298, 500 },
        new ushort[] { 290, 480, 410, 500, 310, 480, 410, 500 },
        new ushort[] { 409, 480, 529, 500, 439, 480, 549, 500 },
    };

    // Positions de textes ecrites dans le code (relevees dans l'exe francais) : adresse, anglais, francais.
    static readonly object[][] TextFixes = {
        // « Commencer le scenario » : add edx,54h -> add edx,27h (45 px plus a gauche)
        new object[] { 0x0042480Du, new byte[] { 0x83, 0xC2, 0x54 }, new byte[] { 0x83, 0xC2, 0x27 }, "Commencer le scénario" },
        // « Rejoindre la cite » : lea eax,[edi+1D2h] -> [edi+1AAh] (40 px plus a gauche)
        new object[] { 0x0052EC6Cu, new byte[] { 0x8D, 0x87, 0xD2, 0x01, 0x00, 0x00 }, new byte[] { 0x8D, 0x87, 0xAA, 0x01, 0x00, 0x00 }, "Rejoindre la cité" },
        // titre « Scenarios personnalises » : add edx,-48h -> -3Eh (10 px plus a droite)
        new object[] { 0x004244D9u, new byte[] { 0x83, 0xC2, 0xB8 }, new byte[] { 0x83, 0xC2, 0xC2 }, "Scénarios personnalisés" },
    };

    public const uint CharTable = 0x0063BE2C;
    const uint DrawGlyph = 0x00508B50, PolishEx = 0x00508C7F;
    const uint PanelGlyph = 0x004F59B0, PanelHeight = 0x004F5A2F, PanelPolishEx = 0x004F5ACF;
    const uint FontGroups = 0x012AFBEC, LastFontGroup = 0x012BC820, LineHeight = 0x00F13960;
    const uint Cave = 0x005D7400;            // place libre en fin de .text (le Customiser ecrit plus loin, en 0x5D7E40)
    const int CaveSize = 0x200;
    // En-tete de .text dans la compilation GOG : taille virtuelle et attributs d'origine
    const uint TextVirtualSize = 0x001D6351, TextFlags = 0x60000020;

    // Endroits du patch FR dans cet exe. frTable : table francaise (224 octets) reprise de l'exe francais ;
    // savedTable, savedHeader : table et en-tete d'origine notes lors du patch (pour reconnaitre et annuler un exe deja patche), ou null.
    public static List<Site> Build(byte[] exe, byte[] frTable, byte[] savedTable, byte[] savedHeader)
    {
        Pe pe = new Pe(exe);
        List<Site> s = new List<Site>();

        // 1. table de caracteres : la table d'origine est celle de l'exe (verifiee a part), jamais une copie fournie
        byte[] cur = pe.Get(CharTable, 224);
        byte[] orig = savedTable ?? (SameBytes(cur, frTable) ? Filled(224, 0xFF) : cur);
        s.Add(new Site("table des caractères", pe.Off(CharTable), orig, frTable));

        // 3. exceptions polonaises
        byte[] polish = { 0xB9, 0x02, 0x00, 0x00, 0x00 }, nops = Filled(5, 0x90);
        s.Add(new Site("exception polonaise (textes)", pe.Off(PolishEx), polish, nops));
        s.Add(new Site("exception polonaise (panneaux)", pe.Off(PanelPolishEx), polish, nops));

        // 4. boutons
        for (int r = 0; r < RectVa.Length; r++)
        {
            byte[] a = new byte[8], b = new byte[8];
            for (int k = 0; k < 4; k++)
            {
                Array.Copy(BitConverter.GetBytes(Rects[r][k]), 0, a, 2 * k, 2);
                Array.Copy(BitConverter.GetBytes(Rects[r][4 + k]), 0, b, 2 * k, 2);
            }
            Site x = new Site(string.Format("bouton {0:X}", RectVa[r]), pe.Off(RectVa[r]), a, b);
            x.Required = false;   // un patch widescreen a pu le deplacer : saute avec un avertissement
            s.Add(x);
        }

        // 5. positions de textes (facultatif)
        foreach (object[] fx in TextFixes)
        {
            Site x = new Site("texte « " + fx[3] + " »", pe.Off((uint)fx[0]), (byte[])fx[1], (byte[])fx[2]);
            x.Required = false;
            s.Add(x);
        }

        // 2. routines ajoutees, dans une place libre de .text, et detours
        uint org = FindCave(exe, pe);
        Asm a2 = Routines(org);
        byte[] code = a2.Build();
        s.Add(new Site("routines ajoutées", pe.Off(org), new byte[code.Length], code));
        s.Add(new Site("détour hauteur de ligne (textes)", pe.Off(DrawGlyph), new byte[] { 0x51, 0x8B, 0x44, 0x24, 0x0C }, Jmp(DrawGlyph, a2.Addr("h1"), 0)));
        s.Add(new Site("détour hauteur de ligne (panneaux)", pe.Off(PanelGlyph), new byte[] { 0x53, 0x8B, 0x5C, 0x24, 0x0C }, Jmp(PanelGlyph, a2.Addr("h2"), 0)));
        s.Add(new Site("détour hauteur des glyphes (panneaux)", pe.Off(PanelHeight), new byte[] { 0x0F, 0xBF, 0x48, 0x16, 0x83, 0xE9, 0x0B }, Jmp(PanelHeight, a2.Addr("h3"), 2)));

        // en-tete de .text : taille virtuelle couvrant les routines et ecriture permise (variable delta).
        // D'origine : valeurs de l'exe (ou notees lors du patch) ; un autre patch a pu deja les changer.
        uint[] t = pe.Secs[0];
        int hdr = (int)t[5];
        byte[] hcur = new byte[8];
        Array.Copy(exe, hdr + 8, hcur, 0, 4); Array.Copy(exe, hdr + 36, hcur, 4, 4);
        uint needSize = org + (uint)code.Length - t[0];
        byte[] hnew = new byte[8];
        uint vs = BitConverter.ToUInt32(savedHeader ?? hcur, 0), fl = BitConverter.ToUInt32(savedHeader ?? hcur, 4);
        Array.Copy(BitConverter.GetBytes(Math.Max(vs, Math.Max(needSize, t[3]))), 0, hnew, 0, 4);   // toute la partie brute de .text
        Array.Copy(BitConverter.GetBytes(fl | 0x80000000u), 0, hnew, 4, 4);
        byte[] horig = savedHeader ?? hcur;
        s.Add(new Site("en-tête .text (taille)", hdr + 8, Part(horig, 0), Part(hnew, 0)));
        s.Add(new Site("en-tête .text (attributs)", hdr + 36, Part(horig, 4), Part(hnew, 4)));
        return s;
    }

    // Place des routines : celle qu'elles occupent deja (exe patche : on suit le detour), sinon 0x5D7400 si libre,
    // sinon la premiere place libre en fin de .text. La zone 0x5D7E40-0x5D8000 est laissee au Resolution Customiser
    // (il y ecrit son propre code), pour qu'il puisse encore etre passe apres notre patch.
    static uint FindCave(byte[] exe, Pe pe)
    {
        int len = Routines(Cave).Build().Length;
        uint h1 = Routines(Cave).Addr("h1") - Cave;
        int d = pe.Off(DrawGlyph);
        if (d >= 0 && exe[d] == 0xE9)
        {
            uint target = (uint)(DrawGlyph + 5 + BitConverter.ToInt32(exe, d + 1));
            uint org0 = target - h1;
            int o = pe.Off(org0);
            if (o >= 0 && o + len <= exe.Length && SameAt(exe, o, Routines(org0).Build())) return org0;
        }
        uint[] t = pe.Secs[0];
        uint start = t[0] + TextVirtualSize;   // fin du code de la compilation GOG
        uint end = Math.Min(t[0] + t[3], 0x005D7E40);
        foreach (uint cand in Candidates(start, end))
        {
            int o = pe.Off(cand);
            if (o < 0 || cand + len + 16 > end) continue;
            bool free = true;
            for (int i = -16; i < len + 16 && free; i++) if (exe[o + i] != 0) free = false;
            if (free) return cand;
        }
        throw new Exception(L.T("pas de place libre dans l'exe pour les routines du patch", "no free space in the exe for the patch routines"));
    }

    static IEnumerable<uint> Candidates(uint start, uint end)
    {
        yield return Cave;
        for (uint a = (start + 31) & ~15u; a + 16 < end; a += 16) if (a != Cave) yield return a;
    }

    static bool SameAt(byte[] exe, int o, byte[] v)
    {
        for (int i = 0; i < v.Length; i++) if (exe[o + i] != v[i]) return false;
        return true;
    }

    // Controles de la table d'origine : table anglaise plausible ('A' -> 27) et differente de la francaise.
    public static bool LooksEnglishTable(byte[] exe, byte[] frTable)
    {
        Pe pe = new Pe(exe);
        int o = pe.Off(CharTable);
        if (o < 0) return false;
        byte[] cur = pe.Get(CharTable, 224);
        return cur['A' - 0x20] == 27 && !SameBytes(cur, frTable);
    }

    static byte[] Part(byte[] b, int at) { byte[] r = new byte[4]; Array.Copy(b, at, r, 0, 4); return r; }

    static Asm Routines(uint org)
    {
        Asm s = new Asm(org);
        s.Label("heights"); s.Emit(LineHeights); s.Align(4);
        s.Label("delta"); s.Dd(16);
        // rank : eax = identifiant de la police (1re image) -> edx = rang dans Zeus_Fonts.sg3, ou -1
        s.Align(16); s.Label("rank");
        s.Emit(0x8B, 0x35); s.Dd(FontGroups);            // mov esi,[groupes]
        s.Emit(0x85, 0xF6); s.Jcc(0x74, "none");          // test esi,esi ; jz none
        s.Emit(0x8B, 0x0D); s.Dd(LastFontGroup);         // mov ecx,[dernier rang]
        s.Emit(0x41);                                     // inc ecx
        s.Emit(0x83, 0xF9, 17); s.Jcc(0x76, "lim");       // cmp ecx,17 ; jbe lim
        s.Emit(0xB9); s.Dd(17);                           // mov ecx,17
        s.Label("lim"); s.Emit(0x33, 0xD2);               // xor edx,edx
        s.Label("loop"); s.Emit(0x3B, 0xD1); s.Jcc(0x73, "none");  // cmp edx,ecx ; jae none
        s.Emit(0x39, 0x44, 0xD6, 0x04); s.Jcc(0x74, "found");      // cmp [esi+edx*8+4],eax ; je found
        s.Emit(0x42); s.Jcc(0xEB, "loop");                // inc edx ; jmp loop
        s.Label("none"); s.Emit(0x83, 0xCA, 0xFF);        // or edx,-1
        s.Label("found"); s.Emit(0xC3);                   // ret

        // a) textes d'une ligne : hauteur de ligne francaise de la police
        s.Align(16); s.Label("h1");
        s.Emit(0x60);                                     // pushad
        s.Emit(0x8B, 0x44, 0x24, 0x24);                   // mov eax,[esp+24h] (1er argument)
        s.Call("rank");
        s.Emit(0x85, 0xD2); s.Jcc(0x78, "h1skip");        // test edx,edx ; js h1skip
        s.Emit(0x0F, 0xB6, 0x82); s.DdLabel("heights");   // movzx eax,byte [heights+edx]
        s.Emit(0xA3); s.Dd(LineHeight);                   // mov [hauteur de ligne],eax
        s.Label("h1skip"); s.Emit(0x61);                  // popad
        s.Emit(0x51, 0x8B, 0x44, 0x24, 0x0C);             // instructions d'origine : push ecx ; mov eax,[esp+0Ch]
        s.JmpAbs(DrawGlyph + 5);

        // b1) entree du dessin des panneaux : valeur a retrancher selon la police (16, ou 11 pour la police 13)
        s.Align(16); s.Label("h2");
        s.Emit(0x60, 0x8B, 0x44, 0x24, 0x24);             // pushad ; mov eax,[esp+24h]
        s.Call("rank");
        s.Emit(0xC7, 0x05); s.DdLabel("delta"); s.Dd(16); // mov dword [delta],16
        s.Emit(0x83, 0xFA, 13); s.Jcc(0x75, "h2done");    // cmp edx,13 ; jne h2done
        s.Emit(0xC7, 0x05); s.DdLabel("delta"); s.Dd(11); // mov dword [delta],11
        s.Label("h2done"); s.Emit(0x61);                  // popad
        s.Emit(0x53, 0x8B, 0x5C, 0x24, 0x0C);             // instructions d'origine : push ebx ; mov ebx,[esp+0Ch]
        s.JmpAbs(PanelGlyph + 5);

        // b2) hauteur du glyphe moins la valeur choisie (au lieu de 11)
        s.Align(16); s.Label("h3");
        s.Emit(0x0F, 0xBF, 0x48, 0x16);                   // movsx ecx,word [eax+16h]
        s.Emit(0x2B, 0x0D); s.DdLabel("delta");           // sub ecx,[delta]
        s.JmpAbs(PanelHeight + 7);
        return s;
    }

    static byte[] Jmp(uint at, uint to, int nops)
    {
        byte[] b = new byte[5 + nops];
        b[0] = 0xE9;
        Array.Copy(BitConverter.GetBytes((int)(to - (at + 5))), 0, b, 1, 4);
        for (int i = 0; i < nops; i++) b[5 + i] = 0x90;
        return b;
    }

    static byte[] Filled(int n, byte v) { byte[] b = new byte[n]; for (int i = 0; i < n; i++) b[i] = v; return b; }

    public static bool SameBytes(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }
}
