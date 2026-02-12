"""
Portable STB Copilot API Server (Zero Dependencies)

This version uses only Python built-in libraries (http.server) 
to avoid dependency issues with Flask.
"""

import http.server
import socketserver
import json
import os
import sys
from datetime import datetime
from urllib.parse import urlparse, parse_qs

# Add backend to path for imports
sys.path.append(os.getcwd())

from auth import AuthManager
from document_manager import DocumentManager
from answer_generator import generate_answer_with_confidence
from logger import AccessLogger
from retriever import simple_search

# Configuration
PORT = 8000
DOCS_PATH = "../docs"
FRONTEND_PATH = "../frontend"

# Initialize managers
doc_manager = DocumentManager(DOCS_PATH)
auth_manager = AuthManager()
access_logger = AccessLogger()

# Global state
chunks_text = []

def startup():
    global chunks_text
    print("\n" + "="*50)
    print("STB Copilot - Portable Server Starting")
    print("="*50)
    doc_manager.load_all_documents()
    doc_manager.chunk_all_documents()
    chunks_text = doc_manager.get_chunks_text_only()
    print(f"✓ Ready with {len(chunks_text)} chunks")
    print(f"✓ Access at: http://localhost:{PORT}/login.html")
    print("="*50 + "\n")

class STBHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=FRONTEND_PATH, **kwargs)

    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        
        try:
            data = json.loads(post_data.decode('utf-8'))
        except:
            self.send_error(400, "Invalid JSON")
            return

        # ---- API: LOGIN ----
        if self.path == '/api/login':
            email = data.get('email', '').strip()
            password = data.get('password', '').strip() # Added .strip() to handle copy-paste spaces
            
            print(f"DEBUG: Login attempt for email: '{email}'")
            print(f"DEBUG: Password length received: {len(password)} characters")
            
            user = auth_manager.authenticate(email, password)
            
            if user:
                token = auth_manager.create_session(user)
                access_logger.log_login(email, success=True, ip_address=self.client_address[0])
                print(f"✓ Success: {email}")
                self._send_response({
                    'success': True,
                    'session_token': token,
                    'user': user
                })
            else:
                access_logger.log_login(email, success=False, ip_address=self.client_address[0])
                print(f"✗ Failure: {email} (Incorrect credentials)")
                self._send_response({
                    'success': False,
                    'message': 'Email ou mot de passe incorrect'
                }, status=401)

        # ---- API: ASK ----
        elif self.path == '/api/ask':
            auth_header = self.headers.get('Authorization', '')
            token = auth_header.replace('Bearer ', '')
            user = auth_manager.verify_session(token)
            
            if not user:
                self._send_response({'error': 'Non autorisé'}, status=401)
                return
                
            question = data.get('question', '').strip()
            relevant_chunks_text = simple_search(question, chunks_text)
            result = generate_answer_with_confidence(question, relevant_chunks_text)
            
            # Metadata mapping
            sources = []
            for chunk_text in relevant_chunks_text:
                meta = doc_manager.get_chunk_metadata(chunk_text)
                sources.append({
                    'text': chunk_text,
                    'document': meta['source_document'] if meta else 'unknown'
                })
                
            access_logger.log_question(user['email'], question, result['confidence'], self.client_address[0])
            
            self._send_response({
                'answer': result['answer'],
                'confidence': result['confidence'],
                'confidence_label': result['confidence_label'],
                'sources': sources
            })
            
        else:
            self.send_error(404, "API Endpoint Not Found")

    def _send_response(self, data, status=200):
        self.send_response(status)
        self.send_header('Content-type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'POST, GET, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
        self.end_headers()
        self.wfile.write(json.dumps(data).encode('utf-8'))

    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'POST, GET, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
        self.end_headers()

if __name__ == "__main__":
    startup()
    with socketserver.TCPServer(("", PORT), STBHandler) as httpd:
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nShutting down server...")
            httpd.shutdown()
