# 🔑 La Sécurité : Le Pass JWT

Comprendre comment on se connecte et comment le serveur sait que c'est toi.

---

## 🎟️ Qu'est-ce qu'un JWT ?
**JWT** (JSON Web Token) est comme un badge de visiteur numérique. Il contient tes informations (nom, rôle) et il est "signé" par le serveur pour qu'on ne puisse pas le modifier.

---

## 🤝 Le Handshake (La poignée de main)

### 1. La Demande (Frontend)
Dans [auth.service.ts](file:///home/pyramid/stb-copilot/frontend/src/app/services/auth.service.ts), quand tu cliques sur "Login" :
*   On envoie ton email et mot de passe au backend.
*   Si c'est bon, le backend renvoie un objet avec un long texte bizarre : le `token`.

### 2. Le Contrôle (Backend)
Dans [AuthService.cs](file:///home/pyramid/stb-copilot/backend/Services/AuthService.cs) :
*   On vérifie tes identifiants par rapport à une liste (ici en dur pour l'MVP).
*   On crée le token en y ajoutant des **Claims** (tes droits) :
    ```csharp
    new Claim(ClaimTypes.Role, role) // "Agent" ou "Admin"
    ```
*   On signe le tout avec une clé secrète.

### 3. Le Stockage (Navigateur)
Une fois reçu, le Frontend stocke le token dans le `localStorage` :
*   C'est comme une petite boîte dans ton navigateur qui garde ton badge même si tu actualises la page.

---

## 🛡️ Comment le serveur nous reconnaît ?

À chaque fois que tu poses une question (`askQuestion`), le service Angular fait ceci :
1.  Il va chercher le token dans le `localStorage`.
2.  Il l'ajoute dans l'entête HTTP : `Authorization: Bearer <TON_TOKEN>`.

### Côté Serveur (Program.cs)
Le fichier [Program.cs#L47](file:///home/pyramid/stb-copilot/backend/Program.cs#L47) dit au serveur : *"Pour chaque requête, regarde s'il y a un badge Bearer. Vérifie s'il est valide et s'il n'a pas expiré."*

Si tu n'as pas de badge, le contrôleur te répondra systématiquement **401 Unauthorized** sans même lire ta question.

---

> [!TIP]
> **Le savais-tu ?**
> Tu peux copier ton token et le coller sur [jwt.io](https://jwt.io) pour voir les informations qu'il contient (ton nom, ton rôle, etc.) en clair !
