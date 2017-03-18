# Visualiseur-zSpace

##############################################
############ Problèmes rencontrés ############
##############################################
-Faire un Reset/Inverser Normal/Charger après avoir changer de niveaux de résolutions
-Charger un fichier dat dont les vertex ne sont pas triés par ordre décroissant (les vertex des niveaux de résolutions faibles en premier ...)

##############################################
############ Fonctionnalités #################
##############################################
-Chercher un fichier dat via un explorateur de fichier.
-Charger un fichier Dat dans Unity
-Afficher un fichier Dat
-Changer de niveaux de résolution
-Manipulation à la souris (rotation, translation, scale)
-Manipulation au stylet
-Découpage de surface au stylet avec surbrillance des zones selectionnées
-Mode Wireframe

##############################################
################## Projet ####################
##############################################

##### 1) Scripts ######
- Load_dat_bis.cs
- StylusObjectManipulationSample.cs
- NewBehaviourScript1.cs

##### 2) Load_dat_bis ######
Ce fichier contient deux classes.

La premières est MRMesh qui contient une structure permettant de stocker toutes les informations des fichiers dat, il y a 2 tableaux très importants : vertices et triangles qui stock un tableau de vertex/triangles pour chaque triangle primaire de l'objet. Ensuite pour accéder aux différents niveaux de résolution au sein d'un tableau, on utilise face_res pour le tableau triangles qui contient les index où aller chercher les niveaux de résolution dans le tableau triangles pour chaque triangle primaire et permet de stocker tout ça dans current_triangles qui est un tableau contenant les triangles du niveau de résolution actuel pour chaque triangle primaire.

La deuxième classe est Load_dat_bis qui hérite de MonoBehaviour.
Elle contient la fonction Start qui permet de charger une première fois un fichier dat.
La fonction Update
###### 3) StylusObjectManipulationSample ######

###### 4) NewBehaviourScript1 ######
