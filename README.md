# VR Sushi (nom provisoire)

Jeu de restauration en réalité virtuelle pour tous les casques (en théorie) en PCVR en utilisant OpenXR. L'objectif est de cuisiner les plats que les clients demandent en utilisant les outils et ingrédients à votre disposition.

## Captures d'écran : 

Pas encore de captures d'écran, je préfère peaufiner le jeu encore un peu.

## Fonctionnalités : 

- Intéraction avec ingrédients/plats
- Intéraction avec ustensiles (Couteaux, casseroles)
- Système de cuisson pour certains ingrédients
- Système de découpe d'ingrédients
- Possibilité de faire des plats (sushis)

### Fonctionnalités à venir : 

- Système d'IA pour les clients (Déplacement, demande de plat, attente). NOTE : IA dans le contexte des jeux vidéos, pas une LLM
- Affichage des recettes avec un graphe en arbre
- Ajout de nouveaux plats
- Ajout de nouveaux ustensiles
- Notion de journée de travail avec amélioration des ustensiles à la fin.
- Trouver un meilleur nom pour le jeu


## Installation : 

Pour le moment aucune release du jeu n'est disponible pour jouer mais il est possible de cloner le projet.

Si vous installez le proje, il faudra modifier le package.json pour enlever la dépendance à VHACD car elle est stockée en local sur mon Ordinateur.


## Sources : 

L'asset pack [Sushi Restaurant Kit](https://quaternius.com/packs/sushirestaurantkit.html) de quaternius pour la plupart des modèles 3D que j'ai modifié pour les intégrer au jeu.

Les templates Unity VR template et Unity VR multiplayer template, pour la réutilisation de modèles, et d'interfaces.

Pour la génération de certains colliders j'ai utilisé VHACD une technologie qui permet de génrer plusieurs colliders convexes pour des objects concaves, plus spécifiquement cette version : [Unity-Technologies/VHACD](https://github.com/Unity-Technologies/VHACD) que j'ai du modifier pour avoir une version fonctionelle sur Unity 6 et que vous pouvez trouver à cette endroit [Fork Unity 6](https://github.com/November304/VHACD).

