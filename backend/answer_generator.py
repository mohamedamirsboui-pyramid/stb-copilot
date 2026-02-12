"""
Answer Generator for STB Copilot

This module generates structured answers from retrieved document chunks.
It uses simple rule-based logic to extract and format information.
"""

def generate_answer(question, relevant_chunks):
    """
    Generate a structured answer from relevant chunks.
    
    Args:
        question (str): The user's question
        relevant_chunks (list): List of text chunks retrieved as relevant
    
    Returns:
        str: A formatted answer with numbered steps or bullet points
    """
    if not relevant_chunks:
        return "I couldn't find relevant information to answer your question."
    
    # Combine all relevant chunks into one text
    combined_text = "\n\n".join(relevant_chunks)
    
    # Try to extract numbered steps from the chunks
    steps = extract_numbered_steps(combined_text)
    
    if steps:
        # Format as numbered list
        answer = format_as_numbered_list(steps)
    else:
        # If no numbered steps found, create bullet points from chunks
        answer = format_as_bullets(relevant_chunks)
    
    return answer


def generate_answer_with_confidence(question, relevant_chunks):
    """
    Generate answer with confidence indicator.
    
    Args:
        question (str): The user's question
        relevant_chunks (list): List of text chunks retrieved as relevant
    
    Returns:
        dict: Answer with confidence score and label
    """
    answer = generate_answer(question, relevant_chunks)
    confidence = calculate_confidence(question, relevant_chunks)
    confidence_label = get_confidence_label(confidence)
    
    return {
        'answer': answer,
        'confidence': confidence,
        'confidence_label': confidence_label
    }


def calculate_confidence(question, chunks):
    """
    Calculate confidence score for the answer.
    
    Based on:
    - Number of relevant chunks found
    - Keyword overlap between question and chunks
    
    Args:
        question (str): The user's question
        chunks (list): Relevant chunks found
    
    Returns:
        float: Confidence score (0-100)
    """
    if not chunks:
        return 0.0
    
    # Factor 1: Number of chunks (more chunks = more confidence)
    chunk_score = min(len(chunks) / 3.0, 1.0) * 40
    
    # Factor 2: Keyword overlap
    question_words = set(question.lower().split())
    
    total_overlap = 0
    for chunk in chunks:
        chunk_words = set(chunk.lower().split())
        overlap = len(question_words & chunk_words)
        total_overlap += overlap
    
    overlap_score = min(total_overlap / len(question_words), 1.0) * 60
    
    # Combine scores
    confidence = chunk_score + overlap_score
    
    return round(confidence, 1)


def get_confidence_label(confidence):
    """
    Get confidence label based on score.
    
    Args:
        confidence (float): Confidence score (0-100)
    
    Returns:
        str: Confidence label (high/medium/low)
    """
    if confidence >= 70:
        return 'high'
    elif confidence >= 40:
        return 'medium'
    else:
        return 'low'


def extract_numbered_steps(text):
    """
    Extract numbered steps from text.
    
    Looks for patterns like:
    - "Step 1: ..."
    - "1. ..."
    - "Step 1 - ..."
    
    Args:
        text (str): Text to extract steps from
    
    Returns:
        list: List of step descriptions (without the step numbers)
    """
    steps = []
    lines = text.split("\n")
    
    for line in lines:
        line = line.strip()
        
        # Check if line starts with "Step X:" pattern
        if line.startswith("Step ") and ":" in line:
            # Extract the part after the colon
            step_content = line.split(":", 1)[1].strip()
            if step_content:
                steps.append(step_content)
        
        # Check if line starts with bullet points that contain steps
        elif line.startswith("- ") and len(line) > 2:
            step_content = line[2:].strip()
            if step_content:
                steps.append(step_content)
    
    return steps


def format_as_numbered_list(steps):
    """
    Format a list of steps as a numbered list.
    
    Args:
        steps (list): List of step descriptions
    
    Returns:
        str: Formatted numbered list
    """
    formatted_steps = []
    
    for index, step in enumerate(steps, start=1):
        formatted_steps.append(f"{index}. {step}")
    
    return "\n".join(formatted_steps)


def format_as_bullets(chunks):
    """
    Format chunks as bullet points when no numbered steps are found.
    
    Args:
        chunks (list): List of text chunks
    
    Returns:
        str: Formatted bullet list
    """
    formatted_bullets = []
    
    for chunk in chunks:
        # Clean up the chunk and use first sentence or line
        chunk = chunk.strip()
        if chunk:
            formatted_bullets.append(f"• {chunk}")
    
    return "\n".join(formatted_bullets)
