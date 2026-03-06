# SURVIVE — Suivi de développement

## Légende
- [x] Fait
- [ ] À faire
- [~] En cours / partiel

---

## 1. Structure & fondations

- [x] Structure du projet C# (.NET 8, VS 1.21)
- [x] `modinfo.json`
- [x] `.gitignore`
- [x] Cahier des charges archivé (`docs/`)

## 2. Configuration

- [x] `SurviveConfig.cs` — classe de config avec tous les paramètres
- [x] `survive.json` — fichier de config par défaut
- [x] Chargement/sauvegarde automatique via `LoadModConfig`
- [x] Toggle on/off par système

## 3. Point d'entrée

- [x] `SurviveModSystem` — enregistrement serveur et client
- [x] Instanciation conditionnelle de chaque système

## 4. Stockage des données joueur

- [x] `PlayerStatsManager` — lecture/écriture des 5 jauges via WatchedAttributes
- [x] Initialisation des attributs au join
- [x] Helpers : fractions, niveaux mentaux, phases fatigue, état alcool

## 5. Localisation

- [x] `lang/en.json`
- [x] `lang/fr.json`

---

## 6. Hydratation (§2 du CdC)

### Système de base
- [x] Décroissance passive par tick
- [x] Interaction croisée hydratation → satiété (modificateur `hungerrate`)
- [x] Dégâts en déshydratation critique
- [~] Affamé → soif augmentée (hook partiel)

### Sources d'eau (§2.2)
- [ ] Eau de rivière / lac — buvable avec risque de maladie
- [ ] Eau de pluie — collecte via récipient à l'extérieur
- [ ] Eau de neige fondue — propre, malus température
- [ ] Eau de source — rare, toujours propre
- [ ] Ébullition de l'eau (intégration craft)

### Qualité de l'hydratation (§2.3)
- [ ] Boissons chaudes → hydratation + réchauffement
- [ ] Alcool → hydratation court terme, soif ensuite
- [ ] Fruits → hydratation légère

### Contenants (§2.4)
- [ ] Gourde en cuir (conservation longue)
- [ ] Bol en bois (conservation courte)
- [ ] Eau non bouillie stockée → devient insalubre
- [ ] Boire directement en s'accroupissant dans l'eau

---

## 7. Fatigue (§3 du CdC)

### Système de base
- [x] Accumulation passive par tick
- [x] 3 phases avec seuils configurables
- [x] Récupération pendant le sommeil
- [x] Pénalité sommeil de jour (rythme circadien)

### Effets des phases (§3.1)
- [~] Phase 1 — réduction endurance (walkspeed appliqué, craft échoué manquant)
- [~] Phase 2 — outils s'usent plus vite (stat appliquée), micro-somnolences manquantes
- [ ] Phase 2 — champ de vision réduit (client-side)
- [ ] Phase 3 — personnage s'assoit, perte de contrôle
- [ ] Phase 3 — impossible de courir ou combattre

### Qualité du sommeil (§3.2)
- [~] Qualité du lit (simplifié — toujours 1.0 pour un lit)
- [ ] Paille → récupération lente
- [ ] Lit en bois → normale
- [ ] Lit en laine + oreiller → bonus
- [ ] Dormir dehors sans abri → récupération incomplète
- [ ] État mental dégradé → insomnie (partiellement implémenté côté serveur)
- [ ] Compagnon animal → amélioration (hors scope v1.0)

---

## 8. Intégrité physique (§4 du CdC)

### Système de base
- [x] 4 zones corporelles (jambes, bras, torse, tête)
- [x] 4 stades de blessure
- [x] Guérison naturelle des blessures légères
- [x] Progression vers l'infection (chance par tick)
- [x] Reset des blessures à la mort

### Effets par zone (§4.1)
- [x] Jambes → vitesse réduite
- [x] Bras → minage et dégâts réduits
- [~] Torse → résistance réduite (via armorDurabilityLoss, à vérifier)
- [ ] Tête → effets visuels, craft raté plus souvent

### Stades de blessure (§4.2)
- [x] Stade 1 — guérison spontanée
- [x] Stade 2 — saignement, perte de PV
- [~] Stade 3 — fièvre (hungerrate modifié, fatigue accélérée manquante)
- [ ] Stade 4 — incapacité partielle selon la zone

### Hook de dégâts
- [~] Détermination de la zone touchée (simplifié par type de dégât)
- [ ] Détermination par direction du projectile / position de l'attaquant

### Ressources médicales (§4.3)
- [ ] Bandages (tissu + eau bouillie) — item + recette
- [ ] Plantes médicinales — récoltables dans le monde
- [ ] Splinte (bois + bandage) — fractures
- [ ] Suture (aiguille + fil + plante) — blessures graves
- [ ] Interaction : appliquer un soin sur une zone spécifique

---

## 9. Santé mentale (§5 du CdC)

### Système de base
- [x] Décroissance passive
- [x] 5 niveaux avec seuils configurables
- [x] Détection des transitions de niveau
- [x] Méthodes `ApplyBoost` / `ApplyPenalty` pour sources externes

### Causes de dégradation (§5.1)
- [x] Isolation (pas de joueur à proximité)
- [x] Obscurité prolongée
- [x] Blessures graves
- [x] Fatigue élevée
- [x] Déshydratation
- [ ] Dormir dans un endroit non sécurisé
- [ ] Nourriture de mauvaise qualité en continu
- [ ] Perte d'objets "attachés"
- [ ] Événements traumatisants (attaque de drifters)
- [ ] Instabilité temporelle (hook vanilla)
- [ ] Météo extrême prolongée
- [ ] Tuer des animaux passifs à répétition
- [ ] Noir complet prolongé (distinct du check lumière actuel)

### Effets par niveau (§5.2)
- [~] N1 Morosité — miningSpeedMul -15% (craft -15% et satiété réduite manquants)
- [~] N2 Anxiété — miningSpeedMul -20% (tremblements, fausses alertes, objet familier manquants)
- [~] N3 Dépression — hungerrate x2, walkspeed -15% (sprint limité, crafts bloqués, items disparaissent manquants)
- [~] N4 Dissociation — hungerrate x2, walkspeed -20% (carte illisible, drifters humains, faux souvenirs, démolition nocturne, sons remplacés manquants)
- [~] N5 Rupture — debuffs sévères (inputs ignorés, sauvegardes désactivées manquants)

### Sources de remontée (§5.3)
- [~] Environnement (check lumière + abri simplifié)
- [ ] Comfort food (repas chaud, bonbons, épices rares)
- [ ] Musique (carillon passif, instrument actif, gramophone + disques)
- [ ] Exploration (découverte de lieu / fil narratif)
- [ ] Compagnon animal (hors scope v1.0)

---

## 10. Système alcool (§5.4 du CdC)

- [x] Compteur invisible avec décroissance passive
- [x] 3 états : naïf → tolérant → dépendant
- [x] Effet mental différent par état
- [x] Consommation rapide → compteur monte plus vite
- [x] Détection des transitions d'état
- [ ] Hook sur la consommation d'items alcoolisés (connecter `OnDrinkAlcohol`)
- [ ] Logs discrets dans le journal (côté client, message envoyé mais pas encore intercepté)

---

## 11. Perception Élargie (§6 du CdC)

### Système de base
- [x] Flags WatchedAttributes selon le niveau mental
- [x] `perceptionLevel` synchronisé client/serveur

### Contenu (§6.1)
- [ ] Inscriptions murales — blocs avec texte conditionnel (N3+)
- [ ] Entités neutres — spawn + loot après observation (N3+)
- [ ] Animaux albinos — variantes d'entités, matériaux uniques (N3+)
- [ ] Pages de carnets — loot worldgen, coordonnées/lore (N2+)
- [ ] Dialogue marchand éveillé — objets sans description, cartes (N3+)
- [ ] Réactions animales — approche facilitée / hésitation (N2+)
- [ ] Portes de ruines — blocs conditionnels (N4)

### Ambiguïté intentionnelle (§6.3)
- [ ] Entités laissent des traces physiques visibles par tous
- [ ] Certaines inscriptions sont fausses
- [ ] Compagnon réagit aux entités invisibles

---

## 12. Client — HUD (§7.2)

- [x] `SurviveHudRenderer` — 4 barres colorées (Ortho)
- [ ] Vraies textures / icônes pour chaque jauge
- [ ] Barre de satiété (intégration vanilla)
- [ ] Animation des barres (pulsation en critique)
- [ ] Tooltips au survol

---

## 13. Client — Effets visuels (§7.2)

- [x] `MentalEffectsRenderer` — vignette, tremblements, distorsion
- [ ] Désaturation progressive (N1+)
- [ ] Fausses alertes sonores (N2)
- [ ] Drifters en silhouettes humaines (N4)
- [ ] Sons environnementaux remplacés (N4)
- [ ] Inputs ignorés aléatoirement (N5)
- [ ] Effets visuels avancés / shaders (hors scope v1.0)

---

## 14. Client — Journal (§7.2)

- [x] `JournalSystem` — entrées narratives FR par niveau mental
- [x] Entrées par transition d'état alcool
- [ ] Interception des messages serveur pour les transitions
- [ ] Entrées pour événements (blessure, combat, météo)
- [ ] Faux souvenirs (N4)
- [ ] UI dédiée pour consulter le journal

---

## 15. Réseau & communication

- [ ] Messages dédiés (`IServerNetworkChannel` / `IClientNetworkChannel`)
- [ ] Remplacer les `SendMessage` chat par des packets typés
- [ ] Synchronisation fine (envoyer uniquement les deltas nécessaires)

---

## 16. Intégration & polish

- [ ] Compiler contre les DLL de Vintage Story
- [ ] Vérifier tous les appels API (stats, WatchedAttributes, MountedOn…)
- [ ] Tests en jeu — équilibrage des vitesses de descente/remontée
- [ ] Compatibilité mod Traces (événements partagés, optionnel)
- [ ] Documentation joueur (wiki / guide in-game)
