def simple_search(question, chunks):
    results = []
    for chunk in chunks:
        if any(word.lower() in chunk.lower() for word in question.split()):
            results.append(chunk)
    return results

