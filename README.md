# PHI Redaction Application

A secure application that processes lab order documents to identify and redact Protected Health Information (PHI).

## 🏥 Features

- **File Upload**: Select multiple lab order text files for processing
- **PHI Detection**: Automatically identifies and redacts sensitive information
- **Secure Processing**: Files processed temporarily with no permanent storage
- **Download Results**: Get sanitized files with `_sanitized.txt` suffix

## 🔒 PHI Elements Detected

- Patient Names
- Date of Birth
- Social Security Numbers
- Addresses
- Phone Numbers
- Email Addresses
- Medical Record Numbers
- Account Numbers
- Other identifiers

## 🚀 Quick Start (Docker - Recommended)

### Prerequisites
- Docker and Docker Compose

### One-Command Setup
```bash
# Build and start the application
./build-and-run.sh
```

**Access the application:**
- Frontend: http://localhost:4200
- Backend API: http://localhost:5001
- API Documentation: http://localhost:5001/swagger

## 💻 Local Development

### Prerequisites
- .NET 6 SDK
- Node.js (16+)
- Angular CLI

### Setup
```bash
# Start everything locally
./run-local.sh

# Or manually:
# Terminal 1 - Backend
cd PHIRedactionAPI
dotnet run

# Terminal 2 - Frontend
cd phi-redaction-app
npm install
ng serve
```

## 🧪 Testing

1. Use the sample file in `sample-data/sample-lab-order.txt`
2. Upload it through the web interface
3. Process the file
4. Download and verify PHI is redacted to `[REDACTED]`

## 📁 Project Structure

```
PHIRedactionApp/
├── PHIRedactionAPI/          # .NET Web API
├── phi-redaction-app/        # Angular Frontend
├── sample-data/              # Test files
├── build-and-run.sh          # Docker setup
├── dev-setup.sh             # Development mode
├── run-local.sh             # Local development
└── docker-compose.yml       # Container orchestration
```

## 🛠️ Development Scripts

- `./build-and-run.sh` - Full Docker deployment
- `./dev-setup.sh` - Backend in Docker, frontend local
- `./run-local.sh` - Everything running locally

## 🔧 Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Set to Production for deployment
- Frontend API URL configured in `environment.ts` files

### Docker
- Backend: Port 5001
- Frontend: Port 4200
- Health checks enabled
- Volume persistence for processed files

## 🏗️ Architecture

- **Backend**: ASP.NET Core Web API with PHI redaction service
- **Frontend**: Angular SPA with file upload and processing UI
- **Processing**: Regex-based pattern matching for PHI detection
- **Security**: CORS enabled, temporary file processing, no PHI persistence

## 📝 Usage

1. Open http://localhost:4200
2. Click "Select Files" and choose lab order .txt files
3. Click "Process Files" to start redaction
4. Monitor processing status
5. Download sanitized files when complete

## 🤝 Contributing

This is a take-home assessment project. The focus is on demonstrating:
- Clean, maintainable code
- Security awareness for PHI handling
- Modern development practices
- Docker containerization

## 🔒 Security Notes

- Files are processed in memory only
- No PHI is logged or persisted
- Temporary files are cleaned up automatically
- HTTPS enforced in production
