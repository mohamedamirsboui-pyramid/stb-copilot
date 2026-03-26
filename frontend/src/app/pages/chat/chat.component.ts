// ╔══════════════════════════════════════════════════════════╗
// ║  chat.component.ts — The chat dashboard logic            ║
// ║                                                          ║
// ║  Analogy: This is the agent's desk at the bank.          ║
// ║  The agent sits here, types questions about procedures,   ║
// ║  and gets answers from the AI assistant.                   ║
// ║  The left sidebar shows previous conversations.           ║
// ╚══════════════════════════════════════════════════════════╝

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CopilotService } from '../../services/copilot.service';
import { AuthService } from '../../services/auth.service';

// Interface for a single chat message.
// Each message is either from the "user" (agent) or the "assistant" (AI).
interface ChatMessage {
    role: 'user' | 'assistant';     // Who sent this message
    content: string;                 // The message text
    sourceDocument?: string;         // Source PDF (only for assistant messages)
    confidence?: number;             // Confidence score (only for assistant)
    timestamp: Date;                 // When the message was sent
}

// Interface for a conversation (group of messages).
interface Conversation {
    id: string;
    title: string;       // First question becomes the title
    messages: ChatMessage[];
    createdAt: Date;
}

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './chat.component.html',
    styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
    // ═══ State variables ═══

    // The text the agent is currently typing in the input box.
    currentQuestion: string = '';

    // Is the AI currently generating an answer?
    isLoading: boolean = false;

    // Current user info (from JWT token).
    userName: string = '';
    userRole: string = '';

    // All conversations stored locally.
    conversations: Conversation[] = [];

    // The currently active conversation.
    activeConversation: Conversation | null = null;

    // Is the sidebar visible? (for mobile responsiveness)
    sidebarOpen: boolean = true;

    constructor(
        private copilotService: CopilotService,
        private authService: AuthService,
        private router: Router
    ) { }

    // ═══ Called when the component loads ═══
    ngOnInit() {
        // Get user info from the stored JWT token.
        const user = this.authService.getCurrentUser();
        if (user) {
            this.userName = user.name;
            this.userRole = user.role;
        }
    }

    // ═══ Create a new conversation ═══
    newConversation() {
        this.activeConversation = null;
        this.currentQuestion = '';
    }

    // ═══ Open an existing conversation from the sidebar ═══
    openConversation(conversation: Conversation) {
        this.activeConversation = conversation;
    }

    // ═══ Send a question to the AI ═══
    async sendQuestion() {
        // Don't send empty questions.
        if (!this.currentQuestion.trim() || this.isLoading) return;

        const question = this.currentQuestion.trim();

        // If no active conversation, create a new one.
        if (!this.activeConversation) {
            this.activeConversation = {
                id: Date.now().toString(),
                title: question.length > 50 ? question.substring(0, 50) + '...' : question,
                messages: [],
                createdAt: new Date()
            };
            this.conversations.unshift(this.activeConversation);
        }

        // Add the user's message to the conversation.
        this.activeConversation.messages.push({
            role: 'user',
            content: question,
            timestamp: new Date()
        });

        // Clear the input and show loading.
        this.currentQuestion = '';
        this.isLoading = true;

        try {
            // Call the backend API to get an answer.
            const response = await this.copilotService.askQuestion(question);

            // Add the AI's response to the conversation.
            this.activeConversation.messages.push({
                role: 'assistant',
                content: response.answer,
                sourceDocument: response.sourceDocument,
                confidence: response.confidence,
                timestamp: new Date()
            });
        } catch (error) {
            // Log the error to the browser console for debugging.
            console.error('Erreur Copilot:', error);

            // If the API call fails, show an error message.
            this.activeConversation.messages.push({
                role: 'assistant',
                content: 'Désolé, une erreur est survenue (vérifiez si le backend est lancé sur le port 5000).',
                timestamp: new Date()
            });
        } finally {
            this.isLoading = false;
        }
    }

    // ═══ Group conversations by date for the sidebar ═══
    getConversationsByDate(): { label: string; conversations: Conversation[] }[] {
        const today = new Date();
        const yesterday = new Date(today);
        yesterday.setDate(yesterday.getDate() - 1);
        const weekAgo = new Date(today);
        weekAgo.setDate(weekAgo.getDate() - 7);

        const groups: { label: string; conversations: Conversation[] }[] = [
            { label: "Aujourd'hui", conversations: [] },
            { label: 'Hier', conversations: [] },
            { label: 'Cette semaine', conversations: [] },
            { label: 'Plus ancien', conversations: [] }
        ];

        for (const conv of this.conversations) {
            const date = new Date(conv.createdAt);
            if (date.toDateString() === today.toDateString()) {
                groups[0].conversations.push(conv);
            } else if (date.toDateString() === yesterday.toDateString()) {
                groups[1].conversations.push(conv);
            } else if (date > weekAgo) {
                groups[2].conversations.push(conv);
            } else {
                groups[3].conversations.push(conv);
            }
        }

        // Only return groups that have conversations.
        return groups.filter(g => g.conversations.length > 0);
    }

    // ═══ Logout — clear token and go to login ═══
    logout() {
        this.authService.logout();
        this.router.navigate(['/login']);
    }

    // ═══ Toggle sidebar on mobile ═══
    toggleSidebar() {
        this.sidebarOpen = !this.sidebarOpen;
    }
}
