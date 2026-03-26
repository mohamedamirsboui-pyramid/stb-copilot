# 🎓 Guide de Compréhension : STB Copilot

Bienvenue ! Ce document est conçu pour t'aider à comprendre ce projet comme si tu l'avais écrit toi-même. On va partir de tes bases en **Java** et **Python** pour expliquer le monde du **.NET (C#)** et de **Angular (TypeScript)**.

---

## 🏗️ 1. La Vue d'Ensemble (Architecture)

Le projet est divisé en deux parties principales qui communiquent par le réseau (HTTP) :

1.  **Le Backend (.NET 8)** : C'est le "cerveau" et le serveur. Il gère la base de données (en mémoire ici), les documents PDF, et l'IA.
2.  **Le Frontend (Angular)** : C'est l'interface visuelle que l'utilisateur voit dans son navigateur.

### Analogie du Restaurant 🍽️
*   **Le Client** : L'utilisateur qui utilise le navigateur.
*   **Le Serveur (Frontend)** : Prend la commande (ta question) et l'apporte en cuisine.
*   **La Cuisine (Backend)** : Prépare la réponse en cherchant dans les recettes (PDFs) et la renvoie au serveur.

---

## 🛡️ 2. Le Backend : C# pour les initiés Java

Si tu connais le Java OOP, tu connais déjà 80% du C#. Voici les équivalences :

| Java | C# (Backend) | Rôle |
| :--- | :--- | :--- |
| `package` | `namespace` | Organise le code en dossiers logiques. |
| `import` | `using` | Importe des bibliothèques. |
| `extends / implements` | `:` | Héritage et interfaces (ex: `class MyClass : MyBase`). |
| `Spring Controller` | `ApiController` | Reçoit les requêtes HTTP (GET, POST). |
| `Dependency Injection` | `Dependency Injection` | .NET gère tout automatiquement (voir `Program.cs`). |

### Les fichiers clés à lire :
1.  [Program.cs](file:///home/pyramid/stb-copilot/backend/Program.cs) : **C'est le point d'entrée.** C'est ici qu'on configure la sécurité (JWT), les services et les accès.
2.  [CopilotController.cs](file:///home/pyramid/stb-copilot/backend/Controllers/CopilotController.cs) : Le "guichet". Il reçoit ta question et demande au service de trouver la réponse.
3.  [RagService.cs](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs) : **Le cerveau IA.** C'est ici que la magie opère (lecture PDF, calcul de score, appel à Groq AI).

---

## 🤖 3. Le Pipeline RAG (Comment l'IA trouve la réponse)

**RAG** signifie *Retrieval-Augmented Generation*. Au lieu de laisser l'IA inventer n'importe quoi, on lui donne le bon document.

1.  **Lecture PDF** : [RagService.cs#L167](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs#L167) extrait le texte des fichiers.
2.  **Découpage (Chunking)** : On coupe le texte en petits morceaux pour que l'IA ne soit pas surchargée.
3.  **Vecteurs (Maths)** : On transforme les mots en chiffres (vecteurs) via un algorithme appelé **TF-IDF**.
4.  **Recherche** : Quand tu poses une question, on cherche le morceau de texte le plus proche mathématiquement (**Cosine Similarity**).
5.  **Génération** : On envoie ce morceau + ta question à l'IA (Groq/Llama3) pour qu'elle rédige une réponse propre.

---

## 🎨 4. Le Frontend : Angular pour les initiés Python/JS

Angular utilise le **TypeScript**, qui est du JavaScript avec des types (un peu comme les "type hints" en Python 3.10+).

### Structure Angular :
*   **Composants (Components)** : Chaque partie de l'écran (Chat, Login) est un composant.
    *   `html` : Le design.
    *   `css` : Le style.
    *   `ts` : La logique (quand je clique ici, fais ça).
*   **Services** : Gèrent la communication avec le backend.
    *   [copilot.service.ts](file:///home/pyramid/stb-copilot/frontend/src/app/services/copilot.service.ts) : Contient les fonctions `askQuestion()` et `uploadDocument()`.

---

## 🚀 5. Tes étapes d'apprentissage

Pour maîtriser ce projet, ouvre les fichiers dans cet ordre précis :

1.  **Étape 1 : Le Flux de Données**
    *   Regarde [copilot.service.ts](file:///home/pyramid/stb-copilot/frontend/src/app/services/copilot.service.ts#L36). C'est là que le bouton "Envoyer" du frontend commence son voyage.
2.  **Étape 2 : L'Arrivée au Backend**
    *   Regarde [CopilotController.cs#L52](file:///home/pyramid/stb-copilot/backend/Controllers/CopilotController.cs#L52). C'est là que la requête de l'agent arrive dans le serveur.
3.  **Étape 3 : Le Calcul IA**
    *   Ouvre [RagService.cs#L112](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs#L112) (`AskQuestion`). C'est le cœur du système. Lis les commentaires, j'ai tout expliqué avec des analogies !

---
> [!TIP]
> **Conseil de pro** : Ne cherche pas à tout comprendre ligne par ligne. Regarde d'abord qui appelle qui. La "Dependency Injection" dans `Program.cs` est ce qui lie les services entre eux.
