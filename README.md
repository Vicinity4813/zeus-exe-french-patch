# Zeus Patcher

**French patch for *Zeus: Master of Olympus* / *Poseidon***
· [Version française plus bas](#français)

Turns an **English `Zeus.exe`** (2.1.4.0: GOG, Steam and every widescreen version, at any resolution) into one that displays the French texts correctly: accents, line heights, wider buttons, menu text positions, as in the French edition. Available from the French interface only (the button is greyed out in the English interface).

Only the chosen `Zeus.exe` is changed. A `Zeus.exe.bak` copy of the original is made first, and one button puts it back.

For full-screen world maps and the **scenario editor patch** (any resolution, any world map, English or French exe), see **[Zeus World Map Converter](https://github.com/Vicinity4813/zeus-world-maps)**.

## Download

Get `Zeus-Patcher-1.0.zip` from the **[Releases](../../releases)** page: the program, instructions in English and French, and the source code.

> Windows may show *"Windows protected your PC"* the first time, because the program is new and unsigned: click **More info**, then **Run anyway**.

## Usage

1. Run `Patcheur Zeus.exe` (French interface) and choose your `Zeus.exe`. The program shows what it found: English or French exe, and its resolution.
2. Click **Patcher en français**.

**Remettre l'exe d'origine** restores the `.bak` copy. Tested on every JackFuste widescreen exe from 1280×720 to 3440×1440. Not recognised: Russian builds and protected original CD exes (SecuROM).

## A 100% French game from the English game

The patch fixes the exe; the French texts, fonts, pictures and voices come from **your own official French game** (CD or install). Copy these into the English game folder, at the same places:

| Needed for | Files |
|---|---|
| **Texts and accented letters** (required) | `Zeus_Text.eng`, `Zeus_MM.eng`, `Model\Zeus eventmsg.txt`, `DATA\Zeus_Fonts.sg3`, `DATA\zeus_Fonts.555` |
| Menus and end screens | `DATA\Zeus_FE_ChooseGame.jpg`, `DATA\Zeus_FE_CampaignSelection.JPG`, `DATA\Zeus_FE_MissionIntroduction.jpg`, `DATA\Zeus_FE_tutorials.JPG`, `DATA\Zeus_FE_Registry.jpg`, `DATA\Poseidon_FE_MainMenu.JPG`, `DATA\Poseidon_FE_Registry.jpg`, `DATA\Zeus_Victory.jpg`, `DATA\Zeus_Defeat.jpg`, `DATA\scoreb.jpg` |
| Voices, sounds, intro video | `Audio\` (whole folder, ~340 MB), `Binks\Zeus_Intro.bik`, `Binks\Zeus_Intro_HiQ.bik` |
| French campaigns and tutorials | in `Adventures\`: the 16 Zeus campaigns (`#La naissance de l'Atlantide.pak` … `}Jeu ouvert - Economie.pak`), the 8 Poseidon adventure folders (`L'odyssée`, `La naissance de Troie`, `La révolte maya`, `Les plus jeunes jumeaux`, `Atlantide, la cité engloutie`, `Jeu ouvert - Armée 2`, `Jeu ouvert - Bac à sable 2`, `Jeu ouvert - Economie 2`), the tutorials (`agora.pak` … `warrior.pak`) and `tutorial00.sav` … `tutorial15.sav` |

Move the English campaigns out of `Adventures` (don't delete them), or every campaign shows up twice. Not needed: `Zeus_Editor_Text.eng` (same in both versions), `Zeus.ini`, `Zeus.inf`. Saved games work in both languages.

## Source code

| File | Role |
|---|---|
| `src/FrenchPatch.cs` | the French patch: character table, line heights, buttons, menu texts |
| `src/FrTable.cs` | French character → glyph table (224 bytes) |
| `src/Sites.cs` | every changed place with its expected original bytes |
| `src/EditorPatch.cs` | recognises the English and French builds |
| `src/Core.cs` | exe recognition, resolution, .bak copy, writing |
| `src/Program.cs` | window (French / English) and command line |

Build with `build.cmd` (C# compiler shipped with Windows, .NET Framework 4); `tools/make_release.ps1` builds the release zip. The patcher checks only the bytes it changes, so an exe already patched for another resolution is accepted; anything unexpected is refused before writing.

Thanks to [zeus-poseidon-arm64](https://github.com/nic0lasm/zeus-poseidon-arm64) (nic0lasm) for documenting how the game draws text. MIT License — Vicinity4813. *Zeus* and *Poseidon* are games by Impressions Games / Sierra; no game file is included except the 224-byte French character table.

---

## Français

**Patch FR pour *Zeus : Le Maître de l'Olympe* / *Poséidon***

Rend un **`Zeus.exe` anglais** (2.1.4.0 : GOG, Steam et toutes ses versions widescreen, quelle que soit la résolution) capable d'afficher correctement les textes français : accents, hauteur des lignes, boutons élargis, position des textes du menu, comme l'édition française.

Seul le `Zeus.exe` choisi est modifié. Une copie `Zeus.exe.bak` de l'original est faite d'abord, et un bouton la remet en place.

Pour les mappemondes plein écran et le **patch de l'éditeur de scénarios** (n'importe quelle résolution, n'importe quelle mappemonde, exe anglais ou français), voir **[Zeus World Map Converter](https://github.com/Vicinity4813/zeus-world-maps)**.

### Téléchargement

`Zeus-Patcher-1.0.zip` sur la page **[Releases](../../releases)** : le programme, les notices en français et en anglais, et le code source.

> Windows peut afficher « Windows a protégé votre ordinateur » au premier lancement (programme récent, non signé) : **Informations complémentaires**, puis **Exécuter quand même**.

### Utilisation

1. Lancez `Patcheur Zeus.exe` et choisissez votre `Zeus.exe` : le programme indique ce qu'il a trouvé (exe anglais ou français, résolution).
2. Cliquez sur **Patcher en français**.

**Remettre l'exe d'origine** remet la copie `.bak`. Testé sur tous les exe widescreen de JackFuste, de 1280×720 à 3440×1440. Non reconnus : versions russes et exe du CD d'origine protégés (SecuROM).

### Un jeu 100 % français à partir du jeu anglais

Le patch corrige l'exe ; les textes, polices, images et voix françaises viennent de **votre propre jeu français officiel** (CD ou installation). Copiez-les dans le dossier du jeu anglais, aux mêmes emplacements :

| Pour | Fichiers |
|---|---|
| **Textes et lettres accentuées** (indispensable) | `Zeus_Text.eng`, `Zeus_MM.eng`, `Model\Zeus eventmsg.txt`, `DATA\Zeus_Fonts.sg3`, `DATA\zeus_Fonts.555` |
| Menus et écrans de fin | `DATA\Zeus_FE_ChooseGame.jpg`, `DATA\Zeus_FE_CampaignSelection.JPG`, `DATA\Zeus_FE_MissionIntroduction.jpg`, `DATA\Zeus_FE_tutorials.JPG`, `DATA\Zeus_FE_Registry.jpg`, `DATA\Poseidon_FE_MainMenu.JPG`, `DATA\Poseidon_FE_Registry.jpg`, `DATA\Zeus_Victory.jpg`, `DATA\Zeus_Defeat.jpg`, `DATA\scoreb.jpg` |
| Voix, sons, vidéo d'introduction | `Audio\` (tout le dossier, ~340 Mo), `Binks\Zeus_Intro.bik`, `Binks\Zeus_Intro_HiQ.bik` |
| Campagnes et tutoriels en français | dans `Adventures\` : les 16 campagnes Zeus (`#La naissance de l'Atlantide.pak` … `}Jeu ouvert - Economie.pak`), les 8 dossiers des aventures Poséidon (`L'odyssée`, `La naissance de Troie`, `La révolte maya`, `Les plus jeunes jumeaux`, `Atlantide, la cité engloutie`, `Jeu ouvert - Armée 2`, `Jeu ouvert - Bac à sable 2`, `Jeu ouvert - Economie 2`), les tutoriels (`agora.pak` … `warrior.pak`) et `tutorial00.sav` … `tutorial15.sav` |

Déplacez les campagnes anglaises hors du dossier `Adventures` (sans les supprimer), sinon chaque campagne apparaît deux fois. Inutile : `Zeus_Editor_Text.eng` (identique dans les deux versions), `Zeus.ini`, `Zeus.inf`. Les sauvegardes restent compatibles dans les deux langues.

Merci à [zeus-poseidon-arm64](https://github.com/nic0lasm/zeus-poseidon-arm64) (nic0lasm) pour sa documentation de l'affichage du texte. Licence MIT — Vicinity4813. *Zeus* et *Poséidon* sont des jeux d'Impressions Games / Sierra ; aucun fichier du jeu n'est inclus, hormis la table des caractères française de 224 octets.
