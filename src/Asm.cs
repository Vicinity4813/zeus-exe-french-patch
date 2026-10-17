using System;
using System.Collections.Generic;

// Mini-assembleur : octets bruts, etiquettes, sauts courts conditionnels, appels et sauts absolus.
class Asm
{
    readonly uint org;
    readonly List<byte> b = new List<byte>();
    readonly Dictionary<string, uint> labels = new Dictionary<string, uint>();
    readonly List<Tuple<int, string, int>> fix = new List<Tuple<int, string, int>>();   // position, etiquette, type (1 rel8, 4 rel32, 0 abs32)

    public Asm(uint org) { this.org = org; }
    uint Here { get { return org + (uint)b.Count; } }
    public void Emit(params byte[] x) { b.AddRange(x); }
    public void Dd(uint v) { b.AddRange(BitConverter.GetBytes(v)); }
    public void Align(int n) { while (b.Count % n != 0) b.Add(0xCC); }
    public void Label(string n) { labels[n] = Here; }
    public uint Addr(string n) { return labels[n]; }
    public void Jcc(byte op, string l) { b.Add(op); fix.Add(Tuple.Create(b.Count, l, 1)); b.Add(0); }
    public void Call(string l) { b.Add(0xE8); fix.Add(Tuple.Create(b.Count, l, 4)); Dd(0); }
    public void DdLabel(string l) { fix.Add(Tuple.Create(b.Count, l, 0)); Dd(0); }
    public void JmpAbs(uint to) { b.Add(0xE9); int p = b.Count; Dd(0); byte[] r = BitConverter.GetBytes((int)(to - (org + (uint)p + 4))); for (int i = 0; i < 4; i++) b[p + i] = r[i]; }

    public byte[] Build()
    {
        foreach (var f in fix)
        {
            uint target = labels[f.Item2];
            if (f.Item3 == 1)
            {
                int rel = (int)target - (int)(org + (uint)f.Item1 + 1);
                if (rel < -128 || rel > 127) throw new Exception("saut trop long vers " + f.Item2);
                b[f.Item1] = (byte)(sbyte)rel;
            }
            else
            {
                byte[] r = f.Item3 == 4 ? BitConverter.GetBytes((int)target - (int)(org + (uint)f.Item1 + 4)) : BitConverter.GetBytes(target);
                for (int i = 0; i < 4; i++) b[f.Item1 + i] = r[i];
            }
        }
        return b.ToArray();
    }
}
