# STB Copilot - Banking AI Assistant

A minimal MVP of a banking AI assistant that processes documents, retrieves relevant information, and generates structured answers through a beautiful web interface.

## 🚀 Quick Start

### 1. Install Dependencies

```bash
cd backend
sudo apt install python3-pip  # If pip not installed
pip3 install -r requirements.txt
```

### 2. Run the Server

```bash
cd backend
python3 api.py
```

### 3. Open the Application

Navigate to: **http://localhost:5000**

## 📁 Project Structure

```
stb-copilot/
├── backend/
│   ├── api.py              # Flask API server
│   ├── chunker.py          # Text chunking
│   ├── retriever.py        # Keyword-based retrieval
│   ├── answer_generator.py # Answer generation
│   ├── main.py             # CLI entry point
│   └── requirements.txt    # Python dependencies
├── frontend/
│   ├── index.html          # Main HTML
│   ├── style.css           # Premium design
│   └── script.js           # Chat interaction
├── docs/
│   └── open_account_procedure.txt
└── README.md
```

## 💬 Usage

**Ask questions like:**
- "What documents are required?"
- "What are the steps to open an account?"
- "Who needs to approve the process?"

The system will:
1. Search through the document
2. Find relevant information
3. Generate a structured answer
4. Show you the source chunks used

## 🎨 Features

- **Beautiful UI**: Dark mode with glassmorphism effects
- **Chat Interface**: Conversational Q&A experience
- **Structured Answers**: Numbered steps and bullet points
- **Source Attribution**: See which document chunks were used
- **Responsive Design**: Works on desktop and mobile

## 🛠️ Technology Stack

**Frontend:**
- HTML5, CSS3, JavaScript (vanilla)
- Google Fonts (Inter)

**Backend:**
- Python 3
- Flask (API server)
- Simple rule-based NLP (no external AI services)

## 📝 CLI Mode

You can also run the backend in CLI mode:

```bash
cd backend
python3 main.py
```

## 🎓 Academic Project

This is a PFE (Projet de Fin d'Études) project for STB - a minimal MVP demonstrating AI-assisted banking decision support.

**Constraints:**
- Beginner-friendly code
- No complex frameworks
- No databases
- No embeddings (yet)
- Clean separation of concerns

## 📄 License

Academic project - STB Copilot


I want to explain my project so you clearly understand my situation before helping me.

My name is Mohamed Amir Sboui. I am a final year student in Business Informatics at FSEGT (Tunisia). I am currently doing my PFE (final year project).

My project is called: STB Copilot.

The goal of the project is to build an internal assistant for a bank (STB). The idea is to help employees quickly find internal procedures and information using a chatbot-like system.

Instead of searching manually through documents, the employee can ask a question and the system will return the relevant information from the internal procedure documents.

Project objectives:
- Allow employees (agents) to search internal procedures easily
- Provide a chatbot-like interface for asking questions
- Process internal procedure documents
- Retrieve the most relevant information
- Return a clear answer to the user

Actors in the system:
1. Agent (bank employee)
   - logs into the system
   - asks questions about procedures
   - receives answers from the system

2. Administrator / Back Office
   - uploads internal procedure documents (PDF)
   - manages the document database
   - triggers document processing

Main system features:
- User authentication
- Upload procedure documents
- Document processing (extract text)
- Document chunking
- Indexing documents
- Search and retrieval
- Question answering interface

Technology stack required for the PFE:
Backend: .NET (ASP.NET Core Web API)
Frontend: Angular

The backend handles:
- API endpoints
- document processing
- search logic
- communication with the frontend

The Angular frontend provides:
- login page
- question interface
- display of answers

Important context:
I am still learning .NET and Angular. My goal is not to build a complex enterprise system, but to create a working prototype (MVP) that demonstrates the concept.

This project will be:
- presented in my PFE defense
- included in my GitHub portfolio
- included in my CV as a real project

What I need from you:
1. Help me design a clear UML architecture for the system using PlantUML.
2. Generate detailed UML diagrams:
   - Use Case Diagram
   - Class Diagram
   - Sequence Diagram
   - Component Diagram
3. Keep the architecture simple enough so a student can understand it.

Important:
The goal of this project is learning + demonstration, not a production-level banking system.

Please generate UML diagrams that are realistic for a final year software engineering project.
