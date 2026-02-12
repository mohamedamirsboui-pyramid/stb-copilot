"""
STB Copilot - Flask API Server

This module provides a REST API for the STB Copilot frontend.
It serves the frontend files and handles question-answering requests.
"""

from flask import Flask, request, jsonify, send_from_directory
from flask_cors import CORS
import os

# Import backend modules
from chunker import chunk_text
from retriever import simple_search
from answer_generator import generate_answer_with_confidence
from document_manager import DocumentManager
from auth import AuthManager
from logger import AccessLogger

# Initialize Flask app
app = Flask(__name__)
CORS(app)

# Initialize managers
DOCS_PATH = "../docs"
doc_manager = DocumentManager(DOCS_PATH)
auth_manager = AuthManager()
access_logger = AccessLogger()

chunks_with_metadata = []
chunks_text = []

def load_documents():
    """Load and chunk all documents at startup."""
    global chunks_with_metadata, chunks_text
    
    print("\n" + "=" * 60)
    print("Loading Documents...")
    print("=" * 60 + "\n")
    
    # Load all documents
    doc_manager.load_all_documents()
    
    # Chunk all documents with metadata
    chunks_with_metadata = doc_manager.chunk_all_documents()
    chunks_text = doc_manager.get_chunks_text_only()
    
    print(f"\n✓ System ready with {len(chunks_text)} chunks from {len(doc_manager.documents)} documents")

# Load documents when server starts
load_documents()


@app.route('/')
def serve_frontend():
    """Serve the main HTML file."""
    return send_from_directory('../frontend', 'index.html')


@app.route('/<path:path>')
def serve_static(path):
    """Serve static files (CSS, JS)."""
    return send_from_directory('../frontend', path)


@app.route('/api/login', methods=['POST'])
def login():
    """
    Handle user login.
    
    Request JSON:
        {
            "email": "user@stb.tn",
            "password": "password"
        }
    
    Response JSON:
        {
            "success": true,
            "session_token": "token",
            "user": {
                "email": "user@stb.tn",
                "name": "User Name",
                "role": "agent"
            }
        }
    """
    try:
        data = request.get_json()
        email = data.get('email', '').strip()
        password = data.get('password', '')
        
        if not email or not password:
            return jsonify({
                'success': False,
                'message': 'Email et mot de passe requis'
            }), 400
        
        # Authenticate user
        user = auth_manager.authenticate(email, password)
        
        # Get client IP
        ip_address = request.remote_addr
        
        if user:
            # Create session
            session_token = auth_manager.create_session(user)
            
            # Log successful login
            access_logger.log_login(email, success=True, ip_address=ip_address)
            
            print(f"✓ Login successful: {email} ({user['role']})")
            
            return jsonify({
                'success': True,
                'session_token': session_token,
                'user': user
            })
        else:
            # Log failed login
            access_logger.log_login(email, success=False, ip_address=ip_address)
            
            print(f"✗ Login failed: {email}")
            
            return jsonify({
                'success': False,
                'message': 'Email ou mot de passe incorrect'
            }), 401
    
    except Exception as error:
        print(f"❌ Login error: {str(error)}")
        return jsonify({
            'success': False,
            'message': 'Erreur serveur'
        }), 500


@app.route('/api/logout', methods=['POST'])
def logout():
    """Handle user logout."""
    try:
        # Get session token from header
        session_token = request.headers.get('Authorization', '').replace('Bearer ', '')
        
        if session_token:
            # Get user before destroying session
            user = auth_manager.verify_session(session_token)
            
            if user:
                # Log logout
                access_logger.log_logout(user['email'], ip_address=request.remote_addr)
            
            # Destroy session
            auth_manager.destroy_session(session_token)
        
        return jsonify({
            'success': True
        })
    
    except Exception as error:
        print(f"❌ Logout error: {str(error)}")
        return jsonify({
            'success': False,
            'message': 'Erreur serveur'
        }), 500


@app.route('/api/ask', methods=['POST'])
def ask_question():
    """
    Handle question-answering requests.
    
    Requires authentication via session token in Authorization header.
    
    Request JSON:
        {
            "question": "user's question"
        }
    
    Response JSON:
        {
            "question": "original question",
            "answer": "formatted answer",
            "confidence": 85.5,
            "confidence_label": "high",
            "sources": [
                {
                    "text": "chunk text",
                    "document": "filename.txt",
                    "type": "procedure"
                }
            ]
        }
    """
    try:
        # Check authentication
        session_token = request.headers.get('Authorization', '').replace('Bearer ', '')
        user = auth_manager.verify_session(session_token)
        
        if not user:
            return jsonify({
                'error': 'Non autorisé. Veuillez vous connecter.'
            }), 401
        
        # Get question from request
        data = request.get_json()
        question = data.get('question', '').strip()
        
        if not question:
            return jsonify({
                'error': 'Question is required'
            }), 400
        
        print(f"\n📝 Question from {user['email']}: {question}")
        
        # Retrieve relevant chunks (text only for search)
        relevant_chunks_text = simple_search(question, chunks_text)
        print(f"✓ Found {len(relevant_chunks_text)} relevant chunks")
        
        # Generate answer with confidence
        result = generate_answer_with_confidence(question, relevant_chunks_text)
        print(f"✓ Generated answer (confidence: {result['confidence']}% - {result['confidence_label']})")
        
        # Log the question
        access_logger.log_question(
            user['email'], 
            question, 
            result['confidence'],
            ip_address=request.remote_addr
        )
        
        # Get metadata for each relevant chunk
        sources_with_metadata = []
        for chunk_text in relevant_chunks_text:
            metadata = doc_manager.get_chunk_metadata(chunk_text)
            if metadata:
                sources_with_metadata.append({
                    'text': chunk_text,
                    'document': metadata['source_document'],
                    'type': metadata['document_type'],
                    'modified': metadata['modified']
                })
            else:
                # Fallback if metadata not found
                sources_with_metadata.append({
                    'text': chunk_text,
                    'document': 'unknown',
                    'type': 'procedure'
                })
        
        # Return response
        response = {
            'question': question,
            'answer': result['answer'],
            'confidence': result['confidence'],
            'confidence_label': result['confidence_label'],
            'sources': sources_with_metadata
        }
        
        return jsonify(response)
    
    except Exception as error:
        print(f"❌ Error: {str(error)}")
        return jsonify({
            'error': 'Internal server error',
            'message': str(error)
        }), 500


@app.route('/api/health', methods=['GET'])
def health_check():
    """Health check endpoint."""
    stats = doc_manager.get_document_stats()
    return jsonify({
        'status': 'healthy',
        'documents_loaded': stats['total_documents'],
        'chunks_count': stats['total_chunks'],
        'documents': stats['documents']
    })


if __name__ == '__main__':
    print("\n" + "=" * 60)
    print("STB Copilot - API Server")
    print("=" * 60)
    
    stats = doc_manager.get_document_stats()
    
    print(f"\n🚀 Starting server...")
    print(f"📄 Documents loaded: {stats['total_documents']}")
    print(f"📦 Total chunks: {stats['total_chunks']}")
    print(f"\n🌐 Frontend: http://localhost:5000")
    print(f"🔌 API: http://localhost:5000/api/ask")
    print("\n" + "=" * 60 + "\n")
    
    app.run(debug=True, host='0.0.0.0', port=5000)
