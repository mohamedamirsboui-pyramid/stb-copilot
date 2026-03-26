// ╔══════════════════════════════════════════════════════════╗
// ║  login.component.ts — The login page logic               ║
// ║                                                          ║
// ║  Analogy: This is the receptionist at the bank entrance. ║
// ║  They take your badge (email + password), check it with  ║
// ║  security (AuthService), and if valid, let you in.        ║
// ╚══════════════════════════════════════════════════════════╝

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';          // For *ngIf, *ngFor, etc.
import { FormsModule } from '@angular/forms';            // For [(ngModel)] two-way binding
import { Router } from '@angular/router';                // To navigate to other pages
import { AuthService } from '../../services/auth.service';

@Component({
    // "standalone: true" means this component doesn't need a module.
    // This is the modern Angular way (simpler for beginners).
    standalone: true,

    // We import CommonModule and FormsModule so we can use
    // *ngIf and [(ngModel)] in the HTML template.
    imports: [CommonModule, FormsModule],

    // The HTML template file for this component.
    templateUrl: './login.component.html',

    // The CSS styles file for this component.
    styleUrls: ['./login.component.css']
})
export class LoginComponent {
    // ═══ Form fields — bound to the HTML inputs ═══
    // [(ngModel)]="email" in HTML means: whatever the user types
    // in the input field automatically updates this variable.
    email: string = '';
    password: string = '';

    // Error message to show if login fails.
    errorMessage: string = '';

    // Loading state — shows a spinner while waiting for the server.
    isLoading: boolean = false;

    // The AuthService and Router are "injected" by Angular.
    // Angular creates them and passes them to us automatically.
    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    // ═══ Called when the user clicks "Se connecter" ═══
    async onLogin() {
        // Reset previous error message.
        this.errorMessage = '';

        // Don't do anything if fields are empty.
        if (!this.email || !this.password) {
            this.errorMessage = 'Veuillez remplir tous les champs';
            return;
        }

        // Show loading spinner.
        this.isLoading = true;

        try {
            // Call the AuthService to log in.
            // This sends email + password to the backend API.
            const response = await this.authService.login(this.email, this.password);

            if (response) {
                // Login successful! Navigate to the appropriate page.
                // Agents go to /chat, Admins go to /admin.
                if (response.role === 'Admin') {
                    this.router.navigate(['/admin']);
                } else {
                    this.router.navigate(['/chat']);
                }
            } else {
                this.errorMessage = 'Email ou mot de passe incorrect';
            }
        } catch (error) {
            this.errorMessage = 'Erreur de connexion au serveur';
        } finally {
            // Hide loading spinner (whether success or failure).
            this.isLoading = false;
        }
    }
}
