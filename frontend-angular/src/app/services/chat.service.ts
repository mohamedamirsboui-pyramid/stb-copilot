import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface SourceChunk {
    text: string;
    document: string;
    type: string;
    modified?: string;
}

export interface AskResponse {
    question: string;
    answer: string;
    confidence: number;
    confidence_label: string;
    sources: SourceChunk[];
}

export interface HealthResponse {
    status: string;
    documents_loaded: number;
    chunks_count: number;
    documents: { name: string; size: number; modified: string }[];
}

@Injectable({ providedIn: 'root' })
export class ChatService {
    private apiUrl = '/api';

    constructor(private http: HttpClient, private authService: AuthService) { }

    askQuestion(question: string): Observable<AskResponse> {
        return this.http.post<AskResponse>(
            `${this.apiUrl}/ask`,
            { question },
            { headers: this.authService.getAuthHeaders() }
        );
    }

    healthCheck(): Observable<HealthResponse> {
        return this.http.get<HealthResponse>(`${this.apiUrl}/health`);
    }
}
