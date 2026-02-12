def chunk_text(text):
    chunks = text.split("\n\n")
    return [chunk.strip() for chunk in chunks if chunk.strip()]
