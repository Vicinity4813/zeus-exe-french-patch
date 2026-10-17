// Endroits modifies dans Zeus.exe : position dans le fichier, octets d'origine (compilation GOG anglaise v2.1.4.0)
// et octets du patch. Le patcheur ne regarde que ces octets-la : un exe deja passe au Resolution Customiser
// (autre resolution, autre empreinte) reste accepte tant que ces endroits sont intacts.
using System;
using System.Collections.Generic;
using System.Text;

public class Site
{
    public string Name;
    public int Offset;          // position dans le fichier
    public byte[] Original;     // octets attendus avant le patch
    public byte[] Patched;      // octets ecrits par le patch

    public Site(string name, int offset, byte[] original, byte[] patched)
    {
        if (original.Length != patched.Length) throw new ArgumentException("longueurs differentes : " + name);
        Name = name; Offset = offset; Original = original; Patched = patched;
    }

    public SiteState StateIn(byte[] exe)
    {
        if (Offset < 0 || Offset + Original.Length > exe.Length) return SiteState.Other;
        if (Same(exe, Patched)) return SiteState.Patched;
        if (Same(exe, Original)) return SiteState.Original;
        return SiteState.Other;
    }

    bool Same(byte[] exe, byte[] v)
    {
        for (int i = 0; i < v.Length; i++) if (exe[Offset + i] != v[i]) return false;
        return true;
    }

    public static string Hex(byte[] b, int max)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < b.Length && i < max; i++) sb.Append(b[i].ToString("X2")).Append(' ');
        if (b.Length > max) sb.Append("...");
        return sb.ToString().Trim();
    }
}

public enum SiteState { Original, Patched, Other }

// Bilan d'un ensemble d'endroits dans un exe.
public class Survey
{
    public int Original, Patched, Other;
    public List<Site> Unexpected = new List<Site>();

    public static Survey Of(List<Site> sites, byte[] exe)
    {
        Survey s = new Survey();
        foreach (Site x in sites)
        {
            switch (x.StateIn(exe))
            {
                case SiteState.Original: s.Original++; break;
                case SiteState.Patched: s.Patched++; break;
                default: s.Other++; s.Unexpected.Add(x); break;
            }
        }
        return s;
    }

    public bool AllOriginal { get { return Patched == 0 && Other == 0; } }
    public bool AllPatched { get { return Original == 0 && Other == 0; } }
}
