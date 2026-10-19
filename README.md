# Patcheur Zeus

**Patch FR pour *Zeus : Le Maître de l'Olympe* / *Poséidon***

Rend un **`Zeus.exe` anglais** (2.1.4.0 : GOG, Steam et toutes ses versions widescreen, quelle que soit la résolution) capable d'afficher correctement les textes français : accents, hauteur des lignes, boutons élargis, position des textes du menu, comme l'édition française.

Seul le `Zeus.exe` choisi est modifié. Une copie `Zeus.exe.bak` de l'original est faite d'abord, et un bouton la remet en place.

Pour les mappemondes plein écran et le **patch de l'éditeur de scénarios** (n'importe quelle résolution, n'importe quelle mappemonde, exe anglais ou français), voir **[World Map Converter (Zeus - World Maps Tools)](https://github.com/Vicinity4813/zeus-world-maps-tools)**.

## Téléchargement

`Zeus-Patcher-1.0.zip` sur la page **[Releases](../../releases)** : le programme, sa notice et le code source.

> Windows peut afficher « Windows a protégé votre ordinateur » au premier lancement (programme récent, non signé) : **Informations complémentaires**, puis **Exécuter quand même**.

## Utilisation

1. Lancez `Patcheur Zeus.exe` et choisissez votre `Zeus.exe` : le programme indique ce qu'il a trouvé (exe anglais ou français, résolution).
2. Cliquez sur **Patcher en français**.

**Remettre l'exe d'origine** remet la copie `.bak`. Testé sur tous les exe widescreen de JackFuste, de 1280×720 à 3440×1440. Non reconnus : versions russes et exe du CD d'origine protégés (SecuROM).

## Un jeu 100 % français à partir du jeu anglais

Le patch corrige l'exe ; les textes, polices, images et voix françaises viennent de **votre propre jeu français officiel** (CD ou installation). Copiez-les dans le dossier du jeu anglais, aux mêmes emplacements :

| Pour | Fichiers |
|---|---|
| **Textes et lettres accentuées** (indispensable) | `Zeus_Text.eng`, `Zeus_MM.eng`, `Model\Zeus eventmsg.txt`, `DATA\Zeus_Fonts.sg3`, `DATA\zeus_Fonts.555` |
| Menus et écrans de fin | `DATA\Zeus_FE_ChooseGame.jpg`, `DATA\Zeus_FE_CampaignSelection.JPG`, `DATA\Zeus_FE_MissionIntroduction.jpg`, `DATA\Zeus_FE_tutorials.JPG`, `DATA\Zeus_FE_Registry.jpg`, `DATA\Poseidon_FE_MainMenu.JPG`, `DATA\Poseidon_FE_Registry.jpg`, `DATA\Zeus_Victory.jpg`, `DATA\Zeus_Defeat.jpg`, `DATA\scoreb.jpg` |
| Voix, sons, vidéo d'introduction | `Audio\` (tout le dossier, ~340 Mo), `Binks\Zeus_Intro.bik`, `Binks\Zeus_Intro_HiQ.bik` |
| Campagnes et tutoriels en français | dans `Adventures\` : les 16 campagnes Zeus (`#La naissance de l'Atlantide.pak` … `}Jeu ouvert - Economie.pak`), les 8 dossiers des aventures Poséidon (`L'odyssée`, `La naissance de Troie`, `La révolte maya`, `Les plus jeunes jumeaux`, `Atlantide, la cité engloutie`, `Jeu ouvert - Armée 2`, `Jeu ouvert - Bac à sable 2`, `Jeu ouvert - Economie 2`), les tutoriels (`agora.pak` … `warrior.pak`) et `tutorial00.sav` … `tutorial15.sav` |

Déplacez les campagnes anglaises hors du dossier `Adventures` (sans les supprimer), sinon chaque campagne apparaît deux fois. Inutile : `Zeus_Editor_Text.eng` (identique dans les deux versions), `Zeus.ini`, `Zeus.inf`. Les sauvegardes restent compatibles dans les deux langues.

## Code source

| Fichier | Rôle |
|---|---|
| `src/FrenchPatch.cs` | le patch FR : table des caractères, hauteur des lignes, boutons, textes du menu |
| `src/FrTable.cs` | table caractère → glyphe de l'édition française (224 octets) |
| `src/Sites.cs` | chaque endroit modifié, avec ses octets d'origine attendus |
| `src/EditorPatch.cs` | reconnaissance des compilations anglaise et française |
| `src/Core.cs` | reconnaissance de l'exe, résolution, copie .bak, écriture |
| `src/Program.cs` | fenêtre et ligne de commande |

Compilation : `build.cmd` (compilateur C# fourni avec Windows, .NET Framework 4) ; `tools/make_release.ps1` prépare le zip de la release. Le patcheur ne vérifie que les octets qu'il modifie : un exe déjà patché pour une autre résolution est accepté, tout ce qui est inattendu est refusé avant d'écrire.

Merci à [zeus-poseidon-arm64](https://github.com/nic0lasm/zeus-poseidon-arm64) (nic0lasm) pour sa documentation de l'affichage du texte. Licence MIT — Vicinity4813. *Zeus* et *Poséidon* sont des jeux d'Impressions Games / Sierra ; aucun fichier du jeu n'est inclus, hormis la table des caractères française de 224 octets.
