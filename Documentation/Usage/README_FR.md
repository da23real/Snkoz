# Snkoz R21 - démarrage musical

[Vidéo](https://youtu.be/prQQBhpKYqU) · [Nouveautés et corrections, PDF](../R21/Snkoz_R21_Nouveautes_et_corrections.pdf)

## 1. Relier MIDI et image

Ouvre le Tableau des effets d’un bloc Montage. Ajoute explicitement une source vidéo, une source MIDI et les effets, textes ou animations souhaités. Tire les connexions entre ports compatibles. Un tableau neuf ne choisit pas de caméra implicitement.

Filtre l’entrée par canal, note ou CC selon les données présentes. Choisis la donnée pilotant le paramètre et ses plages d’entrée/sortie. Une plage descendante inverse la correspondance. Les événements ne sont pas additionnés implicitement.

Pour un texte CC, choisis numéro, valeur ou les deux. Le mode guidé protège la lisibilité ; « Valeurs libres » autorise les réglages étendus. Pour les animations, choisis images, banques et plages ; les presets génériques n’imposent pas d’images.

« Paramètres MIDI disponibles » règle la visibilité des affectations, pas les canaux ou CC. Les paramètres affectés restent accessibles et les choix du projet priment.

## 2. Affiner

Les notes peuvent utiliser Gate et enveloppe ; les CC continus suivent leur valeur. Lissage musical pour les effets, millisecondes pour Texte. La cadence vidéo est distincte de celle des paramètres.

Dans le mélangeur, la durée minimale en mode Valeur MIDI évite les basculements brefs. Le seuil de silence des images d’attente ignore l’activité faible : deux réglages distincts. « Voir » montre une branche au point de pause sans écrire d’automation.

[Les simulations](../R21/Snkoz_R21_Conception_avec_IA.pdf) expliquent les plages utiles. Toutes les familles d’effets ne disposent pas de protections automatiques.

## 3. Passes CC

Le bouton d’automations du Montage ou CC du preview ouvre l’enregistrement. Choisis entrée, canal et éventuellement numéros CC. Avec preview, les événements suivent la lecture ; la pause n’ajoute rien. Sans vidéo, une horloge de montage est utilisée, avec écoute audio possible.

Après arrêt, « Ajouter cette passe au montage » crée une piste MIDI indépendante. Relie-la aux paramètres voulus.

## 4. Écoute et manipulation

Mutes audio/vidéo indépendants par piste ; ordre Gain/Égaliseur/Compresseur/Limiteur réglable. La forme d’onde dBFS représente la source.

Le lecteur direct montre l’original sans FX ; le preview calculé demande une actualisation pour les nouveaux réglages. La dernière prise se retrouve dans Ouvrir. Deux annulations de suppression dans le tableau ; mute réversible directement. Ajouter un module n’ouvre pas ses paramètres. Les miniatures peuvent être reconstruites par piste, orientation comprise.

Voir [VERSION.md](../../VERSION.md) pour les anciens fichiers et les limites. La validation n’est pas exhaustive sur matériel, longs projets ou 4K.
