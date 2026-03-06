# SURVIVE — Mod de survie exigeante pour Vintage Story

## Cahier des Charges — v0.1

---

## 1. Présentation du projet

### 1.1 Contexte

Vintage Story propose une expérience de survie riche mais dont les mécaniques restent superficielles sur certains aspects fondamentaux : la santé y est une simple barre de points de vie, le sommeil n'a quasiment aucune utilité mécanique, et la psychologie du personnage est totalement absente.

Les mods existants (SleepNeed, Balanced Thirst, Hydrate or Diedrate) traitent ces problèmes de manière isolée et sans cohérence entre eux. Survive vise à créer un système unifié, cohérent et narrativement fort.

### 1.2 Objectif

Créer un mod C# pour Vintage Story introduisant cinq piliers de survie interdépendants, dont un système de santé mentale inédit avec des effets progressifs — incluant une "perception élargie" qui ouvre des pans cachés du monde aux joueurs ayant atteint un état mental dégradé.

### 1.3 Public cible

- Joueurs appréciant la survie exigeante et le roleplay
- Joueurs souhaitant une progression narrative émergente
- Compatible solo et multijoueur (certaines mécaniques à ajuster en multi)

### 1.4 Les cinq piliers

| Pilier | Conséquence si vide | Interactions |
|--------|---------------------|--------------|
| 💧 Hydratation | Dégâts, ralentissement, faim accélérée | Fatigue, Santé mentale |
| 🍖 Satiété | Perte de PV, fatigue doublée | Hydratation |
| 😴 Fatigue | Malus cognitifs → effondrement | Santé mentale, Blessures |
| 🩹 Intégrité physique | Blessures localisées, infection | Fatigue, Satiété |
| 🧠 Santé mentale | Sabotage progressif → crise → perception élargie | Tous les piliers |

Chaque pilier interagit avec les autres — la gestion n'est jamais isolée.

---

## 2. Hydratation & Satiété

### 2.1 Interactions croisées

Ces deux jauges s'influencent mutuellement et ne peuvent être gérées indépendamment :

- Déshydraté → la faim descend plus vite (le métabolisme s'emballe)
- Affamé → transpiration inefficace → surchauffe plus rapide → soif augmentée
- Bien hydraté → satiété réduite moins vite (–30% au maximum)
- Déshydraté → satiété réduite plus vite (+30% au maximum)

### 2.2 Sources d'eau

Toutes les sources d'eau ne sont pas équivalentes :

- Eau de rivière / lac → buvable mais risque de maladie sans ébullition
- Eau de pluie → propre, collectible avec un récipient posé à l'extérieur
- Eau de neige fondue → propre mais froide (malus de température)
- Eau de source → rare, toujours propre, pas d'ébullition nécessaire

### 2.3 Qualité de l'hydratation

- Boissons chaudes (tisane, soupe) → hydratation + réchauffement simultanés
- Alcool → hydratation à court terme, augmente la soif ensuite (voir §5)
- Fruits → hydratation légère sans nécessiter de gourde

### 2.4 Gestion des contenants

- Eau non bouillie stockée trop longtemps → devient insalubre
- Gourde en cuir → conservation plus longue qu'un bol en bois
- Boire directement dans une rivière → possible en s'accroupissant dans l'eau

---

## 3. Fatigue

La fatigue est un système en trois phases progressives. La qualité du sommeil dépend du lit, de l'abri, de la température et de l'état de santé mentale du personnage.

### 3.1 Phases de fatigue

| Phase | Effets |
|-------|--------|
| Phase 1 — Fatigué | Champ de vision réduit, craft a une chance d'échouer, endurance réduite de 15% |
| Phase 2 — Épuisé | Outils s'usent plus vite, santé mentale descend plus vite, micro-somnolences |
| Phase 3 — Effondrement | Personnage s'assoit seul, perte de contrôle brève, impossible de courir ou combattre |

### 3.2 Qualité du sommeil

Dormir ne suffit pas — les conditions influencent la récupération :

- Paille → récupération lente
- Lit en bois → récupération normale
- Lit en laine avec oreiller → bonus de récupération
- Dormir dehors sans abri → sommeil agité, récupération incomplète
- Dormir avec un compagnon animal → amélioration de la qualité
- État mental dégradé → insomnie partielle, récupération réduite
- Rythme circadien : dormir de jour = récupération réduite de moitié

---

## 4. Intégrité physique

La santé n'est plus une seule barre mais quatre zones corporelles distinctes. La mort ne survient pas d'un coup — elle se voit venir.

### 4.1 Zones corporelles

| Zone | Conséquence |
|------|-------------|
| 🦵 Jambes | Vitesse de déplacement réduite |
| 💪 Bras | Vitesse de minage et dégâts réduits |
| 🫀 Torse | Résistance aux dégâts entrants réduite |
| 🧠 Tête | Clarté réduite — effets visuels, craft raté plus souvent |

### 4.2 Stades de blessure

| Stade | Soin requis | Symptôme associé |
|-------|-------------|-------------------|
| 1 — Légère | Spontané (temps + sommeil) | Aucun symptôme visible |
| 2 — Ouverte | Bandages | Saignement, perte de PV lente |
| 3 — Infectée | Plantes médicinales + bandages | Fièvre — soif ×2, fatigue accélérée |
| 4 — Grave | Soins avancés (splinte, suture…) | Incapacité partielle selon la zone |

### 4.3 Ressources médicales

Les soins nécessitent des ressources craftables ou trouvables dans le monde :

- Bandages → tissu + eau bouillie
- Plantes médicinales → récoltables dans le monde, certaines rares
- Splinte → bois + bandage (fractures de jambe)
- Suture → aiguille + fil + plante anesthésiante (blessures graves)

Les recettes avancées peuvent être trouvées via le mod Traces (points d'intérêt).

---

## 5. Santé mentale

La santé mentale est le pilier le plus original du mod. Ce n'est pas une barre à remplir — c'est un état qui teinte progressivement la perception du monde. Elle interagit avec tous les autres piliers.

### 5.1 Causes de dégradation

**Survie**
- Rester longtemps seul sans interaction sociale
- Dormir dans un endroit non sécurisé (sans murs, sans toit)
- Être blessé gravement sans pouvoir se soigner
- Manque de lumière prolongé (hivers longs)
- Manger de la nourriture de mauvaise qualité en continu
- Perdre des objets auxquels on était "attaché"

**Événements**
- Assister à des événements traumatisants (attaque de drifters, mort d'un animal apprivoisé)
- Instabilité temporelle non gérée (amplification de la mécanique vanilla)
- Tempête ou météo extrême prolongée
- Tuer des animaux passifs à répétition (moutons, lapins — pas les hostiles)
- Être dans le noir complet prolongé

### 5.2 Niveaux de dégradation et conséquences

| Niveau | Conséquences |
|--------|-------------|
| Niveau 1 — Morosité | Vitesse de craft –15%, endurance remonte moins vite, manger donne moins de satiété, nuits semblent plus longues |
| Niveau 2 — Anxiété | Tremblements : craft/forge peuvent échouer, hypervigilance : fausses alertes sonores, insomnie partielle, objet familier requis pour s'éloigner de la base |
| Niveau 3 — Dépression | Faim et soif descendent ×2, impossible de courir plus de quelques secondes, certains crafts bloqués, items peuvent disparaître de l'inventaire (log discret), journal se remplit d'entrées sombres |
| Niveau 4 — Dissociation | Carte illisible, drifters visuellement non hostiles (silhouettes humaines), faux souvenirs dans le journal, le personnage peut démolir sa propre base la nuit, sons environnementaux partiellement remplacés |
| Niveau 5 — Rupture | Inputs ignorés aléatoirement, sauvegardes automatiques désactivées (sans avertissement), impossible de dormir, impossible de crafter quoi que ce soit de complexe, seule issue : revenir au N4 par interventions extérieures |

### 5.3 Sources de remontée

| Source | Puissance | Fragilité | Détail |
|--------|-----------|-----------|--------|
| 🍬 Comfort food | ★★☆☆☆ | Nulle | Bonbons (crash après), repas chaud, épices rares |
| 🎵 Musique | ★★★☆☆ | Ressources | Carillon passif, instrument actif, gramophone + disques rares |
| 🏡 Environnement | ★★★★☆ | À maintenir | Maison complète, décoration, feu allumé, ciel étoilé |
| 📜 Exploration | ★★★★☆ | Non renouvelable | Découverte de lieu, résolution d'un fil narratif |
| 🍺 Alcool | ★★★★☆ | Dépendance | Fort immédiat → tolérance → descente (3 états, logs discrets) |
| 🐾 Compagnon | ★★★★★ | Peut mourir | Présence passive, nourrissage, caresse — améliore aussi le sommeil |

### 5.4 Système alcool

L'alcool est géré par un compteur invisible avec trois états transitifs. Les changements d'état sont signalés par des logs discrets dans le journal du personnage.

| État | Condition | Effet sur la santé mentale |
|------|-----------|---------------------------|
| État 1 — Naïf | Compteur bas | Boire remonte la santé mentale normalement |
| État 2 — Tolérant | Seuil 1 atteint | Boire ne fait plus rien (ni bonus ni malus) |
| État 3 — Dépendant | Seuil 2 atteint | Boire fait descendre la santé mentale |

- Le compteur remonte à chaque consommation d'alcool
- Il remonte plus vite si on boit beaucoup sur une courte période
- Il redescend passivement sur environ une journée in-game
- On peut passer de l'état 3 à l'état 1 en arrêtant de boire suffisamment longtemps
- Les changements d'état génèrent un log discret — jamais d'alerte explicite

### 5.5 Le compagnon animal

Le compagnon est la source de remontée la plus puissante et la plus fragile du système.

**Apprivoisement**
- Approcher lentement sur plusieurs jours in-game (pas de sprint à proximité)
- Apporter de la nourriture adaptée à l'espèce
- Ne jamais l'effrayer (pas de combat à proximité)
- Il peut partir si négligé trop longtemps

**Effets sur la santé mentale**
- Présence dans un rayon de quelques blocs → remontée lente et constante
- Nourrir activement → petit boost immédiat
- Caresser (interaction dédiée) → boost significatif, cooldown de quelques heures
- Dort près du joueur → améliore la qualité du sommeil

**Mort du compagnon**
- Forte descente de santé mentale
- Entrée narrative dans le journal
- Deuil mécanique : certaines actions bloquées quelques jours
- Construire une tombe → redonne légèrement de la santé mentale

---

## 6. La Perception Élargie

À partir du niveau 3, le personnage accède à une couche cachée du monde. Ce n'est pas un super-pouvoir — c'est un changement de statut perçu par le monde lui-même. Les animaux le sentent. Les marchands le reconnaissent. Certaines portes s'ouvrent.

> **Concept lore** : La folie n'est pas une maladie dans le lore — c'est une "Perception Élargie" que les anciens habitants du monde maîtrisaient et ont peut-être fui. Les ruines qui ne s'ouvrent qu'aux fous témoignent d'un savoir perdu.

### 6.1 Éléments accessibles

| Élément | Niveau requis | Coût mental | Gain |
|---------|---------------|-------------|------|
| Inscriptions murales | N3+ | Aucun | Lore, indications cachées dans le monde |
| Entités neutres | N3+ | Faible | Loot très rare déposé au sol après observation prolongée |
| Animaux albinos | N3+ | Fort | Matériaux uniques — mais chasser fait descendre davantage |
| Pages de carnets | N2+ | Modéré | Coordonnées, lore profond, recettes |
| Dialogue marchand éveillé | N3+ | Léger | Objets sans description, cartes symboliques |
| Réactions animales | N2+ | Aucun | Approche facilitée (passifs), hésitation (hostiles) |
| Portes de ruines | N4 | Sévère | Accès à zones uniques et ressources rares |

### 6.2 La tension centrale

Accéder aux éléments de la perception élargie empire l'état mental. La folie doit être gérée comme une ressource : on la laisse descendre intentionnellement pour accéder à quelque chose, puis on travaille à remonter.

### 6.3 Ambiguïté intentionnelle

La frontière entre effet de la folie et réalité du monde doit rester floue :

- Certaines entités laissent des traces physiques visibles par tous (empreintes, objets déplacés)
- Certaines inscriptions révélées par la folie sont fausses — elles induisent en erreur
- Le compagnon animal réagit aux entités invisibles même quand le joueur est en bonne santé

Le compagnon devient ainsi un instrument de mesure de la réalité.

---

## 7. Architecture technique

### 7.1 Type de mod

- Code Mod C# — .NET 8.0 SDK + Vintage Story 1.21 (version stable actuelle)
- Note : VS 1.22 (pre-release) migre vers .NET 10 — une passe de compatibilité sera nécessaire à sa sortie stable
- Dépendances : aucune (mod standalone)
- Compatible avec le mod Traces (optionnel, interaction via événements partagés)

### 7.2 Systèmes principaux

**Côté serveur**
- `SurviveModSystem` — point d'entrée principal, enregistrement des systèmes
- `PlayerStatsManager` — gestion des 5 jauges par joueur (WatchedAttributes)
- `MentalStateSystem` — calcul de l'état mental, déclenchement des effets
- `AlcoholCounterSystem` — compteur invisible, gestion des 3 états
- `InjurySystem` — blessures localisées, stades, infection
- `FatigueSystem` — phases, qualité de sommeil
- `PerceptionSystem` — détection du niveau mental, spawn conditionnel d'entités

**Côté client**
- `SurviveHudRenderer` — affichage des 5 jauges (`EnumRenderStage.Ortho`)
- `MentalEffectsRenderer` — effets visuels selon le niveau mental
- `JournalSystem` — entrées automatiques et logs discrets

### 7.3 Stockage des données

Toutes les valeurs persistantes sont stockées dans les WatchedAttributes de l'entité joueur, synchronisées automatiquement client/serveur :

- `survive:hydration` — float [0, 1500]
- `survive:fatigue` — float [0, 100]
- `survive:mental` — float [0, 100]
- `survive:alcohol` — float (compteur invisible)
- `survive:injuries` — TreeAttribute (zone → stade)

### 7.4 Configuration

Un fichier JSON de configuration exposera les paramètres clés :

- Vitesses de descente de chaque jauge
- Seuils des niveaux mentaux
- Seuils du compteur alcool et vitesse de redescente
- Activation/désactivation de chaque système indépendamment
- Paramètres multijoueur (certaines mécaniques désactivables)

---

## 8. Périmètre

### 8.1 Dans le scope (v1.0)

- Les 5 piliers avec toutes leurs mécaniques de base
- Le système d'alcool à 3 états
- Les 5 niveaux de santé mentale avec leurs effets
- Les 7 éléments de perception élargie
- L'affichage HUD des jauges
- Le journal avec entrées automatiques et logs discrets
- La configuration JSON

### 8.2 Hors scope (versions futures)

- Le compagnon animal (système dédié, à affiner)
- L'intégration poussée avec le mod Traces
- Les effets visuels avancés (shaders) pour les niveaux 4 et 5
- L'équilibrage multijoueur complet

### 8.3 Mods existants

Survive n'est pas compatible by design avec SleepNeed, Balanced Thirst ou Hydrate or Diedrate — ces mods couvrent les mêmes mécaniques de manière isolée. Survive les remplace par un système unifié.

---

## 9. Glossaire

| Terme | Définition |
|-------|-----------|
| Pilier | L'une des cinq jauges de survie du mod |
| Perception Élargie | État de conscience altérée donnant accès à des éléments cachés du monde |
| Log discret | Message textuel sobre dans le journal du personnage, jamais d'alerte explicite |
| Compteur alcool | Valeur flottante invisible mesurant l'exposition cumulative à l'alcool |
| WatchedAttributes | Système de stockage de données de l'API VS synchronisé client/serveur |
| ModSystem | Classe de base d'un mod C# dans Vintage Story |
| Tick serveur | Mise à jour régulière côté serveur (toutes les N millisecondes) |
| Fil narratif | Séquence de points d'intérêt liés dans le mod Traces |
