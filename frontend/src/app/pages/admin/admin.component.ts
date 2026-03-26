// ╔══════════════════════════════════════════════════════════╗
// ║  admin.component.ts — The admin panel logic              ║
// ║                                                          ║
// ║  Analogy: This is the manager's office at the bank.      ║
// ║  The manager can:                                        ║
// ║  - Upload new procedure documents (PDFs)                  ║
// ║  - See all documents in a table                          ║
// ║  - Delete old documents                                   ║
// ║  Regular agents cannot enter this room.                   ║
// ╚══════════════════════════════════════════════════════════╝

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CopilotService } from '../../services/copilot.service';
import { AuthService } from '../../services/auth.service';

// Interface for a document from the API.
interface Document {
    id: string;
    fileName: string;
    category: string;
    version: string;
    status: string;
    uploadedAt: string;
    uploadedBy: string;
}

@Component({
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin.component.html',
    styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
    // ═══ State variables ═══

    // List of all documents (loaded from API).
    documents: Document[] = [];

    // Filtered documents (based on search).
    filteredDocuments: Document[] = [];

    // Search text for filtering the table.
    searchText: string = '';

    // Is the upload modal visible?
    showUploadModal: boolean = false;

    // Upload form fields.
    selectedFile: File | null = null;
    uploadCategory: string = 'Procédures';
    isUploading: boolean = false;
    uploadMessage: string = '';

    // User info.
    userName: string = '';

    // Available categories for the dropdown.
    categories: string[] = [
        'Procédures',
        'Réglementation',
        'Circulaires',
        'Notes de service',
        'Formulaires'
    ];

    // Pagination
    currentPage: number = 1;
    itemsPerPage: number = 5;

    constructor(
        private copilotService: CopilotService,
        private authService: AuthService,
        private router: Router
    ) { }

    // ═══ Called when the component loads ═══
    ngOnInit() {
        const user = this.authService.getCurrentUser();
        if (user) {
            this.userName = user.name;
        }
        this.loadDocuments();
    }

    // ═══ Load all documents from the API ═══
    async loadDocuments() {
        try {
            this.documents = await this.copilotService.getDocuments();
            this.filterDocuments();
        } catch (error) {
            console.error('Erreur lors du chargement des documents:', error);
        }
    }

    // ═══ Filter documents based on search text ═══
    filterDocuments() {
        if (!this.searchText.trim()) {
            this.filteredDocuments = [...this.documents];
        } else {
            const search = this.searchText.toLowerCase();
            this.filteredDocuments = this.documents.filter(doc =>
                doc.fileName.toLowerCase().includes(search) ||
                doc.category.toLowerCase().includes(search)
            );
        }
        this.currentPage = 1; // Reset to first page when filtering.
    }

    // ═══ Get documents for current page (pagination) ═══
    getPagedDocuments(): Document[] {
        const start = (this.currentPage - 1) * this.itemsPerPage;
        return this.filteredDocuments.slice(start, start + this.itemsPerPage);
    }

    // ═══ Calculate total pages ═══
    get totalPages(): number {
        return Math.ceil(this.filteredDocuments.length / this.itemsPerPage);
    }

    // ═══ File selection handler ═══
    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.selectedFile = input.files[0];
        }
    }

    // ═══ Upload a document ═══
    async uploadDocument() {
        if (!this.selectedFile) {
            this.uploadMessage = 'Veuillez sélectionner un fichier PDF';
            return;
        }

        this.isUploading = true;
        this.uploadMessage = '';

        try {
            await this.copilotService.uploadDocument(this.selectedFile, this.uploadCategory);
            this.uploadMessage = 'Document téléchargé avec succès!';
            this.selectedFile = null;

            // Reload the document list.
            await this.loadDocuments();

            // Close modal after a short delay.
            setTimeout(() => {
                this.showUploadModal = false;
                this.uploadMessage = '';
            }, 1500);
        } catch (error) {
            this.uploadMessage = 'Erreur lors du téléchargement';
        } finally {
            this.isUploading = false;
        }
    }

    // ═══ Delete a document ═══
    async deleteDocument(doc: Document) {
        if (confirm(`Supprimer le document "${doc.fileName}"?`)) {
            try {
                await this.copilotService.deleteDocument(doc.id);
                await this.loadDocuments();
            } catch (error) {
                console.error('Erreur lors de la suppression:', error);
            }
        }
    }

    // ═══ Navigation ═══
    goToChat() {
        this.router.navigate(['/chat']);
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['/login']);
    }
}
