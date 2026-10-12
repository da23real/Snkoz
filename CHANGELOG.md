# Nouveautés et corrections de Snkoz R21

[Vidéo](https://youtu.be/prQQBhpKYqU) · [Journal détaillé PDF](Documentation/R21/Snkoz_R21_Nouveautes_et_corrections.pdf)

## 1. R21 par rapport à R20

- Passes CC avec preview ou sans vidéo, écoute audio possible et piste MIDI indépendante.
- Durée minimale avant changement de source ; audition de branche au point de pause.
- Seuil de silence des images d’attente ; banques vides avec plages visibles.
- Visibilité des paramètres MIDI, affectations signalées et priorité aux choix du projet.
- Ordre des traitements audio ; échelle dBFS, chemin PCM corrigé, RMS et limiteur à anticipation.
- Mutes audio et vidéo indépendants.
- Cadre fixe, rotation précise et format depuis Sortie.
- Deux annulations réservées aux suppressions de modules ; identité évitant les tableaux doublons ; ajout sans ouverture automatique des paramètres.
- Réinitialisation des gestes après perte de focus ; intermittence réelle encore à confirmer.
- Contour étendu explicite ; lecteur direct avec départ/reprise corrigés ; progression agrandie et dernière prise dans Ouvrir.
- Miniatures orientées et reconstruction par piste ; paramètres fixés au départ du rendu.

## 2. Historique cumulé

Le PDF recense la série VA01.15 jusqu’à R21 : graphe d’effets, convertisseurs, coupes et phases, ordre des blocs, presets, plages MIDI/images, import MIDI, VoixMidi, filtrage CC, texte guidé, simulations, banques, mouvement X/Y, mélangeur et corrections d’interface.

Il distingue fonctions remplacées, bugs reproduits/corrigés et problèmes ouverts. Il ne revendique pas un diff exhaustif de toutes les versions antérieures du projet.

## 3. Conception avec l’IA

[Le second PDF](Documentation/R21/Snkoz_R21_Conception_avec_IA.pdf) décrit les cas et mesures : 8 111 CC filtrés, lisibilité du texte, impulsions manquées à 30 images/s, paramètres extrêmes, stabilisation et bugs intermittents d’interface.

Voir [VERSION.md](VERSION.md) pour les formats et les limites.
