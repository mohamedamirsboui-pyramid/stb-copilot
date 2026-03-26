// ╔══════════════════════════════════════════════════════════╗
// ║  app.config.ts — The routing map of our app              ║
// ║                                                          ║
// ║  Analogy: This is the building directory at the bank.    ║
// ║  It tells people where to go:                            ║
// ║  - /login  → Ground floor (entrance)                     ║
// ║  - /chat   → Agent office (2nd floor)                    ║
// ║  - /admin  → Manager office (3rd floor)                  ║
// ╚══════════════════════════════════════════════════════════╝

import { ApplicationConfig } from '@angular/core';
import { provideRouter, Routes } from '@angular/router';

import { LoginComponent } from './pages/login/login.component';
import { ChatComponent } from './pages/chat/chat.component';
import { AdminComponent } from './pages/admin/admin.component';

// ═══ ROUTES — Which URL shows which page ═══
// When the user visits http://localhost:4200/login, Angular shows LoginComponent.
// When they visit /chat, it shows ChatComponent, etc.
const routes: Routes = [
    // Default: redirect to login page.
    { path: '', redirectTo: 'login', pathMatch: 'full' },

    // /login → Login page (no authentication needed).
    { path: 'login', component: LoginComponent },

    // /chat → Chat dashboard (for logged-in agents and admins).
    { path: 'chat', component: ChatComponent },

    // /admin → Admin panel (for admins only).
    { path: 'admin', component: AdminComponent },

    // Any other URL → redirect to login.
    { path: '**', redirectTo: 'login' }
];

// ═══ APP CONFIGURATION ═══
// This is the main configuration object passed to Angular when the app starts.
export const appConfig: ApplicationConfig = {
    providers: [
        // provideRouter tells Angular: "use these routes".
        provideRouter(routes)
    ]
};
