# Snkoz VA01.15 R21

- Application Windows : **10.1.15.20**, livraison du 11 octobre 2026.
- Préparation publique : 12 octobre 2026.
- Exécutable original : `Snkoz-VA01.15-R21.exe`.
- SHA-256 original : `564a090f283337cf650a0bb06cfa91c00ebf43936770eacbdf8ec5833ed11d42`.
- Base de développement : Windows R20, 10.1.15.19.
- Vidéo : https://youtu.be/prQQBhpKYqU

## 1. Validation

Le compte rendu R21 rapporte 475 contrôles réussis sur la livraison entière : fixtures MIDI, rendus FFmpeg, interfaces synthétiques, audio et neuf essais du lecteur Windows réel. Ce total n’est ni un nombre de bugs corrigés ni une couverture exhaustive.

La publication vérifie séparément l’identité des 219 sources C#, leur compilation depuis l’arbre public, l’empreinte du binaire original, le contenu de l’archive et les fichiers publiés. Aucune nouvelle validation musicale ou matérielle n’en est déduite.

Restent à confirmer : son métallique/faible sur le matériel utilisateur, intermittence souris, premier plan Windows et multi-écran physiques, longues sessions et 4K. L’ancien cas d’animation vocale manquante n’est pas identifié. Les réglages guidés ne protègent pas toutes les combinaisons de tous les effets.

## 2. Formats

| Type | Lecture | Écriture |
| --- | --- | --- |
| Projet / montage | 100218 à 100232 | 100232 |
| Graphe | 3 | 3 |
| Preset modulaire | 1 à 13 | 13 |
| Preset de sortie | 1 à 6 | 6 |
| Récupération | 5 à 19 | 19 |

**R20 refuse les nouvelles écritures R21.** Les formats antérieurs à 100218 restent refusés, sans migration automatique. Depuis R19, les Timestamp sont locaux au bloc : les anciens repères saisis comme absolus doivent être vérifiés. La lecture sur fixtures ne garantit pas toutes les archives personnelles.

## 3. Périmètre

Windows R21, deux PDF, vignette et guides. Aucun firmware, code de séquenceur, outil privé de conversion, clé, configuration personnelle ou média utilisateur. Les sources Android A01 restent inchangées et distinctes de l’APK Android R25 séparé.

Les PDF portent la mention « dossier préparatoire » car ils ont été préparés et relus avant publication ; leur contenu est conservé dans cette release.
