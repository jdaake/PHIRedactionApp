import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ProcessingResult } from '../models/processing-result.model';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FileProcessingService {
  private readonly apiUrl = environment.apiUrl;
  private readonly baseUrl = environment.apiUrl.replace('/api', ''); // Base URL without /api

  constructor(private http: HttpClient) { }

  async processFiles(files: FileList): Promise<ProcessingResult> {
    const formData = new FormData();
    
    for (let i = 0; i < files.length; i++) {
      formData.append('files', files[i]);
    }

    try {
      const response = await lastValueFrom(
        this.http.post<ProcessingResult>(`${this.apiUrl}/files/process`, formData)
      );
      return response;
    } catch (error) {
      console.error('Error processing files:', error);
      
      if (error instanceof HttpErrorResponse) {
        if (error.status === 0) {
          throw new Error('Unable to connect to the server. Please ensure the API is running.');
        } else if (error.status >= 400 && error.status < 500) {
          throw new Error(error.error?.message || 'Invalid request. Please check your files and try again.');
        } else if (error.status >= 500) {
          throw new Error('Server error occurred. Please try again later.');
        }
      }
      
      throw new Error('An unexpected error occurred while processing files.');
    }
  }

  async downloadFile(downloadUrl: string, fileName: string): Promise<void> {
    try {
      console.log('Original downloadUrl from backend:', downloadUrl);
      
      // Fix: downloadUrl already contains /api/files/download/filename
      // So we only need the base URL (without /api)
      const fullUrl = `${this.baseUrl}${downloadUrl}`;
      
      console.log('Fixed download URL:', fullUrl);
      
      // Method 1: Try direct download with blob
      const response = await lastValueFrom(
        this.http.get(fullUrl, {
          responseType: 'blob',
          observe: 'response'
        })
      );

      if (response.body) {
        const blob = new Blob([response.body], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Clean up
        window.URL.revokeObjectURL(url);
        
        console.log('File downloaded successfully');
      } else {
        throw new Error('Empty response from server');
      }
    } catch (error) {
      console.error('Download failed with HTTP method, trying direct link...', error);
      
      // Method 2: Fallback to direct link
      try {
        const fullUrl = `${this.baseUrl}${downloadUrl}`;
        const link = document.createElement('a');
        link.href = fullUrl;
        link.download = fileName;
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        console.log('Fallback download triggered');
      } catch (fallbackError) {
        console.error('Both download methods failed:', fallbackError);
        throw new Error(`Download failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
      }
    }
  }

  async testConnection(): Promise<boolean> {
    try {
      await lastValueFrom(
        this.http.get(`${this.baseUrl}/api/test`)
      );
      return true;
    } catch (error) {
      console.error('Connection test failed:', error);
      return false;
    }
  }

  async listProcessedFiles(): Promise<string[]> {
    try {
      const response = await lastValueFrom(
        this.http.get<{files: string[]}>(`${this.apiUrl}/files/list`)
      );
      return response.files;
    } catch (error) {
      console.error('Error listing files:', error);
      return [];
    }
  }
}
