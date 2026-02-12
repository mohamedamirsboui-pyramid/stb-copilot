/**
 * STB Copilot - Login Page JavaScript
 * Handles login form submission and authentication
 */

// API Configuration
const API_URL = window.location.origin.includes('localhost') ? '/api/login' : 'http://localhost:8000/api/login';

// DOM Elements
const loginForm = document.getElementById('loginForm');
const emailInput = document.getElementById('email');
const passwordInput = document.getElementById('password');
const loginButton = document.getElementById('loginButton');
const errorMessage = document.getElementById('errorMessage');

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    // Check if already logged in
    checkExistingSession();

    // Handle form submission
    loginForm.addEventListener('submit', handleLogin);
});

/**
 * Check if user already has a valid session
 */
function checkExistingSession() {
    const sessionToken = localStorage.getItem('stb_session_token');

    if (sessionToken) {
        // Redirect to main app
        window.location.href = 'index.html';
    }
}

/**
 * Handle login form submission
 */
async function handleLogin(event) {
    event.preventDefault();

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (!email || !password) {
        showError('Veuillez remplir tous les champs');
        return;
    }

    // Disable form
    setFormEnabled(false);
    hideError();

    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                email: email,
                password: password
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            // Store session token
            localStorage.setItem('stb_session_token', data.session_token);
            localStorage.setItem('stb_user_name', data.user.name);
            localStorage.setItem('stb_user_role', data.user.role);

            // Show success and redirect
            showSuccess('Connexion réussie! Redirection...');

            setTimeout(() => {
                window.location.href = 'index.html';
            }, 1000);
        } else {
            // Show error
            showError(data.message || 'Email ou mot de passe incorrect');
            setFormEnabled(true);
        }

    } catch (error) {
        console.error('Login error:', error);
        showError('Erreur de connexion. Veuillez réessayer.');
        setFormEnabled(true);
    }
}

/**
 * Show error message
 */
function showError(message) {
    errorMessage.textContent = message;
    errorMessage.classList.add('visible');
    errorMessage.style.background = 'rgba(239, 68, 68, 0.1)';
    errorMessage.style.borderColor = 'rgba(239, 68, 68, 0.3)';
    errorMessage.style.color = '#ef4444';
}

/**
 * Show success message
 */
function showSuccess(message) {
    errorMessage.textContent = message;
    errorMessage.classList.add('visible');
    errorMessage.style.background = 'rgba(16, 185, 129, 0.1)';
    errorMessage.style.borderColor = 'rgba(16, 185, 129, 0.3)';
    errorMessage.style.color = '#10b981';
}

/**
 * Hide error message
 */
function hideError() {
    errorMessage.classList.remove('visible');
}

/**
 * Enable or disable form
 */
function setFormEnabled(enabled) {
    emailInput.disabled = !enabled;
    passwordInput.disabled = !enabled;
    loginButton.disabled = !enabled;

    if (enabled) {
        loginButton.querySelector('.button-text').textContent = 'Se connecter';
    } else {
        loginButton.querySelector('.button-text').textContent = 'Connexion...';
    }
}
