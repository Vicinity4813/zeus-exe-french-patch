# Zeus Patcher

**French patch and widescreen scenario editor for *Zeus: Master of Olympus* / *Poseidon***
· [Version française plus bas](#français)

Two patches for `Zeus.exe`, in one small Windows program:

1. **French patch** — turns an **English `Zeus.exe`** (2.1.4.0: GOG, Steam and every widescreen version, at any resolution) into one that displays the French texts correctly: accents, line heights, wider buttons, menu text positions, as in the French edition.
2. **Scenario editor patch — any resolution, any world map, English or French exe.** The original adventure editor can only place cities on the small 838 × 738 world map. This patch, to our knowledge the first of its kind, makes it work with an **enlarged world map of any size**: cities can be placed anywhere on the painted map, set on the left (like the original), centred or on the right of the picture. The world map size is suggested from the resolution read in the exe.

Only the chosen `Zeus.exe` is changed. A `Zeus.exe.bak` copy of the original is made first, and one button puts it back.

## Download

Get `Zeus-Patcher-1.0.zip` from the **[Releases](../../releases)** page: the program, instructions in English and French, and the source code.

> Windows may show *"Windows protected your PC"* the first time, because the program is new and unsigned: click **More info**, then **Run anyway**.

## Usage

1. Run `Patcheur Zeus.exe` and choose your `Zeus.exe`. The program shows what it found: English or French exe, and its resolution.
2. **Patch to French** (English exe only).
3. **Patch the editor**: check the world map size and where the painted map sits in it, then click.

**Put the original exe back** restores the `.bak` copy.

Tested on every JackFuste widescreen exe from 1280×720 to 3440×1440. Not recognised: Russian builds and protected original CD exes (SecuROM).

## A 100% French game from the English game

The patch fixes the exe; the French texts, fonts, pictures and voices come from **your own official French game** (CD or install). Copy these into the English game folder, at the same places:

| Needed for | Files |
|---|---|
| **Texts and accented letters** (required) | `Zeus_Text.eng`, `Zeus_MM.eng`, `Model\Zeus eventmsg.txt`, `DATA\Zeus_Fonts.sg3`, `DATA\zeus_Fonts.555` |
| Menus and end screens | `DATA\Zeus_FE_ChooseGame.jpg`, `DATA\Zeus_FE_CampaignSelection.JPG`, `DATA\Zeus_FE_MissionIntroduction.jpg`, `DATA\Zeus_FE_tutorials.JPG`, `DATA\Zeus_FE_Registry.jpg`, `DATA\Poseidon_FE_MainMenu.JPG`, `DATA\Poseidon_FE_Registry.jpg`, `DATA\Zeus_Victory.jpg`, `DATA\Zeus_Defeat.jpg`, `DATA\scoreb.jpg` |
| Voices, sounds, intro video | `Audio\` (whole folder, ~340 MB), `Binks\Zeus_Intro.bik`, `Binks\Zeus_Intro_HiQ.bik` |
| French campaigns and tutorials | in `Adventures\`: the 16 Zeus campaigns (`#La naissance de l'Atlantide.pak` … `}Jeu ouvert - Economie.pak`), the 8 Poseidon adventure folders (`L'odyssée`, `La naissance de Troie`, `La révolte maya`, `Les plus jeunes jumeaux`, `Atlantide, la cité engloutie`, `Jeu ouvert - Armée 2`, `Jeu ouvert - Bac à sable 2`, `Jeu ouvert - Economie 2`), the tutorials (`agora.pak` … `warrior.pak`) and `tutorial00.sav` … `tutorial15.sav` |

Move the English campaigns out of `Adventures` (don't delete them), or every campaign shows up twice. Not needed: `Zeus_Editor_Text.eng` (same in both versions), `Zeus.ini`, `Zeus.inf`. Saved games work in both languages.

## Full-screen world maps

For enlarged world maps, use **[World Map Converter](https://github.com/Vicinity4813/zeus-world-maps)** (*Convertisseur mappemonde*): it moves the cities, region names and trade routes of scenarios (`.pak`, `.set`) and saved games (`.sav`) onto the enlarged map, keeping `.bak` copies of the originals. Then patch the editor here with the same world map size and position.

## Source code

| File | Role |
|---|---|
| `src/FrenchPatch.cs` | the French patch: character table, line heights, buttons, menu texts |
| `src/FrTable.cs` | French character → glyph table (224 bytes) |
| `src/EditorPatch.cs` | the editor patch (English and French builds) |
| `src/Sites.cs` | every changed place with its expected original bytes |
| `src/Core.cs` | exe recognition, resolution, .bak copy, writing |
| `src/Program.cs` | window (French / English) and command line |

Build with `build.cmd` (C# compiler shipped with Windows, .NET Framework 4); `tools/make_release.ps1` builds the release zip. The patcher checks only the bytes it changes, so an exe already patched for another resolution is accepted; anything unexpected is refused before writing.

Thanks to [zeus-poseidon-arm64](https://github.com/nic0lasm/zeus-poseidon-arm64) (nic0lasm) for documenting how the game draws text. MIT License — Vicinity4813. *Zeus* and *Poseidon* are games by Impressions Games / Sierra; no game file is included except the 224-byte French character table.

---

## Français

**Patch FR et éditeur de scénarios grand écran pour *Zeus : Le Maître de l'Olympe* / *Poséidon***

Deux patchs pour `Zeus.exe`, dans un petit programme Windows :

1. **Patch FR** : rend un **`Zeus.exe` anglais** (2.1.4.0 : GOG, Steam et toutes ses versions widescreen, quelle que soit la résolution) capable d'afficher correctement les textes français : accents, hauteur des lignes, boutons élargis, position des textes du menu, comme l'édition française.
2. **Patch de l'éditeur de scénarios : n'importe quelle résolution, n'importe quelle mappemonde, exe anglais ou français.** L'éditeur d'origine ne sait placer les cités que sur la petite mappemonde de 838 × 738. Ce patch, à notre connaissance le premier du genre, le fait fonctionner avec une **mappemonde agrandie de n'importe quelle taille** : les cités se placent sur toute la carte peinte, calée à gauche (comme l'originale), au centre ou à droite de l'image. La taille est proposée d'après la résolution lue dans l'exe.

Seul le `Zeus.exe` choisi est modifié. Une copie `Zeus.exe.bak` de l'original est faite d'abord, et un bouton la remet en place.

### Téléchargement

`Zeus-Patcher-1.0.zip` sur la page **[Releases](../../releases)** : le programme, les notices en français et en anglais, et le code source.

> Windows peut afficher « Windows a protégé votre ordinateur » au premier lancement (programme récent, non signé) : **Informations complémentaires**, puis **Exécuter quand même**.

### Utilisation

1. Lancez `Patcheur Zeus.exe` et choisissez votre `Zeus.exe` : le programme indique ce qu'il a trouvé (exe anglais ou français, résolution).
2. **Patcher en français** (exe anglais seulement).
3. **Patcher l'éditeur** : vérifiez la taille de la mappemonde et la place de la carte peinte, puis cliquez.

**Remettre l'exe d'origine** remet la copie `.bak`.

Testé sur tous les exe widescreen de JackFuste, de 1280×720 à 3440×1440. Non reconnus : versions russes et exe du CD d'origine protégés (SecuROM).

### Un jeu 100 % français à partir du jeu anglais

Le patch corrige l'exe ; les textes, polices, images et voix françaises viennent de **votre propre jeu français officiel** (CD ou installation). Copiez-les dans le dossier du jeu anglais, aux mêmes emplacements :

| Pour | Fichiers |
|---|---|
| **Textes et lettres accentuées** (indispensable) | `Zeus_Text.eng`, `Zeus_MM.eng`, `Model\Zeus eventmsg.txt`, `DATA\Zeus_Fonts.sg3`, `DATA\zeus_Fonts.555` |
| Menus et écrans de fin | `DATA\Zeus_FE_ChooseGame.jpg`, `DATA\Zeus_FE_CampaignSelection.JPG`, `DATA\Zeus_FE_MissionIntroduction.jpg`, `DATA\Zeus_FE_tutorials.JPG`, `DATA\Zeus_FE_Registry.jpg`, `DATA\Poseidon_FE_MainMenu.JPG`, `DATA\Poseidon_FE_Registry.jpg`, `DATA\Zeus_Victory.jpg`, `DATA\Zeus_Defeat.jpg`, `DATA\scoreb.jpg` |
| Voix, sons, vidéo d'introduction | `Audio\` (tout le dossier, ~340 Mo), `Binks\Zeus_Intro.bik`, `Binks\Zeus_Intro_HiQ.bik` |
| Campagnes et tutoriels en français | dans `Adventures\` : les 16 campagnes Zeus (`#La naissance de l'Atlantide.pak` … `}Jeu ouvert - Economie.pak`), les 8 dossiers des aventures Poséidon (`L'odyssée`, `La naissance de Troie`, `La révolte maya`, `Les plus jeunes jumeaux`, `Atlantide, la cité engloutie`, `Jeu ouvert - Armée 2`, `Jeu ouvert - Bac à sable 2`, `Jeu ouvert - Economie 2`), les tutoriels (`agora.pak` … `warrior.pak`) et `tutorial00.sav` … `tutorial15.sav` |

Déplacez les campagnes anglaises hors du dossier `Adventures` (sans les supprimer), sinon chaque campagne apparaît deux fois. Inutile : `Zeus_Editor_Text.eng` (identique dans les deux versions), `Zeus.ini`, `Zeus.inf`. Les sauvegardes restent compatibles dans les deux langues.

### Mappemondes plein écran

Pour des mappemondes agrandies, utilisez **[World Map Converter](https://github.com/Vicinity4813/zeus-world-maps)** (*Convertisseur mappemonde*) : il replace les cités, noms de régions et routes des scénarios (`.pak`, `.set`) et des sauvegardes (`.sav`) sur la carte agrandie, en gardant une copie `.bak` des originaux. Patchez ensuite l'éditeur ici avec la même taille de mappemonde et la même position.

Merci à [zeus-poseidon-arm64](https://github.com/nic0lasm/zeus-poseidon-arm64) (nic0lasm) pour sa documentation de l'affichage du texte. Licence MIT — Vicinity4813. *Zeus* et *Poséidon* sont des jeux d'Impressions Games / Sierra ; aucun fichier du jeu n'est inclus, hormis la table des caractères française de 224 octets.
