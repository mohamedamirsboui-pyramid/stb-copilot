# 🔍 Trace d'Exécution : De la question à la réponse

Ce guide suit le chemin d'une question posée par l'agent, du clic sur le bouton "Envoyer" jusqu'à la réponse générée par l'IA.

---

## 🏎️ Étape 1 : Le Frontend (L'Action de l'Agent)

Tout commence dans l'interface de chat.

1.  **Fichier** : [chat.component.ts](file:///home/pyramid/stb-copilot/frontend/src/app/pages/chat/chat.component.ts)
2.  **Fonction** : `sendQuestion()` (Ligne 91)
3.  **Action** : 
    *   L'agent clique sur "Envoyer". 
    *   On ajoute son message à la liste locale (pour qu'il apparaisse tout de suite à l'écran).
    *   On appelle `this.copilotService.askQuestion(question)`.

---

## ☎️ Étape 2 : Le Service Frontend (La Liaison)

On prépare l'appel réseau vers le serveur.

1.  **Fichier** : [copilot.service.ts](file:///home/pyramid/stb-copilot/frontend/src/app/services/copilot.service.ts)
2.  **Fonction** : `askQuestion()` (Ligne 36)
3.  **Action** :
    *   On récupère le **Token JWT** (ton badge de sécurité) via `getHeaders()`.
    *   On fait un `fetch` (appel HTTP POST) vers `http://localhost:5000/api/copilot/ask`.
    *   On attend la réponse du serveur.

---

## 🛂 Étape 3 : Le Contrôleur Backend (Le Guichet)

Le serveur reçoit la requête.

1.  **Fichier** : [CopilotController.cs](file:///home/pyramid/stb-copilot/backend/Controllers/CopilotController.cs)
2.  **Fonction** : `Ask()` (Ligne 52)
3.  **Action** :
    *   Le serveur vérifie si tu as un badge valide (`[Authorize]`).
    *   Il transforme le JSON reçu en objet [QuestionRequest](file:///home/pyramid/stb-copilot/backend/Models/QuestionRequest.cs).
    *   Il passe la question au `CopilotService`.

---

## 🧠 Étape 4 : Le Service RAG (Le Cerveau)

C'est ici que l'intelligence se trouve.

1.  **Fichier** : [RagService.cs](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs)
2.  **Fonction** : `AskQuestion()` (Ligne 112)
3.  **Le Processus Interne** :
    *   **Vectorisation** : On transforme ta question en une liste de nombres (vecteur TF-IDF).
    *   **Recherche** : On compare ce vecteur avec tous les morceaux de documents stockés en mémoire (`CosineSimilarity`).
    *   **Sélection** : On trouve le morceau le plus pertinent (le "Best Chunk").
    *   **Appel Groq** : On envoie ce morceau + ta question à l'IA avec une consigne stricte : *"Réponds uniquement à partir de ce texte"*.

---

## 📤 Étape 5 : Le Retour (Le Voyage Inverse)

1.  L'IA renvoie le texte de la réponse.
2.  Le `RagService` crée un objet [AnswerResponse](file:///home/pyramid/stb-copilot/backend/Models/AnswerResponse.cs).
3.  Le contrôleur renvoie cet objet en JSON.
4.  Le Frontend le reçoit, l'ajoute à la liste des messages, et `isLoading` repasse à `false`.

---

> [!IMPORTANT]
> **Pourquoi c'est sécurisé ?**
> Note l'utilisation du `lock` dans `RagService.cs`. Cela garantit que si deux agents posent des questions en même temps, le serveur ne mélange pas les dossiers !
