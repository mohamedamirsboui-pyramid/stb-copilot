// ╔══════════════════════════════════════════════════════════╗
// ║  auth.service.ts — The security guard service            ║
// ║                                                          ║
// ║  Analogy: This is the bank's ID card system.             ║
// ║  - login() = show your ID at the door                     ║
// ║  - logout() = hand back your visitor pass                 ║
// ║  - getToken() = show your pass to access rooms            ║
// ║  - getCurrentUser() = read the name on your pass          ║
// ╚══════════════════════════════════════════════════════════╝

import { Injectable } from '@angular/core';

@Injectable({
    // "providedIn: 'root'" means Angular creates ONE instance
    // of this service for the entire app. Everyone shares it.
    providedIn: 'root'
})
export class AuthService {

    // The backend API URL.
    private apiUrl = 'http://localhost:5000/api';

    /// ═══ LOGIN — Send credentials to the backend ═══
    async login(email: string, password: string): Promise<any> {
        try {
            // Send a POST request to /api/auth/login with email and password.
            const response = await fetch(`${this.apiUrl}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            // If the server returned an error (401, 500, etc.), return null.
            if (!response.ok) return null;

            // Parse the JSON response (contains token, userName, role).
            const data = await response.json();

            // Save the token and user info in localStorage.
            // localStorage = browser storage that persists even after closing the tab.
            localStorage.setItem('token', data.token);
            localStorage.setItem('userName', data.userName);
            localStorage.setItem('userRole', data.role);

            return data;
        } catch (error) {
            console.error('Erreur de connexion:', error);
            return null;
        }
    }

    // ═══ LOGOUT — Remove the token and user info ═══
    logout(): void {
        localStorage.removeItem('token');
        localStorage.removeItem('userName');
        localStorage.removeItem('userRole');
    }

    // ═══ GET TOKEN — Returns the JWT token for API calls ═══
    getToken(): string | null {
        return localStorage.getItem('token');
    }

    // ═══ IS LOGGED IN — Check if user has a valid token ═══
    isLoggedIn(): boolean {
        return !!this.getToken();
    }

    // ═══ GET CURRENT USER — Read user info from storage ═══
    getCurrentUser(): { name: string; role: string } | null {
        const name = localStorage.getItem('userName');
        const role = localStorage.getItem('userRole');

        if (name && role) {
            return { name, role };
        }
        return null;
    }

    // ═══ GET ROLE — Returns the user's role (Agent or Admin) ═══
    getRole(): string {
        return localStorage.getItem('userRole') || '';
    }
}
