import { Component } from '@angular/core';
import { FileProcessingService } from './services/file-processing.service';
import { ProcessingResult, FileProcessingResult } from './models/processing-result.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'PHI Redaction Application';
  selectedFiles: FileList | null = null;
  isProcessing = false;
  processingResults: ProcessingResult | null = null;
  errorMessage = '';
  isDownloading = false;
  downloadError = '';

  constructor(private fileProcessingService: FileProcessingService) {}

  async ngOnInit() {
    // Test connection on startup
    const isConnected = await this.fileProcessingService.testConnection();
    if (!isConnected) {
      this.errorMessage = 'Cannot connect to backend API. Please ensure the backend is running.';
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFiles = input.files;
    this.processingResults = null;
    this.errorMessage = '';
    this.downloadError = '';
  }

  async processFiles(): Promise<void> {
    if (!this.selectedFiles || this.selectedFiles.length === 0) {
      this.errorMessage = 'Please select files to process';
      return;
    }

    this.isProcessing = true;
    this.errorMessage = '';
    this.processingResults = null;

    try {
      this.processingResults = await this.fileProcessingService.processFiles(this.selectedFiles);
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : 'An error occurred while processing files';
      console.error('Error processing files:', error);
    } finally {
      this.isProcessing = false;
    }
  }

  async downloadFile(result: FileProcessingResult): Promise<void> {
    if (!result.success || !result.downloadUrl) {
      this.downloadError = 'File is not available for download';
      return;
    }

    this.isDownloading = true;
    this.downloadError = '';

    try {
      await this.fileProcessingService.downloadFile(result.downloadUrl, result.processedFileName);
      console.log('Download completed for:', result.processedFileName);
    } catch (error) {
      this.downloadError = error instanceof Error ? error.message : 'Download failed';
      console.error('Download error:', error);
    } finally {
      this.isDownloading = false;
    }
  }

  getSelectedFilesList(): string[] {
    if (!this.selectedFiles) return [];
    return Array.from(this.selectedFiles).map(file => file.name);
  }

  resetForm(): void {
    this.selectedFiles = null;
    this.processingResults = null;
    this.errorMessage = '';
    this.downloadError = '';
    
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }
}
