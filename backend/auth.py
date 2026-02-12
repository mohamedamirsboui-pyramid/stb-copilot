"""
Authentication Manager for STB Copilot

Handles user authentication, session management, and role-based access control.
"""

import hashlib
import secrets
from datetime import datetime, timedelta


class AuthManager:
    """Manages authentication and sessions."""
    
    def __init__(self):
        """Initialize the authentication manager."""
        # In-memory user database (for MVP)
        # In production, this would be in a real database
        self.users = {
            'agent@stb.tn': {
                'password_hash': self._hash_password('agent123'),
                'name': 'Agent STB',
                'role': 'agent'
            },
            'admin@stb.tn': {
                'password_hash': self._hash_password('admin123'),
                'name': 'Administrateur',
                'role': 'admin'
            }
        }
        
        # Active sessions
        self.sessions = {}
    
    def _hash_password(self, password):
        """
        Hash a password using SHA-256.
        
        Args:
            password (str): Plain text password
        
        Returns:
            str: Hashed password
        """
        return hashlib.sha256(password.encode()).hexdigest()
    
    def authenticate(self, email, password):
        """
        Authenticate a user with email and password.
        
        Args:
            email (str): User email
            password (str): User password
        
        Returns:
            dict: User object if authentication successful, None otherwise
        """
        email = email.lower().strip()
        
        if email not in self.users:
            return None
        
        user = self.users[email]
        password_hash = self._hash_password(password)
        
        if password_hash == user['password_hash']:
            return {
                'email': email,
                'name': user['name'],
                'role': user['role']
            }
        
        return None
    
    def create_session(self, user):
        """
        Create a new session for a user.
        
        Args:
            user (dict): User object
        
        Returns:
            str: Session token
        """
        # Generate secure random token
        session_token = secrets.token_urlsafe(32)
        
        # Store session with expiration
        self.sessions[session_token] = {
            'user': user,
            'created_at': datetime.now(),
            'expires_at': datetime.now() + timedelta(hours=8),
            'ip_address': None  # Will be set by API
        }
        
        return session_token
    
    def verify_session(self, session_token):
        """
        Verify if a session token is valid.
        
        Args:
            session_token (str): Session token to verify
        
        Returns:
            dict: User object if session is valid, None otherwise
        """
        if not session_token or session_token not in self.sessions:
            return None
        
        session = self.sessions[session_token]
        
        # Check if session has expired
        if datetime.now() > session['expires_at']:
            # Remove expired session
            del self.sessions[session_token]
            return None
        
        return session['user']
    
    def destroy_session(self, session_token):
        """
        Destroy a session (logout).
        
        Args:
            session_token (str): Session token to destroy
        
        Returns:
            bool: True if session was destroyed, False otherwise
        """
        if session_token in self.sessions:
            del self.sessions[session_token]
            return True
        return False
    
    def get_session_info(self, session_token):
        """
        Get information about a session.
        
        Args:
            session_token (str): Session token
        
        Returns:
            dict: Session information or None
        """
        if session_token in self.sessions:
            session = self.sessions[session_token]
            return {
                'user': session['user'],
                'created_at': session['created_at'].strftime('%Y-%m-%d %H:%M:%S'),
                'expires_at': session['expires_at'].strftime('%Y-%m-%d %H:%M:%S')
            }
        return None
    
    def cleanup_expired_sessions(self):
        """Remove all expired sessions."""
        now = datetime.now()
        expired_tokens = [
            token for token, session in self.sessions.items()
            if now > session['expires_at']
        ]
        
        for token in expired_tokens:
            del self.sessions[token]
        
        return len(expired_tokens)
