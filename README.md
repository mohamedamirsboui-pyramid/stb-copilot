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
