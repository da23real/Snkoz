# Snkoz V1 — installation A01

## 1. Windows

Version prototype issue de Snkoz V9.42 R5. Utiliser Windows avec .NET Framework 4.x (4.8 conseillé), les composants Windows Media Player pour la preview et un dossier accessible en écriture. Sur Windows N, les composants multimédias peuvent devoir être activés.

Installer les dépendances externes selon [DEPENDENCIES](../../DEPENDENCIES.md) : elles ne sont pas incluses dans le ZIP public. Conserver ensuite ensemble `Windows/Snkoz-A01.exe`, `Windows/English.tsv` et le dossier `Dependencies` avec FFmpeg, FFprobe, NVIDIA, platform-tools et BodyMask. Lancer `Windows/Snkoz-A01.exe` ou créer un raccourci vers cet exécutable. Une session déjà ouverte conserve son ancienne version : la fermer normalement et relancer pour afficher **Snkoz V1**.

Choisir la langue dans **Montage > Éditer > Langue**, puis redémarrer. Les préférences existantes sont conservées. Pour une nouvelle installation, les prises vont par défaut dans `Vidéos/Snkoz/Recordings`, les configurations dans `Documents/Snkoz/Config`. Choisir les dossiers dans Prise. Les préférences et récupérations sont dans `%LOCALAPPDATA%/Snkoz`.

L’onglet Urban Kaoz concerne une interface de séquenceur matériel optionnelle. Il n’est pas nécessaire à la prise et au montage MIDI universels. Aucun firmware n’est installé avec Snkoz.

## 2. Caméra Android

L’APK `Android/Prise-USB-A01.apk` nécessite Android 8/API 26 ou supérieur et une caméra compatible. Il est préparé et signé, mais n’a pas été installé ni testé sur le téléphone pour cette livraison.

Installer l’APK comme mise à jour de `fr.roy.obsusb`, en conservant les données existantes. Il garde la signature de développement précédente et utilise `versionCode 100001`, supérieur à 94205. Le nom visible est **V1**. Ne pas désinstaller l’ancienne application pour contourner un refus de mise à jour : ses prises privées pourraient être perdues.

Autoriser caméra, microphone et notifications si demandées. Choisir la caméra, la qualité et le dossier ; 720p constitue un bon premier essai. Activer le débogage USB, relier un câble de données et autoriser ce PC sur le téléphone. Attendre « READY FOR PC », puis **Prise > Actualiser sources** dans Snkoz. La rotation de preview ne change que son affichage.

La vidéo est enregistrée sur le téléphone puis transférée par USB après Stop ; la préparation peut donc continuer après l’arrêt. Cette fonction ne nécessite pas de Wi-Fi. Vérifier les restrictions de batterie du téléphone avant une longue prise. Les prises du dossier privé peuvent disparaître à la désinstallation.

## 3. MIDI et dépendances

Brancher l’interface MIDI puis **Prise > Rafraîchir MIDI**. Choisir les ports et cliquer **Connecter MIDI**. Le rafraîchissement conserve les ports choisis lorsqu’ils sont encore présents. Le MIDI transporte des commandes ; sélectionner séparément la source audio de l’instrument.

L’encodeur Automatique tente NVIDIA puis revient au logiciel si nécessaire. Les effets, masques et préparations peuvent rester sur le CPU. Aucun pilote n’est installé par Snkoz. Conserver les licences des dépendances avec la livraison.

## 4. Fichiers et versions

Un `.snkozproject` référence les médias : garder les dossiers de prises et d’assets. Le format 9450 de R5 est conservé ; les lecteurs plus anciens sans prise en charge de 9450 restent incompatibles. Les récupérations `.snkozrecovery` utilisent une enveloppe distincte A01 et conservent les références médias.

La désignation A01/V1 ne change pas la version réellement installée sur les appareils. Windows utilise techniquement 10.1.0.1 ; Android utilise 100001/V1. [Correspondance complète](../../VERSION.md). Aucun séquenceur Arduino, clé de signature, prise ou configuration personnelle ne doit entrer dans une future distribution sans autorisation adaptée.

[Utilisation](../Usage/README_FR.md) · [English](README.md).
