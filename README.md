# Visualiseur-zSpace

## Projet
- Load_dat_bis.cs
- StylusObjectManipulationSample.cs
- NewBehaviourScript1.cs

### 1) Load_dat_bis
Ce fichier contient deux classes.

La première est MRMesh qui contient une structure permettant de stocker toutes les informations des fichiers dat, il y a 2 tableaux très importants : vertices et triangles qui stock un tableau de vertex/triangles pour chaque triangle primaire de l'objet. Ensuite pour accéder aux différents niveaux de résolution au sein d'un tableau, on utilise face_res pour le tableau triangles qui contient les index où aller chercher les niveaux de résolution dans le tableau triangles pour chaque triangle primaire et permet de stocker tout ça dans current_triangles qui est un tableau contenant les triangles du niveau de résolution actuel pour chaque triangle primaire.

La deuxième classe est Load_dat_bis qui hérite de MonoBehaviour.
Elle contient la fonction Start qui permet de charger une première fois un fichier dat.
La fonction Update prend en charge l'interaction avec la souris, les boutons de l'interface graphique et l'explorateur de fichiers.

### 2) StylusObjectManipulationSample
Ce fichier est d'abord le script fourni dans les ressources du zSpace disponible sur le site officiel, il y a en plus des fonctions permettant de saisir plusieurs objets en même temps à travers les fonctions BeginMultiple et UpdateMultiple.
La fonction BeginGrabCut permet en maintenant le bouton arrière gauche du stylet de sélectionner et de colorier en rouges le groupe d'objet à déplacer. Pour selectionner tous les objets en même temps il faut appuyer sur le bouton arrière droit.

### 3) NewBehaviourScript1 
Cette classe contient l'explorateur de fichiers.

## Fonctionnalités
-Chercher un fichier dat via un explorateur de fichiers.
-Charger un fichier Dat dans Unity
-Afficher un fichier Dat
-Changer de niveaux de résolution
-Manipulation à la souris (rotation, translation, scale)
-Manipulation au stylet
-Découpage de surface au stylet avec surbrillance des zones selectionnées
-Mode Wireframe

## Problèmes rencontrés
-Faire un Reset/Inverser Normal/Charger après avoir changer de niveaux de résolutions entraîne un bug et n'affiche plus l'objet 3D.
-Charger un fichier dat dont les vertex ne sont pas triés par ordre décroissant (les vertex des niveaux de résolutions faibles en premier ...)
