import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface UserInfo {
    email: string;
    name: string;
    role: string;
}

export interface LoginResponse {
    success: boolean;
    message?: string;
    session_token?: string;
    user?: UserInfo;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
    private apiUrl = '/api';
    private currentUser = new BehaviorSubject<UserInfo | null>(null);

    currentUser$ = this.currentUser.asObservable();

    constructor(private http: HttpClient) {
        // Restore user from localStorage
        const name = localStorage.getItem('stb_user_name');
        const role = localStorage.getItem('stb_user_role');
        const token = localStorage.getItem('stb_session_token');

        if (token && name && role) {
            this.currentUser.next({ email: '', name, role });
        }
    }

    get isLoggedIn(): boolean {
        return !!localStorage.getItem('stb_session_token');
    }

    get sessionToken(): string | null {
        return localStorage.getItem('stb_session_token');
    }

    get user(): UserInfo | null {
        return this.currentUser.value;
    }

    login(email: string, password: string): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
            tap(response => {
                if (response.success && response.session_token && response.user) {
                    localStorage.setItem('stb_session_token', response.session_token);
                    localStorage.setItem('stb_user_name', response.user.name);
                    localStorage.setItem('stb_user_role', response.user.role);
                    this.currentUser.next(response.user);
                }
            })
        );
    }

    logout(): Observable<any> {
        const headers = new HttpHeaders({
            'Authorization': `Bearer ${this.sessionToken}`
        });
        return this.http.post(`${this.apiUrl}/logout`, {}, { headers }).pipe(
            tap(() => {
                localStorage.removeItem('stb_session_token');
                localStorage.removeItem('stb_user_name');
                localStorage.removeItem('stb_user_role');
                this.currentUser.next(null);
            })
        );
    }

    getAuthHeaders(): HttpHeaders {
        return new HttpHeaders({
            'Authorization': `Bearer ${this.sessionToken}`
        });
    }
}
