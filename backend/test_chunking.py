from chunker import chunk_text
from retriever import simple_search

with open("../docs/open_account_procedure.txt", "r") as f:
    content = f.read()

chunks = chunk_text(content)

question = "What documents are required?"

results = simple_search(question, chunks)

print("\nRelevant Chunks:")
for r in results:
    print("\n---")
    print(r)

