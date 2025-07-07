# PHI Redaction Application

A secure application that processes lab order documents to identify and redact Protected Health Information (PHI). Built with Angular frontend and .NET Core backend.

**Repository**: <https://github.com/jdaake/PHIRedactionApp>

## Getting Started

### Clone the Repository

First, clone this repository to your local machine:

```bash
git clone https://github.com/jdaake/PHIRedactionApp.git
cd PHIRedactionApp
```

## Features

- **File Upload Interface**: Select multiple lab order text files for processing
- **Automated PHI Detection**: Identifies and redacts sensitive information using regex patterns
- **Secure Processing**: Files are processed temporarily with no permanent storage
- **Download Results**: Get sanitized files with `_sanitized.txt` suffix
- **Real-time Status Updates**: Monitor processing progress
- **Cross-platform Support**: Runs on Windows, macOS, and Linux

## PHI Elements Detected and Redacted

The application identifies and redacts the following PHI elements:

- Patient Names
- Date of Birth (various formats)
- Social Security Numbers
- Physical Addresses
- Phone Numbers
- Email Addresses
- Medical Record Numbers
- Account Numbers
- Patient ID Numbers
- ZIP Codes

All detected PHI is replaced with `[REDACTED]` while preserving document structure.

## Quick Start

### Prerequisites

**Option 1: Docker (Recommended)**

- Docker Desktop installed and running
- No additional dependencies required

**Option 2: Local Development**

- .NET 6 SDK
- Node.js 16+ and npm
- Angular CLI (`npm install -g @angular/cli`)

### One-Command Startup

```bash
# Interactive startup with system detection
./start-app.sh
```

This script will:

- Check system compatibility
- Offer Docker or local development options
- Handle architecture issues (especially on Apple Silicon Macs)
- Start both frontend and backend services
- Monitor services until stopped with Ctrl+C

### Development Mode

For development with hot reload:

```bash
./start-dev.sh
```

### Check Service Status

```bash
./check-status.sh
```

## Manual Setup

### Docker Deployment

```bash
# Build and start containers
docker compose up --build -d

# View logs
docker compose logs -f

# Stop services
docker compose down
```

### Local Development

**Backend:**

```bash
cd PHIRedactionAPI
dotnet restore
dotnet run
```

**Frontend (separate terminal):**

```bash
cd phi-redaction-app
npm install
ng serve
```

## Application URLs

- **Frontend**: <http://localhost:4200>
- **Backend API**: <http://localhost:5001>
- **API Documentation**: <http://localhost:5001/swagger>
- **Health Check**: <http://localhost:5001/health>

## Usage Instructions

1. Open the application at <http://localhost:4200>
2. Click "Select Files" to choose lab order text files (.txt format)
3. Click "Process Files" to begin PHI redaction
4. Monitor the processing status in real-time
5. Download sanitized files when processing completes
6. Verify that PHI has been replaced with `[REDACTED]`

## Sample Data

A sample lab order file is provided in `sample-data/sample-lab-order.txt` for testing purposes. This file contains realistic PHI data that demonstrates the application's redaction capabilities.

## Project Structure

```
PHIRedactionApp/
├── PHIRedactionAPI/          # .NET Core Web API
│   ├── Controllers/          # API endpoints
│   ├── Services/             # PHI redaction logic
│   ├── Models/               # Data models
│   └── Dockerfile            # Backend container config
├── phi-redaction-app/        # Angular frontend
│   ├── src/app/              # Angular components and services
│   ├── src/environments/     # Environment configurations
│   └── Dockerfile            # Frontend container config
├── sample-data/              # Test files
├── docker-compose.yml        # Container orchestration
├── start-app.sh             # Main startup script
├── start-dev.sh             # Development mode
├── check-status.sh          # Status monitoring
└── README.md                # This file
```

## Architecture

### Backend (.NET Core)

- **ASP.NET Core Web API** with .NET 6
- **PHI Redaction Service** using regex pattern matching
- **File Processing Controller** handling upload and download
- **CORS Configuration** for cross-origin requests
- **Swagger Documentation** for API exploration

### Frontend (Angular)

- **Angular 15** single-page application
- **File Upload Component** with drag-and-drop support
- **Processing Status Display** with real-time updates
- **Download Management** with error handling
- **Responsive Design** for desktop and mobile

### Containerization

- **Multi-stage Docker builds** for optimized images
- **Docker Compose** orchestration for development
- **Health checks** for service monitoring
- **Volume persistence** for processed files

## PHI Detection Technology

The application uses sophisticated regex patterns to identify PHI:

- **Labeled Field Detection**: Recognizes structured data with labels (e.g., "Patient Name: John Doe")
- **Standalone Pattern Matching**: Identifies PHI without labels (e.g., standalone SSNs, phone numbers)
- **Context-Aware Processing**: Maintains document structure while redacting sensitive data
- **Multiple Format Support**: Handles various date formats, phone number formats, and address styles

## Security Considerations

- **Temporary Processing**: Files are processed in memory and not permanently stored
- **No PHI Logging**: Sensitive information is never written to logs
- **Automatic Cleanup**: Processed files are cleaned up after download
- **HTTPS Ready**: Production configuration supports SSL/TLS
- **CORS Protection**: Configured for specific origins only

## Environment Configurations

### Development

- Frontend: <http://localhost:4200>
- Backend: <http://localhost:5001>
- Hot reload enabled
- Detailed error messages

### Production

- HTTPS enforcement
- Optimized builds
- Compressed assets
- Security headers enabled

## Troubleshooting

### Common Issues

**Docker not starting:**

- Ensure Docker Desktop is running
- Check available disk space (minimum 4GB)
- Verify Docker has sufficient memory allocation

**Port conflicts:**

- Default ports: 4200 (frontend), 5001 (backend)
- Use `./check-status.sh` to identify conflicts
- Kill conflicting processes or modify port configuration

**Apple Silicon Mac issues:**

- Install ARM64 version of .NET 6
- Use Docker for easiest compatibility
- Avoid Intel x64 .NET versions

**File upload/download issues:**

- Verify backend health endpoint: <http://localhost:5001/health>
- Check browser console for CORS errors
- Ensure file permissions allow read/write operations

### Logs and Debugging

**Docker logs:**

```bash
docker-compose logs backend
docker-compose logs frontend
```

**Local development logs:**

- Backend logs appear in terminal running `dotnet run`
- Frontend logs appear in browser developer tools
- Check `backend.log` and `frontend.log` if running with scripts

## Development Workflow

### Making Changes

**Backend changes:**

- Modify files in `PHIRedactionAPI/`
- Restart with `dotnet run` or use `./start-dev.sh` for auto-reload

**Frontend changes:**

- Modify files in `phi-redaction-app/src/`
- Changes auto-reload when using `ng serve` or `./start-dev.sh`

### Testing

**Manual testing:**

1. Use provided sample file in `sample-data/`
2. Upload through web interface
3. Verify PHI redaction in downloaded file
4. Test various file formats and PHI patterns

**API testing:**

- Access Swagger UI at <http://localhost:5001/swagger>
- Test endpoints directly
- Verify health check endpoint

## Deployment Options

### Local Development

- Use `./start-app.sh` for guided setup
- Use `./start-dev.sh` for development with hot reload

### Docker Production

- Use `docker-compose up --build -d` for containerized deployment
- Configure environment variables for production settings
- Set up reverse proxy (Nginx) for production traffic

### Cloud Deployment

- Backend: Deploy to Azure App Service, AWS ECS, or Google Cloud Run
- Frontend: Deploy to Azure Static Web Apps, AWS S3, or Netlify
- Database: Add persistent storage for audit logs if required

## Performance Considerations

- **File Size Limits**: Default limit is 100MB per file
- **Concurrent Processing**: Backend handles multiple files sequentially
- **Memory Usage**: Files processed in memory for security
- **Scaling**: Use multiple backend instances behind load balancer for high volume

## Contributing

This project was developed as a take-home assessment demonstrating:

- Modern full-stack development practices
- Security-conscious handling of sensitive data
- Professional code organization and documentation
- Cross-platform compatibility and deployment options

## Future Enhancements

Given additional time, potential improvements include:

- Machine learning models for advanced PHI detection
- Support for additional file formats (PDF, DOCX)
- Audit logging and compliance reporting
- Batch processing for large file volumes
- Role-based access controls
- Integration with healthcare systems APIs

## Technology Stack

- **Frontend**: Angular 15, TypeScript, RxJS, Angular CLI
- **Backend**: .NET 6, ASP.NET Core, C#
- **Containerization**: Docker, Docker Compose
- **Development Tools**: Visual Studio Code, npm, dotnet CLI
