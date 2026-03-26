# 📚 RAG et Indexation : Comment la machine lit

Le cœur du projet est le système **RAG** (Retrieval-Augmented Generation). Voici comment on passe d'un fichier PDF à une réponse intelligente.

---

## 🏗️ Étape 1 : L'extraction (Lire le PDF)

Quand un administrateur télécharge un fichier via [DocumentController.cs](file:///home/pyramid/stb-copilot/backend/Controllers/DocumentController.cs), le `RagService` prend le relais.

1.  **Bibliothèque** : On utilise `PdfPig` ([RagService.cs#L167](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs#L167)) pour ouvrir le PDF.
2.  **Texte Brut** : On parcourt chaque page et on concatène tout le texte.

---

## ✂️ Étape 2 : Le Découpage (Chunking)

L'IA ne peut pas lire un livre de 200 pages d'un coup.

1.  **Fonction** : `SplitTextIntoChunks` ([RagService.cs#L184](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs#L184)).
2.  **Méthode** : On coupe le texte par paragraphes (`\n\n`).
3.  **Taille** : Chaque morceau (chunk) fait environ 500 caractères. C'est l'unité de base de notre recherche.

---

## 🧮 Étape 3 : Les Mathématiques (TF-IDF & Vecteurs)

C'est ici que tu peux impressionner tout le monde : on transforme les mots en nombres.

### TF-IDF (Term Frequency - Inverse Document Frequency)
*   **TF (Fréquence du terme)** : Plus un mot apparaît dans un morceau, plus il est important.
*   **IDF (Fréquence inverse dans le document)** : Si un mot est trop commun (ex: "le", "la", "banque"), il perd de son importance.
*   **Résultat** : Chaque morceau de texte devient un **Vecteur** (une liste de nombres).

### Stockage en mémoire
On ne s'embête pas avec une base de données complexe. Tout est dans une `List<DocumentChunk>` ([RagService.cs#L37](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs#L37)).

---

## 🔍 Étape 4 : La Recherche (Cosine Similarity)

Quand tu poses une question :
1.  On transforme Ta Question en vecteur (exactement comme les chunks).
2.  On compare ton vecteur avec tous les chunks stockés.
3.  **Algorithme** : `CosineSimilarity` ([RagService.cs#L258](file:///home/pyramid/stb-copilot/backend/Services/RagService.cs#L258)).
    *   C'est une formule mathématique qui calcule l'angle entre deux vecteurs.
    *   Plus l'angle est petit, plus le sens est proche !

---

## 🪄 Étape 5 : L'appel final à l'IA (Groq)

Une fois qu'on a le morceau le plus proche (le "Best Chunk"), on l'envoie à l'IA **Llama3** hébergée par **Groq**.

1.  **Context Construction** : On lui donne le morceau de texte en lui disant : *"Voici ta seule source de vérité"*.
2.  **Structured Answer** : L'IA rédige une réponse polie en français basée sur ce texte uniquement.

---

> [!TIP]
> **Le secret de l'Admin** : Si tu supprimes un document, le service appelle `RebuildVocabulary()` pour recalculer tous les scores. C'est pour ça qu'il y a un petit délai lors de la suppression !
