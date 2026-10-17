// Patch de l'editeur de mappemonde (exe anglais 2.1.4.0) pour des mappemondes plus grandes que l'originale.
// Meme correction que celle validee sur l'exe francais (fonction de placement 0x408E50-0x409028 de l'exe FR),
// retrouvee dans la compilation anglaise (0x406CE0-0x406F00) :
//   1. le controle « souris dans un rectangle 555 x 556 » (taille de la mappemonde d'origine) est saute ;
//   2. le controle de la position de l'icone s'applique avec tous les outils (sinon cites possibles hors carte) ;
//   3. les limites de ce controle, autour du centre de l'image (-299/+299, -285/+285 a l'origine), suivent la
//      mappemonde installee : la carte peinte (838 x 738 agrandie a la hauteur de l'image) peut etre calee a gauche
//      (comme l'originale, le reste de l'image a droite), au centre ou a droite de l'image.
using System;
using System.Collections.Generic;

public enum MapLayout { Left, Center, Right }

public static class EditorPatch
{
    const uint MouseCheck = 0x00406D0B, ModeTest = 0x00406DD0, ModeJne = 0x00406DD8;
    const uint LimLeft = 0x00406EB1, LimRight = 0x00406EC0, LimTop = 0x00406ED8, LimBottom = 0x00406EE7;

    // Limites (gauche, droite, haut, bas) autour du centre d'une image w x h, carte peinte placee selon layout.
    public static int[] Limits(int w, int h, MapLayout layout)
    {
        double s = h / 738.0;
        int mapW = Math.Min(w, (int)Math.Round(838 * s));
        int dx = layout == MapLayout.Left ? 0 : layout == MapLayout.Right ? w - mapW : (w - mapW) / 2;
        return new[] { w / 2 - dx, dx + mapW - w / 2, h / 2, h / 2 };
    }

    // Endroits du patch de l'editeur. lim : limites voulues (Limits), ou les valeurs d'origine si null.
    public static List<Site> Build(byte[] exe, int[] lim)
    {
        Pe pe = new Pe(exe);
        List<Site> s = new List<Site>();
        // 1. saut par-dessus le controle du rectangle (vers le test du mode)
        byte[] jmp = new byte[5];
        jmp[0] = 0xE9; Array.Copy(BitConverter.GetBytes((int)(ModeTest - (MouseCheck + 5))), 0, jmp, 1, 4);
        s.Add(new Site("éditeur : contrôle de la souris", pe.Off(MouseCheck), new byte[] { 0x6A, 0x0C, 0x6A, 0x03, 0xE8 }, jmp));
        // 2. controle de l'icone pour tous les outils (jne -> 6 NOP)
        s.Add(new Site("éditeur : contrôle de l'icône", pe.Off(ModeJne), new byte[] { 0x0F, 0x85, 0x1A, 0x01, 0x00, 0x00 }, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }));
        // 3. limites (valeurs 32 bits) : lea edx,[eax-L] ; add eax,R ; lea ecx,[eax-T] ; add eax,B
        int[] v = lim ?? new[] { 299, 299, 285, 285 };
        s.Add(Lim("éditeur : limite gauche", pe.Off(LimLeft), -299, -v[0]));
        s.Add(Lim("éditeur : limite droite", pe.Off(LimRight), 299, v[1]));
        s.Add(Lim("éditeur : limite haute", pe.Off(LimTop), -285, -v[2]));
        s.Add(Lim("éditeur : limite basse", pe.Off(LimBottom), 285, v[3]));
        return s;
    }

    static Site Lim(string name, int off, int orig, int now)
    {
        Site x = new Site(name, off, BitConverter.GetBytes(orig), BitConverter.GetBytes(now));
        x.Value = true;
        return x;
    }

    // Octets de contexte autour des limites, pour reconnaitre la fonction meme si les limites ont deja ete changees.
    public static bool FunctionLooksRight(byte[] exe)
    {
        Pe pe = new Pe(exe);
        return Has(exe, pe, LimLeft - 2, 0x8D, 0x90) && Has(exe, pe, LimRight - 1, 0x05) && Has(exe, pe, LimTop - 2, 0x8D, 0x88)
            && Has(exe, pe, LimBottom - 1, 0x05) && Has(exe, pe, ModeTest, 0xA1) && Has(exe, pe, ModeTest + 5, 0x83, 0xF8, 0x01);
    }

    static bool Has(byte[] exe, Pe pe, uint va, params byte[] b)
    {
        int o = pe.Off(va);
        if (o < 0 || o + b.Length > exe.Length) return false;
        for (int i = 0; i < b.Length; i++) if (exe[o + i] != b[i]) return false;
        return true;
    }
}
