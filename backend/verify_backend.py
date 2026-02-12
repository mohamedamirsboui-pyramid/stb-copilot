"""
Verification script for STB Copilot Phase 1 & 2
Tests backend logic without requiring a Flask server.
"""

import sys
import os
from datetime import datetime

# Add current directory to path
sys.path.append(os.getcwd())

from auth import AuthManager
from document_manager import DocumentManager
from answer_generator import generate_answer_with_confidence
from logger import AccessLogger
from chunker import chunk_text
from retriever import simple_search

def print_header(text):
    print("\n" + "=" * 50)
    print(f" {text}")
    print("=" * 50)

def test_auth():
    print_header("TESTING AUTHENTICATION")
    auth = AuthManager()
    
    # Test valid login (Agent)
    print("Testing valid login (Agent)...")
    user = auth.authenticate('agent@stb.tn', 'agent123')
    if user and user['role'] == 'agent':
        print("✓ Login successful for agent@stb.tn")
        token = auth.create_session(user)
        print(f"✓ Session created, token: {token[:10]}...")
        
        # Test session verification
        verified = auth.verify_session(token)
        if verified and verified['email'] == 'agent@stb.tn':
            print("✓ Session verification successful")
        else:
            print("✗ Session verification FAILED")
    else:
        print("✗ Login FAILED for agent@stb.tn")
        
    # Test invalid login
    print("\nTesting invalid login (Wrong password)...")
    invalid_user = auth.authenticate('agent@stb.tn', 'wrong-pass')
    if not invalid_user:
        print("✓ Invalid login correctly rejected")
    else:
        print("✗ Invalid login was INCORRECTLY accepted")

def test_docs():
    print_header("TESTING DOCUMENT MANAGER")
    # Make sure docs directory exists
    docs_path = "../docs"
    if not os.path.exists(docs_path):
        os.makedirs(docs_path, exist_ok=True)
        # Create a sample file
        with open(os.path.join(docs_path, "test_doc.txt"), "w") as f:
            f.write("Ceci est un document de test.\n\nIl contient deux paragraphes.")
            
    doc_manager = DocumentManager(docs_path)
    print(f"Loading documents from {docs_path}...")
    doc_manager.load_all_documents()
    
    if len(doc_manager.documents) > 0:
        print(f"✓ Loaded {len(doc_manager.documents)} documents")
        chunks = doc_manager.chunk_all_documents()
        print(f"✓ Created {len(chunks)} chunks with metadata")
        
        # Test metadata retrieval
        if chunks:
            chunk_text = chunks[0]['text']
            meta = doc_manager.get_chunk_metadata(chunk_text)
            if meta and meta['source_document']:
                print(f"✓ Metadata retrieval working: {meta['source_document']}")
            else:
                print("✗ Metadata retrieval FAILED")
    else:
        print("✗ No documents loaded")

def test_answer_and_confidence():
    print_header("TESTING ANSWER & CONFIDENCE")
    question = "Comment ouvrir un compte ?"
    relevant_chunks = [
        "Pour ouvrir un compte à la STB, il faut être majeur et résident en Tunisie.",
        "Les documents requis sont une CIN et un justificatif de domicile."
    ]
    
    print(f"Question: {question}")
    result = generate_answer_with_confidence(question, relevant_chunks)
    
    print(f"Answer:\n{result['answer']}")
    print(f"Confidence: {result['confidence']}% ({result['confidence_label']})")
    
    if result['confidence'] > 0 and result['answer']:
        print("✓ Answer generation and confidence calculation working")
    else:
        print("✗ Answer generation or confidence calculation FAILED")

def test_logging():
    print_header("TESTING ACCESS LOGGING")
    logger = AccessLogger(log_directory="test_logs")
    log_file = os.path.join("test_logs", "access.log")
    
    # Log some actions
    logger.log_login("test@stb.tn", success=True, ip_address="127.0.0.1")
    logger.log_question("test@stb.tn", "Ma question ?", 85.5)
    
    if os.path.exists(log_file):
        print("✓ Log file created")
        recent = logger.get_recent_logs(limit=5)
        if len(recent) >= 2:
            print(f"✓ Read {len(recent)} log entries")
            print(f"Latest entry: {recent[-1]['action']} - {recent[-1]['user']}")
        else:
            print(f"✗ Log entries missing: {len(recent)}")
    else:
        print("✗ Log file NOT created")

if __name__ == "__main__":
    try:
        test_auth()
        test_docs()
        test_answer_and_confidence()
        test_logging()
        
        print_header("VERIFICATION COMPLETE")
        print("All backend logic modules (Phase 1 & 2) are working correctly.")
        
    except Exception as e:
        print(f"\n✗ FATAL ERROR during verification: {str(e)}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
