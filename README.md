# Zeus - EXE French Patch

**Patch FR pour *Zeus : Le Maître de l'Olympe* / *Poséidon***

Met en français un jeu anglais, en deux étapes indépendantes :

- **Le patch de l'exe** rend un **`Zeus.exe` anglais** (2.1.4.0 : GOG, Steam et toutes ses versions widescreen, quelle que soit la résolution) capable d'afficher correctement les textes français : accents, hauteur des lignes, polices choisies comme dans l'édition française (titres, boutons, messages), boutons élargis, position des textes du menu, fenêtres Finances, Culture, réception des messages et didacticiels élargies, comme l'édition française. Seul le `Zeus.exe` choisi est modifié : une copie `Zeus.exe.bak` de l'original est faite d'abord, et un bouton la remet en place.
- **L'import** copie dans le dossier du jeu les textes, polices, voix, campagnes et tutoriels de **votre propre jeu français officiel**. Celui-ci n'est que lu. Les fichiers anglais remplacés et les campagnes anglaises sont déplacés dans un dossier `Sauvegarde anglais`, et un bouton les remet en place.

Pour les mappemondes plein écran et le **patch de l'éditeur de scénarios** (n'importe quelle résolution, n'importe quelle mappemonde, exe anglais ou français), voir **[World Map Converter (Zeus - World Maps Tools)](https://github.com/Vicinity4813/zeus-world-maps-tools)**.

## Téléchargement

`Zeus-EXE-French-Patch-1.1.zip` sur la page **[Releases](../../releases)** : le programme, sa notice et le code source.

> Windows peut afficher « Windows a protégé votre ordinateur » au premier lancement (programme récent, non signé) : **Informations complémentaires**, puis **Exécuter quand même**.

## Utilisation

1. Lancez `Zeus EXE French Patch.exe` et choisissez votre `Zeus.exe` : le programme indique ce qu'il a trouvé (exe anglais ou français, résolution).
2. Cliquez sur **Patcher en français**.
3. Cliquez sur **Importer le jeu français…** et choisissez le dossier de votre jeu français installé (celui qui contient `Zeus_Text.eng`). Il faut environ 500 Mo de place libre.

**Remettre l'exe d'origine** remet la copie `.bak` ; **Remettre les fichiers anglais** retire les fichiers français et remet le dossier `Sauvegarde anglais`. Les exe widescreen de JackFuste, de 1280×720 à 3440×1440, ont les mêmes octets aux endroits modifiés. Non reconnus : versions russes et exe du CD d'origine protégés (SecuROM).

## Ce que copie l'import

Le patch corrige l'exe ; les textes, polices et voix françaises viennent de **votre propre jeu français officiel** (CD ou installation). L'import copie les fichiers ci-dessous, aux mêmes emplacements. Vos scénarios ajoutés ne sont pas copiés. Sans le bouton, vous pouvez aussi les copier à la main :

| Pour | Fichiers |
|---|---|
| **Textes et lettres accentuées** (indispensable) | `Zeus_Text.eng`, `Zeus_MM.eng`, `Model\Zeus eventmsg.txt`, `DATA\Zeus_Fonts.sg3`, `DATA\zeus_Fonts.555` |
| Textes de l'éditeur de scénarios | `Zeus_Editor_Text.eng` |
| Voix, sons, vidéo d'introduction | `Audio\` (tout le dossier, ~340 Mo), `Binks\Zeus_Intro.bik`, `Binks\Zeus_Intro_HiQ.bik` |
| Campagnes et tutoriels en français | dans `Adventures\` : les 16 campagnes Zeus (`#La naissance de l'Atlantide.pak` … `}Jeu ouvert - Economie.pak`), les 8 dossiers des aventures Poséidon (`L'odyssée`, `La naissance de Troie`, `La révolte maya`, `Les plus jeunes jumeaux`, `Atlantide, la cité engloutie`, `Jeu ouvert - Armée 2`, `Jeu ouvert - Bac à sable 2`, `Jeu ouvert - Economie 2`), les tutoriels (`agora.pak` … `warrior.pak`) et `tutorial00.sav` … `tutorial15.sav` |

Les campagnes anglaises doivent quitter le dossier `Adventures`, sinon chaque campagne apparaît deux fois : l'import s'en charge, à la main déplacez-les sans les supprimer. Inutile : les images de `DATA` (les mêmes dans les deux versions, sans texte), `Zeus.ini`, `Zeus.inf`. Les sauvegardes restent compatibles dans les deux langues.

## Code source

| Fichier | Rôle |
|---|---|
| `src/FrenchPatch.cs` | le patch FR : table des caractères, hauteur des lignes, boutons, textes du menu |
| `src/ReglePolices.cs` | règle des polices de l'édition française, règles propres à certains textes, fenêtres élargies |
| `src/FrenchFiles.cs` | import des fichiers d'un jeu français installé et retour aux fichiers anglais |
| `src/FrTable.cs` | table caractère → glyphe de l'édition française (224 octets) |
| `src/Sites.cs` | chaque endroit modifié, avec ses octets d'origine attendus |
| `src/EditorPatch.cs` | reconnaissance des compilations anglaise et française |
| `src/Core.cs` | reconnaissance de l'exe, résolution, copie .bak, écriture |
| `src/Program.cs` | fenêtre et ligne de commande |

Compilation : `build.cmd` (compilateur C# fourni avec Windows, .NET Framework 4) ; `tools/make_release.ps1` prépare le zip de la release. Le patcheur ne vérifie que les octets qu'il modifie : un exe déjà patché pour une autre résolution est accepté, tout ce qui est inattendu est refusé avant d'écrire.

Merci à [zeus-poseidon-arm64](https://github.com/nic0lasm/zeus-poseidon-arm64) (nic0lasm) pour sa documentation de l'affichage du texte. Licence MIT — Vicinity4813. *Zeus* et *Poséidon* sont des jeux d'Impressions Games / Sierra ; aucun fichier du jeu n'est inclus, hormis la table des caractères française de 224 octets.
