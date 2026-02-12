"""
STB Copilot - Main Entry Point

This is the main script that orchestrates the complete pipeline:
1. Load document
2. Chunk the text
3. Retrieve relevant chunks
4. Generate structured answer
5. Display results
"""

from chunker import chunk_text
from retriever import simple_search
from answer_generator import generate_answer


def load_document(file_path):
    """
    Load a document from the file system.
    
    Args:
        file_path (str): Path to the document file
    
    Returns:
        str: Content of the document
    """
    with open(file_path, "r", encoding="utf-8") as file:
        content = file.read()
    return content


def print_separator():
    """Print a visual separator line."""
    print("\n" + "=" * 60 + "\n")


def print_section_header(title):
    """Print a section header."""
    print(f"\n{title}")
    print("-" * len(title))


def main():
    """
    Main function that runs the STB Copilot pipeline.
    """
    # Configuration
    document_path = "../docs/open_account_procedure.txt"
    question = "What documents are required?"
    
    print_separator()
    print("STB Copilot - Banking AI Assistant")
    print_separator()
    
    # Step 1: Load the document
    print("Loading document...")
    document_content = load_document(document_path)
    print(f"✓ Loaded: {document_path}")
    
    # Step 2: Chunk the document
    print("\nChunking document...")
    chunks = chunk_text(document_content)
    print(f"✓ Created {len(chunks)} chunks")
    
    # Step 3: Retrieve relevant chunks
    print(f"\nSearching for relevant information...")
    print(f"Question: \"{question}\"")
    relevant_chunks = simple_search(question, chunks)
    print(f"✓ Found {len(relevant_chunks)} relevant chunks")
    
    # Step 4: Generate answer
    print("\nGenerating answer...")
    answer = generate_answer(question, relevant_chunks)
    print("✓ Answer generated")
    
    # Step 5: Display results
    print_separator()
    print_section_header("QUESTION")
    print(question)
    
    print_section_header("ANSWER")
    print(answer)
    
    print_section_header("SOURCE CHUNKS")
    for index, chunk in enumerate(relevant_chunks, start=1):
        print(f"\n[Chunk {index}]")
        print(chunk)
    
    print_separator()
    print("✓ Process completed successfully")
    print_separator()


if __name__ == "__main__":
    main()
