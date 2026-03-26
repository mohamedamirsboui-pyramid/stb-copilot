// ╔══════════════════════════════════════════════════════════╗
// ║  app.ts — The root component of the Angular app          ║
// ║                                                          ║
// ║  Analogy: This is the building itself.                    ║
// ║  It contains <router-outlet> which is like an empty       ║
// ║  room that Angular fills with the right page based on     ║
// ║  the current URL.                                         ║
// ╚══════════════════════════════════════════════════════════╝

import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  // The root component selector — matches <app-root> in index.html.
  selector: 'app-root',

  standalone: true,

  // We import RouterOutlet so Angular can swap pages based on the URL.
  imports: [RouterOutlet],

  // The template is simple: just a router-outlet.
  // Angular automatically replaces this with the current page component
  // (LoginComponent, ChatComponent, or AdminComponent).
  template: '<router-outlet />'
})
export class AppComponent { }
