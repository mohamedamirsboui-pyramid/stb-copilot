// ╔══════════════════════════════════════════════════════════╗
// ║  copilot.service.ts — The API communication service      ║
// ║                                                          ║
// ║  Analogy: This is the phone line between the bank        ║
// ║  agent's desk (frontend) and the back office (backend).   ║
// ║  Every time the agent needs something, they "call"        ║
// ║  through this service.                                    ║
// ╚══════════════════════════════════════════════════════════╝

import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class CopilotService {

    // The backend API URL.
    private apiUrl = 'http://localhost:5000/api';

    // We inject AuthService to get the JWT token for authenticated requests.
    constructor(private authService: AuthService) { }

    // ═══ Helper: Create headers with JWT token ═══
    // Every API call (except login) needs the JWT token in the header.
    // It's like showing your badge every time you enter a room.
    private getHeaders(): Record<string, string> {
        const token = this.authService.getToken();
        return {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        };
    }

    // ═══ ASK A QUESTION — Sends question to RAG pipeline ═══
    async askQuestion(question: string): Promise<any> {
        const response = await fetch(`${this.apiUrl}/copilot/ask`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify({ question })
        });

        if (!response.ok) {
            throw new Error('Erreur lors de la requête');
        }

        return await response.json();
    }

    // ═══ GET ALL DOCUMENTS — For the admin table ═══
    async getDocuments(): Promise<any[]> {
        const response = await fetch(`${this.apiUrl}/documents`, {
            method: 'GET',
            headers: this.getHeaders()
        });

        if (!response.ok) {
            throw new Error('Erreur lors du chargement des documents');
        }

        return await response.json();
    }

    // ═══ UPLOAD A DOCUMENT — Send PDF to the backend ═══
    async uploadDocument(file: File, category: string): Promise<any> {
        const token = this.authService.getToken();

        // For file uploads, we use FormData instead of JSON.
        // FormData can contain both text fields and files.
        const formData = new FormData();
        formData.append('file', file);           // The PDF file
        formData.append('category', category);   // The category

        const response = await fetch(`${this.apiUrl}/documents/upload`, {
            method: 'POST',
            headers: {
                // NOTE: Do NOT set Content-Type for FormData.
                // The browser sets it automatically with the correct boundary.
                'Authorization': `Bearer ${token}`
            },
            body: formData
        });

        if (!response.ok) {
            throw new Error('Erreur lors du téléchargement');
        }

        return await response.json();
    }

    // ═══ DELETE A DOCUMENT — Remove from the system ═══
    async deleteDocument(documentId: string): Promise<any> {
        const response = await fetch(`${this.apiUrl}/documents/${documentId}`, {
            method: 'DELETE',
            headers: this.getHeaders()
        });

        if (!response.ok) {
            throw new Error('Erreur lors de la suppression');
        }

        return await response.json();
    }
}
