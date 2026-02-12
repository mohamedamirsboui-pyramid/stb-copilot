/**
 * STB Copilot - Main Application JavaScript
 * Handles chat interface and API communication
 */

// API Configuration
const API_URL = window.location.origin.includes('localhost') ? '/api/ask' : 'http://localhost:8000/api/ask';

// DOM Elements
const chatMessages = document.getElementById('chatMessages');
const questionForm = document.getElementById('questionForm');
const questionInput = document.getElementById('questionInput');
const sendButton = document.getElementById('sendButton');
const loadingIndicator = document.getElementById('loadingIndicator');

// Session management
let sessionToken = null;
let currentUser = null;

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    // Check authentication
    checkAuthentication();

    // Handle form submission
    questionForm.addEventListener('submit', handleSubmit);
});

/**
 * Check if user is authenticated
 */
function checkAuthentication() {
    sessionToken = localStorage.getItem('stb_session_token');
    const userName = localStorage.getItem('stb_user_name');
    const userRole = localStorage.getItem('stb_user_role');

    if (!sessionToken) {
        // Redirect to login
        window.location.href = 'login.html';
        return;
    }

    currentUser = {
        name: userName,
        role: userRole
    };

    console.log('Authenticated as:', currentUser);
}

/**
 * Handle form submission
 */
function handleSubmit(event) {
    event.preventDefault();

    const question = questionInput.value.trim();

    if (!question) {
        return;
    }

    // Add user message to chat
    addUserMessage(question);

    // Clear input
    questionInput.value = '';

    // Disable input while processing
    setInputEnabled(false);

    // Show loading indicator
    showLoading();

    // Send question to API
    sendQuestion(question);
}

/**
 * Add user message to chat
 */
function addUserMessage(text) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message user-message';

    messageDiv.innerHTML = `
        <div class="message-avatar">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
                <circle cx="12" cy="10" r="3" fill="currentColor"/>
                <path d="M6 20C6 16 8 14 12 14C16 14 18 16 18 20" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
        </div>
        <div class="message-content">
            <div class="message-text">
                <p>${escapeHtml(text)}</p>
            </div>
        </div>
    `;

    chatMessages.appendChild(messageDiv);
    scrollToBottom();
}

/**
 * Add assistant message to chat
 */
function addAssistantMessage(answer, confidence, confidenceLabel, sources) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message assistant-message';

    // Format answer as HTML
    const formattedAnswer = formatAnswer(answer);

    // Create confidence badge HTML
    const confidenceBadge = createConfidenceBadge(confidence, confidenceLabel);

    // Create sources HTML if available
    const sourcesHtml = sources && sources.length > 0 ? createSourcesHtml(sources) : '';

    messageDiv.innerHTML = `
        <div class="message-avatar">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
                <path d="M8 14C8 14 9.5 16 12 16C14.5 16 16 14 16 14" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <circle cx="9" cy="9" r="1.5" fill="currentColor"/>
                <circle cx="15" cy="9" r="1.5" fill="currentColor"/>
            </svg>
        </div>
        <div class="message-content">
            <div class="message-text">
                ${formattedAnswer}
            </div>
            ${confidenceBadge}
            ${sourcesHtml}
        </div>
    `;

    chatMessages.appendChild(messageDiv);

    // Add click handler for source toggle if sources exist
    if (sources && sources.length > 0) {
        const toggleButton = messageDiv.querySelector('.source-toggle');
        const sourceContent = messageDiv.querySelector('.source-content');

        toggleButton.addEventListener('click', function () {
            sourceContent.classList.toggle('visible');
            const arrow = toggleButton.querySelector('.arrow');
            arrow.textContent = sourceContent.classList.contains('visible') ? '▼' : '▶';
        });
    }

    scrollToBottom();
}

/**
 * Create confidence badge HTML
 */
function createConfidenceBadge(confidence, label) {
    const colorClass = label === 'high' ? 'confidence-high' :
        label === 'medium' ? 'confidence-medium' : 'confidence-low';

    const icon = label === 'high' ? '✓' : label === 'medium' ? '~' : '!';

    return `
        <div class="confidence-badge ${colorClass}">
            <span class="confidence-icon">${icon}</span>
            <span class="confidence-text">Confidence: ${confidence}%</span>
            <span class="confidence-label">(${label})</span>
        </div>
    `;
}

/**
 * Format answer text to HTML
 */
function formatAnswer(answer) {
    // Split by newlines
    const lines = answer.split('\n').filter(line => line.trim());

    // Check if it's a numbered list
    const isNumberedList = lines.some(line => /^\d+\./.test(line.trim()));

    if (isNumberedList) {
        const items = lines.map(line => {
            const text = line.replace(/^\d+\.\s*/, '').trim();
            return `<li>${escapeHtml(text)}</li>`;
        }).join('');
        return `<ol>${items}</ol>`;
    }

    // Check if it's a bullet list
    const isBulletList = lines.some(line => /^[•\-\*]/.test(line.trim()));

    if (isBulletList) {
        const items = lines.map(line => {
            const text = line.replace(/^[•\-\*]\s*/, '').trim();
            return `<li>${escapeHtml(text)}</li>`;
        }).join('');
        return `<ul>${items}</ul>`;
    }

    // Otherwise, format as paragraphs
    const paragraphs = lines.map(line => `<p>${escapeHtml(line)}</p>`).join('');
    return paragraphs;
}

/**
 * Create sources HTML
 */
function createSourcesHtml(sources) {
    const sourceItems = sources.map((source, index) => {
        // Handle both old format (string) and new format (object with metadata)
        const text = typeof source === 'string' ? source : source.text;
        const document = source.document || 'Unknown document';
        const docType = source.type || 'procedure';

        return `
            <div class="source-chunk">
                <div class="source-chunk-header">
                    <span class="source-chunk-label">Chunk ${index + 1}</span>
                    <span class="source-document-name">📄 ${document}</span>
                </div>
                <div class="source-chunk-text">${escapeHtml(text)}</div>
            </div>
        `;
    }).join('');

    return `
        <div class="source-chunks">
            <button class="source-toggle">
                <span class="arrow">▶</span>
                View ${sources.length} source ${sources.length === 1 ? 'chunk' : 'chunks'}
            </button>
            <div class="source-content">
                ${sourceItems}
            </div>
        </div>
    `;
}

/**
 * Show loading indicator
 */
function showLoading() {
    loadingIndicator.classList.add('visible');
    scrollToBottom();
}

/**
 * Hide loading indicator
 */
function hideLoading() {
    loadingIndicator.classList.remove('visible');
}

/**
 * Send question to API
 */
async function sendQuestion(question) {
    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${sessionToken}`
            },
            body: JSON.stringify({ question: question })
        });

        // Check if unauthorized (session expired)
        if (response.status === 401) {
            // Clear session and redirect to login
            localStorage.clear();
            window.location.href = 'login.html';
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        // Hide loading
        hideLoading();

        // Add assistant response with confidence and metadata
        addAssistantMessage(
            data.answer,
            data.confidence,
            data.confidence_label,
            data.sources
        );

        // Re-enable input
        setInputEnabled(true);

        // Focus input
        questionInput.focus();

    } catch (error) {
        console.error('Error:', error);

        // Hide loading
        hideLoading();

        // Show error message
        addErrorMessage('Sorry, I encountered an error. Please make sure the backend server is running.');

        // Re-enable input
        setInputEnabled(true);
    }
}

/**
 * Add error message
 */
function addErrorMessage(message) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message assistant-message';

    messageDiv.innerHTML = `
        <div class="message-avatar">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
                <path d="M12 8V12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <circle cx="12" cy="16" r="1" fill="currentColor"/>
            </svg>
        </div>
        <div class="message-content">
            <div class="message-text">
                <p style="color: #ef4444;">⚠️ ${escapeHtml(message)}</p>
            </div>
        </div>
    `;

    chatMessages.appendChild(messageDiv);
    scrollToBottom();
}

/**
 * Enable or disable input
 */
function setInputEnabled(enabled) {
    questionInput.disabled = !enabled;
    sendButton.disabled = !enabled;
}

/**
 * Scroll chat to bottom
 */
function scrollToBottom() {
    setTimeout(() => {
        chatMessages.parentElement.scrollTop = chatMessages.parentElement.scrollHeight;
    }, 100);
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
