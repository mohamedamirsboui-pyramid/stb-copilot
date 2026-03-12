import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [FormsModule, CommonModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css'
})
export class LoginComponent {
    email = '';
    password = '';
    errorMessage = '';
    successMessage = '';
    isLoading = false;

    constructor(private authService: AuthService, private router: Router) {
        if (this.authService.isLoggedIn) {
            this.router.navigate(['/chat']);
        }
    }

    onSubmit(): void {
        if (!this.email || !this.password) {
            this.errorMessage = 'Veuillez remplir tous les champs';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        this.authService.login(this.email, this.password).subscribe({
            next: (response) => {
                if (response.success) {
                    this.successMessage = 'Connexion réussie! Redirection...';
                    setTimeout(() => this.router.navigate(['/chat']), 1000);
                } else {
                    this.errorMessage = response.message || 'Email ou mot de passe incorrect';
                    this.isLoading = false;
                }
            },
            error: () => {
                this.errorMessage = 'Erreur de connexion. Veuillez réessayer.';
                this.isLoading = false;
            }
        });
    }
}
