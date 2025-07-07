export interface ProcessingResult {
  totalFiles: number;
  successfulFiles: number;
  results: FileProcessingResult[];
}

export interface FileProcessingResult {
  fileName: string;
  processedFileName: string;
  success: boolean;
  message: string;
  downloadUrl: string;
}
