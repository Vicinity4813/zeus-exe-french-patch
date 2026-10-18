// Interface en francais uniquement : le patcheur ne sert qu'a mettre en francais un Zeus.exe anglais.
// L.T(fr, en) renvoie toujours le texte francais (le texte anglais n'est pas utilise).
static class L
{
    public static string T(string fr, string en) { return fr; }
}
