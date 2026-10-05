# Snkoz V1 — utilisation A01

## 1. Prise

Brancher les caméras et interfaces audio, puis **Actualiser sources**. Ajouter et configurer les pistes : vidéo, audio ou les deux, résolution, cadence et nom court. Vérifier chaque preview avant une longue prise. Choisir le dossier ; Sauver/Charger config conserve les choix de sources.

Pour un usage indépendant du Teensy, choisir **MIDI maître PC**, **Rafraîchir MIDI**, sélectionner entrée/sortie puis **Connecter MIDI**. Régler le tempo. L’instrument doit recevoir l’horloge et Start/Stop ; son audio doit être capturé séparément. L’entrée MIDI fournit notes, CC et programmes aux effets. Éviter une boucle directe entre sortie et entrée MIDI.

La note de calage optionnelle envoie la note MIDI 60 avant Start. Enregistrer démarre les sources et le transport. Stop conserve la dernière mesure partielle ; Pause suit les mesures complètes. Attendre finalisation et transfert Android. En MIDI esclave, Start ne recoupe pas automatiquement le début : régler ensuite les latences.

## 2. Fichier et récupération

**Fichier > Nouveau montage** vide l’éditeur sans supprimer les médias. **Ouvrir > Dossier** charge une prise ; **Prises** ouvre son dossier dans l’Explorateur ; **Projet** ouvre un `.snkozproject`.

**Enregistrer** met à jour le fichier principal ; **Enregistrer sous** choisit un autre fichier. Sans projet nommé, Enregistrer demande son nom. Les modifications automatiques ne réécrivent ni le fichier principal ni le montage source de la prise.

Après trois secondes sans modification, et au moins quinze secondes entre deux écritures automatiques, Snkoz prépare une récupération puis sérialise et écrit en arrière-plan. Déplacements et mutes ne font que marquer les changements : aucune copie complète des événements MIDI ni écriture synchrone à chaque geste. Les récupérations sont suspendues pendant la prise. Changer de montage met les modifications en attente en récupération.

**Fichier > Ouvrir > Récupération** propose les **cinq fichiers les plus récents, tous projets confondus pour l’utilisateur Windows**, avec nom et date locale. Ouvrir une récupération ne modifie pas le projet manuel. Enregistrer explicitement met ensuite à jour son fichier d’origine ; Enregistrer sous permet d’en garder une variante. Les médias doivent toujours être présents. Les dernières secondes peuvent manquer si l’application se ferme avant la récupération : utiliser Enregistrer pour conserver immédiatement un état voulu.

## 3. Position et montage

Les trois champs numériques sont **mesure (à partir de 1), temps (1–4), subdivision (0–3)**. Ils déplacent la tête ; Entrée valide une valeur saisie. Une position au-delà de la durée est ramenée à la fin exacte du projet. Les flèches déplacent la tête selon le pas choisi. Le curseur Zoom change l’échelle, la barre inférieure déplace la vue. Le bouton **▶** à droite ouvre la preview de la piste, avec une infobulle. Sa largeur réduite laisse davantage de place à la timeline.

La règle des mesures reste au-dessus des pistes pendant leur défilement vertical. Les lignes passent derrière les blocs ; les blocs les recouvrent. À faible zoom, l’affichage espace les repères pour rester lisible.

Clic simple sélectionne un bloc, Ctrl+clic ajoute ou retire un bloc. Cliquer dans la règle ou un espace vide place la tête sans effacer la sélection. Ctrl+A sélectionne tout, MIDI compris ; C sélectionne la colonne au curseur. **Éditer > Couper au curseur / Ctrl+B** coupe les blocs sélectionnés traversés par la tête. **Éditer > Supprimer > Bloc** conserve **Suppr** et laisse le trou ; le menu contextuel permet aussi de refermer une tranche complète. **Éditer > Supprimer > Piste > nom** supprime la piste entière, même vide, sans raccourci. Annuler restaure cette suppression dans les deux niveaux existants. Les médias sources sont conservés. Supprimer la piste MIDI retire ses événements du montage mais conserve la grille musicale.

Pour combler un vide, sélectionner les blocs à droite sur toutes les pistes concernées, MIDI compris, puis **Combler le vide**. Le décalage commun conserve leur synchronisation. Glisser déplace horizontalement ; aucun événement MIDI interne n’est quantifié. Le déplacement ne change ni l’espacement interne des notes/CC/programmes, ni la grille musicale. Une note traversant une coupe est bornée aux blocs résultants. Le rendu audio/vidéo garde sa cadence propre.

**Éditer > Copier / Coller**, Ctrl+C/Ctrl+V, copie les blocs sélectionnés et leurs liaisons A/V, puis colle au curseur sur les mêmes pistes ; les chevauchements sont refusés. Le presse-papiers est propre au montage ouvert. Deux annulations structurelles sont conservées pour coupes/suppressions. Déplacement, collage et mute n’ajoutent pas de niveau ; mute se réactive directement.

Les commandes permanentes tiennent sur une ligne : Contrôleur, pictogramme joysticks avec infobulle, position, pas, flèches et Zoom. Les commandes matérielles utiles restent disponibles. Quitter un champ numérique par un clic dans la timeline avant les raccourcis de montage si nécessaire.

## 4. Latences, écoute et effets

**Éditer > Paramètres du projet** donne accès aux latences et à l’audio de preview. Comparer une action visible, la waveform et un événement MIDI connu. Les décalages audio/vidéo sont indépendants : positif retarde, négatif avance. Le MIDI reste fixe. Les exports corrigés portent ces réglages ; les bruts restent inchangés.

Audio de preview est global : automatique, Off ou mélange choisi. Il ne définit pas le mix final. Éviter de doubler le son en mélangeant inutilement micro externe et micro caméra.

Le menu contextuel d’un bloc vidéo distingue **FX de piste** (valeurs par défaut) et **FX du bloc** (override indépendant). Ajouter un effet et son déclencheur Note/CC/Début mesure, canal et paramètres. Les effets se succèdent dans l’ordre de la liste. Un effet MIDI Mute atténue les effets suivants ; il diffère du mute d’un bloc.

La preview FX part du déclenchement choisi dans le bloc, avec une courte avance bornée. Choisir une faible résolution pour les premiers essais. Les modifications d’effets entrent dans les récupérations ; Enregistrer est nécessaire pour modifier le projet principal. Les presets permettent de réutiliser les effets.

## 5. Médias, animations et masques

**Insérer > Média** demande d’abord PNG, GIF ou MP4, puis sa position musicale ; il crée une nouvelle piste. Régler ensuite durée, taille et placement. **Piste transparente** crée un support graphique. L’insertion contextuelle dans un trou reste disponible. Un média fixe dans la timeline est distinct d’une animation déclenchée par MIDI.

Les animations FX acceptent notamment PNG fixe/planche 4×2, GIF et MP4 court. Régler cadence, images de départ/fin, boucles et Gate selon l’effet souhaité. Les notes peuvent sélectionner les images. Vérifier un extrait avant l’export complet.

Après installation de ses dépendances optionnelles, le masque corporel expérimental détecte une silhouette localement ; ce n’est pas un suivi de squelette. Ajuster ses bords et essayer sur la vidéo réelle. Éclairage, occultation et plusieurs personnes peuvent dégrader le résultat. Ce masque n’est pas disponible en Direct.

## 6. Export et essais réels

**Fichier > Exporter > Montage complet** rend le composite ; **Piste > nom de piste** rend une piste. Choisir résolution et encodeur. Les pistes vidéo hautes couvrent les basses selon leur opacité ; l’audio actif est mélangé indépendamment.

L’export montage s’arrête à la fin du dernier bloc vidéo non muté sur une piste non mutée, audio compris. Les trous intermédiaires restent. Si tout est muté, l’export vide est refusé. L’export par piste garde son comportement propre. Annuler abandonne le rendu ; conserver une partie exige qu’un segment soit déjà finalisé.

Les tests synthétiques ne remplacent pas une prise caméra/audio/MIDI réelle, un essai des commandes physiques et une écoute du calage. L’APK A01 n’est pas installé sur le téléphone dans cette livraison. Certains diagnostics historiques restent français. Cette version publique reste expérimentale.

[Installation](../Installation/README_FR.md) · [English](README.md).
