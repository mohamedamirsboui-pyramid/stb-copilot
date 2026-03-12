import { Component, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { ChatService, AskResponse, SourceChunk } from '../services/chat.service';

interface ChatMessage {
    type: 'user' | 'assistant' | 'error';
    text?: string;
    answer?: string;
    confidence?: number;
    confidenceLabel?: string;
    sources?: SourceChunk[];
}

@Component({
    selector: 'app-chat',
    standalone: true,
    imports: [FormsModule, CommonModule],
    templateUrl: './chat.component.html',
    styleUrl: './chat.component.css'
})
export class ChatComponent implements AfterViewChecked {
    @ViewChild('chatMessages') chatMessagesEl!: ElementRef;

    question = '';
    isLoading = false;
    messages: ChatMessage[] = [];
    expandedSources: Set<number> = new Set();

    constructor(
        private authService: AuthService,
        private chatService: ChatService,
        private router: Router
    ) {
        if (!this.authService.isLoggedIn) {
            this.router.navigate(['/login']);
        }
    }

    ngAfterViewChecked(): void {
        this.scrollToBottom();
    }

    onSubmit(): void {
        const q = this.question.trim();
        if (!q || this.isLoading) return;

        this.messages.push({ type: 'user', text: q });
        this.question = '';
        this.isLoading = true;

        this.chatService.askQuestion(q).subscribe({
            next: (response: AskResponse) => {
                this.messages.push({
                    type: 'assistant',
                    answer: response.answer,
                    confidence: response.confidence,
                    confidenceLabel: response.confidence_label,
                    sources: response.sources
                });
                this.isLoading = false;
            },
            error: (err) => {
                if (err.status === 401) {
                    localStorage.clear();
                    this.router.navigate(['/login']);
                    return;
                }
                this.messages.push({
                    type: 'error',
                    text: 'Sorry, I encountered an error. Please make sure the backend server is running.'
                });
                this.isLoading = false;
            }
        });
    }

    toggleSources(index: number): void {
        if (this.expandedSources.has(index)) {
            this.expandedSources.delete(index);
        } else {
            this.expandedSources.add(index);
        }
    }

    isSourceExpanded(index: number): boolean {
        return this.expandedSources.has(index);
    }

    getConfidenceIcon(label?: string): string {
        return label === 'high' ? '✓' : label === 'medium' ? '~' : '!';
    }

    formatAnswer(answer: string): string {
        const lines = answer.split('\n').filter(l => l.trim());
        const isNumbered = lines.some(l => /^\d+\./.test(l.trim()));
        const isBullet = lines.some(l => /^[•\-*]/.test(l.trim()));

        if (isNumbered) {
            const items = lines.map(l => `<li>${this.escapeHtml(l.replace(/^\d+\.\s*/, '').trim())}</li>`).join('');
            return `<ol>${items}</ol>`;
        }
        if (isBullet) {
            const items = lines.map(l => `<li>${this.escapeHtml(l.replace(/^[•\-*]\s*/, '').trim())}</li>`).join('');
            return `<ul>${items}</ul>`;
        }
        return lines.map(l => `<p>${this.escapeHtml(l)}</p>`).join('');
    }

    logout(): void {
        this.authService.logout().subscribe({
            next: () => this.router.navigate(['/login']),
            error: () => {
                localStorage.clear();
                this.router.navigate(['/login']);
            }
        });
    }

    private escapeHtml(text: string): string {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    private scrollToBottom(): void {
        try {
            if (this.chatMessagesEl) {
                const el = this.chatMessagesEl.nativeElement;
                el.scrollTop = el.scrollHeight;
            }
        } catch (e) { }
    }
}
