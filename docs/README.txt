Zeus Patcher 1.0 - Zeus: Master of Olympus / Poseidon
=====================================================

FRENCH PATCH: turns an English Zeus.exe into a French one
   (French interface only: the button is greyed out in the English interface)
   For an English 2.1.4.0 Zeus.exe: GOG, Steam and all its widescreen versions
   (JackFuste exes, Resolution Customiser...), at any resolution.
   Fixes what the English exe gets wrong with French texts: accents, line heights,
   buttons too narrow, misplaced menu texts.

The program only changes the chosen Zeus.exe. A copy of the original (Zeus.exe.bak)
is made next to it first; the "Put the original exe back" button restores it.


A 100% French game from the English game
----------------------------------------
The French patch fixes the exe; the French texts, fonts, pictures and voices come from
YOUR official French game (CD or install). Copy them into the English game folder, at
the same places:

  Required (texts and accented letters):
    Zeus_Text.eng
    Zeus_MM.eng
    Model\Zeus eventmsg.txt
    DATA\Zeus_Fonts.sg3
    DATA\zeus_Fonts.555

  Menu and end screen pictures (French text):
    DATA\Zeus_FE_ChooseGame.jpg
    DATA\Zeus_FE_CampaignSelection.JPG
    DATA\Zeus_FE_MissionIntroduction.jpg
    DATA\Zeus_FE_tutorials.JPG
    DATA\Zeus_FE_Registry.jpg
    DATA\Poseidon_FE_MainMenu.JPG
    DATA\Poseidon_FE_Registry.jpg
    DATA\Zeus_Victory.jpg
    DATA\Zeus_Defeat.jpg
    DATA\scoreb.jpg

  Voices, sounds and intro video:
    Audio\                      (the whole folder, about 340 MB)
    Binks\Zeus_Intro.bik
    Binks\Zeus_Intro_HiQ.bik

  French campaigns and tutorials (Adventures\ folder):
    the 16 Zeus campaigns (#La naissance de l'Atlantide.pak ... }Jeu ouvert - Economie.pak),
    the 8 Poseidon adventure folders (L'odyssée, La naissance de Troie, La révolte maya,
    Les plus jeunes jumeaux, Atlantide, la cité engloutie, Jeu ouvert - Armée 2,
    Jeu ouvert - Bac à sable 2, Jeu ouvert - Economie 2),
    the tutorials (agora.pak ... warrior.pak) and tutorial00.sav to tutorial15.sav.
    Move the English campaigns out of the Adventures folder (do not delete them):
    otherwise every campaign shows up twice.

  Not needed: Zeus_Editor_Text.eng (same in both versions), Zeus.ini, Zeus.inf.
  Saved games work in both languages.


Full-screen world maps and scenario editor
------------------------------------------
Use Zeus World Map Converter (Convertisseur mappemonde,
https://github.com/Vicinity4813/zeus-world-maps): choose the game's Zeus.exe, it
recognises its version, language and resolution, patches the scenario editor for the
enlarged world map (any size, English or French exe), then moves the cities, region
names and trade routes of scenarios (.pak, .set, .map) and saved games (.sav).


Not recognised: Russian exes, protected original CD exes (SecuROM).
Windows may show "Windows protected your PC" the first time (new, unsigned program):
"More info", then "Run anyway".

Thanks to zeus-poseidon-arm64 (nic0lasm) for documenting the text rendering.
MIT License - Vicinity4813. Zeus and Poseidon are games by Impressions Games / Sierra.
