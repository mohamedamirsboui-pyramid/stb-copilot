"""
Document Manager for STB Copilot

Handles loading and managing multiple documents with metadata tracking.
"""

import os
from datetime import datetime
from chunker import chunk_text


class DocumentManager:
    """Manages multiple documents and their metadata."""
    
    def __init__(self, docs_directory):
        """
        Initialize the document manager.
        
        Args:
            docs_directory (str): Path to the documents directory
        """
        self.docs_directory = docs_directory
        self.documents = []
        self.chunks_with_metadata = []
    
    def load_all_documents(self):
        """
        Load all text documents from the docs directory.
        
        Returns:
            list: List of document dictionaries with metadata
        """
        self.documents = []
        
        # Get all .txt files in the docs directory
        for filename in os.listdir(self.docs_directory):
            if filename.endswith('.txt'):
                filepath = os.path.join(self.docs_directory, filename)
                
                try:
                    with open(filepath, 'r', encoding='utf-8') as file:
                        content = file.read()
                    
                    # Get file metadata
                    file_stats = os.stat(filepath)
                    modified_time = datetime.fromtimestamp(file_stats.st_mtime)
                    
                    document = {
                        'filename': filename,
                        'filepath': filepath,
                        'content': content,
                        'size': file_stats.st_size,
                        'modified': modified_time.strftime('%Y-%m-%d %H:%M:%S'),
                        'type': 'procedure'  # Can be extended later
                    }
                    
                    self.documents.append(document)
                    print(f"✓ Loaded: {filename}")
                
                except Exception as error:
                    print(f"✗ Error loading {filename}: {str(error)}")
        
        print(f"\n✓ Total documents loaded: {len(self.documents)}")
        return self.documents
    
    def chunk_all_documents(self):
        """
        Chunk all loaded documents and track source metadata.
        
        Returns:
            list: List of chunks with metadata (text, source, document_name)
        """
        self.chunks_with_metadata = []
        
        for document in self.documents:
            # Chunk the document
            chunks = chunk_text(document['content'])
            
            # Add metadata to each chunk
            for chunk in chunks:
                chunk_with_metadata = {
                    'text': chunk,
                    'source_document': document['filename'],
                    'document_type': document['type'],
                    'modified': document['modified']
                }
                self.chunks_with_metadata.append(chunk_with_metadata)
        
        print(f"✓ Total chunks created: {len(self.chunks_with_metadata)}")
        return self.chunks_with_metadata
    
    def get_chunks_text_only(self):
        """
        Get just the text of all chunks (for compatibility with existing code).
        
        Returns:
            list: List of chunk texts
        """
        return [chunk['text'] for chunk in self.chunks_with_metadata]
    
    def get_chunk_metadata(self, chunk_text):
        """
        Get metadata for a specific chunk.
        
        Args:
            chunk_text (str): The chunk text to find metadata for
        
        Returns:
            dict: Chunk metadata or None if not found
        """
        for chunk in self.chunks_with_metadata:
            if chunk['text'] == chunk_text:
                return {
                    'source_document': chunk['source_document'],
                    'document_type': chunk['document_type'],
                    'modified': chunk['modified']
                }
        return None
    
    def get_document_stats(self):
        """
        Get statistics about loaded documents.
        
        Returns:
            dict: Statistics including document count, total chunks, etc.
        """
        return {
            'total_documents': len(self.documents),
            'total_chunks': len(self.chunks_with_metadata),
            'documents': [
                {
                    'name': doc['filename'],
                    'size': doc['size'],
                    'modified': doc['modified']
                }
                for doc in self.documents
            ]
        }
